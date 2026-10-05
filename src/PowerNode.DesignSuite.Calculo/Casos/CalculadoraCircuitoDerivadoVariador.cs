using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// El circuito derivado de un <b>variador de velocidad</b> (equipo de conversión de potencia de un
/// sistema de accionamiento de velocidad ajustable) — Art. 430 Parte J. Nace en Power Node Web (I-119).
/// </summary>
/// <param name="CorrienteEntradaA">La corriente nominal de entrada del variador, de su placa — 430-122(a).</param>
/// <param name="ProteccionMaximaA">La protección máxima que marca el fabricante del variador — 110-3(b).</param>
/// <param name="NumeroFases">1 (monofásico, F-N o entre fases) o 3.</param>
/// <param name="TensionFaseNeutroV">La tensión de un variador monofásico: F-N en 1 polo, F-F en 2.</param>
public sealed record DatosEntradaCircuitoDerivadoVariador(
    decimal CorrienteEntradaA,
    decimal ProteccionMaximaA,
    int NumeroFases,
    decimal TensionFaseNeutroV,
    decimal TensionFaseFaseV,
    decimal LongitudM,
    int NumeroConductoresParalelo,
    int NumeroConductoresAgrupados,
    decimal TemperaturaAmbienteC,
    MaterialConductor MaterialConductor,
    MaterialCanalizacion MaterialCanalizacion,
    decimal FactorPotencia,
    decimal CaidaTensionMaxPct,
    string TipoAislamiento = "THHN",
    LugarDeInstalacion Lugar = LugarDeInstalacion.Seco,
    MetodoInstalacion MetodoInstalacion = MetodoInstalacion.CanalizacionOCable,
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision,
    bool TerminalesMarcadas75C = false,

    /// <summary>
    /// <b>Cómo se escoge la protección dentro del rango</b> — Power Node Web, M-20, fase 2. Por omisión, el
    /// máximo: lo que hacía antes.
    /// </summary>
    CriterioProteccionMotor CriterioProteccion = CriterioProteccionMotor.Maximo430_52,

    /// <summary>Con <see cref="CriterioProteccionMotor.Manual"/>: la que escogió el proyectista (M-20).</summary>
    decimal? ProteccionElegidaA = null,

    /// <summary>
    /// <b>El dispositivo de desviación (bypass)</b>, con el motor que mueve — 430-122(b); Power Node Web, M-23.
    /// <c>null</c>: sin bypass, como antes.
    /// </summary>
    DesviacionDelVariador? Bypass = null);

/// <summary>
/// <b>El dispositivo de desviación (bypass) de un variador</b> — Power Node Web, M-23 (AM-7, CONFIRMADA · David ·
/// 2026-10-05). Con él el motor también trabaja y arranca directo de la línea. El conductor lleva además el 125 %
/// de la FLC del motor — 430-122(b). La NOM-001-SEDE-2012 no trae una regla propia para la protección del
/// conjunto, y 430-120 lleva a la Parte D: la protección tampoco excede la de la Tabla 430-52 para ese motor.
/// </summary>
/// <param name="Hp">Los HP de placa del motor, para las citas.</param>
/// <param name="FlcMotorA">La FLC del motor, de tabla — 430-6(a).</param>
/// <param name="PorcentajeTabla430_52">El de la Tabla 430-52 para ese motor y el interruptor de tiempo inverso.</param>
/// <param name="Tabla430_52">
/// La protección máxima de 430-52(c)(1) para ese motor, con su Excepción 1, y el mayor tamaño de la serie que no
/// la excede (<see cref="CalculadoraCircuitoDerivadoMotor.ProteccionDeLaTabla430_52"/>).
/// </param>
public sealed record DesviacionDelVariador(decimal Hp, decimal FlcMotorA, decimal PorcentajeTabla430_52, ProteccionDeMotor Tabla430_52);

/// <summary>
/// <b>El derivado de un variador</b> — 430-122(a), 110-3(b). La corriente es la de entrada del variador,
/// no la FLC del motor: el porcentaje de la Tabla 430-52 no aplica a un equipo de conversión. Conductor
/// al 125 % de la entrada; protección, el mayor tamaño estándar que no excede la máxima que marca el
/// fabricante, como la del 440-4(b). El mismo camino que <see cref="CalculadoraCircuitoDerivado440"/>
/// para terminales, aislamiento, factores, conductor, caída y tierra.
///
/// <para>
/// <b>Con dispositivo de desviación</b> (<see cref="DesviacionDelVariador"/>, M-23): el conductor, el mayor de
/// los dos 125 % (430-122(b)); la protección, entre el 125 % de la entrada y la menor de la máxima del fabricante y
/// la de la Tabla 430-52 para el motor; la desconexión, 115 % de la mayor de las dos corrientes (430-128,
/// 430-110(a)). <b>Fuera</b>: los variadores con valores de entrada múltiples.
/// </para>
/// </summary>
public class CalculadoraCircuitoDerivadoVariador(
    ICatalogoCalibres catalogo,
    ITablaAmpacidad ampacidad,
    ITablaProteccionEstandar proteccionEstandar,
    ITablaCorreccionTemperatura correccionTemperatura,
    ITablaAgrupamiento agrupamiento,
    ITablaPuestaTierra puestaTierra,
    ITablaImpedancia impedancia,
    ITablaAislamiento aislamiento)
{
    /// <summary>La regla del rango con bypass: la máxima del fabricante y la Tabla 430-52 del motor — M-23.</summary>
    public const string ReglaConBypass = "110-3(b), 430-52(c)(1)";

    public ResultadoCircuitoDerivado Calcular(DatosEntradaCircuitoDerivadoVariador d)
    {
        var citas = new List<Cita>();
        var entrada = d.CorrienteEntradaA;
        if (entrada <= 0m)
            throw new InvalidOperationException("430-122(a): falta la corriente nominal de entrada del variador, de su placa.");
        if (d.ProteccionMaximaA <= 0m)
            throw new InvalidOperationException(
                "110-3(b): falta la protección máxima que marca el fabricante del variador. Sin ella no se sabe qué interruptor lo protege.");

        // 1. Conductor — 430-122(a): 125 % de la corriente de entrada. Con bypass, también el 125 % de la FLC del
        // motor, y manda el mayor — 430-122(b).
        var bypass = d.Bypass;
        var alEntrada = 1.25m * entrada;
        var capacidadMinConductor = alEntrada;
        citas.Add(new Cita("430-122(a)",
            $"Conductores del variador: 125 % de su corriente nominal de entrada, {entrada:0.##} A = {alEntrada:0.##} A." +
            (bypass is null ? " La FLC del motor y la Tabla 430-52 no se usan: la corriente del circuito es la del variador." : "")));
        if (bypass is not null)
        {
            var alMotor = 1.25m * bypass.FlcMotorA;
            capacidadMinConductor = Math.Max(alEntrada, alMotor);
            citas.Add(new Cita("430-122(b)",
                $"Con dispositivo de desviación (bypass) el motor también trabaja directo de la línea: el conductor lleva además el 125 % " +
                $"de su FLC ({bypass.Hp:0.##} HP, {bypass.FlcMotorA:0.##} A) = {alMotor:0.##} A. Manda el mayor: {capacidadMinConductor:0.##} A."));
        }

        // 2. Protección — 110-3(b): la que marca el fabricante; «no exceda», el mayor estándar que no la pasa.
        var breaker = proteccionEstandar.AnteriorEstandar(d.ProteccionMaximaA)
            ?? throw new InvalidOperationException(
                $"110-3(b): la protección máxima del variador ({d.ProteccionMaximaA:0.##} A) es menor que el tamaño estándar más chico.");
        if (breaker < entrada)
            throw new InvalidOperationException(
                $"110-3(b): ningún tamaño estándar queda entre la corriente de entrada ({entrada:0.##} A) y la protección máxima " +
                $"({d.ProteccionMaximaA:0.##} A) del variador. Revisa los dos datos de la placa.");
        citas.Add(new Cita("110-3(b)",
            breaker == d.ProteccionMaximaA
                ? $"Máximo: {breaker:0.##} A, la protección máxima que marca el fabricante del variador."
                : $"Máximo: {breaker:0.##} A, el mayor tamaño estándar que no excede la máxima del fabricante ({d.ProteccionMaximaA:0.##} A)."));

        // 2.2. CON BYPASS, TAMPOCO LA TABLA 430-52 DEL MOTOR — M-23: el motor arranca directo de la línea, y la
        // NOM-2012 no trae una regla propia para el conjunto (no tiene 430-130); 430-120 lleva a la Parte D. El
        // máximo es el menor de los dos. Si ningún tamaño queda entre el 125 % de la entrada y él, se usa él y se
        // avisa: el rango vacío no es un error de captura, es algo que revisar con el fabricante del conjunto.
        var maximo = breaker;
        var maximoPermitido = d.ProteccionMaximaA;
        var rangoVacio = false;
        if (bypass is not null)
        {
            var t = bypass.Tabla430_52;
            citas.Add(new Cita("430-52(c)(1)",
                $"Con el bypass el motor arranca directo de la línea, y para el conjunto 430-120 lleva a la Parte D: la protección " +
                $"tampoco excede la de la Tabla 430-52 para el motor, {bypass.PorcentajeTabla430_52:0}% x {bypass.FlcMotorA:0.##} A = {t.Explicacion()}."));
            maximo = Math.Min(breaker, t.SeleccionadaA);
            maximoPermitido = Math.Min(d.ProteccionMaximaA, t.MaximoA);
            citas.Add(new Cita("430-122(b)",
                $"Máximo del circuito: {maximo:0.##} A, el menor de la máxima del fabricante ({breaker:0.##} A) y la de la Tabla 430-52 " +
                $"para el motor ({t.SeleccionadaA:0.##} A)."));
            rangoVacio = maximo < alEntrada;
            if (rangoVacio)
                citas.Add(new Cita("430-122(b)",
                    $"⚠ Ningún tamaño de la serie queda entre el 125 % de la entrada ({alEntrada:0.##} A) y el máximo ({maximo:0.##} A): " +
                    $"se usa {maximo:0.##} A, que no lleva el 125 % de la entrada. Revisar con el fabricante del conjunto variador y bypass."));
        }

        // 2.5. EL RANGO — M-20, fase 2: la máxima del fabricante es un techo («no exceda»); cualquiera del
        // rango cumple. Ver ProteccionDentroDelRango. El piso, el 125 % de la entrada (también con bypass).
        var criterio = d.CriterioProteccion;
        var (minimo, valores) = ProteccionDentroDelRango.Rango(proteccionEstandar, alEntrada, maximo);
        (breaker, var paraLaColumna) = ProteccionDentroDelRango.Inicial(criterio, d.ProteccionElegidaA, minimo, maximo, valores);
        var indiceDelRango = citas.Count;

        // 3. Terminales y aislamiento — 110-14(c). Con prioridad al conductor, la columna del piso (M-20).
        var tempTerminales = TemperaturaTerminales.Para(
            paraLaColumna, d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar));
        var indiceDeTerminales = citas.Count;
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({d.Lugar.Nombre()}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {d.Lugar.Nombre()}) cubre los {(int)tempTerminales}°C de la terminal."));

        // 4. Factores de corrección, en la columna del aislamiento — 310-15(b)(2)(a)/(3)(a).
        var factorTemp = correccionTemperatura.Factor(d.TemperaturaAmbienteC, tempAislamiento)
            ?? throw new InvalidOperationException($"La Tabla 310-15(b)(2)(a) no cubre {d.TemperaturaAmbienteC}°C para la columna de {(int)tempAislamiento}°C.");
        var factorAgrup = agrupamiento.Factor(d.NumeroConductoresAgrupados);
        if (factorTemp != 1m)
            citas.Add(new Cita("310-15(b)(2)(a)", $"Factor de corrección por temperatura ambiente ({d.TemperaturaAmbienteC}°C): x{factorTemp}"));
        if (correccionTemperatura.ErrataAplicada(d.TemperaturaAmbienteC) is { } errataTemp)
            citas.Add(new Cita(
                $"ERRATA {errataTemp.TablaId}",
                $"{errataTemp.Descripcion}: se lee como el intervalo {errataTemp.Min:0.##}-{errataTemp.Max:0.##} °C, "
                + $"porque {errataTemp.Sustento}."));
        if (factorAgrup != 1m)
            citas.Add(new Cita("310-15(b)(3)(a)", $"Factor de ajuste por agrupamiento ({d.NumeroConductoresAgrupados} conductores): x{factorAgrup}"));

        // 5. Calibre por ampacidad y por caída, con la corriente de entrada.
        var tensionEfectiva = d.NumeroFases == 1 ? d.TensionFaseNeutroV : d.TensionFaseFaseV;
        SeleccionConductor.Resultado Seleccionar(decimal? protegidoPorA) => SeleccionConductor.Seleccionar(
            catalogo, ampacidad, impedancia,
            capacidadMinConductorA: capacidadMinConductor,
            corrienteParaCaidaA: entrada,
            numeroConductoresParaleloCapturado: d.NumeroConductoresParalelo,
            factorTemp: factorTemp,
            factorAgrup: factorAgrup,
            materialConductor: d.MaterialConductor,
            materialCanalizacion: d.MaterialCanalizacion,
            tempAislamiento: tempAislamiento,
            tempTerminales: tempTerminales,
            longitudM: d.LongitudM,
            factorPotencia: d.FactorPotencia,
            numeroFases: d.NumeroFases,
            tensionEfectivaV: tensionEfectiva,
            caidaTensionMaxPct: d.CaidaTensionMaxPct,
            pisoPracticoCalibreMm2: null,
            proteccionEstandar: protegidoPorA is null ? null : proteccionEstandar,
            proteccionA: protegidoPorA,
            permiteExcepcion2404b: true,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);

        var escogida = ProteccionDentroDelRango.Escoger(
            criterio, breaker, minimo, valores, tempTerminales, proteccionEstandar, d.MaterialConductor, Seleccionar,
            c => SeleccionConductor.AmpacidadUtilizable(ampacidad, c, d.MaterialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, d.MetodoInstalacion));
        var seleccion = escogida.Seleccion;
        breaker = escogida.ProteccionA;
        citas.AddRange(seleccion.Citas);

        var rango = ProteccionDentroDelRango.Armar(
            bypass is null ? "110-3(b)" : ReglaConBypass,
            bypass is null
                ? $"la protección máxima del fabricante ({d.ProteccionMaximaA:0.##} A)"
                : $"la menor de la máxima del fabricante ({d.ProteccionMaximaA:0.##} A) y la de la Tabla 430-52 para el motor ({bypass.Tabla430_52.MaximoA:0.##} A)",
            $"125 % de la corriente de entrada = {alEntrada:0.##} A",
            "la sobrecarga del motor la da el variador si así lo marca (430-124(a))" +
                (bypass is null ? "" : " y, en el bypass, el relevador de su arrancador (430-124(b))") +
                "; el permiso llega por 430-120 a la Parte D, que la Tabla 240-4(g) nombra",
            alEntrada, minimo, maximo,
            // 430-62(a): la que marca el fabricante; con bypass, también la de la Tabla 430-52.
            maximoPermitido, valores, criterio, d.ProteccionElegidaA, null, escogida, proteccionEstandar, d.MaterialConductor,
            (p, max) => new Cita("110-3(b)",
                $"El fabricante marca la protección máxima; con {p:0.##} A, menos que ella, verificar en las instrucciones del variador que el " +
                $"interruptor le sirve (que no dispare al energizarlo); si no, subir hasta {max:0.##} A."));
        citas.Insert(indiceDeTerminales, new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, tempTerminales)));
        citas.InsertRange(indiceDelRango, ProteccionDentroDelRango.Citas(rango, seleccion.CalibreFase, seleccion.AmpacidadUtilizableTotalA));

        // 6. Tierra — Tabla 250-122, con la protección del derivado.
        var (calibreTierra, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: breaker,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: seleccion.CalibreFase,
            nParalelo: seleccion.NumeroConductoresParalelo);
        citas.AddRange(citasTierra);
        citas.Add(bypass is null
            ? new Cita("430-128",
                $"Medio de desconexión en la entrada del variador: no menos de 115 % × {entrada:0.##} A = {1.15m * entrada:0.##} A.")
            : new Cita("430-128",
                $"Medio de desconexión en la entrada del conjunto: no menos de 115 % de la mayor de la corriente de entrada ({entrada:0.##} A) " +
                $"y la FLC del motor ({bypass.FlcMotorA:0.##} A) = {1.15m * Math.Max(entrada, bypass.FlcMotorA):0.##} A — 430-128, 430-110(a)."));

        var nParalelo = seleccion.NumeroConductoresParalelo;
        return new ResultadoCircuitoDerivado(
            CorrienteDisenoA: entrada,
            ProteccionA: breaker,
            CalibreFase: seleccion.CalibreFase,
            CalibreNeutro: seleccion.CalibreFase,
            CalibreTierra: calibreTierra,
            CaidaTensionPct: seleccion.CaidaTensionPct,
            TablaAmpacidadId: d.MetodoInstalacion == MetodoInstalacion.AlAireLibre ? "310-15(b)(17)" : "310-15(b)(16)",
            Citas: citas,
            NumeroConductoresParalelo: nParalelo,
            Rango: rango,
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
}
