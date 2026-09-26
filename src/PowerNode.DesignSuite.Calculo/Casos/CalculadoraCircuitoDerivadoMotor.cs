using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Calcula un circuito de Fuerza (Art. 430): la corriente de plena carga (FLC) sale de tabla
/// (430-6(a), nunca de placa -- Tablas 430-247/248/249/250), el conductor se dimensiona con 125%
/// de esa FLC (430-22), y la protección con el % de la Tabla 430-52 según tipo de motor y
/// dispositivo, redondeado al estándar inmediato superior (430-52(c)(1) Excepción 1). El resto del
/// pipeline (temperatura de terminales, factores de corrección, ampacidad, caída de tensión, tierra)
/// es el mismo que cualquier otro caso.
///
/// <b>Fuera de v1:</b> 430-52(c)(1) Excepción 2 -- cuando ni la Tabla 430-52 ni su redondeo al
/// estándar superior alcanzan para que el motor arranque sin disparar, se permite subir hasta la
/// corriente de rotor bloqueado (430-251/430-7(b), ya importadas pero no conectadas aquí todavía).
/// Detectar ese caso requiere comparar contra la corriente de arranque real, un paso adicional que
/// se deja pendiente en vez de improvisarlo.
/// </summary>
public class CalculadoraCircuitoDerivadoMotor(
    ICatalogoCalibres catalogo,
    ITablaAmpacidad ampacidad,
    ITablaFlcMotor flcMotor,
    ITablaProteccionMotor proteccionMotor,
    ITablaProteccionEstandar proteccionEstandar,
    ITablaCorreccionTemperatura correccionTemperatura,
    ITablaAgrupamiento agrupamiento,
    ITablaPuestaTierra puestaTierra,
    ITablaImpedancia impedancia,
    ITablaAislamiento aislamiento)
{
    public ResultadoCircuitoDerivado Calcular(DatosEntradaCircuitoDerivadoMotor d)
    {
        var citas = new List<Cita>();
        var numeroFases = d.NumeroFases;

        // 1. FLC de tabla -- 430-6(a): nunca de placa, para dimensionar conductor y protección.
        var flc = FlcDeTabla(flcMotor, d.Hp, d.TipoAlimentacion, d.TensionNominalMotorV);
        citas.Add(new Cita("430-6(a)", $"FLC de tabla ({d.Hp} Hp, {d.TensionNominalMotorV} V, {d.TipoAlimentacion}): {flc:0.##} A"));

        // SI EL NÚMERO NO ES EL QUE PUBLICA EL DOF, LA MEMORIA LO DICE. Es la contrapartida de
        // ErratasDeLaNorma: una memoria que se aparta del texto publicado sin declararlo no se puede
        // verificar, y entonces no sirve para lo que se entrega. Null en la inmensa mayoría de los
        // cálculos -- hoy hay una sola errata en toda la norma.
        if (flcMotor.ErrataAplicada(d.Hp, d.TipoAlimentacion, d.TensionNominalMotorV) is { } errata)
            citas.Add(new Cita(
                $"ERRATA {errata.TablaId}",
                $"{errata.Descripcion}: se calcula con {errata.ValorCorregido:0.##} A en lugar de los "
                + $"{errata.ValorPublicado:0.##} A publicados en el DOF, porque {errata.Sustento}."));

        // 2. Capacidad mínima de conductor -- 430-22: 125% de la FLC de tabla.
        var capacidadMinConductor = 1.25m * flc;
        citas.Add(new Cita("430-22", $"Capacidad mínima del conductor: 125% x {flc:0.##} A = {capacidadMinConductor:0.##} A"));

        // 3. Protección -- Tabla 430-52: techo = FLC x %, redondeado al estándar inmediato superior.
        var porcentaje = proteccionMotor.PorcentajeMaximo(d.TipoMotor, d.TipoDispositivoProteccion);
        var techoProteccion = flc * porcentaje / 100m;
        var breaker = proteccionEstandar.SiguienteEstandar(techoProteccion);
        citas.Add(new Cita("430-52", $"Techo de protección: {porcentaje}% x {flc:0.##} A = {techoProteccion:0.##} A -> {breaker} A " +
            "(430-52(c)(1) Excepción 1: redondeo al estándar inmediato superior)"));

        // 4. Temperatura de terminales -- 110-14(c)(1). Con equipo marcado 75 °C la columna depende
        // también del aislamiento, igual que en el circuito no-motor (M-06).
        var tempTerminales = TemperaturaTerminales.Para(
            breaker, d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco));
        citas.Add(new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, tempTerminales)));

        // 4.5. Aislamiento -- 110-14(c): debe alcanzar o superar la temperatura que exige la terminal.
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) cubre los {(int)tempTerminales}°C de la terminal."));

        // 5. Factores de corrección -- 310-15(b)(2)(a)/(3)(a). Se corrige en la columna del
        // AISLAMIENTO -- 110-14(c)(1)a.(2)/b.(2): SeleccionConductor topa el resultado a la terminal.
        // (Fuera de v1: 110-14(c)(1)a.(4), la variante específica para motores con letra de diseño
        // B/C/D/E, que permite 75°C o más incluso en circuitos <=100A -- requiere capturar la letra
        // de diseño, que hoy no se modela; el crédito general de (c)(1)a.(2)/b.(2) ya cubre el caso
        // común de todas formas.)
        var factorTemp = correccionTemperatura.Factor(d.TemperaturaAmbienteC, tempAislamiento)
            ?? throw new InvalidOperationException($"La Tabla 310-15(b)(2)(a) no cubre {d.TemperaturaAmbienteC}°C para la columna de {(int)tempAislamiento}°C.");
        var factorAgrup = agrupamiento.Factor(d.NumeroConductoresAgrupados);

        if (factorTemp != 1m)
            citas.Add(new Cita("310-15(b)(2)(a)", $"Factor de corrección por temperatura ambiente ({d.TemperaturaAmbienteC}°C): x{factorTemp}"));

        // SI LA FILA NO SE LEYÓ COMO LA PUBLICA EL DOF, LA MEMORIA LO DICE — misma regla que la
        // errata de FLC del Art. 430. Null en cualquier temperatura que no caiga en 71-75 °C.
        if (correccionTemperatura.ErrataAplicada(d.TemperaturaAmbienteC) is { } errataTemp)
            citas.Add(new Cita(
                $"ERRATA {errataTemp.TablaId}",
                $"{errataTemp.Descripcion}: se lee como el intervalo {errataTemp.Min:0.##}-{errataTemp.Max:0.##} °C, "
                + $"porque {errataTemp.Sustento}."));
        if (factorAgrup != 1m)
            citas.Add(new Cita("310-15(b)(3)(a)", $"Factor de ajuste por agrupamiento ({d.NumeroConductoresAgrupados} conductores): x{factorAgrup}"));

        // 6-8. Calibre por ampacidad y por caída de tensión, N de conductores en paralelo -- con la
        // FLC de tabla (no un In "de diseño" distinto) y la tensión REAL del tablero
        // (bloque 8: auto-resuelve el caso obligado, sugiere el caso conveniente — ver SeleccionConductor).
        var tensionEfectiva = numeroFases == 1 ? d.TensionFaseNeutroV : d.TensionFaseFaseV;
        var seleccion = SeleccionConductor.Seleccionar(
            catalogo, ampacidad, impedancia,
            capacidadMinConductorA: capacidadMinConductor,
            corrienteParaCaidaA: flc,
            numeroConductoresParaleloCapturado: d.NumeroConductoresParalelo,
            factorTemp: factorTemp,
            factorAgrup: factorAgrup,
            materialConductor: d.MaterialConductor,
            materialCanalizacion: d.MaterialCanalizacion,
            tempAislamiento: tempAislamiento,
            tempTerminales: tempTerminales,
            longitudM: d.LongitudM,
            factorPotencia: d.FactorPotencia,
            numeroFases: numeroFases,
            tensionEfectivaV: tensionEfectiva,
            caidaTensionMaxPct: d.CaidaTensionMaxPct,
            pisoPracticoCalibreMm2: d.PisoPracticoCalibreMm2,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);
        citas.AddRange(seleccion.Citas);

        var calibreFinal = seleccion.CalibreFase;
        var caidaPct = seleccion.CaidaTensionPct;
        var nParalelo = seleccion.NumeroConductoresParalelo;

        // 9. Neutro -- mismo calibre que fase por default (un motor normalmente no usa neutro, pero se deja consistente con los demás casos).
        var calibreNeutro = calibreFinal;

        // 10. Tierra -- 250-122(d), "CircuitosDerivados de motores".
        //
        // (d)(1) es el caso normal: se entra a la tabla con la protección del circuito derivado.
        // (d)(2) es el caso del disparo instantáneo (y del protector contra cortocircuito del
        // motor), donde la protección de 430-52 puede ser muchísimo mayor que el conductor: ahí la
        // norma NO deja usar ese valor, sino "el valor nominal máximo permitido del fusible de doble
        // elemento con retardo de tiempo [...] de acuerdo con 430-52(c)(1), Excepción 1". O sea, se
        // recalcula el techo con el renglón del fusible con retardo y se redondea igual.
        var proteccionParaTierra = breaker;
        if (d.TipoDispositivoProteccion == TipoDispositivoProteccionMotor.InterruptorDisparoInstantaneo)
        {
            var pctFusible = proteccionMotor.PorcentajeMaximo(d.TipoMotor, TipoDispositivoProteccionMotor.FusibleDeDosElementosConRetardo);
            proteccionParaTierra = proteccionEstandar.SiguienteEstandar(flc * pctFusible / 100m);
            citas.Add(new Cita("250-122(d)(2)",
                $"El dispositivo es de disparo instantáneo, así que la tierra NO se dimensiona con sus {breaker} A: se usa el máximo " +
                $"fusible de doble elemento con retardo que permitiría 430-52(c)(1) Exc. 1 ({pctFusible}% x {flc:0.##} A -> {proteccionParaTierra} A)"));
        }

        var (calibreTierra, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: proteccionParaTierra,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: calibreFinal,
            nParalelo: nParalelo);
        citas.AddRange(citasTierra);

        return new ResultadoCircuitoDerivado(
            CorrienteDisenoA: flc,
            ProteccionA: breaker,
            CalibreFase: calibreFinal,
            CalibreNeutro: calibreNeutro,
            CalibreTierra: calibreTierra,
            CaidaTensionPct: caidaPct,
            TablaAmpacidadId: d.MetodoInstalacion == MetodoInstalacion.AlAireLibre ? "310-15(b)(17)" : "310-15(b)(16)",
            Citas: citas,
            NumeroConductoresParalelo: nParalelo,
            // El desglose, igual que en el circuito no-motor y en el alimentador. FALTABA: un
            // circuito de Fuerza salía con Detalle nulo, y la memoria de cálculo lo trata como
            // "elemento sin cálculo" — o sea que un tablero de motores emitía hojas a medias, que es
            // justo lo que C1.3 declaró inaceptable para los otros dos casos. La capacidad mínima que
            // se reporta es la del 430-22 (125 % de la FLC de tabla), que es la que gobierna el
            // conductor; la protección del 430-52 va por su lado y ya está en las citas.
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
                CaidaTensionV: seleccion.CaidaTensionV));
    }

    /// <summary>
    /// FLC de tabla (430-6(a)), compartida con la agregación de varios motores en un alimentador
    /// (430-24, en CascadaCalculoService) para no repetir el mensaje de error en dos lugares.
    /// </summary>
    public static decimal FlcDeTabla(ITablaFlcMotor flcMotor, decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionNominalMotorV) =>
        flcMotor.CorrientePlenaCargaA(hp, tipoAlimentacion, tensionNominalMotorV)
            ?? throw new InvalidOperationException(
                $"Las Tablas 430-247/248/249/250 no traen una fila para {hp} Hp / {tensionNominalMotorV} V / {tipoAlimentacion} -- " +
                "verifica que la tensión de placa del motor sea una de las clases estándar de la tabla (115, 127, 200, 208, 230, 460, 575, 2300...).");
}
