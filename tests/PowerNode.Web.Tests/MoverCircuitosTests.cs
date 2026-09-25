using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// Arrastrar circuitos y optimizar el acomodo — I-69 (David, 2026-09-25): soltar donde no cabe o
/// donde hay otro deja todo como estaba y lo dice; se puede cambiar de lado; el último movimiento se
/// deshace. La regla de si cabe es la del motor (AcomodoEnGabinete) y la optimización, la del
/// escritorio (BalanceoDeFases).
/// </summary>
public class MoverCircuitosTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo(int espacios = 12)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = espacios;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro E(CuadroDeCarga cuadro, int n) => cuadro.Circuitos[n - 1];

    private static void Capturar(CuadroDeCarga cuadro, int n, string nombre, decimal va, int polos = 1)
    {
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(E(cuadro, n), polos));
        E(cuadro, n).Descripcion = nombre;
        E(cuadro, n).Continua = va;
        cuadro.Recalcular();
    }

    [Fact]
    public void AUnEspacioLibre_SeLlevaTodoLoCapturado_YElOrigenQuedaLibre()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Alumbrado", 900m);
        E(cuadro, 1).Aparatos.Add(new AparatoDelCircuito { Descripcion = "Lámpara", CargaUnitaria = 100m, Cantidad = 9, Continua = true });
        cuadro.Recalcular();
        var corriente = E(cuadro, 1).Resultado!.CorrienteDisenoA;

        var r = cuadro.MoverCircuito(1, 4);

        Assert.True(r.Movio);
        Assert.Equal("Alumbrado", E(cuadro, 4).Descripcion);
        Assert.Single(E(cuadro, 4).Aparatos);
        Assert.Equal(corriente, E(cuadro, 4).Resultado!.CorrienteDisenoA);
        Assert.Equal("B", E(cuadro, 4).Fases); // cambió de lado y de barra
        Assert.False(E(cuadro, 1).TieneCaptura);
    }

    [Fact]
    public void AUnEspacioOcupado_NoCambiaNada_YDiceQuienEsta()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Alumbrado", 900m);
        Capturar(cuadro, 3, "Contactos", 1200m);

        var r = cuadro.MoverCircuito(1, 3);

        Assert.Equal(ResultadoDelMovimiento.Tipo.Ocupada, r.Clase);
        Assert.Equal("Posición ocupada. El espacio 3 lo ocupa el circuito 3.", r.Mensaje);
        Assert.Equal("Alumbrado", E(cuadro, 1).Descripcion);
        Assert.Equal("Contactos", E(cuadro, 3).Descripcion);
        Assert.False(cuadro.PuedeDeshacer);
    }

    [Fact]
    public void DondeNoCabe_NoCambiaNada_YDicePorQue()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Bomba", 6000m, polos: 3);

        var r = cuadro.MoverCircuito(1, 9);

        Assert.Equal(ResultadoDelMovimiento.Tipo.NoValida, r.Clase);
        Assert.StartsWith("Posición no válida. Un interruptor de 3 polos desde el espacio 9 necesita hasta el 13", r.Mensaje);
        Assert.Equal(3, E(cuadro, 1).Polos);
    }

    [Fact]
    public void UnMultipolarSobreSusPropiosEspacios_YAlOtroLado()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Bomba", 6000m, polos: 3);

        Assert.True(cuadro.MoverCircuito(1, 3).Movio);
        Assert.Equal(3, E(cuadro, 3).Polos);
        Assert.Equal(3, E(cuadro, 5).ContinuacionDe);
        Assert.Equal(3, E(cuadro, 7).ContinuacionDe);
        Assert.False(E(cuadro, 1).TieneCaptura);

        // Arrastrar desde un polo que no es el primero mueve el interruptor completo.
        Assert.True(cuadro.MoverCircuito(5, 2).Movio);
        Assert.Equal("Bomba", E(cuadro, 2).Descripcion);
        Assert.Equal("ABC", E(cuadro, 2).Fases);
    }

    [Fact]
    public void SeDeshace_YNoDespuesDeCapturarOtraCosa()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Alumbrado", 900m);
        cuadro.MoverCircuito(1, 6);
        Assert.True(cuadro.PuedeDeshacer);

        cuadro.Deshacer();
        Assert.Equal("Alumbrado", E(cuadro, 1).Descripcion);
        Assert.False(E(cuadro, 6).TieneCaptura);
        Assert.False(cuadro.PuedeDeshacer);

        cuadro.MoverCircuito(1, 6);
        E(cuadro, 2).Descripcion = "Nuevo";
        cuadro.Recalcular();
        Assert.False(cuadro.PuedeDeshacer); // se llevaría el «Nuevo»
    }

    [Fact]
    public void ElPrincipalEnEspacios_TambienSeArrastra()
    {
        var cuadro = Nuevo();
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Recalcular();
        Assert.Equal([8, 10, 12], cuadro.EspaciosDelPrincipal);

        Assert.True(cuadro.MoverCircuito(10, 1).Movio);
        Assert.Equal([1, 3, 5], cuadro.EspaciosDelPrincipal);

        // Y un circuito no puede caer sobre él.
        Capturar(cuadro, 2, "Alumbrado", 900m);
        Assert.Equal(ResultadoDelMovimiento.Tipo.Ocupada, cuadro.MoverCircuito(2, 3).Clase);
    }

    [Fact]
    public void OptimizarBalanceo_ReparteLaCarga_YSeDeshace()
    {
        // Tres circuitos iguales, todos en la barra A (1, 7 y 13 en 3F-4H).
        var cuadro = Nuevo(espacios: 18);
        Capturar(cuadro, 1, "Uno", 1500m);
        Capturar(cuadro, 7, "Dos", 1500m);
        Capturar(cuadro, 13, "Tres", 1500m);
        Assert.Equal(100m, cuadro.Resumen.DesbalanceoPct);

        var propuesta = cuadro.OptimizarBalanceo();

        Assert.True(propuesta.Mejora);
        Assert.Equal(0m, cuadro.Resumen.DesbalanceoPct);
        Assert.Equal(propuesta.DesbalanceoDespuesPct, cuadro.Resumen.DesbalanceoPct);
        Assert.Equal(3, cuadro.Circuitos.Count(c => c.TieneCarga));
        Assert.True(cuadro.PuedeDeshacer);

        cuadro.Deshacer();
        Assert.Equal(100m, cuadro.Resumen.DesbalanceoPct);
        Assert.Equal("Dos", E(cuadro, 7).Descripcion);
    }

    [Fact]
    public void OptimizarBalanceo_SinNadaQueMejorar_NoMueve()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Uno", 1500m);
        Capturar(cuadro, 3, "Dos", 1500m);
        Capturar(cuadro, 5, "Tres", 1500m);

        var propuesta = cuadro.OptimizarBalanceo();

        Assert.False(propuesta.Mejora);
        Assert.False(cuadro.PuedeDeshacer);
    }

    // ---- El zócalo del principal (I-70) ----------------------------------------------------------

    [Fact]
    public void DelZocaloAUnEspacio_QuedaEnEspacios_YSeDeshaceConTodoYMontaje()
    {
        var cuadro = Nuevo();
        Assert.Equal(MontajeDelPrincipal.Zocalo, cuadro.Datos.MontajePrincipal);

        var r = cuadro.MoverCircuito(CuadroDeCarga.Zocalo, 2);

        Assert.True(r.Movio);
        Assert.Equal("El interruptor principal pasó del zócalo a los espacios 2-4-6.", r.Mensaje);
        Assert.Equal(MontajeDelPrincipal.EnEspacios, cuadro.Datos.MontajePrincipal);
        Assert.Equal([2, 4, 6], cuadro.EspaciosDelPrincipal);

        cuadro.Deshacer();
        Assert.Equal(MontajeDelPrincipal.Zocalo, cuadro.Datos.MontajePrincipal);
        Assert.Empty(cuadro.EspaciosDelPrincipal);
    }

    [Fact]
    public void DeLosEspaciosAlZocalo_QuedaEnZocalo_YLiberaSusEspacios()
    {
        var cuadro = Nuevo();
        cuadro.Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        cuadro.Recalcular();

        var r = cuadro.MoverCircuito(10, CuadroDeCarga.Zocalo);

        Assert.True(r.Movio);
        Assert.Equal("El interruptor principal pasó al zócalo propio; los espacios 8-10-12 quedan libres.", r.Mensaje);
        Assert.Equal(MontajeDelPrincipal.Zocalo, cuadro.Datos.MontajePrincipal);
        Assert.Empty(cuadro.EspaciosDelPrincipal);
        Assert.Equal(0, cuadro.EspaciosOcupados);
        Assert.True(cuadro.PuedeDeshacer);
    }

    [Fact]
    public void UnCircuitoAlZocalo_NoCambiaNada_YDiceQueEsDelPrincipal()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, "Alumbrado", 900m);

        var r = cuadro.MoverCircuito(1, CuadroDeCarga.Zocalo);

        Assert.Equal(ResultadoDelMovimiento.Tipo.NoValida, r.Clase);
        Assert.Equal("Posición no válida. El zócalo es solo del interruptor principal.", r.Mensaje);
        Assert.Equal("Alumbrado", E(cuadro, 1).Descripcion);
        Assert.False(cuadro.PuedeDeshacer);
    }

    [Fact]
    public void DelZocaloAUnEspacioOcupado_SeQuedaEnElZocalo()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 4, "Contactos", 1200m);

        var r = cuadro.MoverCircuito(CuadroDeCarga.Zocalo, 2);

        Assert.Equal(ResultadoDelMovimiento.Tipo.Ocupada, r.Clase);
        Assert.Equal("Posición ocupada. El espacio 4 lo ocupa el circuito 4.", r.Mensaje);
        Assert.Equal(MontajeDelPrincipal.Zocalo, cuadro.Datos.MontajePrincipal);
        Assert.Equal(ResultadoDelMovimiento.Tipo.SinCambio, cuadro.MoverCircuito(CuadroDeCarga.Zocalo, CuadroDeCarga.Zocalo).Clase);
    }
}
