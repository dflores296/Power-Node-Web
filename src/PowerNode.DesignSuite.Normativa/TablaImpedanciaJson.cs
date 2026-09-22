using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 9. Columnas: 0=área mm2, 1=designación, 2=XL (PVC o Aluminio), 3=XL (Acero),
/// 4..6=R CA cobre (PVC/Aluminio/Acero), 7..9=R CA aluminio (PVC/Aluminio/Acero).
/// </summary>
public class TablaImpedanciaJson(IFuenteTablas fuente, ICatalogoCalibres catalogo) : ITablaImpedancia
{
    private const string TablaId = "9";
    private List<(Calibre Calibre, decimal?[] Valores)>? _cache;

    private List<(Calibre Calibre, decimal?[] Valores)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new List<(Calibre, decimal?[])>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var designacion = f.Texto(1);
            if (string.IsNullOrWhiteSpace(designacion)) continue;

            var calibre = catalogo.BuscarPorDesignacion(designacion);
            if (calibre is null) continue;

            var valores = new decimal?[8]; // 0=XL-PvcAl, 1=XL-Acero, 2..4=R-Cu, 5..7=R-Al
            for (var i = 0; i < 8; i++)
                valores[i] = NormaParsing.Decimal(f.Texto(2 + i));

            resultado.Add((calibre, valores));
        }

        _cache = resultado;
        return _cache;
    }

    public ImpedanciaConductor? Impedancia(Calibre calibre, MaterialConductor material, MaterialCanalizacion canalizacion)
    {
        var fila = Filas().FirstOrDefault(f => f.Calibre.Designacion == calibre.Designacion);
        if (fila.Valores is null) return null;

        var x = canalizacion == MaterialCanalizacion.Acero ? fila.Valores[1] : fila.Valores[0];

        var rIndiceBase = material == MaterialConductor.Cobre ? 2 : 5;
        var rOffset = canalizacion switch
        {
            MaterialCanalizacion.Pvc => 0,
            MaterialCanalizacion.Aluminio => 1,
            MaterialCanalizacion.Acero => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(canalizacion)),
        };
        var r = fila.Valores[rIndiceBase + rOffset];

        if (r is null || x is null) return null;
        return new ImpedanciaConductor(r.Value, x.Value);
    }
}
