using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Tests;

/// <summary>
/// Guardar y abrir el tablero — I-05 (decisión archivo-del-tablero, 2026-09-25). Lo que importa: lo
/// que se abre calcula exactamente lo mismo que lo que se guardó, y un archivo que no es, que está
/// dañado o que es de otra versión no tumba la pestaña.
/// </summary>
public class ArchivoDelCuadroTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static readonly DateTimeOffset Cuando = new(2026, 9, 25, 18, 30, 0, TimeSpan.FromHours(-6));

    /// <summary>Un tablero con un poco de todo lo que se captura.</summary>
    private static CuadroDeCarga TableroCompleto()
    {
        var cuadro = new CuadroDeCarga(Motor);
        var d = cuadro.Datos;
        d.Tablero = "Tablero cocina";
        d.Clave = "TC-1";
        d.Ubicacion = "Cocina y baño, planta baja";
        d.Proyecto = "Casa Flores";
        d.Cliente = "David";
        d.Diseno = "DF";
        d.Montaje = "Empotrar";
        d.GabineteNema = "3R";
        d.NumeroEspacios = 12;
        d.CapacidadBarraA = 125m;
        d.EsEquipoDeAcometida = true;
        d.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        d.TensionFaseFaseV = 208m;
        d.LugarSeco = false;
        d.TipoAislamiento = "THW-LS";
        d.DiametrosFabricante[DatosDelTablero.ClaveDiametro("THW-LS", "12")] = 3.9m;
        d.TemperaturaAmbienteC = 35m;
        d.CargaNoLineal = true;
        d.CaidaMaxDerivadoPct = 2.5m;
        d.LongitudAlimentadorM = 35m;
        d.CambiarFactorDeDemanda(CategoriaDeCarga.Equipo, 0.75m);
        d.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.AparatosFijosVivienda);
        d.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.Otra);
        d.JustificacionOtra[CategoriaDeCarga.Equipo] = "Uso alterno de la cocina";
        cuadro.Recalcular();

        var alumbrado = cuadro.Circuitos[0];
        alumbrado.Descripcion = "Alumbrado cocina";
        alumbrado.NoContinua = 480m;
        alumbrado.LongitudM = 12m;

        var contactos = cuadro.Circuitos[1];
        contactos.Descripcion = "Contactos barra";
        contactos.Categoria = CategoriaDeCarga.Contactos;
        contactos.Uso = UsoDeContactos.AparatosPequenos;
        contactos.NoContinua = 1500m;

        var horno = cuadro.Circuitos[2];
        horno.Descripcion = "Horno";
        horno.Categoria = CategoriaDeCarga.Equipo;
        horno.Unidad = UnidadConsumo.Watts;
        horno.FactorPotencia = 1m;
        horno.Continua = 3500m;
        cuadro.CambiarPolos(horno, 2);
        horno.ConNeutro = true;

        var lavavajillas = cuadro.Circuitos[5];
        lavavajillas.Descripcion = "Lavavajillas y triturador";
        lavavajillas.Categoria = CategoriaDeCarga.Equipo;
        var a = lavavajillas.AgregarAparato();
        a.Descripcion = "Lavavajillas";
        a.Unidad = UnidadConsumo.Amperes;
        a.CargaUnitaria = 10m;
        var b = lavavajillas.AgregarAparato();
        b.Descripcion = "Triturador";
        b.Cantidad = 2;
        b.CargaUnitaria = 300m;
        b.Continua = true;
        b.FactorPotencia = 0.8m;
        cuadro.Recalcular();

        // Una canalización propia, con nombre y opciones, que lleva dos circuitos.
        var pasillo = d.NuevaCanalizacion();
        pasillo.Nombre = "Tubo pasillo";
        pasillo.Tubo = TipoTuboConduit.PvcCedula40;
        pasillo.AlturaSobreTechoMm = 300m;
        pasillo.TierraComun = true;
        pasillo.TamanoFijado = 27;
        alumbrado.Canalizacion = pasillo.Id;
        contactos.Canalizacion = pasillo.Id;
        d.CanalizacionAlimentador.Nombre = "Acometida";
        d.CanalizacionAlimentador.TierraDesnuda = true;
        cuadro.Recalcular();
        return cuadro;
    }

    [Fact]
    public void I05_LoQueSeAbreEsLoQueSeGuardo()
    {
        var original = TableroCompleto();
        var texto = ArchivoDelCuadro.Guardar(original, Cuando);

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        Assert.Empty(apertura.Avisos);
        var abierto = apertura.Cuadro!;
        Assert.Equal(ArchivoDelCuadro.Huella(original), ArchivoDelCuadro.Huella(abierto));
        // Y calcula lo mismo: protección, conductor y caída de cada renglón, y el alimentador.
        Assert.Equal(
            original.Circuitos.Select(c => (c.Espacio, c.Resultado?.ProteccionA, c.Resultado?.CalibreFase.Designacion, c.Resultado?.CaidaTensionPct)),
            abierto.Circuitos.Select(c => (c.Espacio, c.Resultado?.ProteccionA, c.Resultado?.CalibreFase.Designacion, c.Resultado?.CaidaTensionPct)));
        Assert.Equal(original.InterruptorPrincipalA, abierto.InterruptorPrincipalA);
        Assert.Equal(original.Alimentador.Resultado?.CalibreFase.Designacion, abierto.Alimentador.Resultado?.CalibreFase.Designacion);
        Assert.Equal(
            original.TodasLasCanalizaciones.Select(t => (t.Id, t.Nombre, t.TamanoRotulo, t.FactorAgrupamiento)),
            abierto.TodasLasCanalizaciones.Select(t => (t.Id, t.Nombre, t.TamanoRotulo, t.FactorAgrupamiento)));
        Assert.Equal("Uso alterno de la cocina", abierto.Datos.JustificacionOtra[CategoriaDeCarga.Equipo]);
        Assert.Equal(2, abierto.Circuitos[5].Aparatos.Count);
        Assert.Equal(2, abierto.Circuitos[2].Polos);
    }

    [Fact]
    public void I05_ElArchivoSeLeeASimpleVista()
    {
        var texto = ArchivoDelCuadro.Guardar(TableroCompleto(), Cuando);

        Assert.Contains("\"formato\": \"power-node/cuadro-de-carga\"", texto);
        Assert.Contains("\"version\": 1", texto);
        Assert.Contains("\"guardado\": \"2026-09-25T18:30:00-06:00\"", texto);
        Assert.Contains("\"ubicacion\": \"Cocina y baño, planta baja\"", texto); // sin \u00F1
        Assert.Contains("\"justificacionOtra\": {", texto);
        Assert.DoesNotContain("\\u00", texto);
        Assert.Contains("\"categoria\": \"Contactos\"", texto);   // los tipos por su nombre
        Assert.Contains("\"unidad\": \"Watts\"", texto);
        Assert.Contains("\"Equipo\": 0.75", texto);
        // Sin resultados: el cuadro se recalcula al abrir.
        Assert.DoesNotContain("proteccion", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("calibre", texto, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void I05_SoloSeGuardanLosRenglonesConCaptura()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Circuitos[4].Descripcion = "Reserva";
        cuadro.Recalcular();

        var archivo = System.Text.Json.JsonDocument.Parse(ArchivoDelCuadro.Guardar(cuadro, Cuando));

        var circuitos = archivo.RootElement.GetProperty("circuitos");
        Assert.Equal(1, circuitos.GetArrayLength());
        Assert.Equal(5, circuitos[0].GetProperty("espacio").GetInt32());
    }

    [Fact]
    public void I05_LaHuellaNoDependeDeLaHora()
    {
        var cuadro = TableroCompleto();

        Assert.NotEqual(ArchivoDelCuadro.Guardar(cuadro, Cuando), ArchivoDelCuadro.Guardar(cuadro, Cuando.AddHours(1)));
        var huella = ArchivoDelCuadro.Huella(cuadro);
        cuadro.Circuitos[0].LongitudM = 13m;
        Assert.NotEqual(huella, ArchivoDelCuadro.Huella(cuadro));
    }

    [Fact]
    public void I05_UnArchivoConMenosCamposAbreConLosValoresDeUnTableroNuevo()
    {
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 1,
              "datos": { "tablero": "Bodega" },
              "circuitos": [ { "espacio": 3, "descripcion": "Luminarias", "noContinua": 900 } ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        var cuadro = apertura.Cuadro!;
        var nuevo = new CuadroDeCarga(Motor);
        Assert.Equal("Bodega", cuadro.Datos.Tablero);
        Assert.Equal(nuevo.Datos.NumeroEspacios, cuadro.Datos.NumeroEspacios);
        Assert.Equal(nuevo.Datos.TensionFaseFaseV, cuadro.Datos.TensionFaseFaseV);
        Assert.Equal(nuevo.Datos.FrecuenciaHz, cuadro.Datos.FrecuenciaHz);
        Assert.Equal(CircuitoDelCuadro.FactorPotenciaSupuesto, cuadro.Circuitos[2].FactorPotencia);
        Assert.True(cuadro.Circuitos[2].TieneCarga);
        Assert.NotNull(cuadro.Circuitos[2].Resultado); // ya recalculado
        Assert.Equal("T1", cuadro.Circuitos[2].Canalizacion); // y con su tubo, como al capturarlo
    }

    [Theory]
    [InlineData("hola", "no se pudo leer")]
    [InlineData("{ \"tablero\": \"x\" }", "no es un cuadro de carga")]
    [InlineData("{ \"formato\": \"power-node/cuadro-de-carga\" }", "no dice de qué versión")]
    [InlineData("{ \"formato\": \"power-node/cuadro-de-carga\", \"version\": 99 }", "versión más nueva")]
    [InlineData("{ \"formato\": \"power-node/cuadro-de-carga\", \"version\": 1, \"datos\": { \"inmueble\": \"Castillo\" } }", "no se pudo leer")]
    public void I05_LoQueNoEsUnArchivoDePowerNodeNoSeAbre(string texto, string motivo)
    {
        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Cuadro);
        Assert.Contains(motivo, apertura.Error);
    }

    [Fact]
    public void I05_LoQueNoCabeSeAjustaYSeAvisa()
    {
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 1,
              "datos": { "fases": 1, "hilos": 2, "numeroEspacios": 24 },
              "circuitos": [ { "espacio": 30, "descripcion": "Fuera", "noContinua": 100 } ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        Assert.Equal(8, apertura.Cuadro!.Datos.NumeroEspacios);
        Assert.Contains(apertura.Avisos, a => a.Contains("24 espacios") && a.Contains("con 8"));
        Assert.Contains(apertura.Avisos, a => a.Contains("circuito 30"));
    }

    [Theory]
    [InlineData("Tablero cocina", "", "Tablero cocina.powernode.json")]
    [InlineData("TC: 1/2", "", "TC- 1-2.powernode.json")]
    [InlineData("", "TC-1", "TC-1.powernode.json")]
    [InlineData("  ", "", "Tablero.powernode.json")]
    public void I05_ElNombreDelArchivoEsElDelTablero(string tablero, string clave, string esperado)
    {
        var datos = new DatosDelTablero { Tablero = tablero, Clave = clave };

        Assert.Equal(esperado, ArchivoDelCuadro.NombreSugerido(datos));
    }
}
