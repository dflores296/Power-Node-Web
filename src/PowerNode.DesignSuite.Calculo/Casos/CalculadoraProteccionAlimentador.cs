using PowerNode.DesignSuite.Calculo.Magnitudes;
using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <param name="TechoProteccion430_62A">El máximo que 430-62(a) permite para este alimentador, ya
/// sumada la carga no-motor que manda 430-63. <c>null</c> cuando no hay motores en el grupo, o
/// cuando no se conoce la protección de ninguno de sus derivados — sin ese dato el techo no existe.
/// </param>
/// <param name="ProteccionExcedeTecho430_62">La protección elegida quedó por arriba del techo. No
/// es un error de cálculo: es un aviso de que el alimentador, tal como está capturado, no cumple
/// 430-62(a) — y la salida es 430-62(b), que <see cref="CalculadoraAlimentador"/> evalúa cuando ya
/// conoce el conductor.</param>
public sealed record ResultadoProteccionAlimentador(
    decimal CorrienteDisenoA,
    decimal CapacidadMinimaA,
    decimal ProteccionA,
    IReadOnlyList<Cita> Citas,
    decimal? TechoProteccion430_62A = null,
    bool ProteccionExcedeTecho430_62 = false,
    // Van aparte de las citas porque no sustentan el número: dicen qué revisar en campo. Ver
    // CargaContinua100Pct.
    IReadOnlyList<string>? AvisosCargaContinua = null,
    /// <summary>La excepción del ensamble al 100 % se concedió: la carga continua entró al 100 %,
    /// no al 125 %. Es el dato que necesita quien verifica el aislamiento de 90 °C.</summary>
    bool CargaContinuaAlCienPorCiento = false);

/// <summary>
/// Los primeros tres pasos de un Alimentador (215-2/215-3/240-6(a)): corriente de
/// diseño y protección estándar, sin conductor ni caída de tensión. Separado de
/// <see cref="CalculadoraAlimentador"/> porque el interruptor principal de un
/// Tablero (<c>CalculoTablero.BreakerPrincipalA</c>) necesita exactamente esto -- no
/// hay un "conductor" que dimensionar para el bus propio de un tablero, solo para el
/// Alimentador que lo alimenta. <see cref="CalculadoraAlimentador"/> reusa este mismo
/// cálculo como sus primeros tres pasos, para no repetir la fórmula en dos lugares.
/// </summary>
public static class CalculadoraProteccionAlimentador
{
    public static ResultadoProteccionAlimentador Calcular(
        ITablaProteccionEstandar proteccionEstandar,
        decimal cargaContinuaVA,
        decimal cargaNoContinuaVA,
        int numeroFases,
        decimal tensionFaseNeutroV,
        decimal tensionFaseFaseV,
        AgregadoMotores cargaMotores = default,
        bool conjuntoAprobado100Pct = false,
        bool? modeloProteccionEsDe100Pct = null,
        ClaseDeTramo clase = ClaseDeTramo.Alimentador)
    {
        var citas = new List<Cita>();

        // La excepción del 100 % de 215-3 / 215-2(a)(1). Sin declaración devuelve 1.25, que es lo
        // que este método hacía siempre.
        var continua = CargaContinua100Pct.Para(
            conjuntoAprobado100Pct, modeloProteccionEsDe100Pct,
            clase.CapacidadMinima(), clase.Excepcion100Pct());
        citas.AddRange(continua.Citas);

        // 1. Corriente de diseño -- 215-2, mismo criterio que 210-19(a)(1) para un circuito. Si hay
        // motores compartiendo el alimentador, su corriente real (100% de cada FLC, sin el margen
        // de arranque del mayor) se suma directo -- para caída de tensión importa lo que de verdad
        // fluye en operación normal, no el margen de sobredimensionamiento de protección.
        var tensionEfectiva = numeroFases == 1 ? tensionFaseNeutroV : tensionFaseFaseV;
        var iContinua = numeroFases == 3
            ? SistemaTrifasico.CorrienteLineaDesdePotencia(cargaContinuaVA, tensionEfectiva)
            : SistemaTrifasico.CorrienteMonofasicaDesdePotencia(cargaContinuaVA, tensionEfectiva);
        var iNoContinua = numeroFases == 3
            ? SistemaTrifasico.CorrienteLineaDesdePotencia(cargaNoContinuaVA, tensionEfectiva)
            : SistemaTrifasico.CorrienteMonofasicaDesdePotencia(cargaNoContinuaVA, tensionEfectiva);
        var in_ = iContinua + iNoContinua + cargaMotores.CorrienteRealA;

        citas.Add(new Cita(clase.CorrienteDeDiseno(), $"In = {iNoContinua:0.##} A (no continua) + {iContinua:0.##} A (continua)" +
            (cargaMotores.MayorFlcA is not null ? $" + {cargaMotores.CorrienteRealA:0.##} A (motores)" : "") +
            $" = {in_:0.##} A"));

        // 2. Capacidad mínima -- 215-3: 125% de la continua + 100% de la no continua. Si hay
        // motores, 430-24 se suma aparte: 125% del FLC mayor + 100% de la suma de los demás.
        //
        // El factor de la carga continua NO toca la parte de motores: 430-24 es otra regla (125 % del
        // FLC mayor por el arranque, no por operar tres horas), y la excepción del ensamble al 100 %
        // no habla de ella. Mezclarlas dejaría un alimentador de motores por debajo de su piso.
        var capacidadMin = continua.Factor * iContinua + iNoContinua + cargaMotores.CapacidadMinimaA;
        citas.Add(new Cita(clase.CapacidadMinima(), $"Capacidad mínima (no motor): {continua.Factor * 100m:0}% x {iContinua:0.##} A + {iNoContinua:0.##} A = {continua.Factor * iContinua + iNoContinua:0.##} A"));
        if (cargaMotores.MayorFlcA is decimal mayorFlc)
            citas.Add(new Cita("430-24", $"Varios motores: 125% x {mayorFlc:0.##} A (el mayor) + {cargaMotores.SumaRestoFlcA:0.##} A (resto) = {cargaMotores.CapacidadMinimaA:0.##} A -> capacidad mínima total {capacidadMin:0.##} A"));

        // 3. Protección estándar -- 240-6(a), sin piso.
        var breaker = proteccionEstandar.SiguienteEstandar(capacidadMin);
        citas.Add(new Cita("240-6(a)", $"Capacidad mínima {capacidadMin:0.##} A -> protección estándar {breaker} A"));

        // 4. El TECHO de 430-62(a), que hasta el 2026-08-17 no existía: todo lo de arriba es PISO
        // (215-3 y 430-24 dicen "no menor a"), pero con motores colgando la norma pone además un
        // máximo, y el redondeo al estándar superior del paso 3 lo puede rebasar sin que nadie avise.
        //
        // 430-63 es el que dice cómo se combinan las dos cargas cuando el alimentador lleva motores
        // Y otra carga: el dispositivo "debe tener un valor nominal no menor al requerido para la
        // suma de otra carga, más [...] para dos o más motores, el valor nominal permitido en
        // 430-62". Por eso la carga no-motor se suma al techo, no compite con él.
        decimal? techo = null;
        var excedeTecho = false;

        if (cargaMotores.TechoProteccion430_62aA is decimal techoMotores)
        {
            var noMotorA = iContinua + iNoContinua;
            techo = techoMotores + noMotorA;

            citas.Add(new Cita("430-62(a)",
                $"Techo de la protección: {cargaMotores.MayorProteccionDerivadoA:0.##} A (la mayor protección de derivado del grupo) + " +
                $"{cargaMotores.SumaFlcTotalA - cargaMotores.FlcDelMayorProteccionA:0.##} A (suma de los FLC de los demás motores) = {techoMotores:0.##} A"));

            if (noMotorA > 0m)
                citas.Add(new Cita("430-63",
                    $"El alimentador lleva motores y otra carga: al techo de 430-62 se le suma la otra carga ({noMotorA:0.##} A) = {techo:0.##} A"));

            excedeTecho = breaker > techo;
            if (excedeTecho)
                citas.Add(new Cita("430-62(a)",
                    $"⚠ La protección estándar de {breaker} A excede el techo de {techo:0.##} A. 430-62(a) dice \"no mayor\", así que este " +
                    $"alimentador no cumple tal como está -- salvo por 430-62(b), que permite basarse en la ampacidad del conductor " +
                    $"cuando ésta supera lo que exige 430-24."));
        }

        return new ResultadoProteccionAlimentador(
            in_, capacidadMin, breaker, citas, techo, excedeTecho, continua.Avisos, continua.AlCienPorCiento);
    }
}
