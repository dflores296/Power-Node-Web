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
        // M-20, fase 2: la regla que pone el techo y lo que hace falta para el rango. Null en uno de
        // habitación: 440-62(b) ya da el mínimo, no hay rango.
        (string Regla, string Techo, string Piso, decimal MaximoPermitidoA, decimal? ArribaDeA, Func<decimal, decimal, Cita> Arranque)? techo = null;
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
                    ? $"Máximo: {breaker:0.##} A, la protección máxima que marca la placa."
                    : $"Máximo: {breaker:0.##} A, el mayor tamaño estándar que no excede la máxima de placa ({maxima:0.##} A)."));
            techo = ("440-4(b)", $"la protección máxima de placa ({maxima:0.##} A)", $"la ampacidad mínima de placa = {ampacidadMinima:0.##} A",
                // 430-62(a): la que marca la placa.
                maxima, null,
                (p, max) => new Cita("440-22(b)",
                    $"La protección debe ser capaz de conducir la corriente de arranque del equipo. Verificar con la curva del interruptor que " +
                    $"{p:0.##} A no dispara al arrancar; si dispara, subir hasta {max:0.##} A."));
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

            var porcentaje = d.RequiereArranque ? CalculadoraCarga440.TechoProteccionArranquePct : CalculadoraCarga440.TechoProteccionPct;
            var techoA = corriente * porcentaje / 100m;
            techo = ("440-22(a)",
                breaker > techoA
                    ? $"el {porcentaje:0} % de {corriente:0.##} A, o {CalculadoraCarga440.ProteccionMinimaA:0} A (Excepción de 440-22(a))"
                    : $"el {porcentaje:0} % de {corriente:0.##} A = {techoA:0.##} A",
                $"125 % de la corriente = {capacidadMinConductor:0.##} A",
                // 430-62(a): «el valor máximo permitido … de acuerdo con 440-22(a)», de la lista de 240-6(a),
                // sin bajar de 15 A.
                Math.Max(CalculadoraCarga440.ProteccionMinimaA, proteccionEstandar.ValoresDeLaNorma.Where(v => v <= techoA).DefaultIfEmpty(0m).Max()),
                // Con el 225 % declarado, no arranca con lo del 175 %: el rango empieza arriba de él.
                d.RequiereArranque ? CalculadoraCarga440.Calcular(proteccionEstandar, nominal, d.CorrienteSeleccionCircuitoA).ProteccionCortocircuitoA : null,
                (p, max) => new Cita("440-22(a)",
                    $"La protección debe ser capaz de conducir la corriente de arranque del motocompresor. Verificar con la curva del " +
                    $"interruptor que {p:0.##} A no dispara al arrancar; si dispara, subir hasta {max:0.##} A" +
                    (d.RequiereArranque ? "." : " o declarar que no arranca al 175 % (hasta 225 %).")));
        }

        // 3.5. EL RANGO — M-20, fase 2: el techo de 440-22(a) o de la placa es un techo («no exceda»), y
        // cualquiera del rango cumple. Ver ProteccionDentroDelRango.
        var maximo = breaker;
        var criterio = d.CriterioProteccion;
        IReadOnlyList<decimal> valores = [breaker];
        var minimo = breaker;
        var paraLaColumna = breaker;
        if (techo is { } t)
        {
            (minimo, valores) = ProteccionDentroDelRango.Rango(proteccionEstandar, capacidadMinConductor, maximo, t.ArribaDeA);
            (breaker, paraLaColumna) = ProteccionDentroDelRango.Inicial(criterio, d.ProteccionElegidaA, minimo, maximo, valores);
        }
        var indiceDelRango = citas.Count;

        // 4. Temperatura de terminales -- 110-14(c)(1), con la declaración de 75 °C como en los demás derivados.
        // Con prioridad al conductor, la del piso del rango (M-20).
        var tempTerminales = TemperaturaTerminales.Para(
            paraLaColumna, d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar));
        var indiceDeTerminales = citas.Count;

        // 4.5. Aislamiento -- 110-14(c).
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({d.Lugar.Nombre()}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {d.Lugar.Nombre()}) cubre los {(int)tempTerminales}°C de la terminal."));

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
        var deHabitacion = breaker;
        SeleccionConductor.Resultado Seleccionar(decimal? protegidoPorA) => SeleccionConductor.Seleccionar(
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
            // En uno de habitación el conductor cubre la protección, sin 240-4(b) — 440-62(a)(4). Con
            // prioridad al conductor (M-20), queda protegido por la escogida, con 240-4(b).
            proteccionEstandar: d.EsDeHabitacion || protegidoPorA is not null ? proteccionEstandar : null,
            proteccionA: d.EsDeHabitacion ? deHabitacion : protegidoPorA,
            permiteExcepcion2404b: !d.EsDeHabitacion,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);

        SeleccionConductor.Resultado seleccion;
        RangoDeProteccion? rango = null;
        if (techo is { } t2)
        {
            var escogida = ProteccionDentroDelRango.Escoger(
                criterio, breaker, minimo, valores, tempTerminales, proteccionEstandar, d.MaterialConductor, Seleccionar,
                c => SeleccionConductor.AmpacidadUtilizable(ampacidad, c, d.MaterialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, d.MetodoInstalacion));
            seleccion = escogida.Seleccion;
            breaker = escogida.ProteccionA;
            rango = ProteccionDentroDelRango.Armar(
                t2.Regla, t2.Techo, t2.Piso, "la sobrecarga la cuida el protector del motocompresor — 440-52",
                capacidadMinConductor, minimo, maximo, t2.MaximoPermitidoA, valores, criterio, d.ProteccionElegidaA, t2.ArribaDeA,
                escogida, proteccionEstandar, d.MaterialConductor, t2.Arranque);
        }
        else
            seleccion = Seleccionar(null);
        citas.AddRange(seleccion.Citas);

        citas.Insert(indiceDeTerminales, new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, tempTerminales)));
        if (rango is not null)
            citas.InsertRange(indiceDelRango, ProteccionDentroDelRango.Citas(rango, seleccion.CalibreFase, seleccion.AmpacidadUtilizableTotalA));

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
