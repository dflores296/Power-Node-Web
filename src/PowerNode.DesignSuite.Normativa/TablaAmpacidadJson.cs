using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 310-15(b)(16) (canalización/cable/enterrado) y Tabla 310-15(b)(17) (al aire libre) --
/// mismo layout de columnas en las dos: 0=mm2, 1=designación, 2..4=Cobre 60/75/90°C, 5..7=Aluminio
/// 60/75/90°C. Ambas basadas en 30°C (verificado contra el pie de página de cada tabla), así que
/// comparten la misma Tabla 310-15(b)(2)(a) de corrección -- ver <see cref="MetodoInstalacion"/>.
/// </summary>
public class TablaAmpacidadJson(IFuenteTablas fuente, ICatalogoCalibres catalogo) : ITablaAmpacidad
{
    private const string TablaIdCanalizacion = "310-15(b)(16)";
    private const string TablaIdAlAireLibre = "310-15(b)(17)";

    private readonly Dictionary<MetodoInstalacion, List<(Calibre Calibre, decimal?[] Ampacidades)>> _cache = [];

    private static int Columna(MaterialConductor material, TemperaturaAislamiento temperatura)
    {
        var offset = temperatura switch { TemperaturaAislamiento.T60 => 0, TemperaturaAislamiento.T75 => 1, TemperaturaAislamiento.T90 => 2, _ => throw new ArgumentOutOfRangeException(nameof(temperatura)) };
        var baseCol = material == MaterialConductor.Cobre ? 2 : 5;
        return baseCol + offset;
    }

    private List<(Calibre Calibre, decimal?[] Ampacidades)> Filas(MetodoInstalacion metodo)
    {
        if (_cache.TryGetValue(metodo, out var cacheado)) return cacheado;

        var tablaId = metodo == MetodoInstalacion.AlAireLibre ? TablaIdAlAireLibre : TablaIdCanalizacion;
        var resultado = new List<(Calibre, decimal?[])>();
        foreach (var f in fuente.FilasDatos(tablaId))
        {
            var designacion = f.Texto(1);
            if (string.IsNullOrWhiteSpace(designacion)) continue;

            var calibre = catalogo.BuscarPorDesignacion(designacion);
            if (calibre is null) continue;

            var ampacidades = new decimal?[6];
            for (var i = 0; i < 6; i++)
                ampacidades[i] = NormaParsing.Decimal(f.Texto(2 + i));

            resultado.Add((calibre, ampacidades));
        }

        _cache[metodo] = resultado;
        return resultado;
    }

    public decimal? Ampacidad(Calibre calibre, MaterialConductor material, TemperaturaAislamiento temperatura, MetodoInstalacion metodo = MetodoInstalacion.CanalizacionOCable)
    {
        var col = Columna(material, temperatura) - 2; // índice dentro del arreglo Ampacidades (que arranca en columna 2)
        var fila = Filas(metodo).FirstOrDefault(f => f.Calibre.Designacion == calibre.Designacion);
        return fila.Ampacidades?[col];
    }

    public Calibre CalibrePorAmpacidad(decimal corrienteA, MaterialConductor material, TemperaturaAislamiento temperatura, MetodoInstalacion metodo = MetodoInstalacion.CanalizacionOCable)
    {
        var col = Columna(material, temperatura) - 2;
        var fila = Filas(metodo)
            .OrderBy(f => f.Calibre.AreaMm2)
            .FirstOrDefault(f => f.Ampacidades[col] is decimal a && a >= corrienteA);

        if (fila.Calibre is null)
            throw new InvalidOperationException(
                $"No hay calibre en la Tabla {(metodo == MetodoInstalacion.AlAireLibre ? TablaIdAlAireLibre : TablaIdCanalizacion)} con ampacidad >= {corrienteA} A para {material}/{(int)temperatura}°C.");

        return fila.Calibre;
    }
}
