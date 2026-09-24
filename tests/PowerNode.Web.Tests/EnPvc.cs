using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// Toda canalización nace como tubo EMT (David, 2026-09-24), pero los casos de referencia —el
/// escritorio y el documento de pruebas del 2026-09-23— se calcularon con la Tabla 9 en PVC. Las
/// suites que los documentan hacen nacer los tubos en PVC Cédula 40, que era el valor por omisión
/// cuando se anotaron, y pasan a PVC los que ya existen y el del alimentador.
/// </summary>
internal static class EnPvc
{
    public static CuadroDeCarga Todo(CuadroDeCarga cuadro)
    {
        cuadro.Datos.TuboAlNacer = TipoTuboConduit.PvcCedula40;
        foreach (var tubo in cuadro.Datos.Canalizaciones)
            tubo.Tubo = TipoTuboConduit.PvcCedula40;
        cuadro.Datos.CanalizacionAlimentador.Tubo = TipoTuboConduit.PvcCedula40;
        cuadro.Recalcular();
        return cuadro;
    }
}
