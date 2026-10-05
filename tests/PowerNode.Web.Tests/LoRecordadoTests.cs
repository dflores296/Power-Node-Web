using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Lo recordado es lo que se calcularía</b> — I-188 (y I-97). El tablero recuerda el resultado de cada derivado
/// y del alimentador por su entrada, para no volver a calcular lo que no cambió. El riesgo es un dato rezagado: un
/// resultado recordado que ya no corresponde a lo capturado. Estas pruebas aplican cientos de cambios al azar
/// (semilla fija, así que se repiten igual) y, después de cada uno, comparan todo lo que el tablero entrega —la
/// memoria de cálculo completa, el alimentador, el resumen y cada renglón— contra lo que sale al borrar lo recordado
/// y calcular de cero. También que recalcular dos veces dé lo mismo.
/// </summary>
public class LoRecordadoTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static readonly JsonSerializerOptions Json = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Todo lo que el tablero entrega, en texto: lo que se compara.</summary>
    private static string Entrega(CuadroDeCarga cuadro)
    {
        var renglones = cuadro.Circuitos.Select(c => new
        {
            c.Espacio, c.Error, c.SinTipo, c.Fases, c.Polos, c.CargaInstaladaVA, c.ContinuaVA, c.NoContinuaVA, c.FactorPotencia,
            c.CaidaCombinadaPct, c.AvisoCaidaCombinada, c.CriterioProteccion, c.ProteccionElegidaA,
            Canal = c.CanalizacionEfectiva is { } k ? $"{k.Nombre} {k.TamanoRotulo} {k.Circuitos.Count}" : null,
            Avisos = cuadro.AvisosDe(c),
            c.Resultado,
            Desglose = cuadro.Desglose(c),
            Ayuda = cuadro.AyudaDeLaProteccion(c),
        });
        var entrega = new
        {
            Renglones = renglones,
            Memoria = MemoriaDeCalculo.Hojas(cuadro),
            HojaDelAlimentador = MemoriaDeCalculo.DelAlimentador(cuadro),
            cuadro.Alimentador,
            cuadro.Resumen,
            cuadro.OpcionesDeParalelo,
            cuadro.AvisoDelPrincipal,
        };
        return JsonSerializer.Serialize(entrega, Json);
    }

    /// <summary>Borra lo recordado (derivados y alimentador): el siguiente recálculo es de cero.</summary>
    private static void Olvidar(CuadroDeCarga cuadro)
    {
        foreach (var nombre in new[] { "_recordados", "_alimentadoresRecordados" })
        {
            var campo = typeof(CuadroDeCarga).GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException($"No existe {nombre}: esta prueba ya no borra lo recordado.");
            ((System.Collections.IDictionary)campo.GetValue(cuadro)!).Clear();
        }
    }

    private static readonly decimal[] HpTrifasicos = [0.5m, 1m, 3m, 5m, 10m, 25m, 50m, 100m];
    private static readonly SubtipoDeCarga[] Subtipos =
        [SubtipoDeCarga.Luminarias, SubtipoDeCarga.ContactoUsoGeneral, SubtipoDeCarga.OtraCargaEspecifica, SubtipoDeCarga.ContactoLavadora];

    /// <summary>Un cambio al azar, como los que hace alguien capturando; devuelve qué hizo, para el mensaje.</summary>
    private static string CambioAlAzar(CuadroDeCarga cuadro, Random azar)
    {
        var d = cuadro.Datos;
        var libres = cuadro.Circuitos.Where(c => !c.EsContinuacion && !c.EsDelPrincipal).ToList();
        var c = libres[azar.Next(libres.Count)];
        switch (azar.Next(19))
        {
            case 0 or 1:
                c.LongitudM = azar.Next(3, 90);
                return $"longitud de {c.Espacio} = {c.LongitudM}";
            case 2 or 3:
            {
                var a = c.AgregarCarga();
                a.Subtipo = Subtipos[azar.Next(Subtipos.Length)];
                a.Cantidad = azar.Next(1, 5);
                a.CargaUnitaria = azar.Next(1, 40) * 100m;
                a.Continua = azar.Next(2) == 0;
                return $"línea en {c.Espacio}: {a.Subtipo} {a.Cantidad} × {a.CargaUnitaria}";
            }
            case 4 when c.Cargas.Count > 0:
            {
                var a = c.Cargas[azar.Next(c.Cargas.Count)];
                a.CargaUnitaria = azar.Next(1, 60) * 50m;
                return $"carga de una línea de {c.Espacio} = {a.CargaUnitaria}";
            }
            case 5 when c.Cargas.Count > 0:
                c.QuitarCarga(c.Cargas[azar.Next(c.Cargas.Count)]);
                return $"quitar una línea de {c.Espacio}";
            case 6:
            {
                c.QuitarEquipo();
                c.Categoria = CategoriaDeCarga.Motor;
                c.Hp = HpTrifasicos[azar.Next(HpTrifasicos.Length)];
                var polos = azar.Next(2) == 0 ? 3 : 1;
                var motivo = cuadro.CambiarPolos(c, polos);
                return $"motor de {c.Hp} HP en {c.Espacio}, {polos} polos{(motivo is null ? "" : " (no cupo)")}";
            }
            case 7:
            {
                var polos = azar.Next(1, d.MaximoPolos + 1);
                return $"polos de {c.Espacio} = {polos}{(cuadro.CambiarPolos(c, polos) is null ? "" : " (no se pudo)")}";
            }
            case 8 when c.Resultado?.Rango is { } rango:
            {
                var v = rango.Valores[azar.Next(rango.Valores.Count)];
                cuadro.FijarProteccion(c, v);
                return $"protección de {c.Espacio} = {v}";
            }
            case 9 when c.EsMotorSolo:
                if (azar.Next(2) == 0)
                {
                    c.MotorYArrancadorMarcados75C = !c.MotorYArrancadorMarcados75C;
                    return $"motor y arrancador a 75 °C en {c.Espacio} = {c.MotorYArrancadorMarcados75C}";
                }
                c.NoArrancaConLaTabla = !c.NoArrancaConLaTabla;
                return $"Excepción 2 de {c.Espacio} = {c.NoArrancaConLaTabla}";
            case 10:
            {
                var otro = libres[azar.Next(libres.Count)];
                c.Canalizacion = otro.Canalizacion;
                return $"canalización de {c.Espacio} = la de {otro.Espacio}";
            }
            case 11:
            {
                var destino = libres[azar.Next(libres.Count)];
                cuadro.MoverCircuito(c.Espacio, destino.Espacio);
                return $"mover {c.Espacio} a {destino.Espacio}";
            }
            case 12:
                d.TemperaturaAmbienteC = new[] { 25m, 30m, 35m, 40m, 45m }[azar.Next(5)];
                return $"temperatura = {d.TemperaturaAmbienteC}";
            case 13:
                d.SerieInterruptores = Enum.GetValues<SerieDeInterruptores>()[azar.Next(3)];
                return $"serie = {d.SerieInterruptores}";
            case 14:
                d.TensionFaseFaseV = new[] { 208m, 220m, 240m }[azar.Next(3)];
                return $"tensión = {d.TensionFaseFaseV}";
            case 15:
                d.LongitudAlimentadorM = azar.Next(5, 120);
                d.ConductoresPorFaseAlimentador = azar.Next(3) == 0 ? azar.Next(1, 3) : null;
                return $"alimentador: {d.LongitudAlimentadorM} m, {d.ConductoresPorFaseAlimentador?.ToString() ?? "auto"} por fase";
            case 16:
                d.NeutroReducido220_61 = !d.NeutroReducido220_61;
                d.CargaNoLineal = azar.Next(2) == 0;
                return $"neutro 220-61 = {d.NeutroReducido220_61}, no lineal = {d.CargaNoLineal}";
            case 17:
            {
                // Un variador, con o sin bypass (M-23).
                c.QuitarEquipo();
                c.Categoria = CategoriaDeCarga.Motor;
                c.CapturaMotor = CapturaDeMotor.Variador;
                c.CorrienteEntradaVariadorA = azar.Next(5, 60);
                c.ProteccionMaximaVariadorA = c.CorrienteEntradaVariadorA * 2m;
                c.HpMotorDelVariador = HpTrifasicos[azar.Next(HpTrifasicos.Length)];
                c.VariadorConBypass = azar.Next(2) == 0;
                var motivo = cuadro.CambiarPolos(c, 3);
                return $"variador de {c.CorrienteEntradaVariadorA} A en {c.Espacio}, bypass {c.VariadorConBypass}{(motivo is null ? "" : " (no cupo)")}";
            }
            default:
                d.Inmueble = Enum.GetValues<TipoDeInmueble>()[azar.Next(Enum.GetValues<TipoDeInmueble>().Length)];
                d.EsEquipoDeAcometida = azar.Next(2) == 0;
                return $"inmueble = {d.Inmueble}, acometida = {d.EsEquipoDeAcometida}";
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void I188_DespuesDeCadaCambioLoRecordadoEsLoQueSeCalcularia(int semilla)
    {
        var azar = new Random(semilla);
        var cuadro = new CuadroDeCarga(Motor) { ExigirTipo = true };
        cuadro.Datos.NumeroEspacios = 42;
        cuadro.Recalcular();

        var hechos = new List<string>();
        for (var paso = 1; paso <= 120; paso++)
        {
            hechos.Add(CambioAlAzar(cuadro, azar));
            cuadro.Recalcular();
            var conLoRecordado = Entrega(cuadro);

            cuadro.Recalcular();
            Assert.True(conLoRecordado == Entrega(cuadro),
                $"Semilla {semilla}, paso {paso}: recalcular otra vez cambió el resultado. Últimos cambios: {string.Join(" · ", hechos.TakeLast(5))}");

            Olvidar(cuadro);
            cuadro.Recalcular();
            Assert.True(conLoRecordado == Entrega(cuadro),
                $"Semilla {semilla}, paso {paso}: con lo recordado salió distinto que de cero. Últimos cambios: {string.Join(" · ", hechos.TakeLast(5))}");
        }
        // La secuencia hizo de todo: si no, la prueba no prueba lo que dice.
        Assert.Contains(cuadro.Circuitos, c => c.TieneCarga);
    }
}
