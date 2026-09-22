using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>Tabla 310-104(a) — Aplicaciones y aislamientos de conductores de 600 volts.</summary>
public interface ITablaAislamiento
{
    /// <summary>
    /// Temperatura máxima del aislamiento para esta designación (p.ej. "THHN", "THW") en el lugar
    /// dado. Null si la designación no está reconocida, o no es válida para ese lugar (p.ej. THHN en
    /// lugar mojado -- solo está listada para lugares secos), o su temperatura real (150-250°C, los
    /// aislamientos especiales) queda fuera del alcance de este motor (solo cubre 60/75/90°C).
    /// </summary>
    TemperaturaAislamiento? TemperaturaMaxima(string designacion, bool lugarSeco);

    /// <summary>Las designaciones que este motor reconoce, para armar un mensaje de error útil.</summary>
    IReadOnlyList<string> DesignacionesReconocidas { get; }

    /// <summary>
    /// <b>Termoplástico o termofijo</b>, que es lo que decide la constante <c>k</c> de la curva de
    /// daño del conductor (IEC 60364-5-54 Tabla A.54.4).
    ///
    /// <para>
    /// ⚠ <b>Sale de la columna «aislamiento» de la Tabla 310-104(a), no de la temperatura.</b>
    /// Deducirla de la temperatura —90 °C ⇒ termofijo— es lo que hace
    /// <c>CurvaDanioConductor.FamiliaSugerida</c>, y con <b>THHN</b> se equivoca: es de 90 °C y es
    /// <b>termoplástico</b>. El error va del lado peligroso, porque el termofijo tiene <c>k</c> mayor
    /// (143 contra 115 en cobre) y daría un conductor por protegido cuando puede no estarlo. THHN es
    /// además el aislamiento por omisión de este programa, así que el caso equivocado sería el común.
    /// </para>
    ///
    /// <para>
    /// <c>null</c> cuando la tabla no dice el material —hay designaciones con esa celda vacía— o la
    /// designación no se reconoce. <b>No se adivina:</b> sin familia no hay curva de daño, y decirlo
    /// es mejor que suponer la permisiva.
    /// </para>
    /// </summary>
    FamiliaAislamiento? FamiliaDe(string designacion);
}
