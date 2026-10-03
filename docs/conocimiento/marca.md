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

> **Reemplazada en pantalla · David · 2026-10-03** por la de Linear: 4, 6 y 12 px (ver «La piel
> Linear»). La tabla de abajo sigue siendo la del impreso.

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

> **La letra, reemplazada · David · 2026-10-03:** Inter, dentro de la aplicación (ver «La piel
> Linear»); Segoe UI queda para el impreso. Los iconos duotono se quedan.

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
- **Campos de la ficha** (I-178, pedido de David, 2026-10-03): uno por campo de Identificación, Sistema,
  Gabinete y Condiciones de cálculo, a 14 px y del color del rótulo, en un `span.rotulo` junto al texto
  (la etiqueta es una rejilla de dos columnas: un tercer hijo la rompía). 30 nuevos; «Tablero» reusa el
  del tipo de carga. Dibujados aquí con el mismo trazo y relleno, no copiados de ningún juego:
  - Identificación: etiqueta (clave), alfiler (ubicación), portapapeles (proyecto), persona (cliente).
  - Sistema: rayo (tensión), **fasores a 120°** (fases), corte de un cable con sus conductores (hilos),
    senoide (frecuencia).
  - Gabinete: espacios en dos columnas; flecha que entra (acometida); **interruptor de tres polos con la
    manija común** (montaje del principal); el principal ocupando espacios; caja contra el muro (montaje);
    tres barras; escudo con gota (gabinete NEMA: el grado de protección); medidor de aguja (capacidad de
    barra); interruptor (familia); casa (inmueble); área con cotas; tabla (uso, Tabla 220-12); **medidor**
    (equipo de acometida).
  - Condiciones: el cable con la cara del corte del aislamiento y el cobre saliendo, lleno (I-180: el
    primero, una cápsula con una varilla delgada, parecía la llave del SIM; David eligió la F de seis); corte del aislamiento; terminal de ojillo; gota (lugar);
    termómetro; onda con armónicas (carga no lineal); la caída desde el punto de derivación y desde la
    barra (e% máx.); Ø (diámetro del fabricante).

- **El conductor visto de frente** (I-181, pedido de David, 2026-10-03): en el alimentador, antes de «Fase»,
  «Neutro» y «Puesta a tierra», en lugar de las cajitas de color. No es del juego duotono: es **sólido**, a
  26 px (`Layout/CorteDeConductor.razor`). El forro con el color de la NOM —A negro, B rojo, C azul; neutro
  blanco, 200-6; tierra verde, 250-119— y adentro 7 hilos (1 + 6), como en la Tabla 8; las fases, en un haz
  montado. Los hilos van en **aluminio** si el conductor lo es, y la tierra va **sin forro** si la
  canalización del alimentador la lleva desnuda. El forro delgado, como un THHN: grueso, a 22 px, el cobre se
  veía como una mancha.

## El relieve

> **Reemplazado · David · 2026-10-03** por las líneas finas de «La piel Linear». Se queda aquí como
> historia: el bloque ya no está en `app.css`.

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
- **I-65:** las líneas internas de las tablas más oscuras (`--linea` #c2cbd6, `--linea-suave` #d8dfe7):
  junto al borde de las tarjetas se veían tibias. En oscuro, como gitdiagram: fondo #161b21, tarjetas
  #1f262d, bordes y sombras negros, campos un tono más hundidos. Antes la sombra casi negra caía sobre
  un fondo casi negro y el relieve no se veía.

## La piel Linear

> **CONFIRMADA · David · 2026-10-03** (I-176). Pidió llevar la interfaz al `DESIGN.md` de Linear que
> entregó en la sesión («midnight precision instrument»), con la skill
> [ui-ux-pro-max](https://github.com/nextlevelbuilder/ui-ux-pro-max-skill) como referencia y **sin tocar
> funcionalidad**. De cuatro preguntas eligió: **los dos temas** (no solo el oscuro), **lima** para el
> botón primario y la página activa, **líneas finas** en vez del relieve de I-64 e **Inter dentro de la
> aplicación** en vez de Segoe UI (I-62).

Todo es de pantalla. **El impreso no cambia**: los valores de `:root` se quedan como estaban y son los
del papel; la piel los reemplaza en `@media screen`. Se comprobó pixel por pixel con Playwright (cuadro,
memoria y guía, en claro y en oscuro).

- **Colores** (junto a `:root` en `app.css`). Oscuro, los de Linear: lienzo `#08090a` (Void), tarjeta
  `#0f1011` (Carbon), encabezados `#161718` (Obsidian), líneas `#23252a` (Graphite); texto `#d0d6e0`
  (Mist), rótulos `#8a8f98` (Fog), vacío `#62666d` (Ash). Claro, la misma lógica sobre papel: lienzo
  `#f5f6f7`, tarjeta blanca, líneas `#dfe1e5`, rótulos `#62666d` (5.8:1). Tokens nuevos: `--tinta-fuerte`
  (títulos), `--linea-fuerte`, `--campo-*`, `--foco-*`, `--boton-*`, `--barra-fondo`, `--sombra-flotante`.
- **Lima** `#e4f222`, texto `#08090a` encima (16:1): `--accion`. Solo en `.boton.primario` («Imprimir /
  PDF») y en la página activa de la barra. En claro lleva orilla `#b9c40f`: el lima sobre blanco no tiene
  borde. **El azul de la marca se queda** en el logo y en lo informativo (enlaces, el principal, lo
  fijado, la clase del circuito, el relleno de los iconos). Las fases no cambian: son de la NOM.
- **Inter 4.1**, `wwwroot/fuentes/inter-4.1.woff2` (76 KB, OFL en `fuentes/OFL.txt`), recortada a lo
  que escribe la aplicación con `tools/fuente_inter.py`: tamaño óptico fijo en 14, pesos de 300 a 700.
  Variantes `cv01` y `ss03`, como Linear. **Sin el cero cruzado (`zero`)**, aunque Linear lo usa:
  «1/0 AWG» salía «1/Ø», y la aplicación escribe Ø para el diámetro del conductor. Separación -0.01em;
  pesos de 400 a 590 (Linear no usa 700). Se pide con `preload` en `index.html`.
- **Superficies**: tarjetas, cuadro, desglose y campos con línea de 1 px, sin sombra; en oscuro, un brillo
  de 1 px arriba (`--brillo-tarjeta`). Radios 4 (chips), 6 (campos y botones) y 12 (tarjetas): «12 es el
  más grande». Lo que flota (avisos, confirmación, ayuda del teclado, el fantasma del arrastre) sí lleva
  sombra.
- **Botones fantasma**: sin relleno, línea gris, peso 510. La barra superior y la de ayuda, translúcidas
  (`backdrop-filter`). Los títulos de tarjeta, en minúsculas y en tinta; las mayúsculas quedan para
  encabezados de tabla y subtítulos.
- **El foco** del teclado, gris (tinta en claro, Mist en oscuro), ya no del azul. El campo enfocado
  marca su línea, como en Linear.

**Dos trampas que costaron tiempo:**

- `input:not([type="checkbox"])` vale (0,2,1) y le ganaba a `input.desc` e `input.va` (0,1,1): las
  celdas de la cuadrícula, que nacen invisibles, salían con caja. El `:not()` va dentro de `:where()`.
- En el Chromium sin pantalla del contenedor, Inter a 11 px salía con huecos («Aco metida»): es el
  *hinting* de FreeType, que redondea los avances; pasa igual con la Inter original. Las capturas de
  Playwright se toman con `--font-render-hinting=none`, que es como dibujan Windows y macOS.

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
| `fuentes/inter-4.1.woff2`, `fuentes/OFL.txt` | La letra de la pantalla y su licencia. Se genera con `tools/fuente_inter.py`; si cambia, cambia de nombre (como los iconos, I-07) |
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
