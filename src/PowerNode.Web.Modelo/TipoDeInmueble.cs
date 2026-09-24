namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>El inmueble del tablero, una sola lista para dos criterios</b> — R-19 (David, 2026-09-24).
///
/// <para>
/// Es la unión de lo que distingue cada criterio, y cada uno la lee a su manera:
/// </para>
/// <list type="bullet">
/// <item><b>230-79</b>, mínimo del medio de desconexión de la acometida: vivienda unifamiliar
/// (según carga), vivienda popular hasta 60 m² (30 A), todos los demás (60 A).</item>
/// <item><b>Tabla 220-42</b>, alumbrado general: unidades de vivienda, hospitales, hoteles y
/// moteles, almacenes, todos los demás (100 %, sin reducción).</item>
/// <item><b>Vivienda o no</b>: 220-44 y 220-56 solo fuera de vivienda; 220-53 a 220-55, 220-82 y
/// 220-83 solo en vivienda. Multifamiliar (220-84), escuelas (220-86), restaurantes (220-88).</item>
/// </list>
/// <para>
/// 230-79 separa vivienda unifamiliar de popular y junta hospital, hotel y almacén; 220-42 hace lo
/// contrario. Ninguna opción de esta lista queda con dos respuestas en un mismo criterio.
/// </para>
/// </summary>
public enum TipoDeInmueble
{
    ViviendaUnifamiliar,
    ViviendaPopular,
    ViviendaMultifamiliar,
    Hospital,
    HotelOMotel,
    Almacen,
    Escuela,
    Restaurante,

    /// <summary>Oficina, comercio, industria y todo lo demás.</summary>
    Otro,
}

public static class TiposDeInmueble
{
    public static string Nombre(this TipoDeInmueble inmueble) => inmueble switch
    {
        TipoDeInmueble.ViviendaUnifamiliar => "Vivienda unifamiliar",
        TipoDeInmueble.ViviendaPopular => "Vivienda popular (hasta 60 m²)",
        TipoDeInmueble.ViviendaMultifamiliar => "Vivienda multifamiliar",
        TipoDeInmueble.Hospital => "Hospital",
        TipoDeInmueble.HotelOMotel => "Hotel o motel",
        TipoDeInmueble.Almacen => "Almacén",
        TipoDeInmueble.Escuela => "Escuela",
        TipoDeInmueble.Restaurante => "Restaurante",
        _ => "Otro (oficina, comercio, industria)",
    };

    public static bool EsVivienda(this TipoDeInmueble inmueble) =>
        inmueble is TipoDeInmueble.ViviendaUnifamiliar or TipoDeInmueble.ViviendaPopular or TipoDeInmueble.ViviendaMultifamiliar;

    /// <summary>
    /// El uso de los contactos (aparatos pequeños, lavadora, baño) solo cuenta en vivienda:
    /// 210-11(c) es «Unidades de vivienda» y 220-52 «Cargas de aparatos pequeños y lavadoras en
    /// unidades de vivienda». Y no en la popular de hasta 60 m²: 210-11(c), Excepción 1, y la
    /// excepción de 220-52 — I-46.
    /// </summary>
    public static bool AplicaUsoDeContactos(this TipoDeInmueble inmueble) =>
        inmueble is TipoDeInmueble.ViviendaUnifamiliar or TipoDeInmueble.ViviendaMultifamiliar;

    /// <summary>
    /// 230-79: (c) vivienda unifamiliar según la carga conectada —sin número, <c>null</c>—; vivienda
    /// popular hasta 60 m², 30 A; (d) todos los demás, 60 A. Multifamiliar no es unifamiliar: (d).
    /// </summary>
    public static (decimal Amperes, string Referencia)? Minimo230_79(this TipoDeInmueble inmueble) => inmueble switch
    {
        TipoDeInmueble.ViviendaUnifamiliar => null,
        TipoDeInmueble.ViviendaPopular => (30m, "230-79(c)"),
        _ => (60m, "230-79(d)"),
    };

    /// <summary>
    /// El renglón de la Tabla 220-42 que le toca. <c>null</c> en «todos los demás», que va al 100 %:
    /// ahí la tabla no justifica ninguna reducción.
    /// </summary>
    public static string? FilaTabla220_42(this TipoDeInmueble inmueble) => inmueble switch
    {
        _ when inmueble.EsVivienda() => "unidades de vivienda",
        TipoDeInmueble.Hospital => "hospitales",
        TipoDeInmueble.HotelOMotel => "hoteles y moteles",
        TipoDeInmueble.Almacen => "almacenes",
        _ => null,
    };
}
