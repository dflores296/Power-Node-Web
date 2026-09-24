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
    DetalleDelCalculo? Detalle = null,
    /// <summary>La fase con la que se dimensionó, si se capturaron corrientes por fase — R-04.</summary>
    char? FaseQueGobierna = null,
    /// <summary>La caída de cada fase con el neutro — R-02. <see cref="CaidaTensionPct"/> es la mayor.</summary>
    IReadOnlyList<CaidaDeFase>? CaidaPorFase = null,
    /// <summary>Corriente del neutro, suma fasorial de las de fase, con demanda.</summary>
    Magnitudes.Fasor? CorrienteNeutro = null);
