using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Centro de control de motores — Art. 430, <b>Parte H</b> (430-92 a 430-98). El 430-92 lo dice
/// textual: <i>"La Parte H trata de los centros de control de motores instalados para el control de
/// motores, alumbrado y circuitos de potencia."</i> Es entidad con nombre propio en la norma, no una
/// variante de tablero.
///
/// <b>Por qué NO es un <see cref="Tablero"/> con <see cref="TipoTablero.Fuerza"/>.</b> Un tablero de
/// alumbrado y distribución reparte fases por la convención NEMA de pares (1‑2→A, 3‑4→B, 5‑6→C), que
/// es lo que implementa <c>DistribucionBarras</c>. Un CCM no: <b>430-97(b)</b> manda que la
/// disposición de fases en las barras comunes sea <i>"A, B y C del frente hacia atrás, de arriba
/// hacia abajo o de izquierda a derecha"</i>. Meterlo como Tablero le asignaría la fase equivocada a
/// cada unidad — bug silencioso de la misma familia que el bloque 10.6 vino a arreglar.
///
/// <b>Sus hijos son <see cref="Carga"/> directas, no una entidad "gaveta" aparte.</b> Lo que la
/// norma llama <i>unidad de control de motores</i> (430-98(b)) es eléctricamente un arrancador:
/// desconectador + protección contra cortocircuito + contactor + relevador de sobrecarga. Eso es
/// exactamente lo que <c>CalculoCarga</c> ya resuelve — las <b>dos</b> protecciones, 430-52 y
/// 430-32. Así que la unidad no necesita entidad propia: lo eléctrico ya está en la Carga y lo
/// físico (en qué sección va, qué altura ocupa) vive en <see cref="Carga.UnidadControl"/>. La parte
/// de <b>control</b> (contactores, relevadores, PLC, variadores) queda fuera a propósito: no cambia
/// el dimensionamiento de conductores ni de protecciones, que es lo que este programa calcula.
///
/// <b>Lo que hereda gratis:</b> la cascada ya implementa 430-24 (125 % del FLC mayor + 100 % del
/// resto) vía <c>AgregadoMotores</c>. Un CCM es literalmente ese caso, así que agrega igual que
/// cualquier otro elemento con hijos y no hubo que tocar el motor de cálculo.
/// </summary>
public class CentroControlMotores : ElementoTopologia
{
    /// <summary>Tensión entre fases de las barras. Comparte columna con <see cref="Tablero.TensionV"/> en la tabla TPH.</summary>
    public decimal TensionV { get; set; }

    /// <summary>1, 2 o 3. Misma convención que <see cref="Tablero.Fases"/> (ver "Magnitudes" en PLAN-V1.md).</summary>
    public int Fases { get; set; } = 3;

    public int Hilos { get; set; } = 4;

    /// <summary>
    /// <b>Valor nominal de la barra conductora común de potencia</b>, en amperes. Es el dato central
    /// de la Parte H y no es cosmético: <b>430-94</b> dice que el valor nominal o el ajuste del
    /// dispositivo de protección contra sobrecorriente <i>"no debe exceder el valor nominal de la
    /// barra conductora común de potencia"</i>. Contra esto se compara la protección que calcula la
    /// cascada.
    ///
    /// Además, <b>430-98(a)</b> exige que este valor venga marcado en el equipo, así que siempre es
    /// un dato que existe físicamente. 0 = no capturado, y entonces la verificación no se hace y lo
    /// dice — igual que con los kA de un tablero, no se supone un valor.
    /// </summary>
    public decimal CorrienteBarrasA { get; set; }

    /// <summary>
    /// Valor nominal de cortocircuito del CCM, en kA — el otro dato que <b>430-98(a)</b> obliga a
    /// marcar en el equipo. Es lo que se compara contra la corriente de falla disponible en su punto.
    /// Null = no capturada; ver <see cref="Tablero.CapacidadInterruptivaKa"/> para por qué no se
    /// supone un valor.
    /// </summary>
    public decimal? CapacidadInterruptivaKa { get; set; }

    /// <summary>
    /// Dónde está el dispositivo que lo protege. <b>430-94 admite las dos</b> y son decisión de
    /// diseño, no algo deducible: (1) antes del centro de control de motores, o (2) un principal
    /// dentro de él. Ver <see cref="UbicacionProteccionCcm"/>.
    /// </summary>
    public UbicacionProteccionCcm UbicacionProteccion { get; set; } = UbicacionProteccionCcm.AguasArriba;

    /// <summary>
    /// Marca que este CCM se usa como equipo de acometida. <b>430-95</b> le exige entonces
    /// <i>"un solo medio principal de desconexión que desconecte todos los conductores de fase de
    /// acometida"</i>, cosa que la cascada verifica contra <see cref="UbicacionProteccion"/>.
    /// </summary>
    public bool EsEquipoAcometida { get; set; }

    /// <summary>
    /// Cuántas secciones verticales tiene el gabinete. Dato físico, para el acomodo de unidades y
    /// para el entregable; no entra en ningún cálculo eléctrico. 0 = no capturado.
    /// </summary>
    public int NumeroSecciones { get; set; }

    /// <summary>
    /// Tipo de gabinete, de las opciones del Formulario CCM del compendiado (2/16). Dato de captura
    /// y de entregable; no entra en ningún cálculo.
    /// </summary>
    public TipoGabineteCcm TipoGabinete { get; set; } = TipoGabineteCcm.Nema1;

    /// <summary>
    /// Línea de producto. <b>Decide si corren las validaciones de rango del fabricante</b>, que son
    /// de ese producto y no de la norma — ver <see cref="LineaCcm"/>. Por omisión no se declara
    /// ninguna, y entonces solo se le verifica la NOM.
    /// </summary>
    public LineaCcm Linea { get; set; } = LineaCcm.NoEspecificada;

    /// <summary>
    /// Número de catálogo, cuando lo haya. <b>Ojo: un CCM normalmente NO tiene uno.</b> A diferencia
    /// de un tablero NQ/NF o de un transformador seco, un CCM no es producto de línea — se manda
    /// hacer, y por eso el compendiado publica un <i>formulario</i> para pedirlo en vez de una tabla
    /// de modelos. El campo existe por si el proyecto trae una clave del proveedor.
    /// </summary>
    public string? ModeloCatalogo { get; set; }

    /// <summary>Resultado propio del CCM: su protección, la verificación de 430-94 y la de cortocircuito.</summary>
    public CalculoCcm? Calculo { get; set; }
}
