using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Calcula un circuito derivado de Alumbrado o Contactos (Art. 210). Fuerza (Art. 430) tiene su
/// propia metodología — ver <see cref="CalculadoraCircuitoDerivadoNoMotor"/> no aplica ahí.
/// </summary>
public class CalculadoraCircuitoDerivadoNoMotor(
    ICatalogoCalibres catalogo,
    ITablaAmpacidad ampacidad,
    ITablaProteccionEstandar proteccionEstandar,
    ITablaCorreccionTemperatura correccionTemperatura,
    ITablaAgrupamiento agrupamiento,
    ITablaPuestaTierra puestaTierra,
    ITablaImpedancia impedancia,
    ITablaAislamiento aislamiento)
{
    public ResultadoCircuitoDerivado Calcular(DatosEntradaCircuitoDerivadoNoMotor d)
    {
        if (d.TipoCarga == TipoCarga.Fuerza)
            throw new ArgumentException("Fuerza usa su propia metodología (Art. 430), no CalculadoraCircuitoDerivadoNoMotor.", nameof(d));

        var citas = new List<Cita>();

        // 1. Corriente de diseño — 210-19(a)(1).
        // El divisor salió a TensionDeCalculo el 2026-08-21: ConsumoDePlaca hace esta misma cuenta al
        // revés para convertir amperes de placa a VA, y dos copias que se separaran harían que unos
        // amperes capturados no regresaran como los mismos amperes calculados.
        var tensionEfectiva = TensionDeCalculo.Efectiva(d.NumeroFases, d.TensionFaseNeutroV, d.TensionFaseFaseV);
        var divisor = TensionDeCalculo.Divisor(d.NumeroFases, d.TensionFaseNeutroV, d.TensionFaseFaseV);

        // SIN FACTOR DE DEMANDA, y es norma: el 220-42 dice que esos factores «no se deben aplicar
        // para calcular el número de circuitos derivados». Un derivado se dimensiona con su carga
        // PLENA. Hasta el 2026-08-20 aquí se multiplicaba por el factor, o sea que un factor de 0.5
        // encogía a la mitad el conductor y el interruptor de este mismo circuito.
        //
        // Donde sí va es en el alimentador, sobre la suma de todo lo que cuelga -- 220-40.
        var iContinua = d.CargaContinuaVA / divisor;
        var iNoContinua = d.CargaNoContinuaVA / divisor;
        var in_ = iContinua + iNoContinua;

        citas.Add(new Cita("210-19(a)(1)",
            $"In = {iNoContinua:0.##} A (no continua) + {iContinua:0.##} A (continua) = {in_:0.##} A"));

        // 2. Capacidad mínima de protección y conductor. Son DOS artículos distintos que dan el mismo
        // número: 210-19(a)(1) para el conductor y 210-20(a) para la protección. Se citan los dos
        // porque la memoria de cálculo es lo que se sella, y citar solo el del conductor dejaría sin
        // sustento el amperaje del interruptor.
        //
        // El 125 % baja a 100 % cuando el ensamble está declarado aprobado para operar al 100 % de su
        // valor nominal -- la excepción que traen las dos secciones. Sin declaración, esto devuelve
        // 1.25 y el resultado es idéntico al de siempre. Ver CargaContinua100Pct.
        var continua = CargaContinua100Pct.Para(
            d.ConjuntoAprobado100Pct, d.ModeloProteccionEsDe100Pct, "210-20(a)", "210-19(a)(1)");
        citas.AddRange(continua.Citas);

        var capacidadMin = continua.Factor * iContinua + iNoContinua;
        citas.Add(new Cita("210-20(a)",
            $"Capacidad mínima de la protección: {iNoContinua:0.##} A (no continua) + {continua.Factor * 100m:0}% x {iContinua:0.##} A (continua) = {capacidadMin:0.##} A"));

        // 3. Protección — Tabla 240-6(a) (vive como texto en la Sección 240-6(a), no como Tabla), con piso por caso.
        var breaker = proteccionEstandar.SiguienteEstandar(capacidadMin);
        var pisoBreaker = d.TipoCarga == TipoCarga.Alumbrado ? 15m : d.TipoCarga == TipoCarga.Contactos ? 20m : 0m;
        if (breaker < pisoBreaker) breaker = pisoBreaker;
        citas.Add(new Cita("240-6(a)", $"Capacidad mínima {capacidadMin:0.##} A -> protección estándar {breaker} A" +
            (pisoBreaker > 0 ? $" (piso de {pisoBreaker} A para {d.TipoCarga})" : "")));

        // 4. Temperatura de terminales — 110-14(c)(1): fija la columna de ampacidad a usar (el techo
        // que impone el equipo, sin importar qué tan bueno sea el aislamiento del conductor).
        var tempTerminales = TemperaturaTerminales.Para(breaker);
        citas.Add(new Cita("110-14(c)(1)", $"Protección {breaker} A -> terminales a {(int)tempTerminales}°C"));

        // 4.5. Aislamiento — 110-14(c): el aislamiento capturado debe alcanzar (o superar) la
        // temperatura que exige la terminal; si no, el conductor no es válido para este circuito por
        // más buena ampacidad que tenga en teoría.
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) cubre los {(int)tempTerminales}°C de la terminal."));

        // Va aquí porque hasta este punto no se conoce la temperatura del aislamiento, y el requisito
        // de los 90 °C con un interruptor de 100 % es del fabricante, no de la norma.
        var avisosContinua = new List<string>(continua.Avisos);
        if (CargaContinua100Pct.AvisoDeAislamiento(continua.AlCienPorCiento, tempAislamiento) is { } avisoAislamiento)
            avisosContinua.Add(avisoAislamiento);

        // 5. Factores de corrección — 310-15(b)(2)(a) por temperatura ambiente, 310-15(b)(3)(a) por
        // agrupamiento. Se corrige en la columna del AISLAMIENTO, no de la terminal -- 110-14(c)(1)a.
        // (2)/b.(2): el crédito de un aislamiento mejor se aplica ahí; SeleccionConductor topa el
        // resultado final a la columna de la terminal.
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

        // 6-8. Calibre por ampacidad y por caída de tensión, N de conductores en paralelo
        // (bloque 8: auto-resuelve el caso obligado, sugiere el caso conveniente — ver SeleccionConductor).
        var seleccion = SeleccionConductor.Seleccionar(
            catalogo, ampacidad, impedancia,
            capacidadMinConductorA: capacidadMin,
            corrienteParaCaidaA: in_,
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
            pisoPracticoCalibreMm2: d.PisoPracticoCalibreMm2,
            proteccionEstandar: proteccionEstandar,
            proteccionA: breaker,
            // 240-4(b)(1): la excepción no aplica a un circuito derivado que alimenta más de un
            // contacto de uso general -- Contactos normalmente sí (por eso false); Alumbrado no es
            // ese caso (luminarias, no contactos), así que sí califica.
            permiteExcepcion2404b: d.TipoCarga == TipoCarga.Alumbrado,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);
        citas.AddRange(seleccion.Citas);

        var calibreFinal = seleccion.CalibreFase;
        var caidaPct = seleccion.CaidaTensionPct;
        var nParalelo = seleccion.NumeroConductoresParalelo;

        // 9. Neutro — mismo calibre que fase (310-15(b)(5) decide si cuenta para agrupamiento, no cambia el calibre por default).
        var calibreNeutro = calibreFinal;

        // 10. Tierra — Tabla 250-122 por el amperaje de la PROTECCIÓN, más 250-122(b) (si la fase
        // creció, la tierra crece en la misma proporción de área) y el tope de 250-122(a).
        var (calibreTierraUno, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: breaker,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: calibreFinal,
            nParalelo: nParalelo);
        citas.AddRange(citasTierra);

        return new ResultadoCircuitoDerivado(
            CorrienteDisenoA: in_,
            ProteccionA: breaker,
            CalibreFase: calibreFinal,
            CalibreNeutro: calibreNeutro,
            CalibreTierra: calibreTierraUno,
            CaidaTensionPct: caidaPct,
            TablaAmpacidadId: d.MetodoInstalacion == MetodoInstalacion.AlAireLibre ? "310-15(b)(17)" : "310-15(b)(16)",
            Citas: citas,
            NumeroConductoresParalelo: nParalelo,
            AvisosCargaContinua: avisosContinua,
            // El desglose, para que la memoria se pueda recalcular -- ver DetalleDelCalculo.
            Detalle: new DetalleDelCalculo(
                CapacidadMinimaA: capacidadMin,
                FactorTemperatura: factorTemp,
                FactorAgrupamiento: factorAgrup,
                CapacidadMinimaCorregidaA: capacidadMin / (factorTemp * factorAgrup * nParalelo),
                AmpacidadConductorA: seleccion.AmpacidadUtilizableTotalA,
                TemperaturaTerminalesC: (int)tempTerminales,
                TemperaturaAislamientoC: (int)tempAislamiento,
                ResistenciaOhmKm: seleccion.ResistenciaOhmKm,
                ReactanciaOhmKm: seleccion.ReactanciaOhmKm,
                CaidaTensionV: seleccion.CaidaTensionV));
    }
}
