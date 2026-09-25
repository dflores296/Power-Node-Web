namespace PowerNode.Web.Modelo;

/// <summary>
/// Cómo terminó arrastrar un interruptor a otro espacio — I-69. Si no se pudo, el tablero quedó
/// igual y <see cref="Mensaje"/> dice por qué, ya redactado para el aviso.
/// </summary>
public sealed record ResultadoDelMovimiento(ResultadoDelMovimiento.Tipo Clase, string? Mensaje)
{
    public enum Tipo { Hecho, SinCambio, Ocupada, NoValida }

    public bool Movio => Clase == Tipo.Hecho;

    public static ResultadoDelMovimiento Hecho(string mensaje) => new(Tipo.Hecho, mensaje);
    public static ResultadoDelMovimiento SinCambio { get; } = new(Tipo.SinCambio, null);
    public static ResultadoDelMovimiento Ocupada(string motivo) => new(Tipo.Ocupada, $"Posición ocupada. {motivo}");
    public static ResultadoDelMovimiento NoValida(string motivo) => new(Tipo.NoValida, $"Posición no válida. {motivo}");
}
