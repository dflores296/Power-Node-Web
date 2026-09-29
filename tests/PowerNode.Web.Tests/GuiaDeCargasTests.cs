using System.Text.RegularExpressions;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>La guía de cargas es la fuente única</b> — I-126, <c>docs/decisiones/cargas-y-clases-de-circuito.md</c>.
/// La página la dibuja y el selector del tipo de cada carga saldrá de ella: si le falta un tipo o un
/// subtipo, o dos ramas comparten ancla, el «?» lleva a la rama equivocada o a ninguna.
/// </summary>
public class GuiaDeCargasTests
{
    [Fact]
    public void Cada_tipo_de_carga_tiene_su_rama()
    {
        foreach (var tipo in Enum.GetValues<CategoriaDeCarga>())
        {
            var rama = GuiaDeCargas.DeTipo(tipo);
            Assert.Equal(tipo, rama.Tipo);
            Assert.NotEmpty(rama.Ramas);
        }
    }

    [Fact]
    public void Cada_subtipo_aparece_una_vez_y_bajo_su_tipo()
    {
        var subtipos = GuiaDeCargas.Todas().Where(r => r.Subtipo is not null).Select(r => r.Subtipo!.Value).ToList();
        Assert.Equal(subtipos.Count, subtipos.Distinct().Count());
        Assert.Equal(Enum.GetValues<SubtipoDeCarga>().Order(), subtipos.Order());

        foreach (var tipo in GuiaDeCargas.Tipos.Where(t => t.Tipo is not null))
            foreach (var sub in tipo.Ramas)
            {
                Assert.Equal(tipo.Tipo, sub.Tipo);
                Assert.Equal(tipo.Tipo, sub.Subtipo!.Value.Tipo());
            }
    }

    [Fact]
    public void El_tablero_alimentado_es_una_rama_de_primer_nivel_sin_tipo_todavia()
    {
        var tablero = GuiaDeCargas.DeSubtipo(SubtipoDeCarga.TableroAlimentado);
        Assert.Contains(tablero, GuiaDeCargas.Tipos);
        Assert.Null(SubtipoDeCarga.TableroAlimentado.Tipo());
        Assert.Contains(tablero.Referencias, r => r.Cita == "220-40");
    }

    [Fact]
    public void El_nombre_del_subtipo_es_el_de_su_rama()
    {
        foreach (var s in Enum.GetValues<SubtipoDeCarga>())
            Assert.Equal(s.Nombre(), GuiaDeCargas.DeSubtipo(s).Nombre);
    }

    [Fact]
    public void Las_anclas_son_unicas_y_no_chocan_con_las_secciones_de_la_pagina()
    {
        var ids = GuiaDeCargas.Todas().Select(r => r.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.All(ids, id => Assert.Matches(new Regex("^[a-z0-9]+(-[a-z0-9]+)*$"), id));
        // Guia.razor: las secciones son «rama-tipos», «rama-clases» y «rama-glosario».
        Assert.DoesNotContain("tipos", ids);
        Assert.DoesNotContain("clases", ids);
        Assert.DoesNotContain("glosario", ids);
    }

    [Fact]
    public void Cada_rama_cita_la_norma()
    {
        foreach (var rama in GuiaDeCargas.Todas())
        {
            Assert.False(string.IsNullOrWhiteSpace(rama.Nombre), rama.Id);
            Assert.False(string.IsNullOrWhiteSpace(rama.Que), rama.Id);
            Assert.NotEmpty(rama.Referencias);
            Assert.All(rama.Referencias, r =>
            {
                Assert.False(string.IsNullOrWhiteSpace(r.Cita), rama.Id);
                Assert.False(string.IsNullOrWhiteSpace(r.Regla), rama.Id);
            });
            Assert.NotEqual(rama.Nombre, rama.NombreNom); // si es igual, va null
        }
    }

    [Fact]
    public void Cada_tipo_cita_la_carga_el_circuito_y_el_alimentador()
    {
        foreach (var tipo in GuiaDeCargas.Tipos.Where(t => t.Tipo is not null))
        {
            var alcances = tipo.Referencias.Concat(tipo.Ramas.SelectMany(r => r.Referencias)).Select(r => r.Alcance).ToHashSet();
            Assert.Equal(Enum.GetValues<AlcanceDeReferencia>().ToHashSet(), alcances);
        }
    }

    [Fact]
    public void El_glosario_no_repite_terminos_y_trae_los_nombres_de_la_norma()
    {
        var cortos = GuiaDeCargas.Glosario.Select(t => t.Corto).ToList();
        Assert.Equal(cortos.Count, cortos.Distinct().Count());
        Assert.All(GuiaDeCargas.Glosario, t => Assert.False(string.IsNullOrWhiteSpace(t.Nom)));
        foreach (var clase in new[] { "Individual", "Uso general", "Para aparatos", "Multiconductor", "Combinadas" })
            Assert.Contains(clase, cortos);
        foreach (var tipo in GuiaDeCargas.Tipos)
            Assert.Contains(tipo.Nombre, cortos);
    }
}
