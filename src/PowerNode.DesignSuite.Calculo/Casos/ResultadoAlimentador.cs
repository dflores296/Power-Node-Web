using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// ProteccionA es informativa (aparece en la memoria y decide la columna de
/// temperatura de terminales, 110-14(c)(1)) -- no se persiste un campo de protección
/// propio en CalculoAlimentador porque, cuando el destino es un Tablero, esa
/// protección YA es CalculoTablero.BreakerPrincipalA del tablero destino (el mismo
/// número, no hace falta guardarlo dos veces). Cuando el destino es un Transformador,
/// la protección real de placa (Tabla 450-3) sigue fuera de v1 -- ver PLAN-V1.md.
/// </summary>
public sealed record ResultadoAlimentador(
    decimal CorrienteDisenoA,
    decimal ProteccionA,
    Calibre CalibreFase,
    Calibre CalibreNeutro,
    Calibre CalibreTierra,
    decimal CaidaTensionPct,
    string TablaAmpacidadId,
    IReadOnlyList<Cita> Citas,
    int NumeroConductoresParalelo = 1,
    decimal? TechoProteccion430_62A = null,
    bool ProteccionExcedeTecho430_62 = false,
    IReadOnlyList<string>? AvisosCargaContinua = null,
    DetalleDelCalculo? Detalle = null);
