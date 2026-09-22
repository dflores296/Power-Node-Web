using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Una <b>opción de terminal</b> de un interruptor: cuántos conductores admite esa zapata y entre qué
/// calibres, por material.
///
/// <para>
/// <b>Es una opción, no "el rango" del interruptor</b>, y esa distinción salió del catálogo, no de un
/// gusto de diseño. Un mismo aparato publica varias: el QO de 1 polo a 20 A acepta <b>un</b> conductor
/// de 14–8 en cualquier material <b>o dos</b> de 14–10 <b>solo en cobre</b> — dos rangos distintos
/// para el mismo interruptor, y el segundo es más estrecho. Y un marco P acepta la zapata estándar de
/// 500 kcmil o el kit opcional de 750. Guardar un solo intervalo obligaba a elegir cuál mentir.
/// </para>
/// </summary>
/// <param name="MinimoAl">Designación del calibre más chico que admite en aluminio. <c>null</c> = la
/// terminal <b>no</b> está identificada para aluminio, que es un caso real y no un dato faltante.</param>
/// <param name="ConductoresPorTerminal">Cuántos conductores caben en esa zapata. El catálogo lo
/// escribe al frente del rango: "(2) 14–10 Cu", "(3) 3/0 AWG–500 kcmil", "(4) 3/0–500 kcmil".
/// <b>Es un número, no un sí/no</b>: los marcos grandes llegan a cuatro por zapata, y un booleano
/// —que es como estuvo hasta el 2026-08-18— no puede decirlo.</param>
/// <param name="Kit">Número de catálogo del kit de zapatas, cuando el documento lo nombra
/// (`AL150HD`, `AL800P7K`…). Sirve para que el aviso diga <b>qué pedir</b> en vez de solo que no
/// cabe. <c>null</c> en las terminales que el catálogo no numera, como las del QO.</param>
/// <param name="EsEstandar">La que trae el interruptor de fábrica. Las demás son kits opcionales:
/// existen, se piden aparte, y por eso el aviso las nombra en vez de darlas por puestas.</param>
public sealed record RangoTerminal(
    string? MinimoAl, string? MaximoAl,
    string? MinimoCu, string? MaximoCu,
    int ConductoresPorTerminal = 1,
    string? Kit = null,
    bool EsEstandar = true);

/// <summary>
/// Verifica que el conductor que eligió el motor <b>quepa físicamente</b> en la terminal del
/// interruptor — <b>110-14</b>.
///
/// <b>Es una comprobación que no existía, y es distinta de la de temperatura.</b> El motor ya aplica
/// 110-14(c) (la columna de 60/75 °C según el amperaje) desde hace tiempo; eso es el lado
/// <i>térmico</i>. Éste es el lado <i>físico y de material</i>, y hasta ahora nadie lo miraba: el
/// programa podía calcular 300 kcmil por caída de tensión y mandarlo a un interruptor cuya zapata
/// acepta hasta 2/0, sin decir nada.
///
/// <b>Lo que dice la norma</b> (110-14, preámbulo, textual): <i>"Debido a que metales distintos tienen
/// características diferentes, las terminales a compresión, empalmes a compresión y terminales
/// soldadas <b>se deben identificar para el material del conductor</b> y se deben instalar y usar
/// apropiadamente."</i> Y 110-14(a): <i>"Las terminales para <b>más de un conductor</b> y las
/// terminales utilizadas para conectar <b>aluminio</b>, deben estar identificadas para ese uso."</i>
///
/// Por eso el rango se guarda **por material** y no como un solo intervalo: el catálogo publica casos
/// donde el de aluminio y el de cobre <b>no coinciden</b> (un QO-GFI de 40–60 A acepta 12–4 en
/// aluminio pero 14–6 en cobre), y casos donde la opción de dos conductores existe <b>solo en
/// cobre</b> — que es literalmente lo que 110-14(a) exige identificar.
///
/// <b>Aviso, no bloqueo</b>, como todo en esta casa: el conductor calculado es correcto por ampacidad
/// y por caída de tensión; lo que el aviso dice es que <b>ese interruptor no es el adecuado</b> para
/// recibirlo, y eso lo resuelve el proyectista cambiando de modelo o pidiendo el kit de zapatas que
/// sí lo acepta.
/// </summary>
public static class VerificacionTerminal
{
    /// <param name="Cabe"><c>null</c> cuando <b>no se conoce el rango</b> de ese interruptor. Callar
    /// cuando falta el dato, igual que en cortocircuito: <c>null</c> no es <c>false</c>.</param>
    public sealed record Resultado(bool? Cabe, string? Aviso, IReadOnlyList<Cita> Citas);

    public static Resultado Verificar(
        ICatalogoCalibres catalogo,
        RangoTerminal? rango,
        Calibre calibre,
        MaterialConductor material,
        int conductoresPorTerminal = 1) =>
        Verificar(catalogo, rango is null ? [] : [rango], calibre, material, conductoresPorTerminal);

    /// <summary>
    /// Con todas las opciones de terminal que el catálogo publica para ese interruptor. <b>Cabe si
    /// cabe en alguna</b>, y si solo cabe en un kit opcional, el aviso dice cuál pedir.
    /// </summary>
    public static Resultado Verificar(
        ICatalogoCalibres catalogo,
        IReadOnlyList<RangoTerminal> opciones,
        Calibre calibre,
        MaterialConductor material,
        int conductoresPorTerminal = 1)
    {
        if (opciones.Count == 0)
            return new Resultado(null, null, []);

        var esCobre = material == MaterialConductor.Cobre;
        var nombreMaterial = esCobre ? "cobre" : "aluminio";

        // 110-14: la terminal tiene que estar identificada para el material. Que NINGUNA opción tenga
        // rango para ese material no es un hueco de datos: es que el aparato no lo admite.
        var delMaterial = opciones.Where(o => Rango(o, esCobre) is not (null, null)).ToList();
        if (delMaterial.Count == 0)
        {
            var avisoMaterial =
                $"La terminal de este interruptor no está identificada para {nombreMaterial}. 110-14 exige que las terminales " +
                $"se identifiquen para el material del conductor, y 110-14(a) lo repite para las de aluminio.";
            return new Resultado(false, avisoMaterial, [new Cita("110-14", avisoMaterial)]);
        }

        // 110-14(a): "Las terminales para más de un conductor [...] deben estar identificadas para ese
        // uso". Una zapata de N conductores admite menos de N, nunca más.
        var admitenLosConductores = delMaterial.Where(o => o.ConductoresPorTerminal >= conductoresPorTerminal).ToList();
        if (admitenLosConductores.Count == 0)
        {
            var maximo = delMaterial.Max(o => o.ConductoresPorTerminal);
            var avisoDos =
                $"Se pretenden conectar {conductoresPorTerminal} conductores en una terminal identificada para {maximo} " +
                $"— 110-14(a). Hay que usar un interruptor con terminal de más conductores, o una zapata aparte.";
            return new Resultado(false, avisoDos, [new Cita("110-14(a)", avisoDos)]);
        }

        // Un rango que cita un calibre fuera de la Tabla 8 no se puede evaluar: se descarta la opción,
        // no se declara nada por ella.
        var evaluables = admitenLosConductores
            .Select(o => (Opcion: o, Min: Buscar(catalogo, Rango(o, esCobre).Min), Max: Buscar(catalogo, Rango(o, esCobre).Max)))
            .Where(x => x.Min is not null && x.Max is not null)
            .ToList();

        if (evaluables.Count == 0)
            return new Resultado(null, null, []);

        var cabeEn = evaluables.FirstOrDefault(x => calibre.AreaMm2 >= x.Min!.AreaMm2 && calibre.AreaMm2 <= x.Max!.AreaMm2);
        if (cabeEn.Opcion is not null)
        {
            var conKit = cabeEn.Opcion.EsEstandar || cabeEn.Opcion.Kit is null
                ? string.Empty
                : $", con el kit de zapatas {cabeEn.Opcion.Kit}";
            return new Resultado(true, null, [new Cita("110-14",
                $"El conductor {calibre} de {nombreMaterial} entra en el rango de la terminal " +
                $"({cabeEn.Min} a {cabeEn.Max}){conKit}.")]);
        }

        // No cabe en ninguna. El mensaje se arma con la ESTÁNDAR —que es la que trae el aparato— y
        // nombra el kit más grande publicado, para que se sepa si la salida es pedir zapatas o
        // cambiar de interruptor.
        var estandar = evaluables.FirstOrDefault(x => x.Opcion.EsEstandar);
        if (estandar.Opcion is null) estandar = evaluables[0];

        var mayor = evaluables.OrderByDescending(x => x.Max!.AreaMm2).First();
        var direccion = calibre.AreaMm2 > estandar.Max!.AreaMm2 ? "más grande" : "más chico";
        var aviso =
            $"El conductor calculado ({calibre} de {nombreMaterial}) es {direccion} de lo que acepta la terminal de este " +
            $"interruptor, que va de {estandar.Min} a {estandar.Max}. El conductor está bien por ampacidad y caída de tensión: " +
            $"lo que no corresponde es el interruptor. Cámbialo por uno de terminal mayor, o conecta con zapata aparte.";

        if (mayor.Opcion.Kit is not null && mayor.Max!.AreaMm2 > estandar.Max.AreaMm2)
            aviso += $" La zapata opcional más grande que publica el catálogo para este interruptor es {mayor.Opcion.Kit} " +
                     $"({mayor.Min} a {mayor.Max}), y tampoco lo acepta.";

        return new Resultado(false, aviso, [new Cita("110-14", aviso)]);
    }

    private static (string? Min, string? Max) Rango(RangoTerminal o, bool esCobre) =>
        esCobre ? (o.MinimoCu, o.MaximoCu) : (o.MinimoAl, o.MaximoAl);

    private static Calibre? Buscar(ICatalogoCalibres catalogo, string? designacion) =>
        designacion is null ? null : catalogo.BuscarPorDesignacion(designacion);
}
