# La marca en la web, y en qué se aparta del escritorio

El logo sale de `Recursos/Marca/` del repo de escritorio (`PowerNode-DesignSuite`): un unifilar de
**nodo, barra y tres derivaciones**. Tinta `#101418`, azul del nodo `#0B6E99`, gris de apoyo
`#5A6570` — los tres son los colores reales de la marca, no aproximaciones, y la interfaz se alineó
a ellos.

## La diferencia deliberada: remates redondos

> **CONFIRMADA · David · 2026-09-22.** Al ver el icono en la pestaña dijo que le gustaba **más que
> el original**, «bordes redondeados, menos cuadrado», y pidió llevarlo al estilo general.

El escritorio dibuja el logo con `stroke-linecap="square"`. **Aquí es `round`** (con
`stroke-linejoin="round"`), en las cuatro variantes. La diferencia es de un atributo y cambia el
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

## Qué archivo es cuál

| Archivo | Para qué |
|---|---|
| `marca/powernode-icono.svg` | El del encabezado de la aplicación |
| `marca/powernode-icono-16.svg` | Trazo grueso, para tamaños chicos. Es la fuente de los PNG y del `.ico` |
| `marca/powernode-icono-mono.svg` | `currentColor`, para heredar el color del contexto |
| `marca/powernode-firma.svg` | Logo con el texto «Power Node · Design Suite» |
| `marca/powernode-favicon.svg` | El del navegador, con `prefers-color-scheme` adentro |
| `favicon.ico`, `favicon.png` (raíz) | Ver I-07: los que el navegador pide solo |

**Los PNG y el `.ico` se generan**, no se editan: salen de `powernode-icono-16.svg` renderizado con
Playwright. Si cambia el logo, hay que regenerarlos.
