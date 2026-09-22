namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// La familia del aislamiento, que es lo que decide la constante <c>k</c> del daño térmico del
/// conductor — <b>no la temperatura por sí sola</b>.
///
/// <para>
/// Separado de <c>Coordinacion/CurvaDanioConductor.cs</c> (repo de escritorio) al copiar el motor
/// a este repo: ese archivo arrastra <c>CurvaDisparo</c> y el resto de la maquinaria de
/// coordinación/selectividad, que está fuera del alcance de v1 aquí, pero este enum lo usa
/// <see cref="ITablaAislamiento"/>, que sí es del alcance base. Ver
/// docs/decisiones/motor-copiado-no-enlazado.md.
/// </para>
/// </summary>
public enum FamiliaAislamiento
{
    /// <summary>PVC y demás termoplásticos. <c>k</c> = 115 en cobre, 76 en aluminio.</summary>
    Termoplastico,

    /// <summary>XLPE y EPR, termoestables. Aguantan más: <c>k</c> = 143 en cobre, 94 en aluminio.</summary>
    Termoestable,
}
