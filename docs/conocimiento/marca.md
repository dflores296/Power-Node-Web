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

> **Pedido de David · 2026-09-25** (I-57). Botón en el encabezado de la captura y del documento:
> **Automático → Claro → Oscuro**.

- `wwwroot/js/tema.js` resuelve la elección y escribe `<html data-tema="claro|oscuro">`: es lo único
  que lee la hoja de estilos. Automático sigue a `prefers-color-scheme`, también si el sistema cambia
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

## Qué archivo es cuál

| Archivo | Para qué |
|---|---|
| `marca/powernode-icono.svg` | El del encabezado y la pantalla de carga. Trazo normal: borde 3.5, líneas 1.8 (lienzo 64) |
| `marca/powernode-icono-oscuro.svg` | El mismo con tinta clara, para el tema oscuro |
| `marca/powernode-icono-16.svg` | Trazo grueso (borde 5, líneas 2.6), para tamaños chicos. Fuente de los PNG chicos y del `.ico` |
| `marca/powernode-icono-mono.svg` | `currentColor`, para heredar el color del contexto |
| `marca/powernode-firma.svg`, `powernode-firma-oscura.svg` | Logo con el texto «Power Node · Design Suite» — el del documento, en claro y en oscuro |
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
