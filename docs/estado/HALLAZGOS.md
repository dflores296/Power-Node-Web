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
| I-03 · El piso práctico de calibre no se aplicaba | P1 | **Cerrado** | este commit |
| P-02 · Pages quedó en «Deploy from a branch», sirve el README y no la app | P1 | **Cerrado** | David lo cambió a GitHub Actions |
| I-06 · Pantalla de carga: una bola negra, y el favicon de Blazor | P2 | **Cerrado** | este commit |
| P-03 · El navegador se quedaba con el CSS y los iconos viejos | P1 | **Cerrado** | este commit |
| I-07 · Faltaba `favicon.ico`: el navegador seguía con el icono de Blazor | P1 | **Cerrado** | este commit |
| I-04 · No hay resumen de carga ni balanceo por fase | P2 | Pendiente | — |
| I-05 · No se puede guardar ni abrir un proyecto | P1 | Pendiente | — |

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
