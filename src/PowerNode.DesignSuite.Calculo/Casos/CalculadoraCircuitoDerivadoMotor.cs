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
/// <b>Power Node Web, M-20:</b> ese valor es un techo. La protección se escoge dentro de un rango —del
/// menor de la serie que lleva el 125 % de la FLC al techo— con un criterio: el máximo (lo de antes),
/// prioridad al conductor (240-4) o manual. Ver <see cref="RangoDeProteccionMotor"/>.
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

        // 1. FLC de tabla -- 430-6(a): nunca de placa, para dimensionar conductor y protección. Un
        // motor marcado en amperes y no en HP entra con los caballos que le corresponden en la tabla,
        // interpolando (430-6(a)(1)): su FLC es esa misma corriente.
        decimal flc;
        if (d.FlcMarcadaEnAmperesA is { } marcada)
        {
            flc = marcada;
            citas.Add(new Cita("430-6(a)(1)", $"Motor marcado en amperes y no en HP: {flc:0.##} A es la FLC de un motor de " +
                $"{d.Hp:0.##} Hp en la tabla, interpolando ({d.TensionNominalMotorV} V, {d.TipoAlimentacion})"));
        }
        else
        {
            flc = FlcDeTabla(flcMotor, d.Hp, d.TipoAlimentacion, d.TensionNominalMotorV);
            citas.Add(new Cita("430-6(a)", $"FLC de tabla ({d.Hp} Hp, {d.TensionNominalMotorV} V, {d.TipoAlimentacion}): {flc:0.##} A"));
        }

        // SI EL NÚMERO NO ES EL QUE PUBLICA EL DOF, LA MEMORIA LO DICE. Es la contrapartida de
        // ErratasDeLaNorma: una memoria que se aparta del texto publicado sin declararlo no se puede
        // verificar, y entonces no sirve para lo que se entrega. Null en la inmensa mayoría de los
        // cálculos -- hoy hay una sola errata en toda la norma.
        if (d.FlcMarcadaEnAmperesA is null && flcMotor.ErrataAplicada(d.Hp, d.TipoAlimentacion, d.TensionNominalMotorV) is { } errata)
            citas.Add(new Cita(
                $"ERRATA {errata.TablaId}",
                $"{errata.Descripcion}: se calcula con {errata.ValorCorregido:0.##} A en lugar de los "
                + $"{errata.ValorPublicado:0.##} A publicados en el DOF, porque {errata.Sustento}."));

        // 2. Capacidad mínima de conductor -- 430-22: 125% de la FLC de tabla. En servicio no continuo,
        // el porcentaje de la Tabla 430-22(e) sobre la corriente de PLACA -- 430-22(e) (Power Node Web, I-120).
        decimal capacidadMinConductor;
        if (d.Servicio is { } servicio)
        {
            capacidadMinConductor = servicio.PorcentajeTabla * servicio.CorrientePlacaA / 100m;
            citas.Add(new Cita("430-22(e)",
                $"Servicio no continuo: capacidad mínima del conductor {servicio.PorcentajeTabla:0}% x {servicio.CorrientePlacaA:0.##} A " +
                $"(corriente de placa) = {capacidadMinConductor:0.##} A — Tabla 430-22(e)"));
        }
        else
        {
            capacidadMinConductor = 1.25m * flc;
            citas.Add(new Cita("430-22", $"Capacidad mínima del conductor: 125% x {flc:0.##} A = {capacidadMinConductor:0.##} A"));
        }

        // 3. Protección -- Tabla 430-52: techo = FLC x %. La Excepción 1 de 430-52(c)(1) solo deja
        // subir cuando el techo NO es un valor de 240-6(a), y se evalúa contra esa lista, no contra
        // la serie que se instala (Power Node Web, auditoría del 2026-09-29, P1-1).
        var porcentaje = proteccionMotor.PorcentajeMaximo(d.TipoMotor, d.TipoDispositivoProteccion);
        var techoProteccion = flc * porcentaje / 100m;
        //
        // La Excepción 2 —el motor no arranca con ese valor— no se aplica sola: la declara el
        // proyectista (P1-1 de la misma auditoría). Aquí, la del interruptor de tiempo inverso, 2(3).
        var excepcion2 = d.NoArrancaConLaTabla && d.TipoDispositivoProteccion == TipoDispositivoProteccionMotor.InterruptorTiempoInverso;
        var proteccion = ProteccionDeLaTabla430_52(proteccionEstandar, techoProteccion, excepcion2 ? flc : null);
        citas.Add(new Cita("430-52", $"Techo de protección: {porcentaje}% x {flc:0.##} A = {proteccion.Explicacion()}"));
        if (proteccion.ExplicacionExcepcion2() is { } porExcepcion2)
            citas.Add(new Cita("430-52(c)(1) Excepción 2", porExcepcion2));

        // 3.5. EL RANGO — Power Node Web, M-20. 430-52(c)(1) pide un valor «que no exceda» el de la
        // tabla: es un techo, no el valor obligatorio. Va del menor valor de la serie que lleva el 125 %
        // de la FLC (o la capacidad de 430-22(e)) al mayor que no excede el techo; nunca arriba de él.
        // Hasta M-20 la protección era siempre el techo: con una bomba de 1/2 HP, 25 A sobre 14 AWG.
        var (minimo, valores) = RangoDeLaSerie(proteccionEstandar, proteccion, capacidadMinConductor);
        var maximo = proteccion.SeleccionadaA;
        var criterio = d.CriterioProteccion;
        var breaker = criterio == CriterioProteccionMotor.Manual && d.ProteccionElegidaA is { } pedida
            ? MasCercano(valores, pedida)
            : maximo;
        var indiceDelRango = citas.Count;

        // 4. Temperatura de terminales -- 110-14(c)(1). Con equipo marcado 75 °C la columna depende
        // también del aislamiento, igual que en el circuito no-motor (M-06).
        //
        // CON PRIORIDAD AL CONDUCTOR, LA COLUMNA ES LA DEL PISO DEL RANGO, y la protección se queda en la
        // misma regla de 110-14(c)(1) (paso 8 de M-20): con 60 °C, no pasa de 100 A. Recalcular con la
        // columna de la protección no termina: 30 HP a 220 V oscila entre 1 AWG con 110 A y 3 AWG con
        // 100 A. Con el tope, la columna de la protección es siempre la del piso.
        var tempTerminales = TemperaturaTerminales.Para(
            criterio == CriterioProteccionMotor.Conductor ? minimo : breaker,
            d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar));
        var indiceDeTerminales = citas.Count;

        // 4.5. Aislamiento -- 110-14(c): debe alcanzar o superar la temperatura que exige la terminal.
        var tempAislamiento = aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar)
            ?? throw new AislamientoIncompatibleException(
                $"'{d.TipoAislamiento}' no se reconoce, o no es válido para el lugar capturado ({d.Lugar.Nombre()}) -- " +
                $"revisa la Tabla 310-104(a). Designaciones reconocidas: {string.Join(", ", aislamiento.DesignacionesReconocidas)}.");
        if (tempAislamiento < tempTerminales)
            throw new AislamientoIncompatibleException(
                $"El aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C) no alcanza los {(int)tempTerminales}°C que exige la terminal del equipo -- 110-14(c).");
        citas.Add(new Cita("110-14(c)", $"Aislamiento {d.TipoAislamiento} ({(int)tempAislamiento}°C, lugar {d.Lugar.Nombre()}) cubre los {(int)tempTerminales}°C de la terminal."));

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
        //
        // Sin protección, como siempre: el conductor de un motor va por 430-22 y la protección por 430-52
        // (240-4(g)). Con una protección, además queda protegido por ella según 240-4 — M-20.
        SeleccionConductor.Resultado Seleccionar(decimal? protegidoPorA) => SeleccionConductor.Seleccionar(
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
            proteccionEstandar: protegidoPorA is null ? null : proteccionEstandar,
            proteccionA: protegidoPorA,
            // Un motor es un circuito de una sola carga: califica para 240-4(b)(1).
            permiteExcepcion2404b: true,
            metodoInstalacion: d.MetodoInstalacion,
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico);

        var seleccion = Seleccionar(null);

        // PRIORIDAD AL CONDUCTOR — M-20, pasos 5 a 7: el mayor valor del rango que protege al calibre
        // base (el de la ampacidad, antes de subir por caída) según 240-4, 240-4(b) y 240-4(d). Subir el
        // calibre por caída no sube la protección: el conductor más grueso sigue protegido. Si ninguno
        // del rango lo protege (los factores lo dejaron abajo del piso), el piso, y el calibre sube hasta
        // quedar protegido por él.
        var topadoEn100 = false;
        var subioElCalibre = false;
        (Calibre Calibre, decimal AmpacidadA)? protegido = null;
        if (criterio == CriterioProteccionMotor.Conductor)
        {
            var calibreBase = seleccion.CalibreBase;
            var ampacidadBase = (SeleccionConductor.AmpacidadUtilizable(ampacidad, calibreBase, d.MaterialConductor,
                tempAislamiento, tempTerminales, factorTemp, factorAgrup, d.MetodoInstalacion) ?? 0m) * seleccion.NumeroConductoresParalelo;
            var tope = tempTerminales == TemperaturaAislamiento.T60 ? 100m : decimal.MaxValue;
            var protegen = valores.Where(v => Protege(proteccionEstandar, v, ampacidadBase, calibreBase, d.MaterialConductor)).ToList();
            topadoEn100 = protegen.Any(v => v > tope);
            protegen.RemoveAll(v => v > tope);
            subioElCalibre = protegen.Count == 0;
            breaker = subioElCalibre ? minimo : protegen.Max();
            seleccion = Seleccionar(breaker);
            // El que se protegió es el base (si subió por caída, el más grueso sigue protegido).
            protegido = (seleccion.CalibreBase, (SeleccionConductor.AmpacidadUtilizable(ampacidad, seleccion.CalibreBase, d.MaterialConductor,
                tempAislamiento, tempTerminales, factorTemp, factorAgrup, d.MetodoInstalacion) ?? 0m) * seleccion.NumeroConductoresParalelo);
        }
        citas.AddRange(seleccion.Citas);

        citas.Insert(indiceDeTerminales, new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, tempTerminales)));

        var protegeAlConductor = Protege(proteccionEstandar, breaker, seleccion.AmpacidadUtilizableTotalA, seleccion.CalibreFase, d.MaterialConductor);
        var rango = new RangoDeProteccionMotor(
            Tabla430_52: proteccion,
            CapacidadMinimaA: capacidadMinConductor,
            MinimoA: minimo,
            MaximoA: maximo,
            Valores: valores,
            Criterio: criterio,
            ProteccionA: breaker,
            ProtegeAlConductor: protegeAlConductor,
            PorExcepcion240_4b: protegeAlConductor && breaker > seleccion.AmpacidadUtilizableTotalA,
            TopadoEn100A: topadoEn100,
            SubioElCalibre: subioElCalibre,
            PedidaA: criterio == CriterioProteccionMotor.Manual && d.ProteccionElegidaA is { } p2 && p2 != breaker ? p2 : null,
            CalibreProtegido: protegido?.Calibre,
            AmpacidadProtegidaA: protegido?.AmpacidadA);
        citas.InsertRange(indiceDelRango, CitasDelRango(rango, seleccion.CalibreFase, seleccion.AmpacidadUtilizableTotalA, d.Servicio is not null));

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
            proteccionParaTierra = ProteccionDeLaTabla430_52(proteccionEstandar, flc * pctFusible / 100m).MaximoA;
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
            RangoMotor: rango,
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
    /// <b>La protección del derivado de un motor</b> — Tabla 430-52 y 430-52(c)(1) Excepción 1.
    ///
    /// <para>
    /// La Excepción 1 aplica cuando el techo «no corresponde» a un valor normalizado: se evalúa contra
    /// la lista de 240-6(a) (<see cref="ITablaProteccionEstandar.ValoresDeLaNorma"/>), no contra la
    /// serie que se instala. Un techo de 35 A ya es normalizado: el máximo es 35 A aunque la serie
    /// (riel DIN) no lo tenga, y de ella se toma el mayor que no lo excede, 32 A — no 40. Un techo de
    /// 77 A no lo es: el máximo es 80 A, el siguiente de la lista.
    /// </para>
    ///
    /// <para>
    /// Si la serie no tiene ningún tamaño que no exceda el máximo (riel DIN empieza en 16 A), se usa
    /// el máximo de la lista de 240-6(a), marcado <see cref="ProteccionDeMotor.FueraDeLaSerie"/>.
    /// Hallazgo de la auditoría del 2026-09-29 (P1-1): antes se redondeaba hacia arriba dentro de la
    /// serie (35 → 40 en riel DIN) y se citaba la Excepción 1 aunque no hubiera redondeo.
    /// </para>
    /// </summary>
    /// <param name="flcExcepcion2">
    /// La FLC, si el proyectista declara que el motor no arranca con ese valor — Excepción 2(3), con
    /// interruptor de tiempo inverso: se puede aumentar sin exceder 400 % de la FLC (300 % si la FLC
    /// pasa de 100 A). Se toma el mayor tamaño de la serie que no lo excede, si es mayor que el de la
    /// tabla. <c>null</c>: sin Excepción 2, que nunca se aplica sola.
    /// </param>
    public static ProteccionDeMotor ProteccionDeLaTabla430_52(ITablaProteccionEstandar tabla, decimal techoA, decimal? flcExcepcion2 = null)
    {
        var normalizado = tabla.ValoresDeLaNorma.Contains(techoA);
        var maximo = normalizado ? techoA : tabla.SiguienteDeLaNorma(techoA);
        var deLaSerie = tabla.AnteriorEstandar(maximo);
        var p = new ProteccionDeMotor(techoA, maximo, deLaSerie ?? maximo, normalizado, FueraDeLaSerie: deLaSerie is null);
        if (flcExcepcion2 is not { } flc)
            return p;

        var porcentaje = flc <= 100m ? 400m : 300m;
        var techo2 = flc * porcentaje / 100m;
        var mayor = tabla.AnteriorEstandar(techo2);
        return p with
        {
            PorcentajeExcepcion2 = porcentaje,
            TechoExcepcion2A = techo2,
            ProteccionExcepcion2A = mayor is { } b && b > p.ProteccionA ? b : null,
        };
    }

    /// <summary>
    /// <b>El rango de 430-52(c)(1) en la serie que se instala</b> — Power Node Web, M-20.
    /// <list type="bullet">
    /// <item>Máximo: el de hoy, <see cref="ProteccionDeMotor.SeleccionadaA"/> (con la Excepción 2 si se declaró).</item>
    /// <item>Mínimo: el menor valor de la serie que lleva la capacidad mínima del conductor (125 % de la
    /// FLC, o 430-22(e)). Es criterio, no norma: abajo de él el interruptor dispararía antes que el
    /// relevador de 430-32. <b>Nunca arriba del máximo</b>: en riel DIN, un motor de 1/4 HP a 127 V tiene
    /// 15 A de máximo (de la lista de 240-6(a), fuera de la serie) y la serie empieza en 16 A.</item>
    /// <item>Con la Excepción 2 declarada, el motor no arranca con lo de la tabla: el rango empieza arriba
    /// de ella. Si no hay un tamaño mayor, el rango es solo el de la tabla.</item>
    /// </list>
    /// Arriba de la serie (riel DIN pasa de 125 A), los valores de la NOM, como en <see cref="ProteccionDeLaTabla430_52"/>.
    /// </summary>
    internal static (decimal Minimo, IReadOnlyList<decimal> Valores) RangoDeLaSerie(
        ITablaProteccionEstandar tabla, ProteccionDeMotor proteccion, decimal capacidadMinimaA)
    {
        var maximo = proteccion.SeleccionadaA;
        var serie = tabla.ValoresEstandar.Count == 0
            ? tabla.ValoresDeLaNorma
            : [.. tabla.ValoresEstandar, .. tabla.ValoresDeLaNorma.Where(v => v > tabla.ValoresEstandar[^1])];
        var minimo = proteccion.UsaExcepcion2
            ? serie.Where(v => v > proteccion.ProteccionA && v <= maximo).DefaultIfEmpty(maximo).Min()
            : proteccion.TechoExcepcion2A is not null
                ? maximo
                : Math.Min(tabla.SiguienteEstandar(capacidadMinimaA), maximo);
        return (minimo, [.. serie.Where(v => v >= minimo && v <= maximo).Append(minimo).Append(maximo).Distinct().Order()]);
    }

    /// <summary>El valor del rango más cercano a <paramref name="pedidaA"/>; entre dos igual de cerca, el menor.</summary>
    internal static decimal MasCercano(IReadOnlyList<decimal> valores, decimal pedidaA) =>
        valores.OrderBy(v => Math.Abs(v - pedidaA)).ThenBy(v => v).First();

    /// <summary>
    /// <b>El conductor queda protegido por la protección</b> según 240-4: su ampacidad la cubre; o no
    /// es valor normalizado de 240-6(a) y la protección es la inmediata superior, hasta 800 A — 240-4(b);
    /// y sin pasar el tope de los conductores chicos — 240-4(d). La lista de 240-6(a), no la serie.
    /// </summary>
    internal static bool Protege(ITablaProteccionEstandar tabla, decimal proteccionA, decimal ampacidadA, Calibre calibre, MaterialConductor material) =>
        (proteccionA <= ampacidadA
         || (proteccionA <= 800m && !tabla.ValoresDeLaNorma.Contains(ampacidadA) && proteccionA == tabla.SiguienteDeLaNorma(ampacidadA)))
        && (SeleccionConductor.TopeProteccion2404d(calibre, material) is not { } tope || proteccionA <= tope);

    /// <summary>Las citas del rango, del criterio, de la protección del conductor y del arranque — M-20.</summary>
    private static IEnumerable<Cita> CitasDelRango(RangoDeProteccionMotor r, Calibre calibre, decimal ampacidadA, bool servicioNoContinuo)
    {
        var piso = servicioNoContinuo ? $"{r.CapacidadMinimaA:0.##} A de 430-22(e)" : $"125 % de la FLC = {r.CapacidadMinimaA:0.##} A";
        yield return new Cita("430-52(c)(1)", r.MinimoA == r.MaximoA && r.Valores.Count == 1
            ? $"Rango permitido: solo {r.MaximoA:0.##} A" + (r.Tabla430_52.TechoExcepcion2A is not null
                ? " (Excepción 2: el motor no arranca con menos)"
                : $" (ningún valor de la serie entre el {piso} y el máximo)")
            : $"Rango permitido: {r.MinimoA:0.##} A a {r.MaximoA:0.##} A — " + (r.Tabla430_52.UsaExcepcion2
                ? "con la Excepción 2, arriba de lo que da la tabla"
                : $"del menor de la serie que lleva el {piso} al mayor que no excede el máximo") +
              ". La norma pide que la protección «no exceda» el valor de la Tabla 430-52: cualquiera del rango cumple");

        yield return r.Criterio switch
        {
            CriterioProteccionMotor.Conductor => new Cita("240-4",
                r.SubioElCalibre
                    ? $"Criterio «prioridad al conductor»: ningún valor del rango protegía al calibre por ampacidad; {r.ProteccionA:0.##} A, el mínimo, y el calibre sube hasta quedar protegido"
                    : $"Criterio «prioridad al conductor»: {r.ProteccionA:0.##} A, el mayor valor del rango que protege a {r.CalibreProtegido ?? calibre} " +
                      $"({r.AmpacidadProtegidaA ?? ampacidadA:0.##} A), el calibre por ampacidad" +
                      (r.CalibreProtegido is { } cp && cp.Designacion != calibre.Designacion ? $"; {calibre} por caída de tensión sigue protegido" : "") +
                      (r.TopadoEn100A ? "; sin pasar de 100 A: la terminal se queda en 60 °C — 110-14(c)(1)a." : "")),
            CriterioProteccionMotor.Manual => new Cita("430-52(c)(1)",
                $"Criterio «manual»: {r.ProteccionA:0.##} A, la que escogió el proyectista dentro del rango" +
                (r.PedidaA is { } pedida ? $" (pidió {pedida:0.##} A, fuera del rango o de la serie: la más cercana)" : "")),
            _ => new Cita("430-52(c)(1)", $"Criterio «máximo 430-52»: {r.ProteccionA:0.##} A, el mayor del rango"),
        };

        yield return r.ProtegeAlConductor
            ? new Cita(r.PorExcepcion240_4b ? "240-4(b)" : "240-4",
                $"El interruptor de {r.ProteccionA:0.##} A protege a {calibre} ({ampacidadA:0.##} A) según su ampacidad" +
                (r.PorExcepcion240_4b ? ": un escalón arriba de una ampacidad que no es valor normalizado" : ""))
            : new Cita("240-4(g)",
                $"{r.ProteccionA:0.##} A pasa la ampacidad de {calibre} ({ampacidadA:0.##} A): lo permite el Art. 430 (240-4(g)) porque " +
                "la sobrecarga del motor y del conductor la da el relevador del arrancador o el protector térmico del motor — 430-32, 430-31");

        if (r.ProteccionA < r.MaximoA)
            yield return new Cita("430-52(b)",
                $"La protección debe soportar la corriente de arranque del motor: verificar con la curva del interruptor que {r.ProteccionA:0.##} A " +
                $"no dispara al arrancar. Si dispara, se puede subir hasta {r.MaximoA:0.##} A" +
                (r.Tabla430_52.TechoExcepcion2A is null ? " o declarar la Excepción 2." : "."));
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

/// <summary>
/// <b>El rango de la protección del derivado de un motor y lo que se escogió en él</b> — Power Node Web,
/// M-20, decisión <c>proteccion-de-motores-por-rango.md</c>. Ver
/// <see cref="CalculadoraCircuitoDerivadoMotor.RangoDeLaSerie"/>.
/// </summary>
/// <param name="Tabla430_52">El techo de la tabla, sus Excepciones y el mayor de la serie que no lo excede.</param>
/// <param name="CapacidadMinimaA">La del conductor: 125 % de la FLC, o 430-22(e). De ella sale el mínimo.</param>
/// <param name="MinimoA">El menor valor del rango.</param>
/// <param name="MaximoA">El mayor valor del rango: lo que daba el motor antes de M-20.</param>
/// <param name="Valores">Los de la serie entre los dos, de menor a mayor: lo que se puede escoger.</param>
/// <param name="ProteccionA">La que quedó.</param>
/// <param name="ProtegeAlConductor">El conductor que se instala queda protegido por ella según 240-4.</param>
/// <param name="PorExcepcion240_4b">Protegido, pero un escalón arriba de su ampacidad — 240-4(b).</param>
/// <param name="TopadoEn100A">Prioridad al conductor: un valor mayor de 100 A lo protegía, pero la terminal es de 60 °C.</param>
/// <param name="SubioElCalibre">Prioridad al conductor: ninguno lo protegía; quedó el mínimo y el calibre subió.</param>
/// <param name="PedidaA">Manual: la que se pidió, si no estaba en el rango.</param>
/// <param name="CalibreProtegido">Prioridad al conductor: el calibre por ampacidad, el que la protección cubre. Si
/// la caída de tensión lo subió, el instalado es más grueso y sigue protegido.</param>
/// <param name="AmpacidadProtegidaA">La ampacidad utilizable de <paramref name="CalibreProtegido"/>.</param>
public sealed record RangoDeProteccionMotor(
    ProteccionDeMotor Tabla430_52,
    decimal CapacidadMinimaA,
    decimal MinimoA,
    decimal MaximoA,
    IReadOnlyList<decimal> Valores,
    CriterioProteccionMotor Criterio,
    decimal ProteccionA,
    bool ProtegeAlConductor,
    bool PorExcepcion240_4b,
    bool TopadoEn100A = false,
    bool SubioElCalibre = false,
    decimal? PedidaA = null,
    Calibre? CalibreProtegido = null,
    decimal? AmpacidadProtegidaA = null)
{
    /// <summary>La que quedó es el máximo del rango.</summary>
    public bool EsElMaximo => ProteccionA == MaximoA;

    /// <summary>
    /// <b>Lo que entra a 430-62(a) y 430-63(1)</b>: «el valor máximo permitido … de acuerdo con 430-52»,
    /// no la protección escogida (M-20, pregunta 6). El de la lista de 240-6(a) con la Excepción 1 —en
    /// riel DIN, 35 A aunque se instalen 32—, o el de la Excepción 2 si se aplicó.
    /// </summary>
    public decimal MaximoPermitidoA => Tabla430_52.ProteccionExcepcion2A ?? Tabla430_52.MaximoA;
}

/// <summary>
/// La protección del derivado de un motor por la Tabla 430-52 — ver
/// <see cref="CalculadoraCircuitoDerivadoMotor.ProteccionDeLaTabla430_52"/>.
/// </summary>
/// <param name="TechoA">% de la Tabla 430-52 × FLC.</param>
/// <param name="MaximoA">Lo más que se permite: el techo si es valor de 240-6(a); si no, el siguiente de la lista (Excepción 1).</param>
/// <param name="ProteccionA">El mayor tamaño de la serie que no excede el máximo.</param>
/// <param name="TechoNormalizado">El techo es un valor de 240-6(a): la Excepción 1 no aplica.</param>
/// <param name="FueraDeLaSerie">La serie no tiene un tamaño que no exceda el máximo; se usó el de 240-6(a).</param>
/// <param name="PorcentajeExcepcion2">Con la Excepción 2 declarada: 400 % (FLC ≤ 100 A) o 300 %.</param>
/// <param name="TechoExcepcion2A">Con la Excepción 2 declarada: ese porcentaje × FLC.</param>
/// <param name="ProteccionExcepcion2A">El mayor de la serie que no excede ese techo, si es mayor que <paramref name="ProteccionA"/>.</param>
public sealed record ProteccionDeMotor(
    decimal TechoA, decimal MaximoA, decimal ProteccionA, bool TechoNormalizado, bool FueraDeLaSerie,
    decimal? PorcentajeExcepcion2 = null, decimal? TechoExcepcion2A = null, decimal? ProteccionExcepcion2A = null)
{
    /// <summary>La que se instala: la de la Excepción 2 si se declaró y alcanza un tamaño mayor; si no, la de la tabla.</summary>
    public decimal SeleccionadaA => ProteccionExcepcion2A ?? ProteccionA;

    /// <summary>Se declaró que el motor no arranca y hubo un tamaño mayor: la protección es de la Excepción 2.</summary>
    public bool UsaExcepcion2 => ProteccionExcepcion2A is not null;

    /// <summary>Lo que dice la cita de la Excepción 2; <c>null</c> si no se declaró.</summary>
    public string? ExplicacionExcepcion2() => TechoExcepcion2A is not { } techo ? null
        : UsaExcepcion2
            ? $"El motor no arranca con {ProteccionA:0.##} A (declarado por el proyectista): el interruptor de tiempo inverso puede subir sin exceder " +
              $"{PorcentajeExcepcion2:0}% de la FLC = {techo:0.##} A -> {ProteccionExcepcion2A:0.##} A, el mayor tamaño que no lo excede — 2(3)"
            : $"Se declaró que el motor no arranca con {ProteccionA:0.##} A, pero ningún tamaño mayor de la serie queda sin exceder " +
              $"{PorcentajeExcepcion2:0}% de la FLC = {techo:0.##} A — 2(3). Se queda {ProteccionA:0.##} A.";

    /// <summary>Hubo redondeo hacia arriba del techo: solo entonces se cita la Excepción 1.</summary>
    public bool UsaExcepcion1 => ProteccionA > TechoA;

    /// <summary>«35 A, valor normalizado de 240-6(a) -> 32 A, el mayor de la serie que no lo excede».</summary>
    public string Explicacion()
    {
        if (UsaExcepcion1)
            return $"{TechoA:0.##} A, que no es valor normalizado de 240-6(a) -> {ProteccionA:0.##} A " +
                   "(430-52(c)(1) Excepción 1: el valor inmediato superior)";
        if (ProteccionA == TechoA)
            return $"{TechoA:0.##} A -> {ProteccionA:0.##} A (valor normalizado de 240-6(a))";
        var maximo = TechoNormalizado
            ? $"{TechoA:0.##} A, valor normalizado de 240-6(a)"
            : $"{TechoA:0.##} A; la Excepción 1 permite hasta {MaximoA:0.##} A, el siguiente de 240-6(a)";
        return FueraDeLaSerie
            ? $"{maximo} -> {ProteccionA:0.##} A de la lista de 240-6(a): la serie de interruptores no tiene un tamaño que no lo exceda"
            : $"{maximo} -> {ProteccionA:0.##} A, el mayor tamaño de la serie de interruptores que no lo excede";
    }
}
