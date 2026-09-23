using PowerNode.DesignSuite.Calculo.Magnitudes;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Bloque 8: elige el calibre de fase (por ampacidad, Tabla 310-15(b)(16)/(17), y por caída de
/// tensión, Tabla 9) y el número de conductores en paralelo por fase — el paso que comparten
/// <see cref="CalculadoraCircuitoDerivadoNoMotor"/>, <see cref="CalculadoraAlimentador"/> y
/// <see cref="CalculadoraCircuitoDerivadoMotor"/> (mismo pipeline en los tres, solo cambia de dónde sale la
/// corriente de diseño).
///
/// El usuario captura un N de conductores en paralelo (mínimo 1, ver <see cref="Unidades.Calibre.PermiteParalelo"/>
/// para el piso de 1/0 AWG de 310-10(h)(1)). Dos casos donde este paso se aparta de ese N capturado,
/// confirmados con el usuario en la sesión de bloque 8:
///
/// <b>Obligado</b>: si ni el calibre más grande del catálogo baja la caída de tensión al límite con
/// el N capturado, en vez de tronar de una vez se sube automáticamente N (2, 3, ...) hasta que
/// resuelva o se agote el tope de conductores en paralelo (<see cref="MaxNParaleloAutoResueltoPorOmision"/>,
/// configurable por proyecto) — un tope práctico de este motor, la
/// norma no fija un máximo. El resultado trae una cita explicando que se subió de N y que hay que
/// actualizar la captura del circuito/alimentador.
///
/// <b>Conviene</b>: si con 1 solo conductor por fase el calibre final ya es de 250 kcmil o más
/// (127 mm², Tabla 8) — en la práctica un conductor así de grueso pesa y dobla mal, y suele convenir
/// partirlo en 2+ conductores más chicos — se agrega una cita de sugerencia, puramente informativa;
/// no cambia el resultado.
///
/// <b>240-4 (protección de los conductores), agregado después de una sesión de preguntas del
/// usuario:</b> el calibre no se dimensiona solo para la corriente de diseño (capacidadMin) — se
/// dimensiona para que su ampacidad cubra la PROTECCIÓN elegida (240-6(a)), salvo que aplique la
/// excepción 240-4(b) (protección un escalón estándar arriba de la ampacidad del conductor, cuando
/// esa ampacidad no coincide con un valor estándar y la protección no excede 800 A). Además,
/// 240-4(d) pone un tope duro de protección para conductores de cobre 14/12/10 AWG (15/20/30 A)
/// que no se puede resolver subiendo N, solo subiendo de calibre. <c>Motor</c> (Art. 430) no pasa
/// por esta verificación — su protección se rige por 430-52, que por diseño puede exceder la
/// ampacidad del conductor (hay protección térmica de sobrecarga aparte); lo señala pasando
/// <c>proteccionA: null</c>.
///
/// <b>Crédito de aislamiento (110-14(c)(1)a.(2)/b.(2)), agregado después de otra sesión de
/// preguntas del usuario:</b> cuando el aislamiento capturado es de temperatura mayor que la que
/// exige la terminal (p.ej. THHN de 90°C en un circuito cuya terminal solo exige 75°C), la norma
/// permite calcular la ampacidad y aplicar los factores de corrección en la columna del AISLAMIENTO
/// (más favorable, más margen antes de que la corrección la tumbe) -- pero el resultado final no
/// puede exceder la ampacidad TABULADA (sin corregir) de la columna de la TERMINAL, para ese mismo
/// calibre. Es decir: `ampacidad utilizable = mínimo(ampacidad del aislamiento × factores, ampacidad
/// tabulada de la terminal)`. Cuando el aislamiento y la terminal piden la misma columna (el caso
/// típico sin corrección o con aislamiento igual a la terminal), esto da exactamente lo mismo que
/// antes -- la ampacidad de una columna más caliente siempre es mayor o igual a una más fría para el
/// mismo calibre, así que sin corrección activa el mínimo siempre cae en la columna de la terminal.
/// El crédito solo cambia el resultado cuando hay factores de corrección de por medio (temperatura
/// ambiente favorable, agrupamiento) que sí aprovechan el margen extra de la columna más caliente.
/// </summary>
public static class SeleccionConductor
{
    private const decimal UmbralConvieneParaleloMm2 = 127m; // 250 kcmil, Tabla 8.

    /// <summary>
    /// Hasta cuántos conductores por fase sube el motor por su cuenta antes de rendirse, cuando ni
    /// el calibre más grande del catálogo baja la caída de tensión al límite.
    ///
    /// <para>
    /// <b>Es un tope PRÁCTICO, no normativo: la norma no fija un máximo</b> (310-10(h) exige que los
    /// paralelos sean del mismo calibre, material, longitud y terminación, y ahí se acaba). Por eso
    /// dejó de ser constante privada y pasó a parámetro con este valor por omisión —auditoría
    /// 2026-08-19, §4.1 grupo C—: es criterio de diseño de quien proyecta, no del programa. Se captura
    /// en <c>ConfiguracionProyecto.MaxConductoresParaleloAutomatico</c>.
    /// </para>
    ///
    /// <para>
    /// <b>El umbral de "conviene partir en paralelo" (250 kcmil) se queda como constante</b>, y es a
    /// propósito: solo produce una <i>sugerencia</i> informativa y no cambia un calibre ni una
    /// protección. Configurar algo que no altera el resultado agrega una perilla sin consecuencia.
    /// </para>
    /// </summary>
    public const int MaxNParaleloAutoResueltoPorOmision = 6;

    /// <param name="CalibreFase">El que se instala: ya pasó ampacidad, 240-4, caída de tensión y piso práctico.</param>
    /// <param name="CalibreBase">El mínimo que exigían ampacidad y 240-4, <b>antes</b> de subir por caída de
    /// tensión o por el piso práctico. Se devuelve porque 250-122(b) obliga a crecer la tierra en la
    /// misma proporción en que creció la fase, y sin este dato no se sabe si creció.</param>
    /// <param name="AmpacidadUtilizableTotalA">La ampacidad real del juego instalado: la utilizable de
    /// <paramref name="CalibreFase"/> (con el crédito de aislamiento y los factores de corrección ya
    /// aplicados, topada a la columna de la terminal) multiplicada por el número de conductores en
    /// paralelo. Se devuelve porque 430-62(b) permite basar la protección del alimentador en la
    /// ampacidad del conductor cuando ésta supera lo que exige 430-24, y sin este dato ese permiso no
    /// se puede evaluar.</param>
    /// <param name="AmpacidadUtilizableTotalA">Del juego completo (por N conductores en paralelo).</param>
    /// <param name="ResistenciaOhmKm">R del conductor FINAL, Tabla 9. Va al resultado porque la
    /// memoria de cálculo tiene que <b>poder recalcularse</b>: quien la revisa toma el calibre, la R,
    /// la X y el factor de potencia, aplica la fórmula y debe llegar al mismo porcentaje. Si esos
    /// números solo viven dentro de una frase, el documento no se puede verificar.</param>
    /// <param name="CaidaTensionV">La caída en volts, que es de donde sale el porcentaje.</param>
    public sealed record Resultado(
        Calibre CalibreFase,
        Calibre CalibreBase,
        decimal CaidaTensionPct,
        int NumeroConductoresParalelo,
        IReadOnlyList<Cita> Citas,
        decimal AmpacidadUtilizableTotalA = 0m,
        decimal ResistenciaOhmKm = 0m,
        decimal ReactanciaOhmKm = 0m,
        decimal CaidaTensionV = 0m);

    public static Resultado Seleccionar(
        ICatalogoCalibres catalogo,
        ITablaAmpacidad ampacidad,
        ITablaImpedancia impedancia,
        decimal capacidadMinConductorA,
        decimal corrienteParaCaidaA,
        int numeroConductoresParaleloCapturado,
        decimal factorTemp,
        decimal factorAgrup,
        MaterialConductor materialConductor,
        MaterialCanalizacion materialCanalizacion,
        TemperaturaAislamiento tempAislamiento,
        TemperaturaAislamiento tempTerminales,
        decimal longitudM,
        decimal factorPotencia,
        int numeroFases,
        decimal tensionEfectivaV,
        decimal caidaTensionMaxPct,
        decimal? pisoPracticoCalibreMm2,
        ITablaProteccionEstandar? proteccionEstandar = null,
        decimal? proteccionA = null,
        bool permiteExcepcion2404b = false,
        MetodoInstalacion metodoInstalacion = MetodoInstalacion.CanalizacionOCable,
        int maxNParaleloAutoResuelto = MaxNParaleloAutoResueltoPorOmision,
        decimal? cargaAl100PctA = null)
    {
        // Un tope por debajo del N capturado dejaría el bucle sin una sola vuelta y tiraría una
        // excepción de caída de tensión donde el problema es el tope. El N capturado manda.
        maxNParaleloAutoResuelto = Math.Max(maxNParaleloAutoResuelto, Math.Max(1, numeroConductoresParaleloCapturado));

        var nParaleloMinimo = Math.Max(1, numeroConductoresParaleloCapturado);
        var senTheta = TrianguloPotencias.SenoDelAngulo(factorPotencia);
        var k = numeroFases == 3 ? (decimal)Math.Sqrt(3) : 2m;

        for (var nParalelo = nParaleloMinimo; nParalelo <= maxNParaleloAutoResuelto; nParalelo++)
        {
            var intento = IntentarConN(catalogo, ampacidad, impedancia, capacidadMinConductorA, corrienteParaCaidaA,
                nParalelo, factorTemp, factorAgrup, materialConductor, materialCanalizacion, tempAislamiento, tempTerminales,
                longitudM, factorPotencia, senTheta, k, tensionEfectivaV, caidaTensionMaxPct, pisoPracticoCalibreMm2,
                proteccionEstandar, proteccionA, permiteExcepcion2404b, metodoInstalacion, cargaAl100PctA);

            if (intento is null)
                continue; // catálogo agotado con este N -- prueba con más conductores en paralelo.

            var (calibreFinal, calibreBase, caidaPct, ampacidadTotalA, citas, _, _, _) = intento.Value;

            if (nParalelo > nParaleloMinimo)
                citas.Insert(0, new Cita("310-10(h)(1)",
                    $"Con {nParaleloMinimo} conductor(es) por fase, ni el calibre más grande del catálogo bajaba la caída de tensión a " +
                    $"{caidaTensionMaxPct}% -- se sube automáticamente a {nParalelo} conductores en paralelo por fase para resolver. " +
                    "Actualiza la captura del circuito/alimentador a este N."));
            else if (nParalelo == 1 && calibreFinal.AreaMm2 >= UmbralConvieneParaleloMm2)
                citas.Add(new Cita("310-10(h)(1)",
                    $"Sugerencia: {calibreFinal} es de 250 kcmil o mayor -- en la práctica suele convenir correrlo en 2+ conductores en " +
                    "paralelo por fase en vez de uno solo grueso. No es obligatorio; este resultado usa 1 conductor por fase."));

            return new Resultado(calibreFinal, calibreBase, caidaPct, nParalelo, citas, ampacidadTotalA,
                                 intento.Value.ROhmKm, intento.Value.XOhmKm, intento.Value.CaidaV);
        }

        throw new CaidaTensionExcedidaException(
            $"Ni con el calibre más grande del catálogo ni subiendo hasta {maxNParaleloAutoResuelto} conductores en paralelo por fase " +
            $"baja la caída de tensión a {caidaTensionMaxPct}%. Hace falta acortar el circuito/alimentador o relajar el límite en Configuración.");
    }

    /// <summary>
    /// Un intento completo (ampacidad + protección 240-4 + caída de tensión + piso práctico) con un N
    /// de paralelo fijo. Null si el catálogo se agota antes de bajar la caída de tensión al límite --
    /// la señal para que <see cref="Seleccionar"/> pruebe con más conductores. Si el N pedido no es
    /// legal para el calibre resultante (310-10(h)(1), mínimo 1/0 AWG), truena directo: subir más N
    /// solo empeora ese problema (baja aún más la corriente por conductor), nunca lo resuelve.
    /// </summary>
    private static (Calibre CalibreFinal, Calibre CalibreBase, decimal CaidaPct, decimal AmpacidadTotalA,
        List<Cita> Citas, decimal ROhmKm, decimal XOhmKm, decimal CaidaV)? IntentarConN(
        ICatalogoCalibres catalogo, ITablaAmpacidad ampacidad, ITablaImpedancia impedancia,
        decimal capacidadMinConductorA, decimal corrienteParaCaidaA, int nParalelo,
        decimal factorTemp, decimal factorAgrup, MaterialConductor materialConductor, MaterialCanalizacion materialCanalizacion,
        TemperaturaAislamiento tempAislamiento, TemperaturaAislamiento tempTerminales,
        decimal longitudM, decimal factorPotencia, decimal senTheta, decimal k,
        decimal tensionEfectivaV, decimal caidaTensionMaxPct, decimal? pisoPracticoCalibreMm2,
        ITablaProteccionEstandar? proteccionEstandar, decimal? proteccionA, bool permiteExcepcion2404b,
        MetodoInstalacion metodoInstalacion, decimal? cargaAl100PctA)
    {
        // Calibre de partida: por ampacidad utilizable de la corriente de diseño (crédito de
        // aislamiento incluido, ver docstring de la clase), y luego 240-4 decide si ese calibre basta
        // para la PROTECCIÓN ya elegida o si hay que subirlo.
        var (calibreBase, citaProteccion) = DeterminarCalibreBase(
            catalogo, ampacidad, proteccionEstandar, capacidadMinConductorA, proteccionA, permiteExcepcion2404b,
            nParalelo, factorTemp, factorAgrup, materialConductor, tempAislamiento, tempTerminales, metodoInstalacion,
            cargaAl100PctA);

        var objetivoPorConductor = capacidadMinConductorA / nParalelo;
        var columnaDescripcion = tempAislamiento == tempTerminales
            ? $"{(int)tempTerminales}°C"
            : $"{(int)tempAislamiento}°C con crédito, topado a {(int)tempTerminales}°C de la terminal";
        var citas = new List<Cita>
        {
            // «Capacidad mínima», no «corriente de diseño»: este número ya trae el 125 % de la carga
            // continua. Llamarlo corriente de diseño confundía con la In de 210-19(a)(1).
            cargaAl100PctA is decimal carga
                ? new("310-15(b)(16)",
                    $"Capacidad mínima {objetivoPorConductor:0.##} A por conductor contra la ampacidad de tabla a {(int)tempTerminales}°C sin factores, " +
                    $"y carga {carga / nParalelo:0.##} A contra la ampacidad corregida -> calibre {calibreBase} ({columnaDescripcion}, {materialConductor})" +
                    (nParalelo > 1 ? $" x {nParalelo} conductores en paralelo por fase" : ""))
                : new("310-15(b)(16)", $"Capacidad mínima {objetivoPorConductor:0.##} A por conductor -> calibre {calibreBase} " +
                    $"({columnaDescripcion}, {materialConductor})" + (nParalelo > 1 ? $" x {nParalelo} conductores en paralelo por fase" : "")),
        };
        if (citaProteccion is not null)
            citas.Add(citaProteccion);

        // Calibre por caída de tensión -- Tabla 9, e = k·L·In·(R·cosθ + X·senθ) / N, límite como error duro.
        // La Tabla 9 real no trae fila para todos los calibres de la Tabla 8 (p.ej. salta 700/800/900
        // kcmil) -- un hueco legítimo de la norma, no un agotamiento del catálogo. Si el candidato cae
        // en uno de esos huecos, se salta al siguiente calibre en vez de rendirse ahí mismo.
        var calibreCandidato = calibreBase;
        decimal caidaPct;
        var subioPorTope2404d = false;
        while (true)
        {
            var imp = impedancia.Impedancia(calibreCandidato, materialConductor, materialCanalizacion);
            if (imp is null)
            {
                var siguienteTrasHueco = catalogo.Siguiente(calibreCandidato);
                if (siguienteTrasHueco is null)
                    return null; // agotamiento genuino: ya no hay calibre más grande en el catálogo.
                calibreCandidato = siguienteTrasHueco;
                continue;
            }

            var caidaVolts = k * (longitudM / 1000m) * corrienteParaCaidaA * (imp.Value.ROhmKm * factorPotencia + imp.Value.XOhmKm * senTheta) / nParalelo;
            caidaPct = caidaVolts * 100m / tensionEfectivaV;

            var topeChico = proteccionA is decimal p ? TopeProteccion2404d(calibreCandidato, materialConductor) : null;
            var excedeTopeChico = topeChico is decimal tope && proteccionA!.Value > tope;

            if (caidaPct <= caidaTensionMaxPct && !excedeTopeChico) break;
            if (excedeTopeChico) subioPorTope2404d = true;

            var siguiente = catalogo.Siguiente(calibreCandidato);
            if (siguiente is null)
                return null;
            calibreCandidato = siguiente;
        }

        if (subioPorTope2404d)
            citas.Add(new Cita("240-4(d)",
                $"El calibre que hubiera bastado por caída de tensión tiene tope de protección menor a los {proteccionA} A elegidos -- " +
                $"sube a {calibreCandidato} para que la protección quede dentro del tope del conductor."));

        if (calibreCandidato.Designacion != calibreBase.Designacion)
            citas.Add(new Cita("Tabla 9", $"Caída de tensión con {calibreBase} excedía {caidaTensionMaxPct}% -> sube a {calibreCandidato} ({caidaPct:0.##}%)"));
        else
            citas.Add(new Cita("Tabla 9", $"Caída de tensión con {calibreCandidato}: {caidaPct:0.##}% (límite {caidaTensionMaxPct}%)"));

        // Piso práctico configurado (si aplica).
        var calibreFinal = calibreCandidato;
        if (pisoPracticoCalibreMm2 is decimal piso && calibreFinal.AreaMm2 < piso)
        {
            calibreFinal = catalogo.BuscarPorAreaMinima(piso);
            citas.Add(new Cita("Piso práctico", $"Sube a {calibreFinal} por el piso práctico configurado ({piso} mm²)"));
        }

        if (nParalelo > 1 && !calibreFinal.PermiteParalelo)
            throw new ConductoresParaleloNoPermitidoException(
                $"Se pidieron {nParalelo} conductores en paralelo por fase, pero el calibre resultante ({calibreFinal}) es menor a 1/0 AWG -- 310-10(h)(1) no lo permite.");

        // Ampacidad real del juego que se va a instalar -- la del calibre FINAL (que puede haber
        // subido por caída de tensión o por el piso práctico), no la del base. Es el insumo de
        // 430-62(b). Si el calibre final no tiene valor tabulado en la columna de la terminal, se
        // reporta 0 en vez de inventar un número: quien lo consuma debe tratar eso como "no se sabe".
        var ampacidadTotal = (AmpacidadUtilizable(
            ampacidad, calibreFinal, materialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, metodoInstalacion) ?? 0m) * nParalelo;

        // LA CAÍDA SE RECALCULA CON EL CALIBRE FINAL, no con el candidato. Cuando el piso práctico
        // sube el calibre, el conductor instalado es más grueso y la caída real es MENOR: reportar la
        // del candidato erraba del lado seguro, pero dejaba un documento que no cuadra consigo mismo
        // -- quien lo revisa toma el calibre impreso, aplica la fórmula y le da otro número. Una
        // memoria de cálculo tiene que poder recalcularse.
        var impFinal = impedancia.Impedancia(calibreFinal, materialConductor, materialCanalizacion);
        var rFinal = impFinal?.ROhmKm ?? 0m;
        var xFinal = impFinal?.XOhmKm ?? 0m;
        var caidaVoltsFinal = impFinal is null
            ? caidaPct * tensionEfectivaV / 100m
            : k * (longitudM / 1000m) * corrienteParaCaidaA * (rFinal * factorPotencia + xFinal * senTheta) / nParalelo;

        if (impFinal is not null)
            caidaPct = caidaVoltsFinal * 100m / tensionEfectivaV;

        return (calibreFinal, calibreBase, caidaPct, ampacidadTotal, citas, rFinal, xFinal, caidaVoltsFinal);
    }

    /// <summary>
    /// 240-4: el conductor debe protegerse según SU ampacidad utilizable -- así que si ya se eligió
    /// una protección (240-6(a)/430-52), el calibre debe cubrirla, no solo cubrir la corriente de
    /// diseño. Excepción 240-4(b): se permite que la protección quede un escalón estándar arriba de
    /// la ampacidad del conductor cuando esa ampacidad no coincide con un valor estándar de 240-6(a)
    /// y la protección no excede 800 A -- el llamador decide si el circuito califica (240-4(b)(1): no
    /// aplica a un circuito derivado que alimenta más de un contacto de uso general).
    /// <c>proteccionA: null</c> desactiva esta verificación por completo (Motor/430-52).
    /// </summary>
    private static (Calibre CalibreBase, Cita? CitaProteccion) DeterminarCalibreBase(
        ICatalogoCalibres catalogo, ITablaAmpacidad ampacidad, ITablaProteccionEstandar? proteccionEstandar,
        decimal capacidadMinConductorA, decimal? proteccionA, bool permiteExcepcion2404b,
        int nParalelo, decimal factorTemp, decimal factorAgrup, MaterialConductor materialConductor,
        TemperaturaAislamiento tempAislamiento, TemperaturaAislamiento tempTerminales, MetodoInstalacion metodoInstalacion,
        decimal? cargaAl100PctA)
    {
        var objetivoPorCarga = capacidadMinConductorA / nParalelo;
        var calibrePorCarga = cargaAl100PctA is decimal carga
            ? CalibrePorDosRevisiones(
                catalogo, ampacidad, objetivoPorCarga, carga / nParalelo, materialConductor, tempAislamiento, tempTerminales,
                factorTemp, factorAgrup, metodoInstalacion)
            : CalibrePorAmpacidadUtilizable(
                catalogo, ampacidad, objetivoPorCarga, materialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, metodoInstalacion);

        if (proteccionA is not decimal breaker || proteccionEstandar is null)
            return (calibrePorCarga, null); // fuera de alcance de 240-4 (Motor, que se rige por 430-52).

        var ampacidadUtilCarga = AmpacidadUtilizable(ampacidad, calibrePorCarga, materialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, metodoInstalacion)
            ?? throw new InvalidOperationException($"La Tabla 310-15(b)(16)/(17) no trae ampacidad para {calibrePorCarga}.");

        if (breaker <= ampacidadUtilCarga)
            return (calibrePorCarga, null); // la protección ya cabe dentro de la ampacidad del conductor -- caso normal, sin discrepancia.

        var aplicaExcepcion = permiteExcepcion2404b
            && breaker <= 800m
            // Contra la lista COMPLETA de 240-6(a), no contra la serie en que se eligió la protección:
            // «el siguiente valor estándar» es el de la norma. Ver ITablaProteccionEstandar.ValoresDeLaNorma.
            && !proteccionEstandar.ValoresDeLaNorma.Contains(ampacidadUtilCarga)
            && breaker == proteccionEstandar.SiguienteDeLaNorma(ampacidadUtilCarga);

        if (aplicaExcepcion)
            return (calibrePorCarga, new Cita("240-4(b)",
                $"Protección {breaker} A un escalón arriba de la ampacidad de {calibrePorCarga} ({ampacidadUtilCarga} A) -- permitido: " +
                "no corresponde a un valor estándar de 240-6(a) y no excede 800 A."));

        // Sin la excepción: el conductor se dimensiona para cubrir la PROTECCIÓN, no solo la corriente de diseño.
        var objetivoPorProteccion = breaker / nParalelo;
        var calibrePorProteccion = CalibrePorAmpacidadUtilizable(
            catalogo, ampacidad, objetivoPorProteccion, materialConductor, tempAislamiento, tempTerminales, factorTemp, factorAgrup, metodoInstalacion);

        if (calibrePorProteccion.Designacion == calibrePorCarga.Designacion)
            return (calibrePorCarga, null); // la ampacidad utilizable no alcanzaba el umbral exacto de breaker, pero el mismo calibre ya cubre ambos -- nada que reportar.

        return (calibrePorProteccion, new Cita("240-4",
            $"{calibrePorCarga} ({ampacidadUtilCarga} A) no cubre la protección de {breaker} A y no calificaba para la excepción 240-4(b) -- " +
            $"sube a {calibrePorProteccion} para que el conductor quede protegido según su ampacidad."));
    }

    /// <summary>
    /// 110-14(c)(1)a.(2)/b.(2): ampacidad realmente utilizable de un calibre, con el crédito de
    /// aislamiento aplicado. Cuando el aislamiento pide una columna más caliente que la terminal, se
    /// corrige la ampacidad de ESA columna con los factores de temperatura/agrupamiento, pero el
    /// resultado se topa a la ampacidad TABULADA (sin corregir) de la columna de la terminal -- nunca
    /// se puede reclamar más de lo que la terminal aguanta, sin importar qué tan buena sea la
    /// corrección del lado del aislamiento. Si el calibre no tiene valor tabulado en la columna de la
    /// terminal (p.ej. 18/16 AWG, que en esta tabla solo traen columna de 90°C -- son harina de otro
    /// costal, 240-4(d)(1)/(2), fuera del alcance curado de este motor), ese calibre simplemente no
    /// es válido para esa terminal -- null, no "sin tope".
    /// </summary>
    private static decimal? AmpacidadUtilizable(
        ITablaAmpacidad ampacidad, Calibre calibre, MaterialConductor material,
        TemperaturaAislamiento tempAislamiento, TemperaturaAislamiento tempTerminales,
        decimal factorTemp, decimal factorAgrup, MetodoInstalacion metodo)
    {
        var ampacidadPropia = ampacidad.Ampacidad(calibre, material, tempAislamiento, metodo);
        if (ampacidadPropia is null) return null;

        var corregida = ampacidadPropia.Value * factorTemp * factorAgrup;
        if (tempAislamiento == tempTerminales)
            return corregida; // no hay tope distinto que aplicar -- el caso de siempre.

        var topeTerminal = ampacidad.Ampacidad(calibre, material, tempTerminales, metodo);
        return topeTerminal is decimal tope ? Math.Min(corregida, tope) : null;
    }

    /// <summary>
    /// <b>Las dos revisiones de 210-19(a)(1) / 215-2(a)(1), por separado.</b> El texto:
    /// <i>«ampacidad no menor que la correspondiente a la carga máxima […]. El tamaño mínimo del
    /// conductor, <b>antes de la aplicación de cualquier factor de ajuste o de corrección</b>, deberá
    /// tener una ampacidad permisible no menor que la carga no-continua más el 125 por ciento de la
    /// carga continua»</i>. O sea: el 125 % se compara con la ampacidad de TABLA (en la columna de la
    /// terminal, 110-14(c)), y la carga al 100 % con la ampacidad CORREGIDA. El 125 % y los factores
    /// no se multiplican.
    ///
    /// <para>
    /// Hasta el 2026-09-23 (Power Node Web, auditoría de selección de conductor) se exigía que la
    /// ampacidad corregida cubriera el 125 %: sobredimensionaba en cuanto había factores. Con 32 A
    /// continuos y 9 agrupados daba 6 AWG donde la norma admite 8. Sin factores (30 °C, 3 agrupados)
    /// las dos formas dan lo mismo.
    /// </para>
    /// </summary>
    private static Calibre CalibrePorDosRevisiones(
        ICatalogoCalibres catalogo, ITablaAmpacidad ampacidad, decimal capacidadMinimaPorConductorA, decimal cargaPorConductorA,
        MaterialConductor material, TemperaturaAislamiento tempAislamiento, TemperaturaAislamiento tempTerminales,
        decimal factorTemp, decimal factorAgrup, MetodoInstalacion metodo)
    {
        var calibre = catalogo.Listar()
            .OrderBy(c => c.AreaMm2)
            .FirstOrDefault(c =>
                ampacidad.Ampacidad(c, material, tempTerminales, metodo) is decimal deTabla && deTabla >= capacidadMinimaPorConductorA &&
                AmpacidadUtilizable(ampacidad, c, material, tempAislamiento, tempTerminales, factorTemp, factorAgrup, metodo) is decimal u && u >= cargaPorConductorA);

        return calibre ?? throw new InvalidOperationException(
            $"No hay calibre en el catálogo con ampacidad de tabla >= {capacidadMinimaPorConductorA:0.##} A a {(int)tempTerminales}°C y " +
            $"ampacidad corregida >= {cargaPorConductorA:0.##} A para {material}.");
    }

    private static Calibre CalibrePorAmpacidadUtilizable(
        ICatalogoCalibres catalogo, ITablaAmpacidad ampacidad, decimal corrienteObjetivoPorConductor,
        MaterialConductor material, TemperaturaAislamiento tempAislamiento, TemperaturaAislamiento tempTerminales,
        decimal factorTemp, decimal factorAgrup, MetodoInstalacion metodo)
    {
        var calibre = catalogo.Listar()
            .OrderBy(c => c.AreaMm2)
            .FirstOrDefault(c => AmpacidadUtilizable(ampacidad, c, material, tempAislamiento, tempTerminales, factorTemp, factorAgrup, metodo) is decimal u && u >= corrienteObjetivoPorConductor);

        return calibre ?? throw new InvalidOperationException(
            $"No hay calibre en el catálogo con ampacidad utilizable >= {corrienteObjetivoPorConductor:0.##} A para {material} " +
            $"({(int)tempAislamiento}°C, topado a {(int)tempTerminales}°C de la terminal).");
    }

    /// <summary>
    /// 240-4(d): tope de protección para conductores de cobre pequeños, sin importar la corrección
    /// por temperatura/agrupamiento -- el texto de esta NOM solo define valores para cobre
    /// (240-4(d)(3)/(5)/(7); los incisos (1)/(2) de 18/16 AWG traen condiciones adicionales no
    /// modeladas aquí porque prácticamente nunca aparecen en un circuito de fuerza/alumbrado/
    /// contactos real).
    /// </summary>
    private static decimal? TopeProteccion2404d(Calibre calibre, MaterialConductor material)
    {
        if (material != MaterialConductor.Cobre) return null;
        return calibre.Designacion switch
        {
            "14" => 15m,
            "12" => 20m,
            "10" => 30m,
            _ => null,
        };
    }
}
