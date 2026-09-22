using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Derivaciones;

/// <summary>Cómo quedó una de las cinco reglas de 240-21(b) frente a esta derivación.</summary>
/// <param name="Regla">Cuál de los cinco casos se evaluó.</param>
/// <param name="Aplica">Si el caso es siquiera pertinente. Un caso que no aplica —(b)(3) sin
/// transformador, (b)(5) en interior— no se lista como incumplido: no venía al caso.</param>
/// <param name="Condiciones">Cada condición de la regla, con su cláusula y en qué quedó.</param>
public sealed record ReglaEvaluada(ReglaDerivacion Regla, bool Aplica, IReadOnlyList<CondicionDerivacion> Condiciones)
{
    /// <summary>La regla se cumple: aplica y <b>todas</b> sus condiciones están verificadas y en orden.</summary>
    public bool Cumple => Aplica && Condiciones.All(c => c.Estado == EstadoCondicion.Cumple);

    /// <summary>Las condiciones que se revisaron y no pasaron.</summary>
    public IReadOnlyList<CondicionDerivacion> Incumplidas =>
        [.. Condiciones.Where(c => c.Estado == EstadoCondicion.NoCumple)];

    /// <summary>Las condiciones que no se pudieron revisar porque falta capturar el dato.</summary>
    public IReadOnlyList<CondicionDerivacion> Faltantes =>
        [.. Condiciones.Where(c => c.Estado == EstadoCondicion.NoVerificable)];

    /// <summary>
    /// La <b>premisa</b> del caso —su primera condición— se cumple: la longitud cabe en el límite del
    /// inciso, o los conductores están donde el inciso exige. Es lo que separa un caso que todavía
    /// se puede alcanzar de uno que ya no: a una derivación de 5 m se le puede engordar el conductor
    /// para entrar a (b)(2), pero no se le puede quitar longitud para entrar a (b)(1).
    /// </summary>
    public bool PremisaCumple => Condiciones.Count > 0 && Condiciones[0].Estado == EstadoCondicion.Cumple;
}

/// <summary>El veredicto completo de una derivación contra 240-21(b).</summary>
/// <param name="ReglaQueCumple">El primer caso que se cumple por completo, o <c>null</c> si ninguno.
/// Cuando es <c>null</c>, la derivación <b>necesita su propia protección</b> en el punto de conexión
/// (240-21, encabezado) y deja de ser derivación.</param>
/// <param name="Reglas">Las cinco, evaluadas. Sirve para decir <i>por qué</i> no pasó, que es lo que
/// el ingeniero necesita para corregir.</param>
/// <param name="AmpacidadMinimaExigidaA">La ampacidad mínima que la norma le exige a este conductor
/// de derivación, ya tomando el mayor de los criterios aplicables.</param>
/// <param name="Citas">El rastro para la memoria de cálculo.</param>
public sealed record VeredictoDerivacion(
    ReglaDerivacion? ReglaQueCumple,
    IReadOnlyList<ReglaEvaluada> Reglas,
    decimal AmpacidadMinimaExigidaA,
    IReadOnlyList<Cita> Citas)
{
    /// <summary>La derivación está permitida sin protección propia.</summary>
    public bool Permitida => ReglaQueCumple is not null;

    /// <summary>
    /// La regla que <b>estuvo más cerca</b>: la que aplica y tiene menos condiciones abiertas. Es lo
    /// que se le enseña al usuario cuando ninguna pasa — «te faltó esto para el caso de 8 m» sirve;
    /// «no cumple 240-21(b)» no.
    ///
    /// <para>
    /// <b>Con empate gana el caso cuya premisa sí se cumple</b>, y no es un detalle de presentación:
    /// una derivación de 5 m con el conductor corto queda a una condición de (b)(1) y a una de
    /// (b)(2), pero solo la de (b)(2) tiene arreglo — al conductor se le puede subir el calibre, a la
    /// derivación no se le pueden quitar dos metros. Proponer (b)(1) sería mandar al usuario a
    /// corregir lo que no se puede corregir.
    /// </para>
    /// </summary>
    public ReglaEvaluada? MasCercana => Reglas
        .Where(r => r.Aplica)
        .OrderBy(r => r.Incumplidas.Count + r.Faltantes.Count)
        .ThenBy(r => r.PremisaCumple ? 0 : 1)
        .FirstOrDefault();
}

/// <summary>
/// <b>Las cinco reglas de 240-21(b), verificadas contra el texto real de la norma</b> — no de
/// memoria: el texto salió de <c>corpus.json</c>, que está cotejado línea por línea contra el DOF.
///
/// <para>
/// <b>Qué NO hace este verificador.</b> No dimensiona el conductor: eso sigue siendo
/// <see cref="SeleccionConductor"/>, igual que en cualquier otro tramo. Lo que aporta es
/// <see cref="VeredictoDerivacion.AmpacidadMinimaExigidaA"/> — el piso que la norma le impone a esa
/// selección — y el veredicto de si la derivación puede ir <b>sin protección propia</b>.
/// </para>
///
/// <para>
/// <b>Y lo que tiene prohibido.</b> El encabezado de 240-21(b) dice que 240-4(b) no se permite en
/// conductores de derivación: aquí <b>no</b> se redondea al siguiente tamaño estándar. El bloque 8.6
/// enseñó a todo el motor a dimensionar contra la protección; este es el único lugar del programa
/// donde esa regla se apaga, y por eso está dicho en el enum, en el dominio y aquí.
/// </para>
/// </summary>
public static class VerificadorDerivacion
{
    /// <summary>240-21(b)(1): tres metros.</summary>
    public const decimal LimiteTresMetrosM = 3.00m;

    /// <summary>240-21(b)(2) y (b)(3)(3): ocho metros.</summary>
    public const decimal LimiteOchoMetrosM = 8.00m;

    /// <summary>240-21(b)(4): la pared de la nave, más de once metros.</summary>
    public const decimal AlturaMinimaParedNaveM = 11.00m;

    /// <summary>240-21(b)(4)(2): treinta metros de longitud total.</summary>
    public const decimal LimiteTotalNaveM = 30.00m;

    /// <summary>240-21(b)(4)(9): la derivación, a nueve metros del piso o más.</summary>
    public const decimal AlturaMinimaDerivacionNaveM = 9.00m;

    /// <summary>240-21(b)(4)(7): 13.3 mm² (6 AWG) en cobre.</summary>
    public const decimal AreaMinimaNaveCobreMm2 = 13.3m;

    /// <summary>240-21(b)(4)(7): 21.2 mm² (4 AWG) en aluminio.</summary>
    public const decimal AreaMinimaNaveAluminioMm2 = 21.2m;

    /// <summary>
    /// Revisa la derivación contra los cinco casos y devuelve el primero que se cumple completo.
    ///
    /// <para>
    /// <b>El orden no es arbitrario</b>: va de la condición más restrictiva en longitud a la más
    /// laxa, que es como se lee el inciso. Con eso, una derivación de 2 m que cumple (b)(1) se
    /// reporta como (b)(1) aunque también cupiera en (b)(2) — y eso importa, porque (b)(1) no le
    /// exige el tercio de la protección y (b)(2) sí.
    /// </para>
    /// </summary>
    public static VeredictoDerivacion Verificar(DatosDerivacion d)
    {
        List<ReglaEvaluada> reglas =
        [
            TresMetros(d),
            OchoMetros(d),
            Transformador(d),
            NaveIndustrial(d),
            Exterior(d),
        ];

        var cumple = reglas.FirstOrDefault(r => r.Cumple);
        var minima = AmpacidadMinimaExigidaA(d, cumple?.Regla);

        return new VeredictoDerivacion(cumple?.Regla, reglas, minima, Citas(cumple?.Regla));
    }

    /// <summary>
    /// La ampacidad mínima que la norma le exige al conductor de derivación. Cuando ya se sabe bajo
    /// qué caso va, es el piso de ese caso; cuando todavía no, se toma <b>el más exigente de los que
    /// podrían aplicarle</b>, que es lo prudente para dimensionar antes de saber.
    /// </summary>
    public static decimal AmpacidadMinimaExigidaA(DatosDerivacion d, ReglaDerivacion? regla)
    {
        // 240-21(b)(1)(1)a: nunca por debajo de la carga calculada combinada. Vale para los cinco.
        var minima = d.CargaCalculadaA;

        switch (regla)
        {
            case ReglaDerivacion.TresMetros:
                // (b)(1)(1)b: y no menor que el dispositivo alimentado o el de la terminación.
                minima = Math.Max(minima, d.ProteccionEnTerminacionA ?? 0m);
                // (b)(1)(4): si sale de la envolvente, piso de 1/10.
                if (d.SaleDeLaEnvolvente) minima = Math.Max(minima, d.DecimoDeLaProteccionA);
                break;

            case ReglaDerivacion.OchoMetros:
            case ReglaDerivacion.Transformador:
            case ReglaDerivacion.NaveIndustrial:
                // (b)(2)(1), (b)(3)(1), (b)(4)(3): un tercio de la protección del alimentador.
                minima = Math.Max(minima, d.TercioDeLaProteccionA);
                break;

            case ReglaDerivacion.Exterior:
                // (b)(5) no pone fracción: el piso es la carga, y el dispositivo donde termina es
                // el que la limita a la ampacidad del conductor — (b)(5)(2).
                break;

            case null:
                // Todavía no se sabe bajo qué caso va. Se toma el criterio más exigente de los que
                // podrían aplicarle, para no dimensionar corto y tener que rehacerlo.
                minima = Math.Max(minima, d.ProteccionEnTerminacionA ?? 0m);
                if (d.LongitudM > LimiteTresMetrosM) minima = Math.Max(minima, d.TercioDeLaProteccionA);
                else if (d.SaleDeLaEnvolvente) minima = Math.Max(minima, d.DecimoDeLaProteccionA);
                break;
        }

        return minima;
    }

    // ---------------------------------------------------------------------------------
    // Las cinco reglas. Cada condición lleva su cláusula porque es lo que se imprime en la
    // memoria: «cumple 240-21(b)(2)(1)» es verificable por quien revisa el proyecto;
    // «cumple la regla de los 8 metros» no lo es.
    // ---------------------------------------------------------------------------------

    private static ReglaEvaluada TresMetros(DatosDerivacion d)
    {
        List<CondicionDerivacion> condiciones =
        [
            new("240-21(b)(1)", $"La derivación mide {d.LongitudM:0.##} m y el caso admite hasta {LimiteTresMetrosM:0.00} m.",
                Estado(d.LongitudM <= LimiteTresMetrosM)),

            new("240-21(b)(1)(1)a.", $"Ampacidad {d.AmpacidadDerivacionA:0.##} A contra la carga calculada combinada {d.CargaCalculadaA:0.##} A.",
                Estado(d.AmpacidadDerivacionA >= d.CargaCalculadaA)),

            new("240-21(b)(1)(1)b.", d.ProteccionEnTerminacionA is { } terminacion
                    ? $"Ampacidad {d.AmpacidadDerivacionA:0.##} A contra el dispositivo donde termina, de {terminacion:0.##} A."
                    : "No se capturó el valor nominal del dispositivo alimentado por la derivación.",
                d.ProteccionEnTerminacionA is { } t ? Estado(d.AmpacidadDerivacionA >= t) : EstadoCondicion.NoVerificable),

            Declarada("240-21(b)(1)(2)", "Los conductores no se extienden más allá del tablero o desconectador que alimentan.",
                d.NoSeExtiendeMasAllaDelTablero),

            Declarada("240-21(b)(1)(3)", "Salvo en el punto de conexión, los conductores van en canalización hasta la envolvente que alimentan.",
                d.EnCanalizacionDesdeLaDerivacion),
        ];

        // (b)(1)(4) solo existe si los conductores salen de la envolvente donde se hizo la
        // derivación. Si no salen, no es una condición que se incumpla: no aplica.
        if (d.SaleDeLaEnvolvente)
            condiciones.Add(new("240-21(b)(1)(4)",
                $"Los conductores salen de la envolvente: ampacidad {d.AmpacidadDerivacionA:0.##} A contra "
                + $"1/10 de la protección del alimentador, {d.DecimoDeLaProteccionA:0.##} A.",
                Estado(d.AmpacidadDerivacionA >= d.DecimoDeLaProteccionA)));

        return new ReglaEvaluada(ReglaDerivacion.TresMetros, Aplica: !d.AlimentaTransformador, condiciones);
    }

    private static ReglaEvaluada OchoMetros(DatosDerivacion d)
    {
        List<CondicionDerivacion> condiciones =
        [
            new("240-21(b)(2)", $"La derivación mide {d.LongitudM:0.##} m y el caso admite hasta {LimiteOchoMetrosM:0.00} m.",
                Estado(d.LongitudM <= LimiteOchoMetrosM)),

            new("240-21(b)(2)(1)", $"Ampacidad {d.AmpacidadDerivacionA:0.##} A contra 1/3 de la protección del alimentador, {d.TercioDeLaProteccionA:0.##} A.",
                Estado(d.AmpacidadDerivacionA >= d.TercioDeLaProteccionA)),

            Declarada("240-21(b)(2)(2)", "Termina en un solo interruptor automático o un solo conjunto de fusibles que limita la carga a la ampacidad del conductor.",
                d.TerminaEnUnSoloDispositivo),

            Declarada("240-21(b)(2)(3)", "Los conductores están protegidos contra daño físico en canalización aprobada u otros medios aprobados.",
                d.ProtegidaContraDanoFisico),
        ];

        return new ReglaEvaluada(ReglaDerivacion.OchoMetros, Aplica: !d.AlimentaTransformador, condiciones);
    }

    private static ReglaEvaluada Transformador(DatosDerivacion d)
    {
        var longitudTotal = d.LongitudPrimarioMasSecundarioM;

        var ampacidadSecundariaExigida = d.RelacionTensionPrimarioSecundario is { } relacion
            ? relacion * d.TercioDeLaProteccionA
            : (decimal?)null;

        List<CondicionDerivacion> condiciones =
        [
            new("240-21(b)(3)(1)", $"El primario tiene {d.AmpacidadDerivacionA:0.##} A contra 1/3 de la protección del alimentador, {d.TercioDeLaProteccionA:0.##} A.",
                Estado(d.AmpacidadDerivacionA >= d.TercioDeLaProteccionA)),

            new("240-21(b)(3)(2)", ampacidadSecundariaExigida is { } exigida && d.AmpacidadSecundarioA is { } secundario
                    ? $"El secundario tiene {secundario:0.##} A contra la relación de tensiones por 1/3 de la protección, {exigida:0.##} A."
                    : "Falta la relación de tensiones o la ampacidad del secundario para revisar el mínimo del secundario.",
                ampacidadSecundariaExigida is { } e && d.AmpacidadSecundarioA is { } s
                    ? Estado(s >= e)
                    : EstadoCondicion.NoVerificable),

            new("240-21(b)(3)(3)", longitudTotal is { } total
                    ? $"Primario más secundario suman {total:0.##} m, y el caso admite hasta {LimiteOchoMetrosM:0.00} m."
                    : "No se capturó la suma de longitudes de primario y secundario.",
                longitudTotal is { } l ? Estado(l <= LimiteOchoMetrosM) : EstadoCondicion.NoVerificable),

            Declarada("240-21(b)(3)(4)", "Primario y secundario están protegidos contra daño físico en canalización aprobada u otros medios aprobados.",
                d.ProtegidaContraDanoFisico),

            Declarada("240-21(b)(3)(5)", "Los conductores del secundario terminan en un solo interruptor automático o conjunto de fusibles.",
                d.TerminaEnUnSoloDispositivo),
        ];

        return new ReglaEvaluada(ReglaDerivacion.Transformador, Aplica: d.AlimentaTransformador, condiciones);
    }

    private static ReglaEvaluada NaveIndustrial(DatosDerivacion d)
    {
        var areaMinima = d.Material == MaterialConductor.Cobre ? AreaMinimaNaveCobreMm2 : AreaMinimaNaveAluminioMm2;

        List<CondicionDerivacion> condiciones =
        [
            new("240-21(b)(4)", d.AlturaParedM is { } pared
                    ? $"Las paredes de la nave miden {pared:0.##} m, y el caso pide más de {AlturaMinimaParedNaveM:0.00} m."
                    : "No se capturó la altura de las paredes de la nave.",
                d.AlturaParedM is { } p ? Estado(p > AlturaMinimaParedNaveM) : EstadoCondicion.NoVerificable),

            Declarada("240-21(b)(4)(1)", "El mantenimiento y la supervisión aseguran que solo lo atiende personal calificado.",
                d.PersonalCalificado),

            new("240-21(b)(4)(2)", d.LongitudHorizontalM is { } horizontal
                    ? $"Longitud horizontal {horizontal:0.##} m (máximo {LimiteOchoMetrosM:0.00} m) y total {d.LongitudM:0.##} m (máximo {LimiteTotalNaveM:0.00} m)."
                    : $"Longitud total {d.LongitudM:0.##} m; falta la longitud horizontal para revisar el máximo de {LimiteOchoMetrosM:0.00} m.",
                d.LongitudHorizontalM is { } h
                    ? Estado(h <= LimiteOchoMetrosM && d.LongitudM <= LimiteTotalNaveM)
                    : EstadoCondicion.NoVerificable),

            new("240-21(b)(4)(3)", $"Ampacidad {d.AmpacidadDerivacionA:0.##} A contra 1/3 de la protección del alimentador, {d.TercioDeLaProteccionA:0.##} A.",
                Estado(d.AmpacidadDerivacionA >= d.TercioDeLaProteccionA)),

            Declarada("240-21(b)(4)(4)", "Termina en un solo interruptor automático o un solo conjunto de fusibles que limita la carga a la ampacidad del conductor.",
                d.TerminaEnUnSoloDispositivo),

            Declarada("240-21(b)(4)(5)", "Los conductores están protegidos contra daño físico en canalización aprobada u otros medios aprobados.",
                d.ProtegidaContraDanoFisico),

            Declarada("240-21(b)(4)(6)", "Los conductores son continuos de un extremo a otro, sin empalmes.", d.SinEmpalmes),

            new("240-21(b)(4)(7)", d.AreaMm2 is { } area
                    ? $"El conductor es de {area:0.##} mm² y el mínimo en {Nombre(d.Material)} es {areaMinima:0.##} mm²."
                    : "No se capturó el área del conductor de derivación.",
                d.AreaMm2 is { } a ? Estado(a >= areaMinima) : EstadoCondicion.NoVerificable),

            Declarada("240-21(b)(4)(8)", "Los conductores no atraviesan paredes, pisos ni techos.", d.NoAtraviesaMurosNiPisosNiTechos),

            new("240-21(b)(4)(9)", d.AlturaDeLaDerivacionM is { } altura
                    ? $"La derivación está a {altura:0.##} m del piso, y el caso pide {AlturaMinimaDerivacionNaveM:0.00} m o más."
                    : "No se capturó a qué altura del piso se hizo la derivación.",
                d.AlturaDeLaDerivacionM is { } al ? Estado(al >= AlturaMinimaDerivacionNaveM) : EstadoCondicion.NoVerificable),
        ];

        return new ReglaEvaluada(ReglaDerivacion.NaveIndustrial, Aplica: !d.AlimentaTransformador, condiciones);
    }

    private static ReglaEvaluada Exterior(DatosDerivacion d)
    {
        List<CondicionDerivacion> condiciones =
        [
            new("240-21(b)(5)", "Los conductores están en el exterior del edificio o estructura, salvo en el punto de terminación de la carga.",
                Estado(d.EnExterior)),

            new("240-21(b)(5)(1)", $"Ampacidad {d.AmpacidadDerivacionA:0.##} A contra la carga calculada {d.CargaCalculadaA:0.##} A, y protección contra daño físico de manera aprobada.",
                d.ProtegidaContraDanoFisico is { } protegida
                    ? Estado(protegida && d.AmpacidadDerivacionA >= d.CargaCalculadaA)
                    : EstadoCondicion.NoVerificable),

            Declarada("240-21(b)(5)(2)", "Termina en un solo interruptor automático o un solo conjunto de fusibles que limita la carga a la ampacidad del conductor.",
                d.TerminaEnUnSoloDispositivo),

            Declarada("240-21(b)(5)(3)", "La protección es parte integral del medio de desconexión o está inmediatamente adyacente a él.",
                d.DesconectadorIntegradoOAdyacente),

            Declarada("240-21(b)(5)(4)", "El medio de desconexión está en un lugar fácilmente accesible.",
                d.DesconectadorFacilmenteAccesible),
        ];

        return new ReglaEvaluada(ReglaDerivacion.Exterior, Aplica: d.EnExterior, condiciones);
    }

    private static EstadoCondicion Estado(bool cumple) => cumple ? EstadoCondicion.Cumple : EstadoCondicion.NoCumple;

    /// <summary>
    /// Una condición que <b>solo el diseñador sabe</b>: el motor no puede deducir si un conductor va
    /// en canalización. Sin captura queda <see cref="EstadoCondicion.NoVerificable"/>, nunca en
    /// falso — declarar incumplida una condición que nadie revisó sería inventar un hallazgo.
    /// </summary>
    private static CondicionDerivacion Declarada(string clausula, string texto, bool? declarado) =>
        new(clausula, texto, declarado switch
        {
            true => EstadoCondicion.Cumple,
            false => EstadoCondicion.NoCumple,
            null => EstadoCondicion.NoVerificable,
        });

    private static string Nombre(MaterialConductor material) =>
        material == MaterialConductor.Cobre ? "cobre" : "aluminio";

    private static IReadOnlyList<Cita> Citas(ReglaDerivacion? regla)
    {
        List<Cita> citas =
        [
            new("NOM-001-SEDE-2012 240-21(b)",
                "Se permite derivar conductores de un alimentador sin protección contra sobrecorriente en la derivación, "
                + "bajo alguno de los cinco casos de (1) a (5). Las disposiciones de 240-4(b) no aplican a estos conductores."),
        ];

        if (regla is { } r)
            citas.Add(new($"NOM-001-SEDE-2012 {Clausula(r)}", $"La derivación se acoge a este caso: {Titulo(r)}."));

        return citas;
    }

    /// <summary>La cláusula de la norma que corresponde a cada caso.</summary>
    public static string Clausula(ReglaDerivacion regla) => regla switch
    {
        ReglaDerivacion.TresMetros => "240-21(b)(1)",
        ReglaDerivacion.OchoMetros => "240-21(b)(2)",
        ReglaDerivacion.Transformador => "240-21(b)(3)",
        ReglaDerivacion.NaveIndustrial => "240-21(b)(4)",
        ReglaDerivacion.Exterior => "240-21(b)(5)",
        _ => throw new ArgumentOutOfRangeException(nameof(regla)),
    };

    /// <summary>Cómo se nombra cada caso en pantalla y en la memoria.</summary>
    public static string Titulo(ReglaDerivacion regla) => regla switch
    {
        ReglaDerivacion.TresMetros => "derivación no mayor a 3.00 m",
        ReglaDerivacion.OchoMetros => "derivación no mayor a 8.00 m",
        ReglaDerivacion.Transformador => "derivación que alimenta un transformador, primario más secundario no mayor a 8.00 m",
        ReglaDerivacion.NaveIndustrial => "derivación de más de 8.00 m en nave industrial de gran altura",
        ReglaDerivacion.Exterior => "conductores localizados en el exterior del edificio o estructura",
        _ => throw new ArgumentOutOfRangeException(nameof(regla)),
    };
}
