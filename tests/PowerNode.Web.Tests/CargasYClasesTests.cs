using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>El tipo es de la carga; la clase es del circuito</b> — I-123, I-125, decisión
/// <c>cargas-y-clases-de-circuito.md</c>. Los números salen de la norma: 180 VA por contacto y 90 por
/// contacto de uno múltiple (220-14(i)), 600 VA por portalámparas pesado (220-14(e)), 1200 VA por
/// circuito de anuncios (220-14(f)), 5000 VA por secadora en vivienda (220-54); el F.D. por el tipo de
/// cada carga (220 Parte C); otro tablero sin F.D. (220-40) y citado con 215-2(a)(1) y 215-3.
/// </summary>
public class CargasYClasesTests
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

    private static CargaDelCircuito Linea(CircuitoDelCuadro c, SubtipoDeCarga subtipo, int cantidad, decimal cargaVA, bool continua = false)
    {
        var a = c.AgregarCarga();
        a.Subtipo = subtipo;
        a.Cantidad = cantidad;
        a.Unidad = UnidadConsumo.VoltAmperes;
        a.CargaUnitaria = cargaVA;
        a.Continua = continua;
        return a;
    }

    private static decimal Demandada(CuadroDeCarga cuadro, CategoriaDeCarga tipo) =>
        cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == tipo).DemandadaVA;

    private static decimal Instalada(CuadroDeCarga cuadro, CategoriaDeCarga tipo) =>
        cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == tipo).InstaladaVA;

    // ---- El F.D. por el tipo de cada carga ---------------------------------------------------------

    [Fact]
    public void I123_UnCircuitoCombinadoLlevaElFactorDeCadaCarga()
    {
        // 10 luminarias de 100 VA y 5 contactos: 1000 VA de alumbrado y 900 VA de contactos. F.D. 0.5 solo
        // en alumbrado → 500 + 900 = 1400 VA de demanda. Antes todo iba con el F.D. del tipo del circuito.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        Linea(c, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);
        Linea(c, SubtipoDeCarga.ContactoUsoGeneral, 5, 0m);
        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Recalcular();

        Assert.True(c.TieneCargasCombinadas);
        Assert.Equal(1000m, Instalada(cuadro, CategoriaDeCarga.Alumbrado));
        Assert.Equal(900m, Instalada(cuadro, CategoriaDeCarga.Contactos));
        Assert.Equal(500m, Demandada(cuadro, CategoriaDeCarga.Alumbrado));
        Assert.Equal(900m, Demandada(cuadro, CategoriaDeCarga.Contactos));
        Assert.Equal(1400m, cuadro.Resumen.DemandadaVA);
        // El derivado va con la carga plena (220-42 no aplica al circuito) y como Contactos: sin 240-4(b).
        Assert.Equal(1000m, c.ContinuaVA);
        Assert.Equal(900m, c.NoContinuaVA);
        Assert.Equal(ClaseDeCircuito.UsoGeneral, c.ClaseDelCircuito);
        Assert.Equal(15, c.Salidas);
    }

    [Fact]
    public void I123_UnaCargaSinSubtipoTomaElTipoDelCircuito()
    {
        // Lo de hasta el formato 4: sin subtipo, la carga es del tipo del circuito y todo sigue igual.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var a = c.AgregarCarga();
        a.CargaUnitaria = 800m;
        cuadro.Recalcular();

        Assert.Equal(CategoriaDeCarga.Equipo, c.TipoDe(a));
        Assert.False(c.TieneCargasCombinadas);
        Assert.Equal(800m, Instalada(cuadro, CategoriaDeCarga.Equipo));
    }

    [Fact]
    public void I123_UnMotorEntreLasCargasHaceElGrupoYVaConElFactorDeMotores()
    {
        // Alumbrado con un motor de uso general: 430-53 por lo que lleva, sin la unidad «Varios». El motor,
        // con el F.D. de motores; el alumbrado, con el suyo.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        Linea(c, SubtipoDeCarga.Luminarias, 4, 100m);
        var motor = c.AgregarCarga();
        motor.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
        motor.Hp = 0.5m;
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.Equal(ClaseDeAparato.Motor, motor.Clase);
        Assert.Equal(400m, Instalada(cuadro, CategoriaDeCarga.Alumbrado));
        Assert.Equal(c.MotorVA, Instalada(cuadro, CategoriaDeCarga.Motor));
        Assert.True(c.MotorVA > 0m);
        Assert.NotNull(c.Resultado?.Grupo);
    }

    // ---- Los mínimos de cada subtipo — 220-14 ------------------------------------------------------

    [Theory]
    [InlineData(SubtipoDeCarga.ContactoUsoGeneral, 3, 100, 540, "220-14(i)")]   // 3 × 180
    [InlineData(SubtipoDeCarga.ContactoMultiple, 6, 50, 540, "220-14(i)")]      // 6 × 90
    [InlineData(SubtipoDeCarga.PortalamparasPesado, 2, 300, 1200, "220-14(e)")] // 2 × 600
    [InlineData(SubtipoDeCarga.EnsambleDeSalidas, 4, 0, 720, "220-14(h)")]      // 4 tramos × 180
    [InlineData(SubtipoDeCarga.Anuncios, 1, 500, 1200, "220-14(f)")]            // 1200 por circuito
    [InlineData(SubtipoDeCarga.ContactoUsoGeneral, 2, 250, 500, null)]          // ya pasa: se queda
    public void I123_CadaSubtipoLlevaSuMinimo(SubtipoDeCarga subtipo, int cantidad, int vaUnitario, int esperadoVA, string? referencia)
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = subtipo.Tipo();
        var a = Linea(c, subtipo, cantidad, vaUnitario);
        cuadro.Recalcular();

        Assert.Equal(esperadoVA, a.TotalVA);
        Assert.Equal(referencia, a.ReferenciaMinimo);
    }

    [Fact]
    public void I123_LaSecadoraLlevaCincoMilSoloEnVivienda()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var a = Linea(c, SubtipoDeCarga.Secadora, 1, 4000m);
        cuadro.Recalcular();
        Assert.Equal(4000m, a.TotalVA);

        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        cuadro.Recalcular();
        Assert.Equal(5000m, a.TotalVA);
        Assert.Equal("220-54", a.ReferenciaMinimo);
    }

    [Fact]
    public void I123_ElCalentadorDeAguaEsContinuo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var a = Linea(c, SubtipoDeCarga.CalentadorDeAgua, 1, 1500m);
        cuadro.Recalcular();

        Assert.True(a.Continua);
        Assert.Equal("422-13", a.ReferenciaContinua);
        Assert.Equal(1500m, c.ContinuaVA);
        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
    }

    // ---- Otro tablero — I-125 ----------------------------------------------------------------------

    [Fact]
    public void I125_OtroTableroEsUnAlimentadorSinFactorDeDemanda()
    {
        // 12 000 VA continuos y 18 500 no continuos del otro tablero, a 3 polos y 220 V: la capacidad es
        // 1.25 × 12 000 + 18 500 = 33 500 VA → 87.92 A — 215-2(a)(1), 215-3. El F.D. de alumbrado (0.5) no
        // lo toca: entra con sus 30 500 VA — 220-40.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        c.Continua = 12000m;
        c.NoContinua = 18500m;
        c.LongitudM = 10m;
        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Datos.CambiarFactorDeDemanda(CategoriaDeCarga.Tablero, 0.5m);
        cuadro.Recalcular();

        Assert.Equal(1m, cuadro.Datos.FactorDeDemanda(CategoriaDeCarga.Tablero));
        Assert.Equal(30500m, Demandada(cuadro, CategoriaDeCarga.Tablero));
        Assert.Equal(ClaseDeCircuito.Alimentador, c.ClaseDelCircuito);
        var r = c.Resultado!;
        Assert.Equal(90m, r.ProteccionA);
        Assert.Contains(r.Citas, x => x.Referencia == "215-2(a)(1)");
        Assert.Contains(r.Citas, x => x.Referencia == "215-3");
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "210-19(a)(1)");
        Assert.Empty(CategoriaDeCarga.Tablero.JustificacionesPosibles(cuadro.Datos.Inmueble));
    }

    [Fact]
    public void I125_OtroTableroNoSeDesglosa()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        c.NoContinua = 1000m;
        c.Cargas.Add(new CargaDelCircuito { CargaUnitaria = 5000m });
        cuadro.Recalcular();

        Assert.False(c.TieneDesglose);
        Assert.Equal(1000m, c.CargaInstaladaVA);
    }

    // ---- La clase del circuito ----------------------------------------------------------------------

    [Theory]
    [InlineData(CategoriaDeCarga.Alumbrado, ClaseDeCircuito.UsoGeneral)]
    [InlineData(CategoriaDeCarga.Contactos, ClaseDeCircuito.UsoGeneral)]
    [InlineData(CategoriaDeCarga.CalefaccionFija, ClaseDeCircuito.ParaAparatos)]
    [InlineData(CategoriaDeCarga.Equipo, ClaseDeCircuito.Individual)]
    public void I123_LaClaseDelRenglon(CategoriaDeCarga tipo, ClaseDeCircuito clase)
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = tipo;
        c.NoContinua = 1000m;
        cuadro.Recalcular();

        Assert.Equal(clase, c.ClaseDelCircuito);
        Assert.Equal(tipo != CategoriaDeCarga.Equipo, c.EsCargaTotal);
    }

    [Fact]
    public void I123_DosAparatosSinAlumbradoEsParaAparatos()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        Linea(c, SubtipoDeCarga.AparatoFijo, 1, 600m);
        Linea(c, SubtipoDeCarga.AparatoFijo, 1, 400m);
        cuadro.Recalcular();

        Assert.Equal(ClaseDeCircuito.ParaAparatos, c.ClaseDelCircuito);
    }

    [Theory]
    [InlineData(UsoDeContactos.AparatosPequenos, ClaseDeCircuito.ParaAparatos)]
    [InlineData(UsoDeContactos.Lavadora, ClaseDeCircuito.ParaAparatos)]
    [InlineData(UsoDeContactos.Refrigerador, ClaseDeCircuito.Individual)]
    [InlineData(UsoDeContactos.General, ClaseDeCircuito.UsoGeneral)]
    public void I123_LosUsosDeViviendaDanSuClase(UsoDeContactos uso, ClaseDeCircuito clase)
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        c.Uso = uso;
        c.NoContinua = 1000m;
        cuadro.Recalcular();

        Assert.Equal(clase, c.ClaseDelCircuito);
    }

    // ---- El renglón como carga ----------------------------------------------------------------------

    [Fact]
    public void I123_AbrirElDesgloseDeUnMotorLoVuelveLaPrimeraLinea()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 0.5m;
        c.LongitudM = 10m;
        cuadro.Recalcular();
        var antes = c.Resultado!.ProteccionA;

        c.AgregarCarga();
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.Equal(ClaseDeAparato.Motor, c.Cargas[0].Clase);
        Assert.Equal(0.5m, c.Cargas[0].Hp);
        Assert.Equal(antes, c.Resultado!.ProteccionA); // un motor solo sigue siendo un motor — 430-52
    }

    // ---- El archivo, formato 5 -----------------------------------------------------------------------

    [Fact]
    public void I123_ElSubtipoSeGuardaYElTableroTambien()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        Linea(c, SubtipoDeCarga.Luminarias, 2, 100m);
        Linea(c, SubtipoDeCarga.ContactoMultiple, 4, 0m);
        var t = Espacio(cuadro, 2);
        t.Categoria = CategoriaDeCarga.Tablero;
        t.NoContinua = 5000m;
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"subtipo\": \"ContactoMultiple\"", texto);
        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;

        Assert.Equal([SubtipoDeCarga.Luminarias, SubtipoDeCarga.ContactoMultiple], Espacio(abierto, 1).Cargas.Select(a => a.Subtipo));
        Assert.Equal(CategoriaDeCarga.Tablero, Espacio(abierto, 2).Categoria);
        Assert.Equal(cuadro.Resumen.DemandadaVA, abierto.Resumen.DemandadaVA);
    }

    [Fact]
    public void I123_UnSubtipoQueSoloVaEnElRenglonNoSeLeeEnUnaCarga()
    {
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 5,
              "circuitos": [ { "espacio": 1, "categoria": "Equipo",
                "aparatos": [ { "cargaUnitaria": 500, "subtipo": "TableroAlimentado" } ] } ]
            }
            """;
        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;

        Assert.Null(Espacio(abierto, 1).Cargas.Single().Subtipo);
    }
}
