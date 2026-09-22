using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Derivaciones;

/// <summary>
/// Todo lo que hace falta para revisar una derivación contra 240-21(b). Es un <b>dato de entrada</b>,
/// no una entidad: así las cinco reglas se prueban con objetos armados a mano, sin base y sin
/// dominio — el mismo patrón de <see cref="Tableros.DerivadoILine"/>.
///
/// <para>
/// <b>Casi todo lo declarativo es anulable a propósito.</b> Las condiciones de 240-21(b) no son
/// números que el motor pueda deducir: son hechos de la instalación —si va en canalización, si sale
/// de la envolvente, si el personal es calificado— que solo los sabe quien la diseña. <c>null</c>
/// significa «no se capturó», y una condición que no se puede revisar <b>no se declara incumplida</b>
/// (<see cref="EstadoCondicion.NoVerificable"/>).
/// </para>
/// </summary>
/// <param name="LongitudM">Longitud de los conductores de derivación, en metros. Es la del tramo
/// derivado, contada desde el punto de conexión al alimentador — no la del alimentador.</param>
/// <param name="AmpacidadDerivacionA">Ampacidad del conductor de derivación ya corregida (310-15),
/// que es contra la que se miden los tercios y los décimos de la norma.</param>
/// <param name="ProteccionAlimentadorA">Valor nominal del dispositivo de sobrecorriente <b>que
/// protege los conductores del alimentador</b> — el interruptor del tablero padre. Es el número del
/// que cuelgan 1/3 y 1/10.</param>
/// <param name="CargaCalculadaA">Carga calculada combinada de lo que alimenta la derivación.</param>
/// <param name="ProteccionEnTerminacionA">Valor nominal del dispositivo alimentado por la
/// derivación, o de la protección donde termina — lo que exige 240-21(b)(1)(1)b. En un tablero hijo
/// es su interruptor principal (408-36).</param>
/// <param name="TerminaEnUnSoloDispositivo">La derivación termina en <b>un solo</b> interruptor
/// automático o un solo conjunto de fusibles que limita la carga a la ampacidad del conductor. Ese
/// dispositivo sí puede alimentar cuantas protecciones quiera aguas abajo.</param>
/// <param name="ProtegidaContraDanoFisico">Los conductores van en canalización aprobada u otros
/// medios aprobados.</param>
/// <param name="NoSeExtiendeMasAllaDelTablero">Los conductores no se extienden más allá del tablero,
/// desconectador o dispositivo de control que alimentan — 240-21(b)(1)(2).</param>
/// <param name="EnCanalizacionDesdeLaDerivacion">Excepto en el punto de conexión al alimentador, los
/// conductores van alojados en una canalización que llega hasta la envolvente del tablero —
/// 240-21(b)(1)(3). Es más estricto que <paramref name="ProtegidaContraDanoFisico"/> y por eso es
/// un dato aparte.</param>
/// <param name="SaleDeLaEnvolvente">Instalación en campo en la que los conductores de derivación
/// salen de la envolvente o bóveda donde se hace la derivación. Es lo que enciende el piso de 1/10
/// de 240-21(b)(1)(4).</param>
/// <param name="EnExterior">Los conductores están en el exterior del edificio o estructura, excepto
/// en el punto de terminación de la carga.</param>
/// <param name="Material">Material del conductor de derivación. Solo lo usa 240-21(b)(4)(7), que
/// pone un calibre mínimo distinto para cobre y para aluminio.</param>
/// <param name="AreaMm2">Área del conductor de derivación, para ese mismo calibre mínimo.</param>
/// <param name="PersonalCalificado">240-21(b)(4)(1): las condiciones de mantenimiento y supervisión
/// aseguran que solo lo atiende personal calificado.</param>
/// <param name="AlturaParedM">Altura de las paredes de la nave industrial, en metros.</param>
/// <param name="LongitudHorizontalM">Longitud horizontal de la derivación, en metros.</param>
/// <param name="SinEmpalmes">Los conductores son continuos de extremo a extremo.</param>
/// <param name="NoAtraviesaMurosNiPisosNiTechos">Los conductores no atraviesan paredes, pisos ni techos.</param>
/// <param name="AlturaDeLaDerivacionM">A qué altura del piso se hizo la derivación, en metros.</param>
/// <param name="DesconectadorIntegradoOAdyacente">240-21(b)(5)(3): la protección es parte integral
/// del medio de desconexión, o está inmediatamente adyacente a él.</param>
/// <param name="DesconectadorFacilmenteAccesible">240-21(b)(5)(4): el medio de desconexión está en
/// un lugar fácilmente accesible, en el exterior o lo más cerca posible del punto de entrada.</param>
/// <param name="AlimentaTransformador">La derivación alimenta el primario de un transformador, que
/// es lo que manda al caso 240-21(b)(3).</param>
/// <param name="RelacionTensionPrimarioSecundario">Vprimario / Vsecundario. Es el multiplicador de
/// 240-21(b)(3)(2) para la ampacidad mínima del secundario.</param>
/// <param name="AmpacidadSecundarioA">Ampacidad de los conductores alimentados por el secundario.</param>
/// <param name="LongitudPrimarioMasSecundarioM">Suma de las longitudes de primario y secundario,
/// excluyendo la parte del primario que ya esté protegida a su ampacidad — 240-21(b)(3)(3).</param>
public sealed record DatosDerivacion(
    decimal LongitudM,
    decimal AmpacidadDerivacionA,
    decimal ProteccionAlimentadorA,
    decimal CargaCalculadaA,
    decimal? ProteccionEnTerminacionA = null,
    bool? TerminaEnUnSoloDispositivo = null,
    bool? ProtegidaContraDanoFisico = null,
    bool? NoSeExtiendeMasAllaDelTablero = null,
    bool? EnCanalizacionDesdeLaDerivacion = null,
    bool SaleDeLaEnvolvente = false,
    bool EnExterior = false,
    MaterialConductor Material = MaterialConductor.Cobre,
    decimal? AreaMm2 = null,
    bool? PersonalCalificado = null,
    decimal? AlturaParedM = null,
    decimal? LongitudHorizontalM = null,
    bool? SinEmpalmes = null,
    bool? NoAtraviesaMurosNiPisosNiTechos = null,
    decimal? AlturaDeLaDerivacionM = null,
    bool? DesconectadorIntegradoOAdyacente = null,
    bool? DesconectadorFacilmenteAccesible = null,
    bool AlimentaTransformador = false,
    decimal? RelacionTensionPrimarioSecundario = null,
    decimal? AmpacidadSecundarioA = null,
    decimal? LongitudPrimarioMasSecundarioM = null)
{
    /// <summary>Un tercio del valor nominal de la protección del alimentador — 240-21(b)(2)(1),
    /// (b)(3)(1) y (b)(4)(3) piden exactamente esto.</summary>
    public decimal TercioDeLaProteccionA => ProteccionAlimentadorA / 3m;

    /// <summary>Un décimo del valor nominal de la protección del alimentador — 240-21(b)(1)(4).</summary>
    public decimal DecimoDeLaProteccionA => ProteccionAlimentadorA / 10m;
}
