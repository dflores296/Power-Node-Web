using System.Globalization;
using System.Text;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Todo lo que puede enseñar un renglón del cuadro, en una firma</b> — I-188, decisión
/// <c>dibujo-por-renglon.md</c>. La captura se vuelve a dibujar con cada cambio; un renglón cuya firma no cambió
/// se salta, y el navegador no toca sus elementos.
///
/// <para>
/// El riesgo es el de I-184: si algo que el renglón enseña no entra aquí, ese renglón se queda con el valor
/// viejo. Por eso la firma no escoge: lleva <b>todas</b> las propiedades del circuito y de sus líneas, las que hoy
/// se ven y las que no, y <c>FirmaDeDibujoTests</c> cambia cada propiedad con setter y exige que la firma
/// cambie. Una propiedad nueva que no se agregue aquí rompe esa prueba.
/// </para>
/// </summary>
public static class FirmaDeDibujo
{
    /// <summary>La firma de un circuito: lo capturado, lo calculado y sus líneas del desplegable.</summary>
    public static FirmaDelRenglon De(CircuitoDelCuadro c)
    {
        var t = new Texto();
        t.Add(c.Espacio).Add(c.Descripcion).Add(c.DescripcionDelEquipo).Add(c.Categoria).Add(c.TipoElegido).Add(c.SinTipo)
         .Add(c.Uso).Add(c.UsoEfectivo).Add(c.Unidad).Add(c.CapturaMotor).Add(c.Hp).Add(c.CorrientePlacaA)
         .Add(c.CorrienteSeleccionA).Add(c.ArranqueAl225).Add(c.NoArrancaConLaTabla).Add(c.MotorYArrancadorMarcados75C).Add(c.CriterioProteccion)
         .Add(c.ProteccionElegidaA).Add(c.HuellaDelFijado).Add(c.PlacaAire).Add(c.AmpacidadMinimaA).Add(c.ProteccionMaximaA)
         .Add(c.CorrienteEntradaVariadorA).Add(c.ProteccionMaximaVariadorA).Add(c.Servicio).Add(c.EspecificacionServicio)
         .Add(c.CorrientePlacaServicioA).Add(c.CorrienteDeServicioA).Add(c.NoSimultaneoCon).Add(c.OmitidoPorNoSimultaneo)
         .Add(c.CorrienteDeMotorA).Add(c.MotorVA).Add(c.Continua).Add(c.NoContinua).Add(c.ContinuaVA).Add(c.NoContinuaVA)
         .Add(c.LongitudM).Add(c.FactorPotencia).Add(c.Polos).Add(c.PolosElegidos).Add(c.ContinuacionDe).Add(c.EsDelPrincipal)
         .Add(c.Fases).Add(c.Canalizacion).Add(c.ConNeutro).Add(c.LlevaNeutro).Add(c.Error).Add(c.CaidaCombinadaPct)
         .Add(c.CaidaAlimentadorPct).Add(c.AvisoCaidaCombinada).Add(c.Ajuste220_52VA).Add(c.AvisoAireDeHabitacion);
        foreach (var p in c.Porciones)
            t.Add(p.ToString());
        foreach (var r in c.ReglasDeClase)
            t.Add(r.ToString());
        // La canalización se modifica en su lugar en cada recálculo: va por lo que enseña, no por referencia.
        if (c.CanalizacionEfectiva is { } k)
            t.Add(k.Id).Add(k.Nombre).Add(k.Circuitos.Count).Add(k.NingunTamanoAlcanza).Add(k.TamanoRotulo);
        else
            t.Add("sin canalización");

        var objetos = new List<object?>
        {
            c, c.Resultado, c.EntradaDeLaProteccion, c.Sobrecarga, c.MotorEnAmperes, c.MotorAl125,
        };
        foreach (var a in c.Cargas)
        {
            t.Add("línea").Add(a.Descripcion).Add(a.Cantidad).Add(a.Unidad).Add(a.CargaUnitaria).Add(a.Continua).Add(a.NoContinua)
             .Add(a.FactorPotencia).Add(a.TotalVA).Add(a.Clase).Add(a.Subtipo).Add(a.ReferenciaMinimo).Add(a.ReferenciaContinua)
             .Add(a.ProteccionMaximaA).Add(a.CapturaMotor).Add(a.Hp).Add(a.CorrientePlacaA).Add(a.CorrienteSeleccionA)
             .Add(a.CorrienteUnitariaA).Add(a.Error);
            objetos.Add(a);
            objetos.Add(a.MotorEnAmperes);
        }
        return new FirmaDelRenglon(t.ToString(), [.. objetos]);
    }

    /// <summary>
    /// Lo del tablero que leen todos los renglones (sistema, barras, canalizaciones, límites, condiciones). Si
    /// cambia, se dibujan todos, como antes. La identificación (nombre, clave, proyecto…) no entra: ningún renglón la
    /// enseña, y se teclea letra por letra.
    /// </summary>
    public static string De(DatosDelTablero d)
    {
        var t = new Texto();
        t.Add(d.Sistema).Add(d.Fases).Add(d.Hilos).Add(d.NumeroEspacios).Add(d.TensionFaseFaseV).Add(d.FrecuenciaHz)
         .Add(d.TipoAcometida).Add(d.MontajePrincipal).Add(d.EspacioDelPrincipal).Add(d.CapacidadBarraA)
         .Add(d.EsEquipoDeAcometida).Add(d.Inmueble).Add(d.AreaServidaM2).Add(d.UsoTabla220_12).Add(d.SerieInterruptores)
         .Add(d.FactorDemandaAlumbrado).Add(d.FactorDemandaContactos).Add(d.FactorDemandaEquipo).Add(d.FactorDemandaMotores)
         .Add(d.FactorDemandaAireAcondicionado).Add(d.FactorDemandaCalefaccion).Add(d.MaterialConductor).Add(d.TipoAislamiento)
         .Add(d.Lugar).Add(d.TerminalesMarcadas75C).Add(d.TemperaturaAmbienteC).Add(d.CargaNoLineal).Add(d.NeutroReducido220_61)
         .Add(d.TuboAlNacer).Add(d.CaidaMaxDerivadoPct).Add(d.CaidaMaxAlimentadorPct).Add(d.LongitudAlimentadorM)
         .Add(d.ConjuntoAprobado100Pct).Add(d.Montaje).Add(d.MaterialBarras).Add(d.GabineteNema)
         .Add(d.ConductoresPorFaseAlimentador).Add(string.Concat(d.Barras)).Add(d.MaximoPolos);
        foreach (var k in d.Canalizaciones)
            t.Add(k.Id).Add(k.Nombre);
        foreach (var (clave, diametro) in d.DiametrosFabricante.OrderBy(x => x.Key, StringComparer.Ordinal))
            t.Add(clave).Add(diametro);
        return t.ToString();
    }

    /// <summary>Los valores uno tras otro, con un separador que no aparece en ellos y sin depender de la cultura.</summary>
    private sealed class Texto
    {
        private readonly StringBuilder _s = new();

        public Texto Add(object? valor)
        {
            _s.Append(valor switch
            {
                null => "∅",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => valor.ToString(),
            }).Append('␟');
            return this;
        }

        public override string ToString() => _s.ToString();
    }
}

/// <summary>
/// La firma de un renglón: el texto de sus valores y los objetos que enseña. Los objetos se comparan con su
/// igualdad: el circuito y sus líneas, por referencia (otro objeto es otro renglón, aunque diga lo mismo, y sus
/// eventos irían al de antes); los resultados, que son <c>record</c>, por valor.
/// </summary>
public sealed class FirmaDelRenglon(string texto, object?[] objetos) : IEquatable<FirmaDelRenglon>
{
    private readonly string _texto = texto;
    private readonly object?[] _objetos = objetos;

    public bool Equals(FirmaDelRenglon? otra)
    {
        if (otra is null || _texto != otra._texto || _objetos.Length != otra._objetos.Length)
            return false;
        for (var i = 0; i < _objetos.Length; i++)
            if (!Equals(_objetos[i], otra._objetos[i]))
                return false;
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as FirmaDelRenglon);

    public override int GetHashCode() => _texto.GetHashCode(StringComparison.Ordinal);
}
