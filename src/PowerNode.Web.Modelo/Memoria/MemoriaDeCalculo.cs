using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Magnitudes;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo.Memoria;

/// <summary>
/// Las <b>nueve secciones</b> de la memoria de cálculo, en el orden del Excel original (hoja
/// «Memoria de Cálculo», levantada en <c>docs/referencia/cuadro-de-carga.md §4</c> del repo de
/// escritorio). Es la misma plantilla que emite la versión de escritorio en Word
/// (<c>Exportacion.Word.SeccionesDeMemoria</c>), portada aquí a datos: los mismos títulos, las
/// mismas fórmulas y las mismas notas, sin OpenXml de por medio para que las pinte el navegador.
///
/// <para>
/// <b>La diferencia con el Excel es de dónde salen las referencias.</b> Ahí estaban escritas a mano
/// en la plantilla —por eso decía «tabla 310-15(b)( )» con el paréntesis vacío, a rellenar—, y aquí
/// <b>las produce el cálculo</b>: el motor sabe cuál de las tablas 310-15(b) usó, a qué temperatura
/// quedaron las terminales y con qué factores corrigió.
/// </para>
/// </summary>
public static class MemoriaDeCalculo
{
    /// <summary>Las hojas del documento: una por circuito con carga, y la del alimentador al final.</summary>
    public static IReadOnlyList<HojaDeMemoria> Hojas(CuadroDeCarga cuadro)
    {
        var hojas = cuadro.Circuitos
            .Where(c => c.Resultado is not null)
            .Select(c => DeCircuito(cuadro, c))
            .ToList();

        if (DelAlimentador(cuadro) is { } alimentador)
            hojas.Add(alimentador);

        return hojas;
    }

    public static HojaDeMemoria DeCircuito(CuadroDeCarga cuadro, CircuitoDelCuadro circuito)
    {
        var r = circuito.Resultado
            ?? throw new InvalidOperationException($"El circuito {circuito.Espacio} no tiene cálculo.");
        var datos = cuadro.Datos;

        var nombre = string.IsNullOrWhiteSpace(circuito.Descripcion)
            ? Etiqueta(circuito)
            : circuito.Descripcion.Trim();

        var equipo = circuito.EsGrupo ? DelGrupo(cuadro, circuito, r)
            : circuito.EsVariador ? DelVariador(circuito, r)
            : circuito.EsMotor ? DelMotor(cuadro, circuito, r, circuito.Hp, circuito.MotorEnAmperes)
            : circuito.EsAireAcondicionado ? DelAireAcondicionado(cuadro, circuito, r)
            : null;
        if (equipo is not null && MedioDeDesconexion(cuadro, circuito, r) is { } desconexion)
            equipo = equipo with { Proteccion = [.. equipo.Proteccion, desconexion] };

        return new HojaDeMemoria(
            Sujeto: $"Circuito {circuito.Espacio} — {nombre}  ·  fase {circuito.Fases}",
            Articulo: circuito.Categoria == CategoriaDeCarga.Tablero ? "215" : circuito.EsMotor ? "430" : circuito.EsAireAcondicionado ? "440" : "210",
            FrecuenciaHz: datos.FrecuenciaHz,
            CargaContinuaVa: circuito.ContinuaVA,
            CargaNoContinuaVa: circuito.NoContinuaVA,
            // La tensión del TRAMO, no la del tablero: un circuito de 1 polo va a fase-neutro.
            TensionV: circuito.Polos == 1 ? datos.TensionFaseNeutroV : datos.TensionFaseFaseV,
            NumeroFases: circuito.Polos,
            // I-75: el neutro cuenta como hilo solo si el circuito lo lleva — como en la sección 5 (I-73).
            NumeroHilos: circuito.Polos + (circuito.LlevaNeutro ? 1 : 0),
            FactorPotencia: circuito.FactorPotencia,
            LongitudM: circuito.LongitudM,
            MaterialConductor: Etiqueta(datos.MaterialConductor),
            CorrienteDisenoA: r.CorrienteDisenoA,
            ProteccionA: r.ProteccionA,
            ConductorFase: r.CalibreFase.Designacion,
            // I-73: el motor da un calibre de neutro aunque el circuito no lo lleve (2 o 3 polos sin
            // «+N», o 3F-3H). La memoria dice lo mismo que la tabla — I-41.
            ConductorNeutro: circuito.LlevaNeutro ? r.CalibreNeutro.Designacion : null,
            ConductorTierra: r.CalibreTierra.Designacion,
            CaidaTensionPct: r.CaidaTensionPct,
            TablaAmpacidadId: r.TablaAmpacidadId,
            ConductoresPorFase: r.NumeroConductoresParalelo,
            Detalle: r.Detalle,
            Citas: r.Citas,
            SerieDeInterruptores: cuadro.Datos.SerieInterruptores.Explicacion(),
            Aislamiento: Aislamiento(cuadro.Datos),
            DesgloseConductor: cuadro.Desglose(circuito)?.Conductor,
            CaidaCombinada: CaidaCombinada(cuadro, circuito),
            NeutroPortador: circuito.LlevaNeutro ? NeutroPortador(cuadro, circuito.Polos, alimentador: false) : null,
            Desglose: Desglose(circuito),
            NotaDelUso: circuito.UsoEfectivo.Nota(),
            NotaDelMotor: cuadro.NotaDelMotorMayor(circuito),
            AvisoDeHabitacion: circuito.AvisoAireDeHabitacion is { } aviso ? aviso[(aviso.IndexOf(':') + 2)..] : null,
            Canalizacion: DeLaCanalizacion(cuadro, circuito.CanalizacionEfectiva),
            Equipo: equipo,
            CargaMotoresVa: circuito.MotorVA,
            ReglasDeClase: circuito.ReglasDeClase);
    }

    /// <summary>
    /// <b>La hoja de un motor</b> — I-15, I-74, Art. 430: la corriente es la de la tabla (430-6(a)), o
    /// la de un motor marcado en amperes con sus caballos interpolados (430-6(a)(1)); el conductor,
    /// 125 % de ella (430-22); la protección, el porcentaje de la Tabla 430-52 y el tamaño inmediato
    /// superior.
    /// </summary>
    private static EquipoDeLaHoja DelMotor(CuadroDeCarga cuadro, CircuitoDelCuadro c, ResultadoCircuitoDerivado r, decimal? hp, MotorEnAmperes? enAmperes)
    {
        var tipo = c.Polos == 3 ? "trifásico" : "monofásico";
        var flc = c.FlcA;
        var porcentaje = cuadro.PorcentajeProteccionMotor(c);
        var (rotuloFlc, origen, descripcion) = enAmperes is { } m
            ? ("Corriente a plena carga (FLC) — 430-6(a)(1)",
               $"{flc:N2} A — motor marcado en amperes: {m.Hp:0.##} HP, {MotoresEnHp.Interpolacion(m, c.Polos)}",
               $"Marcado en {flc:N2} A · {tipo} · {m.Hp:0.##} HP por interpolación — {MotoresEnHp.Interpolacion(m, c.Polos)}, 430-6(a)(1)")
            : ("Corriente a plena carga (FLC) — 430-6(a)",
               $"{flc:N2} A — {cuadro.FuenteDeFlc(c)}",
               $"{MotoresEnHp.Texto(hp ?? 0m)} HP · {tipo} · FLC {flc:N2} A — {cuadro.FuenteDeFlc(c)}, 430-6(a)");

        return new EquipoDeLaHoja(
            Rotulo: "Motor",
            Descripcion: descripcion,
            Proteccion:
            [
                new(rotuloFlc, origen),
                // Servicio no continuo (I-120): el conductor va por la Tabla 430-22(e), sobre la placa.
                r.Citas.FirstOrDefault(x => x.Referencia == "430-22(e)") is { } servicio
                    ? new("Capacidad mínima del conductor — 430-22(e)", servicio.Descripcion["Servicio no continuo: capacidad mínima del conductor ".Length..])
                    : new("Capacidad mínima del conductor — 430-22", $"125 % × {flc:N2} A = {1.25m * flc:N2} A"),
                new("Protección máxima — Tabla 430-52", $"{porcentaje:0} % × {flc:N2} A = {flc * porcentaje / 100m:N2} A (interruptor automático de tiempo inverso)"),
                new("Protección seleccionada — 430-52(c)(1) Excepción 1", $"{r.ProteccionA:N0} A"),
            ],
            Notas:
            [
                "La FLC sale de la tabla, no de la placa — 430-6(a). El interruptor del tablero protege el circuito contra " +
                "cortocircuito y falla a tierra; puede quedar arriba de la ampacidad del conductor — 240-4(g).",
                "La protección contra sobrecarga del motor va en el arrancador (relevador de sobrecarga) o en el propio " +
                "motor — 430-32. No la da el interruptor del tablero.",
            ],
            Corriente: "FLC");
    }

    /// <summary>
    /// <b>La hoja de un motor con variador</b> — I-119, 430 Parte J: la corriente de entrada del variador,
    /// el conductor al 125 % de ella (430-122(a)) y la protección del fabricante (110-3(b)).
    /// </summary>
    private static EquipoDeLaHoja DelVariador(CircuitoDelCuadro c, ResultadoCircuitoDerivado r)
    {
        var tipo = c.Polos == 3 ? "trifásico" : "monofásico";
        var entrada = c.CorrienteEntradaVariadorA;
        return new EquipoDeLaHoja(
            Rotulo: "Motor con variador",
            Descripcion: $"Variador de velocidad · {tipo} · entrada {entrada:N2} A, protección máxima del fabricante {c.ProteccionMaximaVariadorA:N0} A — 430 Parte J",
            Proteccion:
            [
                new("Corriente — 430-122(a)", $"{entrada:N2} A, la nominal de entrada del variador"),
                new("Capacidad mínima del conductor — 430-122(a)", $"125 % × {entrada:N2} A = {1.25m * entrada:N2} A"),
                new("Protección máxima — 110-3(b)", $"{c.ProteccionMaximaVariadorA:N0} A, la que marca el fabricante del variador"),
                new("Protección seleccionada — 110-3(b)", r.ProteccionA == c.ProteccionMaximaVariadorA
                    ? $"{r.ProteccionA:N0} A"
                    : $"{r.ProteccionA:N0} A, el mayor tamaño estándar que no excede la máxima del fabricante"),
            ],
            Notas:
            [
                "Con variador, la corriente del circuito es la de entrada del variador: la FLC del motor y la Tabla 430-52 no se " +
                "usan. El interruptor del tablero puede quedar arriba de la ampacidad del conductor — 240-4(g).",
                "La sobrecarga del motor la da el variador si así lo marca; si no, va aparte — 430-124(a).",
            ],
            Corriente: "corriente de entrada");
    }

    /// <summary>
    /// <b>La hoja de un grupo de motores</b> — I-115: cada máquina con su corriente, la capacidad mínima
    /// de 430-24 y el límite de 430-53(c)(4) (o de 440-22(b)) con sus números. Un grupo de un solo motor
    /// y nada más se calculó como motor, y así se imprime.
    /// </summary>
    private static EquipoDeLaHoja DelGrupo(CuadroDeCarga cuadro, CircuitoDelCuadro c, ResultadoCircuitoDerivado r)
    {
        var maquinas = c.Cargas.Where(a => a.EsMaquina && a.CorrienteUnitariaA > 0m).ToList();
        if (r.Grupo is not { } g)
        {
            var solo = maquinas.Single();
            return DelMotor(cuadro, c, r, solo.Hp, solo.MotorEnAmperes);
        }

        var tipo = c.Polos == 3 ? "trifásico" : "monofásico";
        var tension = c.Polos == 1 ? cuadro.Datos.TensionFaseNeutroV : cuadro.Datos.TensionFaseFaseV;
        var proteccion = new List<RenglonMemoria>();
        foreach (var a in maquinas)
            proteccion.Add(new(
                $"{CuadroDeCarga.NombreDeMaquina(c, a)} — {(a.Clase == ClaseDeAparato.Motocompresor ? "440-6(a)" : a.MotorEnAmperes is null ? "430-6(a)" : "430-6(a)(1)")}",
                $"{(a.Cantidad > 1 ? $"{a.Cantidad} × " : "")}{a.CorrienteUnitariaA:N2} A — {cuadro.OrigenDeLaCorriente(a, c.Polos)}"));
        var divisor = TensionDeCalculo.Divisor(c.Polos, cuadro.Datos.TensionFaseNeutroV, cuadro.Datos.TensionFaseFaseV);
        if (c.ContinuaVA + c.NoContinuaVA > 0m)
            proteccion.Add(new("Otras cargas", $"{c.ContinuaVA / divisor:N2} A continua + {c.NoContinuaVA / divisor:N2} A no continua"));

        var capacidad = r.Citas.First(x => x.Referencia is "430-24" or "440-32" or "440-33" or "440-34");
        proteccion.Add(new($"Capacidad mínima del conductor — {capacidad.Referencia}", capacidad.Descripcion[(capacidad.Descripcion.IndexOf(':') + 2)..]));
        var limite = r.Citas.First(x => x.Referencia == g.Regla && x.Descripcion.StartsWith("Límite"));
        proteccion.Add(new($"Protección máxima — {g.Regla}", limite.Descripcion["Límite de la protección — ".Length..]));
        proteccion.Add(new($"Protección seleccionada — {(g.Limite240_4bA is null ? g.Regla : "430-53(c)(4), 240-4(b)")}",
            g.Limite240_4bA is { } hasta
                ? $"{r.ProteccionA:N0} A: el máximo no lleva la corriente de operación ({g.PisoA:N2} A) y queda abajo de la ampacidad del " +
                  $"conductor; se permite subir hasta {hasta:N0} A"
                : r.ProteccionA > g.TechoA
                    ? $"{r.ProteccionA:N0} A, no se exige menos — 440-22(a) Excepción"
                    : $"{r.ProteccionA:N0} A, el mayor tamaño estándar que no excede el máximo"));

        var notas = new List<string>
        {
            "Todas las máquinas van a la tensión y los polos del circuito. El interruptor del tablero protege el circuito contra " +
            "cortocircuito y falla a tierra; puede quedar arriba de la ampacidad del conductor — 240-4(g).",
            "Cada motor lleva su protección contra sobrecarga (430-32); controladores y relevadores aprobados para instalación " +
            "en grupo con este interruptor, que no pase del que permite 430-40 al relevador del motor más chico — 430-53(c).",
        };
        notas.AddRange(r.Citas.Where(x => x.Referencia is "430-53(c)(6)" or "430-53(a)").Select(x => $"{x.Descripcion} — {x.Referencia}."));

        return new EquipoDeLaHoja(
            Rotulo: c.EsAireAcondicionado ? "Equipo de A/C" : "Grupo de motores",
            Descripcion: $"{Cuantas(maquinas, ClaseDeAparato.Motor, "motor", "motores")}{Cuantas(maquinas, ClaseDeAparato.Motocompresor, "motocompresor", "motocompresores")}" +
                         $"{(c.ContinuaVA + c.NoContinuaVA > 0m ? " y otras cargas" : "")} · {tipo} {tension:0} V · varios motores en un circuito — " +
                         (maquinas.Any(a => a.Clase == ClaseDeAparato.Motocompresor) ? "430-53, 440-22(b)" : "430-53"),
            Proteccion: proteccion,
            Notas: notas,
            Corriente: "corriente");
    }

    /// <summary>
    /// <b>El medio de desconexión mínimo</b> — I-122: 115 % de la corriente a plena carga de un motor
    /// (430-110(a)), de la de placa de un motocompresor (440-12(a)(1)) o de la suma de un grupo
    /// (430-110(c)(2), 440-12(b)(2)). Es del equipo que se instala junto al motor, no del tablero; la
    /// memoria lo dice para que se especifique.
    /// </summary>
    private static RenglonMemoria? MedioDeDesconexion(CuadroDeCarga cuadro, CircuitoDelCuadro c, ResultadoCircuitoDerivado r)
    {
        const string Rotulo = "Medio de desconexión";
        static string Minimo(decimal a, string que) => $"115 % × {a:N2} A = {1.15m * a:N2} A como mínimo: {que}";

        if (c.EsGrupo)
        {
            var divisor = TensionDeCalculo.Divisor(c.Polos, cuadro.Datos.TensionFaseNeutroV, cuadro.Datos.TensionFaseFaseV);
            var suma = c.CorrienteDeMotorA + (c.ContinuaVA + c.NoContinuaVA) / divisor;
            if (r.Grupo is null)
                return new($"{Rotulo} — 430-110(a)", Minimo(suma, "la corriente a plena carga del motor, o un interruptor de motor de HP no menores — 430-110(a) Excepción"));
            return new($"{Rotulo} — {(c.EsAireAcondicionado ? "440-12(b)(2)" : "430-110(c)(2)")}",
                Minimo(suma, "la suma de las corrientes a plena carga del grupo, las otras cargas incluidas"));
        }
        if (c.EsVariador)
            return new($"{Rotulo} — 430-128", Minimo(c.CorrienteEntradaVariadorA, "la corriente nominal de entrada del variador, en su línea de entrada"));
        if (c.EsMotor)
            return new($"{Rotulo} — 430-110(a)", Minimo(c.FlcA, "la corriente a plena carga, o un interruptor de motor de HP no menores — 430-110(a) Excepción"));

        return c.PlacaAire switch
        {
            PlacaDeAireAcondicionado.CorrienteNominal => new($"{Rotulo} — 440-12(a)(1)",
                Minimo(c.CorrienteDeMotorA, "la corriente de carga nominal o la de selección, la mayor")),
            PlacaDeAireAcondicionado.Habitacion => new($"{Rotulo} — 440-63",
                "La clavija y el contacto, si los controles están a no más de 1.80 m del piso o hay un desconectador a la vista del aparato"),
            _ => new($"{Rotulo} — 440-12(b)(2)",
                "115 % de la suma de las corrientes de placa de los motores del equipo, como mínimo; la placa trae la MCA, no esa suma"),
        };
    }

    /// <summary>«3 motores», «, 1 motocompresor»: cuántas máquinas de una clase lleva el grupo; vacío si ninguna.</summary>
    private static string Cuantas(IEnumerable<CargaDelCircuito> maquinas, ClaseDeAparato clase, string una, string varias)
    {
        var n = maquinas.Where(a => a.Clase == clase).Sum(a => a.Cantidad);
        var primera = clase == ClaseDeAparato.Motor || !maquinas.Any(a => a.Clase == ClaseDeAparato.Motor);
        return n == 0 ? "" : $"{(primera ? "" : ", ")}{n} {(n == 1 ? una : varias)}";
    }

    /// <summary>
    /// <b>La hoja de un equipo de A/C o refrigeración</b> — I-74, Art. 440: con la corriente de placa
    /// (440-6(a); conductor 440-32; protección 440-22(a), sin redondear hacia arriba) o con la ampacidad
    /// mínima y la protección máxima que marca la placa (440-4(b)).
    /// </summary>
    private static EquipoDeLaHoja DelAireAcondicionado(CuadroDeCarga cuadro, CircuitoDelCuadro c, ResultadoCircuitoDerivado r)
    {
        var tipo = c.Polos == 3 ? "trifásico" : "monofásico";
        List<RenglonMemoria> proteccion;
        string descripcion;
        if (c.PlacaAire == PlacaDeAireAcondicionado.Habitacion)
        {
            // 440 Parte G — I-117.
            var i = c.CorrienteDeMotorA;
            descripcion = $"Acondicionador de aire para habitación con cordón y clavija · {tipo} · corriente total de placa {c.CorrientePlacaA:N2} A — 440-62(a)";
            proteccion =
            [
                new("Corriente — 440-62(a)", $"{i:N2} A, la total de la placa: una sola unidad de motor"),
                new("Capacidad mínima del conductor — 440-32", $"125 % × {i:N2} A = {1.25m * i:N2} A"),
                new("Circuito mínimo — 440-62(b)", $"{i:N2} A ÷ 0.8 = {i / 0.8m:N2} A: sin otras cargas, no más del 80 % del circuito"),
                new("Protección seleccionada — 440-62(b), 440-62(a)(4)",
                    $"{r.ProteccionA:N0} A, el primer tamaño estándar que lo cumple; no excede la ampacidad del conductor ni el contacto"),
            ];
        }
        else if (c.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion)
        {
            descripcion = $"Motocompresor hermético · {tipo} · placa: ampacidad mínima {c.AmpacidadMinimaA:N2} A, protección máxima " +
                          $"{c.ProteccionMaximaA:N0} A — 440-4(b)";
            proteccion =
            [
                new("Ampacidad mínima del conductor — 440-4(b)", $"{c.AmpacidadMinimaA:N2} A, de la placa (ya trae el 125 % del motor mayor)"),
                new("Protección máxima — 440-4(b)", $"{c.ProteccionMaximaA:N0} A, de la placa"),
                new("Protección seleccionada — 440-4(b)", r.ProteccionA == c.ProteccionMaximaA
                    ? $"{r.ProteccionA:N0} A"
                    : $"{r.ProteccionA:N0} A, el mayor tamaño estándar que no excede la máxima de placa"),
            ];
        }
        else
        {
            var baseA = c.CorrienteDeMotorA;
            var pct = c.ArranqueAl225 ? CalculadoraCarga440.TechoProteccionArranquePct : CalculadoraCarga440.TechoProteccionPct;
            var porSeleccion = c.CorrienteSeleccionA is { } sel && sel > c.CorrientePlacaA;
            descripcion = $"Motocompresor hermético · {tipo} · placa: corriente de carga nominal {c.CorrientePlacaA:N2} A" +
                          (c.CorrienteSeleccionA is > 0m and { } s ? $", de selección del circuito {s:N2} A" : "") + " — 440-6(a)";
            proteccion =
            [
                new(porSeleccion ? "Corriente — 440-6(a) Excepción 1" : "Corriente — 440-6(a)",
                    porSeleccion ? $"{baseA:N2} A, la de selección del circuito derivado" : $"{baseA:N2} A, la de carga nominal de la placa"),
                new("Capacidad mínima del conductor — 440-32", $"125 % × {baseA:N2} A = {1.25m * baseA:N2} A"),
                new("Protección máxima — 440-22(a)", $"{pct:0} % × {baseA:N2} A = {baseA * pct / 100m:N2} A" +
                    (c.ArranqueAl225 ? ": al 175 % no arranca" : "")),
                new("Protección seleccionada — 440-22(a)", $"{r.ProteccionA:N0} A, el mayor tamaño estándar que no excede el máximo"),
            ];
        }

        return new EquipoDeLaHoja(
            Rotulo: "Equipo de A/C",
            Descripcion: descripcion,
            Proteccion: proteccion,
            Notas:
            [
                "La corriente sale de la placa, no de las tablas del Art. 430 — 440-6(a). El interruptor del tablero protege el " +
                "circuito contra cortocircuito y falla a tierra; puede quedar arriba de la ampacidad del conductor — 240-4(g).",
                "La sobrecarga del motocompresor la cuida su protector o el relevador del equipo — 440-52. No la da el " +
                "interruptor del tablero.",
            ],
            Corriente: "corriente");
    }

    /// <summary>
    /// R-09. En un tramo de <b>2 fases + neutro de una estrella</b> (220Y/127) el neutro lleva
    /// aproximadamente la corriente de fase: cuenta como portador — 310-15(b)(5)(2) — y en el
    /// alimentador no se reduce — 220-61(c)(1). <c>null</c> en cualquier otro tramo: en 1F-3H 120/240
    /// el neutro lleva solo el desbalance — (b)(5)(1).
    /// </summary>
    private static string? NeutroPortador(CuadroDeCarga cuadro, int fases, bool alimentador)
    {
        var estrella = SistemaDelTablero.De(cuadro.Datos.Sistema)
            is ConfiguracionTablero.DosFasesDeEstrella or ConfiguracionTablero.TresFasesCuatroHilos;
        if (!estrella || fases != 2)
            return null;

        return "Portador de corriente: en 2 fases + neutro de estrella lleva ≈ la corriente de fase, y se cuenta en " +
               "su canalización. Mismo calibre que la fase" + (alimentador ? "; no se reduce — 220-61(c)(1)." : ".");
    }

    /// <summary>«Alimentador 4.62 % + circuito 2.31 % = 6.94 % — mayor que 5 %». <c>null</c> sin alimentador.</summary>
    private static string? CaidaCombinada(CuadroDeCarga cuadro, CircuitoDelCuadro circuito)
    {
        if (circuito.CaidaCombinadaPct is not { } combinada || circuito.CaidaAlimentadorPct is not { } alimentador)
            return null;

        var limite = DatosDelTablero.CaidaMaxCombinadaPct;
        return $"Alimentador {alimentador:N2} % + circuito {circuito.Resultado!.CaidaTensionPct:N2} % = {combinada:N2} % " +
               (circuito.AvisoCaidaCombinada is null ? $"≤ {limite:N0} %" : $"— mayor que {limite:N0} %");
    }

    /// <summary>
    /// Los renglones de la sección 4 que dicen de dónde sale el factor de agrupamiento: la
    /// canalización, sus portadores y si el ajuste aplica según su tipo — I-39.
    /// </summary>
    private static IReadOnlyList<RenglonMemoria>? DeLaCanalizacion(CuadroDeCarga cuadro, CanalizacionDelTablero? t)
    {
        if (t?.Conteo is not { } conteo || t.Ajuste is not { } ajuste)
            return null;

        var circuitos = t.Circuitos.Count > 1 ? $" — circuitos {string.Join(", ", t.Circuitos.Select(c => c.Espacio))}" : "";
        var renglones = new List<RenglonMemoria>
        {
            new("Canalización", $"{CuadroDeCarga.NombreDe(t)} ({t.Rotulo}, {t.TamanoRotulo}): {conteo.Portadores} portadores{circuitos}"),
            new("Factor de agrupamiento — Tabla 310-15(b)(3)(a)", $"{t.FactorAgrupamiento:N2} · {ajuste.Motivo}"),
        };
        if (t.SumadorAzoteaC > 0m)
            renglones.Add(new("Azotea al sol — Tabla 310-15(b)(3)(c)",
                $"+{t.SumadorAzoteaC:N0} °C: {cuadro.Datos.TemperaturaAmbienteC + t.SumadorAzoteaC:N0} °C para el factor de temperatura"));
        return renglones;
    }

    /// <summary>
    /// La memoria de cada canalización con conductores calculados — I-40: los conductores con su
    /// área y de qué tabla sale, los portadores y el ajuste, el tamaño y la ocupación.
    /// </summary>
    public static IReadOnlyList<(string Sujeto, IReadOnlyList<BloqueMemoria> Bloques)> Canalizaciones(CuadroDeCarga cuadro)
    {
        var hojas = new List<(string, IReadOnlyList<BloqueMemoria>)>();
        foreach (var t in cuadro.TodasLasCanalizaciones)
        {
            if (t.Ocupacion is not { } o || t.Conteo is not { } conteo || t.Ajuste is not { } ajuste)
                continue;

            var bloques = new List<BloqueMemoria>
            {
                new("1. CONDUCTORES",
                    [.. o.Renglones.Select(r => new RenglonMemoria(
                        $"{Mayuscula(r.Conductor.Circuito)} · {Papel(r.Conductor.Papel)}",
                        r.AreaUnitariaMm2 is { } a
                            ? $"{r.Conductor.Descripcion} × {a:N2} mm² = {a * r.Conductor.Cantidad:N2} mm² — {r.Fuente}"
                            : $"{r.Conductor.Descripcion} — {r.Fuente}"))],
                    o.AreaTotalMm2 is { } total ? [$"Suma = {total:N2} mm² ({o.NumeroConductores} conductores)"] : [],
                    ["Se cuentan todos los conductores, incluida la puesta a tierra — Nota 3 del Capítulo 10."]),
                new("2. PORTADORES Y FACTOR DE AGRUPAMIENTO",
                    [.. conteo.Desglose.Select(d => new RenglonMemoria(
                        Mayuscula(d.Circuito), $"{d.Fases} fase(s) + {d.Neutros} neutro(s) — {d.Motivo}"))],
                    [$"Portadores = {conteo.Portadores}", $"F.A. = {t.FactorAgrupamiento:N2}"],
                    [ajuste.Motivo, "El conductor de puesta a tierra no se cuenta — 310-15(b)(6). Entre canalizaciones se mantiene la separación — 310-15(b)(3)(b)."]),
                new("3. TAMAÑO",
                    [],
                    [.. o.Citas.Select(c => $"{c.Referencia}: {c.Descripcion}")],
                    Notas(t)),
            };
            hojas.Add(($"{CuadroDeCarga.NombreDe(t)} — {t.Rotulo}, {t.TamanoRotulo}", bloques));
        }
        return hojas;

        static string Mayuscula(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
        static string Papel(PapelConductor p) => p switch
        {
            PapelConductor.Fase => "fase",
            PapelConductor.Neutro => "neutro",
            _ => "tierra",
        };
        static List<string> Notas(CanalizacionDelTablero t)
        {
            var notas = new List<string>(t.Avisos);
            if (!t.Tipo.EsTubo())
                notas.Add("La reactancia de la Tabla 9 es de tubo conduit; en esta canalización se usa la columna " +
                          $"{Etiqueta(t.MaterialParaTabla9)} como supuesto.");
            if (t.Circuitos.Count > 0)
                notas.Add("Si un circuito pasa por varias canalizaciones, se le asigna la del tramo más desfavorable — 310-15(a)(2).");
            return notas;
        }
    }

    public static HojaDeMemoria? DelAlimentador(CuadroDeCarga cuadro)
    {
        if (cuadro.Alimentador.Resultado is not { } r)
            return null;

        var datos = cuadro.Datos;
        var nombre = string.IsNullOrWhiteSpace(datos.Tablero) ? "del tablero" : $"del tablero {datos.Tablero.Trim()}";

        return new HojaDeMemoria(
            Sujeto: $"Alimentador general {nombre}",
            Articulo: "215",
            FrecuenciaHz: datos.FrecuenciaHz,
            CargaContinuaVa: cuadro.Resumen.ContinuaVA,
            CargaNoContinuaVa: cuadro.Resumen.NoContinuaVA,
            TensionV: cuadro.Alimentador.Polos == 1 ? datos.TensionFaseNeutroV : datos.TensionFaseFaseV,
            NumeroFases: cuadro.Alimentador.Polos,
            NumeroHilos: datos.Hilos,
            // Resulta de las cargas de la fase que gobierna; no se captura.
            FactorPotencia: cuadro.Alimentador.FactorPotencia,
            LongitudM: datos.LongitudAlimentadorM,
            MaterialConductor: Etiqueta(datos.MaterialConductor),
            CorrienteDisenoA: r.CorrienteDisenoA,
            ProteccionA: r.ProteccionA,
            ConductorFase: r.CalibreFase.Designacion,
            ConductorNeutro: cuadro.SistemaConNeutro ? r.CalibreNeutro.Designacion : null,
            ConductorTierra: r.CalibreTierra.Designacion,
            CaidaTensionPct: r.CaidaTensionPct,
            TablaAmpacidadId: r.TablaAmpacidadId,
            ConductoresPorFase: r.NumeroConductoresParalelo,
            Detalle: r.Detalle,
            Citas: r.Citas,
            FaseQueGobierna: FaseQueGobierna(cuadro),
            SerieDeInterruptores: cuadro.Datos.SerieInterruptores.Explicacion(),
            Aislamiento: Aislamiento(cuadro.Datos),
            DesgloseConductor: cuadro.DesgloseDelAlimentador()?.Conductor,
            Minimo220_52VA: cuadro.Resumen.Minimo220_52VA,
            Superficie: cuadro.Resumen.Superficie,
            NeutroPortador: NeutroPortador(cuadro, cuadro.Alimentador.Polos, alimentador: true),
            CaidaPorFase: r.CaidaPorFase,
            CorrienteNeutro: r.CorrienteNeutro,
            TensionFaseNeutroV: datos.TensionFaseNeutroV,
            FactoresDeDemanda: FactoresDeDemanda(datos),
            Canalizacion: DeLaCanalizacion(cuadro, datos.CanalizacionAlimentador),
            CargaMotoresVa: cuadro.Resumen.MotoresVA,
            EtiquetaDeMotores: cuadro.EtiquetaDeMotores,
            ReferenciaDeMotores: cuadro.ReferenciaDeMotores,
            MotoresQueGobiernan: cuadro.Alimentador.Gobierna?.Motores ?? default,
            Techo430_62A: r.TechoProteccion430_62A);
    }

    /// <summary>«Estufa: 1 × 3,000 W = 3,000 VA · no continua · F.P. 1.00». Un renglón por aparato — I-35.</summary>
    /// <summary>
    /// Un renglón por aparato, solo si cuentan (<see cref="CircuitoDelCuadro.TieneDesglose"/>): un
    /// circuito que tuvo aparatos y pasó a Motor o A/C los conserva por si regresa, pero no se calculan
    /// con ellos, y la memoria los imprimía con su VA viejo — I-113.
    /// </summary>
    private static IReadOnlyList<RenglonMemoria> Desglose(CircuitoDelCuadro circuito) =>
    [
        .. Clase(circuito),
        .. !circuito.TieneDesglose && circuito.Categoria != CategoriaDeCarga.Tablero ? [] : circuito.Cargas.Select((a, i) => new RenglonMemoria(
            // El tipo y el subtipo de cada salida o carga — I-123: de ellos salen su F.D. y su mínimo.
            $"{i + 1}. {circuito.TipoDe(a).Nombre()}{(a.Subtipo is { } st && !a.EsTablero ? $" · {st.Nombre()}" : "")}: " +
            $"{(string.IsNullOrWhiteSpace(a.Descripcion) ? "—" : a.Descripcion.Trim())}",
            // Una máquina de un grupo (I-115): su corriente por unidad, que es con la que calcula.
            a.EsTablero
                ? $"continua {a.CargaUnitaria:N0} {Simbolo(a.Unidad)} · no continua {a.NoContinua:N0} {Simbolo(a.Unidad)} = {a.TotalVA:N0} VA, " +
                  $"ya con sus factores de demanda; sin otro aquí — 220-40 · F.P. {a.FactorPotencia:N2}"
            : a.EsMaquina
                ? $"{a.Cantidad} × {(a.Clase == ClaseDeAparato.Motor && a.MotorEnAmperes is null ? $"{MotoresEnHp.Texto(a.Hp ?? 0m)} HP · " : "")}" +
                  $"{a.CorrienteUnitariaA:N2} A = {a.TotalVA:N0} VA · F.P. {a.FactorPotencia:N2}"
                : $"{a.Cantidad} × {a.CargaUnitaria:N0} {Simbolo(a.Unidad)}{(a.ReferenciaMinimo is { } rm ? $", con el mínimo de {rm}," : "")} = {a.TotalVA:N0} VA · " +
                  $"{(a.Continua ? a.ReferenciaContinua is { } rc ? $"continua — {rc}" : "continua" : "no continua")} · F.P. {a.FactorPotencia:N2}")),
        .. Tableros(circuito),
    ];

    /// <summary>
    /// Un alimentador a otros tableros (David, 2026-09-30): cada tablero protegido a no más de su capacidad
    /// — 408-36; con varios, las derivaciones del alimentador a cada uno — 240-21(b).
    /// </summary>
    private static IEnumerable<RenglonMemoria> Tableros(CircuitoDelCuadro circuito)
    {
        var tableros = circuito.Cargas.Count(a => a.EsTablero);
        if (circuito.Categoria != CategoriaDeCarga.Tablero || tableros == 0 || circuito.Resultado is not { } r)
            yield break;
        yield return new RenglonMemoria("Protección de cada tablero",
            $"No mayor que la capacidad del tablero — 408-36. La de este alimentador, {r.ProteccionA:N0} A, basta para " +
            $"{(tableros == 1 ? "el tablero si es" : "cada tablero que sea")} de {r.ProteccionA:N0} A o más; si no, lleva su interruptor principal.");
        if (tableros > 1)
            yield return new RenglonMemoria("Derivaciones a cada tablero",
                "Del alimentador a cada tablero, derivaciones según 240-21(b): hasta 3 m, con ampacidad no menor que la " +
                "carga del tablero ni que la capacidad del dispositivo en que terminan; hasta 7.5 m, con ampacidad no menor " +
                $"que un tercio de esta protección ({r.ProteccionA / 3m:N1} A) y terminando en un solo interruptor o juego de fusibles.");
    }

    /// <summary>
    /// La clase del circuito, con su definición del Art. 100, y si lleva cargas combinadas — I-123. En otro
    /// tablero, que entra sin factor de demanda — 220-40 (I-125).
    /// </summary>
    private static IEnumerable<RenglonMemoria> Clase(CircuitoDelCuadro circuito)
    {
        if (circuito.ClaseDelCircuito is not { } clase)
            yield break;
        yield return new RenglonMemoria("Clase del circuito", clase switch
        {
            ClaseDeCircuito.GrupoDeMotores => "Varios motores en un circuito derivado — 430-53, 440-22(b)",
            ClaseDeCircuito.Individual => "Circuito derivado individual: alimenta a un solo equipo de utilización — Art. 100",
            ClaseDeCircuito.UsoGeneral => "Circuito derivado de uso general: dos o más salidas para alumbrado y aparatos — Art. 100",
            ClaseDeCircuito.ParaAparatos => "Circuito derivado para aparatos: salidas para aparatos, sin alumbrado conectado permanentemente — Art. 100",
            _ => circuito.Cargas.Count(a => a.EsTablero) > 1
                ? "Alimentador a otros tableros: la suma de sus cargas calculadas, sin otro factor de demanda — Art. 100, 215-2(a)(1), 220-40"
                : "Alimentador a otro tablero: la carga calculada de ese tablero, sin otro factor de demanda — Art. 100, 215-2(a)(1), 220-40",
        });
        if (circuito.TieneCargasCombinadas)
            yield return new RenglonMemoria("Cargas combinadas",
                string.Join(" · ", circuito.Porciones.Where(p => p.TotalVA > 0m).Select(p => $"{p.Tipo.NombreCompleto()} {p.TotalVA:N0} VA")) +
                " — cada una con el factor de demanda de su tipo en el alimentador (220 Parte C)");
    }

    /// <summary>
    /// «Mínimo 220-12: 200 m² × 39 VA/m² = 7,800 VA; capturado 200 VA → se agregan 7,600 VA, continuos» —
    /// M-14. Nada sin área servida.
    /// </summary>
    private static IEnumerable<(string, string?)> MinimosPorSuperficie(MinimoPorSuperficie? sp)
    {
        if (sp is null)
            yield break;
        yield return ($"Mínimo de alumbrado general — 220-12, Tabla 220-12",
            $"{sp.Renglon}: {sp.AreaM2:N0} m² × {sp.VaPorM2:N0} VA/m² = {sp.MinimoAlumbradoVA:N0} VA; capturado " +
            $"{sp.AlumbradoCapturadoVA:N0} VA{(sp.IncluyeContactos ? " con los contactos de uso general (220-14(j))" : "")} → " +
            (sp.AjusteAlumbradoVA > 0m ? $"se agregan {sp.AjusteAlumbradoVA:N0} VA, {(sp.AlumbradoContinuo ? "continuos" : "no continuos")}" : "ya lo cubre"));
        if (sp.MinimoContactosVA is { } mc)
            yield return ("Mínimo de contactos — 220-14(k)",
                $"{sp.AreaM2:N0} m² × 11 VA/m² = {mc:N0} VA contra {sp.ContactosCapturadosVA:N0} VA capturados → " +
                (sp.AjusteContactosVA > 0m ? $"se agregan {sp.AjusteContactosVA:N0} VA" : "ya lo cubren"));
    }

    private static string Simbolo(UnidadConsumo unidad) => unidad switch
    {
        UnidadConsumo.Watts => "W",
        UnidadConsumo.Amperes => "A",
        _ => "VA",
    };

    /// <summary>
    /// «F.D. alumbrado — 220-40: 0.80 · Tabla 220-42 — alumbrado general». Un renglón por tipo con
    /// factor menor que 1 — R-12, R-17. Sin justificación lo dice en mayúsculas: la memoria no la
    /// inventa.
    /// </summary>
    private static IReadOnlyList<RenglonMemoria> FactoresDeDemanda(DatosDelTablero datos) =>
    [
        .. CategoriasDeCarga.Todas
            .Where(c => datos.FactorDeDemanda(c) < 1m)
            .Select(c => new RenglonMemoria(
                $"F.D. {c.NombreCompleto().ToLowerInvariant()} — 220-40",
                $"{datos.FactorDeDemanda(c):N2} · {datos.JustificacionDe(c) ?? "SIN JUSTIFICACIÓN"}")),
    ];

    /// <summary>
    /// «Fase C, la más cargada: 125 % × 12.20 A + 0.00 A = 15.25 A». Es lo que explica por qué la
    /// corriente de diseño del alimentador no es la carga total entre √3·V: el alimentador se
    /// dimensiona con la barra que más lleva — M-02.
    /// </summary>
    private static string? FaseQueGobierna(CuadroDeCarga cuadro)
    {
        if (cuadro.Alimentador.Gobierna is not { } g || cuadro.Alimentador.Fases is not { Count: > 1 } fases)
            return null;

        return $"Fase {g.Fase}, la más cargada: {g.FactorContinua * 100m:0} % × {g.ContinuaA:N2} A (continua) + " +
               $"{g.NoContinuaA:N2} A (no continua)" +
               (g.TieneMotores ? $" + {g.Motores.CapacidadMinimaA:N2} A ({cuadro.EtiquetaDeMotores.ToLowerInvariant()}, {cuadro.ReferenciaDeMotores})" : "") +
               $" = {g.CapacidadA:N2} A. Las demás: " +
               string.Join(", ", fases.Where(f => f.Fase != g.Fase).Select(f => $"fase {f.Fase} {f.CapacidadA:N2} A")) +
               ". El alimentador se dimensiona con la corriente de esta fase, no con la carga total repartida.";
    }

    /// <summary>Las nueve secciones de una hoja, en el orden en que se imprimen.</summary>
    public static IReadOnlyList<BloqueMemoria> Secciones(HojaDeMemoria hoja)
    {
        var d = hoja.Detalle;
        var cargaTotal = hoja.CargaContinuaVa + hoja.CargaNoContinuaVa + hoja.CargaMotoresVa;
        var senTheta = TrianguloPotencias.SenoDelAngulo(hoja.FactorPotencia);
        var bloques = new List<BloqueMemoria>();
        var m = hoja.Equipo;

        // ---- 1
        var seccion1 = Seccion("1. DATOS DEL SISTEMA", [
            ("Carga total conectada", $"{cargaTotal:N0} VA"),
            (m?.Rotulo ?? "Motor", m is null ? null
                : $"{m.Descripcion}. {hoja.CargaMotoresVa:N0} VA = {m.Corriente} × tensión{(hoja.NumeroFases == 3 ? " × √3" : "")}"),
            ("Carga continua", m is null ? $"{hoja.CargaContinuaVa:N0} VA" : null),
            ("Carga no continua", m is null ? $"{hoja.CargaNoContinuaVa:N0} VA" : null),
            (hoja.EtiquetaDeMotores, m is null && hoja.CargaMotoresVa > 0m
                ? $"{hoja.CargaMotoresVa:N0} VA — entran al alimentador con su corriente, {hoja.ReferenciaDeMotores}"
                : null),
            ("Mínimo 220-52", hoja.Minimo220_52VA > 0m
                ? $"{hoja.Minimo220_52VA:N0} VA — aparatos pequeños y lavadora a 1,500 VA por circuito, 220-52(a) y (b)"
                : null),
            .. MinimosPorSuperficie(hoja.Superficie),
            ("Carga calculada", hoja.Minimo220_52VA > 0m || hoja.Superficie is { AjusteTotalVA: > 0m }
                ? $"{cargaTotal + hoja.Minimo220_52VA + (hoja.Superficie?.AjusteTotalVA ?? 0m):N0} VA" : null),
            ("Tensión nominal", $"{hoja.TensionV:N1} V"),
            ("Frecuencia", $"{hoja.FrecuenciaHz} Hz"),
            ("Factor de potencia", hoja.Sujeto.StartsWith("Alimentador general", StringComparison.Ordinal)
                ? $"{hoja.FactorPotencia:N2} — resulta de combinar las cargas de la fase que gobierna"
                : $"{hoja.FactorPotencia:N2}"),
            ("Fases / hilos", $"{hoja.NumeroFases} / {hoja.NumeroHilos}")]);
        bloques.Add(seccion1 with
        {
            Renglones = [.. seccion1.Renglones, .. hoja.Desglose ?? [], .. hoja.FactoresDeDemanda ?? []],
        });

        // ---- 2
        bloques.Add(Seccion("2. CONSIDERACIONES", [
            ("Material del conductor", hoja.MaterialConductor),
            ("Aislamiento — Tabla 310-104(a)", hoja.Aislamiento),
            ("Hilos por fase", hoja.ConductoresPorFase.ToString()),
            ("Longitud del tramo", $"{hoja.LongitudM:N2} m"),
            ("Temperatura del aislamiento", d is null ? null : $"{d.TemperaturaAislamientoC} °C"),
            ("Temperatura de terminales", d is null ? null : $"{d.TemperaturaTerminalesC} °C — 110-14(c)(1)"),
            ("Neutro — 310-15(b)(5)(2)", hoja.NeutroPortador)]));

        // ---- 3
        if (m is not null)
        {
            // I-15, I-74: el derivado de un motor (Art. 430) o de un equipo de A/C (Art. 440), ya redactado.
            bloques.Add(new BloqueMemoria(
                "3. SELECCIÓN DE LA PROTECCIÓN",
                [.. m.Proteccion, new("Tamaños de interruptor", hoja.SerieDeInterruptores ?? "—")],
                [],
                m.Notas));
        }
        else
        {
            var articuloProteccion = hoja.Articulo == "215" ? "215-3" : "210-20(a)";
            var motores = hoja.MotoresQueGobiernan;
            bloques.Add(Seccion("3. SELECCIÓN DE LA PROTECCIÓN", [
                ("Fase que gobierna", hoja.FaseQueGobierna),
                ("Corriente de diseño (In)", Amperes(hoja.CorrienteDisenoA)),
                ("Uso del circuito", hoja.NotaDelUso),
                ("Aparato con motor — 220-18(a)", hoja.NotaDelMotor),
                ("Acondicionador de habitación — 440-62", hoja.AvisoDeHabitacion),
                // Las reglas de la clase del circuito — I-124. La que no se cumple, marcada.
                .. (hoja.ReglasDeClase ?? []).Select(x => ($"{(x.Aviso ? "REVISAR — " : "")}{x.Referencia}", (string?)x.Texto)),
                ($"{hoja.EtiquetaDeMotores} — {hoja.ReferenciaDeMotores}", motores.MayorFlcA is { } mayor
                    ? (mayor > 0m
                        ? $"125 % × {mayor:N2} A (el mayor, completo) + {motores.SumaRestoFlcA:N2} A (los demás, con su F.D.) = {motores.CapacidadMinimaA:N2} A"
                        : $"{DesgloseDeSeleccion.Grupo430_24(motores)} = {motores.CapacidadMinimaA:N2} A")
                    : null),
                ($"Capacidad mínima — {articuloProteccion}{(motores.MayorFlcA is null ? "" : $", {hoja.ReferenciaDeMotores}")}", d is null ? null : Amperes(d.CapacidadMinimaA)),
                ("Protección seleccionada — 240-6(a)", Amperes(hoja.ProteccionA, "N0")),
                ("Protección máxima — 430-62(a), 430-63", hoja.Techo430_62A is { } techo
                    ? $"{techo:N2} A: la mayor protección de motor o de equipo de A/C, la corriente de los demás y lo que 215-3 pide para la otra carga"
                    : null),
                ("Tamaños de interruptor", hoja.SerieDeInterruptores)]));
        }

        // ---- 4: cómo se llegó a la ampacidad del conductor, paso por paso. Hasta el 2026-09-23 aquí
        // decía «Icm = In / (FT × FA)» y sustituía la CAPACIDAD MÍNIMA, no In; con 6 agrupados
        // imprimía «= 50 A» y luego un conductor de 40 A, sin decir que esos 50 son de la columna
        // del aislamiento y los 40 el tope de la terminal. Ahora se enseñan las dos columnas.
        bloques.Add(new BloqueMemoria(
            "4. CÁLCULO POR CAPACIDAD",
            hoja.Canalizacion ?? [],
            hoja.DesgloseConductor ?? [],
            [
                "FT es el factor de corrección por temperatura ambiente (Tabla 310-15(b)(2)(a)) y FA el factor de " +
                "ajuste por agrupamiento (Tabla 310-15(b)(3)(a)). Se aplican en la columna del aislamiento; la " +
                "ampacidad que se usa no pasa de la de la terminal — 110-14(c).",
            ],
            // Con los renglones de la canalización arriba, el rótulo quedaría separado de sus fórmulas.
            Introduccion: hoja.Canalizacion is null ? "Ampacidad del conductor elegido:" : null));

        // ---- 5
        bloques.Add(Seccion("5. CONDUCTOR DE FASE SELECCIONADO", [
            ("Calibre", CalibreDe(hoja.ConductorFase, hoja.ConductoresPorFase)),
            ("Conductor de neutro", hoja.ConductorNeutro is null ? "No lleva: la carga va entre fases" : CalibreDe(hoja.ConductorNeutro, 1)),
            ("Ampacidad utilizable", d is { AmpacidadConductorA: > 0m } ? $"{d.AmpacidadConductorA:N2} A" : null)]));

        // ---- 6
        if (hoja.CaidaPorFase is { Count: > 0 } porFase && d is not null)
        {
            // R-02: fase por fase con el neutro. Cada renglón se puede recalcular a mano: Z por la
            // suma fasorial de la corriente de la fase y la del neutro, proyectada sobre su tensión.
            var peor = porFase.Aggregate((max, f) => f.CaidaPct > max.CaidaPct ? f : max);
            var formulasFase = new List<string>
            {
                "e_f = Re[ Z × (I_f + I_N) × conj(û_f) ],   I_N = suma fasorial de las corrientes de fase",
                $"Z = ( {d.ResistenciaOhmKm:N2} + j {d.ReactanciaOhmKm:N2} ) Ω/km × {hoja.LongitudM:N2} m ÷ 1000 / {hoja.ConductoresPorFase}",
            };
            if (hoja.CorrienteNeutro is { } iN)
                formulasFase.Add($"I_N = {iN.Magnitud:N2} A ∠ {iN.AnguloGrados:N1}°");
            formulasFase.AddRange(porFase.Select(f =>
                $"Fase {f.Fase}: I = {f.Corriente.Magnitud:N2} A ∠ {f.Corriente.AnguloGrados:N1}° → e = {f.CaidaV:N2} V ({f.CaidaPct:N2} %)"));
            bloques.Add(new BloqueMemoria("6. CÁLCULO DE CAÍDA DE TENSIÓN", [], formulasFase,
            [
                "R y X en ohm/km, de la Tabla 9 de la NOM-001-SEDE-2012. Todos los valores se muestran con dos decimales; el cálculo usa los completos. El neutro es del mismo calibre que la fase.",
                $"Ángulos respecto a V_AN = 0°; cada corriente, atrasada según el F.P. de sus circuitos. Porcentaje sobre " +
                $"V_FN = {hoja.TensionFaseNeutroV:N2} V. Manda la fase {peor.Fase}.",
            ]));
        }
        else
        {
            var formulas6 = new List<string>
            {
                hoja.NumeroFases == 3
                    ? "e = √3 × L × In × [ R × cos(θ) + X × sen(θ) ] ÷ 1000 / N"
                    : "e = 2 × L × In × [ R × cos(θ) + X × sen(θ) ] ÷ 1000 / N",
            };
            var notas6 = new List<string>();
            if (d is not null)
            {
                var k = hoja.NumeroFases == 3 ? "√3" : "2";
                formulas6.Add(
                    $"e = {k} × {hoja.LongitudM:N2} m × {hoja.CorrienteDisenoA:N2} A × " +
                    $"[ {d.ResistenciaOhmKm:N2} × {hoja.FactorPotencia:N2} + {d.ReactanciaOhmKm:N2} × {senTheta:N2} ] ÷ 1000 / " +
                    $"{hoja.ConductoresPorFase} = {d.CaidaTensionV:N2} V");
                notas6.Add(
                    $"L en m; R y X en ohm/km, de la Tabla 9 de la NOM-001-SEDE-2012. Todos los valores se muestran con dos decimales; el cálculo usa los completos. cos(θ) = {hoja.FactorPotencia:N2}, " +
                    $"sen(θ) = {senTheta:N2}.");
            }
            bloques.Add(new BloqueMemoria("6. CÁLCULO DE CAÍDA DE TENSIÓN", [], formulas6, notas6));
        }

        // ---- 7
        var renglones7 = new List<(string, string)>
        {
            ("Caída de tensión", (d is null ? $"{hoja.CaidaTensionPct:N2} %" : $"{d.CaidaTensionV:N2} V  ({hoja.CaidaTensionPct:N2} %)")
                + (hoja.CaidaPorFase is { Count: > 0 } fs ? $" — fase {fs.Aggregate((m, f) => f.CaidaPct > m.CaidaPct ? f : m).Fase}" : "")),
        };
        if (hoja.CaidaCombinada is { } combinada)
            renglones7.Add(("Caída combinada", combinada));
        renglones7.Add(("Referencia", hoja.Articulo == "215"
            ? "215-2(a)(4), NOTA 2 — caída recomendada: 3 % en el alimentador y 5 % combinada con el derivado"
            : "210-19(a)(1), NOTA 4 — caída recomendada: 3 % en el derivado y 5 % combinada con el alimentador"));
        bloques.Add(Seccion("7. CAÍDA DE TENSIÓN EN EL TRAMO", [.. renglones7]));

        // ---- 8
        bloques.Add(new BloqueMemoria(
            "8. SELECCIÓN DEL CONDUCTOR DE PUESTA A TIERRA",
            [],
            [],
            [
                "Se selecciona de la Tabla 250-122 de la NOM-001-SEDE-2012, entrando con la capacidad del dispositivo " +
                "de protección contra sobrecorriente" + (Amperes(hoja.ProteccionA, "N0") is { } p ? $" ({p})." : "."),
                "Cuando el tamaño nominal de los alimentadores se ajuste para compensar la caída de tensión eléctrica, " +
                "el conductor de puesta a tierra de equipo deberá ajustarse proporcionalmente según el área en mm² de " +
                "su sección transversal. — 250-122(b)",
            ]));

        // ---- 9
        bloques.Add(Seccion("9. CONDUCTOR DE PUESTA A TIERRA SELECCIONADO", [
            ("Calibre", CalibreDe(hoja.ConductorTierra, 1))]));

        return bloques;
    }

    private static BloqueMemoria Seccion(string titulo, (string Rotulo, string? Valor)[] renglones) =>
        new(titulo,
            [.. renglones
                .Where(r => !string.IsNullOrWhiteSpace(r.Valor))
                .Select(r => new RenglonMemoria(r.Rotulo, r.Valor!))],
            [],
            []);

    private static string? CalibreDe(string? designacion, int porFase) =>
        designacion is null ? null
        : porFase > 1 ? $"{porFase} × {designacion} AWG/kcmil por fase"
        : $"{designacion} AWG/kcmil";

    /// <summary>
    /// Una corriente, o <c>null</c> si vale cero. <b>Cero amperes no es un resultado</b>: es un
    /// cálculo que no se ha corrido, y en una memoria que se firma «Protección seleccionada: 0 A»
    /// es peor que un renglón ausente — el renglón que falta se ve, el cero se lee como dato.
    /// </summary>
    private static string? Amperes(decimal valor, string formato = "N2") =>
        valor <= 0m ? null : $"{valor.ToString(formato)} A";

    /// <summary>«THHN · lugar seco»: lo que decide la columna de la Tabla 310-15(b)(16).</summary>
    public static string Aislamiento(DatosDelTablero datos) =>
        $"{datos.TipoAislamiento} · lugar {(datos.LugarSeco ? "seco" : "húmedo o mojado")}" +
        (datos.TerminalesMarcadas75C ? " · terminales marcadas 75 °C" : "");

    /// <summary>
    /// «Contactos · Aparatos pequeños (cocina)»: el tipo, y el uso si no es general. Con cargas de tipos
    /// distintos no hay un tipo: «Cargas combinadas»; la clase va aparte — captura-en-el-desplegable.md.
    /// </summary>
    public static string Etiqueta(CircuitoDelCuadro circuito) =>
        circuito.TieneCargasCombinadas ? "Cargas combinadas"
        : circuito.UsoEfectivo == UsoDeContactos.General
            ? circuito.Categoria.NombreCompleto()
            : $"{circuito.Categoria.NombreCompleto()} · {circuito.UsoEfectivo.Nombre()}";

    public static string Etiqueta(TipoCarga tipo) => tipo switch
    {
        TipoCarga.Alumbrado => "Alumbrado",
        TipoCarga.Contactos => "Contactos",
        TipoCarga.Fuerza => "Fuerza",
        _ => "Equipo",
    };

    public static string Etiqueta(MaterialConductor material) =>
        material == MaterialConductor.Cobre ? "Cobre" : "Aluminio";

    public static string Etiqueta(MaterialCanalizacion canalizacion) => canalizacion switch
    {
        MaterialCanalizacion.Pvc => "PVC",
        MaterialCanalizacion.Aluminio => "Aluminio",
        _ => "Acero",
    };
}
