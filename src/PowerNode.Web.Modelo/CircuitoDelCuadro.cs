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
    public decimal ContinuaVA { get; set; }
    public decimal NoContinuaVA { get; set; }
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
