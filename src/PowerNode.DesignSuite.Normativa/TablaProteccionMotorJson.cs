using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>Tabla 430-52. Columnas: 0=tipo de motor, 1..4=% por tipo de dispositivo.</summary>
public class TablaProteccionMotorJson(IFuenteTablas fuente) : ITablaProteccionMotor
{
    private const string TablaId = "430-52";

    // La fila de texto de la tabla trae notas al pie pegadas ("Sincrónicos3"); se ubica por substring.
    private static readonly Dictionary<TipoMotor, string> PalabraClavePorFila = new()
    {
        [TipoMotor.Monofasico] = "Motores monofásicos",
        [TipoMotor.PolifasicoJaulaArdilla] = "De jaula de ardilla",
        [TipoMotor.DisenoBAltaEficiencia] = "De diseño B",
        [TipoMotor.Sincrono] = "Sincrónicos",
        [TipoMotor.RotorDevanado] = "Con rotor devanado",
        [TipoMotor.CorrienteContinua] = "De corriente continua",
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

    public decimal PorcentajeMaximo(TipoMotor tipoMotor, TipoDispositivoProteccionMotor tipoDispositivo)
    {
        // StartsWith, no Contains: la fila "De jaula de ardilla: diferentes de los de diseño B..."
        // contiene la frase "de diseño B" como subcadena y produciría un falso match con Contains.
        var palabraClave = PalabraClavePorFila[tipoMotor];
        var fila = Filas().FirstOrDefault(f => f.TextoFila.StartsWith(palabraClave, StringComparison.OrdinalIgnoreCase));
        if (fila.TextoFila is null)
            throw new InvalidOperationException($"No se encontró la fila de '{tipoMotor}' en la Tabla 430-52.");

        var col = (int)tipoDispositivo;
        return fila.Porcentajes[col]
            ?? throw new InvalidOperationException($"La Tabla 430-52 no trae valor para {tipoMotor}/{tipoDispositivo}.");
    }
}
