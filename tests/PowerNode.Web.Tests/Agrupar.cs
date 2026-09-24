using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// Antes había un número de «Agrupados» para todo el tablero; ahora el agrupamiento sale de qué
/// circuitos van juntos (decisión canalizaciones-y-agrupamiento, 2026-09-24). Las pruebas que fijaban
/// «9 agrupados» arman ahora una canalización real con los portadores equivalentes.
/// </summary>
internal static class Agrupar
{
    /// <summary>
    /// Pone el circuito —ya con su carga— en una canalización compartida y le agrega circuitos de 1
    /// polo de 100 VA (2 portadores cada uno: fase y neutro) mientras no pase de
    /// <paramref name="portadores"/>. Con 9 quedan 8, del mismo renglón de la Tabla 310-15(b)(3)(a)
    /// (7 a 9, 70 %).
    /// </summary>
    public static CanalizacionDelTablero EnTubo(CuadroDeCarga cuadro, CircuitoDelCuadro circuito, int portadores)
    {
        var tubo = cuadro.Datos.NuevaCanalizacion();
        circuito.Canalizacion = tubo.Id;
        cuadro.Recalcular();

        var libres = cuadro.Circuitos
            .Where(c => c != circuito && !c.EsContinuacion && !c.TieneCarga && c.Polos == 1)
            .OrderByDescending(c => c.Espacio)
            .ToList();
        foreach (var otro in libres)
        {
            if (tubo.Conteo!.Portadores + 2 > portadores)
                break;
            otro.NoContinua = 100m;
            otro.Canalizacion = tubo.Id;
            cuadro.Recalcular();
        }

        return tubo;
    }
}
