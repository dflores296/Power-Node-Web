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
/// prioridad al conductor (240-4) o manual. Ver <see cref="ProteccionDentroDelRango"/>.
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
        // Con la Excepción 2 declarada, el motor no arranca con lo de la tabla: el rango empieza arriba.
        var maximo = proteccion.SeleccionadaA;
        var (minimo, valores) = ProteccionDentroDelRango.Rango(
            proteccionEstandar, capacidadMinConductor, maximo, excepcion2 ? proteccion.ProteccionA : null);
        var criterio = d.CriterioProteccion;
        var (inicial, paraLaColumna) = ProteccionDentroDelRango.Inicial(criterio, d.ProteccionElegidaA, minimo, maximo, valores);
        var indiceDelRango = citas.Count;

        // 4. Temperatura de terminales -- 110-14(c)(1). Con equipo marcado 75 °C la columna depende
        // también del aislamiento, igual que en el circuito no-motor (M-06).
        //
        // CON PRIORIDAD AL CONDUCTOR, LA COLUMNA ES LA DEL PISO DEL RANGO, y la protección se queda en la
        // misma regla de 110-14(c)(1) (paso 8 de M-20): con 60 °C, no pasa de 100 A. Recalcular con la
        // columna de la protección no termina: 30 HP a 220 V oscila entre 1 AWG con 110 A y 3 AWG con
        // 100 A. Con el tope, la columna de la protección es siempre la del piso.
        var tempTerminales = TemperaturaTerminales.Para(
            paraLaColumna, d.TerminalesMarcadas75C, aislamiento.TemperaturaMaxima(d.TipoAislamiento, d.Lugar));
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
        // 110-14(c)(1)a.(4) —el motor de diseño B, C, D o E— entra en el paso 5.5, con la terminal del motor.
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
        // El mínimo lo pone la terminal del motor (paso 5.5).
        Calibre? minimoPorElMotor = null;
        SeleccionConductor.Resultado Seleccionar(decimal? protegidoPorA) => SeleccionarCon(tempTerminales, protegidoPorA, minimoPorElMotor);
        SeleccionConductor.Resultado SeleccionarCon(TemperaturaAislamiento terminales, decimal? protegidoPorA, Calibre? minimoBase) => SeleccionConductor.Seleccionar(
            catalogo, ampacidad, impedancia,
            capacidadMinConductorA: capacidadMinConductor,
            corrienteParaCaidaA: flc,
            numeroConductoresParaleloCapturado: d.NumeroConductoresParalelo,
            factorTemp: factorTemp,
            factorAgrup: factorAgrup,
            materialConductor: d.MaterialConductor,
            materialCanalizacion: d.MaterialCanalizacion,
            tempAislamiento: tempAislamiento,
            tempTerminales: terminales,
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
            maxNParaleloAutoResuelto: d.MaxConductoresParaleloAutomatico,
            calibreMinimoBase: minimoBase);

        // 5.5. LA TERMINAL DEL MOTOR Y DEL ARRANCADOR — Power Node Web, M-21 (AM-4, CONFIRMADA · David ·
        // 2026-10-05). La columna es la más baja de las terminales del circuito: «la temperatura nominal de
        // operación del conductor … debe seleccionarse … de manera que no exceda la temperatura nominal más baja
        // de cualquier terminación» — 110-14(c). En un extremo está el interruptor (paso 4); en el otro, el motor
        // y su arrancador, cuyo marcado no se conoce: con conductor de 14 a 1 AWG, 60 °C (110-14(c)(1)a.); mayor
        // que 1 AWG, 75 °C (110-14(c)(1)b.). Si el proyectista declara un motor de diseño B, C, D o E con
        // arrancador marcado 75 °C, 75 °C — a.(3) y a.(4). Solo cambia algo con la terminal del interruptor en
        // 75 °C: en 60 °C ya manda ella.
        //
        // EL CALIBRE DECIDE LA TERMINAL Y LA TERMINAL EL CALIBRE: se resuelve con el conductor que pide la
        // ampacidad en la columna de 60 °C. Si es de 1 AWG o menor, la terminal del motor es de 60 °C y ese es
        // el conductor. Si pasa de 1 AWG, la terminal es de 75 °C, y en esa columna el conductor no puede bajar
        // de 1/0 AWG: con 1 AWG o menos volvería a ser de 60 °C, y no alcanzaría.
        var terminalDelInterruptor = tempTerminales;
        Cita? citaDelMotor = null;
        if (terminalDelInterruptor == TemperaturaAislamiento.T75 && d.MotorYArrancadorMarcados75C)
            citaDelMotor = new Cita("110-14(c)(1)a.(4)",
                "Terminal del motor y del arrancador: motor de diseño B, C, D o E y arrancador marcado 75°C -> 75°C " +
                "(110-14(c)(1)a.(3) y a.(4)). Las dos terminales del circuito, a 75°C.");
        else if (terminalDelInterruptor == TemperaturaAislamiento.T75)
        {
            var unoCero = catalogo.Listar().Single(c => c.Designacion == "1/0");
            var a60 = SeleccionarCon(TemperaturaAislamiento.T60, null, null).CalibreBase;
            if (a60.AreaMm2 < unoCero.AreaMm2)
            {
                tempTerminales = TemperaturaAislamiento.T60;
                citaDelMotor = new Cita("110-14(c)",
                    $"Terminal del motor y del arrancador: con {a60.DesignacionConUnidad} (de 14 a 1 AWG) y sin marcado de 75°C, 60°C " +
                    "(110-14(c)(1)a.). Manda la más baja de las terminales del circuito: el conductor va en la columna de 60°C. " +
                    "Con un motor de diseño B, C, D o E y el arrancador marcado 75°C se permite 75°C — 110-14(c)(1)a.(3) y a.(4).");
            }
            else
            {
                minimoPorElMotor = unoCero;
                citaDelMotor = new Cita("110-14(c)",
                    $"Terminal del motor y del arrancador: en la columna de 60°C el conductor sería {a60.DesignacionConUnidad}, mayor que " +
                    "1 AWG, y con conductor mayor que 1 AWG la terminal es de 75°C (110-14(c)(1)b.). Las dos terminales del circuito, " +
                    "a 75°C, con 1/0 AWG como mínimo: con 1 AWG o menos la del motor sería de 60°C.");
            }
        }
        else if (d.MotorYArrancadorMarcados75C)
            citaDelMotor = new Cita("110-14(c)",
                "El motor y el arrancador están marcados 75°C, pero la terminal del interruptor es de 60°C: manda la más baja.");

        // PRIORIDAD AL CONDUCTOR — M-20: ver ProteccionDentroDelRango.Escoger. El tope de 100 A es el de la
        // terminal del interruptor (paso 8 de M-20); la ampacidad, la de la columna que quedó.
        var escogida = ProteccionDentroDelRango.Escoger(
            criterio, inicial, minimo, valores, terminalDelInterruptor, proteccionEstandar, d.MaterialConductor, Seleccionar,
            c => SeleccionConductor.AmpacidadUtilizable(ampacidad, c, d.MaterialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, d.MetodoInstalacion));
        var seleccion = escogida.Seleccion;
        var breaker = escogida.ProteccionA;
        citas.AddRange(seleccion.Citas);

        citas.Insert(indiceDeTerminales, new Cita("110-14(c)(1)", TemperaturaTerminales.Explicacion(breaker, d.TerminalesMarcadas75C, terminalDelInterruptor)));
        if (citaDelMotor is not null)
            citas.Insert(indiceDeTerminales + 1, citaDelMotor);

        var rango = ProteccionDentroDelRango.Armar(
            regla: "430-52(c)(1)",
            techo: "el valor de la Tabla 430-52",
            piso: d.Servicio is not null ? $"{capacidadMinConductor:0.##} A de 430-22(e)" : $"125 % de la FLC = {capacidadMinConductor:0.##} A",
            sobrecarga: "la sobrecarga del motor y del conductor la da la protección que exige 430-32 (relevador en el arrancador o motor «Protegido térmicamente») — 430-31",
            capacidadMinimaA: capacidadMinConductor,
            minimoA: minimo, maximoA: maximo,
            // 430-62(a), 430-63(1): «el valor máximo permitido … de acuerdo con 430-52» (pregunta 6).
            maximoPermitidoA: proteccion.ProteccionExcepcion2A ?? proteccion.MaximoA,
            valores: valores, criterio: criterio, elegidaA: d.ProteccionElegidaA,
            soloArribaDeA: excepcion2 ? proteccion.ProteccionA : null,
            e: escogida, tabla: proteccionEstandar, material: d.MaterialConductor,
            arranque: (p, max) => new Cita("430-52(b)",
                $"La protección debe soportar la corriente de arranque del motor. Verificar con la curva del interruptor que {p:0.##} A " +
                $"no dispara al arrancar; si dispara, subir hasta {max:0.##} A" + (proteccion.TechoExcepcion2A is null ? " o declarar la Excepción 2." : ".")),
            tabla430_52: proteccion);
        citas.InsertRange(indiceDelRango, ProteccionDentroDelRango.Citas(rango, seleccion.CalibreFase, seleccion.AmpacidadUtilizableTotalA));

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
            Rango: rango,
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
