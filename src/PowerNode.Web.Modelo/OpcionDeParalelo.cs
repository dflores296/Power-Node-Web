using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Una forma de correr el alimentador: N conductores por fase</b> — 310-10(h)(1), I-162. Lo que sale con
/// ese N fijado, para comparar contra el automático. El cobre es el de todos los conductores que se
/// instalan: (fases + neutro) × N más las tierras (N, o una con «Paralelos en un tubo»), en mm² de la Tabla 8; con el neutro reducido por 220-61 si se
/// pidió.
/// </summary>
public sealed record OpcionDeParalelo(
    int PorFase,
    Calibre Fase,
    Calibre Neutro,
    Calibre Tierra,
    decimal AmpacidadA,
    decimal CaidaPct,
    string Canalizacion,
    decimal CobreMm2,
    bool EsAutomatico = false,
    // R4-3: un N fijado que solo cumple con 1/0 AWG, el mínimo en paralelo — 310-10(h)(1).
    bool PorPisoDeParalelo = false,
    // Cumple por ampacidad, pero el llenado de su canalización no se pudo verificar: falta el diámetro del
    // fabricante de este conductor (Capítulo 10, Nota 5). Null si se verificó.
    string? LlenadoSinVerificar = null);
