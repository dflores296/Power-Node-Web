using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Lo de criterio de la auditoría de motores, decidido por David el 2026-10-05</b> — AM-4 (M-21), AM-5 (M-22),
/// AM-7 (M-23) y AM-11 (I-189). Cada decisión está en <c>docs/decisiones/</c>; aquí, los casos con que se tomó.
/// </summary>
public class CriterioDeMotores20261005Tests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int espacio) =>
        cuadro.Circuitos.Single(x => x.Espacio == espacio);

    // ---- AM-5 · M-22: 430-62(b) con el conductor mínimo -------------------------------------------------

    /// <summary>
    /// El caso A de la auditoría: vivienda, 2F-3H 220/127 V, cobre THHN, 20 m, terminales de 60 °C, centro de
    /// carga NEMA. En la fase A: bomba de 1/2 HP a 127 V (8.9 A), hidroneumático de 1 HP a 220 V (8 A), minisplits
    /// de 1 TR (MCA 10.5 A, MOCP 15 A) y de 2 TR (MCA 19.5 A, MOCP 30 A) y la lavadora (1500 VA); en la B, además del
    /// hidroneumático, el refrigerador (3.5 A) y la bomba de la alberca (placa de 10 A, sin HP).
    /// </summary>
    private static CuadroDeCarga ElCasoA(SerieDeInterruptores serie = SerieDeInterruptores.CentroDeCargaNema, decimal longitudM = 20m)
    {
        var cuadro = new CuadroDeCarga(Motor);
        var d = cuadro.Datos;
        d.NumeroEspacios = 20;
        d.Fases = 2;
        d.Hilos = 3;
        d.TensionFaseFaseV = 220m;
        d.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        d.SerieInterruptores = serie;
        d.LongitudAlimentadorM = longitudM;
        cuadro.Recalcular();

        var bomba = Espacio(cuadro, 1);
        bomba.Categoria = CategoriaDeCarga.Motor;
        bomba.Hp = 0.5m;
        var hidroneumatico = Espacio(cuadro, 3);
        hidroneumatico.Categoria = CategoriaDeCarga.Motor;
        hidroneumatico.Hp = 1m;
        Assert.Null(cuadro.CambiarPolos(hidroneumatico, 2));
        foreach (var (espacio, mca, mocp) in new[] { (9, 10.5m, 15m), (13, 19.5m, 30m) })
        {
            var minisplit = Espacio(cuadro, espacio);
            minisplit.Categoria = CategoriaDeCarga.AireAcondicionado;
            minisplit.PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
            minisplit.AmpacidadMinimaA = mca;
            minisplit.ProteccionMaximaA = mocp;
        }
        var refrigerador = Espacio(cuadro, 15);
        refrigerador.Categoria = CategoriaDeCarga.Contactos;
        var r = refrigerador.AgregarCarga();
        r.Subtipo = SubtipoDeCarga.ContactoRefrigerador;
        r.Unidad = UnidadConsumo.Amperes;
        r.CargaUnitaria = 3.5m;
        var lavadora = Espacio(cuadro, 17);
        lavadora.Categoria = CategoriaDeCarga.Contactos;
        var l = lavadora.AgregarCarga();
        l.Subtipo = SubtipoDeCarga.ContactoLavadora;
        l.CargaUnitaria = 1500m;
        var alberca = Espacio(cuadro, 19);
        alberca.Categoria = CategoriaDeCarga.Motor;
        alberca.CapturaMotor = CapturaDeMotor.Amperes;
        alberca.CorrientePlacaA = 10m;
        foreach (var c in cuadro.Circuitos.Where(c => c.TieneCarga))
            c.LongitudM = 20m;
        cuadro.Recalcular();
        return cuadro;
    }

    /// <summary>
    /// 430-24: 125 % × 8.9 A + 38 A = 49.13 A, más 11.81 A de la lavadora = 60.93 A → 70 A (240-6(a)); techo de
    /// 430-62(a) y 430-63: 30 A + 27.4 A + 11.81 A = 69.21 A. El conductor mínimo, 4 AWG, tiene 70 A a 60 °C: más
    /// que los 60.93 A, así que 430-62(b) permite los 70 A (lectura literal, A). La cita dice además las dos cosas
    /// que lo sostienen (B y lo que pidió David): que los 70 A son la ampacidad del 4 AWG y lo protegen (240-4), y que
    /// no hay tamaño estándar entre 60.93 y 69.21 A: el anterior, 60 A, queda debajo de 430-24.
    /// </summary>
    [Fact]
    public void M22_ElCasoADiceQueElConductorQuedaProtegidoYQueNoHayTamanoIntermedio()
    {
        var r = ElCasoA().Alimentador.Resultado!;
        Assert.Equal(60.93m, Math.Round(r.Detalle!.CapacidadMinimaA, 2));
        Assert.Equal(69.21m, Math.Round(r.TechoProteccion430_62A!.Value, 2));
        Assert.Equal(70m, r.ProteccionA);
        Assert.Equal("4", r.CalibreFase.Designacion);
        Assert.False(r.ProteccionExcedeTecho430_62);

        var cita = Assert.Single(r.Citas, x => x.Referencia == "430-62(b)").Descripcion;
        Assert.Contains("El conductor instalado (4 AWG) tiene una ampacidad de 70 A, mayor que los 60.93 A que exigía 430-24", cita);
        Assert.Contains("70 A = ampacidad del 4 AWG: el conductor queda protegido — 240-4.", cita);
        Assert.Contains("No hay tamaño estándar entre 60.93 A y el techo de 69.21 A: el anterior, 60 A, queda debajo de la capacidad que pide 430-24.", cita);
    }

    /// <summary>
    /// Con el conductor más grande por caída de tensión (60 m: 3 AWG, 85 A a 60 °C), los 70 A quedan debajo de su
    /// ampacidad: la cita lo dice con «&lt;» y no con «=».
    /// </summary>
    [Fact]
    public void M22_ConElConductorMasGrandeLaProteccionQuedaDebajoDeSuAmpacidad()
    {
        var r = ElCasoA(longitudM: 60m).Alimentador.Resultado!;
        Assert.Equal(70m, r.ProteccionA);
        Assert.NotEqual("4", r.CalibreFase.Designacion);

        var cita = Assert.Single(r.Citas, x => x.Referencia == "430-62(b)").Descripcion;
        Assert.Contains($"70 A < {r.Detalle!.AmpacidadConductorA:0.##} A, la ampacidad del {r.CalibreFase.DesignacionConUnidad}: el conductor queda protegido — 240-4.", cita);
        Assert.Contains("el anterior, 60 A, queda debajo", cita);
    }

    /// <summary>
    /// Con riel DIN sí hay tamaño entre la capacidad mínima y el techo (63 A): se escoge solo, no excede el techo y
    /// 430-62(b) no hace falta.
    /// </summary>
    [Fact]
    public void M22_ConRielDinHayTamanoDebajoDelTechoY430_62bNoHaceFalta()
    {
        var r = ElCasoA(SerieDeInterruptores.RielDinIec).Alimentador.Resultado!;
        Assert.Equal(63m, r.ProteccionA);
        Assert.False(r.ProteccionExcedeTecho430_62);
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "430-62(b)");
    }
}
