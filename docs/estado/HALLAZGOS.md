# Hallazgos

Cada fila es un defecto o un cabo suelto real, con **ID estable** que no cambia aunque el documento
se reordene. Patrón de `msa-toolkit` (`docs/auditoria-2026-08-31.md`): **el hash de commit es la
prueba de que se cerró**, no una palabra.

ID: `<letra>-<número>`. La letra dice el frente (`M` motor, `I` interfaz, `P` publicación,
`E` estructura). Prioridad `P0` (bloquea) → `P3` (cosmético).

| ID | Hallazgo | Prioridad | Estado | Commit |
|---|---|---|---|---|
| E-01 · El motor traía código de coordinación mezclado dentro de `TablasNom` | P2 | **Cerrado** | este commit |
| M-01 · No hay implementación de las tablas de la norma sin base de datos | P0 | **Cerrado** | este commit |
| I-01 · No existe ninguna pantalla todavía | P0 | **Cerrado** | este commit |
| P-01 · No hay workflow de despliegue a GitHub Pages | P1 | **Cerrado** (sin ejecutar) | este commit |
| I-02 · `NumeroFases` se pasaba del tablero, no del circuito | P0 | **Cerrado** | este commit |
| I-03 · El piso práctico de calibre no se aplicaba | P1 | **Revertido por decisión de David** | ver nota |
| P-02 · Pages quedó en «Deploy from a branch», sirve el README y no la app | P1 | **Cerrado** | David lo cambió a GitHub Actions |
| I-06 · Pantalla de carga: una bola negra, y el favicon de Blazor | P2 | **Cerrado** | este commit |
| P-03 · El navegador se quedaba con el CSS y los iconos viejos | P1 | **Cerrado** | este commit |
| I-07 · Faltaba `favicon.ico`: el navegador seguía con el icono de Blazor | P1 | **Cerrado** | este commit |
| I-08 · El selector de tipo cortaba el texto: «Alumbrac» | P2 | **Cerrado** | este commit |
| I-04 · No hay resumen de carga ni balanceo por fase | P2 | **Cerrado** | `cc92ad5` |
| I-05 · No se puede guardar ni abrir un proyecto | P1 | Pendiente | — |
| I-09 · Los datos de identificación del Excel no se capturaban | P2 | **Cerrado** | `cc92ad5` |
| I-10 · La fase del espacio se calculaba en la pantalla, no en el motor | P1 | **Cerrado** | `cc92ad5` |
| I-11 · Un multipolar no ocupaba los espacios que se come | P1 | **Cerrado** | `cc92ad5` |
| I-12 · No había alimentador ni interruptor principal | P1 | **Cerrado** | `cc92ad5` |
| I-13 · No había documento imprimible ni memoria por tablero | P1 | **Cerrado** | `cc92ad5` |
| I-14 · La tensión F-N salía siempre de dividir entre √3 | P1 | **Cerrado** | `cc92ad5` |
| I-15 · Los circuitos de Fuerza (Art. 430) no se pueden capturar | P2 | Pendiente | — |
| I-16 · Encabezados partidos en dos líneas: el cuadro se leía torcido | P2 | **Cerrado** | `c2bb19c` |
| I-17 · El cuadro no traía hilos, mm² ni designación del conductor | P2 | **Cerrado** | `c2bb19c` |
| I-18 · No se dibujaba el interior del tablero ni dónde cae cada circuito | P1 | **Cerrado** | `c2bb19c` |
| I-19 · Un circuito multipolar pintaba sus barras en negro | P2 | **Cerrado** | `4730182` |
| I-20 · Unidades en mayúsculas: «MM2», «L (M)», «E (%)» | P2 | **Cerrado** | `4730182` |
| I-21 · Sin líneas entre columnas, el cuadro se veía amontonado | P2 | **Cerrado** | `4730182` |
| I-22 · La clase `grupo` del cuadro chocaba con la de las tarjetas: cada celda se dibujó como tarjeta | P1 | **Cerrado** | `228478d` |
| I-23 · Un multipolar decía «ocupado por el circuito N» en vez de verse ocupando | P2 | **Cerrado** | `acdcf42` |
| I-24 · Al bloque combinado le faltaba la línea del número: `:last-child` se la comía | P2 | **Cerrado** | `73cb16b` |
| M-02 · El alimentador se dimensionaba como si el tablero estuviera balanceado | P0 | **Cerrado** | `b37de00` |
| M-03 · No se avisaba cuando el principal es menor que el derivado más grande | P1 | **Cerrado** (aviso; bloqueo a decisión de David) | `b37de00` |
| I-25 · La captura solo aceptaba VA; las placas dicen W o A | P1 | **Cerrado** | `b37de00` |
| I-26 · El factor de potencia es uno solo para todo el tablero | P1 | **Cerrado** | `2ebf20f` |
| I-27 · «Total (kW)» sale de multiplicar los VA por el FP del tablero | P2 | **Cerrado** | `2ebf20f` |
| I-28 · Los avisos del interruptor principal citan «el Excel original» | P2 | **Cerrado** | `e7fdf0a` |
| I-29 · La 240-6(a) de la NOM trae 16, 32 y 63 A; un centro de carga QO/NQ no | P2 | **Cerrado** | `b9406ab` |
| I-30 · El tooltip de «Tipo» describía un piso de calibre que ya no existe | P3 | **Cerrado** | `b37de00` |
| M-04 · La excepción 240-4(b) solo se concede en Alumbrado, no en Equipo | P3 | Pendiente — es del motor de escritorio | — |
| I-32 · El aislamiento estaba fijo en THHN: con factores podía salir un calibre de menos | P0 | **Cerrado** | `b1a84d6` |
| I-33 · La memoria decía «Icm = In / (FT × FA)» y sustituía la capacidad mínima; el porqué no se veía en pantalla | P1 | **Cerrado** | este commit |
| I-31 · «Acometida» se cortaba en «Interruptor prin» | P3 | **Cerrado** | `b9406ab` |

---

### E-01 — `FamiliaAislamiento` vivía dentro de `Coordinacion/CurvaDanioConductor.cs` · este commit

Al copiar `Calculo/` excluyendo `Coordinacion/` (decisión: coordinación es v2), la compilación
reventó porque `ITablaAislamiento.cs` — que **sí** es del alcance base, decide qué columna de
ampacidad usar — dependía de un enum, `FamiliaAislamiento`, que físicamente vivía adentro del
archivo de verificación de daño térmico del conductor contra la curva de disparo. Dos
responsabilidades sin relación (la ampacidad tabulada de un circuito común, y la coordinación
tiempo-corriente contra una protección) compartían un archivo por conveniencia de cuándo se escribió,
no por diseño.

**Corregido aquí** moviendo el enum a su propio archivo en `TablasNom/FamiliaAislamiento.cs`, donde
ya vive el resto de lo que usa `ITablaAislamiento`.

**Vale la pena reportarlo en `PowerNode-DesignSuite`** (el repo de escritorio, de donde salió) —
ahí el acoplamiento no rompe nada porque `Coordinacion` siempre está presente, pero es la misma
mezcla de responsabilidades. No se tocó ese repo desde aquí.

### I-02 — `NumeroFases` era el del tablero, no el del circuito · este commit

**Bug real, encontrado corriendo la pantalla, no compilándola.** Un circuito de 1 polo con 720 VA en
un tablero de 220 V 3F daba **1.89 A** en vez de **5.67 A** — exactamente un tercio. La pantalla
pasaba `NumeroFases: 3` (las del tablero), y el motor repartía la carga entre las tres fases como si
fuera una carga trifásica.

`NumeroFases` es **cuántas barras toca el circuito**, o sea sus polos. El escritorio lo resuelve con
`circuito.Fase.Length` (`CalculoCircuitoDerivadoService.MapearEntrada`). Corregido pasando
`c.Polos`, y de paso se agregó la columna de polos a la pantalla — que además es fiel al Excel, que
lleva una columna `P × A`.

### I-03 — Faltaba el piso práctico de calibre · este commit

Con el bug anterior corregido, la web daba **14 AWG / 2.07 %** donde el escritorio da
**12 AWG / 1.34 %**, y los dos cumplen el límite de caída del 3 %. La diferencia no era del cálculo:
`ConfiguracionProyecto` en escritorio trae un **piso práctico** por omisión —12 AWG en Alumbrado,
10 AWG en Contactos— más estricto que el normativo (14 AWG, 210-19(a)(4)), y la pantalla pasaba
`PisoPracticoCalibreMm2: null`.

Es criterio de diseño, no norma, y por eso vive en la configuración y no en el motor. Ahora la
pantalla lo aplica según el tipo de carga, con los mismos valores por omisión. **Equipo se queda sin
piso a propósito**: un aparato se dimensiona por su consumo de placa.

> ### ⛔ REVERTIDO el 2026-09-22 por decisión de David
>
> Este arreglo **se deshizo el mismo día**, y no por estar mal implementado: David vio el efecto
> —contactos en 10 AWG y un equipo con la misma carga por fase en 12— y ordenó quitar el piso.
> **La web ya no aplica ninguno.** No volver a agregarlo sin que él lo pida: está razonado, con los
> números medidos, en
> [`../decisiones/sin-piso-practico-de-calibre.md`](../decisiones/sin-piso-practico-de-calibre.md).

### M-01 — Las tablas de la norma, sin base de datos · este commit

`PowerNode.DesignSuite.Normativa` implementa las trece interfaces de `Calculo/TablasNom` leyendo un
JSON de **39 KB** en vez de SQL Server. Detalle en
[`../conocimiento/tablas-de-la-norma.md`](../conocimiento/tablas-de-la-norma.md).

**Hallazgo lateral, que quedó fijado con una prueba:** la lista de valores estandarizados de
240-6(a) de la **NOM incluye los valores IEC** —16, 32, 63— que el NEC no tiene. La primera versión
de la prueba esperaba que el inmediato superior de 15.1 A fuera 20 A, por costumbre del NEC, y falló:
son **16 A**. El código estaba bien; la prueba estaba mal.

### P-02 — Pages sirve el README, no la aplicación

**El `deploy.yml` funciona: el que está mal configurado es el repo.** La corrida del 2026-09-22
(`35681198926`) pasó los once pasos del job `build` —clonar la norma, `--check` de las tablas, las
6 pruebas, `dotnet publish` del WASM y subir el artefacto— y **falló sólo en `actions/deploy-pages@v4`**,
que es exactamente lo que ocurre cuando el origen de Pages no es GitHub Actions.

La prueba del otro lado: hay una corrida `pages build and deployment`
(`dynamic/pages/pages-build-deployment`) que **sí** tuvo éxito. Ése es el workflow de **Jekyll** que
GitHub dispara solo cuando el origen es una rama — y es el que está publicando el `README.md`
renderizado en lugar de la aplicación.

**Arreglo:** `Settings → Pages → Build and deployment → Source:` cambiar de *Deploy from a branch* a
**GitHub Actions**. No lo puede hacer una sesión de Claude.

### I-06 — La bola negra del arranque, y el icono morado · este commit

Dos defectos visibles que **sólo se ven en el sitio publicado**, no corriendo en local, porque son
del primer instante de carga y de la pestaña del navegador:

1. **La bola negra.** La plantilla de Blazor pone un `<svg class="loading-progress">` con dos
   círculos, y los dibuja con estilos que ella misma mete en `css/app.css`. Al reescribir esa hoja
   por completo, esos estilos se fueron — y **un `<circle>` de SVG sin `fill` declarado se pinta
   negro sólido**. De ahí el disco negro en la esquina.
2. **El icono morado.** El `favicon.png` que trae la plantilla, nunca reemplazado.

**Corregido con la marca real**, tomada de `Recursos/Marca/` del repo de escritorio
(`powernode-icono.svg` y sus variantes): el unifilar de nodo, barra y tres derivaciones. Los colores
de la aplicación se alinearon a los de la marca —tinta `#101418`, azul `#0B6E99`, gris `#5A6570`—
en vez de las aproximaciones que traía.

**El favicon lleva `prefers-color-scheme` adentro del SVG**, y eso no es adorno: la tinta de la marca
es casi negra y la barra de pestañas del usuario está en tema oscuro, donde el logo desaparecía. El
azul del nodo no cambia — tiene contraste contra los dos fondos.

**Verificado como lo sirve GitHub Pages, no en local:** `dotnet publish` + el mismo `sed` del
`base href` del workflow, servido desde un subdirectorio `/Power-Node-Web/`. Arranca sin un solo
error de consola ni un 404.

### P-03 — Caché: el index nuevo con el CSS viejo · este commit

**El despliegue era correcto y aun así el sitio se veía mal.** Tras publicar la marca, la pantalla
de carga salía **sin formato** —el logo arriba a la izquierda, el texto en color de enlace— y la
pestaña seguía con el icono morado de Blazor.

No era el despliegue: la corrida de `7cf69c5` terminó en verde a las 03:54:58, y esa vez **ya no
corrió el workflow de Jekyll**, lo que confirma que P-02 quedó resuelto. Era el **navegador**, que
tenía en caché el `css/app.css` anterior. Con el `index.html` nuevo —que ya trae el marcado
`.arranque`— y la hoja vieja —que aún no tiene sus reglas—, el resultado es marcado sin estilo. Los
favicons son peor: se cachean tan agresivamente que ni `Ctrl+F5` los refresca.

**Es exactamente el problema que `msa-toolkit` ya documentó** en su `docs/despliegue.md`, con las
mismas palabras: *«la página se ve igual y parece que no se publicó nada»*.

**Corregido con el mismo patrón, pero automatizado:** el workflow le pega `?v=<hash del commit>` al
CSS y a los tres iconos al publicar. `msa-toolkit` lo lleva a mano (`?v=20260830b`, que hay que
acordarse de subir en cada cambio); aquí sale del `GITHUB_SHA`, porque un número que depende de la
memoria tarde o temprano no se sube. `_framework/` no lo necesita: Blazor ya versiona lo suyo.

**De paso, tres iconos en vez de uno:** el SVG (nítido, con `prefers-color-scheme`), un PNG de 32
para el navegador que no sirva SVG como icono, y uno de 180 para iOS.

**Verificado sirviendo el publicado desde `/Power-Node-Web/` con caché vacía:** `.arranque` computa
`display=flex` centrado, los tres iconos llevan su `?v=`, cero errores y cero 404.

### I-07 — El icono de Blazor sobrevivió a borrar el archivo, al `?v=` y a limpiar la caché

Tres intentos fallaron antes de dar con la causa, y vale la pena registrarlos porque el diagnóstico
equivocado costó dos rondas:

1. Se borró `wwwroot/favicon.png` (el de Blazor) y se declaró un SVG. **No bastó.**
2. Se agregó `?v=<commit>` al icono. **No bastó** — y de hecho fue contraproducente: las query
   strings en favicons se comportan distinto en cada navegador, y hay casos documentados en que
   hacen que el icono se ignore.
3. El usuario limpió la caché. **Tampoco bastó.**

**La causa: la plantilla de Blazor nunca trajo un `favicon.ico`, sólo un `favicon.png`.** Al borrar
ese PNG sin poner un `.ico` en su lugar, el navegador se quedó **sin nada que pedir implícitamente**
— y `favicon.ico` en la raíz del sitio es justo lo que Chromium y Edge piden solos, sin que ningún
`<link>` se lo diga. Sin esa petición, Edge siguió pintando lo que tenía en su base de favicons, que
es un almacén aparte y muy pegajoso.

**Corregido cubriendo los cuatro caminos por los que un navegador puede llegar al icono:**

| Archivo | Para qué |
|---|---|
| `favicon.ico` (raíz) | El que el navegador pide solo. Tres tamaños reales adentro: 16, 32 y 48. |
| `favicon.png` (raíz) | La URL exacta que usaba la plantilla. Si algo la sigue pidiendo, ahora recibe Power Node y no un 404. |
| `marca/powernode-favicon.svg` | El bueno donde se soporte, con `prefers-color-scheme`. |
| `marca/powernode-180.png` | iOS, al guardar en la pantalla de inicio. |

El `.ico` se construyó a mano (encabezado ICONDIR + PNG embebido por tamaño) porque el contenedor no
tiene PIL ni ImageMagick, y se verificó su estructura byte a byte. Los tamaños chicos llevan **fondo
blanco**: un icono transparente con la tinta casi negra de la marca desaparece en una pestaña
oscura, y un `.ico` no admite media queries.

**Verificado observando qué pide el navegador de verdad**, no suponiéndolo: con caché vacía,
Chromium pide `marca/powernode-favicon.svg` y lo recibe con 200, y los cuatro archivos se sirven con
su tipo MIME correcto desde el subdirectorio `/Power-Node-Web/`.

> **Para distinguir "no se publicó" de "el navegador no lo suelta"**, abre el archivo directo:
> `https://dflores296.github.io/Power-Node-Web/favicon.ico`. Si ahí se ve el logo, el sitio está
> bien y lo que queda es caché del navegador.

### I-08 — El selector de tipo decía «Alumbrac» · este commit

A 92 px de ancho, el navegador cortaba «Alumbrado» en **«Alumbrac»** y «Contactos» en
«Contacto». **Un campo que miente sobre su propio valor es peor que uno estrecho** — sobre todo
cuando el valor decide el piso de protección de 15/20 A, así que leer mal el tipo es leer mal el
resultado. Subido a 108 px, que es lo que pide la palabra completa más la flecha.

Salió al revisar la captura de la pantalla ya redondeada, no de una prueba: es la clase de defecto
que ninguna aserción atrapa porque el valor del `<select>` era correcto todo el tiempo — lo que
fallaba era que el usuario no podía leerlo.

---

### I-10 — La fase de cada espacio se calculaba en la pantalla · `cc92ad5`

`Pages/CuadroDeCarga.razor` traía su propia copia de la convención NEMA
(`"ABC"[((numero - 1) / 2) % 3]`) y su propia noción de cuántos polos caben (`_tensionFF <= 127 ? [1]
: [1,2,3]`). Las dos eran correctas para el caso trifásico y **las dos estaban mal para un 1F-3H**:
un tablero de dos barras admite interruptores de 2 polos, y con la regla de la tensión no los
ofrecía.

**Corregido** delegando en `DistribucionBarras.FasesQueOcupa` y `SistemaDelTablero.MaximoPolos`, que
son donde el motor guarda esa regla — el mismo arreglo que el escritorio hizo el 2026-08-21.

### I-11 — Un interruptor de 2 o 3 polos no ocupaba nada · `cc92ad5`

El selector de polos cambiaba la fase que se mostraba, pero los espacios `N+2` y `N+4` **seguían
capturando carga propia**: se podían capturar dos circuitos encima del mismo interruptor, y los dos
sumaban al total.

**Corregido** con `AcomodoEnGabinete.MotivoNoCabe` —el mismo validador del editor de gabinete de
escritorio— y marcando los renglones ocupados como continuación. Un cambio de polos que no cabe
**no se aplica y dice por qué**, en vez de dejar que el selector mienta.

### I-14 — La tensión fase-neutro salía siempre de dividir entre √3 · `cc92ad5`

Venía del Excel (`=ROUND(T22/SQRT(3),1)`), y es correcto **solo en una estrella**. En un centro de
carga de 240 V (1F-3H, derivación central) daba **138.6 V** en vez de 120 — y con esa tensión, la
corriente de cada circuito sale 13 % baja.

**Corregido** llamando a `SistemaDelTablero.TensionFaseNeutro`, que distingue los tres casos. Lo
cubre `LaTensionFaseNeutroNoEsSiempreEntreRaizDeTres`.

### I-16 a I-18 — Lo que David señaló al ver el cuadro corriendo · `c2bb19c`

Tres cosas, todas de la misma raíz: **el cuadro enseñaba resultados pero no se dejaba leer.**

1. **Encabezados partidos en dos líneas** (`IN` sobre `A`, `L` sobre `m`). Con el resto de las
   columnas en una sola línea y todo centrado verticalmente, las cabeceras no cuadraban entre sí.
   Ahora la unidad va entre paréntesis en el mismo renglón —`In (A)`, `L (m)`, `e (%)`— y lo que no
   cabe en el rótulo va en un **tooltip**, no en una segunda línea. Y `FASE N T`, que no decía nada,
   ahora es **Fase / Neutro / Puesta a tierra**.
2. **Faltaban los hilos, los mm² y la designación AWG/kcmil** de cada conductor — las nueve columnas
   que el Excel sí lleva (`BM`–`BX`) y que el documento impreso ya emitía. El cuadro de captura las
   tenía resumidas a una sola columna por conductor.
3. **No se dibujaba el interior del tablero.** El editor de gabinete es de lo más útil que tiene la
   versión de escritorio y aquí no existía: no había dónde ver **en qué espacio cae cada circuito**
   ni qué barra muerde. Se portó con la misma regla de allá — **una celda por interruptor, tan alta
   como polos tiene**, no una por espacio.

### I-19 — Un multipolar pintaba sus barras en negro · `4730182`

La celda de barras salía con `class="fase-@c.Fases.ToLowerInvariant()"`, así que un circuito de una
fase daba `fase-a` —que existe— y uno de tres daba **`fase-abc`, que no existe**: el navegador no
encontraba la regla y caía al color por omisión. **Se veía como una decisión de diseño y era un
error de composición.** Ahora cada letra se pinta sola, con el color de su barra: la A vino, la B
azul, la C verde, también dentro de «ABC».

### I-20 — Las unidades se pasaban a mayúsculas con el rótulo · `4730182`

`text-transform: uppercase` no distingue entre una palabra y un símbolo, así que el encabezado
imprimía **`MM2`**, **`L (M)`**, **`E (%)`** e **`IN (A)`**. Un símbolo de unidad con la caja
cambiada no es un detalle tipográfico: `M` es mega y `m` es metro, y en un documento que se firma
eso es un error. El rótulo se queda en mayúsculas —es lo que pidió David— y la unidad va dentro de
un `.simbolo` que apaga la transformación: **`mm²`, `L (m)`, `In (A)`, `e (%)`, `AWG/kcmil`**.

### I-21 — El cuadro se veía amontonado y con divisiones a medias · `4730182`

Solo los grupos (Fase, Neutro, Puesta a tierra) llevaban línea vertical, así que las tres columnas
de adentro —hilos, mm², AWG/kcmil— quedaban como un bloque suelto y el encabezado parecía
incompleto, sobre todo en la frontera con la caída de tensión. Ahora **todas** las columnas llevan
su línea, en dos pesos: fina entre hermanas, marcada donde empieza un grupo. Y más aire: el
relleno de las celdas subió, y el lienzo pasó de 1400 a 1720 px porque con 23 columnas el cuadro
no cabía y había que recorrerlo de lado para ver la caída de tensión.

**Y la protección dejó de ir en negritas.** Una columna entera en negritas no dice «esto importa
más», dice que el cuadro tiene un favorito; las negritas se quedan para los renglones de total.

### I-22 — `grupo` era el nombre de dos cosas distintas · `228478d`

Para marcar dónde abría cada grupo de columnas se reusó `class="grupo"` — **que ya era la clase de
las cuatro tarjetas de la ficha del tablero** (`background`, `border`, `border-radius: 14px`,
`padding: 12px 14px` y `width: 118px` para sus campos). Resultado: cada celda que abría grupo se
dibujó **como una tarjeta**, con su borde redondeado y su relleno, y los campos de captura se
estiraron a 118 px. El cuadro quedó irreconocible.

**Dos arreglos, y el segundo es el que evita la reincidencia:**

1. Las reglas de la ficha se acotaron a `.ficha .grupo`, así que ya no pueden alcanzar nada fuera
   de ella.
2. La marca de grupo en el cuadro **se eliminó**: ahora *todas* las celdas llevan la misma línea,
   que es lo que se había pedido —una rejilla pareja, como una hoja de cálculo— y de paso deja de
   existir la clase que causó el choque.

**Lo que esto enseña:** el CSS de este repo no tiene ámbito. Una clase con nombre genérico
(`grupo`, `fila`, `celda`) alcanza cualquier elemento de cualquier pantalla. Los nombres nuevos van
acotados a su bloque.

### I-23 — La ocupación de un multipolar se leía en vez de verse · `acdcf42`

Un interruptor de 2 o 3 polos ponía «↳ ocupado por el circuito 1» en cada espacio que se comía.
**Es exactamente la primera versión del exportador de escritorio, y David la señaló allá con la
misma razón:** eso *se lee* en vez de *verse*.

La solución es la que ya quedó decidida en `PowerNode-DesignSuite`
(`docs/referencia/cuadro-de-carga.md` §5) y aquí se reprodujo tal cual: **las celdas se combinan
hacia abajo** cubriendo los espacios que ocupa —así se ve que abarca tres renglones, como en
cualquier directorio de tablero—, **menos la columna del número, que no se combina**, porque
1 / 3 / 5 es justo lo que se quiere leer. Contenido centrado en el alto que abarca, igual que el
`XLAlignmentVerticalValues.Center` del exportador de escritorio.

Va en la captura y en el documento impreso. Y sigue sin repetir la carga en los renglones
ocupados: repetirla haría que sumar la columna la contara dos o tres veces.

### I-24 — `:last-child` le quitaba el borde al renglón de continuación · `73cb16b`

La regla que apaga el borde derecho de la última columna estaba escrita como
`.cuadro td:last-child { border-right: 0 }`. **En el renglón de continuación de un multipolar la
única celda que existe es la del número** —las demás vienen combinadas desde arriba—, así que
`:last-child` la alcanzaba y le quitaba su línea derecha: justo la que separa el número de la
descripción. El bloque combinado se veía sin borde por ese lado, que fue lo que David señaló
(marcado en rojo sobre el sitio publicado, en el teléfono).

**Arreglado marcando la última columna con una clase** (`.ultima`) en vez de deducirla de la
posición: la posición cambia con `rowspan`, el rótulo no.

De paso, cada espacio ocupa ahora el mismo alto (`.cuadro tbody tr { height: 30px }`). Antes el
navegador repartía el alto del contenido combinado y un interruptor de 3 polos salía **más corto**
que los tres espacios que abarca; ahora mide exactamente tres renglones, como en un directorio de
tablero.

**Lo que enseña, que es lo mismo que I-22:** los selectores por posición (`:last-child`,
`:nth-child`) mienten en cuanto la tabla deja de ser una rejilla pareja. Con celdas combinadas, lo
que hay que marcar es el papel de la celda, no dónde cayó.

---

## Prueba del 2026-09-22: refrigerador, microondas y air fryer

Prueba manual de David con tres cargas en un 3F-4H 220/127 V de 6 espacios —cobre THHN en PVC a
30 °C, 3 agrupados, FP 0.9, 20 m, e% máx. 3 %—, las tres de tipo Equipo y continuas: refrigerador
750 VA en la fase A, microondas 1500 VA en la B, air fryer 1550 VA en la C. **Los tres derivados
salieron correctos**, verificados a mano. Los hallazgos estaban en el alimentador, en la captura y en
los textos. Ese caso quedó como prueba de regresión (`CuadroDeCargaTests.TresAparatos`).

### M-02 — El alimentador se dimensionaba como si el tablero estuviera balanceado · `b37de00`

**Bug de cálculo, del lado inseguro.** El alimentador tomaba la carga **total** entre √3·V_FF:
3800 / (√3 × 220) = **9.97 A** → 12.46 A al 125 % → **principal de 15 A y fase de 14 AWG**. Pero
cada fase lleva su propia corriente, y la C trae la air fryer: 1550 / 127 = **12.20 A continuos →
15.25 A al 125 %**, que ya no cabe ni en 15 A ni en un 14 AWG (15 A en la columna de 60 °C). **El
resultado impreso estaba subdimensionado en un caso real.**

**El Excel sí lo hacía bien** (`CU79`/`CV79`: `MAX(1.25·CO78+CR78, 1.25·CP78+CS78, 1.25·CQ78+CT78)`
entre la tensión F-N), así que frente a él era una regresión.

**Corregido sin reescribir el motor:**

1. **La corriente de cada barra sale de la regla del desbalanceo**, que ya medía en corriente por
   fase. Se extrajo como `CalculadoraDesbalanceo.CorrientePorFase` —`Porcentaje` la usa— para que no
   haya dos copias. Se suma en **corriente, no en VA**: un interruptor de 2 polos a 220 V lleva su
   corriente completa por cada línea, no la mitad de sus VA entre 127. La corriente de cada circuito
   sale de su carga (`TensionDeCalculo.Divisor`), no de su resultado, para que un renglón que el motor
   rechazó siga pesando en el alimentador.
2. **Gobierna la fase que pide más capacidad**: `max(factor × continua + no continua)`, con el
   factor de demanda del 220-40 aplicado antes y el mismo 125 % (o 100 % con el ensamble aprobado)
   de la calculadora.
3. **Se le entrega al motor la carga que da exactamente esa corriente** —su corriente por el mismo
   divisor; en un 3F-4H, 3 × los VA de la fase—. Así protección, conductor, tierra **y caída de
   tensión** salen con la corriente de la fase más cargada, y `CalculadoraAlimentador` queda como se
   copió.
4. **La memoria dice qué fase gobierna** (sección 3): «Fase C, la más cargada: 125 % × 12.20 A
   (continua) + 0.00 A (no continua) = 15.25 A…», y la pantalla y el cuadro impreso lo marcan junto a
   la corriente de diseño.

**Resultado en el caso:** In = 12.20 A, capacidad mínima 15.25 A → **principal de 16 A** (20 A si se
confirma I-29) y **fase de 12 AWG**. El reporte decía 15.26 A porque redondeó la tensión a 127 V;
aquí es 220/√3 = 127.02 V. Un tablero balanceado da lo mismo que antes
(`M02_UnTableroBalanceadoDaLoMismoQueAntes`).

**Se reporta también en `PowerNode-DesignSuite`** (regla de `CLAUDE.md`: el motor es copia de allá).
Allá la cascada entrega la carga total al alimentador y `CalculoTablero.BreakerPrincipalA` reusa
`CalculadoraProteccionAlimentador` con la misma suma, así que el defecto tiene toda la pinta de
existir igual. **Pendiente: la sesión del 2026-09-23 no tuvo acceso a ese repo**; el texto del
reporte está abajo, listo para abrirlo como issue.

> **M-02 (desde Power Node Web): el alimentador se dimensiona con la carga total, no con la fase más
> cargada.** `CalculadoraAlimentador` / `CalculadoraProteccionAlimentador` calculan la corriente
> como carga total / (√3·V_FF). Con 750 VA en A, 1500 VA en B y 1550 VA en C (continuas, 3F-4H
> 220/127 V) da 9.97 A → principal de 15 A y 14 AWG, cuando la fase C lleva 12.20 A → 15.25 A al
> 125 % → 16 A y 12 AWG. El Excel de referencia (`CU79`/`CV79`) usa la fase más cargada entre la
> tensión F-N. En la web se corrigió sin tocar la calculadora: `CalculadoraDesbalanceo.CorrientePorFase`
> (extraída de `Porcentaje`) da la corriente por barra, y al alimentador se le entrega la carga
> equivalente de la fase que gobierna. Revisar `CalculoTablero.BreakerPrincipalA` y la cascada.

### M-03 — Principal menor que el derivado más grande, sin aviso · `b37de00`

En el caso, antes de M-02, el principal quedó en **15 A** con un derivado de **16 A** (la air fryer),
y la pantalla no dijo nada: solo había aviso cuando eran **iguales**. Ahora hay uno cuando
`principal < max(derivados)`, que nombra el circuito y no menciona el Excel (ver I-28). Con M-02
corregido el caso ya no lo dispara, así que la prueba usa otro: 500 VA de contactos → derivado de
20 A por el mínimo de contactos, principal de 15 A.

**Es aviso, no bloqueo.** Cuál de los dos, lo decide David — propuesta en
[`../decisiones/interruptor-principal-criterios-del-excel.md`](../decisiones/interruptor-principal-criterios-del-excel.md).

### I-25 — La captura solo aceptaba VA · `b37de00`

El motor ya traía `ConsumoDePlaca.AVoltAmperes` (VA / W / A), y el escritorio lo usa en
`CircuitoDerivado.VaUnitarioDe`; la web no lo exponía. Ahora cada renglón lleva **unidad** (VA por
omisión, así que nada de lo ya capturado cambia) y el valor **tal como viene en la placa**
(`Continua`/`NoContinua`). `CuadroDeCarga` convierte con esa misma función —la tensión y los polos
del circuito, el FP del tablero— y deja el resultado en `ContinuaVA`/`NoContinuaVA`, que es lo que
usa todo lo demás. **Debajo de cada valor en W o A se ve el VA con el que se calcula.** Capturar
«8 A» regresa una corriente de diseño de 8 A (`I25_LosAmperesCapturadosRegresanComoLosMismosAmperes`).

Mientras no se decida I-26, los W se convierten con el FP del tablero.

### I-26 e I-27 — El F.P. es de cada carga, y los kW son reales · `2ebf20f`

Confirmado por David el 2026-09-23, después de revisar la NOM: **un tablero no tiene F.P., sus cargas
sí** (la nota 2 de la Tabla 9 habla del «factor de potencia del circuito»). Se quitó el F.P. de la
ficha; cada renglón lleva el suyo, prellenado en 0.9; el del alimentador resulta de combinar las
cargas de la fase que gobierna, y el resumen enseña los kW reales (en el caso, **3.65**, no 3.42) y
el F.P. resultante. Detalle y citas en
[`../decisiones/factor-de-potencia-por-circuito.md`](../decisiones/factor-de-potencia-por-circuito.md).

**Salió al probarlo:** suponer 0.9 en una carga resistiva **subestimaba** la caída de tensión (en
12 AWG manda la R: Ze sube de 6.04 a 6.60 Ω/km al pasar de 0.9 a 1.0). La air fryer con F.P. 1 da
2.54 %, no 2.31 %.

### I-28 — Los avisos del principal ya no hablan del Excel · `e7fdf0a`

David eligió la combinación propuesta: el **piso de 30 A** se volvió el campo «Mínimo del principal
(A)» de la ficha, vacío por omisión, y **solo avisa** cuando el calculado queda debajo —ya no sale en
todos los tableros chicos—; el **empate con el derivado mayor** conserva su aviso, redactado sin el
Excel. Detalle en
[`../decisiones/interruptor-principal-criterios-del-excel.md`](../decisiones/interruptor-principal-criterios-del-excel.md).

### I-29 — Tamaños de interruptor por familia · `b9406ab`

La 240-6(a) mezcla los tamaños de centro de carga (15, 30, 35, 45…) con los de riel DIN (16, 32, 63).
David eligió tres opciones —**Centro de carga (NEMA)** por omisión, **Riel DIN (IEC)** y **NOM
completa**— en el campo «Interruptores» de la ficha. Con centro de carga, la air fryer de la prueba y
el principal quedan en **20 A**. Detalle en
[`../decisiones/serie-de-interruptores.md`](../decisiones/serie-de-interruptores.md).

**Tocó el motor copiado**, y se tiene que reportar en `PowerNode-DesignSuite` junto con M-02: la
excepción 240-4(b) («el siguiente valor estándar superior») se lee contra la lista completa de la
norma, no contra la serie. `ITablaProteccionEstandar` ganó `ValoresDeLaNorma` y `SiguienteDeLaNorma`,
con implementación por omisión igual a la de antes, y `SeleccionConductor` los usa. Sin eso, en riel
DIN se aceptaban 63 A sobre un 6 AWG de 55 A (`Serie_En240_4bManda_ElSiguienteDeLaNorma_NoElDeLaSerie`
falla sin el cambio).

### M-04 — La excepción 240-4(b) solo se concede en Alumbrado

Salió al escribir la prueba de I-29. `CalculadoraCircuitoDerivadoNoMotor` pasa
`permiteExcepcion2404b: d.TipoCarga == TipoCarga.Alumbrado`. La 240-4(b)(1) solo excluye el circuito
derivado que alimenta **más de un contacto** para equipo conectado con cordón; un circuito de
**Equipo** a un solo aparato sí califica. Hoy en Equipo el conductor siempre sube hasta cubrir la
protección: es conservador (más cobre, nunca menos), no inseguro. Es del motor de escritorio y se
reporta allá; aquí no se toca.

### I-31 — «Acometida» se cortaba · `b9406ab`

El mismo defecto que I-08: el selector de 118 px mostraba «Interruptor prin». Se vio al revisar el
campo nuevo «Interruptores», que tenía el mismo problema («Centro de carg»). Los dos van ahora en su
propio renglón, a todo lo ancho de la tarjeta.

La lista de la prueba ponía también **I-30** entre las decisiones; no lo es. Es un tooltip que
contradecía una decisión **ya confirmada** (`sin-piso-practico-de-calibre.md`), y se corrigió.

### I-30 — El tooltip de «Tipo» describía el piso práctico de calibre · `b37de00`

Decía «Decide el piso práctico de calibre (12 AWG en alumbrado, 10 en contactos) y el mínimo de
protección». El piso se quitó el 2026-09-22. Ahora dice lo que el tipo decide de verdad: el mínimo de
protección —15 A en alumbrado, 20 A en contactos, sin mínimo en equipo— y si aplica la excepción
240-4(b).

### Prueba pendiente del reporte, ya corrida

El mismo caso con microondas y air fryer como **no continuas** (210-19: continua es de 3 h o más, y
estos dos aparatos no lo son) da lo que el reporte calculó a mano: microondas 11.81 A, 15 A, 12 AWG
por caída, 2.24 %; air fryer 12.20 A, 15 A, 12 AWG por caída, 2.31 %. Y en el alimentador la fase C
ya no entra al 125 %: 12.20 A → principal de 15 A. Quedó como
`ElMotorDistingueContinuaDeNoContinua_EnElDerivadoYEnElAlimentador`.

---

## Auditoría de selección de conductor y protección (2026-09-23)

David acotó el alcance a seleccionar conductor y protección con la Tabla 310-15(b)(16) y pidió
revisar ocho temas contra la NOM. La auditoría completa, con enlaces a cada artículo y los casos
corridos en el motor, está en
[`../conocimiento/seleccion-conductor-y-proteccion.md`](../conocimiento/seleccion-conductor-y-proteccion.md).
Seis propuestas, aprobadas para hacerse una por una.

### I-32 — El aislamiento estaba fijo en THHN · `b1a84d6`

El motor sabe elegir la columna de la Tabla 310-15(b)(16) por aislamiento y lugar (Tabla
310-104(a), 110-14(c)), pero la web nunca se lo pasaba: siempre THHN, lugar seco. **Del lado
inseguro**: con 9 conductores agrupados (factor 0.7), 20 A continuos daban 10 AWG en THHN
(40 × 0.7 = 28 A ≥ 25 A), y en THW-LS ese mismo 10 AWG no alcanza (35 × 0.7 = 24.5 A < 25 A) — pide
8 AWG. Si en obra se instalaba THW-LS, el calibre impreso no cumplía.

Ahora «Condiciones de cálculo» tiene **Aislamiento** (THHN por omisión, THHW-LS, THW-LS, THWN,
THWN-2, XHHW-2, TW) y **Lugar** (seco, o húmedo o mojado), y se pasan al motor en los derivados y en
el alimentador. THHN en lugar mojado lo rechaza el motor y el renglón lo dice. Queda impreso en el
documento («Conductor: Cobre · THHN · lugar seco») y en la memoria, sección 2. THW no se ofrece: la
Tabla 310-104(a), como está leída, lo da solo para lugares mojados.

### I-33 — El porqué de la protección y del calibre no se veía, y la memoria no cuadraba · este commit

Dos cosas de la misma raíz (propuesta 2 de la auditoría):

1. **La sección 4 de la memoria estaba mal rotulada.** Decía «Icm = In / [(FT) × (FA) × hilos]»
   pero sustituía la **capacidad mínima** (con el 125 %), no In. Con 32 A continuos y 6 agrupados
   imprimía «= 50 A» y en la sección 5 un conductor de 40 A: los 50 A eran de la columna de 90 °C y
   los 40 A el tope de la terminal de 60 °C, y la memoria no lo decía. La cita de 310-15(b)(16) del
   motor llamaba «corriente de diseño» a esa misma capacidad mínima; ahora dice «capacidad mínima».
2. **En la captura no se veía el porqué**, solo In, protección y calibre.

Ahora `DesgloseDeSeleccion` (en `PowerNode.Web.Modelo`, sin decidir nada: relee lo que calculó el
motor) arma, paso por paso y con su artículo, la protección (In → capacidad mínima → tamaño estándar
de la familia elegida) y el conductor (columna del aislamiento × FT × FA, tope de la terminal,
ampacidad utilizable, y por qué subió si subió: 240-4, 240-4(d), 240-4(b), caída). Se ve en un
**tooltip sobre la protección y sobre el calibre** de cada renglón y del alimentador —sin columnas
nuevas— y es la sección 4 de la memoria.
