using System.Globalization;
using System.Text.RegularExpressions;
using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Normativa;

// NACIDO EN LA WEB (2026-09-24). Lectores de las tablas del Capítulo 10 que dimensionan
// canalizaciones. Decisión: docs/decisiones/canalizaciones-y-agrupamiento.md.

/// <summary>Tabla 1 del Capítulo 10. Columnas: 0 = número de conductores, 1 = porcentaje.</summary>
public class TablaOcupacionJson(IFuenteTablas fuente) : ITablaOcupacion
{
    private const string TablaId = "1";
    private (decimal Uno, decimal Dos, decimal MasDeDos)? _cache;

    public decimal PorcentajeMaximo(int numeroConductores)
    {
        if (numeroConductores < 1)
            throw new ArgumentOutOfRangeException(nameof(numeroConductores), numeroConductores, "Una canalización vacía no tiene porcentaje de ocupación.");

        var (uno, dos, masDeDos) = _cache ??= Leer();
        return numeroConductores switch { 1 => uno, 2 => dos, _ => masDeDos };
    }

    private (decimal, decimal, decimal) Leer()
    {
        decimal? uno = null, dos = null, masDeDos = null;
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var rotulo = (f.Texto(0) ?? "").Trim();
            var pct = NormaParsing.Decimal(f.Texto(1));
            if (pct is null) continue;
            if (rotulo == "1") uno = pct;
            else if (rotulo == "2") dos = pct;
            else if (rotulo.StartsWith("Más de 2", StringComparison.OrdinalIgnoreCase)) masDeDos = pct;
        }

        return (uno ?? Falta("1"), dos ?? Falta("2"), masDeDos ?? Falta("Más de 2"));

        static decimal Falta(string fila) =>
            throw new InvalidOperationException($"La Tabla 1 del Capítulo 10 no trae la fila «{fila}».");
    }
}

/// <summary>
/// Tabla 4 del Capítulo 10: un bloque por tipo de tubo, cada uno con su renglón de título
/// («Artículo 344 – Tubo conduit metálico pesado (RMC)»), dos de encabezado y uno por tamaño.
/// Columnas de datos: 0 designación métrica, 1 tamaño comercial, 2 diámetro interior, 3 área al
/// 100 %, 4 al 60 %, 5 al 53 %, 6 al 31 %, 7 al 40 %.
///
/// <para>
/// <b>Cada bloque se reconoce por su artículo y su variante, no por el rótulo completo</b>, porque el
/// DOF trae dos rótulos que no se pueden leer al pie de la letra:
/// </para>
/// <list type="bullet">
/// <item>El Art. 358 sale como «Tubo conduit <b>no metálico</b> (EMT)»; el propio Art. 358 se titula
/// «Tubo conduit metálico ligero Tipo EMT». Además es el primer bloque y su título cae dentro de los
/// renglones de encabezado de la tabla (<c>header_rows</c> = 3): se reconoce por posición.</item>
/// <item>Hay <b>dos</b> bloques «Artículo 352 – … Cédula 80». El segundo da 56.40 mm interiores en
/// 53 (2), más que Cédula 40 en el mismo tamaño; una cédula 80 tiene la pared más gruesa y no puede
/// tener más diámetro interior que la 40. Sus valores son los del PVC tipo EB del NEC. <b>No se
/// ofrece</b>: con un rótulo que contradice sus números, cualquiera de las dos lecturas sería
/// adivinar. Se cuenta la ocurrencia y se descarta la segunda.</item>
/// </list>
/// </summary>
public class TablaTuboConduitJson(IFuenteTablas fuente) : ITablaTuboConduit
{
    private const string TablaId = "4";
    private Dictionary<TipoTuboConduit, List<TamanoDeTubo>>? _cache;

    public IReadOnlyList<TamanoDeTubo> Tamanos(TipoTuboConduit tipo)
    {
        _cache ??= Leer();
        return _cache.TryGetValue(tipo, out var lista)
            ? lista
            : throw new InvalidOperationException($"La Tabla 4 del Capítulo 10 no trae el bloque de {tipo.Nombre()}.");
    }

    private Dictionary<TipoTuboConduit, List<TamanoDeTubo>> Leer()
    {
        var resultado = new Dictionary<TipoTuboConduit, List<TamanoDeTubo>>();
        // El primer bloque (EMT) no trae renglón de título en los datos: su título es encabezado.
        TipoTuboConduit? actual = TipoTuboConduit.Emt;
        var cedula80Vistas = 0;

        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var c0 = f.Texto(0);
            if (c0 is null) continue; // renglón de unidades, cubierto por el rowspan del encabezado

            if (f.Texto(2) is null && f.Texto(1) is null)
            {
                // Renglón de título del bloque.
                actual = Reconocer(c0, ref cedula80Vistas);
                continue;
            }

            if (actual is not { } tipo) continue; // bloque descartado o desconocido
            if (!int.TryParse(c0.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var designacion))
                continue; // encabezado del bloque

            var diametro = NormaParsing.Decimal(f.Texto(2));
            var a100 = NormaParsing.Decimal(f.Texto(3));
            var a60 = NormaParsing.Decimal(f.Texto(4));
            var a53 = NormaParsing.Decimal(f.Texto(5));
            var a31 = NormaParsing.Decimal(f.Texto(6));
            var a40 = NormaParsing.Decimal(f.Texto(7));
            if (diametro is null || a100 is null || a60 is null || a53 is null || a31 is null || a40 is null)
                continue; // «––»: ese tamaño no existe en ese tipo

            if (!resultado.TryGetValue(tipo, out var lista))
                resultado[tipo] = lista = [];
            lista.Add(new TamanoDeTubo(designacion, Comercial(f.Texto(1)), diametro.Value, a100.Value, a60.Value, a53.Value, a31.Value, a40.Value));
        }

        foreach (var lista in resultado.Values)
            lista.Sort((a, b) => a.DesignacionMetrica.CompareTo(b.DesignacionMetrica));
        return resultado;
    }

    private static TipoTuboConduit? Reconocer(string titulo, ref int cedula80Vistas)
    {
        var t = titulo.Replace('‑', '-');
        if (t.Contains("LFNC-B")) return TipoTuboConduit.LfncB;
        if (t.Contains("LFNC-A")) return TipoTuboConduit.LfncA;
        if (t.Contains("(EMT)")) return TipoTuboConduit.Emt;
        if (t.Contains("(ENT)")) return TipoTuboConduit.Ent;
        if (t.Contains("(FMC)")) return TipoTuboConduit.Fmc;
        if (t.Contains("(IMC)")) return TipoTuboConduit.Imc;
        if (t.Contains("(LFMC)")) return TipoTuboConduit.Lfmc;
        if (t.Contains("(RMC)")) return TipoTuboConduit.Rmc;
        if (t.Contains("Cédula 40")) return TipoTuboConduit.PvcCedula40;
        if (t.Contains("Tipo A")) return TipoTuboConduit.PvcTipoA;
        if (t.Contains("Cédula 80"))
            return ++cedula80Vistas == 1 ? TipoTuboConduit.PvcCedula80 : null; // la segunda: descartada
        return null;
    }

    private static string Comercial(string? texto) => Regex.Replace((texto ?? "").Trim(), @"(\d)([¼½¾])", "$1 $2");
}

/// <summary>
/// Tabla 5 del Capítulo 10 (conductores aislados) y Tabla 8 (desnudos).
///
/// <para>
/// La Tabla 5 viene por grupos: un renglón «Tipo: …» a todo lo ancho y, dentro, bloques cuyo primer
/// renglón trae el rótulo de tipos con rowspan («THHN, THWN, THWN-2») seguido de mm², AWG/kcmil,
/// diámetro y área. Columnas: 0 tipos, 1 mm², 2 AWG/kcmil, 3 diámetro (mm), 4 área (mm²).
/// </para>
///
/// <para>
/// <b>Se entra por la designación AWG/kcmil, nunca por los mm²</b>: en el bloque THHN del DOF la
/// columna de mm² está corrida («6.63 | 8», «8.37 | 6», cuando son 8.37 y 13.30) mientras las áreas
/// del aislado son las correctas (23.61 y 32.71). Leer por mm² daría el 6 AWG en lugar del 8.
/// </para>
/// </summary>
public class TablaDimensionesConductorJson(IFuenteTablas fuente) : ITablaDimensionesConductor
{
    private Dictionary<(string Tipo, string Designacion), (decimal, decimal)>? _aislados;
    private Dictionary<string, (decimal Diametro, decimal Area, decimal Hilos)>? _desnudos;

    public (decimal DiametroMm, decimal AreaMm2)? Aislado(string designacion, string tipoAislamiento)
    {
        _aislados ??= LeerTabla5();
        return _aislados.TryGetValue((NormalizarTipo(tipoAislamiento), NormaParsing.DesignacionLimpia(designacion)), out var d) ? d : null;
    }

    public (decimal DiametroMm, decimal AreaMm2)? Desnudo(string designacion)
    {
        _desnudos ??= LeerTabla8();
        return _desnudos.TryGetValue(NormaParsing.DesignacionLimpia(designacion), out var d) ? (d.Diametro, d.Area) : null;
    }

    /// <summary>«THW- 2» → «THW-2», «RHW‑2» (guion no separable) → «RHW-2», mayúsculas, sin asterisco.</summary>
    internal static string NormalizarTipo(string tipo) =>
        Regex.Replace(tipo.Replace('‑', '-').ToUpperInvariant(), @"\s+", "").TrimEnd('*');

    private Dictionary<(string, string), (decimal, decimal)> LeerTabla5()
    {
        var resultado = new Dictionary<(string, string), (decimal, decimal)>();
        string[] tipos = [];

        foreach (var f in fuente.FilasDatos("5"))
        {
            var c0 = f.Texto(0);
            if (c0 is not null && f.Texto(1) is null)
            {
                tipos = []; // «Tipo: …» a todo lo ancho: empieza grupo
                continue;
            }
            if (c0 is not null)
                tipos = c0.Split(',').Select(NormalizarTipo).Where(t => t.Length > 0).ToArray();

            var designacion = NormaParsing.DesignacionLimpia(f.Texto(2) ?? "");
            var diametro = NormaParsing.Decimal(f.Texto(3));
            var area = NormaParsing.Decimal(f.Texto(4));
            if (designacion.Length == 0 || diametro is null || area is null) continue;

            foreach (var tipo in tipos)
                resultado.TryAdd((tipo, designacion), (diametro.Value, area.Value));
        }

        return resultado;
    }

    /// <summary>
    /// Tabla 8: 0 designación, 3 cantidad de hilos, 5 diámetro total (mm), 6 área total (mm²). Hay
    /// sólido y trenzado en calibres chicos; se toma el trenzado, que es el que se instala — el mismo
    /// criterio que <see cref="CatalogoCalibresJson"/>.
    /// </summary>
    private Dictionary<string, (decimal, decimal, decimal)> LeerTabla8()
    {
        var resultado = new Dictionary<string, (decimal Diametro, decimal Area, decimal Hilos)>();
        foreach (var f in fuente.FilasDatos("8"))
        {
            var designacion = NormaParsing.DesignacionLimpia(f.Texto(0) ?? "");
            var hilos = NormaParsing.Decimal(f.Texto(3)) ?? 1;
            var diametro = NormaParsing.Decimal(f.Texto(5));
            var area = NormaParsing.Decimal(f.Texto(6));
            if (designacion.Length == 0 || diametro is null || area is null) continue;
            if (!resultado.TryGetValue(designacion, out var existente) || hilos > existente.Hilos)
                resultado[designacion] = (diametro.Value, area.Value, hilos);
        }
        return resultado;
    }
}

/// <summary>
/// Tabla 310-15(b)(3)(c): 0 = distancia del techo a la base del tubo, en mm («De 0 hasta 13», «Más
/// de 13 hasta 90»…), 1 = °C que se suman. Los rangos son contiguos: el primero cuyo tope alcance la
/// altura es el que aplica.
/// </summary>
public class TablaTemperaturaAzoteaJson(IFuenteTablas fuente) : ITablaTemperaturaAzotea
{
    private const string TablaId = "310-15(b)(3)(c)";
    private List<(decimal Tope, decimal Sumador)>? _cache;

    public decimal Sumador(decimal alturaSobreTechoMm)
    {
        if (alturaSobreTechoMm < 0)
            throw new ArgumentOutOfRangeException(nameof(alturaSobreTechoMm), alturaSobreTechoMm, "La altura sobre el techo no puede ser negativa.");

        _cache ??= Leer();
        foreach (var (tope, sumador) in _cache)
            if (alturaSobreTechoMm <= tope)
                return sumador;

        throw new InvalidOperationException(
            $"La Tabla 310-15(b)(3)(c) llega a {_cache[^1].Tope:0} mm sobre el techo; {alturaSobreTechoMm:0} mm queda fuera. "
            + "Arriba de eso la tabla no aplica: captura el tubo sin «azotea».");
    }

    private List<(decimal, decimal)> Leer()
    {
        var resultado = new List<(decimal, decimal)>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var numeros = Regex.Matches(f.Texto(0) ?? "", @"\d+(\.\d+)?")
                .Select(m => decimal.Parse(m.Value, CultureInfo.InvariantCulture)).ToList();
            var sumador = NormaParsing.Decimal(f.Texto(1));
            if (numeros.Count == 0 || sumador is null) continue;
            resultado.Add((numeros[^1], sumador.Value));
        }
        return resultado.OrderBy(r => r.Item1).ToList();
    }
}
