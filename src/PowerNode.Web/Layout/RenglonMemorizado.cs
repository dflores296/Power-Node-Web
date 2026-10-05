using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Layout;

/// <summary>
/// <b>Un renglón del cuadro que solo se dibuja si cambió</b> — I-188, decisión <c>dibujo-por-renglon.md</c>.
///
/// <para>
/// Envuelve el marcado de un circuito de <c>Captura.razor</c> sin cambiarlo: el marcado sigue allí, con sus
/// eventos, que van a la página como siempre. Cuando la página se vuelve a dibujar, este componente compara la
/// firma del circuito (<see cref="FirmaDeDibujo.De(CircuitoDelCuadro)"/>) y la del tablero
/// (<see cref="FirmaDeDibujo.De(DatosDelTablero)"/>) con las del último dibujo: iguales, se lo salta, y el
/// navegador no toca esos elementos. Con el desplegable abierto se dibuja siempre: lee otros circuitos (el par
/// no simultáneo) y las tablas de HP, y es uno o dos a la vez.
/// </para>
/// </summary>
public sealed class RenglonMemorizado : ComponentBase
{
    /// <summary>El marcado del renglón, el de siempre.</summary>
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>Lo que enseña el circuito: lo capturado, lo calculado y sus líneas.</summary>
    [Parameter, EditorRequired] public FirmaDelRenglon Firma { get; set; } = default!;

    /// <summary>Lo del tablero que leen todos los renglones.</summary>
    [Parameter, EditorRequired] public string Tablero { get; set; } = default!;

    /// <summary>El desplegable está abierto: se dibuja siempre.</summary>
    [Parameter] public bool Abierto { get; set; }

    private FirmaDelRenglon? _firma;
    private string? _tablero;
    private bool _abierto;

    protected override bool ShouldRender() =>
        Abierto || _abierto || !Firma.Equals(_firma) || Tablero != _tablero;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Lo que se dibujó, para comparar la próxima vez (también en el primer dibujo, que no pregunta).
        _firma = Firma;
        _tablero = Tablero;
        _abierto = Abierto;
        builder.AddContent(0, ChildContent);
    }
}
