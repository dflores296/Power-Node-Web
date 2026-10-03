using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>La protección contra sobrecarga que pide el equipo del circuito</b> — I-183 (CONFIRMADA · David ·
/// 2026-10-03). En todo circuito de motor que calcula la app la NOM la exige: 430-32 en un motor, 430-124(a)
/// en un variador, 440-52 en un motocompresor. Solo el motor portátil de 1 HP o menos a la vista
/// (430-32(d)(2)a.) y la bomba contra incendio (430-31) van sin ella, y la app no los calcula como motor. Por
/// eso no se pregunta: se dice. Con ella, el interruptor puede pasar la ampacidad del conductor (240-4(g)).
/// </summary>
/// <param name="Referencia">«430-32(a)», «430-32(b)», «430-33», «430-124(a), 430-126», «430-53» o «440-52».</param>
/// <param name="Texto">Quién la da y cómo, ya redactado: el desglose, la memoria y el título de «OL».</param>
/// <param name="Aparte">
/// Va en otro dispositivo, no en el interruptor del tablero: el renglón lleva «OL». No, en un motor de
/// servicio no continuo, donde la puede dar el mismo interruptor (430-33).
/// </param>
public sealed record SobrecargaRequerida(string Referencia, string Texto, bool Aparte)
{
    /// <summary>
    /// La de un motor de servicio continuo — 430-32(a)(1): el relevador a no más de 125 % de la corriente de
    /// placa con factor de servicio de 1.15 o más o elevación de 40 °C o menos; 115 % en los demás.
    /// </summary>
    private const string DeUnMotor =
        "Requerida aparte del interruptor: relevador en el arrancador ajustado a no más de 125 % de la corriente de placa " +
        "(115 % si el factor de servicio es menor de 1.15 y la elevación de temperatura mayor de 40 °C), o motor marcado " +
        "«Protegido térmicamente»";

    /// <summary>
    /// La que pide el equipo del circuito; <c>null</c> si no es motor, variador ni equipo de A/C con rango
    /// (el acondicionador de habitación es un aparato con cordón — 440-62), o si no se calculó.
    /// </summary>
    public static SobrecargaRequerida? De(CircuitoDelCuadro c)
    {
        if (c.Resultado is null)
            return null;
        return c.EntradaDeLaProteccion switch
        {
            // Un motor solo, o un grupo de un solo motor, que se calcula como motor.
            DatosEntradaCircuitoDerivadoMotor { Servicio: not null } => new("430-33",
                "La puede dar este mismo interruptor, que no pasa la Tabla 430-52: el motor es de servicio no continuo. " +
                "O un relevador en el arrancador", false),
            DatosEntradaCircuitoDerivadoMotor m when m.Hp > CriteriosDeProteccion.CorteDelAutomaticoHp =>
                new("430-32(a)", DeUnMotor, true),
            DatosEntradaCircuitoDerivadoMotor => new("430-32(b)",
                DeUnMotor + ". En una bomba o un motor chico suele venir en el motor: verificarlo en la placa", true),
            DatosEntradaCircuitoDerivadoVariador => new("430-124(a), 430-126",
                "La da el variador si está marcado así; si no, un relevador aparte. Si el motor trabaja a baja velocidad sin " +
                "ventilación propia, además protección contra sobretemperatura (430-126)", true),
            DatosEntradaCircuitoDerivado440 => new("440-52",
                "De fábrica: la trae el equipo, en el protector del motocompresor o el relevador del equipo", true),
            _ when c.EsGrupo && c.Cargas.Any(a => a.EsMaquina) => DelGrupo(c),
            _ => null,
        };
    }

    /// <summary>Varios motores en un circuito: cada máquina con la suya — 430-53, 430-32, 440-52, 430-124.</summary>
    private static SobrecargaRequerida DelGrupo(CircuitoDelCuadro c)
    {
        var clases = c.Cargas.Where(a => a.EsMaquina).Select(a => a.Clase).ToHashSet();
        var partes = new List<string>();
        if (clases.Contains(ClaseDeAparato.Motor))
            partes.Add("cada motor, la de 430-32 (relevador en su arrancador o motor «Protegido térmicamente»)");
        if (clases.Contains(ClaseDeAparato.Variador))
            partes.Add("cada variador, la suya si está marcado así (430-124(a))");
        if (clases.Contains(ClaseDeAparato.Motocompresor))
            partes.Add("cada motocompresor, la de fábrica (440-52)");
        return new("430-53", "Requerida aparte del interruptor: " + string.Join("; ", partes), true);
    }

    /// <summary>«OL», junto a la clase del circuito, en el renglón y en el documento — I-183.</summary>
    public const string Etiqueta = "OL";

    /// <summary>El título de «OL»: qué es y quién la da.</summary>
    public string Titulo => $"Protección contra sobrecarga — {Referencia}: {Texto}.";
}
