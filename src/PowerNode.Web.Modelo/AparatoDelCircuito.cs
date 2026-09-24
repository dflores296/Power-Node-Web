using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Un aparato dentro de un circuito</b> — I-35. Un espacio del tablero es un circuito, no un
/// aparato: un circuito puede alimentar uno solo o varios. Con el desglose, la carga del circuito es
/// la suma de sus aparatos, y la memoria dice qué alimenta.
///
/// <para>
/// Es opcional: un circuito sin aparatos se captura como siempre, con su total.
/// </para>
/// </summary>
public sealed class AparatoDelCircuito
{
    /// <summary>La carga de un contacto, sencillo o múltiple en un mismo yugo — 220-14(i).</summary>
    public const decimal VAPorContacto = 180m;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Cuántos aparatos iguales. Mínimo 1.</summary>
    public int Cantidad { get; set; } = 1;

    public UnidadConsumo Unidad { get; set; } = UnidadConsumo.VoltAmperes;

    /// <summary>La carga de <b>uno</b>, como la dice su placa, en <see cref="Unidad"/>.</summary>
    public decimal CargaUnitaria { get; set; }

    /// <summary>Opera 3 h o más: entra al 125 % — 210-19(a)(1).</summary>
    public bool Continua { get; set; }

    public decimal FactorPotencia { get; set; } = CircuitoDelCuadro.FactorPotenciaSupuesto;

    /// <summary>Los VA de todos (cantidad × carga), convertidos con la tensión y los polos del circuito.</summary>
    public decimal TotalVA { get; internal set; }

    /// <summary>«Contacto», «contactos dobles»…: se llena con 180 VA si no trae carga — 220-14(i).</summary>
    internal bool EsContactoSinCarga =>
        CargaUnitaria == 0m && Descripcion.Trim().StartsWith("contacto", StringComparison.OrdinalIgnoreCase);
}
