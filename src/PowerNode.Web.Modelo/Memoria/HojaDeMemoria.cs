using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.Web.Modelo.Memoria;

/// <summary>
/// Lo que la memoria dice de <b>un</b> tramo, sea un circuito derivado o el alimentador del
/// tablero. Es la hoja que se repite.
/// </summary>
/// <param name="Sujeto">Cómo se nombra en el encabezado: «Circuito 3 — Contactos».</param>
/// <param name="Articulo">«210» en un derivado, «215» en un alimentador — decide qué se cita.</param>
/// <param name="ConductorNeutro"><c>null</c> si el tramo no lleva neutro: 2 o 3 polos sin «+N», o
/// 3F-3H — I-73.</param>
/// <param name="FaseQueGobierna">Solo en el alimentador: cuál barra es la más cargada y con qué
/// corriente se dimensiona, ya redactado. Sin él, la corriente de diseño no se deduce de la carga
/// total de la sección 1.</param>
/// <param name="Desglose">Solo en un circuito desglosado: un renglón por aparato — I-35.</param>
/// <param name="FactoresDeDemanda">Solo en el alimentador: un renglón por tipo con factor menor que 1,
/// con su justificación — R-12, R-17.</param>
/// <param name="CaidaPorFase">Solo en un alimentador con neutro: la caída de cada fase con el
/// neutro — R-02. Cambia la sección 6 a la fórmula fasorial.</param>
/// <param name="NeutroPortador">Solo en un tramo de 2 fases + neutro de estrella: por qué el neutro
/// cuenta en el agrupamiento — 310-15(b)(5)(2).</param>
/// <param name="Minimo220_52VA">Solo en el alimentador: lo que agrega 220-52 a la carga instalada.</param>
/// <param name="CaidaCombinada">Solo en un derivado: alimentador + circuito, ya redactado — R-01.</param>
/// <param name="Canalizacion">La canalización del tramo: portadores, factor de agrupamiento y azotea —
/// I-39. Va en la sección 4.</param>
/// <param name="Equipo">Solo en el derivado de un motor o de un equipo de A/C: lo que cambia las
/// secciones 1 y 3 al Art. 430 o al 440 — I-15, I-74.</param>
/// <param name="CargaMotoresVa">La carga de los motores y los equipos de A/C, que no es continua ni no
/// continua.</param>
/// <param name="MotoresQueGobiernan">Solo en el alimentador: los motores y equipos de A/C de la fase que
/// gobierna, para 430-24 / 440-33.</param>
/// <param name="NotaDelUso">Solo en un circuito de contactos con un uso que la norma trata aparte sin
/// mínimo: el individual del refrigerador — I-76.</param>
/// <param name="EtiquetaDeMotores">«Motores», o «Motores y A/C»; y su referencia, «430-24» o «430-24,
/// 440-33».</param>
/// <param name="FrecuenciaHz">La del tablero, la misma que dice el cuadro de carga — I-96.</param>
/// <param name="Techo430_62A">Solo en el alimentador con motores: el máximo de 430-62(a) más la otra
/// carga (430-63).</param>
public sealed record HojaDeMemoria(
    string Sujeto,
    string Articulo,
    decimal CargaContinuaVa,
    decimal CargaNoContinuaVa,
    decimal TensionV,
    int NumeroFases,
    int NumeroHilos,
    decimal FactorPotencia,
    decimal LongitudM,
    string MaterialConductor,
    decimal CorrienteDisenoA,
    decimal ProteccionA,
    string? ConductorFase,
    string? ConductorNeutro,
    string? ConductorTierra,
    decimal CaidaTensionPct,
    string? TablaAmpacidadId,
    int ConductoresPorFase,
    DetalleDelCalculo? Detalle,
    IReadOnlyList<Cita> Citas,
    string? FaseQueGobierna = null,
    string? SerieDeInterruptores = null,
    string? Aislamiento = null,
    IReadOnlyList<string>? DesgloseConductor = null,
    string? CaidaCombinada = null,
    decimal Minimo220_52VA = 0m,
    string? NeutroPortador = null,
    IReadOnlyList<CaidaDeFase>? CaidaPorFase = null,
    PowerNode.DesignSuite.Calculo.Magnitudes.Fasor? CorrienteNeutro = null,
    decimal TensionFaseNeutroV = 0m,
    IReadOnlyList<RenglonMemoria>? FactoresDeDemanda = null,
    IReadOnlyList<RenglonMemoria>? Desglose = null,
    IReadOnlyList<RenglonMemoria>? Canalizacion = null,
    EquipoDeLaHoja? Equipo = null,
    decimal CargaMotoresVa = 0m,
    AgregadoMotores MotoresQueGobiernan = default,
    decimal? Techo430_62A = null,
    string EtiquetaDeMotores = "Motores",
    string ReferenciaDeMotores = "430-24",
    string? NotaDelUso = null,
    int FrecuenciaHz = 60);

/// <summary>
/// <b>Un motor o un equipo de A/C, como lo pone la memoria</b> — I-15, I-74. Ya redactado, porque las
/// dos formas de cada uno (HP o amperes; placa con MCA o con corriente nominal) no se escriben igual.
/// </summary>
/// <param name="Rotulo">«Motor» o «Equipo de A/C».</param>
/// <param name="Descripcion">El renglón de la sección 1: «5 HP · trifásico · FLC 15.20 A — Tabla
/// 430-250, columna de 230 V (…), 430-6(a)».</param>
/// <param name="Proteccion">La sección 3, renglón por renglón.</param>
/// <param name="Notas">Las notas de la sección 3.</param>
/// <param name="Corriente">«FLC» o «corriente»: cómo se llama lo que da los VA en la sección 1.</param>
public sealed record EquipoDeLaHoja(
    string Rotulo,
    string Descripcion,
    IReadOnlyList<RenglonMemoria> Proteccion,
    IReadOnlyList<string> Notas,
    string Corriente);

/// <summary>Un renglón «rótulo: valor» de una sección de la memoria.</summary>
public sealed record RenglonMemoria(string Rotulo, string Valor);

/// <summary>
/// Una de las nueve secciones. <paramref name="Formulas"/> van centradas y con sus números ya
/// sustituidos: es lo que permite que quien revisa la memoria la recalcule y llegue al mismo
/// resultado. <paramref name="Notas"/> son las aclaraciones al pie de la sección.
/// </summary>
public sealed record BloqueMemoria(
    string Titulo,
    IReadOnlyList<RenglonMemoria> Renglones,
    IReadOnlyList<string> Formulas,
    IReadOnlyList<string> Notas,
    string? Introduccion = null);
