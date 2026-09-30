namespace PowerNode.Web.Modelo;

/// <summary>A qué parte del cálculo toca una cita de la guía.</summary>
public enum AlcanceDeReferencia
{
    /// <summary>Cómo se calcula la carga de la salida o del equipo — Art. 220 Parte B y los artículos del equipo.</summary>
    Carga,

    /// <summary>El circuito derivado: conductor, protección, contactos, cargas permisibles.</summary>
    Circuito,

    /// <summary>El alimentador: la suma, el factor de demanda, los motores del grupo.</summary>
    Alimentador,
}

/// <summary>Una cita de la norma: el artículo, sección o tabla, y lo que decide ahí, en corto.</summary>
public sealed record ReferenciaDeGuia(AlcanceDeReferencia Alcance, string Cita, string Regla);

/// <summary>
/// Una rama del árbol de la guía: un tipo de carga, un subtipo, una clase de circuito.
/// </summary>
/// <param name="Id">Para el ancla de la página (<c>#rama-motores</c>) y para enlazar desde la captura.</param>
/// <param name="Nombre">El corto, basado en la NOM: el de la pantalla.</param>
/// <param name="NombreNom">El de la norma, si es distinto del corto.</param>
/// <param name="Que">Qué es, con ejemplos.</param>
/// <param name="Captura">Cómo se captura en Power Node. <c>null</c> si no se captura (una condición).</param>
public sealed record RamaDeGuia(
    string Id,
    string Nombre,
    string? NombreNom,
    string Que,
    string? Captura,
    IReadOnlyList<ReferenciaDeGuia> Referencias,
    IReadOnlyList<RamaDeGuia> Ramas,
    CategoriaDeCarga? Tipo = null,
    SubtipoDeCarga? Subtipo = null);

/// <summary>Un término de la pantalla: el corto, el de la norma, dónde está y cómo se decía antes.</summary>
public sealed record TerminoDeGlosario(string Corto, string Nom, string Donde, string? Antes = null);

/// <summary>
/// <b>La guía de cargas</b> — I-126, decisión <c>cargas-y-clases-de-circuito.md</c>. En qué tipo va
/// cada carga, qué subtipos tiene y a qué artículos, secciones y tablas queda sujeta; las clases de
/// circuito que define el Art. 100 con sus reglas; y el glosario de la pantalla contra la norma.
///
/// <para>
/// <b>Es la fuente única</b>: la página la dibuja y el selector del tipo de cada carga sale de aquí. Si
/// una cita cambia, cambia en los dos lugares. Texto leído del repo <c>dflores296/NOM-001-SEDE-2012</c>
/// (<c>corpus.json</c>, <c>definiciones.json</c>, <c>tablas_revisadas.json</c>), no de memoria.
/// </para>
/// </summary>
public static class GuiaDeCargas
{
    private static ReferenciaDeGuia C(string cita, string regla) => new(AlcanceDeReferencia.Carga, cita, regla);
    private static ReferenciaDeGuia D(string cita, string regla) => new(AlcanceDeReferencia.Circuito, cita, regla);
    private static ReferenciaDeGuia A(string cita, string regla) => new(AlcanceDeReferencia.Alimentador, cita, regla);

    // ---- TIPOS DE CARGA ----------------------------------------------------------------------------

    public static IReadOnlyList<RamaDeGuia> Tipos { get; } =
    [
        new("alumbrado", "Alumbrado", null,
            "Salidas para alumbrado: luminarias, portalámparas, anuncios, aparadores.",
            "En el renglón, la carga total del grupo de salidas (VA, W o A), continua si opera 3 h o más. En el desplegable, " +
            "una línea por tipo de luminaria, con cantidad y valor unitario.",
            [
                C("220-14(d)", "Una salida de luminarias se calcula con el valor máximo en VA del equipo y sus lámparas."),
                C("220-18(b)", "Con balastros, transformadores o LED, con la corriente total de las unidades, no con los watts de las lámparas."),
                C("220-12, Tabla 220-12", "Carga mínima de alumbrado general por m² según el tipo de inmueble (vivienda 33 VA/m², oficinas 39)."),
                D("210-19(a)(1), 210-20(a)", "La carga continua —3 h o más, Art. 100— al 125 % en el conductor y en la protección."),
                D("210-23", "Dos o más salidas: 15 y 20 A para alumbrado; 30 A solo portalámparas de servicio pesado fuera de vivienda; más de 50 A, sin alumbrado."),
                A("220-42, Tabla 220-42", "Factor de demanda del alumbrado general; no se usa para contar los circuitos."),
            ],
            [
                new("alumbrado-luminarias", "Luminarias", "Salidas para alumbrado",
                    "Luminarias y portalámparas de uso general, interiores y exteriores.",
                    "El VA de placa del equipo con sus lámparas; LED o con balastro, en amperes.",
                    [
                        C("220-14(d)", "El valor máximo en VA del equipo y las lámparas."),
                        C("220-18(b)", "Con balastro, transformador o LED, la corriente de las unidades."),
                    ],
                    [], CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias),
                new("alumbrado-portalamparas-pesado", "Portalámparas pesado", "Portalámparas de trabajo pesado",
                    "Portalámparas de servicio pesado (tipo mogul): naves, bodegas, alumbrado industrial.",
                    "No menos de 600 VA por salida.",
                    [
                        C("220-14(e)", "600 VA como mínimo por salida."),
                        D("210-23(b), (c)", "Solo ellos pueden ir en circuitos de 30 a 50 A con alumbrado, y fuera de vivienda."),
                    ],
                    [], CategoriaDeCarga.Alumbrado, SubtipoDeCarga.PortalamparasPesado),
                new("alumbrado-anuncios", "Anuncios y contorno", "Alumbrado de anuncios y de contorno",
                    "Anuncios luminosos e iluminación de contorno.",
                    "No menos de 1200 VA por cada circuito exigido.",
                    [
                        C("220-14(f)", "1200 VA como mínimo por cada circuito derivado exigido en 600-5(a)."),
                        D("600-5(a)", "Un circuito de al menos 20 A, sin otras cargas, en cada entrada de un local comercial con acceso al público."),
                    ],
                    [], CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Anuncios),
                new("alumbrado-aparador", "Aparador", "Aparadores",
                    "Escaparates de tiendas.",
                    "La carga de cada salida, o 200 VA por cada 30 cm de aparador.",
                    [C("220-14(g)", "La carga unitaria por salida, o 200 VA por cada 30 cm de aparador.")],
                    [], CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Aparador),
            ],
            CategoriaDeCarga.Alumbrado),

        new("contactos", "Contactos", "Salidas para contactos",
            "Salidas con uno o más contactos para equipo con cordón y clavija.",
            "En el renglón, la carga total de los contactos. En el desplegable, la cantidad de salidas: cada una cuenta al menos 180 VA.",
            [
                C("220-14(i)", "Al menos 180 VA por contacto sencillo o múltiple en un mismo yugo; múltiple de cuatro o más, 90 VA por contacto."),
                C("220-14(j)", "En vivienda, los contactos de uso general de hasta 20 A van dentro del alumbrado general de 220-12."),
                C("220-14(k)", "En bancos y oficinas, el mayor entre 180 VA por contacto y 11 VA/m²."),
                D("210-21(b)", "El valor del contacto: en circuito individual, no menor que el circuito; con dos o más, según la Tabla 210-21(b)(3)."),
                D("240-4(b)(1)", "Sin el tamaño estándar siguiente sobre la ampacidad si el circuito alimenta más de un contacto para cargas portátiles."),
                D("210-52", "Las salidas para contactos que se exigen en vivienda."),
                A("220-44", "Fuera de vivienda, con los factores de la Tabla 220-42 o de la Tabla 220-44."),
            ],
            [
                new("contactos-uso-general", "Uso general", "Salida para contactos",
                    "Contacto sencillo, dúplex o triple en un mismo yugo.",
                    "La cantidad de salidas; cada una, al menos 180 VA.",
                    [C("220-14(i)", "180 VA por contacto sencillo o múltiple en un mismo yugo.")],
                    [], CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral),
                new("contactos-multiple", "Múltiple (4 o más)", "Contacto múltiple de cuatro o más",
                    "Un yugo con cuatro o más contactos.",
                    "La cantidad de contactos; cada uno, al menos 90 VA.",
                    [C("220-14(i)", "Un contacto múltiple de cuatro o más contactos: no menos de 90 VA por contacto.")],
                    [], CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoMultiple),
                new("contactos-ensamble", "Ensamble de salidas", "Ensamble fijo de múltiples salidas",
                    "Canaletas o regletas fijas con contactos a lo largo, fuera de vivienda.",
                    "La cantidad de tramos; cada uno, al menos 180 VA.",
                    [C("220-14(h)", "Por cada 1.50 m, una salida de 180 VA; por cada 30 cm si los aparatos se usan a la vez.")],
                    [], CategoriaDeCarga.Contactos, SubtipoDeCarga.EnsambleDeSalidas),
            ],
            CategoriaDeCarga.Contactos),

        new("aparatos", "Aparatos", "Aparatos y cargas específicas",
            "Equipo de utilización que se conecta como una unidad —lavar, cocinar, calentar agua— y cualquier otra carga específica, con su placa.",
            "En el renglón, un equipo con su placa: circuito individual. En el desplegable, cada aparato con su cantidad.",
            [
                C("220-14(a)", "Con la corriente del aparato o de la carga conectada."),
                D("422-10(a)", "Circuito individual: no menor que el valor marcado; si es carga continua y no es operado por motor, 125 %."),
                D("422-10(b)", "Con otras cargas, según 210-23."),
                D("422-11(e)", "Un solo aparato no operado por motor: protección no mayor que la marcada; sin marca, 20 A hasta 13.30 A y 150 % arriba."),
                A("220-53", "En vivienda, cuatro o más aparatos fijos en el mismo alimentador: 75 % (no estufas, secadoras, calefacción ni aire acondicionado)."),
            ],
            [
                new("aparatos-fijo", "Fijo", "Aparato fijo",
                    "Lavavajillas, triturador, horno de microondas empotrado, bomba de agua con clavija…",
                    "VA, W o A de placa.",
                    [
                        C("220-14(a)", "Con la corriente del aparato."),
                        A("220-53", "En vivienda, cuatro o más: 75 %."),
                    ],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.AparatoFijo),
                new("aparatos-secadora", "Secadora", "Secadora eléctrica de ropa",
                    "Secadora eléctrica de ropa en vivienda.",
                    "La placa; en vivienda, no menos de 5000 VA.",
                    [
                        C("220-14(b), 220-54", "En vivienda, 5000 VA o la placa, la mayor."),
                        A("Tabla 220-54", "Factores de demanda por número de secadoras."),
                    ],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.Secadora),
                new("aparatos-coccion", "Cocción", "Estufas y otros aparatos de cocción",
                    "Estufa eléctrica doméstica, horno de pared, parrilla empotrada.",
                    "La placa.",
                    [
                        C("220-14(b), 220-55", "En vivienda, aparatos de más de 1.75 kW con la Tabla 220-55."),
                        D("210-19(a)(3)", "Conductores no menores que el circuito ni que la carga; estufa de 8.75 kW o más, circuito de 40 A como mínimo."),
                        A("Tabla 220-55", "Demanda por número de aparatos de cocción."),
                    ],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.Coccion),
                new("aparatos-cocina-comercial", "Cocina comercial", "Equipo de cocina fuera de vivienda",
                    "Equipo de cocción comercial, calentadores de agua de lavaplatos, otros equipos de cocina.",
                    "La placa.",
                    [A("220-56, Tabla 220-56", "Factor de demanda del equipo de cocina; nunca menos que los dos equipos más grandes.")],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial),
                new("aparatos-calentador", "Calentador de agua", "Calentador de agua de tipo con almacenamiento",
                    "Calentador eléctrico de agua con tanque.",
                    "La placa; siempre continua.",
                    [D("422-13", "Con almacenamiento de hasta 450 L: carga continua para dimensionar el circuito.")],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.CalentadorDeAgua),
                new("aparatos-con-motor", "Con motor", "Aparato operado por motor",
                    "Lavadora, extractor, ventilador fijo, triturador: un aparato con motor.",
                    "La corriente de placa del aparato, o sus HP.",
                    [
                        C("430-6(a)(1) Exc. 3", "La corriente de plena carga marcada en el aparato, no la de sus HP."),
                        D("220-18(a)", "Fijo, de más de ⅛ hp y con otras cargas: 125 % del motor mayor más lo demás; solo cargas de motor: Art. 430."),
                        D("422-10(a)", "Circuito individual sin valor marcado: Art. 430 Parte B."),
                        D("422-11(g)", "La protección contra sobrecarga del motor, según el Art. 430 Parte C."),
                    ],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.AparatoConMotor),
                new("aparatos-otra", "Otra carga", "Otra carga específica",
                    "UPS, equipo de cómputo, soldadoras (Art. 630), equipo de rayos X (Art. 660) y lo que no está en otro subtipo.",
                    "VA, W o A de placa; si tiene artículo propio, además lo que él pida.",
                    [C("220-14(a)", "Con la corriente de la carga conectada.")],
                    [], CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica),
            ],
            CategoriaDeCarga.Equipo),

        new("motores", "Motores", null,
            "Motores de uso general, solos o varios en un circuito, con o sin otras cargas.",
            "En HP —la tabla da la corriente— o en A si la placa no trae HP. Varios motores: una línea por motor en el desplegable.",
            [
                C("430-6(a)(1)", "La corriente de las Tablas 430-248 (monofásicos) y 430-250 (trifásicos), no la de placa; en A, interpolando los HP."),
                D("430-22", "Conductor al 125 % de la corriente de tabla."),
                D("430-52, Tabla 430-52", "Protección: 250 % con interruptor de tiempo inverso; el tamaño siguiente — 430-52(c)(1) Exc. 1."),
                D("430-53", "Varios motores, o motores y otras cargas: protección no mayor que 430-52 del mayor más los demás — (c)(4); conductor por 430-24."),
                D("430-32", "La sobrecarga, en el arrancador o en el motor."),
                D("430-110(a)", "Medio de desconexión: no menos de 115 % de la corriente de plena carga."),
                A("430-24", "125 % del motor mayor más los demás y las otras cargas."),
                A("430-62(a), 430-63", "La protección máxima del alimentador con motores."),
                A("430-26", "Factor de demanda solo si alcanza para la carga máxima."),
            ],
            [
                new("motores-uso-general", "Uso general", "Motor de uso general",
                    "Bombas, extractores, compresores de aire, portones: monofásicos a 127 o 220 V, o trifásicos.",
                    "HP o A; servicio continuo por omisión.",
                    [
                        C("Tablas 430-248, 430-250", "La corriente de plena carga por HP y tensión."),
                        D("430-22(c), (d)", "Estrella-delta y devanado dividido: 125 % del lado de línea; 72 % o 62.5 % del arrancador al motor."),
                        D("430-22(e), Tabla 430-22(e)", "Servicio de corta duración, intermitente, periódico o variable: el porcentaje de la tabla sobre la corriente de placa."),
                        A("430-24 Exc. 1", "El motor de servicio no continuo, con el valor de 430-22(e)."),
                        A("430-24 Exc. 3", "Motores que no funcionan a la vez: cuenta el grupo que da la mayor corriente."),
                    ],
                    [], CategoriaDeCarga.Motor, SubtipoDeCarga.MotorUsoGeneral),
                new("motores-velocidad-ajustable", "Velocidad ajustable", "Equipo de conversión de potencia",
                    "Motor con variador de velocidad.",
                    "La corriente nominal de entrada del variador y la protección máxima de su fabricante.",
                    [
                        D("430-122(a)", "Conductor al 125 % de la corriente nominal de entrada del equipo de conversión."),
                        D("110-3(b)", "La protección que indica el fabricante."),
                        D("430-124", "La sobrecarga la da el equipo si así lo marca."),
                        D("430-128", "Medio de desconexión: no menos de 115 % de la corriente de entrada."),
                    ],
                    [], CategoriaDeCarga.Motor, SubtipoDeCarga.MotorVelocidadAjustable),
            ],
            CategoriaDeCarga.Motor),

        new("aire", "A/A y refrig.", "Aire acondicionado y refrigeración",
            "Equipos con motocompresor hermético: minisplit, paquete, condensadora, cámara de refrigeración.",
            "Con la placa: la ampacidad y la protección máxima, o la corriente de carga nominal del motocompresor.",
            [
                C("440-6(a)", "La corriente de placa, no la de las tablas del Art. 430."),
                C("440-3(b)", "Equipo sin motocompresor —manejadora, condensador remoto—: Arts. 422, 424 o 430."),
                C("440-3(c)", "Acondicionador de habitación, refrigerador y congelador domésticos, enfriador de agua: son aparatos (Art. 422)."),
                D("440-22(b), 440-33, 440-34", "Varios motocompresores, o con otros motores y cargas, en un circuito."),
                D("440-12", "Medio de desconexión: no menos de 115 %."),
                A("440-33, 440-7", "125 % del motor o motocompresor mayor más los demás."),
                A("220-60", "Si no funciona a la vez que la calefacción, cuenta el mayor."),
            ],
            [
                new("aire-motocompresor", "Motocompresor", "Motocompresor hermético de refrigeración",
                    "Compresor suelto, unidad condensadora sin ampacidad de placa, cámara de refrigeración.",
                    "La corriente de carga nominal y, si la trae, la de selección del circuito derivado.",
                    [
                        C("440-6(a) Exc. 1", "La mayor entre la corriente de carga nominal y la de selección."),
                        D("440-32", "Conductor al 125 %."),
                        D("440-22(a)", "Protección no mayor que 175 %; 225 % si no arranca; no se exige menos de 15 A."),
                        D("440-52", "La sobrecarga, en el protector del motocompresor."),
                    ],
                    [], CategoriaDeCarga.AireAcondicionado, SubtipoDeCarga.Motocompresor),
                new("aire-carga-combinada", "Carga combinada", "Equipo con varios motores y carga combinada",
                    "Minisplit, paquete, condensadora: la placa trae ampacidad y protección máxima (MCA y MOCP).",
                    "La ampacidad de los conductores y la protección máxima de la placa.",
                    [
                        C("440-4(b)", "La placa marca la ampacidad de los conductores y el valor nominal máximo de la protección."),
                        D("440-35", "Conductor no menor que la ampacidad de placa."),
                    ],
                    [], CategoriaDeCarga.AireAcondicionado, SubtipoDeCarga.CargaCombinada),
                new("aire-habitacion", "De habitación", "Acondicionador de aire para habitación",
                    "Aire de ventana, de consola o de pared, con cordón y clavija, monofásico hasta 250 V.",
                    "La corriente total de placa.",
                    [
                        C("440-60", "Monofásico de hasta 250 V; puede ir con cordón y clavija."),
                        D("440-62(a)", "Una sola unidad de motor, hasta 40 A."),
                        D("440-62(b)", "Solo en su circuito: no más del 80 % del circuito."),
                        D("440-62(c)", "Con alumbrado u otros aparatos: no más del 50 %."),
                        D("440-63", "La clavija puede ser el medio de desconexión."),
                    ],
                    [], CategoriaDeCarga.AireAcondicionado, SubtipoDeCarga.AireDeHabitacion),
            ],
            CategoriaDeCarga.AireAcondicionado),

        new("calefaccion", "Calefacción", "Calefacción eléctrica fija de ambiente",
            "Calefactores fijos de ambiente: resistencias, zoclo, con ventilador.",
            "VA, W o A de placa; siempre continua.",
            [
                C("220-14(a)", "La carga de la salida, con la corriente del equipo."),
                D("424-3(b)", "Los motores y el equipo fijo de calefacción son cargas continuas."),
                A("220-51", "Al 100 % de la carga conectada; nunca menos que el circuito derivado mayor."),
                A("220-60", "Si no funciona a la vez que el aire acondicionado, cuenta el mayor."),
            ],
            [
                new("calefaccion-resistencia", "Resistencia", "Calefactor de resistencia",
                    "Calefactor de zoclo, de pared, de ducto, de resistencia.",
                    "VA, W o A de placa.",
                    [D("424-3(b)", "Carga continua.")],
                    [], CategoriaDeCarga.CalefaccionFija, SubtipoDeCarga.CalefaccionResistencia),
                new("calefaccion-con-motor", "Con motor", "Calefactor con motor",
                    "Calefactor con ventilador.",
                    "VA, W o A de placa.",
                    [
                        D("424-3(b)", "El motor también es carga continua."),
                        A("430-24 Exc. 2", "La ampacidad del alimentador, según 424-3(b)."),
                    ],
                    [], CategoriaDeCarga.CalefaccionFija, SubtipoDeCarga.CalefaccionConMotor),
            ],
            CategoriaDeCarga.CalefaccionFija),

        new("tablero", "Tablero", "Alimentador a tablero",
            "Otro tablero de alumbrado y control alimentado desde un interruptor de este.",
            "La carga calculada del otro tablero, continua y no continua, ya con sus factores de demanda.",
            [
                D("Art. 100", "Es un alimentador, no un circuito derivado: llega hasta el dispositivo de protección de los derivados del otro tablero."),
                D("215-2(a)(1)", "Conductor: la no continua más el 125 % de la continua."),
                D("215-3", "Protección: la no continua más el 125 % de la continua."),
                A("220-40", "La carga del alimentador es la suma después de los factores de demanda: aquí no se aplica otro."),
            ],
            [],
            CategoriaDeCarga.Tablero, SubtipoDeCarga.TableroAlimentado),
    ];

    // ---- CLASES DE CIRCUITO ------------------------------------------------------------------------

    public static IReadOnlyList<RamaDeGuia> Clases { get; } =
    [
        new("clase-derivado", "Circuito derivado", null,
            "Los conductores desde el dispositivo final de protección contra sobrecorriente hasta las salidas (Art. 100).",
            "Cada renglón del cuadro. Su clase sale de sus cargas: no se captura.",
            [
                D("210-19, 210-20", "Conductor y protección: la no continua más el 125 % de la continua."),
                D("240-4", "El conductor, protegido según su ampacidad."),
                D("210-3", "El valor nominal del circuito es el de su protección."),
            ],
            [
                new("clase-individual", "Individual", "Circuito derivado individual",
                    "Alimenta a un solo equipo de utilización (Art. 100).",
                    "Sale solo: una carga de un equipo, cantidad 1.",
                    [
                        D("210-21(b)(1)", "Un contacto sencillo en circuito individual: de valor no menor que el circuito."),
                        D("422-10(a)", "No menor que el valor marcado del aparato."),
                        D("422-11(e)", "Un solo aparato no operado por motor: protección no mayor que la marcada; sin marca, 20 A hasta 13.30 A y 150 % arriba."),
                        D("240-4(b)", "Se permite el tamaño estándar siguiente sobre la ampacidad del conductor, hasta 800 A."),
                    ],
                    [
                        new("clase-refrigerador", "Refrigerador", "Circuito derivado individual del refrigerador",
                            "En vivienda, el contacto del refrigerador en su propio circuito.",
                            "Contactos, uso «Refrigerador».",
                            [
                                D("210-52(b)(1) Exc. 2", "Circuito derivado individual de 15 A o más para el refrigerador."),
                                A("220-52(a) Exc.", "No lleva los 1500 VA de los circuitos para aparatos pequeños."),
                            ],
                            []),
                    ]),
                new("clase-uso-general", "Uso general", "Circuito derivado de uso general",
                    "Alimenta a dos o más salidas para alumbrado y aparatos (Art. 100).",
                    "Sale solo: una carga total o dos o más salidas, con alumbrado.",
                    [
                        D("210-23", "Cargas permisibles por tamaño: 15 y 20 A, alumbrado y equipo; 30 A, portalámparas pesado fuera de vivienda o equipo; 40 y 50 A, cocción; más de 50 A, sin alumbrado."),
                        D("210-23(a)(1)", "Equipo con cordón y clavija no fijo: no más del 80 % del circuito."),
                        D("210-23(a)(2)", "Equipo fijo que no es luminaria, junto con alumbrado o equipo con clavija: no más del 50 %."),
                        D("210-21(b)(2), (3)", "Contactos: la carga con clavija por contacto y el valor del contacto según el circuito."),
                        D("210-24, Tabla 210-24", "Resumen de requisitos de los circuitos de dos o más salidas."),
                        D("240-4(b)(1)", "Con más de un contacto para cargas portátiles, sin el tamaño siguiente sobre la ampacidad."),
                    ],
                    []),
                new("clase-para-aparatos", "Para aparatos", "Circuito derivado para aparatos",
                    "Una o más salidas para aparatos, sin alumbrado conectado permanentemente (Art. 100).",
                    "Sale solo: una carga total o dos o más salidas, sin alumbrado.",
                    [
                        D("210-23", "Cargas permisibles por tamaño."),
                        D("210-21(b)(3), Tabla 210-21(b)(3)", "El valor de los contactos según el del circuito."),
                    ],
                    [
                        new("clase-aparatos-pequenos", "Aparatos pequeños", "Circuito derivado para aparatos pequeños",
                            "En vivienda, los contactos de cocina, despensa, comedor y desayunador.",
                            "Contactos, uso «Ap. pequeños».",
                            [
                                D("210-11(c)(1)", "Dos o más circuitos de 20 A para los contactos de 210-52(b)."),
                                A("220-52(a)", "1500 VA por cada uno en el alimentador; se permite sumarlos al alumbrado general con la Tabla 220-42."),
                            ],
                            []),
                        new("clase-lavadora", "Lavadora", "Circuito derivado para lavadora",
                            "En vivienda, los contactos de la lavadora.",
                            "Contactos, uso «Lavadora».",
                            [
                                D("210-11(c)(2)", "Al menos un circuito de 20 A, sin otras salidas."),
                                A("220-52(b)", "1500 VA en el alimentador."),
                            ],
                            []),
                        new("clase-bano", "Baño", "Circuito derivado para cuartos de baño",
                            "En vivienda, los contactos del cuarto de baño.",
                            "Contactos, uso «Baño».",
                            [D("210-11(c)(3)", "Al menos un circuito de 20 A, sin otras salidas.")],
                            []),
                    ]),
                new("clase-multiconductor", "Multiconductor", "Circuito derivado multiconductor",
                    "Dos o más conductores de fase con un neutro común a todos (Art. 100).",
                    "Circuitos de 1 polo en barras distintas que comparten el neutro de su canalización.",
                    [
                        D("210-4(b)", "Los conductores de fase se desconectan juntos donde nace el circuito."),
                        D("240-15(b)(1)", "Se permiten interruptores de un polo con enclavamiento o con manijas identificadas, solo con cargas de fase a neutro."),
                    ],
                    []),
                new("clase-combinadas", "Combinadas", "Cargas combinadas",
                    "No es una clase: un circuito con cargas de tipos distintos.",
                    null,
                    [
                        D("220-18(a)", "Aparato fijo con motor de más de ⅛ hp y otras cargas: 125 % del motor mayor más lo demás."),
                        D("430-53, 440-22(b)", "Motores o motocompresores y otras cargas: la protección del grupo."),
                        D("440-34", "Motocompresores y alumbrado o aparatos: el conductor, para las dos cargas."),
                        D("210-23(a)(2)", "El equipo fijo, no más del 50 % del circuito junto con alumbrado."),
                    ],
                    []),
            ]),
        new("clase-alimentador", "Alimentador", null,
            "Los conductores hasta el dispositivo final de protección de los circuitos derivados (Art. 100): el del tablero, o el de otro tablero alimentado desde este.",
            "El renglón del alimentador, abajo del cuadro; otro tablero, con el tipo «Tablero».",
            [
                A("215-2(a)(1)", "Conductor: la carga calculada según el Art. 220, la no continua más el 125 % de la continua."),
                A("215-3", "Protección: la no continua más el 125 % de la continua."),
                A("220-40", "La suma de los derivados después de los factores de demanda."),
                A("Art. 220, Parte C", "Los factores de demanda, por tipo de carga: 220-42 a 220-56."),
            ],
            []),
    ];

    // ---- GLOSARIO ----------------------------------------------------------------------------------

    /// <summary>
    /// La pantalla contra la norma: el nombre corto que se ve, el de la NOM, dónde está y cómo se decía
    /// antes. David: «nombres basados en la NOM, pero más cortos» (2026-09-29).
    /// </summary>
    public static IReadOnlyList<TerminoDeGlosario> Glosario { get; } =
    [
        new("Alumbrado", "Alumbrado", "220-12, 220-14(d), 220-42"),
        new("Contactos", "Salidas para contactos", "Art. 100, 220-14(i)"),
        new("Aparatos", "Aparatos y cargas específicas", "220-14(a), Art. 422", "Equipo"),
        new("Motores", "Motores", "Art. 430", "Motor"),
        new("A/A y refrig.", "Aire acondicionado y refrigeración", "Art. 440", "A/C y refrig."),
        new("Calefacción", "Calefacción eléctrica fija de ambiente", "Art. 424"),
        new("Tablero", "Alimentador a tablero", "Art. 100, Art. 215"),
        new("Individual", "Circuito derivado individual", "Art. 100", "Dedicado"),
        new("Uso general", "Circuito derivado de uso general", "Art. 100"),
        new("Para aparatos", "Circuito derivado para aparatos", "Art. 100"),
        new("Multiconductor", "Circuito derivado multiconductor", "Art. 100, 210-4", "Neutro comp."),
        new("Combinadas", "Cargas combinadas", "220-18(a), 440-34, 430-110(c)", "Mixto"),
        new("Salidas y cargas", "Salida; carga", "Art. 100", "Desglose, «Aparato del circuito»"),
        new("Carga total", "Carga declarada por el proyectista, sin desglosar", "—", "La captura del renglón"),
        new("Con motor", "Aparato operado por motor", "430-6(a)(1) Exc. 3, 422-10(a)", "Aparato con motor"),
        new("De habitación", "Acondicionador de aire para habitación", "440 Parte G", "Hab., A/C de cuarto"),
        new("Velocidad ajustable", "Equipo de conversión de potencia", "430 Parte J", "VFD"),
        new("Nominal", "Corriente de carga nominal", "440-6(a)", "RLA"),
        new("Selección", "Corriente de selección del circuito derivado", "440-6(a) Exc. 1", "BCSC, Sel."),
        new("Ampacidad", "Ampacidad de los conductores (placa del equipo)", "440-4(b)", "MCA"),
        new("Prot. máx.", "Valor nominal máximo del dispositivo de protección (placa del equipo)", "440-4(b)", "MOCP"),
        new("Ap. pequeños", "Circuito derivado para aparatos pequeños", "210-11(c)(1)", "Cocina"),
        new("Lavadora", "Circuito derivado para lavadora", "210-11(c)(2)"),
        new("Baño", "Circuito derivado para cuartos de baño", "210-11(c)(3)"),
        new("Refrigerador", "Circuito derivado individual del refrigerador", "210-52(b)(1) Exc. 2"),
        new("Continua", "Carga continua: su corriente máxima circula 3 h o más", "Art. 100"),
        new("F.D.", "Factor de demanda", "Art. 100"),
        new("Carga conectada", "Carga total conectada", "Art. 100 («Factor de demanda»)", "Carga instalada"),
        new("Demanda", "Demanda máxima", "Art. 100 («Factor de demanda»)", "Carga demandada"),
    ];

    // ---- CONSULTAS ---------------------------------------------------------------------------------

    /// <summary>Todas las ramas, en orden, con sus hijas.</summary>
    public static IEnumerable<RamaDeGuia> Todas() => Tipos.Concat(Clases).SelectMany(Aplanar);

    private static IEnumerable<RamaDeGuia> Aplanar(RamaDeGuia r) => r.Ramas.SelectMany(Aplanar).Prepend(r);

    /// <summary>La rama de un tipo de carga: la del «?» del selector.</summary>
    public static RamaDeGuia DeTipo(CategoriaDeCarga tipo) => Tipos.First(r => r.Tipo == tipo);

    /// <summary>La rama de un subtipo.</summary>
    public static RamaDeGuia DeSubtipo(SubtipoDeCarga subtipo) => Todas().First(r => r.Subtipo == subtipo);

    /// <summary>El ancla de una rama en la página de la guía.</summary>
    public static string Ancla(RamaDeGuia r) => $"rama-{r.Id}";
}
