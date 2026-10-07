using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>El mínimo de 220-14 en «Carga c/u»</b> — D14 a D19 de decisiones/acomodo-del-desplegable.md (David, 2026-10-07).
/// El campo trae el mínimo y no se deja debajo; el resultado del cálculo no cambia (ya usaba el mínimo).
/// </summary>
public class MinimosDeLaCargaTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static (CuadroDeCarga, CircuitoDelCuadro) Nuevo(TipoDeInmueble inmueble = TipoDeInmueble.Otro)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Inmueble = inmueble;
        cuadro.Recalcular();
        var c = cuadro.Circuitos.Single(x => x.Espacio == 1);
        c.LongitudM = 10m;
        return (cuadro, c);
    }

    private static CargaDelCircuito Linea(CuadroDeCarga cuadro, CircuitoDelCuadro c, SubtipoDeCarga subtipo)
    {
        var a = new CargaDelCircuito { Subtipo = subtipo };
        c.Cargas.Add(a);
        MinimosDeLaCarga.LlenarAlEscoger(cuadro, c, a);
        return a;
    }

    [Theory]
    [InlineData(SubtipoDeCarga.ContactoUsoGeneral, 180)]
    [InlineData(SubtipoDeCarga.ContactoMultiple, 90)]
    [InlineData(SubtipoDeCarga.PortalamparasPesado, 600)]
    [InlineData(SubtipoDeCarga.EnsambleDeSalidas, 180)]
    [InlineData(SubtipoDeCarga.Anuncios, 1200)]
    public void D14_AlEscogerElSubtipoElCampoTraeElMinimo(SubtipoDeCarga subtipo, int minimo)
    {
        var (cuadro, c) = Nuevo();
        Assert.Equal(minimo, Linea(cuadro, c, subtipo).CargaUnitaria);
    }

    [Fact]
    public void D14_UnaCargaMayorQueElMinimoSeQueda()
    {
        var (cuadro, c) = Nuevo();
        var a = new CargaDelCircuito { CargaUnitaria = 300m };
        c.Cargas.Add(a);
        a.Subtipo = SubtipoDeCarga.ContactoUsoGeneral;
        MinimosDeLaCarga.LlenarAlEscoger(cuadro, c, a);
        Assert.Equal(300m, a.CargaUnitaria);
    }

    [Fact]
    public void D15_LoEscritoAbajoDelMinimoSubeYLoDice()
    {
        var (cuadro, c) = Nuevo();
        var a = Linea(cuadro, c, SubtipoDeCarga.ContactoUsoGeneral);
        a.CargaUnitaria = 100m;

        Assert.Equal("Contacto: no menos de 180 VA.", MinimosDeLaCarga.SubirAlMinimo(cuadro, c, a));
        Assert.Equal(180m, a.CargaUnitaria);
        Assert.Null(MinimosDeLaCarga.SubirAlMinimo(cuadro, c, a));
    }

    [Fact]
    public void D16_ElContactoSoloEnVA_LaSecadoraConvierteSuMinimo()
    {
        var (cuadro, c) = Nuevo(TipoDeInmueble.ViviendaUnifamiliar);
        var contacto = new CargaDelCircuito { Unidad = UnidadConsumo.Watts };
        c.Cargas.Add(contacto);
        contacto.Subtipo = SubtipoDeCarga.ContactoUsoGeneral;
        MinimosDeLaCarga.LlenarAlEscoger(cuadro, c, contacto);
        Assert.Equal((UnidadConsumo.VoltAmperes, 180m), (contacto.Unidad, contacto.CargaUnitaria));

        var (cuadro2, c2) = Nuevo(TipoDeInmueble.ViviendaUnifamiliar);
        var secadora = Linea(cuadro2, c2, SubtipoDeCarga.Secadora);
        Assert.Equal(5000m, secadora.CargaUnitaria);
        MinimosDeLaCarga.CambiarUnidad(cuadro2, c2, secadora, UnidadConsumo.Amperes);
        // 5000 VA a 127 V (1 polo): 39.37 A, hacia arriba.
        Assert.Equal(Math.Ceiling(5000m / cuadro2.Datos.TensionFaseNeutroV * 100m) / 100m, secadora.CargaUnitaria);
    }

    [Fact]
    public void D17_LaSecadoraConElMinimoSigueAlInmueble_LaEscritaNo()
    {
        var (cuadro, c) = Nuevo(TipoDeInmueble.ViviendaUnifamiliar);
        var conMinimo = Linea(cuadro, c, SubtipoDeCarga.Secadora);
        var escrita = Linea(cuadro, c, SubtipoDeCarga.Secadora);
        escrita.CargaUnitaria = 6200m;

        MinimosDeLaCarga.CambiarInmueble(cuadro, TipoDeInmueble.Otro);

        Assert.Equal(TipoDeInmueble.Otro, cuadro.Datos.Inmueble);
        Assert.Equal(0m, conMinimo.CargaUnitaria);   // fuera de vivienda, 220-54 no pone mínimo
        Assert.Equal(6200m, escrita.CargaUnitaria);

        MinimosDeLaCarga.CambiarInmueble(cuadro, TipoDeInmueble.ViviendaUnifamiliar);
        Assert.Equal(6200m, escrita.CargaUnitaria);
    }

    [Fact]
    public void D18_ConVariasLineasDeAnunciosNoSeLlena()
    {
        var (cuadro, c) = Nuevo();
        var primera = Linea(cuadro, c, SubtipoDeCarga.Anuncios);
        var segunda = Linea(cuadro, c, SubtipoDeCarga.Anuncios);
        Assert.Equal(1200m, primera.CargaUnitaria);
        Assert.Equal(0m, segunda.CargaUnitaria);
    }

    [Fact]
    public void D19_UnArchivoConLaCargaEnCeroEnsenaElMinimo_ElResultadoNoCambia()
    {
        var (cuadro, c) = Nuevo();
        c.Categoria = CategoriaDeCarga.Contactos;
        c.Cargas.Add(new CargaDelCircuito { Subtipo = SubtipoDeCarga.ContactoUsoGeneral, Cantidad = 4 });
        cuadro.Recalcular();
        var antes = (c.CargaInstaladaVA, c.Resultado!.CorrienteDisenoA);

        var abierto = ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.UnixEpoch), Motor).Cuadro!;
        var d = abierto.Circuitos.Single(x => x.Espacio == 1);

        Assert.Equal(180m, d.Cargas[0].CargaUnitaria);
        Assert.Equal(antes, (d.CargaInstaladaVA, d.Resultado!.CorrienteDisenoA));
    }
}

/// <summary>
/// D2 (acomodo-del-desplegable.md): al pasar el motor solo a Cant. 2 o más, lo capturado no se pierde; la protección
/// fijada y las casillas del motor solo regresan al volver a Cant. 1.
/// </summary>
public class CantidadDelMotorSoloTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    [Fact]
    public void D2_LaProteccionFijadaYLasCasillasRegresanAlVolverACantidadUno()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        var c = cuadro.Circuitos.Single(x => x.Espacio == 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 5m;
        c.LongitudM = 10m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        c.MotorYArrancadorMarcados75C = true;
        cuadro.Recalcular();
        var rango = c.Resultado!.Rango!;
        var fijada = rango.Valores.First(v => v != c.Resultado.ProteccionA);
        cuadro.FijarProteccion(c, fijada);
        cuadro.Recalcular();
        Assert.Equal(fijada, c.Resultado!.ProteccionA);

        // Cant. 3: grupo.
        Assert.True(c.RenglonALineas());
        c.Cargas[0].Cantidad = 3;
        cuadro.Recalcular();
        Assert.True(c.EsGrupo);

        // De regreso a Cant. 1: el motor solo, con su protección fijada y su casilla.
        c.Cargas[0].Cantidad = 1;
        Assert.True(c.LineaARenglon());
        cuadro.Recalcular();
        Assert.False(c.EsGrupo);
        Assert.Equal(fijada, c.Resultado!.ProteccionA);
        Assert.True(c.MotorYArrancadorMarcados75C);
    }
}
