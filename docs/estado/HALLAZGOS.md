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
