using System.Text.RegularExpressions;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Lo que leen el renglón y el gabinete fuera de su firma</b> — I-188. <c>FirmaDeDibujoTests</c> cuida las
/// propiedades del circuito, de sus líneas y del tablero; no puede ver si el marcado empieza a leer otra cosa del
/// cuadro (un valor nuevo de <c>CuadroDeCarga</c>, otro dato del tablero). Si eso pasa sin agregarlo a la firma,
/// el renglón o el gabinete se quedan con el valor viejo en pantalla. Estas pruebas leen el marcado y fallan si
/// aparece algo de <c>Cuadro</c>, <c>Datos</c> o <c>CuadroDeCarga</c> que no esté en la lista: al agregarlo, revisar
/// que la firma lo cubra (<c>FirmaDeDibujo</c>, o la de <c>InteriorDelGabinete</c>) y luego sí, a la lista.
/// </summary>
public class LoQueLeeElDibujoTests
{
    private static string Fuente(params string[] ruta)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "CLAUDE.md")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine([dir!.FullName, .. ruta]));
    }

    private static SortedSet<string> LoQueLee(string marcado) =>
        [.. Regex.Matches(Regex.Replace(marcado, @"@\*[\s\S]*?\*@", ""), @"(?<![.\w])(Cuadro|Datos|CuadroDeCarga)\.([A-Z]\w*)")
            .Select(m => $"{m.Groups[1].Value}.{m.Groups[2].Value}")];

    [Fact]
    public void I188_ElRenglonCerradoSoloLeeLoQueCubreSuFirma()
    {
        var captura = Fuente("src", "PowerNode.Web", "Pages", "Captura.razor");
        var inicio = captura.IndexOf("<RenglonMemorizado", StringComparison.Ordinal);
        var fin = captura.IndexOf("@if (Abierto(c))", inicio, StringComparison.Ordinal); // abierto, se dibuja siempre
        Assert.True(inicio > 0 && fin > inicio, "No se encontró el renglón memorizado en Captura.razor.");

        // Cada uno, cubierto: los avisos y el desglose salen del circuito, su canalización y el tablero; la
        // protección fijada, del circuito; barras, canalizaciones y polos, de FirmaDeDibujo.De(Datos). Los demás
        // circuitos (la lista de «No simultáneo con», D11), de FirmaDelPar(), que va con la firma del tablero.
        Assert.Equal(
            ["Cuadro.AvisosDe", "Cuadro.Circuitos", "Cuadro.Desglose", "CuadroDeCarga.ProteccionFijada", "Datos.Barras", "Datos.Canalizaciones", "Datos.MaximoPolos"],
            LoQueLee(captura[inicio..fin]));
    }

    [Fact]
    public void I188_ElGabineteSoloLeeLoQueCubreSuFirma()
    {
        var gabinete = Fuente("src", "PowerNode.Web", "Layout", "InteriorDelGabinete.razor");
        // Cada uno está en InteriorDelGabinete.Firma(); Cuadro.Datos es el acceso a los datos de abajo, y
        // CuadroDeCarga.Zocalo, una constante (el destino «zócalo» del arrastre).
        Assert.Equal(
            ["Cuadro.AvisoDelPrincipal", "Cuadro.Datos", "Cuadro.EspaciosDelPrincipal", "Cuadro.Gabinete", "Cuadro.InterruptorPrincipalA",
             "Cuadro.UnaSolaBarra", "CuadroDeCarga.Zocalo", "Datos.Barras", "Datos.NumeroEspacios", "Datos.PolosDelPrincipal", "Datos.PrincipalEnEspacios",
             "Datos.Sistema", "Datos.UsaInterruptorPrincipal"],
            LoQueLee(gabinete));
    }
}
