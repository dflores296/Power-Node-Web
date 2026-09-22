using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Corriente a plena carga (FLC) de motores por tabla — 430-247 (CD), 430-248 (1φ), 430-249 (2φ),
/// 430-250 (3φ). 430-6(a) exige usar el valor de tabla, no el de placa, para dimensionar
/// conductor y protección (la placa se reserva para el relevador de sobrecarga térmica).
/// </summary>
public interface ITablaFlcMotor
{
    /// <param name="tensionV">
    /// Tensión del <b>sistema</b>. Las columnas de las tablas son tensiones <b>nominales del
    /// motor</b>, y las tres tablas de corriente alterna declaran en su introducción los intervalos
    /// de sistema que cada columna cubre (110-120, 220-240, 440-480, 550-600) — o sea que un motor
    /// en 220 V resuelve por la columna de 230 V. La implementación hace esa traducción; quien
    /// llama pasa la tensión tal como la capturó.
    /// </param>
    /// <summary>Null si la tabla no trae fila para ese Hp, o si la tensión no cae en ninguna columna ni intervalo.</summary>
    decimal? CorrientePlenaCargaA(decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionV);

    /// <summary>
    /// La errata del texto publicado que se aplicó para resolver esa consulta, o null si no hubo
    /// ninguna — que es el caso normal. Ver <see cref="ErratasDeLaNorma"/>.
    ///
    /// <para>
    /// Tiene implementación por omisión (<c>null</c>) a propósito: una tabla de prueba armada a mano
    /// no tiene erratas que declarar, y obligarla a decirlo no aportaría nada. Lo que no puede pasar
    /// es lo contrario — que una implementación aplique una errata y no la reporte—, y de eso se
    /// encarga la implementación real, que hace las dos cosas en el mismo lugar.
    /// </para>
    /// </summary>
    ErrataDeCelda? ErrataAplicada(decimal hp, TipoAlimentacionMotor tipoAlimentacion, decimal tensionV) => null;
}
