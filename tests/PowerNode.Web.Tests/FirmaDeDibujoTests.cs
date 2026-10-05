using System.Reflection;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>La firma de un renglón no deja nada fuera</b> — I-188, decisión <c>dibujo-por-renglon.md</c>. Un renglón
/// cuya firma no cambió no se vuelve a dibujar: si una propiedad no entra en la firma, su cambio no se vería
/// (el riesgo de I-184). Estas pruebas cambian, por reflexión, cada propiedad con setter del circuito, de sus
/// líneas y del tablero, y exigen que la firma cambie. Una propiedad nueva que no se agregue a
/// <see cref="FirmaDeDibujo"/> las rompe.
/// </summary>
public class FirmaDeDibujoTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    /// <summary>La identificación del tablero: ningún renglón la enseña, y no entra a propósito.</summary>
    private static readonly HashSet<string> DatosQueNoEnsenaUnRenglon =
        ["Tablero", "Clave", "Ubicacion", "Proyecto", "Cliente", "Diseno", "Reviso", "Aprobo", "Fecha", "Revision"];

    private static CuadroDeCarga Tablero()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Recalcular();
        return cuadro;
    }

    /// <summary>Un resultado de verdad, para el valor de <c>Resultado</c>.</summary>
    private static ResultadoCircuitoDerivado UnResultado()
    {
        var cuadro = Tablero();
        var c = cuadro.Circuitos.Single(x => x.Espacio == 1);
        var a = c.AgregarCarga();
        a.Subtipo = SubtipoDeCarga.Luminarias;
        a.CargaUnitaria = 500m;
        cuadro.Recalcular();
        return c.Resultado!;
    }

    /// <summary>Otro valor para una propiedad de tipo simple; varios candidatos, porque algunos setters normalizan.</summary>
    private static IEnumerable<object?> Candidatos(Type tipo, object? actual)
    {
        var t = Nullable.GetUnderlyingType(tipo) ?? tipo;
        if (t == typeof(string))
            return [(actual as string ?? "") + "x", "otro valor", null];
        if (t == typeof(bool))
            return [!(actual as bool? ?? false)];
        if (t == typeof(int))
            return [((actual as int?) ?? 0) + 1, ((actual as int?) ?? 0) - 1, 2, 3, 4, 6, 12, 42];
        if (t == typeof(decimal))
            return [((actual as decimal?) ?? 0m) + 1.5m, 127m, 440m, 0.8m, 1m, 2m];
        if (t.IsEnum)
            return [.. Enum.GetValues(t).Cast<object>().Where(v => !v.Equals(actual)), .. tipo != t ? new object?[] { null } : []];
        return [];
    }

    private static bool EsSimple(Type tipo)
    {
        var t = Nullable.GetUnderlyingType(tipo) ?? tipo;
        return t == typeof(string) || t == typeof(bool) || t == typeof(int) || t == typeof(decimal) || t.IsEnum;
    }

    /// <summary>
    /// Cambia cada propiedad con setter de <paramref name="objeto"/> y exige que la firma cambie. Las de tipo
    /// complejo, con el valor de <paramref name="complejos"/>; una que no esté ahí falla.
    /// </summary>
    private static void CadaPropiedadCambiaLaFirma<T>(
        Func<T> nuevo, Func<T, object> firma, IReadOnlyDictionary<string, Func<object?>> complejos, ISet<string>? excluidas = null)
    {
        var faltan = new List<string>();
        foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetSetMethod(nonPublic: true) is not { } setter || p.GetIndexParameters().Length > 0 || excluidas?.Contains(p.Name) == true)
                continue;
            var objeto = nuevo();
            var antes = firma(objeto!);
            var actual = p.GetValue(objeto);
            IEnumerable<object?> valores = EsSimple(p.PropertyType) ? Candidatos(p.PropertyType, actual)
                : complejos.TryGetValue(p.Name, out var valor) ? [valor()]
                : [];
            var cambio = false;
            foreach (var v in valores)
            {
                try { setter.Invoke(objeto, [v]); }
                catch (TargetInvocationException) { continue; } // el setter rechaza ese valor: el siguiente
                if (Equals(p.GetValue(objeto), actual))
                    continue; // lo normalizó al mismo: el siguiente
                cambio = true;
                if (Equals(firma(objeto!), antes))
                    faltan.Add($"{p.Name}: cambió y la firma no");
                break;
            }
            if (!cambio)
                faltan.Add($"{p.Name}: la prueba no supo cambiarla — agrega su valor a la lista de tipos complejos");
        }
        Assert.True(faltan.Count == 0, string.Join("\n", faltan));
    }

    [Fact]
    public void I188_CadaPropiedadDelCircuitoEntraEnLaFirma()
    {
        var resultado = UnResultado();
        CadaPropiedadCambiaLaFirma(
            () => new CircuitoDelCuadro(1),
            c => FirmaDeDibujo.De(c),
            new Dictionary<string, Func<object?>>
            {
                ["Sobrecarga"] = () => new SobrecargaRequerida("430-32(a)", "Relevador de sobrecarga", true),
                ["EntradaDeLaProteccion"] = () => new object(),
                ["Porciones"] = () => new List<PorcionDeCarga> { new(CategoriaDeCarga.Equipo, 100m, 0m, 0m) },
                ["MotorEnAmperes"] = () => new MotorEnAmperes(10m, 0.6m, 0.5m, 8.9m, 0.75m, 11.5m),
                ["MotorAl125"] = () => new CargaDelCircuito(),
                ["CanalizacionEfectiva"] = () => new CanalizacionDelTablero("T9"),
                ["Resultado"] = () => resultado,
                ["ReglasDeClase"] = () => new List<ReglaDeClase> { new("210-23", "Texto", true) },
            });
    }

    [Fact]
    public void I188_CadaPropiedadDeUnaLineaEntraEnLaFirma()
    {
        // El mismo circuito antes y después: uno nuevo en cada firma ya la cambiaría (es otra referencia).
        var circuitoDe = new Dictionary<CargaDelCircuito, CircuitoDelCuadro>(ReferenceEqualityComparer.Instance);
        CadaPropiedadCambiaLaFirma(
            () =>
            {
                var c = new CircuitoDelCuadro(1);
                var a = new CargaDelCircuito();
                c.Cargas.Add(a);
                circuitoDe[a] = c;
                return a;
            },
            a => FirmaDeDibujo.De(circuitoDe[a]),
            new Dictionary<string, Func<object?>>
            {
                ["MotorEnAmperes"] = () => new MotorEnAmperes(10m, 0.6m, 0.5m, 8.9m, 0.75m, 11.5m),
            });
    }

    [Fact]
    public void I188_CadaDatoDelTableroEntraEnLaFirmaSalvoLaIdentificacion()
    {
        CadaPropiedadCambiaLaFirma(
            () => Tablero().Datos,
            d => FirmaDeDibujo.De(d),
            new Dictionary<string, Func<object?>>(),
            DatosQueNoEnsenaUnRenglon);

        // Las listas sin setter que leen los renglones: las canalizaciones (la columna «Canal.») y los diámetros.
        var cuadro = Tablero();
        var antes = FirmaDeDibujo.De(cuadro.Datos);
        var nueva = cuadro.Datos.NuevaCanalizacion();
        var conUna = FirmaDeDibujo.De(cuadro.Datos);
        Assert.NotEqual(antes, conUna);
        Assert.Null(cuadro.Datos.RenombrarCanalizacion(nueva, "Tubo A"));
        Assert.NotEqual(conUna, FirmaDeDibujo.De(cuadro.Datos));
        var conNombre = FirmaDeDibujo.De(cuadro.Datos);
        cuadro.Datos.DiametrosFabricante["THHN|1250"] = 40m;
        Assert.NotEqual(conNombre, FirmaDeDibujo.De(cuadro.Datos));
    }

    [Fact]
    public void I188_LaIdentificacionNoMueveLosRenglones()
    {
        var d = Tablero().Datos;
        var antes = FirmaDeDibujo.De(d);
        d.Tablero = "Cocina";
        d.Cliente = "Alguien";
        Assert.Equal(antes, FirmaDeDibujo.De(d));
    }

    [Fact]
    public void I188_OtroCircuitoConLoMismoEsOtraFirma()
    {
        // Sus eventos irían al circuito de antes (al abrir otro tablero, por ejemplo): se dibuja de nuevo.
        Assert.False(FirmaDeDibujo.De(new CircuitoDelCuadro(1)).Equals(FirmaDeDibujo.De(new CircuitoDelCuadro(1))));
        var c = new CircuitoDelCuadro(1);
        Assert.True(FirmaDeDibujo.De(c).Equals(FirmaDeDibujo.De(c)));
    }

    [Fact]
    public void I188_RecalcularSinCambiosNoCambiaLaFirma()
    {
        var cuadro = Tablero();
        var c = cuadro.Circuitos.Single(x => x.Espacio == 1);
        var a = c.AgregarCarga();
        a.Subtipo = SubtipoDeCarga.Luminarias;
        a.CargaUnitaria = 500m;
        cuadro.Recalcular();
        var antes = FirmaDeDibujo.De(c);
        cuadro.Recalcular();
        Assert.True(antes.Equals(FirmaDeDibujo.De(c)), "con la misma captura, el renglón no se vuelve a dibujar");

        // Otro circuito cambia: este no.
        cuadro.Circuitos.Single(x => x.Espacio == 3).LongitudM = 45m;
        cuadro.Recalcular();
        Assert.True(antes.Equals(FirmaDeDibujo.De(c)));
    }
}
