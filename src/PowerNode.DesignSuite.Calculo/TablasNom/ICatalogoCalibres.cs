using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>Tabla 8 — Propiedades de los conductores. El catálogo de los 40 calibres estándar.</summary>
public interface ICatalogoCalibres
{
    IReadOnlyList<Calibre> Listar();

    /// <summary>Busca por designación exacta ("12", "1/0", "250"). Null si no es un calibre estándar.</summary>
    Calibre? BuscarPorDesignacion(string designacion);

    /// <summary>El calibre estándar más chico cuya área sea &gt;= areaMm2 — para cuando la caída de tensión exige un área mínima.</summary>
    Calibre BuscarPorAreaMinima(decimal areaMm2);

    /// <summary>El calibre inmediato superior al dado, en la secuencia estándar de la Tabla 8. Null si ya es el mayor.</summary>
    Calibre? Siguiente(Calibre calibre);
}
