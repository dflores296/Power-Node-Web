using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// <b>El 125 % de la carga continua, y la excepción que lo baja a 100 %.</b>
///
/// <para>
/// La regla general vive en cuatro secciones que dicen lo mismo por pares — <b>210-19(a)(1)</b> y
/// <b>210-20(a)</b> para el conductor y la protección de un derivado, <b>215-2(a)(1)</b> y
/// <b>215-3</b> para los de un alimentador —: la capacidad no debe ser menor a la carga no continua
/// más el <b>125 %</b> de la continua. Ese 25 % extra es el margen por operar tres horas o más.
/// </para>
///
/// <para>
/// Las cuatro traen la <b>misma excepción</b>, y conviene leerla con cuidado porque no dice lo que
/// se suele resumir: <i>"Cuando <b>el ensamble, incluidos los dispositivos de sobrecorriente</b> que
/// están protegiendo el circuito derivado, esté aprobado para funcionamiento al 100 por ciento de su
/// valor nominal, se permitirá que el valor nominal en amperes del dispositivo de sobrecorriente no
/// sea menor que la suma de la carga continua más la carga no-continua."</i>
/// </para>
///
/// <para>
/// <b>Dice "el ensamble", no "el interruptor", y de ahí sale todo el diseño de este bloque.</b> Un
/// interruptor listado para 100 % montado en la envolvente equivocada <b>no</b> da un conjunto de
/// 100 %: el catálogo de Schneider trae gabinetes marcados <i>"Accepts standard 80% rated circuit
/// breakers only. Not rated for 100% rated circuit breakers"</i>. Eso el programa no lo puede
/// deducir de un número de parte, así que la aprobación del conjunto es una <b>declaración del
/// proyectista</b> —igual que <c>Transformador.ProteccionTermicaCoordinadaFabrica</c> para la nota 3
/// del 450-3(b)— y el catálogo sirve para <b>contradecirla cuando hay evidencia</b>, no para
/// concederla.
/// </para>
///
/// <para>
/// <b>Por qué esto NO sigue la regla del 240-86</b>, que también relaja un criterio y sin embargo
/// exige evidencia positiva: allá lo que se relaja es un <i>veredicto de seguridad</i> (la capacidad
/// interruptiva contra la corriente de falla) y la única evidencia posible es la tabla de
/// combinaciones probadas — al usuario no se le pregunta nada. Aquí el criterio de la norma <b>es</b>
/// una declaración de campo: el ensamble está aprobado o no, y eso se lee en su marcado. Exigir
/// además un modelo de catálogo dejaría la excepción inalcanzable en alimentadores y en el principal
/// de un tablero, que hoy no guardan modelo de interruptor — un campo muerto, que es el error que
/// este proyecto ya cometió otras veces.
/// </para>
///
/// <para>
/// <b>La única condición que sí se verifica contra el catálogo es la contraria:</b> si el modelo
/// elegido está sembrado y es de <b>80 %</b>, se mantiene el 125 % aunque el conjunto se haya
/// declarado — ahí sí hay evidencia positiva de que el aparato no es el que la excepción supone.
/// </para>
/// </summary>
public static class CargaContinua100Pct
{
    /// <summary>El 125 % de siempre. Es lo que aplica salvo que algo diga lo contrario.</summary>
    public const decimal FactorEstandar = 1.25m;

    /// <summary>Con la excepción concedida, la carga continua entra al 100 %.</summary>
    public const decimal FactorAlCienPorCiento = 1m;

    /// <param name="Factor">1.25 o 1.00. Es lo que multiplica a la corriente continua.</param>
    /// <param name="Avisos">Van a las advertencias del elemento, no a las citas: no sustentan el
    /// número, dicen qué hay que revisar en campo.</param>
    public sealed record Resultado(decimal Factor, IReadOnlyList<Cita> Citas, IReadOnlyList<string> Avisos)
    {
        public bool AlCienPorCiento => Factor == FactorAlCienPorCiento;
    }

    /// <param name="conjuntoAprobado100Pct">Lo declara el proyectista. <b>Falso por omisión</b>: sin
    /// declaración se aplica el 125 %, que es la regla general.</param>
    /// <param name="modeloElegidoEsDe100Pct"><c>null</c> = no hay modelo elegido o no está en el
    /// catálogo. <b>Null no contradice la declaración</b> (ver el resumen de la clase); <c>false</c>
    /// sí.</param>
    /// <param name="articuloProteccion">"210-20(a)" en un derivado, "215-3" en un alimentador.</param>
    /// <param name="articuloConductor">"210-19(a)(1)" en un derivado, "215-2(a)(1)" en un alimentador.</param>
    public static Resultado Para(
        bool conjuntoAprobado100Pct,
        bool? modeloElegidoEsDe100Pct,
        string articuloProteccion,
        string articuloConductor)
    {
        if (!conjuntoAprobado100Pct)
            return new Resultado(FactorEstandar, [], []);

        if (modeloElegidoEsDe100Pct == false)
            return new Resultado(FactorEstandar, [],
            [
                $"Se declaró que el conjunto está aprobado para operar al 100 % de su valor nominal, pero el interruptor " +
                $"elegido del catálogo es de rating estándar (80 %). Se mantiene el 125 % de {articuloProteccion}: la " +
                $"excepción exige que el ensamble COMPLETO —envolvente y dispositivo de sobrecorriente— esté aprobado, y " +
                $"este dispositivo no lo está. Elige un modelo listado para 100 % o quita la declaración.",
            ]);

        var citas = new List<Cita>
        {
            new(articuloProteccion,
                "Excepción: el ensamble, incluidos sus dispositivos de sobrecorriente, se declara aprobado para " +
                "funcionamiento al 100 % de su valor nominal, así que la capacidad de la protección se calcula con la " +
                "suma de la carga continua más la no continua, sin el 125 %."),
            new(articuloConductor,
                "Excepción: con el ensamble aprobado para operación al 100 %, la ampacidad del conductor tampoco tiene " +
                "que ser menor a la suma de las cargas continuas más las no continuas."),
        };

        var avisos = new List<string>
        {
            // Requisito del FABRICANTE, no de la NOM-001 -- se cita como tal, igual que el derrateo
            // por altitud del transformador, que sale de la NMX-J-116 y no de la norma.
            "Con un interruptor de 100 % el fabricante exige conductor de 90 °C como mínimo, dimensionado con la " +
            "ampacidad de la columna de 75 °C (Schneider, Digest 0100CT2401, pág. 7-90). No es requisito de la " +
            "NOM-001: verifícalo contra la ficha del equipo instalado.",
        };

        if (modeloElegidoEsDe100Pct is null)
            avisos.Add(
                "La aprobación del conjunto al 100 % es declaración del proyectista y no se pudo corroborar contra el " +
                "catálogo, porque no hay un modelo de interruptor elegido. Queda bajo su responsabilidad: la norma pide " +
                "que el ENSAMBLE esté aprobado, no solo el interruptor.");

        return new Resultado(FactorAlCienPorCiento, citas, avisos);
    }

    /// <summary>
    /// El aviso del aislamiento, aparte porque se emite donde la temperatura ya se conoce — que es
    /// después de elegir la protección, no antes.
    ///
    /// <b>Es aviso y no bloqueo</b>: la exigencia de los 90 °C es del fabricante, no de la norma, y
    /// este programa no inventa requisitos normativos. Pero con el aislamiento capturado el programa
    /// SÍ puede ver que no se cumple, y callarse eso sería peor.
    /// </summary>
    public static string? AvisoDeAislamiento(bool alCienPorCiento, TemperaturaAislamiento aislamiento) =>
        alCienPorCiento && aislamiento < TemperaturaAislamiento.T90
            ? $"El aislamiento capturado es de {(int)aislamiento}°C y un interruptor de 100 % exige 90 °C como " +
              $"mínimo (requisito del fabricante, no de la NOM-001). O se cambia el aislamiento, o el conjunto no puede " +
              $"operar al 100 % y hay que volver al 125 %."
            : null;
}
