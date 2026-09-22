namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>Tabla 310-15(b)(3)(a) — factor de ajuste por más de tres conductores portadores de corriente agrupados.</summary>
public interface ITablaAgrupamiento
{
    /// <summary>1.0 si numeroConductores &lt;= 3 (la tabla no aplica correción, es la base de la Tabla 310-15(b)(16)).</summary>
    decimal Factor(int numeroConductores);
}
