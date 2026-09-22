using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>Tabla 250-122 — calibre mínimo del conductor de puesta a tierra de equipos, por el amperaje de la protección.</summary>
public interface ITablaPuestaTierra
{
    /// <summary>Calibre mínimo para una protección de amperesProteccion. Busca la primera fila cuya capacidad sea &gt;= amperesProteccion.</summary>
    Calibre CalibreMinimo(decimal amperesProteccion, MaterialConductor material);
}
