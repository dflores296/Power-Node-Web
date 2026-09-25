# La marca en la web, y en qué se aparta del escritorio

## El logo es una carta de Smith

> **CONFIRMADA · David · 2026-09-25.** De tres niveles de detalle, eligió la variante **B («media»)**
> para todo: encabezado, pestaña, `.ico`, PNG y firma.

La carta se **dibuja con sus ecuaciones**, no se calca de ninguna imagen: es geometría y queda libre
de derechos de terceros (`tools/marca_carta_smith.py`). Todas las curvas son tangentes en (1, 0):

- **resistencia constante *r***: círculo con centro en (r/(1+r), 0) y radio 1/(1+r) — r = 0.5, 1, 2;
- **reactancia constante *x***: arco con centro en (1, 1/x) y radio 1/|x|, recortado al círculo
  unidad — x = ±0.5, ±1, ±2;
- el eje real, y **el punto azul al centro**: z = 1, la adaptación perfecta. Es el nodo.

Tinta `#101418`, azul del nodo `#0B6E99`, gris de apoyo `#5A6570` — los colores reales de la marca.

Antes el logo salía de `Recursos/Marca/` del repo de escritorio (`PowerNode-DesignSuite`): un
unifilar de **nodo, barra y tres derivaciones**. ⚠ **El escritorio sigue con el unifilar**; si se
quiere una sola marca, hay que llevar la carta allá.

## La diferencia deliberada: remates redondos

> **CONFIRMADA · David · 2026-09-22.** Al ver el icono en la pestaña dijo que le gustaba **más que
> el original**, «bordes redondeados, menos cuadrado», y pidió llevarlo al estilo general.

El escritorio dibuja el logo con `stroke-linecap="square"`. **Aquí es `round`** (con
`stroke-linejoin="round"`), en todos los archivos — también en la carta de Smith. La diferencia es de un atributo y cambia el
carácter entero: las líneas rematan en semicírculo en vez de en escuadra.

⚠ **El repo de escritorio sigue con la versión cuadrada.** Si se quiere una sola marca, hay que
propagar el cambio allá; desde aquí no se tocó ese repo.

## La escala de redondeo sale del logo

No es un valor por capricho: los remates del logo son semicírculos de la mitad del grosor del trazo,
y la interfaz aplica el mismo criterio — **radio proporcional al tamaño del elemento**. Antes había
valores sueltos de 3, 4 y 5 px que no respondían a nada.

| Variable | Valor | Dónde |
|---|---|---|
| `--radio-xs` | 6 px | Celdas editables de la cuadrícula, chips de cita |
| `--radio-sm` | 9 px | Campos y selectores del encabezado |
| `--radio-md` | 14 px | La tabla del cuadro de carga |
| `--radio-lg` | 18 px | El panel de la memoria |

## Dos trampas que ya costaron tiempo

**`border-collapse: collapse` ignora `border-radius`.** Una tabla con `collapse` sale con las
esquinas cuadradas por más radio que se le ponga. La del cuadro de carga usa
`border-collapse: separate; border-spacing: 0` más `overflow: hidden`, y sus líneas interiores las
dibuja el `border-bottom` de cada celda.

**Los iconos no se versionan con `?v=`.** Ver `../estado/HALLAZGOS.md` I-07: las query strings en
favicons se comportan distinto en cada navegador. El CSS sí lo lleva; los iconos se versionan
cambiándoles el nombre, o no se versionan.

## Tema oscuro

> **Pedido de David · 2026-09-25** (I-57). En la barra superior (I-58), segmentado como el de
> msa-toolkit: **Sistema | Claro | Oscuro**.

- `wwwroot/js/tema.js` resuelve la elección y escribe `<html data-tema="claro|oscuro">`: es lo único
  que lee la hoja de estilos. Sistema sigue a `prefers-color-scheme`, también si el sistema cambia
  con la página abierta. Claro y oscuro se recuerdan en `localStorage` (`powernode.tema`).
- Va en el `<head>` y sin `defer`, para que la página no destelle en claro mientras carga.
- Todos los colores de `app.css` son variables de `:root`; la paleta oscura se escribe **una sola vez**,
  en `@media screen { :root[data-tema="oscuro"] }`. **Impreso sale siempre en claro**: el bloque es
  solo de pantalla.
- Los colores de fase (A negro, B rojo, C azul, neutro blanco, tierra verde; 200-6, 250-119) no
  cambian. En oscuro, el negro de la fase A lleva un borde (`--fase-borde`) para no perderse.
- Un `<img>` no sabe del tema de la página: el logo va dos veces, con `solo-claro` y `solo-oscuro`, y
  la hoja **solo oculta** el que no toca (así cada imagen conserva su propio `display`). Al imprimir,
  la regla es `html:root .solo-oscuro`: con menos peso, `.doc-marca img { display: block }` le ganaba
  y salían las dos firmas.
- Tinta oscura del logo `#E8ECEF`; el azul del nodo es el mismo en los dos temas.

## La barra superior

> **Pedido de David · 2026-09-25** (I-58): «métete a nom y a msa-toolkit, conviértelo en una barra
> superior».

El mismo patrón que las otras dos herramientas: `header.top` del sitio de la NOM y
`header.toolbar` de msa-toolkit. A todo lo ancho, **fija arriba** (`position: sticky`), fondo de
papel y una línea abajo; 64 px de alto (56 hasta I-59).

- Izquierda: el icono (40 px desde I-59; era de 28), «Power Node» y, debajo, «NOM-001-SEDE-2012». Sin
  tooltip: David lo pidió quitar.
- Las páginas: **Captura · Cuadro de carga · Memoria de cálculo** (`NavLink`, con `aria-current`).
  La memoria tiene ruta propia, `/documento/memoria`; antes era una pestaña sin dirección.
- Derecha: en la captura, **Abrir** y **Guardar** (I-05); en el documento, **Imprimir / PDF**; el
  tema; y al final **GitHub ★ n**, como el botón de gitdiagram.com (I-61): lleva al repositorio, en otra
  pestaña, con sus estrellas. La cuenta sale de la API pública de GitHub (60 consultas por hora sin
  sesión, por dirección IP) y se guarda una hora en el navegador (`js/github.js`); si la consulta
  falla, el botón sale sin número. «17.1k» arriba de mil, como GitHub. La marca de GitHub es la de
  Octicons, para enlazar al repositorio como lo permiten sus lineamientos.
- En el celular (≤ 760 px) las páginas bajan a un segundo renglón, y el tema e Imprimir quedan como
  símbolos (◐ ☀ ☾, una carpeta, un disquete y una impresora) con el nombre para el lector de pantalla:
  con texto, la barra del documento se iba a tres renglones (I-60). A 400 px o menos, en la captura, el
  logo va sin «Power Node» junto a Abrir y Guardar; el nombre sigue en la pestaña. En el segundo
  renglón, las páginas con su nombre corto (Captura · Cuadro · Memoria) y GitHub a la derecha, solo con
  la marca y el número (I-61).
- `html { scroll-padding-top }`: Enter, las flechas y Alt+1…5 mueven el foco con `scrollIntoView`, y
  sin esto el campo quedaba debajo de la barra.
- Impreso no sale (`no-imprime`). Vive en `Layout/BarraSuperior.razor`, dentro de `MainLayout`.

## La carta que se dibuja

> **Pedido de David · 2026-09-25** (I-59). De cuatro propuestas (se dibuja con la descarga, el punto
> busca la adaptación, tres fases, latido del nodo) eligió **la primera**: en la pantalla de carga, y
> en la barra solo al pasar el cursor. Y el logo más grande: **80 px** en la carga (era 56) y **40 px**
> en la barra (era 28).

- `--p`, de 0 a 1, dibuja la carta: el borde avanza (`pathLength="100"`, `stroke-dasharray`), cada
  línea aparece en su umbral (`--t`, de .10 a .82) y el punto azul crece al final (de .9 a 1).
  `@property --p` (número, valor inicial 1): sin nadie que lo mueva, la carta está completa.
- **En la pantalla de carga, `--p` sigue el avance real.** Blazor lo publica en
  `<html style="--blazor-load-percentage: 42%">` por cada recurso que baja
  (`onDownloadResourceProgress` en `blazor.webassembly.js`); un script de `index.html` lo pasa a `--p`
  y al texto («… 42 %»). Si en 1.5 s no llega ningún avance, se da por descargado.
- **Se ve completa aunque la carga sea rápida** (I-60, David: «la carga es muy rápida y no muestra la
  animación»). La pantalla de carga es una **capa encima de `#app`**, no su contenido: Blazor ya no la
  borra al arrancar. El dibujo nunca adelanta al avance real, pero avanza a lo más a una velocidad
  (`DIBUJO_MS` = 1.6 s para la carta completa); cuando la app ya pintó y la carta está completa, espera
  0.35 s y se desvanece.
- **La trampa:** mientras arranca .NET el navegador se congela (~1.5 s en caché) y no pinta nada. Al
  volver, la carta brincaba de 42 % a 100 % porque el script compensaba el tiempo perdido. Cada cuadro
  avanza a lo más 1/30 s, así que después de la congelación sigue desde donde iba.
- **En la barra**, `@keyframes dibujar` anima `--p` de 0 a 1 en 1.2 s, una vez, al pasar el cursor o
  con el foco del teclado.
- Con «reducir movimiento» (`prefers-reduced-motion`) la carta queda quieta y completa.
- Para poder animarla, la carta va **en línea** (SVG dentro de la página, con `currentColor`), no en
  `<img>`: en `index.html` y en `Layout/BarraSuperior.razor`. Son copias de `powernode-icono.svg`; si
  cambia el logo, hay que cambiarlas también. Al ir en línea, ya no necesitan la versión oscura: toman
  la tinta del tema.

## Tipografía e iconos

> **CONFIRMADA · David · 2026-09-25** (I-62). En el canvas «Power Node — tipografía e iconos» se
> compararon la actual contra IBM Plex, Barlow, Atkinson Hyperlegible y Chivo, y tres estilos de
> iconos (trazo redondo, duotono, técnico). David eligió **Segoe UI** y **duotono**.

- **Segoe UI se queda**, la del sistema (`"Segoe UI", system-ui, -apple-system, sans-serif`): sin
  fuentes que descargar. En Mac sale San Francisco y en Android Roboto, a propósito. La firma del
  documento pedía IBM Plex Sans, que nunca se cargó; ahora pide la misma pila que la aplicación.
- **Iconos duotono** (`Layout/Icono.razor`): rejilla de 24, trazo de 1.5 px con remates redondos (el
  de la carta de Smith) y color del texto, sobre un relleno del acento al 16 % (26 % en oscuro); los
  avisos, en ámbar; en un botón primario, el relleno es del color del trazo. Doce: abrir, guardar,
  imprimir, agregar, quitar, deshacer, desglose (cerrado y abierto), aviso, y sistema, claro y oscuro
  para el tema en el celular. Se usan con `<Icono Nombre="guardar" />` (`Tamano` en px, 16 por
  omisión; 14 dentro del cuadro).
- **Reemplazaron a los caracteres que hacían de icono** (▸ ▾ ✕ ↺ + ◐ ☀ ☾): cada sistema los dibujaba
  con su fuente, a su tamaño, y ☀ a veces como emoji. Los avisos llevan su triángulo al frente.
- El GitHub de la barra es su marca, no un icono de este juego: se queda como está.
- **Tipos de carga** (I-63): alumbrado, contactos, equipo, motor y calefacción, en el resumen de carga
  (uno por renglón) y junto al selector «Tipo» del cuadro, solo en los renglones con carga. No en la
  puesta a tierra (ya lleva la muestra verde de la NOM) ni en el documento impreso, que se firma.

## El relieve

> **CONFIRMADA · David · 2026-09-25** (I-64): «lo siento muy plano». De tres puntos intermedios entre la
> app plana y gitdiagram (sombra suave, relieve marcado, superficies tintadas) eligió **B, relieve
> marcado**.

- Tarjetas (ficha, tarjetas, cuadro, memoria, documento en pantalla): borde de 1.5 px `--relieve-borde`
  y una sombra sólida desplazada 4 px, `--relieve-sombra`. Campos con borde de 1.5 px.
- Botones y el selector de tema: borde `--trazo-fuerte` y sombra sólida de 2 px; al presionarlos se
  hunden. El primario, con `--acento-hondo`. La barra superior, con una línea de 2 px abajo.
- Más contraste: fondo `#e6ebf1` (un tono abajo del papel), `--tenue` más oscuro, títulos de tarjeta en
  tinta y negrita, encabezados de tabla `#dfe6ee`. En oscuro, el fondo baja a `#0b0e12`.
- Solo en pantalla (`@media screen`): el documento impreso sigue plano.

## Qué archivo es cuál

| Archivo | Para qué |
|---|---|
| `marca/powernode-icono.svg` | El logo. La barra superior y la pantalla de carga llevan una copia en línea (ver arriba). Trazo normal: borde 3.5, líneas 1.8 (lienzo 64) |
| `marca/powernode-icono-oscuro.svg` | El mismo con tinta clara, para usarlo en `<img>` sobre fondo oscuro |
| `marca/powernode-icono-16.svg` | Trazo grueso (borde 5, líneas 2.6), para tamaños chicos. Fuente de los PNG chicos y del `.ico` |
| `marca/powernode-icono-mono.svg` | `currentColor`, para heredar el color del contexto |
| `marca/powernode-firma.svg`, `powernode-firma-oscura.svg` | Logo con el texto «Power Node · Design Suite», en Segoe UI — el del documento, en claro y en oscuro |
| `marca/powernode-carta-favicon.svg` | El de la pestaña: trazo grueso, con `prefers-color-scheme` adentro |
| `marca/powernode-carta.ico` | El `.ico` que declara `index.html`: PNG de 16, 32 y 48 adentro |
| `marca/powernode-carta-16.png`, `-32.png`, `-48.png` | Los tamaños chicos, del trazo grueso |
| `marca/powernode-carta-180.png`, `-192.png`, `-512.png` | iOS (180) y tamaños grandes, del trazo normal |
| `favicon.ico`, `favicon.png` (raíz) | Ver I-07: los que el navegador pide solo. El `.ico` es copia de `powernode-carta.ico`; `favicon.png` va sobre blanco |

**Por qué «carta» en el nombre de los iconos de la pestaña:** el navegador guarda el favicon por URL.
Con los nombres de antes (`powernode-favicon.svg`, `powernode-32.png`…) seguía mostrando el unifilar
aunque el archivo ya fuera la carta. Un icono se versiona cambiándole el nombre (I-07).

**Nada de esto se edita a mano; se genera:**

```bash
python3 tools/marca_carta_smith.py            # los SVG, con las ecuaciones
node tools/marca_png.mjs                      # los PNG, con Playwright (NODE_PATH si no es global)
python3 tools/marca_carta_smith.py --ico      # powernode-carta.ico y favicon.ico con los PNG de 16, 32 y 48
```
