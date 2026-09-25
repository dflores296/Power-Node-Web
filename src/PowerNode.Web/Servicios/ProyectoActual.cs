using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Servicios;

/// <summary>
/// El tablero que se está capturando, <b>uno solo</b> — el alcance de v1 es un cuadro de carga,
/// como el Excel original. Ver <c>docs/decisiones/alcance-v1-un-tablero.md</c>. Para varios a la vez,
/// una pestaña del navegador por tablero: cada pestaña es su propia aplicación, con su propio
/// proyecto, y la pestaña lleva el nombre del tablero (I-05).
///
/// <para>
/// Vive aquí y no dentro de la pantalla para que la captura y el documento que se imprime miren el
/// mismo objeto: si el documento recalculara por su cuenta, podría imprimir números que la pantalla
/// no enseñó, que es la peor forma de equivocarse en algo que se firma.
/// </para>
/// </summary>
public sealed class ProyectoActual
{
    private readonly MotorNom _motor;

    /// <summary>La huella de lo último que se guardó o se abrió: si la actual es otra, hay cambios sin guardar.</summary>
    private string _huellaGuardada;

    public ProyectoActual(MotorNom motor)
    {
        _motor = motor;
        Cuadro = new CuadroDeCarga(motor);
        _huellaGuardada = ArchivoDelCuadro.Huella(Cuadro);
    }

    /// <summary>Cambia al abrir un archivo: las páginas lo leen siempre de aquí, nunca lo guardan.</summary>
    public CuadroDeCarga Cuadro { get; private set; }

    /// <summary>Se abrió otro tablero: las páginas que lo muestran se vuelven a dibujar.</summary>
    public event Action? Cambio;

    /// <summary>Hay algo capturado que no está en ningún archivo. Un tablero nuevo, sin tocar, no.</summary>
    public bool SinGuardar => ArchivoDelCuadro.Huella(Cuadro) != _huellaGuardada;

    /// <summary>
    /// El nombre de la pestaña: «Tablero cocina — Power Node» (David, 2026-09-25). Con varias pestañas
    /// abiertas, es lo que dice cuál es cuál. Sin nombre, la clave; sin clave, «Tablero sin nombre».
    /// </summary>
    public string TituloDeVentana
    {
        get
        {
            var d = Cuadro.Datos;
            var nombre = !string.IsNullOrWhiteSpace(d.Tablero) ? d.Tablero.Trim()
                : !string.IsNullOrWhiteSpace(d.Clave) ? d.Clave.Trim()
                : "Tablero sin nombre";
            return $"{nombre} — Power Node";
        }
    }

    /// <summary>El archivo, listo para escribirse, y el nombre que se le sugiere.</summary>
    public (string Nombre, string Texto) ParaGuardar() =>
        (ArchivoDelCuadro.NombreSugerido(Cuadro.Datos), ArchivoDelCuadro.Guardar(Cuadro, DateTimeOffset.Now));

    /// <summary>El archivo ya se escribió: lo capturado hasta aquí está guardado.</summary>
    public void Guardado() => _huellaGuardada = ArchivoDelCuadro.Huella(Cuadro);

    /// <summary>
    /// Abre un archivo. Si se pudo leer, el tablero de la pestaña pasa a ser el del archivo; si no, se
    /// queda el que estaba y <see cref="Apertura.Error"/> dice por qué.
    /// </summary>
    public Apertura Abrir(string texto)
    {
        var apertura = ArchivoDelCuadro.Abrir(texto, _motor);
        if (apertura.Cuadro is { } cuadro)
        {
            Cuadro = cuadro;
            _huellaGuardada = ArchivoDelCuadro.Huella(cuadro);
            Cambio?.Invoke();
        }
        return apertura;
    }
}
