using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// FLC de motores por tabla: 430-247 (CD), 430-248 (1φ), 430-249 (2φ), 430-250 (3φ — solo las
/// columnas de inducción; jaula de ardilla y rotor devanado, que es la inmensa mayoría de los
/// casos reales. Los motores síncronos de FP unitario de 430-250 quedan pendientes.)
///
/// <para>
/// <b>Las columnas son tensiones NOMINALES DEL MOTOR, no del sistema</b>, y las tres tablas de
/// corriente alterna lo dicen en su propio texto introductorio. El de la 430-250, literal:
/// <i>"Las tensiones enumeradas son las nominales de los motores. Las corrientes enumeradas se
/// permitirán para sistemas con intervalos de tensión de 110 a 120 volts, 220 a 240 volts, 440 a
/// 480 volts y 550 a 600 volts."</i> La 430-249 dice lo mismo y la 430-248 nombra los dos
/// intervalos que le aplican.
/// </para>
///
/// <para>
/// <b>Por eso la búsqueda no puede ser de coincidencia exacta</b>, y ése era un hueco real: un
/// motor en un sistema de <b>220 V</b> —la tensión de utilización trifásica más común en México, y
/// justo la del tablero 220Y/127 que este programa toma como caso base— no tiene columna propia y
/// el motor tronaba con una excepción en vez de resolver por la columna de 230 V que la norma
/// manda usar. La 430-247 (corriente continua) <b>no</b> trae esa cláusula, así que ahí la
/// coincidencia sigue siendo exacta.
/// </para>
///
/// <para><b>La columna explícita gana siempre.</b> 200 V y 208 V son columnas propias y no caen en
/// ningún intervalo; 127 V es columna propia de la 430-248 (añadido mexicano, no está en el NEC).
/// Primero se busca la columna exacta y solo si no existe se aplica el intervalo.</para>
/// </summary>
public class TablaFlcMotorJson(IFuenteTablas fuente) : ITablaFlcMotor
{
    private static readonly Dictionary<TipoAlimentacionMotor, (string TablaId, Dictionary<int, int> ColumnaPorVoltaje)> Config = new()
    {
        [TipoAlimentacionMotor.CorrienteContinua] = ("430-247", new() { [120] = 2, [240] = 3, [500] = 4 }),
        [TipoAlimentacionMotor.Monofasico] = ("430-248", new() { [115] = 2, [127] = 3, [208] = 4, [230] = 5 }),
        [TipoAlimentacionMotor.DosFases] = ("430-249", new() { [115] = 2, [230] = 3, [460] = 4, [575] = 5, [2300] = 6 }),
        [TipoAlimentacionMotor.Trifasico] = ("430-250", new() { [115] = 2, [200] = 3, [208] = 4, [230] = 5, [460] = 6, [575] = 7, [2300] = 8 }),
    };

    /// <summary>
    /// Los intervalos de tensión de sistema que cada tabla declara en su introducción, y a qué
    /// columna nominal mandan. Solo las tres tablas de corriente alterna los traen; la 430-248
    /// nombra únicamente los dos primeros, así que solo ésos se le aplican — no se le extienden los
    /// otros dos "por simetría", que sería inventar texto que la tabla no tiene (de todos modos no
    /// tiene columnas de 460/575, pero la razón para no ponerlos es la primera, no la segunda).
    /// </summary>
    private static readonly Dictionary<TipoAlimentacionMotor, (decimal Min, decimal Max, int Nominal)[]> IntervalosDeSistema = new()
    {
        [TipoAlimentacionMotor.Monofasico] = [(110m, 120m, 115), (220m, 240m, 230)],
        [TipoAlimentacionMotor.DosFases] = [(110m, 120m, 115), (220m, 240m, 230), (440m, 480m, 460), (550m, 600m, 575)],
        [TipoAlimentacionMotor.Trifasico] = [(110m, 120m, 115), (220m, 240m, 230), (440m, 480m, 460), (550m, 600m, 575)],
    };

    private readonly Dictionary<TipoAlimentacionMotor, List<(decimal Hp, decimal?[] PorColumna)>> _cache = [];

    private List<(decimal Hp, decimal?[] PorColumna)> Filas(TipoAlimentacionMotor tipo)
    {
        if (_cache.TryGetValue(tipo, out var cacheado)) return cacheado;

        var (tablaId, columnas) = Config[tipo];
        var maxCol = columnas.Values.Max();
        var resultado = new List<(decimal, decimal?[])>();

        foreach (var f in fuente.FilasDatos(tablaId))
        {
            var hpTexto = f.Texto(1);
            if (string.IsNullOrWhiteSpace(hpTexto)) continue;

            var hp = NormaParsing.Hp(hpTexto);
            var porColumna = new decimal?[maxCol + 1];
            foreach (var col in columnas.Values)
                // LA ÚNICA PUERTA POR LA QUE ESTE PROGRAMA SE APARTA DEL DOF -- ver ErratasDeLaNorma.
                // Se aplica AL LEER y no al sembrar: así la base sigue siendo espejo fiel del corpus
                // público, y la corrección queda a la vista donde se usa.
                porColumna[col] = ErratasDeLaNorma.Aplicar(
                    tablaId, hpTexto.Trim(), col, NormaParsing.Decimal(f.Texto(col)));

            resultado.Add((hp, porColumna));
        }

        _cache[tipo] = resultado;
        return resultado;
    }

    public decimal? CorrientePlenaCargaA(decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionV)
    {
        var (_, columnas) = Config[tipoAlimentacion];
        if (ColumnaPara(tipoAlimentacion, columnas, tensionV) is not { } col) return null;

        var fila = Filas(tipoAlimentacion).FirstOrDefault(f => Math.Abs(f.Hp - hp) < 0.01m);
        return fila.PorColumna?[col];
    }

    /// <summary>
    /// La errata que se aplicó para resolver esa consulta, o null si no hubo ninguna. Es lo que
    /// permite que la memoria <b>declare</b> que un número no es el que publica el DOF: una memoria
    /// que se aparta de la norma sin decirlo no se puede verificar.
    /// </summary>
    public ErrataDeCelda? ErrataAplicada(decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionV)
    {
        var (tablaId, columnas) = Config[tipoAlimentacion];
        if (ColumnaPara(tipoAlimentacion, columnas, tensionV) is not { } col) return null;

        return ErratasDeLaNorma.DeLaCelda(tablaId, ClaveDeFila(hp), col);
    }

    /// <summary>
    /// Cómo se escribe el Hp en la primera columna del corpus. Los enteros van sin decimales ("10",
    /// no "10.0"); las fracciones de la tabla ("½", "7½") no tienen errata hoy y se dejan al
    /// formato invariante, que es lo que devolvería una comparación fallida — y una errata que no
    /// casa no corrige nada, que es el comportamiento correcto.
    /// </summary>
    private static string ClaveDeFila(decimal hp) =>
        hp == decimal.Truncate(hp)
            ? decimal.Truncate(hp).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : hp.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// La columna que le toca a una tensión: primero la exacta (es tensión nominal de motor), y si
    /// no existe, la del intervalo de sistema que la contenga. Null si no cae en ninguno — ahí sí
    /// es una tensión que la tabla no cubre.
    /// </summary>
    private static int? ColumnaPara(TipoAlimentacionMotor tipo, Dictionary<int, int> columnas, decimal tensionV) =>
        TensionDeColumna(tipo, tensionV) is { } nominal ? columnas[nominal] : null;

    /// <summary>
    /// <b>La tensión nominal de la columna con la que se lee <paramref name="tensionV"/></b>: la
    /// misma si la tabla tiene esa columna, o la del intervalo de sistema que la contiene — 220 V se
    /// lee en la de 230 V. Null si la tabla no la cubre. Es lo que la memoria tiene que decir para
    /// que el número se encuentre en la tabla (Power Node Web, I-15).
    /// </summary>
    public static int? TensionDeColumna(TipoAlimentacionMotor tipo, decimal tensionV)
    {
        var (_, columnas) = Config[tipo];
        if (columnas.ContainsKey((int)tensionV)) return (int)tensionV;

        if (!IntervalosDeSistema.TryGetValue(tipo, out var intervalos)) return null;

        foreach (var (min, max, nominal) in intervalos)
            if (tensionV >= min && tensionV <= max && columnas.ContainsKey(nominal))
                return nominal;

        return null;
    }

    /// <summary>La tabla de la norma que da la FLC de cada tipo de alimentación: «430-250» en trifásico.</summary>
    public static string TablaDe(TipoAlimentacionMotor tipo) => Config[tipo].TablaId;
}
