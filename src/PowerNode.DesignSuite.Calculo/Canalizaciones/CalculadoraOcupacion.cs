using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Calculo.Canalizaciones;

public enum PapelConductor { Fase, Neutro, Tierra }

/// <summary>Uno o varios conductores iguales dentro de una canalización.</summary>
/// <param name="TipoAislamiento">Null: conductor desnudo (Tabla 8).</param>
/// <param name="DiametroFabricanteMm">El diámetro exterior del fabricante, para un aislamiento que no
/// está en la Tabla 5 (Nota 5 del Capítulo 10). Si viene, manda sobre la Tabla 5.</param>
public sealed record ConductorEnCanalizacion(
    string Circuito,
    PapelConductor Papel,
    string Designacion,
    string? TipoAislamiento,
    decimal? DiametroFabricanteMm = null,
    int Cantidad = 1)
{
    public string Descripcion =>
        $"{Cantidad} × {Unidades.Calibre.UnidadDe(Designacion)} {(TipoAislamiento ?? "desnudo")}";
}

/// <param name="AreaUnitariaMm2">Null cuando falta el dato (aislamiento fuera de la Tabla 5 sin diámetro).</param>
/// <param name="Fuente">«Tabla 5», «Tabla 8», «diámetro del fabricante (Nota 5)».</param>
public sealed record RenglonDeOcupacion(ConductorEnCanalizacion Conductor, decimal? DiametroMm, decimal? AreaUnitariaMm2, string Fuente)
{
    public decimal? AreaMm2 => AreaUnitariaMm2 * Conductor.Cantidad;
}

/// <param name="Tamano">El tubo elegido o fijado (tubo y niple). Null en ductos, canales y
/// superficiales, o si falta un dato o nada alcanza.</param>
/// <param name="AreaDisponibleMm2">Lo que se puede ocupar: la columna de la Tabla 4, o el 20 % del
/// área interior capturada en ductos y canales.</param>
/// <param name="AreaMinimaMm2">Ductos y canales sin dimensiones: el área interior mínima que hace
/// falta (la suma entre 0.20).</param>
/// <param name="TamanoCalculado">Tubo y niple: el que elige el cálculo, aunque el diseñador haya
/// fijado otro. Null si nada alcanza.</param>
public sealed record ResultadoOcupacion(
    IReadOnlyList<RenglonDeOcupacion> Renglones,
    int NumeroConductores,
    decimal? AreaTotalMm2,
    decimal PorcentajePermitido,
    TamanoDeTubo? Tamano,
    decimal? AreaDisponibleMm2,
    decimal? AreaMinimaMm2,
    decimal? OcupacionPct,
    bool Nota2Aplicada,
    bool Excede,
    IReadOnlyList<string> Faltantes,
    IReadOnlyList<string> Avisos,
    IReadOnlyList<Cita> Citas,
    TamanoDeTubo? TamanoCalculado = null);

/// <summary>
/// <b>El tamaño de la canalización</b> — Capítulo 10. NACIDO EN LA WEB (2026-09-24).
///
/// <list type="number">
/// <item>Área de cada conductor: Tabla 5 si es aislado, Tabla 8 si es desnudo (Nota 8), π·d²/4 con
/// el diámetro del fabricante si su aislamiento no está en la Tabla 5 (Nota 5). Para calibres
/// mezclados, Tablas 5 y 4 (Nota 6).</item>
/// <item>Se cuentan <b>todos</b>: fases, neutros y tierras (Nota 3).</item>
/// <item>Tubo: el porcentaje de la Tabla 1 (53, 31, 40) y el tamaño más chico de la Tabla 4 cuya
/// columna alcance. Niple de 60 cm o menos: 60 % (Nota 4).</item>
/// <item>Nota 2 de la Tabla 1: con tres conductores, si el diámetro interior del tubo entre el del
/// conductor queda entre 2.8 y 3.2, se pueden atascar y <b>se debe</b> usar el tamaño inmediato
/// superior.</item>
/// <item>Ductos (376-22, 378-22) y canales auxiliares (366-22): 20 % del área interior.
/// Superficiales (386-22, 388-22): no más conductores de los que marca el fabricante.</item>
/// </list>
/// </summary>
public class CalculadoraOcupacion(ITablaOcupacion ocupacion, ITablaTuboConduit tubos, ITablaDimensionesConductor dimensiones)
{
    public const decimal PorcentajeNiple = 60m;
    public const decimal PorcentajeDuctos = 20m;

    public ResultadoOcupacion Calcular(
        TipoCanalizacion tipo,
        TipoTuboConduit tubo,
        IReadOnlyList<ConductorEnCanalizacion> conductores,
        decimal? anchoMm = null,
        decimal? altoMm = null,
        decimal? areaInteriorMm2 = null,
        int? maxConductoresFabricante = null,
        int? tamanoFijado = null)
    {
        var citas = new List<Cita>();
        var avisos = new List<string>();
        var faltantes = new List<string>();
        var renglones = conductores.Select(c => Renglon(c, faltantes)).ToList();
        var n = conductores.Sum(c => c.Cantidad);
        citas.Add(new Cita("Capítulo 10, Nota 3", $"Se cuentan todos los conductores, incluida la puesta a tierra: {n}."));

        if (faltantes.Count > 0 || n == 0)
        {
            avisos.AddRange(faltantes.Select(f => $"Falta el diámetro exterior del fabricante de {f} (Capítulo 10, Nota 5): sin él no se calcula la canalización."));
            return new ResultadoOcupacion(renglones, n, null, 0m, null, null, null, null, false, false, faltantes, avisos, citas);
        }

        // Una errata que se aplica llega al papel, una sola vez — ErratasDeLaNorma.
        foreach (var e in renglones
                     .Where(r => r.Conductor.TipoAislamiento is not null && r.Fuente.StartsWith("Tabla 5, con errata"))
                     .Select(r => dimensiones.ErrataAplicada(r.Conductor.Designacion, r.Conductor.TipoAislamiento!)!)
                     .Distinct())
            citas.Add(new Cita($"ERRATA Tabla {e.TablaId}",
                $"{e.Descripcion}: se calcula con {e.ValorCorregido} en lugar de los {e.ValorPublicado} publicados en el DOF, porque {e.Sustento}."));

        var total = renglones.Sum(r => r.AreaMm2!.Value);
        citas.Add(new Cita("Capítulo 10, Tablas 5 y 8", $"Suma de las áreas de los conductores: {total:N2} mm²."));

        if (tipo.EsTubo())
            return EnTubo(tipo, tubo, renglones, n, total, tamanoFijado, citas, avisos);

        if (tipo.EsDuctoOCanal())
        {
            var referencia = tipo switch
            {
                TipoCanalizacion.DuctoMetalico => "376-22(a)",
                TipoCanalizacion.DuctoNoMetalico => "378-22",
                TipoCanalizacion.CanalAuxiliarMetalico => "366-22(a)",
                _ => "366-22(b)",
            };
            var minima = total * 100m / PorcentajeDuctos;
            if (anchoMm is not > 0m || altoMm is not > 0m)
            {
                citas.Add(new Cita(referencia, $"20 % del área interior: hace falta un área interior de al menos {minima:N0} mm²."));
                return new ResultadoOcupacion(renglones, n, total, PorcentajeDuctos, null, null, minima, null, false, false, [], avisos, citas);
            }

            var interior = anchoMm.Value * altoMm.Value;
            var disponible = interior * PorcentajeDuctos / 100m;
            var pct = total * 100m / interior;
            var excede = total > disponible;
            citas.Add(new Cita(referencia, $"{anchoMm:N0} × {altoMm:N0} mm = {interior:N0} mm²; al 20 %: {disponible:N0} mm². Ocupación: {pct:N1} %."));
            if (excede)
                avisos.Add($"{tipo.Nombre()} de {anchoMm:N0} × {altoMm:N0} mm: los conductores ocupan {pct:N1} %, más del 20 % que permite {referencia}. Hace falta al menos {minima:N0} mm² de área interior.");
            return new ResultadoOcupacion(renglones, n, total, PorcentajeDuctos, null, disponible, minima, pct, false, excede, [], avisos, citas);
        }

        // Superficiales: el número del fabricante.
        var referenciaSup = tipo == TipoCanalizacion.SuperficialMetalica ? "386-22" : "388-22";
        var excedeSup = maxConductoresFabricante is { } max && n > max;
        decimal? pctSup = areaInteriorMm2 is > 0m ? total * 100m / areaInteriorMm2.Value : null;
        citas.Add(new Cita(referenciaSup, maxConductoresFabricante is { } m
            ? $"{n} conductores; el fabricante permite {m}."
            : $"{n} conductores: el número máximo lo marca el fabricante."));
        if (excedeSup)
            avisos.Add($"{tipo.Nombre()}: {n} conductores, más de los {maxConductoresFabricante} para los que está diseñada — {referenciaSup}.");
        if (maxConductoresFabricante is null)
            avisos.Add($"{tipo.Nombre()}: captura el número máximo de conductores que marca el fabricante — {referenciaSup}.");
        return new ResultadoOcupacion(renglones, n, total, 0m, null, null, null, pctSup, false, excedeSup, [], avisos, citas);
    }

    private ResultadoOcupacion EnTubo(
        TipoCanalizacion tipo, TipoTuboConduit tubo, List<RenglonDeOcupacion> renglones, int n, decimal total,
        int? tamanoFijado, List<Cita> citas, List<string> avisos)
    {
        var niple = tipo == TipoCanalizacion.Niple;
        var pct = niple ? PorcentajeNiple : ocupacion.PorcentajeMaximo(n);
        citas.Add(niple
            ? new Cita("Capítulo 10, Nota 4", $"Niple de 60 cm o menos: hasta {PorcentajeNiple} % de su sección.")
            : new Cita("Capítulo 10, Tabla 1", $"{n} conductor{(n == 1 ? "" : "es")}: hasta {pct} % de la sección."));

        var tamanos = tubos.Tamanos(tubo);
        var diametroMayor = renglones.Max(r => r.DiametroMm ?? 0m);
        bool Atasca(TamanoDeTubo t) =>
            n == 3 && diametroMayor > 0m && t.DiametroInteriorMm / diametroMayor is >= 2.8m and <= 3.2m;

        // El que elige el cálculo: el más chico que alcanza, y el siguiente si se atasca (Nota 2).
        // Se calcula también con un tamaño fijado, para que la pantalla diga cuál sería.
        var calculado = tamanos.FirstOrDefault(t => t.AreaDisponible(pct) >= total);
        Cita? citaNota2 = null;
        if (calculado is not null && Atasca(calculado))
        {
            var i = tamanos.ToList().IndexOf(calculado);
            citaNota2 = new Cita("Capítulo 10, Tabla 1, Nota 2",
                $"{tubo.Corto()} {calculado.Rotulo}: {calculado.DiametroInteriorMm:N2} mm ÷ {diametroMayor:N2} mm = {calculado.DiametroInteriorMm / diametroMayor:N2}, entre 2.8 y 3.2: se sube al tamaño inmediato superior.");
            calculado = i + 1 < tamanos.Count ? tamanos[i + 1] : null;
        }

        TamanoDeTubo? elegido;
        var nota2 = false;
        if (tamanoFijado is { } fijo)
        {
            elegido = tamanos.FirstOrDefault(t => t.DesignacionMetrica == fijo)
                ?? throw new InvalidOperationException($"{tubo.Nombre()} no tiene designación métrica {fijo} en la Tabla 4.");
            if (Atasca(elegido))
                avisos.Add($"{tubo.Corto()} {elegido.Rotulo} con 3 conductores: la relación de diámetros es {elegido.DiametroInteriorMm / diametroMayor:N2}, entre 2.8 y 3.2; se pueden atascar y se debe usar el tamaño inmediato superior — Nota 2 de la Tabla 1.");
        }
        else
        {
            elegido = calculado;
            if (citaNota2 is not null)
            {
                nota2 = true;
                citas.Add(citaNota2);
            }
        }

        if (elegido is null)
        {
            var mayor = tamanos[^1];
            avisos.Add($"Ningún {tubo.Corto()} de la Tabla 4 alcanza: {total:N0} mm² contra {mayor.AreaDisponible(pct):N0} mm² del {mayor.Rotulo}. Reparte los conductores en más de una canalización.");
            return new ResultadoOcupacion(renglones, n, total, pct, null, null, null, null, nota2, true, [], avisos, citas, calculado);
        }

        var disponible = elegido.AreaDisponible(pct);
        var ocupacionPct = total * 100m / elegido.Area100Mm2;
        var excede = total > disponible;
        citas.Add(new Cita("Capítulo 10, Tabla 4",
            $"{tubo.Corto()} {elegido.Rotulo}: {disponible:N0} mm² al {pct} % ≥ {total:N2} mm². Ocupación: {ocupacionPct:N1} %."));
        if (excede)
            avisos.Add($"{tubo.Corto()} {elegido.Rotulo} fijado: los conductores suman {total:N0} mm² y el tubo admite {disponible:N0} mm² al {pct} % — Tabla 1 del Capítulo 10.");

        return new ResultadoOcupacion(renglones, n, total, pct, elegido, disponible, null, ocupacionPct, nota2, excede, [], avisos, citas, calculado);
    }

    private RenglonDeOcupacion Renglon(ConductorEnCanalizacion c, List<string> faltantes)
    {
        if (c.TipoAislamiento is null)
        {
            var d = dimensiones.Desnudo(c.Designacion)
                ?? throw new InvalidOperationException($"La Tabla 8 no trae el calibre {c.Designacion}.");
            return new RenglonDeOcupacion(c, d.DiametroMm, d.AreaMm2, "Tabla 8 (desnudo)");
        }

        if (c.DiametroFabricanteMm is > 0m and var diametro)
        {
            var area = (decimal)Math.PI * diametro * diametro / 4m;
            return new RenglonDeOcupacion(c, diametro, Math.Round(area, 3), "diámetro del fabricante (Nota 5)");
        }

        if (dimensiones.Aislado(c.Designacion, c.TipoAislamiento) is { } t5)
            return new RenglonDeOcupacion(c, t5.DiametroMm, t5.AreaMm2,
                dimensiones.ErrataAplicada(c.Designacion, c.TipoAislamiento) is not null
                    ? "Tabla 5, con errata (ver la cita)"
                    : "Tabla 5");

        var falta = $"{Unidades.Calibre.UnidadDe(c.Designacion)} {c.TipoAislamiento}";
        if (!faltantes.Contains(falta)) faltantes.Add(falta);
        return new RenglonDeOcupacion(c, null, null, "falta el diámetro del fabricante (Nota 5)");
    }
}
