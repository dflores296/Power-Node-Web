using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 430-22(e) — I-120. Columnas: 0=clasificación del servicio, 1..4=% de la corriente de placa para
/// un motor especificado para 5 min, 15 min, 30 y 60 min y funcionamiento continuo. Nace en Power Node Web.
/// </summary>
public class TablaServicioMotorJson(IFuenteTablas fuente) : ITablaServicioMotor
{
    private const string TablaId = "430-22(e)";

    // La fila se ubica por el principio de su texto: «Servicio de corto tiempo: Accionamiento de…».
    private static readonly Dictionary<ServicioDeMotor, string> PalabraClavePorFila = new()
    {
        [ServicioDeMotor.CortaDuracion] = "Servicio de corto tiempo",
        [ServicioDeMotor.Intermitente] = "Servicio intermitente",
        [ServicioDeMotor.Periodico] = "Servicio periódico",
        [ServicioDeMotor.Variable] = "Servicio variable",
    };

    private List<(string TextoFila, decimal?[] Porcentajes)>? _cache;

    private List<(string TextoFila, decimal?[] Porcentajes)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new List<(string, decimal?[])>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var textoFila = f.Texto(0);
            if (string.IsNullOrWhiteSpace(textoFila)) continue;

            var porcentajes = new decimal?[4];
            for (var i = 0; i < 4; i++)
                porcentajes[i] = NormaParsing.Decimal(f.Texto(1 + i));

            resultado.Add((textoFila, porcentajes));
        }

        _cache = resultado;
        return _cache;
    }

    public decimal? Porcentaje(ServicioDeMotor servicio, EspecificacionDeTiempo especificacion)
    {
        var palabraClave = PalabraClavePorFila[servicio];
        var fila = Filas().FirstOrDefault(f => f.TextoFila.StartsWith(palabraClave, StringComparison.OrdinalIgnoreCase));
        if (fila.TextoFila is null)
            throw new InvalidOperationException($"No se encontró la fila de '{palabraClave}' en la Tabla 430-22(e).");
        return fila.Porcentajes[(int)especificacion];
    }
}
