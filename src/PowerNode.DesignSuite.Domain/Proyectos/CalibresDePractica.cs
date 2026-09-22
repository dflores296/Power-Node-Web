namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Las áreas de la Tabla 8 que este despacho usa como <b>piso de práctica</b>, con nombre en vez de
/// número suelto.
///
/// <para>
/// <b>Por qué son constantes y no una consulta al catálogo:</b> son el valor por omisión de un campo
/// de <see cref="ConfiguracionProyecto"/>, y el inicializador de un campo no puede ir a la base. La
/// prueba <c>PisoPracticoPorOmisionTests</c> es la que amarra estos dos números contra la Tabla 8
/// sembrada de verdad, para que no puedan separarse de ella.
/// </para>
///
/// <para>
/// <b>Ojo con el sentido de la comparación, que es donde está el peligro.</b>
/// <c>SeleccionConductor</c> sube el calibre cuando <c>AreaMm2 &lt; piso</c>, así que el piso tiene
/// que quedar <b>en o por debajo</b> del área tabulada del calibre que se quiere permitir, y por
/// encima de la del escalón anterior. Redondear hacia arriba lo rompe en silencio: un 5.27 aquí
/// dejaría fuera al propio 10 AWG y treparía todo circuito de contactos a <b>8 AWG</b> — un resultado
/// perfectamente válido, solo que el equivocado, que ninguna prueba de cálculo denunciaría.
/// <b>Ante la duda, se redondea hacia abajo.</b>
/// </para>
/// </summary>
public static class CalibresDePractica
{
    /// <summary>12 AWG — 3.31 mm² en la Tabla 8, exacto. Piso de alumbrado.</summary>
    public const decimal DoceAwgMm2 = 3.31m;

    /// <summary>
    /// 10 AWG — piso de contactos.
    ///
    /// <para>
    /// <b>La Tabla 8 dice 5.261, no 5.26</b>, y aquí va el 5.26 a propósito por dos razones que
    /// apuntan al mismo lado: la columna que lo guarda es <c>decimal(8,2)</c> y redondearía el tercer
    /// decimal de todos modos, y quedarse por debajo del área tabulada es justo lo que hace que el
    /// 10 AWG pase. Lo encontró <c>PisoPracticoPorOmisionTests</c>.
    /// </para>
    /// </summary>
    public const decimal DiezAwgMm2 = 5.26m;
}
