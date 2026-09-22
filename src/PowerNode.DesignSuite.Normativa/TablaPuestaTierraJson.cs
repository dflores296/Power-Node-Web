using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>Tabla 250-122. Columnas: 0=capacidad de protección (A), 1..2=Cu (mm2, designación), 3..4=Al (mm2, designación).</summary>
public class TablaPuestaTierraJson(IFuenteTablas fuente, ICatalogoCalibres catalogo) : ITablaPuestaTierra
{
    private const string TablaId = "250-122";
    private List<(decimal CapacidadA, string? DesignacionCu, string? DesignacionAl)>? _cache;

    private List<(decimal, string?, string?)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new List<(decimal, string?, string?)>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var capacidad = NormaParsing.Decimal(f.Texto(0));
            if (capacidad is null) continue;

            var designacionCu = f.Texto(2);
            var designacionAl = f.Texto(4);
            resultado.Add((capacidad.Value, EsValida(designacionCu) ? designacionCu : null, EsValida(designacionAl) ? designacionAl : null));
        }

        _cache = resultado.OrderBy(f => f.Item1).ToList();
        return _cache;

        static bool EsValida(string? d) => !string.IsNullOrWhiteSpace(d) && d.Trim() != "-";
    }

    public Calibre CalibreMinimo(decimal amperesProteccion, MaterialConductor material)
    {
        foreach (var (capacidadA, designacionCu, designacionAl) in Filas())
        {
            if (capacidadA < amperesProteccion) continue;

            var designacion = material == MaterialConductor.Cobre ? designacionCu : designacionAl;
            if (designacion is null) continue; // ese material no tiene valor a esta capacidad; se busca la siguiente fila

            return catalogo.BuscarPorDesignacion(designacion)
                ?? throw new InvalidOperationException($"La Tabla 250-122 referencia el calibre '{designacion}' que no está en el catálogo de la Tabla 8.");
        }

        throw new InvalidOperationException($"La Tabla 250-122 no cubre una protección de {amperesProteccion} A para {material}.");
    }
}
