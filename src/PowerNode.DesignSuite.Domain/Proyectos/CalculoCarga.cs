namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Resultado propio de una <see cref="Carga"/>, simétrico con <see cref="CalculoTablero"/>.
///
/// <b>Existe para que ninguna carga quede jamás sin protección en el modelo.</b> Se le preguntó al
/// usuario si el sistema debía *exigir* un elemento <see cref="Proteccion"/> antes de cada carga, y la
/// respuesta salió del corpus: <b>no</b>, por dos razones. 240-21 pone la protección "en el punto en
/// el que los conductores reciben su alimentación" — o sea en el tablero de arriba, no junto al
/// chiller — y tiene ocho excepciones (a) a (h), las derivaciones, así que un requisito duro
/// contradiría a la norma misma. En vez de exigirlo, cada carga calcula su propia protección aquí,
/// igual que un tablero calcula la de su alimentador entrante. Poner un elemento `Proteccion` sigue
/// siendo opcional, para cuando es un aparato físico aparte.
/// </summary>
public class CalculoCarga
{
    public int Id { get; set; }
    public int CargaId { get; set; }
    public Carga Carga { get; set; } = null!;

    /// <summary>
    /// La corriente que gobierna el cálculo, según el régimen: VA/tensión en genérica, la FLC de
    /// tabla (o de placa) en Motor430, y la mayor entre corriente nominal y de selección del
    /// circuito derivado en el 440.
    /// </summary>
    public decimal CorrienteDisenoA { get; set; }

    /// <summary>
    /// La corriente con la que se dimensiona el conductor que la alimenta: 125 % en carga continua
    /// (215-2), en motor (430-22) y en motocompresor (440-32). Es distinta de
    /// <see cref="CorrienteDisenoA"/> y es la que debe usar el <see cref="Alimentador"/>.
    /// </summary>
    public decimal CorrienteConductorA { get; set; }

    /// <summary>
    /// Protección contra <b>cortocircuito y falla a tierra</b> del circuito derivado — 240-6(a) en
    /// genérica, 430-52 en motor, 440-22(a) en motocompresor.
    /// </summary>
    public decimal ProteccionCortocircuitoA { get; set; }

    /// <summary>
    /// Protección contra <b>sobrecarga</b>: el relevador térmico del arrancador — 430-32 en motor,
    /// 440-52(a)(1) en motocompresor. <b>Null en carga genérica</b>, donde no hay dos aparatos.
    ///
    /// Son dos cosas distintas y por eso son dos campos. En un motor la protección del circuito
    /// derivado (430-52) puede exceder por diseño la ampacidad del conductor, y quien de verdad
    /// protege contra sobrecarga es este otro dispositivo. Reportar un solo número sería mentir
    /// sobre qué aparato hace qué.
    /// </summary>
    public decimal? ProteccionSobrecargaA { get; set; }

    /// <summary>
    /// Solo en el régimen 440: el techo del 225 % de 440-22(a), hasta donde se permite subir la
    /// protección si la del 175 % no conduce la corriente de arranque. Se guarda porque es el dato
    /// que el ingeniero necesita cuando el equipo dispara al arrancar.
    /// </summary>
    public decimal? ProteccionMaximaPermitidaA { get; set; }

    public string TextoMemoria { get; set; } = string.Empty;
}
