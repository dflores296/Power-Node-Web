using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Calcula un Alimentador (Art. 215-2/215-3): la carga que atraviesa se trata como
/// "un circuito grande" -- 125% de la parte continua + 100% de la no continua, mismo
/// pipeline de ampacidad (310-15(b)(16)), caída de tensión (Tabla 9, error duro) y
/// tierra (250-122) que <see cref="CalculadoraCircuitoDerivadoNoMotor"/> -- pero sin el piso
/// de 15/20 A de un circuito derivado, que no aplica aquí.
/// </summary>
public class CalculadoraAlimentador(
    ICatalogoCalibres catalogo,
    ITablaAmpacidad ampacidad,
    ITablaProteccionEstandar proteccionEstandar,
    ITablaCorreccionTemperatura correccionTemperatura,
    ITablaAgrupamiento agrupamiento,
    ITablaPuestaTierra puestaTierra,
    ITablaImpedancia impedancia,
    ITablaAislamiento aislamiento)
{
    public ResultadoAlimentador Calcular(DatosEntradaAlimentador d)
    {
        var tensionEfectiva = d.NumeroFases == 1 ? d.TensionFaseNeutroV : d.TensionFaseFaseV;

        // 0. EL FACTOR DE DEMANDA, Y ÉSTE ES SU LUGAR -- 220-40: la carga calculada de un alimentador
        // es la suma de las cargas de los derivados «después de aplicar cualquier factor de demanda
        // aplicable». Se aplica sobre la carga ACUMULADA, que es lo que la cascada ya trae aquí.
        //
        // Ojo con lo que NO toca: la carga de MOTORES no pasa por aquí. El Art. 430 tiene su propia
        // metodología —125 % del mayor más la suma del resto, 430-24— y no admite reducción por
        // demanda; meterla en el mismo saco mezclaría dos artículos que no se mezclan.
        var continuaConDemanda = d.FactorDemandaContinua * d.CargaContinuaVA;
        var noContinuaConDemanda = d.FactorDemandaNoContinua * d.CargaNoContinuaVA;

        // 1-3. Corriente de diseño y protección estándar -- compartido con CalculoTablero.BreakerPrincipalA.
        var proteccion = CalculadoraProteccionAlimentador.Calcular(
            proteccionEstandar, continuaConDemanda, noContinuaConDemanda, d.NumeroFases, d.TensionFaseNeutroV, d.TensionFaseFaseV, d.CargaMotores,
            d.ConjuntoAprobado100Pct, d.ModeloProteccionEsDe100Pct, d.Clase);
        var in_ = proteccion.CorrienteDisenoA;
        var capacidadMin = proteccion.CapacidadMinimaA;
        var breaker = proteccion.ProteccionA;
        var citas = new List<Cita>(proteccion.Citas);

        // 240-21(b): en una derivación, la ampacidad mínima no la fija solo la carga. Las fracciones
        // del inciso (1/3 de la protección del alimentador padre, 1/10 cuando los conductores salen
        // de la envolvente) y el dispositivo donde termina la derivación son PISOS, y el que mande
        // es el mayor. Llega ya resuelto desde VerificadorDerivacion para que esta clase no tenga
        // que conocer la topología.
        if (d.PisoAmpacidadDerivacionA is { } pisoDerivacion && pisoDerivacion > capacidadMin)
        {
            capacidadMin = pisoDerivacion;
            citas.Add(new Cita("240-21(b)",
                $"Conductor de derivación: la ampacidad mínima sube a {pisoDerivacion:0.##} A por las condiciones del inciso."));
        }


        // Un factor de demanda REDUCE COBRE, así que tiene que quedar escrito en la memoria: quien
        // revisa necesita ver de cuánto era la carga antes de reducirla y con qué se justificó.
        if (d.FactorDemandaContinua != 1m || d.FactorDemandaNoContinua != 1m)
            citas.Add(new Cita("220-40",
                $"Factor de demanda sobre la carga acumulada: continua {d.CargaContinuaVA:0.##} VA x {d.FactorDemandaContinua} = "
                + $"{continuaConDemanda:0.##} VA; no continua {d.CargaNoContinuaVA:0.##} VA x {d.FactorDemandaNoContinua} = "
                + $"{noContinuaConDemanda:0.##} VA. Es criterio de diseño del proyectista: el Art. 220 no se automatiza."));

        // 4. Temperatura de terminales -- 110-14(c)(1).
        var tempTerminales = TemperaturaTerminales.Para(breaker);
        citas.Add(new Cita("110-14(c)(1)", $"Protección {breaker} A -> terminales a {(int)tempTerminales}°C"));

        // 4.5. Aislamiento -- 110-14(c): debe alcanzar o superar la temperatura que exige la terminal.
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.LugarInstalacionSeco)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {(d.LugarInstalacionSeco ? "seco" : "húmedo/mojado")}) cubre los {(int)tempTerminales}°C de la terminal."));

        // El aislamiento de 90 °C que exige el fabricante con un interruptor de 100 %. Va aquí y no
        // arriba porque hasta este punto no se conoce la temperatura del aislamiento.
        var avisosContinua = new List<string>(proteccion.AvisosCargaContinua ?? []);
        if (CargaContinua100Pct.AvisoDeAislamiento(proteccion.CargaContinuaAlCienPorCiento, tempAislamiento) is { } avisoAislamiento)
            avisosContinua.Add(avisoAislamiento);

        // 5. Factores de corrección -- 310-15(b)(2)(a)/(3)(a). Se corrige en la columna del
        // AISLAMIENTO -- 110-14(c)(1)a.(2)/b.(2): SeleccionConductor topa el resultado a la terminal.
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
            // 240-4(b)(1) solo excluye "un circuito derivado que alimenta más de un contacto de uso
            // general" -- un Alimentador nunca es un circuito derivado, así que siempre califica.
            //
            // SALVO EN UNA DERIVACIÓN. El encabezado de 240-21(b) lo prohíbe con todas sus letras:
            // "Las disposiciones de 240-4(b) no se deben permitir para conductores de derivación".
            permiteExcepcion2404b: !d.EsDerivacion240_21b,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico,
            // 215-2(a)(1): mismas dos revisiones que 210-19(a)(1). Solo sin motores y fuera de una
            // derivación: 430-24 y 240-21(b) son pisos de ampacidad con su propia regla, y se quedan
            // como estaban (contra la ampacidad corregida).
            cargaAl100PctA: d.CargaMotores.MayorFlcA is null && d.PisoAmpacidadDerivacionA is null ? in_ : null);
        citas.AddRange(seleccion.Citas);

        var calibreFinal = seleccion.CalibreFase;
        var caidaPct = seleccion.CaidaTensionPct;
        var nParalelo = seleccion.NumeroConductoresParalelo;

        // 9. Neutro -- mismo calibre que fase por default.
        var calibreNeutro = calibreFinal;

        // 10. Tierra -- Tabla 250-122 por el amperaje de la protección, con 250-122(b) (la fase
        // crece por caída de tensión muy seguido en un alimentador largo, y la tierra debe crecer
        // con ella) y el tope de 250-122(a).
        var (calibreTierra, citasTierra) = PuestaTierraEquipos.Seleccionar(
            puestaTierra, catalogo,
            proteccionParaTablaA: breaker,
            material: d.MaterialConductor,
            calibreFaseBase: seleccion.CalibreBase,
            calibreFaseFinal: calibreFinal,
            nParalelo: nParalelo);
        citas.AddRange(citasTierra);

        // 11. 430-62(b), la salida del techo -- solo se puede evaluar aquí, porque necesita el
        // conductor ya elegido: "Cuando los conductores del alimentador tengan una ampacidad mayor a
        // la exigida en 430-24, se permitirá que el valor nominal o de ajuste del dispositivo de
        // protección contra sobrecorriente del alimentador se base en la ampacidad de los conductores
        // del alimentador."
        //
        // Pasa seguido en este programa, porque el paso de caída de tensión engorda el conductor muy
        // por encima de lo que pedía la ampacidad: en cuanto eso ocurre, el alimentador que "excedía
        // el techo" deja de excederlo, y el aviso se retira citando el permiso que lo autoriza.
        var techo = proteccion.TechoProteccion430_62A;
        var excedeTecho = proteccion.ProteccionExcedeTecho430_62;

        if (excedeTecho && seleccion.AmpacidadUtilizableTotalA > capacidadMin && breaker <= seleccion.AmpacidadUtilizableTotalA)
        {
            citas.Add(new Cita("430-62(b)",
                $"El conductor instalado ({calibreFinal}{(nParalelo > 1 ? $" x{nParalelo}" : "")}) tiene una ampacidad de " +
                $"{seleccion.AmpacidadUtilizableTotalA:0.##} A, mayor que los {capacidadMin:0.##} A que exigía 430-24, así que se permite basar " +
                $"la protección en la ampacidad del conductor: los {breaker} A quedan permitidos aunque excedan el techo de 430-62(a)."));
            excedeTecho = false;
        }

        return new ResultadoAlimentador(
            CorrienteDisenoA: in_,
            ProteccionA: breaker,
            CalibreFase: calibreFinal,
            CalibreNeutro: calibreNeutro,
            CalibreTierra: calibreTierra,
            CaidaTensionPct: caidaPct,
            TablaAmpacidadId: d.MetodoInstalacion == MetodoInstalacion.AlAireLibre ? "310-15(b)(17)" : "310-15(b)(16)",
            Citas: citas,
            NumeroConductoresParalelo: nParalelo,
            TechoProteccion430_62A: techo,
            ProteccionExcedeTecho430_62: excedeTecho,
            AvisosCargaContinua: avisosContinua,
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
