# Hallazgos

Registrar cada defecto con ID estable, prioridad y commit de cierre. Ningún hallazgo se cierra sin
commit.

ID: `<letra>-<número>` — `E` estructura, `P` publicación, `M` motor, `I` interfaz. Prioridad: `P0`
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
| I-05 · Sin guardar ni abrir el proyecto | P1 | Pendiente | — |
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

**M-03** — Hecho: sin aviso con principal menor que el derivado más grande. Corrección: aviso con el número de circuito. Prueba: `M03_…`. Abierto: aviso o bloqueo.

**M-04** — Hecho: 240-4(b) solo en Alumbrado. Corrección: aplicar en Alumbrado y Equipo; excluir Contactos. Referencia: 240-4(b)(1). Prueba: `Excepcion240_4b_AplicaEnEquipo_NoEnContactos` (32 A continuos, 9 agrupados: Equipo 8 AWG, Contactos 6 AWG).

**M-05** — Hecho: el 125 % se comparaba contra la ampacidad corregida (32 A continuos, 9 agrupados: 6 AWG). Corrección: `SeleccionConductor.CalibrePorDosRevisiones` — 125 % contra la ampacidad de tabla sin factores; carga al 100 % contra la corregida (8 AWG). No aplica a motores (430-22). Referencia: 210-19(a)(1), 215-2(a)(1). Prueba: `DosRevisiones_…`.

**M-06** — Hecho: sin declaración de equipo marcado 75 °C. Corrección: campo «Terminales»; `TemperaturaTerminales.Para(protección, marcado75C, aislamiento)`. Conductor de 60 °C: columna de 60 °C. Referencia: 110-14(c)(1)a.(1) y a.(3). Prueba: `Terminales_…` (45 A no continuos: 6 AWG a 60 °C, 8 AWG a 75 °C).

### Interfaz

**I-01** — Hecho: sin pantalla. Corrección: pantalla de captura.

**I-02** — Hecho: `NumeroFases` tomaba las fases del tablero (720 VA, 1 polo: 1.89 A). Corrección: pasar los polos del circuito (5.67 A). Prueba: `ElCircuitoSeCalculaConSusPolos_…`.

**I-03** — Hecho: sin piso práctico de calibre. Corrección revertida por decisión de David — [`../decisiones/sin-piso-practico-de-calibre.md`](../decisiones/sin-piso-practico-de-calibre.md).

**I-04** — Hecho: sin resumen de carga ni balanceo. Corrección: resumen, balanceo por fase y desbalanceo.

**I-05** — Pendiente: guardar y abrir el proyecto como archivo.

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
