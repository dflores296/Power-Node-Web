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
    public void OtroTableroDelRenglonPasaASuLineaConSusDosCantidades()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        c.Continua = 12000m;
        c.NoContinua = 18500m;
        cuadro.Recalcular();

        var linea = Assert.Single(c.Cargas);
        Assert.True(linea.EsTablero);
        Assert.Equal((12000m, 18500m), (linea.CargaUnitaria, linea.NoContinua));
        Assert.Equal(12000m, c.ContinuaVA);
        Assert.Equal(18500m, c.NoContinuaVA);
        Assert.Null(c.SubtipoDelRenglon);
        Assert.Equal(ClaseDeCircuito.Alimentador, c.ClaseDelCircuito);
    }

    [Fact]
    public void VariosTablerosSeGuardanYAbrenIgual()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        c.Cargas.Add(new CargaDelCircuito { Descripcion = "TA", Subtipo = SubtipoDeCarga.TableroAlimentado, CargaUnitaria = 10000m, NoContinua = 5000m });
        c.Cargas.Add(new CargaDelCircuito { Descripcion = "TB", Subtipo = SubtipoDeCarga.TableroAlimentado, CargaUnitaria = 2000m, NoContinua = 3000m });
        cuadro.Recalcular();

        var abierto = Espacio(ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.UnixEpoch), Motor).Cuadro!, 1);

        Assert.Equal(2, abierto.Cargas.Count(a => a.EsTablero));
        Assert.Equal((12000m, 8000m), (abierto.ContinuaVA, abierto.NoContinuaVA));
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

    // ---- Variadores (David, 2026-09-30) -------------------------------------------------------------

    private static CargaDelCircuito Variador(string nombre, decimal entradaA, decimal maximaA) =>
        new() { Descripcion = nombre, Subtipo = SubtipoDeCarga.MotorVelocidadAjustable, CorrientePlacaA = entradaA, ProteccionMaximaA = maximaA };

    [Fact]
    public void DosVariadoresEnUnCircuitoSonUnGrupoConElTopeDelFabricante()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        cuadro.CambiarPolos(c, 3);
        c.Cargas.Add(Variador("V1", 10m, 40m));
        c.Cargas.Add(Variador("V2", 6m, 30m));
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.Equal(ClaseDeCircuito.GrupoDeMotores, c.ClaseDelCircuito);
        Assert.Equal(16m, c.CorrienteDeMotorA);
        var r = c.Resultado!;
        // 430-24: 125 % × 10 + 6 = 18.5 A. 430-53(c)(4): 40 (máx. de V1) + 6 = 46 A; 430-53(c)(2): V2 admite 30 A.
        Assert.Contains(r.Citas, x => x.Referencia == "430-24" && x.Descripcion.Contains("18.5"));
        Assert.Contains(r.Citas, x => x.Referencia == "430-53(c)(4)" && x.Descripcion.Contains("46"));
        Assert.Contains(r.Citas, x => x.Referencia == "430-53(c)(2)");
        Assert.Equal(30m, r.ProteccionA);
    }

    [Fact]
    public void UnVariadorQueNoLlevaLaCorrienteDelGrupoPideSuCircuito()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        cuadro.CambiarPolos(c, 3);
        c.Cargas.Add(Variador("V1", 10m, 40m));
        c.Cargas.Add(Variador("V2", 6m, 15m)); // 15 A no lleva los 16 A del grupo
        cuadro.Recalcular();

        Assert.Null(c.Resultado);
        Assert.Contains("430-53(c)(2)", c.Error);
    }

    [Fact]
    public void UnSoloVariadorEnSuLineaRegresaAlRenglon()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Cargas.Add(Variador("Mezcladoras", 12m, 30m));

        Assert.True(c.LineaARenglon());
        cuadro.Recalcular();

        Assert.True(c.EsVariador);
        Assert.Equal((12m, 30m), (c.CorrienteEntradaVariadorA, c.ProteccionMaximaVariadorA));
        Assert.Empty(c.Cargas);
    }

    [Fact]
    public void ElVariadorDelRenglonPasaASuLineaAlAgregarOtraCarga()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = 12m;
        c.ProteccionMaximaVariadorA = 30m;

        c.AgregarCarga();

        var v = c.Cargas[0];
        Assert.Equal(ClaseDeAparato.Variador, v.Clase);
        Assert.Equal((12m, 30m), (v.CorrientePlacaA, v.ProteccionMaximaA));
    }

    [Fact]
    public void LosVariadoresSeGuardanYAbrenIgual()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        cuadro.CambiarPolos(c, 3);
        c.Cargas.Add(Variador("V1", 10m, 40m));
        c.Cargas.Add(Variador("V2", 6m, 30m));
        cuadro.Recalcular();

        var abierto = Espacio(ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.UnixEpoch), Motor).Cuadro!, 1);

        Assert.Equal((10m, 40m), (abierto.Cargas[0].CorrientePlacaA, abierto.Cargas[0].ProteccionMaximaA));
        Assert.Equal(30m, abierto.Resultado!.ProteccionA);
    }
}
