using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// <b>El circuito derivado de un equipo de aire acondicionado o refrigeración con motocompresor
/// hermético</b> (Art. 440), completo: protección, conductor, caída y tierra. Nace en Power Node Web
/// (I-74). El escritorio resuelve la protección y la corriente del conductor con
/// <see cref="CalculadoraCarga440"/> y dimensiona el conductor aparte; aquí se arma el derivado
/// entero, con el mismo camino que <see cref="CalculadoraCircuitoDerivadoMotor"/>.
///
/// <para>
/// <b>La protección puede quedar arriba de la ampacidad del conductor</b>: protege contra
/// cortocircuito y falla a tierra, y la Tabla 240-4(g) remite los conductores de estos equipos al
/// Art. 440, Partes C y F. La sobrecarga la cuida el protector del motocompresor (440-52).
/// </para>
/// </summary>
public class CalculadoraCircuitoDerivado440(
    ICatalogoCalibres catalogo,
    ITablaAmpacidad ampacidad,
    ITablaProteccionEstandar proteccionEstandar,
    ITablaCorreccionTemperatura correccionTemperatura,
    ITablaAgrupamiento agrupamiento,
    ITablaPuestaTierra puestaTierra,
    ITablaImpedancia impedancia,
    ITablaAislamiento aislamiento)
{
    public ResultadoCircuitoDerivado Calcular(DatosEntradaCircuitoDerivado440 d)
    {
        var citas = new List<Cita>();

        // 1-3. Corriente, capacidad del conductor y protección, según lo que traiga la placa.
        decimal corriente, capacidadMinConductor, breaker;
        if (d.EsDeHabitacion)
        {
            // 440 Parte G — I-117: un aparato monofásico de hasta 250 V y 40 A, con cordón y clavija,
            // que cuenta como una sola unidad de motor (440-62(a)).
            corriente = d.CorrienteTotalHabitacionA!.Value;
            if (corriente <= 0m)
                throw new InvalidOperationException("440-62(a)(3): falta la corriente total de carga nominal de la placa.");
            if (d.NumeroFases != 1 || d.TensionFaseNeutroV > 250m)
                throw new InvalidOperationException(
                    "440-60: un acondicionador de habitación trifásico o de más de 250 V no es de la Parte G; se conecta directo y se " +
                    "calcula con su placa (corriente nominal o MCA).");
            if (corriente > 40m)
                throw new InvalidOperationException(
                    $"440-62(a)(2): {corriente:0.##} A pasa de los 40 A de un acondicionador de habitación; se calcula con su placa (corriente nominal o MCA).");

            // Una sola unidad de motor: conductor al 125 % — 440-62(a), 440-32.
            capacidadMinConductor = 1.25m * corriente;
            citas.Add(new Cita("440-62(a)",
                $"Acondicionador de aire para habitación con cordón y clavija: una sola unidad de motor, con su corriente total de " +
                $"placa, {corriente:0.##} A. Conductor al 125 %: {capacidadMinConductor:0.##} A — 440-32."));

            // 440-62(b): la corriente marcada no excede el 80 % del valor del circuito — el circuito es, al
            // menos, el tamaño estándar que la deja en 80 %.
            breaker = proteccionEstandar.SiguienteEstandar(corriente / 0.8m);
            citas.Add(new Cita("440-62(b)",
                $"Sin otras cargas, {corriente:0.##} A no debe exceder el 80 % del circuito: {corriente:0.##} A ÷ 0.8 = " +
                $"{corriente / 0.8m:0.##} A → {breaker:0.##} A."));
            citas.Add(new Cita("440-62(a)(4)",
                "La protección no excede la ampacidad del conductor ni el valor nominal del contacto: el contacto, de no menos " +
                $"de {breaker:0.##} A."));
        }
        else if (d.EsPorAmpacidadYProteccion)
        {
            // 440-4(b): la ampacidad y la protección las marcó el fabricante con las Partes C y D.
            var ampacidadMinima = d.AmpacidadMinimaPlacaA!.Value;
            if (ampacidadMinima <= 0m)
                throw new InvalidOperationException("440-4(b): falta la ampacidad mínima de los conductores que marca la placa.");
            if (d.ProteccionMaximaPlacaA is not { } maxima || maxima <= 0m)
                throw new InvalidOperationException("440-4(b): falta la protección máxima que marca la placa.");

            corriente = ampacidadMinima;
            capacidadMinConductor = ampacidadMinima;
            citas.Add(new Cita("440-4(b)",
                $"Ampacidad mínima de los conductores, de la placa: {ampacidadMinima:0.##} A. El fabricante ya la calculó con la " +
                "Parte D, con el 125 % del motor mayor; no se le vuelve a aplicar."));

            // «No debe exceder» la marcada: el mayor tamaño estándar que no la pase.
            breaker = proteccionEstandar.AnteriorEstandar(maxima)
                ?? throw new InvalidOperationException(
                    $"440-4(b): la protección máxima de placa ({maxima:0.##} A) es menor que el tamaño estándar más chico.");
            if (breaker < ampacidadMinima)
                throw new InvalidOperationException(
                    $"440-4(b): ningún tamaño estándar queda entre la ampacidad mínima ({ampacidadMinima:0.##} A) y la protección " +
                    $"máxima ({maxima:0.##} A) de la placa. Revisa los dos datos.");
            citas.Add(new Cita("440-4(b)",
                breaker == maxima
                    ? $"Protección: {breaker:0.##} A, la máxima que marca la placa."
                    : $"Protección: {breaker:0.##} A, el mayor tamaño estándar que no excede la máxima de placa ({maxima:0.##} A)."));
        }
        else
        {
            if (d.CorrienteNominalPlacaA is not { } nominal || nominal <= 0m)
                throw new InvalidOperationException("440-6(a): falta la corriente de carga nominal de la placa.");

            var r = CalculadoraCarga440.Calcular(proteccionEstandar, nominal, d.CorrienteSeleccionCircuitoA, d.RequiereArranque);
            citas.AddRange(r.Citas);
            corriente = r.CorrienteBaseA;
            capacidadMinConductor = r.CorrienteConductorA;
            breaker = r.ProteccionCortocircuitoA;
        }
        if (!d.EsDeHabitacion)
            citas.Add(new Cita("240-4(g)",
                "La protección del derivado puede quedar arriba de la ampacidad del conductor: protege contra cortocircuito y falla " +
                "a tierra (Art. 440, Partes C y F). La sobrecarga la cuida el protector del motocompresor — 440-52."));

        // 4. Temperatura de terminales -- 110-14(c)(1), con la declaración de 75 °C como en los demás derivados.
        var tempTerminales = TemperaturaTerminales.Para(
            breaker, d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco));
        citas.Add(new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, tempTerminales)));

        // 4.5. Aislamiento -- 110-14(c).
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) cubre los {(int)tempTerminales}°C de la terminal."));

        // 5. Factores de corrección, en la columna del aislamiento -- 310-15(b)(2)(a)/(3)(a).
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

        // 6-8. Calibre por ampacidad y por caída. La caída, con la corriente de placa; con la
        // ampacidad mínima, con ella, del lado seguro (ya trae el 25 % del motor mayor).
        var tensionEfectiva = d.NumeroFases == 1 ? d.TensionFaseNeutroV : d.TensionFaseFaseV;
        var seleccion = SeleccionConductor.Seleccionar(
            catalogo, ampacidad, impedancia,
            capacidadMinConductorA: capacidadMinConductor,
            corrienteParaCaidaA: corriente,
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
            // En uno de habitación el conductor cubre la protección, sin 240-4(b) — 440-62(a)(4).
            proteccionEstandar: d.EsDeHabitacion ? proteccionEstandar : null,
            proteccionA: d.EsDeHabitacion ? breaker : null,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);
        citas.AddRange(seleccion.Citas);

        var calibreFinal = seleccion.CalibreFase;
        var nParalelo = seleccion.NumeroConductoresParalelo;

        // 10. Tierra -- Tabla 250-122, con la protección del derivado.
        var (calibreTierra, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: breaker,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: calibreFinal,
            nParalelo: nParalelo);
        citas.AddRange(citasTierra);

        return new ResultadoCircuitoDerivado(
            CorrienteDisenoA: corriente,
            ProteccionA: breaker,
            CalibreFase: calibreFinal,
            CalibreNeutro: calibreFinal,
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
