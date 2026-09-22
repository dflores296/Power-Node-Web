using PowerNode.DesignSuite.Calculo.Magnitudes;

namespace PowerNode.DesignSuite.Calculo.Casos;

public sealed record ResultadoCorrienteTransformador(decimal CorrientePrimariaA, decimal CorrienteSecundariaA);

/// <summary>
/// Corriente nominal (de placa) del primario y del secundario de un transformador:
/// In = kVA·1000 / (√3·V_línea) en trifásico, o In = kVA·1000 / V en monofásico --
/// Ley de Ohm / definición de potencia aparente aplicada a cada devanado por
/// separado, nada específico de la norma. <b>No</b> es lo mismo que la protección
/// contra sobrecorriente del transformador (Art. 450-3, Tabla 450-3(a)/(b)): esa
/// tabla define QUÉ PORCENTAJE de esta corriente nominal debe usarse para elegir el
/// interruptor/fusible (125%, 167%, etc., según el caso, y si hay protección en el
/// secundario o no) -- esa selección de protección real sigue <b>fuera de v1</b> (ver
/// PLAN-V1.md), porque depende del esquema de protección elegido (solo primario vs.
/// primario+secundario), una decisión de diseño que este motor todavía no modela.
/// Lo que sí se calcula aquí -- la corriente nominal -- es exacto y no tiene ese tipo
/// de ambigüedad.
/// </summary>
public static class CalculadoraTransformador
{
    public static ResultadoCorrienteTransformador Calcular(
        decimal capacidadKva, decimal tensionPrimariaV, decimal tensionSecundariaV, int numeroFases)
    {
        if (capacidadKva <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacidadKva), "La capacidad de un transformador debe ser mayor a cero.");
        if (numeroFases is not (1 or 3))
            throw new ArgumentOutOfRangeException(nameof(numeroFases), "Un transformador es monofásico (1) o trifásico (3).");

        var potenciaVA = capacidadKva * 1000m;

        var ip = numeroFases == 3
            ? SistemaTrifasico.CorrienteLineaDesdePotencia(potenciaVA, tensionPrimariaV)
            : SistemaTrifasico.CorrienteMonofasicaDesdePotencia(potenciaVA, tensionPrimariaV);

        var is_ = numeroFases == 3
            ? SistemaTrifasico.CorrienteLineaDesdePotencia(potenciaVA, tensionSecundariaV)
            : SistemaTrifasico.CorrienteMonofasicaDesdePotencia(potenciaVA, tensionSecundariaV);

        return new ResultadoCorrienteTransformador(ip, is_);
    }
}
