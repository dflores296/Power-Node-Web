using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// En qué unidad viene declarado el consumo de un renglón de carga.
///
/// <para>
/// Existe desde el 2026-08-21, con <see cref="TipoCarga.Equipo"/>: la placa de un aparato
/// —refrigerador, minisplit, cocina eléctrica— casi nunca dice volt-amperes. Dice <b>watts</b> o dice
/// <b>amperes</b>. Obligar a convertirlos a mano antes de capturarlos es pedirle al proyectista que
/// haga a lápiz justo la cuenta que este programa existe para hacer, y con el riesgo de que la haga
/// con la tensión equivocada.
/// </para>
/// </summary>
public enum UnidadConsumo
{
    /// <summary>Volt-amperes. Es lo único que el programa aceptaba antes, y sigue siendo el caso por omisión.</summary>
    VoltAmperes,

    /// <summary>Watts (potencia activa). Se convierte con el factor de potencia del circuito: VA = W / fp.</summary>
    Watts,

    /// <summary>Amperes de placa. Se convierte con la tensión del circuito y su número de fases.</summary>
    Amperes,
}

/// <summary>
/// Convierte un consumo declarado en la unidad que trae la placa a los volt-amperes con los que
/// trabaja el motor de cálculo.
///
/// <para>
/// <b>La propiedad que esto tiene que cumplir, y por la que comparte el divisor con la calculadora:</b>
/// capturar «8 A» en un renglón tiene que devolver una corriente de diseño de 8 A. Si esta conversión
/// usara una tensión distinta de la que <see cref="CalculadoraCircuitoDerivadoNoMotor"/> usa para
/// dividir, los amperes capturados y los calculados no coincidirían — y el proyectista no tendría
/// forma de saber cuál de los dos está mal. Por eso el divisor vive en
/// <see cref="TensionDeCalculo"/> y no hay una segunda copia de esa regla.
/// </para>
/// </summary>
public static class ConsumoDePlaca
{
    /// <summary>
    /// Los volt-amperes que representa <paramref name="valor"/> declarado en <paramref name="unidad"/>.
    ///
    /// <para>
    /// <b>Un factor de potencia no positivo no convierte, devuelve el valor tal cual.</b> Dividir
    /// entre cero o entre un negativo daría infinito o una carga negativa, y las dos cosas se verían
    /// como un dato capturado. Devolver los watts como si fueran VA se equivoca <b>del lado seguro</b>
    /// (subestima la carga aparente solo cuando el fp real sería &lt; 1) y es visible: el número que
    /// sale es el que se tecleó.
    /// </para>
    /// </summary>
    public static decimal AVoltAmperes(
        decimal valor,
        UnidadConsumo unidad,
        decimal tensionFaseNeutroV,
        decimal tensionFaseFaseV,
        int numeroFases,
        decimal factorPotencia) => unidad switch
        {
            UnidadConsumo.VoltAmperes => valor,

            // S = P / fp. La placa de un aparato publica watts (potencia activa); lo que dimensiona
            // el conductor y el interruptor es la aparente.
            UnidadConsumo.Watts => factorPotencia > 0 ? valor / factorPotencia : valor,

            // S = I × V (× √3 en trifásico). Es la misma cuenta de la calculadora, al revés.
            UnidadConsumo.Amperes =>
                valor * TensionDeCalculo.Divisor(numeroFases, tensionFaseNeutroV, tensionFaseFaseV),

            _ => valor,
        };
}

/// <summary>
/// La tensión contra la que se divide la carga de un circuito derivado para obtener su corriente.
///
/// <para>
/// Estaba escrita en dos renglones dentro de <see cref="CalculadoraCircuitoDerivadoNoMotor"/> y salió
/// de ahí el 2026-08-21, cuando <see cref="ConsumoDePlaca"/> necesitó hacer exactamente la misma
/// cuenta en sentido contrario. <b>Es una sola regla y tiene que vivir en un solo lugar</b>: dos
/// copias que se separen harían que unos amperes capturados no regresaran como los mismos amperes
/// calculados.
/// </para>
/// </summary>
public static class TensionDeCalculo
{
    /// <summary>
    /// Monofásico divide entre la tensión fase-neutro; bifásico, entre la fase-fase; trifásico, entre
    /// la fase-fase por √3.
    /// </summary>
    public static decimal Divisor(int numeroFases, decimal tensionFaseNeutroV, decimal tensionFaseFaseV)
    {
        var tensionEfectiva = Efectiva(numeroFases, tensionFaseNeutroV, tensionFaseFaseV);
        return numeroFases == 3 ? tensionEfectiva * (decimal)Math.Sqrt(3) : tensionEfectiva;
    }

    /// <summary>
    /// La tensión a la que está el circuito, <b>sin</b> el √3 del trifásico: fase-neutro en
    /// monofásico, fase-fase en los demás. Es la que se usa para la caída de tensión, donde el √3
    /// entra por otro lado de la fórmula.
    /// </summary>
    public static decimal Efectiva(int numeroFases, decimal tensionFaseNeutroV, decimal tensionFaseFaseV)
        => numeroFases == 1 ? tensionFaseNeutroV : tensionFaseFaseV;
}
