using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>La celda del renglón del circuito que enmarca una advertencia — estados-de-aviso.md, opción A.</summary>
public enum CeldaDelAviso
{
    /// <summary>La clase del circuito (columna «Tipo»): lo que causa el aviso son sus cargas.</summary>
    Clase,
    Polos,
    Proteccion,
    Canalizacion,
    Caida,
}

/// <summary>
/// Una advertencia de la tabla: el circuito se calcula, pero hay algo que revisar. <paramref name="Texto"/> es la
/// condición y la acción (guía de David, redaccion-de-avisos.md); <paramref name="Referencia"/>, el artículo, que va
/// en otra línea del cuadro de ayuda.
/// </summary>
public sealed record AdvertenciaDeLaTabla(CeldaDelAviso Celda, string Texto, string Referencia);

/// <summary>
/// El error de un circuito, como lo dice la tabla: <paramref name="Texto"/> en las celdas de resultado;
/// <paramref name="Campo"/>, la columna del desplegable con el dato que falta o que choca (el nombre de la columna
/// en la pantalla, p. ej. «Tension», «Hp»), y <paramref name="Linea"/>, la línea donde está (<c>null</c>: el equipo
/// del circuito). <paramref name="Original"/> es el mensaje completo del cálculo, que sigue yendo a la memoria.
/// </summary>
public sealed record ErrorDeLaTabla(string Texto, string? Campo, CargaDelCircuito? Linea, string Original);

/// <summary>
/// <b>Lo que la tabla de captura le dice al usuario</b> — guía de David (decisiones/redaccion-de-avisos.md) y los
/// textos aprobados el 2026-10-07 (conocimiento/avisos-de-la-captura.md, secciones B, C y F). La pantalla traduce:
/// el cálculo y sus mensajes no cambian, y la memoria sigue con la justificación completa. Un caso que no se
/// conoce dice «No se puede calcular el circuito. Revisa sus datos.» (C31).
/// </summary>
public static class AvisosDeLaTabla
{
    public const string Desconocido = "No se puede calcular el circuito. Revisa sus datos.";

    /// <summary>El error del circuito, traducido; <c>null</c> si calcula.</summary>
    public static ErrorDeLaTabla? ErrorDe(CuadroDeCarga cuadro, CircuitoDelCuadro c)
    {
        if (c.Error is not { } original)
            return null;
        // En un grupo o un desglose, el error de una máquina llega como «{nombre}: {error}».
        var linea = c.Cargas.FirstOrDefault(a => a.EsMaquina && a.Error is not null && original.EndsWith(a.Error, StringComparison.Ordinal));
        var (texto, campo) = Traducir(cuadro, c, linea, linea?.Error ?? original);
        return new ErrorDeLaTabla(texto, campo, linea, original);
    }

    private static (string Texto, string? Campo) Traducir(CuadroDeCarga cuadro, CircuitoDelCuadro c, CargaDelCircuito? linea, string m)
    {
        bool Empieza(string s) => m.StartsWith(s, StringComparison.Ordinal);
        bool Tiene(string s) => m.Contains(s, StringComparison.Ordinal);
        var datos = cuadro.Datos;

        if (m == CuadroDeCarga.MensajeSinTipo)
            return ("Falta el tipo de carga.", "Tipo");
        if (m == CuadroDeCarga.MensajeUnPoloSinNeutro)
            return ("El sistema 3F-3H no tiene neutro. Cambia el circuito a 2 o 3 polos.", null);

        // C1 a C3: la tensión de placa.
        if (Empieza("Falta la tensión de entrada"))
            return ("Falta la tensión de entrada.", "Tension");
        if (Empieza("Falta la tensión de placa"))
            return ("Falta la tensión de placa.", "Tension");
        if (Empieza("La tensión de placa (") && Tiene("no es de este tablero"))
            return ("La tensión de placa no es de este tablero. Escoge otra tensión.", "Tension");
        if (Empieza("La tensión de placa (") && c.TensionDePlaca is { } t)
            return ($"La tensión de placa pide {t.Polos()} polos. Escoge otra tensión o cambia los polos.", "Tension");

        // C4, C5: el servicio del motor.
        if (Empieza("430-22(e):"))
            return ("Falta la corriente de placa.", "CorrientePlaca");
        if (Empieza("Tabla 430-22(e):"))
            return ("Faltan los minutos del servicio.", "Servicio");

        // C6, C7, C8 del variador.
        if (Empieza("430-53: el grupo no tiene motores") || Empieza("430-53: el grupo necesita"))
            return ("El grupo no tiene motores.", null);
        if (Empieza("430-122(b): con bypass"))
            return ("Faltan los HP del motor.", "HpMotor");
        if (Empieza("430-122(b): la tabla no trae"))
            return ($"La tabla no trae un motor de {MotoresEnHp.Texto(c.HpMotorDelVariador ?? 0m)} HP a {cuadro.TensionDelMotorV(c.Polos):0} V. Escoge otros HP.", "HpMotor");

        // C8, C9, C26: la tabla de motores no trae la fila.
        if (Tiene("no trae motores"))
            return ($"La tabla no trae motores {(c.Polos == 3 ? "trifásicos" : "monofásicos")} a {cuadro.TensionDelMotorV(c.Polos):0} V. Cambia los polos.", null);
        if (Tiene("no trae un motor") || Empieza("Las Tablas 430-247"))
        {
            var hp = linea?.Hp ?? c.Hp ?? 0m;
            return ($"La tabla no trae un motor de {MotoresEnHp.Texto(hp)} HP a {cuadro.TensionDelMotorV(c.Polos):0} V. Escoge otros HP.", "Hp");
        }
        if (Tiene("pasa del más grande"))
        {
            var amperes = linea?.CorrientePlacaA ?? c.CorrientePlacaA;
            return ($"La tabla no trae un motor de {amperes:0.##} A a {cuadro.TensionDelMotorV(c.Polos):0} V. Revisa la corriente de placa.", "CorrientePlaca");
        }

        // C10 a C14: las cargas del desplegable.
        if (Empieza("220-18(a): el circuito solo alimenta motores"))
            return ("El circuito lleva motores y líneas sin carga. Quita las líneas sin carga.", null);
        if (Empieza("Un alimentador a tableros solo lleva tableros"))
            return ("Un alimentador a tableros solo lleva tableros. Pasa las demás cargas a otro circuito.", null);
        if (Tiene("va solo en su circuito y es uno"))
            return c.Cargas.Count > 1
                ? ("El contacto del refrigerador va solo. Pasa las demás cargas a otro circuito.", null)
                : ("El contacto del refrigerador es uno. Deja la cantidad en 1.", "Cant");
        if (Empieza("Contactos · ") && Tiene("el circuito solo alimenta esas salidas"))
        {
            var exclusiva = c.Cargas.FirstOrDefault(a => a.Subtipo?.SoloConSuSubtipo() == true)?.Subtipo;
            var uso = exclusiva switch
            {
                SubtipoDeCarga.ContactoAparatosPequenos => "aparatos pequeños",
                SubtipoDeCarga.ContactoLavadora => "lavadora",
                _ => "baño",
            };
            return ($"Los contactos de {uso} van solos. Pasa las demás cargas a otro circuito.", null);
        }

        // C15 a C25: lo que el cálculo de equipos no puede calcular.
        if (Empieza("440-6(a): falta"))
            return ("Falta la corriente de placa.", "CorrienteNominal");
        if (Empieza("440-4(b): falta la ampacidad"))
            return ("Falta la ampacidad mínima (MCA).", "Ampacidad");
        if (Empieza("440-4(b): falta la protección"))
            return ("Falta la protección máxima.", "ProtMaxPlaca");
        if (Empieza("440-62(a)(3): falta"))
            return ("Falta la corriente de placa.", "CorrienteTotal");
        if (Empieza("430-122(a): falta"))
            return ("Falta la corriente de entrada.", "CorrienteEntrada");
        if (Empieza("110-3(b): falta"))
            return ("Falta la protección máxima.", "ProtMaxFabricante");
        if (Empieza("440-60"))
            return ("Un acondicionador trifásico o de más de 250 V no es de habitación. Escoge otro subtipo de A/A.", "Subtipo");
        if (Empieza("440-62(a)(2)"))
            return ("Un acondicionador de más de 40 A no es de habitación. Escoge otro subtipo de A/A.", "Subtipo");
        if (Tiene("es menor que el tamaño estándar más chico"))
        {
            var maxima = Empieza("110-3(b)") ? c.ProteccionMaximaVariadorA : c.ProteccionMaximaA;
            return ($"La protección máxima de {maxima:0.##} A es menor que cualquier interruptor. Revisa la placa.",
                Empieza("110-3(b)") ? "ProtMaxFabricante" : "ProtMaxPlaca");
        }
        if (Empieza("430-53(c)(2)"))
            return ("La protección del variador no alcanza la corriente del grupo. Pasa el variador a su propio circuito.", null);
        if (Tiene("no deja un interruptor que lleve la corriente"))
            return ("Ninguna protección cabe para la corriente del grupo. Divide los motores en más circuitos.", null);

        // C27 a C30: el conductor y las condiciones de cálculo.
        if (Empieza("Ni subiendo hasta"))
            return ("Ningún conductor alcanza la corriente del circuito. Divide la carga en más circuitos.", null);
        if (Empieza("Ni con el calibre más grande"))
            return ($"Ningún conductor baja la caída a {datos.CaidaMaxDerivadoPct:0.##} %. Acorta el circuito o sube la caída permitida en Condiciones de cálculo.", null);
        if (Tiene("no se reconoce, o no es válido para el lugar") || Tiene("no vale en lugar"))
            return ($"El aislamiento {datos.TipoAislamiento} no vale en lugar {datos.Lugar.Nombre()}. Cambia el aislamiento en Condiciones de cálculo.", null);
        if (Empieza("La Tabla 310-15(b)(2)(a) no cubre"))
            return ($"La temperatura ambiente de {datos.TemperaturaAmbienteC:0.##} °C está fuera de tabla. Cambia la temperatura en Condiciones de cálculo.", null);

        return (Desconocido, null);
    }

    /// <summary>Las advertencias del circuito, cada una con la celda que la causa (B7 a B18).</summary>
    public static IReadOnlyList<AdvertenciaDeLaTabla> AdvertenciasDe(CuadroDeCarga cuadro, CircuitoDelCuadro c)
    {
        if (c.Resultado is not { } r)
            return [];
        var p = r.ProteccionA;
        var lista = new List<AdvertenciaDeLaTabla>();
        foreach (var regla in c.ReglasDeClase.Where(x => x.Aviso))
            lista.Add(DeLaRegla(cuadro, regla, p));
        if (c.AvisoCaidaCombinada is not null)
            lista.Add(new(CeldaDelAviso.Caida, $"Caída combinada de {c.CaidaCombinadaPct ?? 0m:N2} %. Baja la caída permitida en Condiciones de cálculo.",
                "215-2(a)(4) nota 2"));
        if (c.AvisoAireDeHabitacion is { } habitacion)
            lista.Add(habitacion.Contains("440-62(c)", StringComparison.Ordinal)
                ? new(CeldaDelAviso.Proteccion, "El acondicionador usa más del 50 % del circuito. Pasa las demás cargas a otro circuito.", "440-62(c)")
                : habitacion.Contains("440-62(b)", StringComparison.Ordinal)
                    ? new(CeldaDelAviso.Proteccion, "El acondicionador usa más del 80 % del circuito. Revísalo.", "440-62(b)")
                    : new(CeldaDelAviso.Clase, "El acondicionador no es de habitación. Escoge otro subtipo de A/A.", "440-60, 440-62(a)(2)"));
        if (c.EntradaDeLaProteccion is DatosEntradaCircuitoDerivadoVariador { Bypass: not null } && r.Rango is { } rango && rango.MaximoA < rango.CapacidadMinimaA)
            lista.Add(new(CeldaDelAviso.Proteccion,
                $"Con bypass, ninguna protección cabe entre {rango.CapacidadMinimaA:N2} y {rango.MaximoA:N0} A. Revísalo con el fabricante.", "430-122(b)"));
        if (c.CanalizacionEfectiva is { NingunTamanoAlcanza: true })
            lista.Add(new(CeldaDelAviso.Canalizacion, "Ningún tubo admite todos los conductores. Reparte los circuitos en más canalizaciones.", "Cap. 10, Tabla 1"));
        return lista;
    }

    private static AdvertenciaDeLaTabla DeLaRegla(CuadroDeCarga cuadro, ReglaDeClase regla, decimal p) => regla.Referencia switch
    {
        "Tabla 210-21(b)(3)" => new(CeldaDelAviso.Proteccion, $"Contactos de 15 o 20 A en un circuito de {p:N0} A. Divide los contactos en circuitos de 20 A.", regla.Referencia),
        "210-23(a)(2)" => new(CeldaDelAviso.Proteccion, "El equipo fijo usa más del 50 % del circuito. Pasa el equipo a otro circuito.", regla.Referencia),
        "210-23(b)" or "210-23(c)" => new(CeldaDelAviso.Proteccion, $"Alumbrado común en un circuito de {p:N0} A. Divide el alumbrado en circuitos de 20 A.", regla.Referencia),
        "210-23(d)" => new(CeldaDelAviso.Proteccion, $"Alumbrado en un circuito de {p:N0} A. Pasa el alumbrado a otro circuito.", regla.Referencia),
        "210-6(a)(2)" => new(CeldaDelAviso.Polos, $"Contactos de vivienda a {cuadro.Datos.TensionFaseNeutroV:0} V. Pasa los contactos a un circuito de 127 V.", regla.Referencia),
        // Las que el catálogo aprobado no traía (propuesta de Claude, 2026-10-07), con la misma guía.
        "210-3" => new(CeldaDelAviso.Proteccion, $"Varias salidas en un circuito de {p:N0} A. Divide las cargas en circuitos individuales.", regla.Referencia),
        "422-11(e)" => new(CeldaDelAviso.Proteccion, $"Protección de {p:N0} A para un aparato sin valor marcado. Revisa la placa del aparato.", regla.Referencia),
        "600-5(a)" => new(CeldaDelAviso.Clase, "Anuncios con otras cargas. Pasa las demás cargas a otro circuito.", regla.Referencia),
        "600-5(b)(2)" => new(CeldaDelAviso.Proteccion, $"Anuncios en un circuito de {p:N0} A. Divide los anuncios en circuitos de 20 A.", regla.Referencia),
        "serie de interruptores" => new(CeldaDelAviso.Polos,
            $"Interruptor de 1 polo de {p:N0} A; la familia llega por lo común a {cuadro.Datos.SerieInterruptores.MaximoUnPolo():N0} A. Pasa el circuito a 2 o 3 polos.",
            "Serie de interruptores"),
        _ => new(CeldaDelAviso.Proteccion, regla.Texto, regla.Referencia),
    };

    /// <summary>F1: una protección fijada que regresa al cálculo, en el aviso flotante.</summary>
    public static string ProteccionQueRegreso(IReadOnlyList<ProteccionQueRegreso> regresaron) => regresaron switch
    {
        [var una] => $"La protección fijada del circuito {una.Espacio} ya no está en su rango. Regresa a {una.CalculadaA ?? una.MaximoA:N0} A.",
        _ => $"Las protecciones fijadas de los circuitos {string.Join(", ", regresaron.Select(x => x.Espacio))} ya no están en su rango. Regresan al cálculo.",
    };
}
