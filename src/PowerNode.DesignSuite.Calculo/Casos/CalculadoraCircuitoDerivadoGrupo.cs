using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// <b>Varios motores, o motores y otras cargas, en un circuito derivado</b> — 430-53(c) y, con
/// motocompresores, 440-22(b). Nace en Power Node Web (I-115): el escritorio calcula un motor por
/// circuito y agrupa motores solo en el alimentador. Mismo camino que
/// <see cref="CalculadoraCircuitoDerivadoMotor"/>; cambia de dónde salen la capacidad mínima y la
/// protección.
///
/// <list type="bullet">
/// <item><b>Conductor — 430-24</b> (440-33 y 440-34 con motocompresores): 125 % de la máquina de
/// mayor corriente, las demás al 100 %, las otras cargas continuas al 125 % y las no continuas al
/// 100 %.</item>
/// <item><b>Protección — 430-53(c)(4)</b>: no debe exceder el porcentaje de la Tabla 430-52 del motor
/// mayor sobre su FLC, más la FLC de los demás motores y las otras cargas. «No exceda»: el mayor
/// tamaño estándar que no lo pase, sin el redondeo hacia arriba de 430-52(c)(1) Exc. 1 — decisión de
/// David, 2026-09-29. Si ese límite queda abajo de la ampacidad del conductor, el mismo inciso deja
/// subir hasta lo que permite 240-4(b); aquí se sube solo lo necesario para que el interruptor lleve la
/// corriente de operación.</item>
/// <item><b>Con motocompresores — 440-22(b)</b>: si el motocompresor es la carga más grande, el
/// 175 % (225 %) de 440-22(a) sobre el mayor en lugar del de la Tabla 430-52 — (b)(1); si no, la suma
/// de los motocompresores más lo de 430-53(c)(4) para los motores, o lo de 240-4 para las otras
/// cargas — (b)(2).</item>
/// </list>
///
/// <para>
/// La protección puede quedar arriba de la ampacidad del conductor (Tabla 240-4(g): Art. 430 Parte D
/// y Art. 440 Parte C): protege contra cortocircuito y falla a tierra; la sobrecarga de cada motor la
/// cuida su propio relevador — 430-53(c), 430-32.
/// </para>
///
/// <b>Fuera</b>: 430-53(b) (proteger al motor más chico) da un interruptor menor y más fácil de
/// disparar en el arranque; no se elige solo. 430-53(d) (derivaciones a cada motor) es de la
/// instalación, no del tablero.
/// </summary>
public class CalculadoraCircuitoDerivadoGrupo(
    ICatalogoCalibres catalogo,
    ITablaAmpacidad ampacidad,
    ITablaProteccionMotor proteccionMotor,
    ITablaProteccionEstandar proteccionEstandar,
    ITablaCorreccionTemperatura correccionTemperatura,
    ITablaAgrupamiento agrupamiento,
    ITablaPuestaTierra puestaTierra,
    ITablaImpedancia impedancia,
    ITablaAislamiento aislamiento)
{
    /// <summary>430-53(a): hasta 1 hp y 6 A por motor.</summary>
    public const decimal HpMaximo430_53a = 1m;

    /// <summary>430-53(a)(1).</summary>
    public const decimal FlcMaxima430_53aA = 6m;

    public ResultadoCircuitoDerivado Calcular(DatosEntradaCircuitoDerivadoGrupo d)
    {
        var citas = new List<Cita>();
        var miembros = d.Miembros.Lista;
        if (miembros.Count == 0 || miembros.Any(m => m.Cantidad < 1 || m.CorrienteUnitariaA <= 0m))
            throw new InvalidOperationException("430-53: el grupo necesita al menos una máquina, cada una con su corriente y su cantidad.");
        if (d.TipoDispositivoProteccion is not (TipoDispositivoProteccionMotor.InterruptorTiempoInverso
            or TipoDispositivoProteccionMotor.FusibleSinRetardoDeTiempo or TipoDispositivoProteccionMotor.FusibleDeDosElementosConRetardo))
            throw new InvalidOperationException(
                "430-53: el dispositivo de protección de un circuito con varios motores debe tener fusibles o interruptores automáticos de tiempo inverso.");

        var otrasContinua = Math.Max(0m, d.OtrasContinuaA);
        var otrasNoContinua = Math.Max(0m, d.OtrasNoContinuaA);
        var otras = otrasContinua + otrasNoContinua;
        var hayCompresor = miembros.Any(m => m.Clase == ClaseDeMiembro.Motocompresor);
        // «Cuando la única carga del circuito sea un motocompresor» — 440-22(b): 440-22(a) y 440-32.
        var compresorSolo = miembros is [{ Clase: ClaseDeMiembro.Motocompresor, Cantidad: 1 }] && otras == 0m;

        // 1. Cada máquina con su corriente — 430-6(a) (tabla) o 440-6(a) (placa). El que la redacta es el llamador.
        foreach (var m in miembros)
            citas.Add(new Cita(m.Clase switch { ClaseDeMiembro.Motor => "430-6(a)", ClaseDeMiembro.Variador => "430-122(a)", _ => "440-6(a)" },
                $"{m.Nombre}: {(m.Cantidad > 1 ? $"{m.Cantidad} × " : "")}{m.CorrienteUnitariaA:0.##} A — {m.Origen}"));

        var sumaMaquinas = miembros.Sum(m => m.Cantidad * m.CorrienteUnitariaA);
        // La mayor — 430-17, 440-7. Empate: el motocompresor, que es el que decide 440-22(b)(1).
        var mayor = miembros
            .OrderByDescending(m => m.CorrienteUnitariaA)
            .ThenBy(m => m.Clase == ClaseDeMiembro.Motocompresor ? 0 : 1)
            .First();
        // Un variador cuenta como motor del grupo, con la corriente de entrada del variador — 430-120, 430-122(a).
        var mayorMotor = miembros.Where(m => m.Clase is ClaseDeMiembro.Motor or ClaseDeMiembro.Variador).MaxBy(m => m.CorrienteUnitariaA);
        var mayorCompresor = miembros.Where(m => m.Clase == ClaseDeMiembro.Motocompresor).MaxBy(m => m.CorrienteUnitariaA);

        // 2. Capacidad mínima del conductor — 430-24; con motocompresores, 440-33 (y 440-34 con otras cargas).
        var capacidadMinConductor = sumaMaquinas + 0.25m * mayor.CorrienteUnitariaA + 1.25m * otrasContinua + otrasNoContinua;
        var terminos = new List<string> { $"125 % × {mayor.CorrienteUnitariaA:0.##} A ({mayor.Nombre}, la de mayor corriente)" };
        if (sumaMaquinas - mayor.CorrienteUnitariaA > 0m)
            terminos.Add($"{sumaMaquinas - mayor.CorrienteUnitariaA:0.##} A (las demás máquinas, al 100 %)");
        if (otrasContinua > 0m)
            terminos.Add($"125 % × {otrasContinua:0.##} A (otras cargas, continua)");
        if (otrasNoContinua > 0m)
            terminos.Add($"{otrasNoContinua:0.##} A (otras cargas, no continua)");
        citas.Add(new Cita(!hayCompresor ? "430-24" : compresorSolo ? "440-32" : otras > 0m ? "440-34" : "440-33",
            $"Capacidad mínima del conductor: {string.Join(" + ", terminos)} = {capacidadMinConductor:0.##} A"));

        // 3. El límite de la protección — 430-53(c)(4), 440-22(b)(1)/(2).
        var detalle = LimiteDeProteccion(d, citas, sumaMaquinas, otrasContinua, otrasNoContinua, mayorMotor, mayorCompresor, compresorSolo);
        var techo = detalle.TechoA;
        var piso = sumaMaquinas + 1.25m * otrasContinua + otrasNoContinua;

        // «No exceda»: el mayor tamaño estándar que no lo pase. Con un motocompresor al frente, nunca se
        // exige bajar de 15 A — Excepción de 440-22(a).
        var breaker = proteccionEstandar.AnteriorEstandar(techo);
        if (detalle.Regla is "440-22(b)(1)" or "440-22(a)" && (breaker is null || breaker < CalculadoraCarga440.ProteccionMinimaA))
        {
            breaker = proteccionEstandar.SiguienteEstandar(CalculadoraCarga440.ProteccionMinimaA);
            citas.Add(new Cita("440-22(a) Excepción",
                $"El límite ({techo:0.##} A) queda abajo de {CalculadoraCarga440.ProteccionMinimaA:0} A; no se exige bajar de ese valor."));
        }

        // 4-8. Terminales, aislamiento, factores y conductor — con la protección que resulte.
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        var factorTemp = correccionTemperatura.Factor(d.TemperaturaAmbienteC, tempAislamiento)
            ?? throw new InvalidOperationException($"La Tabla 310-15(b)(2)(a) no cubre {d.TemperaturaAmbienteC}°C para la columna de {(int)tempAislamiento}°C.");
        var factorAgrup = agrupamiento.Factor(d.NumeroConductoresAgrupados);
        var tensionEfectiva = d.NumeroFases == 1 ? d.TensionFaseNeutroV : d.TensionFaseFaseV;

        (TemperaturaAislamiento Terminal, SeleccionConductor.Resultado Seleccion) Conductor(decimal proteccion)
        {
            var terminal = TemperaturaTerminales.Para(proteccion, d.TerminalesMarcadas75C, tempAislamiento);
            if (tempAislamiento < terminal)
                throw new AislamientoIncompatibleException(
                    $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)terminal}°C que exige la terminal del equipo -- 110-14(c).");
            return (terminal, SeleccionConductor.Seleccionar(
                catalogo, ampacidad, impedancia,
                capacidadMinConductorA: capacidadMinConductor,
                // La caída, con la corriente de operación: las máquinas y las cargas al 100 %.
                corrienteParaCaidaA: sumaMaquinas + otras,
                numeroConductoresParaleloCapturado: d.NumeroConductoresParalelo,
                factorTemp: factorTemp,
                factorAgrup: factorAgrup,
                materialConductor: d.MaterialConductor,
                materialCanalizacion: d.MaterialCanalizacion,
                tempAislamiento: tempAislamiento,
                tempTerminales: terminal,
                longitudM: d.LongitudM,
                factorPotencia: d.FactorPotencia,
                numeroFases: d.NumeroFases,
                tensionEfectivaV: tensionEfectiva,
                caidaTensionMaxPct: d.CaidaTensionMaxPct,
                pisoPracticoCalibreMm2: d.PisoPracticoCalibreMm2,
                metodoInstalacion: d.MetodoInstalacion,
                maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico));
        }

        var (tempTerminales, seleccion) = Conductor(breaker ?? proteccionEstandar.SiguienteEstandar(piso));

        // Un interruptor abajo de la corriente de operación dispara sin falla. 430-53(c)(4), segunda
        // oración: si el límite queda abajo de la ampacidad del conductor, se puede subir hasta lo que
        // permite 240-4(b). Se sube lo necesario, no más.
        decimal? limite240_4b = null;
        if ((breaker is null || breaker < piso) && detalle.TopeDelFabricante)
            throw new InvalidOperationException(
                $"430-53(c)(2): la protección máxima de un variador ({techo:0.##} A, la de su fabricante — 110-3(b)) no lleva la " +
                $"corriente de operación del grupo ({piso:0.##} A). Pasa ese variador a su propio circuito.");
        if (breaker is null || breaker < piso)
        {
            var ampacidadConductor = seleccion.AmpacidadUtilizableTotalA;
            var necesario = proteccionEstandar.SiguienteEstandar(piso);
            limite240_4b = Limite240_4b(ampacidadConductor);
            if (techo >= ampacidadConductor || necesario > limite240_4b)
                throw new InvalidOperationException(
                    $"{detalle.Regla}: el límite de la protección ({techo:0.##} A) no deja un interruptor que lleve la corriente de " +
                    $"operación del grupo ({piso:0.##} A)" +
                    (techo < ampacidadConductor
                        ? $", ni subiendo hasta lo que permite 240-4(b) para el conductor ({limite240_4b:0.##} A)."
                        : ".") +
                    " Separa las otras cargas en otro circuito.");
            breaker = necesario;
            citas.Add(new Cita("430-53(c)(4)",
                $"El límite ({techo:0.##} A) queda abajo de la ampacidad del conductor ({ampacidadConductor:0.##} A): se permite subir " +
                $"hasta lo que deja 240-4(b) ({limite240_4b:0.##} A). Se usa {breaker:0.##} A, el primer tamaño estándar que lleva la " +
                $"corriente de operación ({piso:0.##} A)."));
            var terminalFinal = TemperaturaTerminales.Para(breaker.Value, d.TerminalesMarcadas75C, tempAislamiento);
            if (terminalFinal != tempTerminales)
                (tempTerminales, seleccion) = Conductor(breaker.Value);
        }
        else
        {
            citas.Add(new Cita(detalle.Regla,
                $"Protección: {breaker:0.##} A, el mayor tamaño estándar que no excede {techo:0.##} A. Sin el redondeo hacia arriba de " +
                "430-52(c)(1) Excepción 1: ese permiso es del motor solo."));
        }
        var proteccion = breaker!.Value;
        detalle = detalle with { PisoA = piso, Limite240_4bA = limite240_4b };

        citas.Add(new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(proteccion, d.TerminalesMarcadas75C, tempTerminales)));
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) cubre los {(int)tempTerminales}°C de la terminal."));
        if (factorTemp != 1m)
            citas.Add(new Cita("310-15(b)(2)(a)", $"Factor de corrección por temperatura ambiente ({d.TemperaturaAmbienteC}°C): x{factorTemp}"));
        if (correccionTemperatura.ErrataAplicada(d.TemperaturaAmbienteC) is { } errataTemp)
            citas.Add(new Cita(
                $"ERRATA {errataTemp.TablaId}",
                $"{errataTemp.Descripcion}: se lee como el intervalo {errataTemp.Min:0.##}-{errataTemp.Max:0.##} °C, "
                + $"porque {errataTemp.Sustento}."));
        if (factorAgrup != 1m)
            citas.Add(new Cita("310-15(b)(3)(a)", $"Factor de ajuste por agrupamiento ({d.NumeroConductoresAgrupados} conductores): x{factorAgrup}"));
        citas.AddRange(seleccion.Citas);

        // Lo que el cuadro no puede revisar, dicho para que se revise en campo.
        citas.Add(new Cita("240-4(g)",
            "La protección del grupo puede quedar arriba de la ampacidad del conductor: protege contra cortocircuito y falla a " +
            "tierra (Art. 430 Parte D" + (hayCompresor ? ", Art. 440 Parte C" : "") + ")."));
        citas.Add(new Cita("430-53(c)",
            "Cada motor con su protección contra sobrecarga (430-32" + (hayCompresor ? ", 440-52" : "") + "); sus controladores y " +
            "relevadores, aprobados para instalación en grupo con este interruptor — (c)(1) a (c)(3); y el interruptor no mayor " +
            "que el que permite 430-40 al relevador del motor más chico — (c)(5)."));
        if (otras > 0m)
            citas.Add(new Cita("430-53(c)(6)",
                "Las otras cargas, con su protección contra sobrecorriente según el Art. 240: si el interruptor del grupo pasa la " +
                "ampacidad de sus derivaciones, cada una lleva la suya."));
        if (Cumple430_53a(d, proteccion) is { } nota)
            citas.Add(new Cita("430-53(a)", nota));

        var (calibreTierra, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: proteccion,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: seleccion.CalibreFase,
            nParalelo: seleccion.NumeroConductoresParalelo);
        citas.AddRange(citasTierra);

        var nParalelo = seleccion.NumeroConductoresParalelo;
        return new ResultadoCircuitoDerivado(
            // La corriente de operación: las máquinas y las otras cargas al 100 %.
            CorrienteDisenoA: sumaMaquinas + otras,
            ProteccionA: proteccion,
            CalibreFase: seleccion.CalibreFase,
            CalibreNeutro: seleccion.CalibreFase,
            CalibreTierra: calibreTierra,
            CaidaTensionPct: seleccion.CaidaTensionPct,
            TablaAmpacidadId: d.MetodoInstalacion == MetodoInstalacion.AlAireLibre ? "310-15(b)(17)" : "310-15(b)(16)",
            Citas: citas,
            NumeroConductoresParalelo: nParalelo,
            Detalle: new DetalleDelCalculo(
                CapacidadMinimaA: capacidadMinConductor,
                FactorTemperatura: factorTemp,
                FactorAgrupamiento: factorAgrup,
                CapacidadMinimaCorregidaA: capacidadMinConductor / (factorTemp * factorAgrup * nParalelo),
                AmpacidadConductorA: seleccion.AmpacidadUtilizableTotalA,
                TemperaturaTerminalesC: (int)tempTerminales,
                TemperaturaAislamientoC: (int)tempAislamiento,
                ResistenciaOhmKm: seleccion.ResistenciaOhmKm,
                ReactanciaOhmKm: seleccion.ReactanciaOhmKm,
                CaidaTensionV: seleccion.CaidaTensionV),
            Grupo: detalle);
    }

    /// <summary>
    /// De qué regla sale el límite y cuánto vale. La máquina que va al porcentaje entra una sola vez:
    /// las demás unidades de su renglón van con «las demás».
    /// </summary>
    private DetalleDelGrupo LimiteDeProteccion(
        DatosEntradaCircuitoDerivadoGrupo d, List<Cita> citas, decimal sumaMaquinas, decimal otrasContinua, decimal otrasNoContinua,
        MiembroDelGrupo? mayorMotor, MiembroDelGrupo? mayorCompresor, bool compresorSolo)
    {
        var otras = otrasContinua + otrasNoContinua;
        var compresorEsLaMayor = mayorCompresor is not null
            && mayorCompresor.CorrienteUnitariaA >= (mayorMotor?.CorrienteUnitariaA ?? 0m)
            && mayorCompresor.CorrienteUnitariaA >= d.MayorOtraCargaA;

        string regla;
        MiembroDelGrupo mayor;
        decimal porcentaje, otrasEnElLimite;
        string porQue;
        if (compresorEsLaMayor)
        {
            regla = compresorSolo ? "440-22(a)" : "440-22(b)(1)";
            mayor = mayorCompresor!;
            porcentaje = d.RequiereArranque ? CalculadoraCarga440.TechoProteccionArranquePct : CalculadoraCarga440.TechoProteccionPct;
            otrasEnElLimite = otras;
            porQue = (compresorSolo ? "el motocompresor es la única carga" : "el motocompresor es la carga más grande") + $": {porcentaje:0} % de 440-22(a)" +
                     (d.RequiereArranque ? " (el 175 % no conduce la corriente de arranque)" : "");
        }
        else if (mayorMotor is not null)
        {
            regla = mayorCompresor is null ? "430-53(c)(4)" : "440-22(b)(2)";
            mayor = mayorMotor;
            otrasEnElLimite = otras;
            if (mayor.Clase == ClaseDeMiembro.Variador)
            {
                // El «valor de 430-52» de un variador es la protección máxima de su fabricante — 430-120, 110-3(b).
                porcentaje = mayor.ProteccionMaximaA is > 0m and var pm ? 100m * pm / mayor.CorrienteUnitariaA : 100m;
                porQue = "la protección máxima del fabricante del variador mayor (110-3(b)) en lugar de la Tabla 430-52";
            }
            else
            {
                porcentaje = proteccionMotor.PorcentajeMaximo(d.TipoMotor, d.TipoDispositivoProteccion);
                porQue = $"{porcentaje:0} % de la Tabla 430-52 para el motor mayor";
            }
            porQue += mayorCompresor is null ? "" : ", porque el motocompresor no es la carga más grande — con 430-53(c)(4)";
        }
        else
        {
            // Solo motocompresores y otras cargas, y una de estas es la más grande — 440-22(b)(2): la
            // suma de los motocompresores más lo que 240-4 permite para las otras cargas.
            regla = "440-22(b)(2)";
            mayor = mayorCompresor!;
            porcentaje = 100m;
            otrasEnElLimite = otras > 0m ? proteccionEstandar.SiguienteDeLaNorma(1.25m * otrasContinua + otrasNoContinua) : 0m;
            porQue = "la carga más grande no es un motocompresor: los motocompresores al 100 % y, para las otras cargas, lo que permite 240-4";
        }

        var valorDelMayor = porcentaje * mayor.CorrienteUnitariaA / 100m;
        var demas = sumaMaquinas - mayor.CorrienteUnitariaA;
        var techo = valorDelMayor + demas + otrasEnElLimite;

        var partes = new List<string>
        {
            mayor.Clase == ClaseDeMiembro.Variador
                ? $"{valorDelMayor:0.##} A ({mayor.Nombre}, protección máxima del fabricante)"
                : $"{porcentaje:0} % × {mayor.CorrienteUnitariaA:0.##} A ({mayor.Nombre})",
        };
        if (demas > 0m)
            partes.Add($"{demas:0.##} A (las demás máquinas)");
        if (otrasEnElLimite > 0m)
            partes.Add($"{otrasEnElLimite:0.##} A (otras cargas{(regla == "440-22(b)(2)" && mayorMotor is null ? ", por 240-4" : "")})");
        citas.Add(new Cita(regla, $"Límite de la protección — {porQue}: {string.Join(" + ", partes)} = {techo:0.##} A"));

        // 430-53(c)(2)(a): cada variador, como controlador, admite hasta la protección que marca su fabricante.
        var tope = false;
        if (d.Miembros.Lista.Where(m => m.Clase == ClaseDeMiembro.Variador && m.ProteccionMaximaA is > 0m)
                .MinBy(m => m.ProteccionMaximaA) is { } limitante && limitante.ProteccionMaximaA < techo)
        {
            techo = limitante.ProteccionMaximaA!.Value;
            tope = true;
            citas.Add(new Cita("430-53(c)(2)",
                $"{limitante.Nombre} admite a lo más {techo:0.##} A de protección, la que marca su fabricante (110-3(b)): el límite baja a ese valor."));
        }

        return new DetalleDelGrupo(regla, mayor, porcentaje, valorDelMayor, demas, otrasEnElLimite, techo, 0m, null, tope);
    }

    /// <summary>
    /// Lo que 240-4(b) permite sobre un conductor: su ampacidad, o el tamaño siguiente de la norma si
    /// no es un valor estándar y no pasa de 800 A.
    /// </summary>
    private decimal Limite240_4b(decimal ampacidadConductorA) =>
        proteccionEstandar.ValoresDeLaNorma.Contains(ampacidadConductorA)
            ? ampacidadConductorA
            : proteccionEstandar.SiguienteDeLaNorma(ampacidadConductorA) is var siguiente && siguiente <= 800m ? siguiente : ampacidadConductorA;

    /// <summary>
    /// 430-53(a): motores de hasta 1 hp y 6 A cada uno, en un circuito de 120 V nominales protegido a no
    /// más de 20 A (15 A en otra tensión). El 127 V de fase a neutro se lee como el de 120 V nominales.
    /// Solo se informa: el cálculo del grupo ya da un interruptor permitido.
    /// </summary>
    private static string? Cumple430_53a(DatosEntradaCircuitoDerivadoGrupo d, decimal proteccionA)
    {
        var maquinas = d.Miembros.Lista;
        if (maquinas.Any(m => m.Clase != ClaseDeMiembro.Motor || m.Hp is not { } hp || hp > HpMaximo430_53a || m.CorrienteUnitariaA > FlcMaxima430_53aA))
            return null;
        var de120 = d.NumeroFases == 1 && d.TensionFaseNeutroV <= 127m;
        var maximo = de120 ? 20m : 15m;
        return proteccionA > maximo
            ? null
            : $"Todos los motores son de 1 hp o menos y de 6 A o menos, con {proteccionA:0} A ({(de120 ? "circuito de 127 V, hasta 20 A" : "hasta 15 A")}): " +
              "el grupo cumple también 430-53(a), con la protección individual contra sobrecarga de 430-32 y sin pasar la protección " +
              "que marque cualquier controlador — 430-53(a)(2), (a)(3).";
    }
}
