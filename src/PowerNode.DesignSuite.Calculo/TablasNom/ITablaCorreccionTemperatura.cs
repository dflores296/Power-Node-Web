using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>Tabla 310-15(b)(2)(a) — factor de corrección de ampacidad por temperatura ambiente distinta de 30°C.</summary>
public interface ITablaCorreccionTemperatura
{
    /// <summary>Factor para la temperatura ambiente y columna de temperatura del conductor dadas. Null fuera de rango de la tabla.</summary>
    decimal? Factor(decimal temperaturaAmbienteC, TemperaturaAislamiento temperatura);

    /// <summary>
    /// La errata del texto publicado que se aplicó para resolver esa consulta, o null si no hubo
    /// ninguna — que es el caso normal. Ver <see cref="ErratasDeLaNorma"/>.
    ///
    /// <para>
    /// Tiene implementación por omisión (<c>null</c>) a propósito, igual que en
    /// <see cref="ITablaFlcMotor"/>: una tabla de prueba armada a mano no tiene erratas que declarar.
    /// Lo que no puede pasar es lo contrario —que una implementación aplique una errata y no la
    /// reporte—, y de eso se encarga la implementación real, que hace las dos cosas en el mismo sitio.
    /// </para>
    /// </summary>
    ErrataDeRotulo? ErrataAplicada(decimal temperaturaAmbienteC) => null;
}
