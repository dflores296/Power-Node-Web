namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Un interruptor montado, tal como se dibuja el interior del tablero</b>: una celda tan alta
/// como polos tiene, en la columna que le toca.
///
/// <para>
/// <b>Una celda por INTERRUPTOR, no por espacio</b> — es la corrección que el editor de gabinete de
/// escritorio hizo el 2026-08-20, y la razón es de fondo: en el cuadro de carga del Excel, y en
/// cualquier directorio de tablero real, un interruptor de 3 polos es <b>una</b> celda que abarca
/// tres renglones, no tres celdas sueltas con una flecha de «continúa».
/// </para>
///
/// <para>Las coordenadas se cuentan desde 1, listas para una rejilla CSS.</para>
/// </summary>
/// <param name="Fila">Renglón donde empieza: el espacio 1 y el 2 están en el primero, el 3 y el 4 en el segundo…</param>
/// <param name="Columna">1 = nones (izquierda), 2 = pares (derecha).</param>
/// <param name="Espacios">Cuántos renglones abarca: sus polos.</param>
/// <param name="Numeros">Los espacios que se come: «1» o «1-3-5», como en un directorio de tablero.</param>
/// <param name="Barras">Las barras que toca, en orden: «A», «AB», «ABC».</param>
public sealed record BloqueDelGabinete(
    CircuitoDelCuadro Circuito,
    int Fila,
    int Columna,
    int Espacios,
    string Numeros,
    string Barras)
{
    /// <summary>¿Hay un interruptor de verdad aquí, o es un espacio sin usar?</summary>
    public bool Ocupado => Circuito.TieneCarga;
}
