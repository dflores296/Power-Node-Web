# Bitácora

Qué se hizo cada sesión. Del 2026-09-22 en adelante (nace el repo).

---

## 2026-09-23 — La prueba de los tres aparatos

**Lo que pidió David:** implementar los hallazgos de su prueba del 2026-09-22 —refrigerador,
microondas y air fryer, uno por fase, en un 3F-4H 220/127 V—. Los derivados salieron bien; el
alimentador no.

**Lo que se corrigió** (detalle en `HALLAZGOS.md`):

- **M-02, P0: el alimentador se dimensionaba con la carga total entre √3·V_FF**, como si el tablero
  estuviera balanceado. En el caso daba 15 A y 14 AWG cuando la fase C pide 15.25 A. Ahora gobierna
  la fase más cargada, con la corriente por barra de `CalculadoraDesbalanceo` —extraída como
  `CorrientePorFase`, sin segunda copia de la regla— y sin reescribir `CalculadoraAlimentador`: se le
  entrega la carga equivalente de esa fase. La memoria dice qué fase gobierna. Resultado: 16 A, 12 AWG.
- **M-03: aviso cuando el principal es menor que el derivado más grande.**
- **I-25: la carga se captura en VA, W o A**, con `ConsumoDePlaca.AVoltAmperes` del motor, y el
  renglón enseña el VA que resulta.
- **I-30: el tooltip de «Tipo»** ya no habla del piso de calibre que se quitó ayer.

**Lo que quedó como propuesta**, porque es decisión de diseño: I-26 (FP por circuito), I-27 (kW
reales), I-28 (avisos sin «el Excel original») e I-29 (serie sin 16/32/63 A). La lista de la prueba
metía también a I-30 entre las decisiones; no lo era, y se corrigió directo.

**Pruebas:** 40 en `PowerNode.Web.Tests` (eran 28), con el caso de los tres aparatos como regresión
de M-02 y M-03, y la «prueba pendiente» del reporte —microondas y air fryer no continuas— corrida y
verde con los valores que se calcularon a mano. **Verificado corriendo** con Playwright contra
`dotnet run`: el caso capturado en W, el VA debajo del valor, «Fase que gobierna: C» en el
alimentador, la memoria con su renglón y el cuadro impreso con «(fase C)»; sin errores de consola.

**No se pudo:** reportar M-02 en `PowerNode-DesignSuite` — la sesión no tuvo acceso a ese repo. El
texto del reporte quedó en `HALLAZGOS.md` §M-02.

**Después, el F.P. (I-26 e I-27), confirmado por David.** Su objeción a la propuesta fue la que
cambió el diseño: *«un tablero no tiene FP, las cargas sí»*. Se revisó la NOM entera buscando
«factor de potencia» y le da la razón —la Tabla 9 habla del F.P. «del circuito», el Art. 220 calcula
en VA y en amperes y en ningún lado fija 0.9—. Quedó: sin F.P. de tablero, uno por circuito
prellenado en 0.9, el del alimentador combinado de sus cargas y los kW reales en el resumen. Al
probarlo salió que el 0.9 **subestimaba** la caída de tensión de una carga resistiva en calibre
chico. 46 pruebas; verificado en el navegador con el caso capturado en W.

**Y los avisos del principal (I-28), confirmado por David.** Ya no mencionan «el Excel original». El
piso fijo de 30 A se volvió el campo «Mínimo del principal (A)», vacío por omisión, que solo avisa;
el aviso de empate con el derivado mayor se quedó, reescrito. 48 pruebas; verificado en el navegador
con y sin mínimo capturado.

**Y los tamaños de interruptor (I-29), confirmado por David.** Él corrigió los nombres: la
distinción real es centro de carga (NEMA) contra riel DIN (IEC), que ya se usa en México. Al
nombrarla salió que la lista completa de la NOM no es «riel DIN» —trae 15, 30, 35, 45 que ahí no
existen—, así que quedaron tres opciones, con centro de carga por omisión. Hubo que tocar el motor
para que la excepción 240-4(b) siga leyéndose contra la norma completa. De paso: M-04 (240-4(b)
solo en alumbrado, del escritorio) e I-31 («Acometida» cortada). 54 pruebas; verificado en el
navegador con las tres opciones.

**Después del despliegue, una duda de David:** si la carga va en VA y se cambia el F.P., ¿qué se
mueve? Solo la caída de tensión (y el cable si pasa del límite) y los kW; la corriente y el
interruptor no, porque salen directo de los VA. Pidió que se dijera en algún lado que no estorbe:
va en el tooltip del encabezado «F.P.» y en el de cada casilla, que cambia según la unidad del
renglón (W, VA o A).

**Auditoría de selección de conductor y protección.** David acotó el alcance a la Tabla 310-15(b)(16)
y pidió saber, con la NOM en la mano, si el programa cubre corriente de diseño, 125 %, aislamiento,
factores, calibres pequeños, terminales y la regla de inmediata superior/inferior. Se leyó el motor y
se corrió con casos concretos: queda en `../conocimiento/seleccion-conductor-y-proteccion.md`, con
seis propuestas por prioridad. **La 1 ya está (I-32):** el aislamiento estaba fijo en THHN y, con
factores, podía salir un calibre de menos para THW-LS. 59 pruebas; verificado en el navegador.
**La 2 también (I-33):** el porqué de la protección y del calibre en un tooltip de cada renglón, y la
sección 4 de la memoria reescrita con las dos columnas (aislamiento y terminal) en vez de la fórmula
que sustituía la capacidad mínima donde decía In. 61 pruebas.
**Y la 3 (M-05):** el 125 % ya no se multiplica con los factores; son dos revisiones, como dice
210-19(a)(1). Al hacerla salió que el ejemplo con el que se justificó la 1 (20 A continuos) estaba
calculado con la regla vieja: se corrigió con un caso que sí se sostiene (26 A no continuos) y se
dijo así en el hallazgo. 63 pruebas.

---

## 2026-09-22 (tercera parte) — El tablero entero, y el entregable que se imprime

**Lo que pidió David:** traer del escritorio cómo se arma el tablero, cómo se ocupan las fases, el
documento de exportación de cuadro de carga y memoria por tablero, el alimentador y el interruptor
principal — **sin catálogos**, y con la configuración de espacios y fases basada en el Excel.

**El Excel se leyó celda por celda**, no se supuso: quedó levantado en
[`../conocimiento/cuadro-de-carga-excel.md`](../conocimiento/cuadro-de-carga-excel.md) — los 42
espacios partidos en nones y pares, el encabezado fila por fila, las 48 comparaciones de `CO34` que
resultan ser la convención NEMA por pares, y la fila 79, que es el alimentador con **las mismas
fórmulas que un circuito**.

**Lo que se construyó:**

- **`PowerNode.Web.Modelo`, un proyecto nuevo sin Blazor.** `DatosDelTablero` (la ficha del Excel),
  `CircuitoDelCuadro` (un renglón por espacio), `CuadroDeCarga` (reparto, resumen, alimentador) y
  `Memoria/` (las nueve secciones). Es aparte para que se pueda probar con `dotnet test` en Linux
  sin cargar el runtime de WebAssembly, y para que el compilador impida que una regla termine
  dentro de un `.razor`. **23 pruebas verdes.**
- **La geometría del tablero ahora la resuelve el motor copiado**, no una función a mano en la
  pantalla: `DistribucionBarras` para qué barra toca cada espacio, `SistemaDelTablero` para cuántas
  barras hay de verdad, y `AcomodoEnGabinete` para si un multipolar cabe donde se le quiere poner.
- **Un multipolar ocupa `N`, `N+2`, `N+4` del mismo lado y se ve ocupando:** los renglones que se
  come dicen «↳ ocupado por el circuito N» y no capturan nada — repetir la carga la contaría dos o
  tres veces al sumar la columna.
- **Alimentador e interruptor principal** con `CalculadoraAlimentador` (Art. 215) y el factor de
  demanda del 220-40 donde la norma lo admite: en el total, no en el derivado. El principal **es**
  la protección del alimentador, un solo número, igual que en escritorio y que en el Excel.
- **`/documento`:** el cuadro de carga con las 24 columnas del exportador de escritorio, y la
  memoria con las nueve secciones por circuito y por alimentador. Se imprime desde el navegador —
  ver [`../decisiones/documento-imprimible-en-vez-de-archivo.md`](../decisiones/documento-imprimible-en-vez-de-archivo.md).

**Tres cosas del Excel que NO se copiaron, y por qué:**

1. **La tensión fase-neutro no es siempre ÷√3.** El Excel lo hace siempre; en un 1F-3H son ÷2 y en
   un 1F-2H la tensión capturada ya *es* la fase-neutro. Manda `SistemaDelTablero`.
2. **El piso de 30 A del principal y el «no empatar con el derivado mayor»** no están en la NOM: se
   avisan al lado del resultado y no se aplican solos. PROPUESTA pendiente de David en
   [`../decisiones/interruptor-principal-criterios-del-excel.md`](../decisiones/interruptor-principal-criterios-del-excel.md).
3. **El factor de demanda por renglón** (columnas `AX`/`AZ`) se imprime en `1.00` y se captura solo
   en el total: 220-42 lo prohíbe en el derivado y 220-40 lo pone en el alimentador.

**Verificado corriendo**, con Playwright contra `dotnet run`: captura, cuadro impreso y memoria, sin
un solo error de consola; y el PDF de impresión revisado con `media: print` emulado.

**Fuera el piso práctico de calibre**, por orden de David con el caso enfrente: *«me recomiendas
calibre 10 para contactos pero 12 para aire acondicionado con la misma carga por fase»*. Esa
diferencia no la pedía ningún artículo — la producía el piso de 10 AWG que la web traía fijo para
contactos. Ahora el conductor sale sólo del cálculo. **Deshace el I-03 de la sesión pasada**, que
justamente lo había agregado para empatar con el escritorio: la consecuencia es que los dos
programas ya no dan el mismo calibre, y queda dicho en
[`../decisiones/sin-piso-practico-de-calibre.md`](../decisiones/sin-piso-practico-de-calibre.md) con
los números medidos. Lo que **sigue** separando a contactos con cargas chicas es el piso de
protección de 20 A, que vive en el motor copiado y viene del `MAX(20, …)` del Excel; ése no se toca
desde aquí.

**Y el remate** (I-24): al bloque combinado le faltaba la línea que separa el número de la
descripción. La regla que apaga el borde de la última columna usaba `:last-child`, y en el renglón
de continuación la única celda que hay ES la del número. Se marcó la última columna con una clase;
los selectores por posición mienten en cuanto hay celdas combinadas. De paso, cada espacio mide
ahora lo mismo: un 3 polos ocupa exactamente tres renglones.

**Un multipolar ya se ve ocupando, no lo cuenta** (I-23): decía «↳ ocupado por el circuito 1» en
cada espacio que se comía, que es la primera versión del exportador de escritorio y la queja que
David hizo allá. Se reprodujo la decisión que ya estaba tomada: celdas combinadas hacia abajo menos
la del número, que sigue renglón por renglón para que se lea 1 / 3 / 5.

**Y un destrozo propio, corregido en caliente** (I-22): para marcar los grupos de columnas se reusó
`class="grupo"`, que **ya era la clase de las tarjetas de la ficha** — cada celda que abría grupo se
dibujó como tarjeta, con borde redondeado, relleno y campos de 118 px. Se acotó la regla de la ficha
a `.ficha .grupo` y se quitó la marca de grupo del cuadro: todas las celdas llevan la misma línea,
que era lo que se había pedido. El CSS de este repo no tiene ámbito y un nombre genérico alcanza
cualquier pantalla.

**Tercera pasada, también con el cuadro enfrente** (I-19 a I-21): un multipolar pintaba sus barras
en negro —`fase-abc` no existe como clase y el navegador caía al color por omisión—, las unidades
salían en mayúsculas junto con el rótulo (`MM2`, `L (M)`, `E (%)`), y las líneas entre columnas
estaban a medias: solo los grupos las tenían. Se corrigió cada letra con el color de su barra, un
`.simbolo` que apaga el `text-transform` para las unidades, líneas en todas las columnas en dos
pesos, más relleno en las celdas y el lienzo a 1720 px para que las 23 columnas quepan sin
recorrerlo de lado. La protección dejó de ir en negritas.

**Segunda corrección del mismo día, con el cuadro ya corriendo enfrente** (I-16 a I-18): los
encabezados partidos en dos líneas (`IN` sobre `A`) desalineaban el cuadro, `FASE N T` no decía
nada, y faltaban los hilos/mm²/AWG de cada conductor. Ahora la unidad va entre paréntesis en el
mismo renglón, lo que no cabe va en **tooltip**, y están las nueve columnas de conductor que el
Excel lleva en `BM`–`BX`. Y lo más gordo que faltaba: **el dibujo del interior del tablero**, que es
donde se ve en qué espacio cae cada circuito. Se portó del editor de gabinete de escritorio con su
regla —una celda por interruptor, tan alta como polos tiene— y sus pastillas de barra apiladas.

**Corrección del mismo día, a pedido de David:** los espacios estaban en dos tablas lado a lado, y
en el Excel van **una encima de la otra**. Se comprobó en el archivo antes de moverlo: la hoja no
dice «LADO IZQUIERDO» ni «DERECHO» en ninguna de sus 4 022 celdas con contenido — son los nones
(34‑54), un renglón en blanco de 6 pt (55) y los pares (56‑76), y el número de circuito es lo que
dice de qué columna del gabinete es cada uno. Ahora es **un solo `<table>` a todo lo ancho** con un
renglón separador: si fueran dos tablas apiladas, cada una calcularía sus anchos de columna por
separado y los renglones de abajo no cuadrarían con los de arriba. Se quitó el rótulo de lado
también del documento impreso.

---

## 2026-09-22 (segunda parte) — Primera pantalla viva, calculando de verdad

**M-01, I-01 y P-01 cerrados.** El sitio ya calcula un cuadro de carga en el navegador.

- **Las tablas de la norma, sin base de datos.** Proyecto `PowerNode.DesignSuite.Normativa`: las
  trece implementaciones copiadas de `Data/TablasNom` del escritorio, con un solo cambio mecánico
  (`IFuenteTablas` en vez de `DbContext`). **La lógica de interpretación no se tocó.** Los datos
  salen de `tools/extraer_tablas.py`, que extrae 14 tablas y una sección del repo público de la
  norma: **39 KB** en vez de ~9 MB. Detalle en `../conocimiento/tablas-de-la-norma.md`.
- **6/6 pruebas verdes**, contra los mismos valores que el escritorio ya verificó a mano contra el
  PDF del DOF — no contra lo que la implementación devuelve hoy.
- **Primera pantalla** (`Pages/CuadroDeCarga.razor`): dos bloques nones/pares como el Excel,
  captura de tipo/VA/longitud/polos, fase por la convención NEMA de pares, y la memoria con las
  citas de norma de cada paso.
- **CI y despliegue** escritos, con el `--check` que rompe el build si las tablas se despegan de la
  norma, y las tres trampas de Blazor en Pages cubiertas (`.nojekyll`, `base href`, `404.html`).

**Dos bugs reales, los dos encontrados CORRIENDO la pantalla en el navegador, no compilándola**
(I-02 y I-03 en `HALLAZGOS.md`): `NumeroFases` se pasaba del tablero en vez del circuito —un circuito
de 1 polo daba un tercio de su corriente— y faltaba el piso práctico de calibre. Después de
corregirlos, los tres circuitos de prueba dan **exactamente** los mismos números que la app de
escritorio.

**Queda pendiente y no lo puede hacer una sesión de Claude:** activar GitHub Pages en el repo
(Settings → Pages → Source: GitHub Actions). El workflow está escrito pero **nunca se ha ejecutado**.

---

## 2026-09-22 — Nace el repo, motor copiado y verificado

**Contexto:** venía de una sesión larga en `PowerNode-DesignSuite` (el escritorio) intentando
resolver "no puedo ver el WPF desde el trabajo". Pasó por tres planteamientos antes de aterrizar en
éste — quedan registrados para no repetirlos:

1. Blazor **Server** + SQLite, dentro del mismo repo de escritorio. **Revertido**: nunca fue la idea
   del usuario que el escritorio fuera portable con SQLite — su idea era archivos con base propia
   estilo AutoCAD/Revit, pero con SQL Server. Ver la nota del 2026-09-22 en
   `PowerNode-DesignSuite/docs/estado/00-RETOMAR-AQUI.md`.
2. Repo aparte, Blazor **WebAssembly**, **motor completo** (incluido catálogo Square D y
   coordinación) enlazado por `git submodule`. Descartado en dos partes: el submodule ata un repo
   público a uno privado (CI no podría clonarlo sin credenciales) y el catálogo Square D no debe
   viajar a un repo público — es material con derechos de autor de Schneider Electric.
3. **Esta:** alcance recortado a lo que el Excel original ya hacía — un cuadro de carga, un tablero,
   sin cascada ni catálogo ni coordinación — con el motor **copiado** (no enlazado) desde
   `PowerNode-DesignSuite`.

**Lo que se hizo:**

- Repo `dflores296/Power-Node-Web` creado (David), público.
- `Calculo/` y `Domain/` copiados de `PowerNode-DesignSuite` commit `29f660f`, excluyendo
  `Calculo/Coordinacion/`, `Domain/Catalogo/`, y las entidades de coordinación de `Domain/Proyectos`
  (`AjustesDisparoCapturados`, `CoberturaDeCoordinacion`, `CoordinacionDeProtecciones`,
  `Proteccion`, `CalculoProteccion`) y `Domain/Advertencias/AvisosDeCoordinacion`.
- Cinco ajustes quirúrgicos para que compilara solo — el más interesante es E-01 (ver
  `HALLAZGOS.md`): `FamiliaAislamiento` estaba mezclado dentro del código de coordinación aunque lo
  usa una tabla de ampacidad común.
- **Verificado compilando**, no sólo copiado: `dotnet build` limpio, 119 archivos, 10 719 líneas,
  cero referencia a EF Core / SQL Server / Square D.
- Estructura de `docs/` armada: `estado/` (tope de 4 archivos), `decisiones/` (con autor y estado),
  `conocimiento/`, `historico/` — patrón tomado de `AbaSuite` y `msa-toolkit`.

**Queda para la próxima sesión:** implementar las tablas de la norma sin base de datos (M-01), y
empezar la primera pantalla.
