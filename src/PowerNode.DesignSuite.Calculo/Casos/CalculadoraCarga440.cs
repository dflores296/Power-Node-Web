using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>Lo que sale de aplicar el Art. 440 a un motocompresor hermético.</summary>
public sealed record ResultadoCarga440(
    decimal CorrienteBaseA,
    decimal CorrienteConductorA,
    decimal ProteccionCortocircuitoA,
    decimal ProteccionMaximaPermitidaA,
    decimal ProteccionSobrecargaA,
    IReadOnlyList<Cita> Citas);

/// <summary>
/// circuito derivado de un <b>motocompresor hermético de refrigeración</b> — chiller, manejadora,
/// cualquier equipo de aire acondicionado o refrigeración accionado por motor (Art. 440).
///
/// <b>Por qué no se resuelve con la calculadora del 430.</b> 440-3(a) dice que las disposiciones
/// del 440 "son adicionales o modifican las disposiciones del Artículo 430", y las que modifica son
/// justo las que importan aquí:
///
/// <list type="bullet">
/// <item><b>440-6(a): la corriente sale de la PLACA</b>, no de las Tablas 430-247/248/249/250 por
/// Hp. Un chiller muchas veces ni trae Hp marcado.</item>
///
/// <item><b>440-22(a): la protección NO se redondea hacia arriba.</b> Es la diferencia que más
/// fácil se modela mal. El 430-52(c)(1) Excepción 1 permite subir al siguiente tamaño estándar
/// cuando el valor calculado no corresponde a uno; el 440-22(a) dice "no exceda el 175 por ciento",
/// sin ese permiso, así que se toma el mayor estándar que <b>quepa por debajo</b> del techo.</item>
///
/// <item><b>440-8: es UNA SOLA MÁQUINA</b> aunque traiga varios motores adentro (compresor,
/// ventiladores, bombas). Por eso una <c>Carga</c> de este régimen es un solo elemento y no un
/// tablero.</item>
/// </list>
///
/// <b>Los dos datos de placa.</b> Cada regla del Artículo dice "la corriente de carga nominal del
/// motocompresor, o la corriente de selección del circuito derivado, de estos dos valores el que
/// sea mayor". Son dos marcas distintas de la placa; la segunda solo viene en algunos equipos
/// (Excepción 1 de 440-6(a)). Todo lo de abajo trabaja sobre el mayor de las dos.
///
/// <b>Fuera de alcance a propósito:</b> 440-22(b) (equipos con varios motocompresores o con cargas
/// adicionales — remite a 430-53), y las excepciones de circuitos derivados de 15/20 A conectados
/// con cordón y clavija (440-54/440-55), que son electrodomésticos, no equipo de proyecto.
/// </summary>
public static class CalculadoraCarga440
{
    /// <summary>440-32: los conductores no menores al 125 % de la corriente base.</summary>
    public const decimal FactorConductorPct = 125m;

    /// <summary>440-22(a): techo normal de la protección contra cortocircuito y falla a tierra.</summary>
    public const decimal TechoProteccionPct = 175m;

    /// <summary>440-22(a): techo ampliado, solo cuando el normal no aguanta la corriente de arranque.</summary>
    public const decimal TechoProteccionArranquePct = 225m;

    /// <summary>440-52(a)(1): el relevador de sobrecarga dispara a no más del 140 %.</summary>
    public const decimal FactorSobrecargaPct = 140m;

    /// <summary>
    /// Excepción de 440-22(a): "No se exigirá que el valor nominal del dispositivo de protección
    /// contra cortocircuito y falla a tierra del circuito derivado sea menor a 15 amperes."
    /// </summary>
    public const decimal ProteccionMinimaA = 15m;

    /// <summary>
    /// La corriente sobre la que trabaja todo el Artículo: la mayor entre la corriente de carga
    /// nominal de placa y la corriente de selección del circuito derivado, cuando esta viene
    /// marcada (440-6(a) y su Excepción 1).
    /// </summary>
    public static decimal CorrienteBase(decimal corrienteNominalPlacaA, decimal? corrienteSeleccionCircuitoA) =>
        Math.Max(corrienteNominalPlacaA, corrienteSeleccionCircuitoA ?? 0m);

    /// <param name="requiereArranque">
    /// El usuario declara que la protección al 175 % no aguanta la corriente de arranque. Es
    /// declaración, no deducción: depende del perfil de arranque real del equipo, que el programa no
    /// tiene. Sube el techo al 225 %, que es el máximo que permite 440-22(a).
    /// </param>
    public static ResultadoCarga440 Calcular(
        ITablaProteccionEstandar proteccionEstandar,
        decimal corrienteNominalPlacaA,
        decimal? corrienteSeleccionCircuitoA = null,
        bool requiereArranque = false)
    {
        if (corrienteNominalPlacaA <= 0)
            throw new ArgumentOutOfRangeException(nameof(corrienteNominalPlacaA),
                "440-6(a) exige la corriente de carga nominal de la placa; sin ella no hay nada que calcular.");

        var citas = new List<Cita>();
        var baseA = CorrienteBase(corrienteNominalPlacaA, corrienteSeleccionCircuitoA);

        citas.Add(new Cita("440-6(a)",
            corrienteSeleccionCircuitoA is { } sel && sel > corrienteNominalPlacaA
                ? $"Corriente base {baseA:N2} A: la de selección del circuito derivado ({sel:N2} A) es mayor que la " +
                  $"de carga nominal ({corrienteNominalPlacaA:N2} A), y la Excepción 1 manda usar aquella."
                : $"Corriente base {baseA:N2} A, de la placa del equipo. No se usa la FLC por Hp de las Tablas " +
                  "430-247/248/249/250: el 440-6(a) manda la corriente de placa."));

        // ---------------------------------------------------------------- conductor (440-32)
        var corrienteConductor = Math.Round(baseA * FactorConductorPct / 100m, 3);
        citas.Add(new Cita("440-32",
            $"Conductores del circuito derivado a no menos del {FactorConductorPct:N0} % de {baseA:N2} A = " +
            $"{corrienteConductor:N2} A."));

        // ---------------------------------------------------------------- cortocircuito (440-22(a))
        var techoPct = requiereArranque ? TechoProteccionArranquePct : TechoProteccionPct;
        var techoA = baseA * techoPct / 100m;
        var techoMaximoA = baseA * TechoProteccionArranquePct / 100m;

        // NO es SiguienteEstandar: 440-22(a) dice "no exceda", sin el permiso de redondear hacia
        // arriba que sí trae 430-52(c)(1) Excepción 1. Se toma el mayor estándar que quepa debajo.
        var proteccion = proteccionEstandar.AnteriorEstandar(techoA);

        if (proteccion is null || proteccion < ProteccionMinimaA)
        {
            // Excepción de 440-22(a): nunca se exige bajar de 15 A.
            proteccion = ProteccionMinimaA;
            citas.Add(new Cita("440-22(a) Excepción",
                $"El {techoPct:N0} % de {baseA:N2} A da {techoA:N2} A, por debajo del valor estándar más chico. " +
                $"La Excepción releva de bajar de {ProteccionMinimaA:N0} A, así que se usa ese."));
        }
        else
        {
            citas.Add(new Cita("440-22(a)",
                $"Protección contra cortocircuito y falla a tierra: no debe exceder el {techoPct:N0} % de " +
                $"{baseA:N2} A = {techoA:N2} A. Se elige {proteccion:N0} A, el mayor valor estándar que no lo " +
                $"excede (240-6(a))." +
                (requiereArranque
                    ? " Se usó el techo ampliado del 225 % porque el del 175 % no conduce la corriente de arranque."
                    : $" Si no conduce la corriente de arranque, el Artículo permite subir hasta el " +
                      $"{TechoProteccionArranquePct:N0} % = {techoMaximoA:N2} A.")));
        }

        // ---------------------------------------------------------------- sobrecarga (440-52(a)(1))
        var sobrecarga = Math.Round(baseA * FactorSobrecargaPct / 100m, 3);
        citas.Add(new Cita("440-52(a)(1)",
            $"Relevador de sobrecarga separado, ajustado para disparar a no más del {FactorSobrecargaPct:N0} % " +
            $"de {baseA:N2} A = {sobrecarga:N2} A. Es un aparato DISTINTO de la protección contra cortocircuito."));

        return new ResultadoCarga440(baseA, corrienteConductor, proteccion.Value, techoMaximoA, sobrecarga, citas);
    }
}
