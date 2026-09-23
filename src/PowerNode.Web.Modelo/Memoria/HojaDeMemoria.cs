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
    string? Aislamiento = null);

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
