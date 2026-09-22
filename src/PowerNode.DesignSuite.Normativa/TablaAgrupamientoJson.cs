using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>Tabla 310-15(b)(3)(a). Columnas: 0=rango de número de conductores, 1=porcentaje.</summary>
public class TablaAgrupamientoJson(IFuenteTablas fuente) : ITablaAgrupamiento
{
    private const string TablaId = "310-15(b)(3)(a)";
    private List<(RangoNorma Rango, decimal Porcentaje)>? _cache;

    private List<(RangoNorma Rango, decimal Porcentaje)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new List<(RangoNorma, decimal)>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var rango = RangoNorma.Parsear(f.Texto(0) ?? "", TablaId);
            var porcentaje = NormaParsing.Decimal(f.Texto(1));
            if (rango is null || porcentaje is null) continue;
            resultado.Add((rango, porcentaje.Value));
        }

        _cache = resultado;
        return _cache;
    }

    public decimal Factor(int numeroConductores)
    {
        if (numeroConductores <= 3) return 1m;

        var fila = Filas().FirstOrDefault(f => f.Rango.Contiene(numeroConductores));
        if (fila.Rango is null)
            throw new InvalidOperationException($"La Tabla 310-15(b)(3)(a) no tiene un rango para {numeroConductores} conductores.");

        return fila.Porcentaje / 100m;
    }
}
