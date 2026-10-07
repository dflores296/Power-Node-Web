using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>El mínimo de 220-14 en «Carga c/u»</b> — D14 a D19 de decisiones/acomodo-del-desplegable.md (David, 2026-10-07:
/// «Sí a todo»). Antes, el campo podía quedar en 0 o abajo del mínimo y una nota debajo decía «mín. 180 VA»; el cálculo
/// ya usaba el mínimo (el piso de <see cref="CuadroDeCarga"/>), pero el campo enseñaba otra cosa. Ahora el campo trae
/// el mínimo y no se deja debajo; el resultado no cambia.
/// </summary>
public static class MinimosDeLaCarga
{
    /// <summary>
    /// D16: contactos, portalámparas de servicio pesado y ensamble de salidas, solo en VA — su mínimo es en VA por
    /// salida. La secadora deja escoger: es un aparato con placa en W.
    /// </summary>
    public static bool SoloEnVA(this SubtipoDeCarga s) => s is SubtipoDeCarga.ContactoUsoGeneral or SubtipoDeCarga.ContactoMultiple
        or SubtipoDeCarga.ContactoBano or SubtipoDeCarga.ContactoRefrigerador or SubtipoDeCarga.PortalamparasPesado
        or SubtipoDeCarga.EnsambleDeSalidas;

    /// <summary>
    /// El mínimo por salida de la línea, en su unidad, con su referencia; <c>null</c> si su subtipo no lo lleva. Los
    /// anuncios (1,200 VA por circuito, 220-14(f)) solo si es su única línea del circuito (D18): con varias, queda el piso
    /// del cálculo.
    /// </summary>
    public static (decimal Valor, string Referencia)? MinimoDe(CuadroDeCarga cuadro, CircuitoDelCuadro c, CargaDelCircuito a) =>
        MinimoDe(cuadro, c, a, cuadro.Datos.Inmueble.EsVivienda());

    private static (decimal Valor, string Referencia)? MinimoDe(CuadroDeCarga cuadro, CircuitoDelCuadro c, CargaDelCircuito a, bool vivienda)
    {
        if (a.Subtipo is not { } s || a.Clase != ClaseDeAparato.Carga)
            return null;
        (decimal VA, string Referencia)? minimo = s.MinimoUnitarioVA(vivienda)
            ?? (s.MinimoPorCircuitoVA() is { } porCircuito && c.Cargas.Count(x => x.Subtipo == s) == 1
                ? (porCircuito.VA / Math.Max(1, a.Cantidad), porCircuito.Referencia)
                : null);
        if (minimo is not { } m)
            return null;
        var d = cuadro.Datos;
        var unoEnVA = ConsumoDePlaca.AVoltAmperes(1m, a.Unidad, d.TensionFaseNeutroV, d.TensionFaseFaseV, c.Polos, a.FactorPotencia);
        // Hacia arriba, a dos decimales: 5000 VA a 220 V son 22.73 A, nunca 22.72 (quedaría abajo del mínimo).
        var valor = unoEnVA > 0m ? Math.Ceiling(m.VA / unoEnVA * 100m) / 100m : m.VA;
        return (valor, m.Referencia);
    }

    /// <summary>D14: al escoger el subtipo, el campo trae el mínimo (y la unidad, VA, si solo va en VA — D16).</summary>
    public static void LlenarAlEscoger(CuadroDeCarga cuadro, CircuitoDelCuadro c, CargaDelCircuito a)
    {
        if (a.Subtipo?.SoloEnVA() == true)
            a.Unidad = UnidadConsumo.VoltAmperes;
        if (MinimoDe(cuadro, c, a) is { } m && a.CargaUnitaria < m.Valor)
            a.CargaUnitaria = m.Valor;
    }

    /// <summary>
    /// D15: lo escrito abajo del mínimo sube solo al mínimo. Regresa el aviso («Contacto: no menos de 180 VA.»), o
    /// <c>null</c> si no hizo falta.
    /// </summary>
    public static string? SubirAlMinimo(CuadroDeCarga cuadro, CircuitoDelCuadro c, CargaDelCircuito a)
    {
        if (MinimoDe(cuadro, c, a) is not { } m || a.CargaUnitaria >= m.Valor)
            return null;
        a.CargaUnitaria = m.Valor;
        return $"{NombreCorto(a.Subtipo!.Value)}: no menos de {m.Valor:#,0.##} {Simbolo(a.Unidad)}.";
    }

    /// <summary>D16: la secadora cambió de unidad; si traía el mínimo, lo trae en la unidad nueva.</summary>
    public static void CambiarUnidad(CuadroDeCarga cuadro, CircuitoDelCuadro c, CargaDelCircuito a, UnidadConsumo unidad)
    {
        var traiaElMinimo = MinimoDe(cuadro, c, a) is { } antes && a.CargaUnitaria == antes.Valor;
        a.Unidad = unidad;
        if (traiaElMinimo && MinimoDe(cuadro, c, a) is { } despues)
            a.CargaUnitaria = despues.Valor;
    }

    /// <summary>
    /// D17: al cambiar el inmueble, la secadora que sigue con el mínimo que se llenó solo toma el del inmueble nuevo
    /// (5,000 VA en vivienda; fuera de ella, ninguno: 0). Si se escribió otra carga, no se toca.
    /// </summary>
    public static void CambiarInmueble(CuadroDeCarga cuadro, TipoDeInmueble nuevo)
    {
        var anterior = cuadro.Datos.Inmueble;
        var lineas = cuadro.Circuitos.SelectMany(c => c.Cargas.Select(a => (c, a)))
            .Where(x => MinimoDe(cuadro, x.c, x.a, anterior.EsVivienda()) is { } m && x.a.CargaUnitaria == m.Valor)
            .ToList();
        cuadro.Datos.Inmueble = nuevo;
        foreach (var (c, a) in lineas)
            a.CargaUnitaria = MinimoDe(cuadro, c, a) is { } m ? m.Valor : 0m;
    }

    /// <summary>
    /// D19: un archivo con la carga en 0 enseña el mínimo al abrir; y un contacto en W o A pasa a VA (D16), con los
    /// mismos VA. El resultado no cambia: el cálculo ya usaba el mínimo.
    /// </summary>
    public static void LlenarAlAbrir(CuadroDeCarga cuadro)
    {
        var d = cuadro.Datos;
        foreach (var c in cuadro.Circuitos)
            foreach (var a in c.Cargas.Where(a => a.Clase == ClaseDeAparato.Carga))
            {
                if (a.Subtipo?.SoloEnVA() == true && a.Unidad != UnidadConsumo.VoltAmperes)
                {
                    a.CargaUnitaria = ConsumoDePlaca.AVoltAmperes(a.CargaUnitaria, a.Unidad, d.TensionFaseNeutroV, d.TensionFaseFaseV, c.Polos, a.FactorPotencia);
                    a.Unidad = UnidadConsumo.VoltAmperes;
                }
                if (a.CargaUnitaria == 0m && MinimoDe(cuadro, c, a) is { } m)
                    a.CargaUnitaria = m.Valor;
            }
    }

    private static string NombreCorto(SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.ContactoUsoGeneral or SubtipoDeCarga.ContactoBano or SubtipoDeCarga.ContactoRefrigerador => "Contacto",
        SubtipoDeCarga.ContactoMultiple => "Contacto múltiple",
        SubtipoDeCarga.PortalamparasPesado => "Portalámparas",
        SubtipoDeCarga.Anuncios => "Anuncios",
        _ => s.Nombre(),
    };

    public static string Simbolo(UnidadConsumo u) => u switch
    {
        UnidadConsumo.Watts => "W",
        UnidadConsumo.Amperes => "A",
        _ => "VA",
    };
}
