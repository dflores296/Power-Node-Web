using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>Qué es cada máquina de un grupo: decide su porcentaje de protección y el artículo.</summary>
public enum ClaseDeMiembro
{
    /// <summary>Motor de uso general — Art. 430; su protección, de la Tabla 430-52.</summary>
    Motor,

    /// <summary>Motocompresor hermético de refrigeración — Art. 440; su protección, 175 % / 225 % de 440-22(a).</summary>
    Motocompresor,

    /// <summary>
    /// Motor con variador de velocidad — 430 Parte J (nace en Power Node Web, David 2026-09-30): su
    /// corriente es la de entrada del variador (430-122(a)); su protección, la máxima que marca su
    /// fabricante (110-3(b)), en lugar del porcentaje de la Tabla 430-52.
    /// </summary>
    Variador,
}

/// <summary>
/// Una máquina del grupo: su corriente por unidad (la FLC de tabla de un motor, 430-6(a); la corriente
/// de carga nominal o la de selección de un motocompresor, la mayor, 440-6(a)) y de dónde sale, ya
/// redactado para la cita.
/// </summary>
/// <param name="Hp">Los caballos de un motor (de placa, o interpolados si se marcó en amperes); null en un
/// motocompresor. Solo sirven para la nota de 430-53(a).</param>
/// <param name="ProteccionMaximaA">Solo en un variador: la protección máxima que marca su fabricante — 110-3(b).</param>
public sealed record MiembroDelGrupo(string Nombre, ClaseDeMiembro Clase, int Cantidad, decimal CorrienteUnitariaA, string Origen, decimal? Hp = null, decimal? ProteccionMaximaA = null);

/// <summary>
/// La lista de máquinas, comparable por su contenido: el cálculo se recuerda por su entrada
/// (<c>CuadroDeCarga.Recordado</c>), y un record compara sus listas por referencia.
/// </summary>
public sealed class MiembrosDelGrupo(IEnumerable<MiembroDelGrupo> miembros) : IEquatable<MiembrosDelGrupo>
{
    public IReadOnlyList<MiembroDelGrupo> Lista { get; } = [.. miembros];

    public bool Equals(MiembrosDelGrupo? otro) => otro is not null && Lista.SequenceEqual(otro.Lista);

    public override bool Equals(object? obj) => Equals(obj as MiembrosDelGrupo);

    public override int GetHashCode() => Lista.Aggregate(17, (h, m) => HashCode.Combine(h, m));
}

/// <summary>
/// <b>Varios motores, o motores y otras cargas, en un circuito derivado</b> — 430-53; con
/// motocompresores, 440-22(b). Todas las máquinas van a la tensión y las fases del circuito. Las otras
/// cargas (alumbrado, resistencias…) entran ya en amperes, continua y no continua.
/// </summary>
/// <param name="MayorOtraCargaA">La corriente de la otra carga más grande, sola: decide si el
/// motocompresor es «la carga más grande» — 440-22(b)(1) contra (b)(2).</param>
/// <param name="TipoMotor">El renglón de la Tabla 430-52 para los motores del grupo.</param>
/// <param name="RequiereArranque">El motocompresor mayor no arranca al 175 %: 225 % — 440-22(a).</param>
public sealed record DatosEntradaCircuitoDerivadoGrupo(
    MiembrosDelGrupo Miembros,
    decimal OtrasContinuaA,
    decimal OtrasNoContinuaA,
    decimal MayorOtraCargaA,
    TipoMotor TipoMotor,
    TipoDispositivoProteccionMotor TipoDispositivoProteccion,
    bool RequiereArranque,
    int NumeroFases,
    decimal TensionFaseNeutroV,
    decimal TensionFaseFaseV,
    decimal LongitudM,
    int NumeroConductoresParalelo,
    int NumeroConductoresAgrupados,
    decimal TemperaturaAmbienteC,
    MaterialConductor MaterialConductor,
    MaterialCanalizacion MaterialCanalizacion,
    decimal FactorPotencia,
    decimal CaidaTensionMaxPct,
    decimal? PisoPracticoCalibreMm2,
    string TipoAislamiento = "THHN",
    LugarDeInstalacion Lugar = LugarDeInstalacion.Seco,
    MetodoInstalacion MetodoInstalacion = MetodoInstalacion.CanalizacionOCable,
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision,
    bool TerminalesMarcadas75C = false);

/// <summary>
/// Los números del grupo, para el desglose y la memoria: con ellos se rehace el cálculo a mano.
/// </summary>
/// <param name="Regla">«430-53(c)(4)», «440-22(b)(1)» o «440-22(b)(2)»: de dónde sale el límite de la protección.</param>
/// <param name="Mayor">La máquina de mayor corriente — 440-7, 430-17.</param>
/// <param name="PorcentajeMayor">El de la Tabla 430-52 (motor) o el de 440-22(a) (motocompresor); 0 en 440-22(b)(2) sin motores.</param>
/// <param name="ValorDelMayorA">Lo que la protección permite por el mayor: su porcentaje × su corriente.</param>
/// <param name="SumaDemasA">Las demás máquinas, al 100 %.</param>
/// <param name="OtrasCargasA">Las otras cargas en el límite: su corriente, o lo que 240-4 permite para ellas en 440-22(b)(2).</param>
/// <param name="TechoA">El límite de la protección.</param>
/// <param name="PisoA">La corriente de operación: las máquinas al 100 % y la continua al 125 % — el interruptor no puede quedar abajo.</param>
/// <param name="Limite240_4bA">Solo si se subió por 240-4(b): hasta dónde se podía.</param>
/// <param name="TopeDelFabricante">El límite es la protección máxima de un variador — 430-53(c)(2), 110-3(b): no se sube por 240-4(b).</param>
public sealed record DetalleDelGrupo(
    string Regla,
    MiembroDelGrupo Mayor,
    decimal PorcentajeMayor,
    decimal ValorDelMayorA,
    decimal SumaDemasA,
    decimal OtrasCargasA,
    decimal TechoA,
    decimal PisoA,
    decimal? Limite240_4bA,
    bool TopeDelFabricante = false);
