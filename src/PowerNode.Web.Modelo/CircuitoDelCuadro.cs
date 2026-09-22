using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.DesignSuite.Domain.Proyectos;

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

    /// <summary>
    /// El piso de calibre por criterio de diseño, <b>no por norma</b> —la NOM permite 14 AWG en un
    /// derivado de 15 A, 210-19(a)(4)—. Son los mismos valores por omisión que trae
    /// <c>ConfiguracionProyecto</c> en la versión de escritorio: 12 AWG en alumbrado, 10 en
    /// contactos. Equipo nace sin piso a propósito: un aparato se dimensiona por su consumo de placa.
    /// </summary>
    public static decimal? PisoPracticoDe(TipoCarga tipo) => tipo switch
    {
        TipoCarga.Alumbrado => CalibresDePractica.DoceAwgMm2,
        TipoCarga.Contactos => CalibresDePractica.DiezAwgMm2,
        _ => null,
    };

    internal void Limpiar()
    {
        Resultado = null;
        Error = null;
    }
}
