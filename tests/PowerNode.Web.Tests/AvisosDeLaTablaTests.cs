using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Lo que la tabla de captura le dice al usuario</b> — la guía de David (decisiones/redaccion-de-avisos.md) y los
/// textos aprobados el 2026-10-07 (conocimiento/avisos-de-la-captura.md). La tabla traduce; el mensaje del cálculo
/// no cambia y sigue yendo a la memoria.
/// </summary>
public class AvisosDeLaTablaTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static CuadroDeCarga Nuevo()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    private static (CuadroDeCarga, CircuitoDelCuadro) UnVariador()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = 12m;
        c.ProteccionMaximaVariadorA = 30m;
        cuadro.Recalcular();
        return (cuadro, c);
    }

    [Fact]
    public void C1_SinTensionDeEntradaDiceQueFaltaYMarcaSuColumna()
    {
        var (cuadro, c) = UnVariador();

        var e = AvisosDeLaTabla.ErrorDe(cuadro, c)!;
        Assert.Equal("Falta la tensión de entrada.", e.Texto);
        Assert.Equal("Tension", e.Campo);
        Assert.Null(e.Linea);
        // El mensaje completo, con su porqué, no se pierde: va a la memoria.
        Assert.Equal(c.Error, e.Original);
    }

    [Fact]
    public void C7_ConBypassSinHpDiceQueFaltanLosHp()
    {
        var (cuadro, c) = UnVariador();
        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionesDePlaca.De(cuadro.Datos).First(t => t.Polos() == 3)));
        c.VariadorConBypass = true;
        cuadro.Recalcular();

        var e = AvisosDeLaTabla.ErrorDe(cuadro, c)!;
        Assert.Equal(("Faltan los HP del motor.", "HpMotor"), (e.Texto, e.Campo));
    }

    [Fact]
    public void C24_UnVariadorQueNoLlevaLaCorrienteDelGrupo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        cuadro.CambiarPolos(c, 3);
        c.Cargas.Add(new CargaDelCircuito { Subtipo = SubtipoDeCarga.MotorVelocidadAjustable, CorrientePlacaA = 10m, ProteccionMaximaA = 40m });
        c.Cargas.Add(new CargaDelCircuito { Subtipo = SubtipoDeCarga.MotorVelocidadAjustable, CorrientePlacaA = 6m, ProteccionMaximaA = 15m });
        cuadro.Recalcular();

        Assert.Equal("La protección del variador no alcanza la corriente del grupo. Pasa el variador a su propio circuito.",
            AvisosDeLaTabla.ErrorDe(cuadro, c)!.Texto);
    }

    [Fact]
    public void C13_ElContactoDelRefrigeradorEsUno()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        c.Cargas.Add(new CargaDelCircuito { Subtipo = SubtipoDeCarga.ContactoRefrigerador, CargaUnitaria = 600m, Cantidad = 2 });
        cuadro.Recalcular();

        var e = AvisosDeLaTabla.ErrorDe(cuadro, c)!;
        Assert.Equal(("El contacto del refrigerador es uno. Deja la cantidad en 1.", "Cant"), (e.Texto, e.Campo));
    }

    [Fact]
    public void ElErrorDeUnaMaquinaDelGrupoMarcaSuLinea()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        cuadro.CambiarPolos(c, 3);
        c.Cargas.Add(new CargaDelCircuito { Subtipo = SubtipoDeCarga.MotorUsoGeneral, CapturaMotor = CapturaDeMotor.Hp, Hp = 2m });
        var malo = new CargaDelCircuito { Subtipo = SubtipoDeCarga.MotorUsoGeneral, CapturaMotor = CapturaDeMotor.Amperes, CorrientePlacaA = 5000m };
        c.Cargas.Add(malo);
        cuadro.Recalcular();

        var e = AvisosDeLaTabla.ErrorDe(cuadro, c)!;
        Assert.StartsWith("La tabla no trae un motor de 5000 A a ", e.Texto);
        Assert.EndsWith("Revisa la corriente de placa.", e.Texto);
        Assert.Equal("CorrientePlaca", e.Campo);
        Assert.Same(malo, e.Linea);
    }

    [Fact]
    public void SinErrorNoHayError() =>
        Assert.Null(AvisosDeLaTabla.ErrorDe(Nuevo(), Espacio(Nuevo(), 1)));

    [Fact]
    public void B9_AlumbradoEnUnCircuitoDe25AEnmarcaLaProteccion()
    {
        // 3000 VA / 127 V = 23.6 A → 25 A: con alumbrado común no se permite — 210-23(b).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        c.NoContinua = 3000m;
        c.LongitudM = 5m;
        cuadro.Recalcular();

        var a = Assert.Single(AvisosDeLaTabla.AdvertenciasDe(cuadro, c));
        Assert.Equal(CeldaDelAviso.Proteccion, a.Celda);
        Assert.Equal("Alumbrado común en un circuito de 25 A. Divide el alumbrado en circuitos de 20 A.", a.Texto);
        Assert.Equal("210-23(b)", a.Referencia);
    }

    [Fact]
    public void F1_LaProteccionFijadaQueRegresa()
    {
        Assert.Equal("La protección fijada del circuito 3 ya no está en su rango. Regresa a 45 A.",
            AvisosDeLaTabla.ProteccionQueRegreso([new ProteccionQueRegreso(3, 15m, 25m, 45m, 45m)]));
        Assert.Equal("Las protecciones fijadas de los circuitos 3, 5 ya no están en su rango. Regresan al cálculo.",
            AvisosDeLaTabla.ProteccionQueRegreso([new ProteccionQueRegreso(3, 15m, 25m, 45m), new ProteccionQueRegreso(5, 15m, 25m, 45m)]));
    }

    /// <summary>
    /// Ningún texto de la tabla lleva lo que la guía prohíbe: el «—» de cita, «Captúrala», la flecha del desplegable.
    /// </summary>
    [Fact]
    public void LosTextosSiguenLaGuia()
    {
        var (cuadro, c) = UnVariador();
        var e = AvisosDeLaTabla.ErrorDe(cuadro, c)!;
        Assert.DoesNotContain("—", e.Texto);
        Assert.DoesNotContain("desplegable", e.Texto);
        Assert.EndsWith(".", e.Texto);
    }
}
