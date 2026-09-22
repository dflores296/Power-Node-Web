using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Corriente máxima a rotor bloqueado (arranque) — 430-251(a) monofásico, 430-251(b) polifásico
/// diseños B/C/D/E. La usa la Excepción 2 de 430-52(c)(1) cuando el techo normal de la Tabla 430-52
/// no alcanza para el arranque del motor.
/// </summary>
public interface ITablaRotorBloqueado
{
    /// <summary>Null si no hay fila exacta de Hp/tensión, o si el tipo de alimentación no aplica (CD no tiene rotor bloqueado).</summary>
    decimal? CorrienteRotorBloqueadoA(decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionV);
}

/// <summary>Tabla 430-7(b) — letra de código, kVA/hp a rotor bloqueado. Ruta alterna para calcular la corriente de arranque cuando el motor no está en 430-251.</summary>
public interface ITablaLetraCodigoMotor
{
    /// <summary>Rango [mínimo, máximo] de kVA por hp para la letra dada. Null si la letra no existe en la tabla.</summary>
    (decimal Minimo, decimal Maximo)? RangoKvaPorHp(string letraCodigo);
}
