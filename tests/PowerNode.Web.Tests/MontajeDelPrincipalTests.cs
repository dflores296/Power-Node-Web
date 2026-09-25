using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Tests;

/// <summary>
/// Dónde va el interruptor principal — I-68, decisión <c>montaje-del-interruptor-principal.md</c>
/// con las respuestas de David del 2026-09-25: 1F-2H arranca con zapatas; en 2 y 3 barras, zócalo;
/// en espacios, los últimos pares (1F-2H: el espacio 1); polos = barras; nunca borra lo capturado.
/// </summary>
public class MontajeDelPrincipalTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo(int fases = 3, int hilos = 4, int espacios = 24)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.Fases = fases;
        cuadro.Datos.Hilos = hilos;
        cuadro.Datos.NumeroEspacios = espacios;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int n) => cuadro.Circuitos[n - 1];

    [Fact]
    public void UnaFaseDosHilos_ArrancaConZapatas_YAlSalirRegresaElPrincipal()
    {
        var cuadro = Nuevo();
        Assert.Equal(TipoAcometidaTablero.InterruptorPrincipal, cuadro.Datos.TipoAcometida);

        cuadro.Datos.Fases = 1;
        cuadro.Datos.Hilos = 2;
        Assert.Equal(TipoAcometidaTablero.ZapatasPrincipales, cuadro.Datos.TipoAcometida);

        // Lo que se elija ya en 1F-2H se respeta.
        cuadro.Datos.TipoAcometida = TipoAcometidaTablero.InterruptorPrincipal;
        cuadro.Datos.NumeroEspacios = 8;
        Assert.Equal(TipoAcometidaTablero.InterruptorPrincipal, cuadro.Datos.TipoAcometida);

        cuadro.Datos.Hilos = 3;
        Assert.Equal(TipoAcometidaTablero.InterruptorPrincipal, cuadro.Datos.TipoAcometida);
        cuadro.Datos.TipoAcometida = TipoAcometidaTablero.ZapatasPrincipales;
        cuadro.Datos.Fases = 3;
        Assert.Equal(TipoAcometidaTablero.ZapatasPrincipales, cuadro.Datos.TipoAcometida);
    }

    [Fact]
    public void DosYTresBarras_PorOmisionEnZocalo_NoOcupaEspacios()
    {
        var cuadro = Nuevo();
        Assert.Equal(MontajeDelPrincipal.Zocalo, cuadro.Datos.MontajePrincipal);
        Assert.False(cuadro.Datos.PrincipalEnEspacios);
        Assert.Empty(cuadro.EspaciosDelPrincipal);
        Assert.Equal(0, cuadro.EspaciosOcupados);
        Assert.Equal(3, cuadro.Datos.PolosDelPrincipal);
    }

    [Theory]
    [InlineData(3, 4, 24, new[] { 20, 22, 24 }, "ABC")]
    [InlineData(1, 3, 24, new[] { 22, 24 }, "AB")]
    [InlineData(2, 3, 12, new[] { 10, 12 }, "AB")]
    public void EnEspacios_PorOmisionLosUltimosPares_PolosIgualABarras(int fases, int hilos, int espacios, int[] esperados, string barras)
    {
        var cuadro = Nuevo(fases, hilos, espacios);
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Recalcular();

        Assert.Equal(esperados, cuadro.EspaciosDelPrincipal);
        Assert.Null(cuadro.AvisoDelPrincipal);
        Assert.Equal(esperados.Length, cuadro.EspaciosOcupados);
        var bloque = Assert.Single(cuadro.Gabinete, b => b.EsPrincipal);
        Assert.Equal(esperados.Length, bloque.Espacios);
        Assert.Equal(barras, bloque.Barras);
        Assert.All(esperados, e => Assert.True(Espacio(cuadro, e).EsDelPrincipal));
        Assert.All(esperados, e => Assert.False(Espacio(cuadro, e).TieneCarga));
    }

    [Fact]
    public void UnaBarraConPrincipal_VaEnElEspacio1()
    {
        var cuadro = Nuevo(1, 2, 8);
        cuadro.Datos.TipoAcometida = TipoAcometidaTablero.InterruptorPrincipal;
        cuadro.Recalcular();

        Assert.True(cuadro.Datos.PrincipalEnEspacios);
        Assert.Equal([1], cuadro.EspaciosDelPrincipal);
        Assert.Equal(1, cuadro.Datos.PolosDelPrincipal);
    }

    [Fact]
    public void SeMueveAOtroEspacio_YUnEspacioQueNoCabeRegresaAlDeOmision()
    {
        var cuadro = Nuevo();
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Datos.EspacioDelPrincipal = 1;
        cuadro.Recalcular();
        Assert.Equal([1, 3, 5], cuadro.EspaciosDelPrincipal);

        // 21-23-25 no cabe en 24: el valor por omisión, no un principal colgado.
        cuadro.Datos.EspacioDelPrincipal = 21;
        cuadro.Recalcular();
        Assert.Equal([20, 22, 24], cuadro.EspaciosDelPrincipal);
    }

    [Fact]
    public void PorOmision_SiAbajoHayUnCircuito_SubePorLaMismaColumna()
    {
        // La regla del escritorio (AcomodoEnGabinete.UltimoHuecoDeLaColumnaPar).
        var cuadro = Nuevo();
        Espacio(cuadro, 22).Descripcion = "Bomba";
        Espacio(cuadro, 22).Continua = 1000m;
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Recalcular();

        Assert.Equal([16, 18, 20], cuadro.EspaciosDelPrincipal);
        Assert.Null(cuadro.AvisoDelPrincipal);
    }

    [Fact]
    public void UnCircuitoCapturadoDondeSeEligio_NoSeBorra_ElPrincipalNoSeMontaYSeAvisa()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 22).Descripcion = "Bomba";
        Espacio(cuadro, 22).Continua = 1000m;
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Datos.EspacioDelPrincipal = 20;
        cuadro.Recalcular();

        Assert.Empty(cuadro.EspaciosDelPrincipal);
        Assert.Equal("El interruptor principal no se montó en los espacios 20-22-24: el circuito 22 ya está ahí. Mover el principal o el circuito.",
            cuadro.AvisoDelPrincipal);
        Assert.True(Espacio(cuadro, 22).TieneCarga);
        Assert.Equal(1000m, Espacio(cuadro, 22).Continua);

        // Se mueve el principal: se monta y el circuito sigue ahí.
        cuadro.Datos.EspacioDelPrincipal = 2;
        cuadro.Recalcular();
        Assert.Equal([2, 4, 6], cuadro.EspaciosDelPrincipal);
        Assert.Null(cuadro.AvisoDelPrincipal);
        Assert.True(Espacio(cuadro, 22).TieneCarga);
    }

    [Fact]
    public void UnMultipolarNoPuedeCrecerSobreElPrincipal()
    {
        var cuadro = Nuevo();
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Recalcular();

        var motivo = cuadro.CambiarPolos(Espacio(cuadro, 16), 3);
        Assert.NotNull(motivo);
        Assert.Contains("el interruptor principal", motivo);
        Assert.Equal(1, Espacio(cuadro, 16).Polos);
    }

    [Fact]
    public void ElArchivoGuardaElMontaje_YUnArchivoViejoAbreEnZocalo()
    {
        var cuadro = Nuevo();
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Datos.EspacioDelPrincipal = 2;
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.UnixEpoch);
        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;
        Assert.Equal(MontajeDelPrincipal.EnEspacios, abierto.Datos.MontajePrincipal);
        Assert.Equal([2, 4, 6], abierto.EspaciosDelPrincipal);

        var viejo = texto.Replace("\"montajePrincipal\": \"EnEspacios\",", "").Replace("\"espacioDelPrincipal\": 2,", "");
        Assert.DoesNotContain("montajePrincipal", viejo);
        var deAntes = ArchivoDelCuadro.Abrir(viejo, Motor).Cuadro!;
        Assert.Equal(MontajeDelPrincipal.Zocalo, deAntes.Datos.MontajePrincipal);
        Assert.Empty(deAntes.EspaciosDelPrincipal);
    }

    [Fact]
    public void UnArchivo1F2HConPrincipal_NoLoPisanLasZapatasPorOmision()
    {
        var cuadro = Nuevo(1, 2, 8);
        cuadro.Datos.TipoAcometida = TipoAcometidaTablero.InterruptorPrincipal;
        cuadro.Recalcular();

        var abierto = ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.UnixEpoch), Motor).Cuadro!;
        Assert.Equal(TipoAcometidaTablero.InterruptorPrincipal, abierto.Datos.TipoAcometida);
        Assert.Equal([1], abierto.EspaciosDelPrincipal);
    }
}
