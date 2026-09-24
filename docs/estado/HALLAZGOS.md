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
| R-12 · Sin factores de demanda del Art. 220 por tipo de inmueble | P2 | Pendiente | — |
| R-14 · Mínimo de 20 A en todos los contactos (criterio del Excel); sin 210-11(c) ni 220-52 | P1 | **Cerrado** | `ab9c785` |
| R-15 · Límites de caída por omisión 3 % + 3 % = 6 %, contra el 5 % combinado; aviso de caída combinada dentro de la tabla | P2 | **Cerrado** | `6ca60a1` |
| R-16 · 240-4(b) no se revisa en calibres intermedios: más cobre del necesario | P3 | Pendiente | — |
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

**R-16** — Hecho: `SeleccionConductor.DeterminarCalibreBase` revisa 240-4(b) solo en el calibre por carga; si no califica, salta al primer calibre cuya ampacidad cubre toda la protección. Caso base con principal de 60 A (230-79(d)): 12 AWG no alcanza y salta a 4 AWG (70 A); 6 AWG (55 A a 60 °C, 240-4(b) → 60 A) también cumple. Del lado seguro, con más cobre. Viene del motor copiado. Pendiente: decisión de David.
