using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>La carga se captura en el desplegable; el renglón solo resume</b> — decisión
/// <c>captura-en-el-desplegable.md</c> (David, 2026-09-30). Lo del renglón pasa a sus líneas sin perder
/// números; un solo equipo regresa al renglón y se calcula con su artículo; los circuitos dedicados no
/// admiten más líneas; el uso de vivienda es un subtipo de Contactos.
/// </summary>
public class CapturaEnElDesplegableTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = 3;
        cuadro.Datos.Hilos = 4;
        cuadro.Datos.TensionFaseFaseV = 220m;
        return EnPvc.Todo(cuadro);
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    [Fact]
    public void ElRenglonPasaASusLineasSinCambiarElCalculo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        c.Continua = 1200m;
        c.NoContinua = 300m;
        cuadro.Recalcular();
        var antes = (c.ContinuaVA, c.NoContinuaVA, c.Resultado!.ProteccionA, c.Resultado.CalibreFase.Designacion);

        Assert.True(c.RenglonALineas(conSubtipo: true));
        cuadro.Recalcular();

        Assert.Equal(2, c.Cargas.Count);
        Assert.All(c.Cargas, a => Assert.Equal(SubtipoDeCarga.Luminarias, a.Subtipo));
        Assert.Equal(antes, (c.ContinuaVA, c.NoContinuaVA, c.Resultado!.ProteccionA, c.Resultado.CalibreFase.Designacion));
    }

    [Fact]
    public void ElUsoDeViviendaDelRenglonPasaASubtipoYSigueDando20A()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        c.Uso = UsoDeContactos.AparatosPequenos;
        c.NoContinua = 900m;
        cuadro.Recalcular();

        c.RenglonALineas(conSubtipo: true);
        c.Uso = UsoDeContactos.General; // ya no cuenta: el uso sale de la línea
        cuadro.Recalcular();

        Assert.Equal(SubtipoDeCarga.ContactoAparatosPequenos, c.Cargas.Single().Subtipo);
        Assert.Equal(UsoDeContactos.AparatosPequenos, c.UsoEfectivo);
        Assert.Equal(20m, c.Resultado!.ProteccionA);                 // 210-11(c)(1)
        Assert.Equal(600m, c.Ajuste220_52VA);                          // 1500 VA — 220-52(a)
        Assert.Equal(ClaseDeCircuito.ParaAparatos, c.ClaseDelCircuito);
    }

    [Fact]
    public void UnSoloMotorEnElDesplegableRegresaAlRenglon()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.LongitudM = 10m;
        var motor = c.AgregarCarga();
        motor.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
        motor.Hp = 0.5m;
        motor.Descripcion = "Bomba";

        Assert.True(c.LineaARenglon());
        cuadro.Recalcular();

        Assert.Empty(c.Cargas);
        Assert.True(c.EsMotor);
        Assert.Equal(0.5m, c.Hp);
        Assert.Equal("Bomba", c.Descripcion);
        Assert.Equal(SubtipoDeCarga.MotorUsoGeneral, c.SubtipoDelRenglon);
        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
        Assert.Equal(25m, c.Resultado!.ProteccionA); // 8.9 A × 250 % → 25 A — 430-52
    }

    [Fact]
    public void OtroTableroPasaAlRenglonConSusDosCantidades()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        var linea = c.AgregarCarga();
        linea.CargaUnitaria = 12000m;
        linea.Continua = true;

        Assert.True(c.PasarARenglon(SubtipoDeCarga.TableroAlimentado));
        c.NoContinua = 18500m; // la segunda cantidad de la misma línea
        cuadro.Recalcular();

        Assert.Equal(CategoriaDeCarga.Tablero, c.Categoria);
        Assert.Equal(12000m, c.ContinuaVA);
        Assert.Equal(18500m, c.NoContinuaVA);
        Assert.Equal(SubtipoDeCarga.TableroAlimentado, c.SubtipoDelRenglon);
        Assert.Equal(ClaseDeCircuito.Alimentador, c.ClaseDelCircuito);
    }

    [Fact]
    public void UnEquipoQueVaSoloNoSePasaConOtrasLineas()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.AgregarCarga();
        c.AgregarCarga();

        Assert.False(c.PasarARenglon(SubtipoDeCarga.MotorVelocidadAjustable));
        Assert.Equal(2, c.Cargas.Count);
    }

    [Theory]
    [InlineData(SubtipoDeCarga.ContactoRefrigerador, SubtipoDeCarga.Luminarias, "210-52(b)(1)")]
    [InlineData(SubtipoDeCarga.ContactoLavadora, SubtipoDeCarga.ContactoUsoGeneral, "210-11(c)(2)")]
    [InlineData(SubtipoDeCarga.ContactoAparatosPequenos, SubtipoDeCarga.Luminarias, "210-52(b)(2)")]
    public void LosCircuitosDedicadosNoAdmitenOtrasLineas(SubtipoDeCarga dedicado, SubtipoDeCarga otra, string referencia)
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        var a = c.AgregarCarga();
        a.Subtipo = dedicado;
        a.CargaUnitaria = 500m;
        var b = c.AgregarCarga();
        b.Subtipo = otra;
        b.CargaUnitaria = 200m;
        cuadro.Recalcular();

        Assert.Null(c.Resultado);
        Assert.Contains(referencia, c.Error);
    }

    [Fact]
    public void ElRefrigeradorEsUnoSolo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        var a = c.AgregarCarga();
        a.Subtipo = SubtipoDeCarga.ContactoRefrigerador;
        a.Cantidad = 2;
        cuadro.Recalcular();

        Assert.Contains("va solo en su circuito y es uno", c.Error);
    }

    [Fact]
    public void DosMotoresSonUnGrupoDeMotores()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        var m = c.AgregarCarga();
        m.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
        m.Hp = 0.5m;
        m.Cantidad = 2;
        cuadro.Recalcular();

        Assert.Equal(ClaseDeCircuito.GrupoDeMotores, c.ClaseDelCircuito);
        Assert.False(c.LineaARenglon()); // cantidad 2: no es un solo equipo
    }

    [Fact]
    public void ElSubtipoDeUsoSeGuardaEnElArchivo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        var a = c.AgregarCarga();
        a.Subtipo = SubtipoDeCarga.ContactoBano;
        a.CargaUnitaria = 360m;
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"subtipo\": \"ContactoBano\"", texto);
        Assert.Equal(SubtipoDeCarga.ContactoBano, Espacio(ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!, 1).Cargas.Single().Subtipo);
    }

    [Fact]
    public void AgregarUnaCargaAUnMotorDelRenglonLoVuelveGrupo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 0.5m;
        cuadro.Recalcular();

        var nueva = c.AgregarCarga();
        nueva.Subtipo = SubtiposDeCarga.PorOmision(c.Categoria);
        cuadro.Recalcular();

        Assert.Equal(2, c.Cargas.Count);
        Assert.True(c.EsGrupo);
        Assert.Null(c.SubtipoDelRenglon);
    }

    [Fact]
    public void UnSoloContactoDeRefrigeradorEsIndividualAunFueraDeVivienda()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        c.Cargas.Add(new CargaDelCircuito { Subtipo = SubtipoDeCarga.ContactoRefrigerador, CargaUnitaria = 600m });
        cuadro.Recalcular();

        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
    }
}
