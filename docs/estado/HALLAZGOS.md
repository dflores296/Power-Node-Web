# Hallazgos

Registrar cada defecto con ID estable, prioridad y commit de cierre. Ningún hallazgo se cierra sin
commit.

ID: `<letra>-<número>` — `E` estructura, `P` publicación, `M` motor, `I` interfaz, `R` revisión de David
(2026-09-23). Prioridad: `P0`
(bloquea) a `P3` (cosmético).

| ID | Hallazgo | Prioridad | Estado | Commit |
|---|---|---|---|---|
| E-01 · `TablasNom` dependía de código de coordinación | P2 | **Cerrado** | `bfc9d47` |
| P-01 · Sin workflow de despliegue a GitHub Pages | P1 | **Cerrado** | `c46954f` |
| P-02 · Pages publicaba el README en lugar de la aplicación | P1 | **Cerrado** | Configuración de Pages (David) |
| P-03 · El navegador conservaba CSS e iconos anteriores | P1 | **Cerrado** | `dfc674a` |
| M-01 · Sin tablas de la norma sin base de datos | P0 | **Cerrado** | `c46954f` |
| M-02 · Alimentador dimensionado con la carga total, no con la fase más cargada | P0 | **Cerrado** | `b37de00` |
| M-03 · Sin aviso con principal menor que el derivado más grande | P1 | **Cerrado** (aviso) | `b37de00` |
| M-04 · 240-4(b) aplicada solo en Alumbrado | P3 | **Cerrado** | `3aa1c3a` |
| M-05 · 125 % comparado contra la ampacidad corregida — 210-19(a)(1) | P2 | **Cerrado** | `74d6783` |
| M-06 · Sin declaración de equipo marcado 75 °C — 110-14(c)(1)a.(3) | P2 | **Cerrado** | `3aa1c3a` |
| I-01 · Sin pantalla | P0 | **Cerrado** | `c46954f` |
| I-02 · `NumeroFases` tomado del tablero y no del circuito | P0 | **Cerrado** | `c46954f` |
| I-03 · Sin piso práctico de calibre | P1 | **Revertido** (decisión de David) | — |
| I-04 · Sin resumen de carga ni balanceo por fase | P2 | **Cerrado** | `cc92ad5` |
| I-05 · Sin guardar ni abrir el proyecto | P1 | **Cerrado** | `0169b78` |
| I-06 · Pantalla de arranque sin estilos e icono de Blazor | P2 | **Cerrado** | `7cf69c5` |
| I-07 · Sin `favicon.ico` | P1 | **Cerrado** | `c19cca6` |
| I-08 · Selector de tipo con texto cortado | P2 | **Cerrado** | `1b53276` |
| I-09 · Sin datos de identificación del tablero | P2 | **Cerrado** | `cc92ad5` |
| I-10 · Fase del espacio calculada en la pantalla | P1 | **Cerrado** | `cc92ad5` |
| I-11 · Multipolar sin ocupar sus espacios | P1 | **Cerrado** | `cc92ad5` |
| I-12 · Sin alimentador ni interruptor principal | P1 | **Cerrado** | `cc92ad5` |
| I-13 · Sin documento imprimible ni memoria | P1 | **Cerrado** | `cc92ad5` |
| I-14 · Tensión F-N siempre ÷√3 | P1 | **Cerrado** | `cc92ad5` |
| I-15 · Sin captura de circuitos de Fuerza (Art. 430) | P2 | Pendiente | — |
| I-16 · Encabezados en dos líneas | P2 | **Cerrado** | `c2bb19c` |
| I-17 · Sin hilos, mm² ni designación por conductor | P2 | **Cerrado** | `c2bb19c` |
| I-18 · Sin dibujo del interior del tablero | P1 | **Cerrado** | `c2bb19c` |
| I-19 · Barras de un multipolar en negro | P2 | **Cerrado** | `4730182` |
| I-20 · Unidades en mayúsculas | P2 | **Cerrado** | `4730182` |
| I-21 · Líneas entre columnas incompletas | P2 | **Cerrado** | `4730182` |
| I-22 · Clase `grupo` compartida entre cuadro y tarjetas | P1 | **Cerrado** | `228478d` |
| I-23 · Multipolar mostrado como texto, sin celdas combinadas | P2 | **Cerrado** | `acdcf42` |
| I-24 · Borde faltante en el renglón de continuación | P2 | **Cerrado** | `73cb16b` |
| I-25 · Captura solo en VA | P1 | **Cerrado** | `b37de00` |
| I-26 · Un factor de potencia para todo el tablero | P1 | **Cerrado** | `2ebf20f` |
| I-27 · «Total (kW)» = VA × F.P. del tablero | P2 | **Cerrado** | `2ebf20f` |
| I-28 · Avisos del principal con referencia al Excel | P2 | **Cerrado** | `e7fdf0a` |
| I-29 · 16, 32 y 63 A seleccionables en centro de carga | P2 | **Cerrado** | `b9406ab` |
| I-30 · Tooltip de «Tipo» con piso de calibre retirado | P3 | **Cerrado** | `b37de00` |
| I-31 · «Acometida» con texto cortado | P3 | **Cerrado** | `b9406ab` |
| I-32 · Aislamiento fijo en THHN | P0 | **Cerrado** | `b1a84d6` |
| I-33 · Memoria, sección 4, con capacidad mínima rotulada como In; sin desglose en pantalla | P1 | **Cerrado** | `13b3093` |
| I-34 · «Agrupados» sin regla de conteo | P3 | **Cerrado** | `3aa1c3a` |
| I-35 · Sin desglose de los aparatos de un circuito | P2 | **Cerrado** | `e429cbb` |
| I-36 · Carga con decimales cortada en su campo | P2 | **Cerrado** | `8235454` |
| I-37 · Hilos que no existen para las fases elegidas | P1 | **Cerrado** | `8235454` |
| I-38 · En móvil la página se aleja y todo se ve a dos tercios | P1 | **Cerrado** | `6d7eb4b` |
| I-39 · Un solo «Agrupados» para derivados y alimentador | P1 | **Cerrado** | `6e88585` |
| I-40 · Sin cálculo de la canalización | P2 | **Cerrado** | `6e88585`, `04a9f9f` |
| I-41 · Neutro en todos los circuitos multipolares | P2 | **Cerrado** | `6e88585` |
| M-07 · Errata de la Tabla 5: TW 10 AWG con 55.68 mm² | P1 | **Cerrado** | `6e88585` |
| I-42 · Selector de aislamiento con 7 de 17 tipos | P1 | **Cerrado** | `23da7ce` |
| M-08 · THW rechazado en lugar seco | P1 | **Cerrado** | `23da7ce` |
| I-43 · Canalización de los circuitos sin compartida fijada en Condiciones de cálculo | P2 | **Cerrado** | `58febd2` |
| I-44 · «Propia» como nombre de tubería; opciones apiladas que ensanchaban el renglón; columna vacía | P2 | **Cerrado** | `c7ab778` |
| I-45 · El selector «Canal.» mostraba otro tubo que el del circuito | P1 | **Cerrado** | `2ddc0b5` |
| I-46 · Reglas de vivienda (210-11(c), 220-52) aplicadas fuera de vivienda | P1 | **Cerrado** | `1cebf41` |
| I-47 · F.P. y corriente de neutro del alimentador distintos de los de la caída | P2 | **Cerrado** | `1cebf41` |
| I-48 · «1 polos» en el interruptor principal | P3 | **Cerrado** | `1cebf41` |
| I-49 · Números con más de dos decimales y formatos distintos para el mismo valor | P2 | **Cerrado** | `9fe6995` |
| I-50 · Fórmula de caída de la memoria sin «÷ 1000» (L en m, R y X en Ω/km) | P2 | **Cerrado** | `9fe6995` |
| I-51 · Selector de uso de contactos desalineado bajo el de tipo | P3 | **Cerrado** | `5cdc8bb` |
| I-52 · F.P. cortado en «0.9» tras I-49 | P2 | **Cerrado** | `5cdc8bb` |
| I-53 · Tensión sin rótulo correcto ni aviso: 1F-2H a 220 V F-N, 1F-3H a 220/110 V | P1 | **Cerrado** | `317110e` |
| I-54 · 1F-2H con nones y pares y gabinetes de 12 a 42 espacios | P3 | **Cerrado** | `e7af563`, `5f74680` |
| I-55 · Campo numérico: el «0» se sumaba a lo tecleado; un campo vaciado dejaba el valor anterior en el cálculo | P1 | **Cerrado** | `dbc8f70` |
| I-56 · Navegación con teclado: Enter sin moverse, flechas que cambiaban valores, ayudas solo con ratón | P2 | **Cerrado** | `a5e5b3d` |
| I-57 · Sin tema oscuro; la pestaña seguía con el icono del unifilar | P3 | **Cerrado** | `f84d575` |
| I-58 · Encabezado que se iba al bajar; el documento sin navegación a la vista | P3 | **Cerrado** | `af8e83b` |
| I-59 · Pantalla de carga sin avance; logo chico en la carga y en la barra | P3 | **Cerrado** | `04a2e56` |
| I-60 · La carta de la carga no se veía completa; hueco en el encabezado del documento; resumen fuera de su tarjeta a 360 px | P3 | **Cerrado** | `da669de` |
| I-61 · Sin enlace al repositorio en la barra | P3 | **Cerrado** | `005be3c` |
| I-62 · Caracteres como iconos (▸ ✕ ↺ ◐ ☀ ☾), distintos en cada sistema; la firma pedía una fuente que no se cargaba | P3 | **Cerrado** | `5518836` |
| I-63 · Sin iconos de tipo de carga; el resumen se salía de su tarjeta arriba de 1400 px | P3 | **Cerrado** | `a7e6219` |
| I-64 · Interfaz plana, con poco contraste entre fondo, tarjetas y títulos | P3 | **Cerrado** | `3ed4e44` |
| I-65 · Líneas de tabla tibias junto al relieve; en oscuro el relieve no se veía | P3 | **Cerrado** | `7ade4dc` |
| I-66 · El gabinete dibujaba cada interruptor como texto («20 A»), sin el aparato | P3 | **Cerrado** | `HASH` |
| R-01 · Caída del alimentador fija en 5 %, sin verificar la combinada — 215-2(a)(4) NOTA 2 | P1 | **Cerrado** | `2a973ea` |
| R-02 · Caída del alimentador sin la caída del neutro | P2 | **Cerrado** | `d6cd1c6` |
| R-03 · La publicación no corre `PowerNode.Web.Tests` | P1 | **Cerrado** | `986c9d7` |
| R-04 · Cálculo por fase del alimentador resuelto en la web, no en el motor | P2 | **Cerrado** | `d6cd1c6` |
| R-05 · Documento de pruebas, 4.2 con F.P. 0.8: 2.01 % en vez de 2.00 % | P3 | **Cerrado** | `986c9d7` |
| R-06 · Aviso de riel DIN > 125 A con la sintaxis rota | P3 | **Cerrado** | `de855ee` |
| R-07 · Sin las pruebas del documento del 2026-09-23 | P1 | **Cerrado** | `986c9d7` |
| R-08 · Principal menor que un derivado: aviso o bloqueo sin decidir | P3 | **Cerrado** (aviso) | `ab9c785` |
| R-09 · 2F-3H: neutro portador sin cita en la memoria ni prueba — 310-15(b)(5)(2) | P2 | **Cerrado** | `04c3df3` |
| R-10 · 2F-3H 220Y/127: sin prueba de que no se aplican 220-61(a) excepción ni 310-15(b)(7) | P2 | **Cerrado** | `c942ccb` |
| R-11 · Sin mínimo del principal según 230-79 | P2 | **Cerrado** | `62fc7f9` |
| R-12 · Sin factores de demanda del Art. 220 por tipo de inmueble | P2 | **Cerrado** (justificación; el factor, a criterio) | `eb50462` |
| R-14 · Mínimo de 20 A en todos los contactos (criterio del Excel); sin 210-11(c) ni 220-52 | P1 | **Cerrado** | `ab9c785` |
| R-15 · Límites de caída por omisión 3 % + 3 % = 6 %, contra el 5 % combinado; aviso de caída combinada dentro de la tabla | P2 | **Cerrado** | `6ca60a1` |
| R-19 · Inmueble solo para 230-79 (3 opciones) y no para el factor de demanda | P2 | **Cerrado** | `be9985f` |
| R-18 · Motores / A/C y calefacción fijos en 1.00, contra 430-26 y 220-51 Excepción; calefacción capturable como no continua | P2 | **Cerrado** | `b3020f0` |
| R-17 · Factor de demanda por continua / no continua, no por tipo de carga como el Art. 220 | P2 | **Cerrado** | `8e28f86` |
| R-16 · 240-4(b) no se revisa en calibres intermedios: más cobre del necesario | P3 | **Cerrado** | `2da832f` |
| R-13 · «Tabla 310-15(b)(5)(3)» en `CircuitoDerivado.cs`: es numeral | P3 | **Cerrado** | `69a5ed6` |

## Detalle

Formato: **Hecho** (defecto observado) · **Corrección** · **Referencia** · **Prueba**.

### Estructura y publicación

**E-01** — Hecho: `ITablaAislamiento` dependía de `FamiliaAislamiento`, definido en `Coordinacion/CurvaDanioConductor.cs` (fuera del alcance). Corrección: mover el enum a `TablasNom/FamiliaAislamiento.cs`. Pendiente: reportar en `PowerNode-DesignSuite`.

**P-01** — Hecho: sin workflow de despliegue. Corrección: agregar `.github/workflows/deploy.yml` con `.nojekyll`, `base href` y `404.html`.

**P-02** — Hecho: Pages publicaba el README (origen «Deploy from a branch»). Corrección: origen «GitHub Actions» (David).

**P-03** — Hecho: el navegador conservaba el CSS y los iconos anteriores. Corrección: agregar `?v=<commit>` al CSS y a los iconos al publicar.

### Motor

**M-01** — Hecho: sin tablas de la norma sin base de datos. Corrección: `PowerNode.DesignSuite.Normativa` lee las trece tablas desde JSON. Prueba: `PowerNode.Normativa.Tests`.

**M-02** — Hecho: el alimentador se calculaba con la carga total ÷ √3·V_FF (tres aparatos: 15 A, 14 AWG). Corrección: dimensionar con la fase de mayor capacidad requerida; `CalculadoraDesbalanceo.CorrientePorFase` da la corriente por barra (tres aparatos: fase C, 12.20 A, 16 A con NOM completa, 12 AWG). Referencia: 215-2(a)(1), 215-3. Prueba: `M02_…`. Pendiente: reportar en `PowerNode-DesignSuite`.

**M-03** — Hecho: sin aviso con principal menor que el derivado más grande. Corrección: aviso con el número de circuito. Prueba: `M03_…`. Aviso, no bloqueo — R-08.

**M-04** — Hecho: 240-4(b) solo en Alumbrado. Corrección: aplicar en Alumbrado y Equipo; excluir Contactos. Referencia: 240-4(b)(1). Prueba: `Excepcion240_4b_AplicaEnEquipo_NoEnContactos` (32 A continuos, 9 agrupados: Equipo 8 AWG, Contactos 6 AWG).

**M-05** — Hecho: el 125 % se comparaba contra la ampacidad corregida (32 A continuos, 9 agrupados: 6 AWG). Corrección: `SeleccionConductor.CalibrePorDosRevisiones` — 125 % contra la ampacidad de tabla sin factores; carga al 100 % contra la corregida (8 AWG). No aplica a motores (430-22). Referencia: 210-19(a)(1), 215-2(a)(1). Prueba: `DosRevisiones_…`.

**M-06** — Hecho: sin declaración de equipo marcado 75 °C. Corrección: campo «Terminales»; `TemperaturaTerminales.Para(protección, marcado75C, aislamiento)`. Conductor de 60 °C: columna de 60 °C. Referencia: 110-14(c)(1)a.(1) y a.(3). Prueba: `Terminales_…` (45 A no continuos: 6 AWG a 60 °C, 8 AWG a 75 °C).

### Interfaz

**I-01** — Hecho: sin pantalla. Corrección: pantalla de captura.

**I-02** — Hecho: `NumeroFases` tomaba las fases del tablero (720 VA, 1 polo: 1.89 A). Corrección: pasar los polos del circuito (5.67 A). Prueba: `ElCircuitoSeCalculaConSusPolos_…`.

**I-03** — Hecho: sin piso práctico de calibre. Corrección revertida por decisión de David — [`../decisiones/sin-piso-practico-de-calibre.md`](../decisiones/sin-piso-practico-de-calibre.md).

**I-04** — Hecho: sin resumen de carga ni balanceo. Corrección: resumen, balanceo por fase y desbalanceo.

**I-05** — Hecho (David: «que te permita exportar el avance en un archivo y lea archivos de vuelta… con circuitos, configuración, tablero», y «que tenga el nombre del tablero - Power Node» en la pestaña): sin servidor ni base de datos, lo capturado se perdía al cerrar la pestaña, y todas las pestañas se llamaban «Cuadro de carga». Corrección, decisión propuesta en `docs/decisiones/archivo-del-tablero.md`: archivo `.powernode.json`, JSON legible con formato y versión, solo lo capturado (al abrir se recalcula), solo los renglones con captura, todo opcional al leer (`Modelo/Archivo/`, serializador generado al compilar). Abrir y Guardar en la barra superior de la captura; Guardar pregunta dónde cada vez en Chrome y Edge (sin sobrescribir solo) y descarga en Firefox y Safari; abrir con cambios sin guardar pregunta, y cerrar la pestaña también. La pestaña se llama «<tablero> — Power Node»; varios tableros a la vez, uno por pestaña. Defectos vistos al probar, ya corregidos: el escritor escapaba acentos y el «+» de la zona horaria (`\u002B`), y el lector legible tronaba por el orden de inicialización estática. Prueba: 15 pruebas (`I05_…`: lo que se abre calcula igual que lo guardado —protección, calibre, caída, alimentador y canalizaciones—, archivo legible sin resultados, solo renglones con captura, huella sin la hora, archivo con menos campos, cinco archivos que no se abren, gabinete y circuito que no caben, nombre del archivo); navegador, en el servidor de desarrollo y en la publicación Release servida como estática — título al teclear el nombre, guardar, cerrar con cambios pregunta, abrir en otra pestaña con los mismos datos y el mismo principal, confirmación al abrir con cambios y cancelar los conserva, archivo ajeno con error, el documento con el mismo título y sin Abrir ni Guardar, descarga sin el diálogo, barra de 90 px a 360 y 412 px; sin errores en la consola.

**I-06** — Hecho: círculo negro en la carga y favicon de Blazor. Corrección: estilos de arranque y marca Power Node.

**I-07** — Hecho: faltaba `favicon.ico`. Corrección: `favicon.ico` (16, 32, 48), `favicon.png`, SVG y 180 px.

**I-08** — Hecho: «Alumbrado» se cortaba. Corrección: selector de 108 px.

**I-09** — Hecho: sin datos de identificación. Corrección: capturar tablero, clave, ubicación, proyecto y cliente.

**I-10** — Hecho: la fase se calculaba en la pantalla. Corrección: `DistribucionBarras.FasesQueOcupa` y `SistemaDelTablero.MaximoPolos`.

**I-11** — Hecho: un multipolar no ocupaba sus espacios. Corrección: `AcomodoEnGabinete.MotivoNoCabe`; marcar espacios de continuación. Prueba: `NoSePuedeMontarUnInterruptorEncimaDeOtro`.

**I-12** — Hecho: sin alimentador ni principal. Corrección: `CalculadoraAlimentador`. Prueba: `ElInterruptorPrincipalEsLaProteccionDelAlimentador`.

**I-13** — Hecho: sin documento imprimible. Corrección: `/documento` con cuadro y memoria.

**I-14** — Hecho: V F-N siempre ÷√3 (1F-3H 240 V: 138.6 V). Corrección: `SistemaDelTablero.TensionFaseNeutro` (120 V). Prueba: `LaTensionFaseNeutroNoEsSiempreEntreRaizDeTres`.

**I-15** — Pendiente: capturar circuitos de Fuerza (Art. 430).

**I-16 a I-18** — Hecho: encabezados en dos líneas; sin hilos, mm² ni AWG por conductor; sin dibujo del gabinete. Corrección: unidad entre paréntesis y tooltip; nueve columnas de conductor; interior del gabinete.

**I-19 a I-21** — Hecho: multipolar en negro; unidades en mayúsculas; líneas incompletas. Corrección: color por barra; `.simbolo`; líneas en todas las columnas.

**I-22** — Hecho: la clase `grupo` del cuadro tomaba el estilo de las tarjetas. Corrección: acotar a `.ficha .grupo`.

**I-23** — Hecho: un multipolar mostraba «ocupado por el circuito N». Corrección: celdas combinadas, salvo la columna del número.

**I-24** — Hecho: `:last-child` quitaba el borde del número en el renglón de continuación. Corrección: clase `.ultima`.

**I-25** — Hecho: carga solo en VA. Corrección: unidad VA, W o A por renglón con `ConsumoDePlaca.AVoltAmperes`; mostrar el VA de cálculo. Prueba: `I25_…`.

**I-26 e I-27** — Hecho: un F.P. por tablero; «Total (kW)» = VA × F.P. del tablero (3.42 kW). Corrección: F.P. por circuito (inicial 0.9); F.P. del alimentador combinado de la fase que gobierna; kW = suma de potencia activa (3.65 kW). Referencia: Tabla 9, nota 2. Prueba: `FP_…`.

**I-28** — Hecho: los avisos del principal citaban «el Excel original». Corrección: redactar sin el Excel; campo «Mínimo del principal (A)», solo aviso. Prueba: `NingunAvisoLeHablaAlUsuarioDelExcel`, `ConMinimoCapturadoSoloAvisa_…`.

**I-29** — Hecho: 16, 32 y 63 A no existen en centro de carga. Corrección: campo «Interruptores» (centro de carga NEMA por omisión, riel DIN IEC, NOM completa); 240-4(b) contra la lista completa (`ITablaProteccionEstandar.ValoresDeLaNorma`). Referencia: 240-6(a), 240-4(b). Prueba: `Serie_…`.

**I-30** — Hecho: el tooltip de «Tipo» citaba el piso de calibre retirado. Corrección: citar el mínimo de protección y 240-4(b).

**I-31** — Hecho: «Acometida» e «Interruptores» se cortaban. Corrección: selector en renglón propio.

**I-32** — Hecho: aislamiento fijo en THHN, lugar seco. Corrección: campos «Aislamiento» y «Lugar». Caso: 26 A no continuos, 9 agrupados — THHN 10 AWG (28 A ≥ 26 A); THW-LS 8 AWG (10 AWG: 24.5 A < 26 A). Referencia: Tabla 310-104(a), 110-14(c). Prueba: `Aislamiento_…`.

**I-33** — Hecho: la memoria, sección 4, sustituía la capacidad mínima en «Icm = In / (FT × FA)»; sin desglose en pantalla. Corrección: `DesgloseDeSeleccion` en tooltip y en la sección 4; cita 310-15(b)(16) con «capacidad mínima». Prueba: `LaSeccion4CuadraConElConductorElegido`.

**I-34** — Hecho: «Agrupados» sin regla de conteo. Corrección: tooltip con la Tabla 310-15(b)(3)(a) y 310-15(b)(5), (b)(6).

**I-35** — Hecho (David): un espacio es un circuito, no un aparato; la carga de un circuito con varios aparatos se sumaba a mano. Es la base para contar aparatos (220-53 a 220-56) y para motores (I-15, 430-24 necesita el motor mayor). Corrección: desglose opcional por circuito (▸ en la descripción): aparato, cantidad, unidad, carga c/u, continua, F.P. La carga del circuito es la suma y el F.P. el combinado; Unidad, Continua, No continua y F.P. se bloquean. El desglose va debajo del último espacio del circuito, a todo lo ancho: no se mete entre los espacios de un multipolar. «Contacto» sin carga toma 180 VA (220-14(i)); en calefacción todos son continuos (424-3(b)). Al abrirlo, lo capturado se conserva como los primeros aparatos. Memoria, sección 1: un renglón por aparato. El tipo sigue siendo del circuito. Prueba: `I35_…`.

**I-36** — Hecho (David): el campo de carga medía 62 px; 931.25 VA (745 W ÷ 0.8, desde el desglose) se veía «931.2». Corrección: 80 px.

**I-37** — Hecho (David): «Hilos» ofrecía 2, 3 y 4 con cualquier número de fases; «1 fase, 4 hilos» se calculaba como 1F-3H sin avisar. Corrección: `DatosDelTablero.HilosValidos` (1F: 2 o 3; 2F: 3; 3F: 3 o 4) en el selector, y al cambiar las fases los hilos pasan al sistema más común (1F-2H, 2F-3H, 3F-4H). Prueba: `AlCambiarLasFases_…`.

**I-38** — Hecho (David): en el teléfono la ficha y el cuadro ocupaban dos tercios de la pantalla y las tarjetas del cierre se salían. Causa: columnas `1fr` en `.cierre` y `.ficha`, que no bajan del ancho mínimo de su contenido; con 412 px de pantalla la tabla del resumen hacía la columna de 554 px y el navegador alejaba toda la página. Corrección: `minmax(0, 1fr)`; en pantalla angosta los rótulos del resumen parten en dos líneas. Verificado con Playwright (Pixel 7): la página mide lo que la pantalla.

**I-39** — Hecho (David): «Agrupados» era un número para todo el tablero y se aplicaba igual a cada derivado y al alimentador; en la obra los circuitos se agrupan en tubos y el alimentador va solo. Corrección: cada circuito va en su canalización propia o en una compartida (columna «Canal.»); `ContadorDePortadores` cuenta con 310-15(b)(3)(a), (b)(5) y (b)(6) —1 polo: el neutro cuenta; 2 fases + N de estrella: cuenta; 3 fases + N: no, salvo carga no lineal; neutro compartido de 210-4—, y `AjusteDeAgrupamientoPorCanalizacion` aplica la regla de cada tipo: tubo, niple (sin ajuste), ductos y canales metálicos (solo arriba de 30, 376-22(b), 366-23(a)), no metálicos, superficiales (386-22, 388-22). Azotea al sol: Tabla 310-15(b)(3)(c). El material para la Tabla 9 sale del tubo. Decisión: [`../decisiones/canalizaciones-y-agrupamiento.md`](../decisiones/canalizaciones-y-agrupamiento.md). Prueba: `I39_…`, `CanalizacionesTests`.

**I-40** — Hecho (David): el programa no calculaba la canalización. Corrección: `CalculadoraOcupacion` con las Tablas 1, 4, 5 y 8 del Capítulo 10 y sus Notas 2 a 5 (se cuentan todos los conductores; Nota 2 de atascamiento); ductos y canales al 20 %; superficiales con el número del fabricante. Tarjeta «Canalizaciones» con tierra común (250-122(c)), tierra desnuda, tamaño fijado, alimentador con un juego por tubo o todos juntos (310-10(h)(3)); diámetro del fabricante para THW-LS y THHW-LS (Nota 5). Documento con columna «Canal.» y tabla; memoria con una hoja por canalización. En la Tabla 4 del DOF el EMT sale como «no metálico» y hay dos bloques «Cédula 80»: el segundo tiene más diámetro que la cédula 40 y no se ofrece. En la Tabla 5 los mm² del bloque THHN están corridos: se lee por AWG. El llenado coincide con el Apéndice C (Tabla C-1). Prueba: `Llenado_…`, `Tabla4_…`, `Tabla5_…`.

**I-41** — Hecho: todos los circuitos salían con neutro, también un bipolar de 220 V o un trifásico balanceado. Corrección: 1 polo siempre con neutro; 2 y 3 polos solo con la casilla «+N»; nunca en 3F-3H. La columna Neutro dice «—». Prueba: `I41_…`, `R09_…`.

**M-07** — Hecho: la Tabla 5 del DOF publica 55.68 mm² para TW/THHW/THW/THW-2 de 10 AWG; la misma fila da 4.470 mm de diámetro (15.69 mm²), el bloque va 11.68 → 55.68 → 28.19 y el NEC da 15.68. Cada conductor de 10 AWG contaba 3.5 veces su área. Corrección: `ErratasDeLaNorma.AreaTw10Awg` (se verifica antes de corregir y se cita en la memoria). Se barrió la Tabla 5 y la Tabla 4 comparando área contra diámetro: no hay otra. Prueba: `Tabla5_…`.

**I-42** — Hecho (David): el selector de «Aislamiento» era una lista escrita en la pantalla con 7 tipos; el motor reconoce 17 (Tabla 310-104(a)). Faltaban THW, THW-2, THHW, XHH, XHHW, RHH, RHW, RHW-2, USE y USE-2. Corrección: el selector sale de `ITablaAislamiento.DesignacionesReconocidas`; los que no trae la Tabla 5 (THW-LS, THHW-LS, USE, USE-2) dicen «diámetro del fabricante». Prueba: `TodosLosAislamientosDelMotor_…`.

**M-08** — Hecho: THW en lugar seco daba «no es válido para el lugar capturado». El DOF publica THW solo con «75 °C · Lugares mojados» (verificado contra el PDF, pág. 156); `TablaAislamientoJson` buscaba un renglón que dijera «seco». Pero 310-10(a) permite en lugar seco cualquier tipo de la NOM, y 310-10(b) nombra al THW. Corrección: sin renglón para seco, la temperatura que da la tabla (THW: 75 °C). Lo destapó I-42: con la lista corta, THW no se podía elegir. Prueba: `Tabla310_104a_ThwEnLugarSeco_…`.

**I-43** — Hecho (David): el tipo y el tubo de las canalizaciones propias salían de «Condiciones de cálculo», una configuración para todas; para cambiar un solo circuito había que inventarle una compartida. Las propias se rehacían en cada recálculo y las canalizaciones no tenían nombre. Corrección: salen «Canalización» y «Tubo» de Condiciones; toda canalización nace como tubo conduit EMT y se configura en su renglón de la tarjeta «Canalizaciones»; la propia es del circuito (`CircuitoDelCuadro.CanalizacionPropia`) y se conserva; nombre editable en el cuadro, el documento y la memoria. Los casos de referencia en PVC lo fijan (`EnPvc.Todo`). Decisión: [`../decisiones/canalizaciones-y-agrupamiento.md`](../decisiones/canalizaciones-y-agrupamiento.md), «Cambio · David · 2026-09-24». Prueba: `TodaCanalizacionNaceEmt_…`, `ElNombreEsEditable_…`. La canalización propia como objeto aparte la reemplazó I-44.

**I-44** — Hecho (David): al capturar la carga nacía una canalización llamada «Propia» y el selector del cuadro mezclaba esa propiedad («1 circuito = 1 tubo por omisión») con la lista de tubos; en la tarjeta había dos, tres renglones «Propia». Las opciones apiladas hacían cada renglón de tres líneas y la última columna quedaba vacía en las propias. Corrección: una sola lista; cada circuito con carga que no va en ninguna recibe la suya con el número libre más chico (T1, T2…); la que nació sola se quita al quedarse sin circuitos y las de «+ Nueva» se quedan; en el cuadro, «+ Nueva» saca al circuito de una compartida. Cada opción en su columna (neutro compartido y tierra común con dos o más circuitos); el ✕ junto al nombre; tamaño en mm e in como combos con el que da el cálculo ya elegido (`ResultadoOcupacion.TamanoCalculado`), elegir otro lo fija y ↺ regresa. Renglón de 42 px. Prueba: `CadaCircuitoConCargaNaceEnSuTubo_…`, `TamanoFijado_…`, `QuitarUnaCanalizacion_…`.

**I-45** — Hecho (David): con la cocina armada, el circuito 4 decía «Canalización cocina» en el selector pero seguía en T3 (su tamaño, 16 mm, era el de T3), y T3 no se iba. Causa: al quitarse un tubo de la lista, Blazor reutilizaba los `<option>` por posición y les cambiaba el texto; la opción seleccionada pasaba a decir otro tubo sin que el circuito cambiara, y elegir lo que ya se veía seleccionado no dispara el cambio. Reproducido: tras pasar el 2 a T1, el 3 mostraba T4 estando en T3. Corrección: `@key` en las opciones del selector, en los tamaños y en los renglones de la tarjeta. El selector crece con el nombre («Canalización cocina» se cortaba en «Canalizac»). Prueba: navegador (Playwright), cada selector contra la columna «Circuitos» de la tarjeta; es del render, sin prueba unitaria.

**I-46** — Hecho (Claude, revisando la cocina de David en 1F-2H): con el inmueble en «Otro», el uso «Cocina» de un circuito de contactos aplicaba reglas de vivienda: 20 A por 210-11(c)(1), 780 VA de «Mínimo 220-52» en el alimentador y el aviso de «dos o más circuitos». 210-11(c) es «Unidades de vivienda» y 220-52 «Cargas de aparatos pequeños y lavadoras en unidades de vivienda»; ninguna aplica a la popular de hasta 60 m² (210-11(c) Excepción 1; excepción de 220-52). El programa no miraba el inmueble. En la cocina: circuito 1 de 20 A y 10 AWG a 15 A y 12 AWG; principal de 30 A a 25 A. Corrección: `TipoDeInmueble.AplicaUsoDeContactos` (unifamiliar y multifamiliar); `UsoEfectivo` lo pone el recálculo; el selector de uso solo aparece ahí; el uso capturado se conserva si el inmueble cambia. Prueba: `I46_FueraDeViviendaElUsoNoCuenta`.

**I-47** — Hecho (Claude, misma revisión): en 1F-2H la tarjeta del alimentador daba corriente de diseño 26.86 A y de neutro 25.86 A, por el mismo conductor; y F.P. 0.98 cuando la caída se calculaba con 0.97. La de diseño es la suma directa de VA demandados (dimensiona, conservadora); la de neutro, la fasorial (caída). El F.P. mostrado era el de la carga instalada, sin factor de demanda ni 220-52. Corrección: el F.P. del alimentador lleva el factor de demanda y el mínimo de 220-52, igual que la corriente de la caída; en 1F-2H no se muestra un renglón de neutro aparte; ayudas en «Corriente de diseño», «Factor de potencia» y «F.P. resultante» (este, de la carga instalada). Prueba: `I47_ElFpDelAlimentadorEsElDeLaCorrienteDeLaCaida`.

**I-48** — Hecho (Claude, misma revisión): el interruptor principal decía «30 A · 1 polos». Corrección: «1 polo». Prueba: navegador.

**I-49** — Hecho (David): los campos capturables mostraban el valor completo —una carga en W con su F.P. salía «823.529411…», el F.P. combinado de un desglose con cuatro decimales—, y un mismo valor salía distinto según dónde: F.D. «1» en la tabla y «1.00» en «Mínimo 220-52»; alumbrado «156.25» en «Continua» y «156» en el total del desglose y el balanceo. Regla de David: al usuario no se le muestran más de dos decimales; el cálculo conserva todos. Corrección: los campos muestran hasta dos decimales (`Numero`, «0.##») y los factores F.P. y F.D. siempre con dos (`Factor`, «0.00»); lo capturado se guarda completo; las cargas en VA van con hasta dos decimales en pantalla y documento («#,0.##»); en la memoria, R y X de la Tabla 9 con dos, y una nota dice que el cálculo usa los completos. Barrido en el navegador (captura, documento y memoria; texto, campos y tooltips) de un caso con W, A, desglose y F.P. de tres decimales: ningún número con más de dos. Prueba: navegador; `I50_…`.

**I-50** — Hecho (Claude, en el barrido de I-49): la fórmula de caída de la memoria escribía «e = 2 × 23.70 m × 6.63 A × [ R × cos + X × sen ] / 1 = 2.83 V», con L en m y R, X en Ω/km: a mano daba 1000 veces la caída. En el alimentador, «× 0.037 km». Corrección: «÷ 1000» en la fórmula general y en la sustituida, con L en m. Prueba: `I50_LaCaidaSeRecalculaAMano_ConMetrosEntre1000_YDosDecimales`.

**I-51** — Hecho (David): en un circuito de contactos de vivienda, el selector de uso («Cocina») salía en bloque pegado a la izquierda de la celda y con otra letra, debajo del de tipo centrado; la nota «Alimentador: 1,500 VA — 220-52(a)» también a la izquierda. Se veía chueco. Corrección: tipo, uso y nota en una columna centrada (`.tipo-uso`), los dos selectores de 108 × 23 px con el mismo borde. Prueba: navegador (mismas coordenadas y medidas; nota centrada bajo el tipo).

**I-52** — Hecho (Claude, revisando I-51): I-49 puso el F.P. siempre con dos decimales («0.90») y el campo de 52 px lo cortaba en «0.9». Corrección: 60 px. Prueba: navegador, ningún campo con el texto más ancho que su caja, en un caso con W, A, desglose y F.P. de tres decimales.

**I-53** — Hecho (David, con dos capturas): con 220 V de un 3F-4H pasó a 1F-2H y el tablero calculó **220 V de fase a neutro**; a 1F-3H, **110 V**. Ninguna es tensión de la NOM (110-4: 120, 127, 120/240, 220Y/127, 208Y/120, 240, 480Y/277…). El campo decía «Tensión F-F» también en 1F-2H, que no tiene dos fases, y el programa usaba ese número como F-N; al cambiar de configuración la tensión se quedaba y nada avisaba. David preguntó además si 1F-3H era «fase, neutro y tierra»: no, los hilos son fases más neutro y la tierra no se cuenta; 1F-3H es «120/240 volts, 1 fase, 3 hilos», dos vivos en oposición con derivación central, y el «bifásico» es 2F-3H, «220Y/127 volts, derivado de un sistema de 3 fases, 4 hilos». Corrección: `DatosDelTablero.TensionesNominales` por configuración; al cambiar de configuración una tensión que no es nominal pasa a la de la NOM (127, 240, 220), y una que sí lo es se queda (3F-4H a 480 → 3F-3H sigue en 480); `AvisoTension` en la ficha si la capturada no es nominal, con la F-N que resulta; el campo dice «Tensión F-N» en 1F-2H; ayudas en Fases e Hilos (la tierra no es hilo); «2F-3H (dos fases de estrella)» en vez de «2F de estrella»; el documento no imprime «V F-F» en 1F-2H. Prueba: `I53_AlCambiarDeConfiguracion_…`, `I53_UnaTensionQueNoEsDeLaNom_SeAvisa`; navegador.

**I-54** — Hecho (David): en 1F-2H, con una sola barra, el cuadro seguía separando nones arriba y pares abajo, y el gabinete en dos columnas; y se ofrecían gabinetes de 6 a 42 espacios, cuando en 1F-2H hay centros de carga de 1 a 8 y de 12 en adelante no existen. Corrección: `DatosDelTablero.EspaciosValidos` (1F-2H: 1 a 8; las demás: 6 a 42); al cambiar de configuración, un gabinete que no existe pasa al más cercano (24 → 8 al pasar a 1F-2H; 5 → 6 al regresar a 3F; 8 → 12 a 1F-3H); `CuadroDeCarga.Lados` lista en orden 1, 2, 3… con una sola barra, en la captura y en el documento; el gabinete en una columna. Prueba: `I54_1F2H_OfreceDe1a8Espacios_…`, `I54_ConUnaSolaBarra_…`; navegador. Ajuste (David): en 1F-2H solo 1, 2, 4, 6 y 8 espacios, los que se venden — `5f74680`.

**I-55** — Hecho (David): (1) un campo de carga en 0 tenía el «0» escrito; según dónde caía el cursor, teclear 5 daba «50» o «05». (2) Al cortar o borrar el valor de un campo, el campo quedaba en blanco y el cálculo seguía con el último valor: reproducido, «No continua» vaciado seguía cargando 5 VA en el balanceo, y el F.P. vaciado seguía en 0.90. Corrección: en las cargas (continua, no continua, carga c/u) el 0 no se escribe, se ve como marca de agua, y vacío es 0 (`Carga`, `LeerCarga`); los demás campos numéricos llevan `data-requerido` y un script en `index.html`, que corre antes que Blazor, les devuelve su valor si se vacían; al entrar a cualquier campo numérico se selecciona todo, así lo tecleado reemplaza (clic en medio de «20» y teclear 15 da 15). Prueba: navegador — vacío con marca «0»; 5 sobre el vacío da 5; cortar una carga la regresa a 0 y el renglón se vacía; F.P. y tensión vaciados regresan a 0.90 y 220.

**I-56** — Hecho (David pidió revisar la navegación y aplicar las 10 mejoras): medido en el navegador con el tablero por omisión (3F-4H, 24 espacios): 280 paradas de Tab (240 en el cuadro, 10 por renglón), 32 para llegar a la primera carga; Enter guardaba sin mover el foco; ↑/↓ en un número restaba 1 (100 → 99); ↓ en «P» cambiaba a 2 polos y reacomodaba el tablero; las 61 ayudas eran `title` en un `span`, solo con ratón; tras «+ Agregar aparato», «+ Nueva canalización» o «▸» el foco se quedaba en el botón; sin `aria-label` ni atajos. Corrección, en `wwwroot/js/teclado.js` (absorbe el script de I-55) y el marcado de la captura: (1) Enter/Shift+Enter bajan y suben al mismo campo (`data-col`) del renglón siguiente, en el cuadro, el desglose, las canalizaciones y los F.D.; (2) ↑/↓ cambian de renglón y ya no suman ni restan; (3) las flechas no cambian un selector — Alt+↓ o Espacio abren la lista; (4) Esc deshace la edición del campo; (5) el foco va a lo que se crea o se abre (aparato nuevo, canalización nueva, desglose) y, al quitar, al que queda en su lugar; (6) barra de ayuda al pie con la ayuda del campo con el foco y las teclas; (7) Alt+1…5: Ficha, Cuadro, Canalizaciones, Resumen, Alimentador; (8) el ▸ sale del recorrido de Tab y el desglose se abre con **Ctrl+Enter** — no Alt+D, que en Chrome, Edge y Firefox lleva a la barra de direcciones; (9) foco uniforme con el color de acento (`:focus-visible`); (10) `aria-label` en cada campo del cuadro, del desglose, de las canalizaciones y de los F.D. Quedan 256 paradas de Tab. Prueba: navegador, los 10 puntos (Enter baja del 1 al 3 y guarda; ↓ no resta; ↓ en P no cambia; Esc regresa 20 y la descripción vacía; Ctrl+Enter abre y cierra; foco en el aparato nuevo, en el que queda y en «+ Agregar aparato»; barra con la ayuda de «Continua» y de «Fases»; Alt+1…5; contorno de acento; 0 campos sin `aria-label`); a 412 px la página no se ensancha.

**I-57** — Hecho (David: «renombra los archivos del favicon y crea el tema oscuro»): (1) con el logo nuevo, la pestaña seguía mostrando el unifilar: el navegador guarda el favicon por URL y los archivos conservaban el nombre de antes. Se renombraron a `marca/powernode-carta-*` (`.ico`, SVG y PNG de 16 a 512); `favicon.ico` y `favicon.png` de la raíz se quedan, ya con la carta. (2) No había tema oscuro y los colores estaban sueltos en cada regla. Corrección: todos los colores de `app.css` pasan a variables; la paleta oscura se escribe una sola vez, en `@media screen { :root[data-tema="oscuro"] }`, así que impreso sale siempre en claro. `js/tema.js` (en el `<head>`, sin `defer`, para que no destelle) resuelve la elección del botón **Automático → Claro → Oscuro** del encabezado de la captura y del documento y la guarda en `localStorage`; automático sigue al sistema, también si cambia con la página abierta. Logo, firma y pantalla de carga llevan su versión de tinta clara (`solo-claro`/`solo-oscuro`); los colores de fase no cambian y la fase A lleva borde en oscuro. En la prueba salieron las dos firmas al imprimir (`.doc-marca img { display: block }` le ganaba a `.solo-oscuro`): la regla de impresión va con `html:root`. «+ Nueva canalización» salía con el botón nativo del navegador: ahora es `.boton`. Y `deploy.yml` pone `?v=<commit>` también a `js/`: un `teclado.js` viejo en caché pisaba `window.powerNode` y el botón del tema tronaba la página. Prueba: navegador — sistema claro y oscuro sin elección; el ciclo Automático → Claro → Oscuro → Automático; la elección sobrevive a la recarga; el cambio del sistema en vivo; impreso en oscuro con fondo blanco, tinta `#101418`, una sola firma y sin el botón; el PDF de la memoria igual en los dos temas; a 412 px la página no se ensancha; los iconos nuevos responden 200; sin errores en la consola.

**I-58** — Hecho (David, con una captura del encabezado: «métete a nom y a msa-toolkit, conviértelo en una barra superior»): el encabezado de la captura (logo, título, tema y «Documento y memoria») se iba al bajar, y el documento tenía su propia barra de botones con pestañas sin dirección. Se revisaron `header.top` del sitio de la NOM (`site/src/layouts/Base.astro`, `global.css`) y `header.toolbar` de msa-toolkit (`index.html`, `style.css`): los dos son una barra a todo lo ancho, fija arriba, con línea abajo, la marca a la izquierda y la navegación y el tema a la derecha; msa-toolkit, con el tema segmentado. Corrección: `Layout/BarraSuperior.razor` en `MainLayout`, con la marca (icono, «Power Node», NOM-001-SEDE-2012), las páginas **Captura · Cuadro de carga · Memoria de cálculo** (la memoria con ruta propia, `/documento/memoria`), el tema segmentado **Sistema | Claro | Oscuro** e **Imprimir / PDF** en el documento; en el celular las páginas bajan a un segundo renglón; `scroll-padding-top` para que el foco que mueve el teclado no quede debajo; el título de la captura queda para lectores de pantalla. Prueba: navegador — claro y oscuro a 1600, 1366 y 412 px sin desbordar; la barra sigue arriba al bajar 1500 px; la página activa resaltada y con `aria-current`; Captura → Cuadro → Memoria y recarga directa en `/documento/memoria`; Imprimir solo en el documento; impreso sin la barra; el tema segmentado guarda Oscuro y Sistema lo borra; Shift+Enter hacia un campo fuera de vista lo deja debajo de la barra, en escritorio y celular; sin errores en la consola.

**I-59** — Hecho (David eligió la propuesta A de cuatro — se dibuja con la descarga, el punto busca la adaptación, tres fases, latido del nodo —; pidió el logo «dos escalones» más grande y quitar el tooltip): la pantalla de carga era el logo quieto, sin decir cuánto faltaba. Corrección: la carta va en línea (SVG con `currentColor`) en `index.html` y en `BarraSuperior.razor`, y `--p` (de 0 a 1, con `@property`) la dibuja: el borde avanza, cada línea aparece en su umbral y el punto azul llega al final. En la carga, `--p` es el **avance real**: Blazor publica `--blazor-load-percentage` por cada recurso (`onDownloadResourceProgress`) y un script lo pasa a `--p` y al texto; si en 1.5 s no llega avance, la carta sale completa. En la barra, la misma animación una vez al pasar el cursor o con el foco. Con «reducir movimiento», quieta. Logo de 80 px en la carga (era 56) y 40 px en la barra (era 28); barra de 64 px; en el celular el tema cabe junto a la marca, también a 360 px. Prueba: navegador — con la red limitada, `--p` y el texto suben de 1 % a 100 % en 35 pasos y la app arranca; al pasar el cursor `--p` va de 0.12 a 1.00 en ~1 s y regresa a 1 al quitarlo; con `reducedMotion: 'reduce'`, 1 en la carga y en la barra; sin `title` en el logo; barra de 90 px a 360 y 412 px; claro y oscuro; sin errores en la consola. Visto al probar, no causado por esto: a 360 px la tabla del resumen se sale 8 px.

**I-60** — Hecho (David, con capturas: «la carga es muy rápida y no muestra la animación, retarda la carga»; «arregla ese espacio vacío» en el cuadro y en la memoria; y el resumen a 360 px, visto en I-59): (1) En caché la descarga acaba en ~0.2 s y Blazor borraba la pantalla de carga (vivía dentro de `#app`) al arrancar. Además, mientras arranca .NET el navegador se congela ~1.5 s sin pintar, y al volver el script compensaba el tiempo perdido: la carta brincaba de 42 % a 100 %. Corrección: la pantalla de carga es una capa encima de `#app`; el dibujo sigue al avance real sin adelantarlo, tarda por lo menos 1.6 s de cuadros pintados (cada cuadro avanza a lo más 1/30 s) y la capa se desvanece cuando la app ya pintó y la carta está completa. (2) En el documento la firma ocupaba una de tres columnas, casi vacía: ahora la firma y el nombre del documento van en un renglón arriba y los datos debajo en dos columnas, en el cuadro y en la memoria. (3) A 360 px la tabla del resumen se salía 22 px de su tarjeta y el F.D. se cortaba («1.0»): parten también las cifras, 12 px, y menos margen en el lienzo y las tarjetas. (4) En el celular, la barra del documento se iba a tres renglones con Imprimir: el tema e Imprimir quedan como símbolos (◐ ☀ ☾ y una impresora), con el nombre para el lector de pantalla. Prueba: navegador — en caché la app pinta a 1.75 s y la carta sigue de 25 % a 100 % sin brincar, capa fuera a 3.8 s; con la red limitada el dibujo nunca adelanta al real (app a 8.2 s, capa fuera a 9.1 s); con «reducir movimiento» la capa se va 0.1 s después de que la app pinta; encabezado del cuadro y de la memoria en claro, oscuro e impreso; resumen de 310 px en 310 px de tarjeta a 360 px, con el F.D. completo; barra de 90 px a 360 y 412 px en la captura y el documento, 65 px en escritorio; los botones del tema y de imprimir por su nombre accesible; sin errores en la consola.

**I-61** — Hecho (David, con captura de gitdiagram.com: «agrega un botón de GitHub con las estrellas como este»): la barra no llevaba al repositorio. Corrección: al final de la barra, en todas las páginas, «GitHub ★ n» con la marca de GitHub (Octicons), en otra pestaña; Imprimir pasa antes del tema. La cuenta sale de la API pública de GitHub (`js/github.js`), que da 60 consultas por hora sin sesión por dirección IP: se guarda una hora en el navegador; si la consulta falla, el botón sale sin número. «17.1k» arriba de mil. En el celular GitHub va a la derecha del segundo renglón y las páginas con nombre corto (Captura · Cuadro · Memoria), con el completo para el lector de pantalla: con el nombre completo, GitHub bajaba a un tercer renglón (barra de 126 px). Prueba: navegador, con la API simulada — «GitHub ★ 17.1k» en claro y oscuro, en la captura y el documento; una sola consulta al recargar y cambiar de página; sin red, «GitHub» sin número; «1 estrella» y «0 estrellas» en el nombre accesible; enlace a `github.com/dflores296/Power-Node-Web` con `target=_blank`; barra de 93 px a 360 y 412 px sin desbordar; los tres enlaces de página por su nombre completo; sin errores en la consola.

**I-62** — Hecho (David pidió propuestas de tipografía e iconos y eligió en el canvas «Power Node — tipografía e iconos»: Segoe UI y duotono): la aplicación mezclaba tres SVG (Abrir, Guardar, Imprimir) con caracteres que hacían de icono —▸ ▾ del desglose, ✕ de quitar, ↺ del tamaño, «+» de agregar, ◐ ☀ ☾ del tema en el celular—, que cada sistema dibujaba con su fuente, a su tamaño y a veces como emoji; y la firma del documento pedía IBM Plex Sans, que nunca se cargó. Corrección: `Layout/Icono.razor` con doce iconos duotono (trazo de 1.5 px redondo sobre relleno del acento al 16 %, 26 % en oscuro; avisos en ámbar; en el botón primario, relleno del color del trazo) en todos esos lugares, y el triángulo al frente de cada aviso; la firma, en la pila de Segoe UI. Visto al probar y corregido: en escritorio, los iconos del tema salían encima de «Sistema · Claro · Oscuro» (la regla general del icono le ganaba a la que los oculta). Prueba: navegador, en claro y oscuro — barra, desglose (abre, agrega un aparato, el bote quita uno), canalizaciones, avisos de tensión y del principal, documento con Imprimir; 34 iconos y ningún carácter suelto en un botón; en el celular, el tema con iconos y por su nombre accesible, barra de 93 px; sin errores en la consola.

**I-63** — Hecho (David: «implementa directo»; de los lugares revisados, el resumen y el selector «Tipo»): los iconos de tipo de carga de la propuesta no tenían dónde ir. Corrección: cinco iconos duotono (alumbrado, contactos, equipo, motor, calefacción), uno por renglón del resumen de carga y junto al selector «Tipo» del cuadro solo en renglones con carga (hueco del mismo ancho en los vacíos). Visto al probar, ya existía: arriba de 1400 px la tabla del resumen se salía de su tarjeta 33 a 103 px (el icono le sumaba 23); el tipo y los encabezados ahora parten en dos líneas. Prueba: navegador, claro y oscuro, cinco circuitos de los cinco tipos — 5 iconos en el cuadro y 5 en el resumen; desborde 0 px a 1280, 1366, 1500, 1600 y 1920 px; sin errores en la consola.

**I-64** — Hecho (David: «lo siento muy plano… algo intermedio» entre la app y gitdiagram; en el canvas «Power Node — relieve y contraste» eligió B, relieve marcado): fondo gris claro, títulos en gris, tarjetas blancas sin sombra. Corrección, solo en pantalla: tarjetas con borde de 1.5 px y sombra sólida desplazada 4 px; botones y tema con borde oscuro y sombra de 2 px que se hunden al presionarlos; barra con línea de 2 px; títulos en tinta; fondo #e6ebf1, rótulos #45505c, encabezados de tabla #dfe6ee; en oscuro el fondo baja a #0b0e12. Prueba: navegador, claro y oscuro con carga capturada — la página no se ensancha a 1500 px; impreso, el documento sin sombra ni borde; sin errores en la consola.

**I-65** — Hecho (David, con capturas y gitdiagram en oscuro de ejemplo): las líneas internas de las tablas se veían tibias junto al borde marcado de las tarjetas, y en oscuro la sombra casi negra caía sobre un fondo casi negro y el relieve no se veía. Corrección: `--linea` #c2cbd6 y `--linea-suave` #d8dfe7 en claro; en oscuro fondo #161b21, tarjetas #1f262d, líneas más claras, bordes y sombras negros y campos un tono más hundidos. Prueba: navegador, claro y oscuro con carga capturada; página sin ensancharse a 1500 px; impreso sin sombra; sin errores en la consola.

**I-66** — Hecho (David, con fotos de interruptores NEMA y DIN y captura del gabinete): el gabinete ponía el valor del interruptor como texto a la derecha de la celda. Corrección: `Layout/Interruptor.razor`, duotono como `Icono.razor` (trazo de 1.5 px, relleno del acento al 16 %), genérico, sin marca ni logotipo. NEMA: de lado, polos apilados cada 34 px (renglón de 30 + hueco de 4) unidos por una sola manija con el valor calado; en el gabinete, cada polo queda frente a su espacio. DIN: de pie, módulos de 18 lado a lado, bornes arriba y abajo, «C16» en el frente; hecho pero sin usar, porque el único gabinete es NEMA. Valores: los de la serie (`SerieDeInterruptores`). Propuesta en el canvas «Iconos de termomagnético». Prueba: navegador, claro y oscuro, circuitos de 1, 2 y 3 polos — bloque de 30/64/98 px con icono de 24/58/92 px; sin desborde a 390 px; sin errores en la consola salvo la API de GitHub sin red.

### Revisión del 2026-09-23

Validada contra el texto de la NOM (`NOM-001-SEDE-2012/data/corpus.json`), el código y el caso base
de tres aparatos.

**R-01** — Hecho: `DatosDelTablero.CaidaMaxAlimentadorPct = 5`, sin campo en pantalla; sin verificación de la caída combinada. Caso base con alimentador de 80 m: 4.62 % + 2.31 % (circuito 5) = 6.94 %, sin aviso. Además, la memoria, sección 7, citaba «310-15, NOTA 4». Corrección: «e% máx. alimentador» capturable, 3 % por omisión; `CaidaTensionAcumulada.Evaluar` por circuito con el alimentador (límite 5 %, solo aviso) en la captura, el documento y la memoria, sección 7; la sección 7 cita 210-19(a)(1) NOTA 4 en el derivado y 215-2(a)(4) NOTA 2 en el alimentador. Con 3 % el alimentador de 80 m sube a 10 AWG (2.75 %) y el circuito 5 sigue avisando (5.07 %). Referencia: 215-2(a)(4) NOTA 2 (3 % alimentador, 5 % combinada), 210-19(a)(1) NOTA 4. Prueba: `R01_…`.

**R-02** — Hecho: la caída del alimentador usa el equivalente balanceado (√3·I·Z / V_FF). Caso base con 80 m: neutro 6.11 A; fase C 4.62 % calculada contra 6.77 % con el neutro (fasorial). 2F-3H balanceado: `2·I·Z / V_FF` subestima una fase ≈39 %. Va con R-04. Corrección: `CaidaPorFase` en el motor — e_f = Re[Z·(I_f + I_N)·conj(û_f)], I_N suma fasorial; el conductor se limita con la peor fase. Método fasorial (decisión de David). Memoria, sección 6, fase por fase; tarjeta con la corriente de neutro. La caída combinada de R-01 usa la de la fase de cada circuito. Sin neutro (3F-3H), la balanceada de siempre. Caso base con 80 m y 3 %: el alimentador sube de 10 a 8 AWG (fase C 2.64 %). Prueba: `R02_…` (balanceado 3F-4H y 1F-2H: igual a la fórmula de siempre; 2F-3H: A 4.20 %, B 7.17 %).

**R-03** — Hecho: `deploy.yml` solo corre `PowerNode.Normativa.Tests`.

**R-04** — Hecho: `CuadroDeCarga.CalcularAlimentador` entrega al motor la corriente de la fase × divisor, deshace la demanda (`SinDemanda`) y reescribe la cita 220-40. Corrección: `DatosEntradaAlimentador.CorrientesPorFase` (aritmética para dimensionar, fasorial para la caída); el motor elige la fase que gobierna (cita 215-2(a)(1)), aplica la demanda y cita 220-40 con la carga real. Fuera de la web: ×3, `SinDemanda` y `Cita220_40`. Prueba: `R04_…`. Pendiente: llevar al escritorio ([`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md)).

**R-05** — Hecho: el documento de pruebas, 4.2 con F.P. 0.8, dice 2.01 %. Cálculo: 0.04 km × 11.81 A × (6.6 × 0.8 + 0.177 × 0.6) Ω/km = 2.544 V → 2.00 %. Corrección: `P4_2_…` espera 2.00 %. El documento de pruebas vive fuera del repo; lo corrige David.

**R-06** — Hecho: con circuito y principal fuera de riel DIN el aviso dice «para el circuito 3 (150 A), el principal (175 A) se tomó…». Corrección: «En riel DIN no hay interruptores de más de 125 A. El circuito 2 (175 A) y el principal (175 A) se calcularon con la lista completa de 240-6(a); esos tamaños ya no son de riel DIN.» Singular con un solo tamaño; «Los circuitos 1 y 2 (175 A y 200 A)» con varios; «la protección del alimentador» con zapatas principales. Prueba: `R06_…`.

**R-07** — Hecho: las 20 pruebas del documento del 2026-09-23 no estaban en el repo. Prueba: `PruebasDelDocumento20260923Tests`.

**R-08** — Hecho: decisión abierta en [`../decisiones/interruptor-principal-criterios-del-excel.md`](../decisiones/interruptor-principal-criterios-del-excel.md). Corrección: CONFIRMADA · David · 2026-09-24: aviso, no bloqueo. Prueba: `M03_…`, `P2_1_…`.

**R-09** — Hecho: el tooltip de «Agrupados» ya cita (b)(5)(2) y el neutro ya sale del calibre de la fase; la memoria no lo cita y no hay prueba. Corrección: memoria, sección 2, renglón «Neutro — 310-15(b)(5)(2)» en todo tramo de 2 fases + neutro de estrella (alimentador 2F-3H y derivados de 2 polos en 2F-3H o 3F-4H), con 220-61(c)(1) en el alimentador; tooltip en «Conductor de neutro». No se cita en 1F-3H 120/240, donde el neutro lleva solo el desbalance — (b)(5)(1). Referencia: 310-15(b)(5)(2), 220-61(c)(1). Prueba: `R09_…` (2F-3H, 1270 VA por fase: 10 A, neutro = fase).

**R-10** — Hecho: ninguno se aplica hoy, sin prueba que lo asegure. 220-61(a) excepción: sistemas bifásicos (90°), no 2 fases de estrella. 310-15(b)(7): solo 120/240 V. Corrección: dos pruebas con alimentador 2F-3H de 99.99 A por fase y terminales 75 °C — 100 A, fase y neutro 3 AWG, Tabla 310-15(b)(16); con × 140 % el neutro sería 1/0 AWG y con (b)(7) la fase sería 4 AWG. Lectura en [`../conocimiento/seleccion-conductor-y-proteccion.md`](../conocimiento/seleccion-conductor-y-proteccion.md). Prueba: `R10_…`.

**R-11** — Hecho: sin tipo de inmueble ni indicación de equipo de acometida. Referencia: 230-79(c) vivienda según carga conectada, vivienda popular hasta 60 m² no menor que 30 A; 230-79(d) demás, no menor que 60 A. Corrección (flujo de David): casilla «Equipo de acometida»; marcada, selector «Inmueble» (vivienda unifamiliar, vivienda popular, otro). El principal **sube** al mínimo (`DatosEntradaAlimentador.ProteccionMinimaA`) y el conductor se protege con él (240-4). Vivienda unifamiliar: sin número fijo, manda la carga. Se quitó «Mínimo del principal (A)», que solo avisaba. Caso base: otro 60 A (4 AWG), vivienda popular 30 A (10 AWG). Prueba: `R11_…`.

**R-12** — Hecho: solo factores de demanda continua y no continua capturados. Contradice la decisión del motor «automatizar el Art. 220 queda fuera de v1» (`Domain/Proyectos/Alimentador.cs`): requiere decisión antes de implementar. Referencia: Tabla 220-42, Tabla 220-44, 220-53, 220-82. Corrección (decisión de David): el factor de demanda sigue a criterio del ingeniero, sin automatizar. Se agrega la justificación de selección múltiple (Tablas 220-42, 220-54, 220-55, 220-56, 220-86, 220-88; 220-44, 220-53, 220-60, 220-82 a 220-84, 220-87; «Otra» con texto), que aparece con algún factor menor que 1 y se imprime en la memoria, sección 1, del alimentador. Sin justificación, aviso. Prueba: `R12_…`.

**R-13** — Hecho: `Domain/Proyectos/CircuitoDerivado.cs:117` cita «Tabla 310-15(b)(5)(3)». Es el numeral 310-15(b)(5)(3). Corrección: citar «310-15(b)(5)(3)». Pendiente: llevar a `PowerNode-DesignSuite` ([`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md)).

**R-14** — Hecho: `CalculadoraCircuitoDerivadoNoMotor` aplicaba 15 A mínimo en alumbrado y 20 A en contactos, el `MAX(20, …)` del Excel. La NOM no lo pide (210-3 y 210-21(b)(3) permiten contactos en 15 A); solo 210-11(c) exige 20 A en cocina, lavadora y baño de vivienda, y 220-52 carga 1500 VA por circuito de cocina y de lavadora. 500 VA de contactos salían en 20 A y 12 AWG. Corrección: quitar el mínimo; «Uso» por circuito de contactos (General, Cocina, Lavadora, Baño) con 20 A y su cita; 1500 VA al alimentador (renglón «Mínimo 220-52» en el resumen y en la memoria); aviso con un solo circuito de aparatos pequeños. Referencia: 210-11(c), 220-52(a) y (b). Decisión: [`../decisiones/minimo-de-proteccion-por-uso.md`](../decisiones/minimo-de-proteccion-por-uso.md). Prueba: `SinMinimoPorTipo_…`, `Vivienda_…`. Pendiente: llevar a `PowerNode-DesignSuite`.

**R-15** — Hecho (David, revisando R-01): el alimentador arrancaba en 3 % y el derivado en 3 %: 6 %, arriba del 5 % combinado de la misma nota, sin aviso en los campos. El aviso de caída combinada salía como renglón dentro del cuadro. Corrección: alimentador 2 % por omisión (2 % + 3 % = 5 %); aviso en «Condiciones de cálculo» si los límites suman más de 5 %; los avisos de caída combinada, en su tarjeta debajo del cuadro. Con 2 % + 3 % ningún circuito pasa del 5 %: el aviso por circuito solo sale si se suben los límites. Caso base con 80 m: el alimentador sube a 6 AWG. Referencia: 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4. Prueba: `R15_…`.

**R-16** — Hecho: `SeleccionConductor.DeterminarCalibreBase` revisa 240-4(b) solo en el calibre por carga; si no califica, salta al primer calibre cuya ampacidad cubre toda la protección. Caso base con principal de 60 A (230-79(d)): 12 AWG no alcanza y salta a 4 AWG (70 A); 6 AWG (55 A a 60 °C, 240-4(b) → 60 A) también cumple. Del lado seguro, con más cobre. Viene del motor copiado. Corrección: subir calibre por calibre desde el de la carga y quedarse en el primero protegido, por ampacidad o por 240-4(b); los topes de 240-4(d) siguen después. 60 A: 6 AWG. Prueba: `R16_…`. Pendiente: llevar a `PowerNode-DesignSuite`.

**R-17** — Hecho (David): los dos factores de demanda venían del Excel (continua, no continua); el Art. 220 los da por tipo de carga. «Equipo» mezclaba cargas que no se reducen: motores y A/C (220-50) y calefacción fija (220-51). Corrección: cinco tipos (Alumbrado, Contactos, Equipo, Motor / A/C, Calefacción); factor capturable en los tres primeros, 1.00 fijo en los otros dos. Cada circuito lleva el factor de su tipo en la continua y en la no continua; el 125 % de 215-3 sigue sobre la continua. Resumen por tipo con «Continua / no continua» abajo. Justificación por tipo, solo con las opciones que le aplican. Motor / A/C y calefacción se calculan como carga de placa (Art. 430 pendiente, I-15). Referencia: 220-40, 220-42, 220-44, 220-50, 220-51, 220-53 a 220-56. Prueba: `R17_…`.

**R-18** — Hecho (Claude, revisando R-17 con David): Motor / A/C y calefacción quedaban fijos en 1.00, pero 220-50 remite a 430-26 (menos ampacidad si no todos los motores funcionan a la vez o por servicio intermitente) y 220-51 tiene Excepción (calefacción por ciclos o no toda a la vez). Además la calefacción se podía capturar como no continua, contra 424-3(b). Corrección: los cinco tipos con factor capturable y justificación; motores: 430-26, 220-60, Otra; calefacción: 220-51 Excepción, 220-60, Otra. La calefacción pasa sola a continua y su columna «No continua» queda deshabilitada. Tooltip del tipo con ejemplos (inverter frío/calor en Motor / A/C; resistencias en Calefacción). Referencia: 220-50, 220-51, 430-26, 424-3(b). Prueba: `R18_…`.

**R-19** — Hecho (David): el inmueble solo existía con la casilla de acometida, con tres opciones para 230-79; el factor de demanda también depende del inmueble (Tabla 220-42, vivienda o no). Corrección: un «Inmueble» global de nueve opciones, unión de los dos criterios (vivienda unifamiliar, vivienda popular, vivienda multifamiliar, hospital, hotel o motel, almacén, escuela, restaurante, otro). 230-79 lo lee como unifamiliar / popular / demás; la Tabla 220-42, como su renglón; vivienda o no para 220-44, 220-53 a 220-56 y la Parte D. Las justificaciones se filtran por inmueble; la de 220-42 imprime su renglón. «Equipo de acometida» queda como casilla aparte. Prueba: `R19_…`.
