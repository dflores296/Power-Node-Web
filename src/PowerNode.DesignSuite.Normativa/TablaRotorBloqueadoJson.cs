using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// 430-251(a) monofásico, 430-251(b) polifásico (diseños B/C/D/E — la inmensa mayoría de motores
/// reales). Para Excepción 2 de 430-52(c)(1), cuando el techo normal no alcanza para el arranque.
/// </summary>
public class TablaRotorBloqueadoJson(IFuenteTablas fuente) : ITablaRotorBloqueado
{
    private static readonly Dictionary<int, int> ColumnasMonofasico = new() { [115] = 2, [208] = 3, [230] = 4 };
    private static readonly Dictionary<int, int> ColumnasPolifasico = new() { [115] = 2, [200] = 3, [208] = 4, [230] = 5, [460] = 6, [575] = 7 };

    private readonly Dictionary<string, List<(decimal Hp, decimal?[] PorColumna)>> _cache = [];

    private List<(decimal, decimal?[])> Filas(string tablaId, Dictionary<int, int> columnas)
    {
        if (_cache.TryGetValue(tablaId, out var cacheado)) return cacheado;

        var maxCol = columnas.Values.Max();
        var resultado = new List<(decimal, decimal?[])>();
        foreach (var f in fuente.FilasDatos(tablaId))
        {
            var hpTexto = f.Texto(1);
            if (string.IsNullOrWhiteSpace(hpTexto)) continue;

            var hp = NormaParsing.Hp(hpTexto);
            var porColumna = new decimal?[maxCol + 1];
            foreach (var col in columnas.Values)
                porColumna[col] = NormaParsing.Decimal(f.Texto(col));

            resultado.Add((hp, porColumna));
        }

        _cache[tablaId] = resultado;
        return resultado;
    }

    public decimal? CorrienteRotorBloqueadoA(decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionV)
    {
        var (tablaId, columnas) = tipoAlimentacion switch
        {
            TipoAlimentacionMotor.Monofasico => ("430-251(a)", ColumnasMonofasico),
            TipoAlimentacionMotor.DosFases or TipoAlimentacionMotor.Trifasico => ("430-251(b)", ColumnasPolifasico),
            _ => (null, null)!,
        };
        if (tablaId is null || columnas is null || !columnas.TryGetValue((int)tensionV, out var col)) return null;

        var fila = Filas(tablaId, columnas).FirstOrDefault(f => Math.Abs(f.Item1 - hp) < 0.01m);
        return fila.Item2?[col];
    }
}

/// <summary>430-7(b). Columnas: 0=letra de código, 1=rango "min – max" de kVA/hp.</summary>
public class TablaLetraCodigoMotorEf(IFuenteTablas fuente) : ITablaLetraCodigoMotor
{
    private const string TablaId = "430-7(b)";
    private Dictionary<string, (decimal, decimal)>? _cache;

    private Dictionary<string, (decimal, decimal)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new Dictionary<string, (decimal, decimal)>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var letra = f.Texto(0)?.Trim();
            var rango = f.Texto(1);
            if (string.IsNullOrWhiteSpace(letra) || string.IsNullOrWhiteSpace(rango)) continue;

            var partes = rango.Split('–', StringSplitOptions.TrimEntries);
            if (partes.Length != 2) continue;
            if (!decimal.TryParse(partes[0], System.Globalization.CultureInfo.InvariantCulture, out var min)) continue;

            var maxTexto = partes[1].Replace("en adelante", "").Trim();
            var max = maxTexto.Length == 0 ? decimal.MaxValue
                : decimal.TryParse(maxTexto, System.Globalization.CultureInfo.InvariantCulture, out var m) ? m : decimal.MaxValue;

            resultado[letra] = (min, max);
        }

        _cache = resultado;
        return resultado;
    }

    public (decimal Minimo, decimal Maximo)? RangoKvaPorHp(string letraCodigo)
        => Filas().TryGetValue(letraCodigo.Trim(), out var rango) ? rango : null;
}
