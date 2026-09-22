# Bitácora

Qué se hizo cada sesión. Del 2026-09-22 en adelante (nace el repo).

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
