using PowerNode.Web.Modelo;

namespace PowerNode.Web.Servicios;

/// <summary>
/// El tablero que se está capturando, <b>uno solo</b> — el alcance de v1 es un cuadro de carga,
/// como el Excel original. Ver <c>docs/decisiones/alcance-v1-un-tablero.md</c>.
///
/// <para>
/// Vive aquí y no dentro de la pantalla para que la captura y el documento que se imprime miren el
/// mismo objeto: si el documento recalculara por su cuenta, podría imprimir números que la pantalla
/// no enseñó, que es la peor forma de equivocarse en algo que se firma.
/// </para>
/// </summary>
public sealed class ProyectoActual
{
    public ProyectoActual(MotorNom motor) => Cuadro = new CuadroDeCarga(motor);

    public CuadroDeCarga Cuadro { get; }
}
