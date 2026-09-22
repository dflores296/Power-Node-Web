namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Estado de "varios motores en un alimentador": lo que hace falta saber del grupo para aplicarle
/// tanto el <b>piso</b> de 430-24 como el <b>techo</b> de 430-62(a). Son dos campeones distintos, y
/// por eso se guardan por separado:
///
/// <list type="bullet">
/// <item><b>430-24</b> (piso, ampacidad) pide el <b>FLC mayor</b> (se le aplica 125%) y la suma de
/// los demás al 100%.</item>
/// <item><b>430-62(a)</b> (techo, protección) pide la <b>protección de derivado mayor</b> del grupo
/// más la suma de los FLC de <b>los demás motores</b> -- "los demás" respecto de ese motor, que no
/// tiene por qué ser el del FLC mayor: un motor chico con fusible sin retardo (300%) puede tener una
/// protección mayor que un motor grande con interruptor de tiempo inverso (250%).</item>
/// </list>
///
/// Se representa así -- en vez de una lista completa de motores -- porque es exactamente lo que
/// necesitan las dos fórmulas, y porque se puede fusionar entre ramas del árbol de tableros sin
/// perder ninguna de las dos distinciones: <see cref="Combinar"/> recalcula los dos campeones al
/// juntar dos ramas (si el mayor de una rama deja de ser el mayor global, se mueve a la suma del
/// resto).
/// </summary>
/// <param name="MayorProteccionDerivadoA">La mayor protección de circuito derivado del grupo
/// (430-52 / 440-22(a)). <c>null</c> mientras nadie la haya aportado -- ver
/// <see cref="TechoProteccion430_62aA"/>.</param>
/// <param name="FlcDelMayorProteccionA">El FLC del motor <b>dueño</b> de esa protección, que es el
/// que hay que descontar del total para obtener "la suma de los demás" de 430-62(a).</param>
public readonly record struct AgregadoMotores(
    decimal? MayorFlcA,
    decimal SumaRestoFlcA,
    decimal? MayorProteccionDerivadoA = null,
    decimal FlcDelMayorProteccionA = 0m)
{
    public static readonly AgregadoMotores Vacio = new(null, 0m);

    /// <summary>
    /// Un motor del que solo se conoce el FLC. Sirve para el piso de 430-24, pero <b>no</b> aporta
    /// techo: sin la protección de su derivado, 430-62(a) no se puede calcular, y eso se propaga
    /// como <c>null</c> en vez de inventarse un número.
    /// </summary>
    public static AgregadoMotores DeUnMotor(decimal flcA) => new(flcA, 0m);

    /// <summary>Un motor con su protección de derivado ya calculada (430-52 / 440-22(a)).</summary>
    public static AgregadoMotores DeUnMotor(decimal flcA, decimal proteccionDerivadoA) =>
        new(flcA, 0m, proteccionDerivadoA, flcA);

    public static AgregadoMotores Combinar(AgregadoMotores a, AgregadoMotores b)
    {
        // El campeón de PROTECCIÓN se resuelve aparte del de FLC, porque no tienen por qué ser el
        // mismo motor. El ">=" implementa de paso la regla de empates de 430-62(a): "cuando en dos o
        // más de los circuitos derivados del grupo se utilice un dispositivo de protección [...] del
        // mismo valor nominal o ajuste, uno de los dispositivos de protección se debe considerar
        // como el de mayor corriente" -- o sea, en un empate solo cuenta uno, y el otro entra a la
        // suma de FLC como cualquier motor más.
        var (mayorProteccion, flcDeEsaProteccion) = (a.MayorProteccionDerivadoA, b.MayorProteccionDerivadoA) switch
        {
            (decimal pa, decimal pb) => pa >= pb
                ? (a.MayorProteccionDerivadoA, a.FlcDelMayorProteccionA)
                : (b.MayorProteccionDerivadoA, b.FlcDelMayorProteccionA),
            (not null, null) => (a.MayorProteccionDerivadoA, a.FlcDelMayorProteccionA),
            (null, not null) => (b.MayorProteccionDerivadoA, b.FlcDelMayorProteccionA),
            _ => (null, 0m),
        };

        if (a.MayorFlcA is not decimal mayorA)
            return b with { MayorProteccionDerivadoA = mayorProteccion, FlcDelMayorProteccionA = flcDeEsaProteccion };
        if (b.MayorFlcA is not decimal mayorB)
            return a with { MayorProteccionDerivadoA = mayorProteccion, FlcDelMayorProteccionA = flcDeEsaProteccion };

        return mayorA >= mayorB
            ? new AgregadoMotores(mayorA, a.SumaRestoFlcA + b.SumaRestoFlcA + mayorB, mayorProteccion, flcDeEsaProteccion)
            : new AgregadoMotores(mayorB, a.SumaRestoFlcA + b.SumaRestoFlcA + mayorA, mayorProteccion, flcDeEsaProteccion);
    }

    /// <summary>430-24: 125% del mayor + 100% del resto -- lo que exige el bucket de motores para ampacidad/protección.</summary>
    public decimal CapacidadMinimaA => MayorFlcA is decimal mayor ? 1.25m * mayor + SumaRestoFlcA : 0m;

    /// <summary>Corriente real, sin el margen de arranque del mayor -- para caída de tensión (que refleja operación normal, no arranque).</summary>
    public decimal CorrienteRealA => (MayorFlcA ?? 0m) + SumaRestoFlcA;

    /// <summary>La suma de TODOS los FLC del grupo, sin ningún margen.</summary>
    public decimal SumaFlcTotalA => CorrienteRealA;

    /// <summary>
    /// El techo de 430-62(a): <i>"un dispositivo de protección con un valor nominal o ajuste no
    /// mayor al mayor valor nominal o ajuste del dispositivo de protección [...] del circuito
    /// derivado para cualquier motor alimentado por el alimentador [...], más la suma de todas las
    /// corrientes de plena carga de los demás motores del grupo"</i>.
    ///
    /// <c>null</c> cuando no se conoce la protección de ningún derivado del grupo -- sin ese dato el
    /// techo no existe, y un techo inventado sería peor que no tenerlo.
    /// </summary>
    public decimal? TechoProteccion430_62aA => MayorProteccionDerivadoA is decimal mayorProteccion
        ? mayorProteccion + (SumaFlcTotalA - FlcDelMayorProteccionA)
        : null;
}
