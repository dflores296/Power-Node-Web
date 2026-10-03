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
        Cuadro = EnBlanco(motor);
        _huellaGuardada = ArchivoDelCuadro.Huella(Cuadro);
    }

    private static CuadroDeCarga EnBlanco(MotorNom motor)
    {
        var cuadro = new CuadroDeCarga(motor) { ExigirTipo = true };
        cuadro.Recalcular();
        return cuadro;
    }

    /// <summary>
    /// <b>Un tablero nuevo, en blanco</b> — I-186: el de una pestaña recién abierta, sin cambios que guardar.
    /// Quien llama pregunta antes si hay cambios sin guardar.
    /// </summary>
    public void Nuevo()
    {
        Cuadro = EnBlanco(_motor);
        _huellaGuardada = ArchivoDelCuadro.Huella(Cuadro);
        Cambio?.Invoke();
    }

    /// <summary>Cambia al abrir un archivo: las páginas lo leen siempre de aquí, nunca lo guardan.</summary>
    public CuadroDeCarga Cuadro { get; private set; }

    /// <summary>Se abrió otro tablero, o uno nuevo: las páginas que lo muestran se vuelven a dibujar.</summary>
    public event Action? Cambio;

    /// <summary>Hay algo capturado que no está en ningún archivo. Un tablero nuevo, sin tocar, no.</summary>
    public bool SinGuardar => ArchivoDelCuadro.Huella(Cuadro) != _huellaGuardada;

    /// <summary>
    /// <see cref="SinGuardar"/> y la huella con que se decidió, en una sola serialización: la huella es
    /// el archivo completo sin la fecha, y es lo que se copia en la pestaña (P1-3).
    /// </summary>
    public (bool SinGuardar, string Huella) Estado()
    {
        var huella = ArchivoDelCuadro.Huella(Cuadro);
        return (huella != _huellaGuardada, huella);
    }

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
    public Apertura Abrir(string texto) => Cargar(texto, guardado: true);

    /// <summary>
    /// Recupera la copia de la pestaña (P1-3): se abre como un archivo, pero sigue <b>sin guardar</b>
    /// —no está en ningún archivo—, así que al cerrar la pestaña el navegador pregunta.
    /// </summary>
    public Apertura Recuperar(string texto) => Cargar(texto, guardado: false);

    private Apertura Cargar(string texto, bool guardado)
    {
        var apertura = ArchivoDelCuadro.Abrir(texto, _motor);
        if (apertura.Cuadro is { } cuadro)
        {
            cuadro.ExigirTipo = true;
            cuadro.Recalcular();
            Cuadro = cuadro;
            if (guardado)
                _huellaGuardada = ArchivoDelCuadro.Huella(cuadro);
            Cambio?.Invoke();
        }
        return apertura;
    }
}
