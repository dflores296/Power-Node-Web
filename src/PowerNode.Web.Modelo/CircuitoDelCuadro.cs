using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// Un renglón del cuadro de carga: <b>el espacio de la barra y lo que se le colgó</b>.
///
/// <para>
/// Hay uno por espacio del tablero, ocupado o no — igual que el Excel, que trae los 42 renglones
/// dibujados y en blanco los que no se usan. Un interruptor de 2 o 3 polos <b>se queda en el
/// renglón donde empieza</b> y se come los siguientes de su lado (N, N+2, N+4): esos renglones
/// quedan marcados con <see cref="ContinuacionDe"/> y no capturan nada, porque repetir la carga
/// haría que sumar la columna la contara dos o tres veces.
/// </para>
/// </summary>
public sealed class CircuitoDelCuadro
{
    public CircuitoDelCuadro(int espacio) => Espacio = espacio;

    /// <summary>El número de circuito, que es el número de espacio en la barra. Nones a la izquierda, pares a la derecha.</summary>
    public int Espacio { get; }

    public string Descripcion { get; set; } = string.Empty;
    public TipoCarga Tipo { get; set; } = TipoCarga.Alumbrado;

    /// <summary>
    /// En qué unidad viene lo que se capturó: VA, W o A, como lo diga la placa. <b>VA por omisión</b>,
    /// que es lo único que la pantalla aceptaba antes — lo ya capturado no cambia.
    /// </summary>
    public UnidadConsumo Unidad { get; set; } = UnidadConsumo.VoltAmperes;

    /// <summary>La carga continua <b>tal como viene en la placa</b>, en <see cref="Unidad"/>.</summary>
    public decimal Continua { get; set; }

    /// <summary>La carga no continua tal como viene en la placa, en <see cref="Unidad"/>.</summary>
    public decimal NoContinua { get; set; }

    /// <summary>
    /// La carga continua ya en volt-amperes, que es con lo que calcula el motor. La convierte
    /// <see cref="CuadroDeCarga"/> con <c>ConsumoDePlaca.AVoltAmperes</c> —la misma regla del
    /// escritorio (<c>CircuitoDerivado.VaUnitarioDe</c>)—, porque la conversión necesita la tensión
    /// y el factor de potencia, que son del tablero.
    /// </summary>
    public decimal ContinuaVA { get; internal set; }

    /// <summary>La carga no continua en volt-amperes. Ver <see cref="ContinuaVA"/>.</summary>
    public decimal NoContinuaVA { get; internal set; }
    public decimal LongitudM { get; set; } = 20m;

    /// <summary>Polos del interruptor. Se cambia por <see cref="CuadroDeCarga.CambiarPolos"/>, que verifica que quepa.</summary>
    public int Polos { get; internal set; } = 1;

    /// <summary>
    /// El espacio del interruptor multipolar que se comió este renglón, o <c>null</c> si el renglón
    /// es suyo. Lo mantiene <see cref="CuadroDeCarga"/>.
    /// </summary>
    public int? ContinuacionDe { get; internal set; }

    /// <summary>Las barras que toca, en el orden en que las toca: «A», «AB», «ABC». La resuelve la geometría del tablero, no se captura.</summary>
    public string Fases { get; internal set; } = "A";

    public ResultadoCircuitoDerivado? Resultado { get; internal set; }

    /// <summary>Lo que el motor rechazó, ya redactado. <c>null</c> = el renglón calculó.</summary>
    public string? Error { get; internal set; }

    public bool EsContinuacion => ContinuacionDe is not null;

    public decimal CargaInstaladaVA => ContinuaVA + NoContinuaVA;

    public bool TieneCarga => !EsContinuacion && CargaInstaladaVA > 0m;

    /// <summary>
    /// Lo que este circuito le carga a cada una de sus barras, en VA — la banda «BALANCEO DE FASES»
    /// del Excel (columnas BB, BC, BD): la carga entre el número de fases que toca.
    /// </summary>
    public decimal CargaPorFaseVA => Fases.Length == 0 ? 0m : CargaInstaladaVA / Fases.Length;

    internal void Limpiar()
    {
        Resultado = null;
        Error = null;
    }
}
