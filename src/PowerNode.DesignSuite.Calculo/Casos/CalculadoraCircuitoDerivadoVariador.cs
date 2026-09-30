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
    bool TerminalesMarcadas75C = false);

/// <summary>
/// <b>El derivado de un variador</b> — 430-122(a), 110-3(b). La corriente es la de entrada del variador,
/// no la FLC del motor: el porcentaje de la Tabla 430-52 no aplica a un equipo de conversión. Conductor
/// al 125 % de la entrada; protección, el mayor tamaño estándar que no excede la máxima que marca el
/// fabricante, como la del 440-4(b). El mismo camino que <see cref="CalculadoraCircuitoDerivado440"/>
/// para terminales, aislamiento, factores, conductor, caída y tierra.
///
/// <para>
/// <b>Fuera</b>: el dispositivo de desviación (430-122(b)) —con él, el conductor también cubre el 125 %
/// de la FLC del motor— y los variadores con valores de entrada múltiples.
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
    public ResultadoCircuitoDerivado Calcular(DatosEntradaCircuitoDerivadoVariador d)
    {
        var citas = new List<Cita>();
        var entrada = d.CorrienteEntradaA;
        if (entrada <= 0m)
            throw new InvalidOperationException("430-122(a): falta la corriente nominal de entrada del variador, de su placa.");
        if (d.ProteccionMaximaA <= 0m)
            throw new InvalidOperationException(
                "110-3(b): falta la protección máxima que marca el fabricante del variador. Sin ella no se sabe qué interruptor lo protege.");

        // 1. Conductor — 430-122(a): 125 % de la corriente de entrada.
        var capacidadMinConductor = 1.25m * entrada;
        citas.Add(new Cita("430-122(a)",
            $"Conductores del variador: 125 % de su corriente nominal de entrada, {entrada:0.##} A = {capacidadMinConductor:0.##} A. " +
            "La FLC del motor y la Tabla 430-52 no se usan: la corriente del circuito es la del variador."));

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
                ? $"Protección: {breaker:0.##} A, la máxima que marca el fabricante del variador."
                : $"Protección: {breaker:0.##} A, el mayor tamaño estándar que no excede la máxima del fabricante ({d.ProteccionMaximaA:0.##} A)."));
        citas.Add(new Cita("240-4(g)",
            "La protección puede quedar arriba de la ampacidad del conductor: protege contra cortocircuito y falla a tierra " +
            "(Art. 430, Partes D y J). La sobrecarga del motor la da el variador si así lo marca — 430-124(a)."));

        // 3. Terminales y aislamiento — 110-14(c).
        var tempTerminales = TemperaturaTerminales.Para(
            breaker, d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar));
        citas.Add(new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, tempTerminales)));
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
        var seleccion = SeleccionConductor.Seleccionar(
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
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);
        citas.AddRange(seleccion.Citas);

        // 6. Tierra — Tabla 250-122, con la protección del derivado.
        var (calibreTierra, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: breaker,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: seleccion.CalibreFase,
            nParalelo: seleccion.NumeroConductoresParalelo);
        citas.AddRange(citasTierra);
        citas.Add(new Cita("430-128",
            $"Medio de desconexión en la entrada del variador: no menos de 115 % × {entrada:0.##} A = {1.15m * entrada:0.##} A."));

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
