using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.Web.Modelo.Memoria;

/// <summary>
/// Lo que la memoria dice de <b>un</b> tramo, sea un circuito derivado o el alimentador del
/// tablero. Es la hoja que se repite.
/// </summary>
/// <param name="Sujeto">Cómo se nombra en el encabezado: «Circuito 3 — Contactos».</param>
/// <param name="Articulo">«210» en un derivado, «215» en un alimentador — decide qué se cita.</param>
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
    IReadOnlyList<RenglonMemoria>? Canalizacion = null);

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
