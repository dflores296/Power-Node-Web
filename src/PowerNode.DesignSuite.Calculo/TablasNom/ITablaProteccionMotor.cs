using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Tabla 430-52 — techo de protección como porcentaje del FLC. El breaker/fusible real se
/// redondea al tamaño estándar de 240-6(a) inmediato superior a ese techo (430-52(c)(1),
/// Excepción 1) — el redondeo es hacia arriba, igual que un circuito normal.
/// </summary>
public interface ITablaProteccionMotor
{
    decimal PorcentajeMaximo(TipoMotor tipoMotor, TipoDispositivoProteccionMotor tipoDispositivo);
}
