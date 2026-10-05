using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>La tensión de placa, como la escoge la pantalla</b> — I-189. Desde el 2026-10-05 un equipo de A/C o un variador
/// no se calcula sin ella. Las pruebas que no son de eso escogen la que corresponde a los polos que ya tiene el
/// circuito: el resultado es el de antes.
/// </summary>
internal static class Placas
{
    public static CircuitoDelCuadro ConSuTension(this CuadroDeCarga cuadro, CircuitoDelCuadro c)
    {
        var t = TensionesDePlaca.ConPolos(cuadro.Datos, c.Polos);
        Assert.True(t is not null, $"El tablero no tiene tensión de placa de {c.Polos} polos.");
        Assert.Null(cuadro.CambiarTensionDePlaca(c, t));
        return c;
    }
}
