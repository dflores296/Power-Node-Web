using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>Tabla 310-15(b)(2)(a). Columnas: 0=rango de temp. ambiente, 1..3=factor a 60/75/90°C.</summary>
public class TablaCorreccionTemperaturaJson(IFuenteTablas fuente) : ITablaCorreccionTemperatura
{
    private const string TablaId = "310-15(b)(2)(a)";
    private List<(RangoNorma Rango, decimal?[] Factores)>? _cache;

    private List<(RangoNorma Rango, decimal?[] Factores)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new List<(RangoNorma, decimal?[])>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var rango = RangoNorma.Parsear(f.Texto(0) ?? "", TablaId);
            if (rango is null) continue;

            var factores = new decimal?[3];
            for (var i = 0; i < 3; i++)
                factores[i] = NormaParsing.Decimal(f.Texto(1 + i));

            resultado.Add((rango, factores));
        }

        _cache = resultado;
        return _cache;
    }

    public decimal? Factor(decimal temperaturaAmbienteC, TemperaturaAislamiento temperatura)
    {
        var col = temperatura switch { TemperaturaAislamiento.T60 => 0, TemperaturaAislamiento.T75 => 1, TemperaturaAislamiento.T90 => 2, _ => throw new ArgumentOutOfRangeException(nameof(temperatura)) };
        var fila = Filas().FirstOrDefault(f => f.Rango.Contiene(temperaturaAmbienteC));
        return fila.Factores?[col];
    }

    /// <summary>
    /// La errata que se aplicó para resolver esa consulta, o null — que es el caso de todas las
    /// temperaturas menos las de 71 a 75 °C. <b>Es la contrapartida de corregir</b>: quien calcula
    /// con un valor que no es el publicado tiene que decirlo en la memoria.
    /// </summary>
    public ErrataDeRotulo? ErrataAplicada(decimal temperaturaAmbienteC) =>
        Filas().FirstOrDefault(f => f.Rango.Contiene(temperaturaAmbienteC)).Rango?.Errata;
}
