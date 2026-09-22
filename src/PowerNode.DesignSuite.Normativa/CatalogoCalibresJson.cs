using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 8 (Propiedades de los conductores). Trae dos filas por designación en calibres chicos
/// (sólido de 1 hilo y trenzado de 7+ hilos); se usa la trenzada como canónica — es la que se
/// instala en la práctica salvo alambrado de control. El área nominal (columna "Area" en mm2)
/// es la misma para ambas variantes de una misma designación; el área "Total" de la tabla varía
/// por construcción y no se usa aquí.
/// </summary>
public class CatalogoCalibresJson(IFuenteTablas fuente) : ICatalogoCalibres
{
    private const string TablaId = "8";
    private List<Calibre>? _cache;

    private List<Calibre> Calibres()
    {
        if (_cache is not null) return _cache;

        var filas = fuente.FilasDatos(TablaId);
        var porDesignacion = new Dictionary<string, (Calibre Calibre, decimal Hilos)>();

        foreach (var f in filas)
        {
            var designacion = NormaParsing.DesignacionLimpia(f.Texto(0) ?? "");
            var areaMm2 = NormaParsing.Decimal(f.Texto(1));
            var hilos = NormaParsing.Decimal(f.Texto(3)) ?? 1;
            var diametroMm = NormaParsing.Decimal(f.Texto(5));
            var rCuNoCubierto = NormaParsing.Decimal(f.Texto(7));
            var rCuRecubierto = NormaParsing.Decimal(f.Texto(8));
            var rAl = NormaParsing.Decimal(f.Texto(9));

            if (designacion.Length == 0 || areaMm2 is null || diametroMm is null || rCuNoCubierto is null || rCuRecubierto is null)
                continue;

            var calibre = new Calibre(designacion, areaMm2.Value, diametroMm.Value, rCuNoCubierto.Value, rCuRecubierto.Value, rAl);

            if (!porDesignacion.TryGetValue(designacion, out var existente) || hilos > existente.Hilos)
                porDesignacion[designacion] = (calibre, hilos);
        }

        _cache = porDesignacion.Values.Select(v => v.Calibre).OrderBy(c => c.AreaMm2).ToList();
        return _cache;
    }

    public IReadOnlyList<Calibre> Listar() => Calibres();

    public Calibre? BuscarPorDesignacion(string designacion)
    {
        var buscado = NormaParsing.DesignacionLimpia(designacion);
        return Calibres().FirstOrDefault(c => string.Equals(c.Designacion, buscado, StringComparison.OrdinalIgnoreCase));
    }

    public Calibre BuscarPorAreaMinima(decimal areaMm2)
    {
        var calibres = Calibres();
        var encontrado = calibres.FirstOrDefault(c => c.AreaMm2 >= areaMm2);
        return encontrado ?? throw new InvalidOperationException(
            $"No hay calibre estándar con área >= {areaMm2} mm² en la Tabla 8.");
    }

    public Calibre? Siguiente(Calibre calibre)
    {
        var calibres = Calibres();
        var idx = calibres.FindIndex(c => c.Designacion == calibre.Designacion);
        if (idx < 0 || idx == calibres.Count - 1) return null;
        return calibres[idx + 1];
    }
}
