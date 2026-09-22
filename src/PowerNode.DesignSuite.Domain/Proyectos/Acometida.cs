using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// El punto de entrega de la suministradora — la raíz del árbol del proyecto. Antes de esta
/// entidad la raíz era un tablero huérfano (uno sin <see cref="ElementoTopologia.AlimentadorEntrante"/>),
/// que funciona para la cascada pero deja sin lugar el dato que de verdad falta:
/// <see cref="CorrienteFallaDisponibleA"/>.
///
/// Para la cascada es un elemento **transparente**: no aporta carga propia, solo deja pasar la de todo
/// lo que cuelga de ella. Lo que aporta es hacia abajo, no hacia arriba — la corriente de falla
/// disponible de la red, que es la referencia contra la que se compara la capacidad interruptiva
/// de los tableros (ver <see cref="Calculo.Casos.CalculadoraImpedanciaTransformador"/>: hoy la
/// falla se calcula con barra infinita justamente porque no había dónde poner el dato real).
/// </summary>
public class Acometida : ElementoTopologia
{
    /// <summary>Tensión nominal de suministro, entre fases.</summary>
    public decimal TensionV { get; set; }

    /// <summary>1, 2 o 3 fases. Misma convención que <see cref="Tablero.Fases"/> (ver "Magnitudes" en PLAN-V1.md).</summary>
    public int Fases { get; set; } = 3;

    public int Hilos { get; set; } = 4;

    /// <summary>
    /// Corriente de cortocircuito disponible en el punto de entrega, en Amperes. **Es un dato de
    /// la suministradora** (viene en la factibilidad o se pide a CFE), no se calcula — igual que el
    /// %Z de un transformador viene de la placa.
    ///
    /// Solo se usa si <see cref="MetodoCortocircuito"/> es
    /// <see cref="Unidades.MetodoCortocircuito.CorrienteDeLaSuministradora"/>. 0 = no capturada.
    /// </summary>
    public decimal CorrienteFallaDisponibleA { get; set; }

    /// <summary>
    /// Con qué hipótesis se evalúa el cortocircuito. **Barra infinita es el valor por omisión y
    /// siempre está disponible**: pedir el dato a la suministradora es el deber ser de un proyecto
    /// formal, pero no puede ser requisito para poder calcular. Ver <see cref="MetodoCortocircuito"/>.
    /// </summary>
    public MetodoCortocircuito MetodoCortocircuito { get; set; } = MetodoCortocircuito.BarraInfinita;

    /// <summary>
    /// La corriente de falla que el análisis debe usar, o <c>null</c> si toca resolver por barra
    /// infinita. Devuelve null tanto cuando el método elegido es barra infinita como cuando se pidió
    /// usar el dato de la suministradora pero no se capturó (0) — así quien consuma esto no tiene
    /// que repetir las dos condiciones, y **el cálculo nunca se queda sin poder correr**: si esto es
    /// null, se sigue con barra infinita, que es exactamente lo que se hacía antes de que existiera
    /// la Acometida.
    /// </summary>
    public decimal? CorrienteFallaParaAnalisisA =>
        MetodoCortocircuito == MetodoCortocircuito.CorrienteDeLaSuministradora && CorrienteFallaDisponibleA > 0
            ? CorrienteFallaDisponibleA
            : null;

    /// <summary>
    /// Cómo está puesto a tierra el sistema de la acometida. **Campo de captura y memoria, no de
    /// cálculo todavía**: hoy ningún motor lo consume, se guarda para la memoria de cálculo y para
    /// que la UI lo muestre. Ver <see cref="PuestaTierraSistema"/>.
    /// </summary>
    public PuestaTierraSistema PuestaTierra { get; set; } = PuestaTierraSistema.SolidamentePuestoATierra;
}
