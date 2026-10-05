# Rapidez de la captura: recalcular y dibujar solo lo que cambió

**PROPUESTA · Claude · 2026-10-03** — hallazgo **I-188** (auditoría de motores del 2026-10-03, AM-10: la
página se congeló cuatro veces, más de 45 s, al capturar varios campos seguidos). David contestó el
2026-10-05 («sí, las dos fases juntas; por ahora»): implementada en `02a71ca` (fase 1), `71ecdab` (fase 2) y
`b49763f` (lo del navegador, que salió al medir) — ver [Lo implementado](#lo-implementado-2026-10-05). No cambia
ningún número: es cómo se dibuja la pantalla.

## El problema

Cada cambio en la captura —una longitud, una carga, la temperatura— hace dos cosas completas:

1. **Recalcula el tablero entero** (`CuadroDeCarga.Recalcular`): los 42 espacios, el alimentador, las
   canalizaciones, el resumen.
2. **Vuelve a dibujar la página entera**: Blazor construye otra vez los ~6 600 elementos de `Captura.razor`
   (2 455 líneas, un solo componente), los compara con los anteriores y el navegador aplica la diferencia.

En esta máquina eso tarda ~240 ms. La auditoría corrió en un equipo que tardó de 40 s a 3 min en cargar la
aplicación (aquí, 2.2 s): con ese mismo factor, cada cambio son de 5 a 20 s, y cuatro o cinco seguidos pasan de
45 s. No hay fuga: 16 cambios seguidos tardan lo mismo cada uno.

## Dónde se va el tiempo

Medido con Playwright sobre la publicación en Release (la misma que GitHub Pages), con un tablero de 42
espacios y 26 circuitos, cambiando un campo de la ficha. Para separar las partes se apagó la tabla de
circuitos y el gabinete en una compilación de prueba (no publicada).

| Parte | Tiempo | Qué es |
|---|---|---|
| Recálculo | ~50 ms | El alimentador, 20–40 ms (se calcula 7 veces: el resultado y la comparación de 1 a 6 conductores por fase, I-162); canalizaciones, 8–19 ms; los circuitos, 4–20 ms (ya se recuerdan si no cambian, `Recordado`) |
| Dibujo en .NET de la tabla de circuitos | ~33 ms | 42 renglones con ~25 celdas, sus listas y sus textos de ayuda |
| Dibujo en .NET del gabinete | ~23 ms | `InteriorDelGabinete`: el SVG de las barras e interruptores |
| Dibujo en .NET de lo demás | ~32 ms | Ficha, canalizaciones, resumen, alimentador, avisos |
| El navegador aplicando la diferencia | ~100 ms | Sobre todo de la tabla y el gabinete: sin ellos, baja a ~30 ms |

Sin la tabla y sin el gabinete, el mismo cambio tarda ~115 ms en lugar de ~240 ms: **la mitad del tiempo es
volver a dibujar 42 renglones y un gabinete que casi nunca cambian**. Cambiar la longitud del circuito 5 solo
mueve el renglón 5, el alimentador si la caída lo pide, y el resumen.

## La propuesta

### Fase 1 — lo que no toca la pantalla (bajo riesgo)

- **Recordar el alimentador** como ya se recuerdan los circuitos: si su entrada (corrientes por fase, longitud,
  canalización, condiciones) no cambió, no se vuelve a calcular, ni sus seis opciones. Cambiar la longitud de un
  derivado no toca el alimentador: −20 a −40 ms en ese caso.
- **El gabinete se dibuja solo si cambió lo que enseña**: `InteriorDelGabinete` con `ShouldRender` y una firma
  de lo que dibuja (por espacio: polos, amperes, descripción, fases; el principal y su montaje). Una longitud o
  una temperatura no lo mueven: −23 ms de .NET y su parte del navegador.

### Fase 2 — un componente por renglón (el cambio estructural)

Hoy la tabla es un `@foreach` dentro de `Captura.razor`. La propuesta es sacar cada renglón (con su desplegable)
a un componente, `RenglonDelCircuito.razor`, que recibe su circuito y **decide si se vuelve a dibujar**:

- Cada renglón lleva una **firma**: todo lo que enseña, en un texto — lo capturado (descripción, polos, longitud,
  canalización, cargas del desplegable), lo calculado (el `Resultado`, que ya es otro objeto cuando cambia), sus
  avisos, si el desplegable está abierto, si la protección está fijada.
- En cada dibujo, el renglón compara su firma con la anterior. Igual: Blazor se lo salta, y el navegador no toca
  esos elementos. Distinta: se dibuja como hoy.
- El marcado de cada celda no cambia: los mismos `data-col`, `aria-label`, `data-espacio` y `data-arrastre`.
  `teclado.js` (I-55, I-56) y `arrastre.js` (I-69) siguen funcionando porque no ven la diferencia.
- Las acciones del renglón (cambiar polos, abrir el desplegable, quitar una carga, enfocar) siguen siendo de la
  página: el renglón las llama, no las duplica.

Con las dos fases, un cambio en un renglón debería bajar de ~240 ms a ~90–120 ms aquí. Un cambio que mueve todo
(la tensión, la temperatura, la serie de interruptores) sigue dibujando todo, como hoy: no se pierde nada, solo
no se gana.

### El riesgo, y cómo se cuida

El riesgo es el mismo que I-184: **una pantalla que no enseña lo que el cálculo dice**. Si algo que el renglón
enseña no entra en su firma, ese renglón se queda con el valor viejo. Para evitarlo:

1. La firma se arma en un solo lugar, junto al marcado, y una prueba recorre lo que enseña el renglón.
2. Antes y después, la misma captura en el navegador debe dar **la misma página**: se compara el HTML de la tabla
   completa (sin la firma) después de una serie de cambios (los de las auditorías: motor de 100 HP con la
   Excepción 2, quitar la lavadora, mover circuitos, abrir y cerrar desplegables, cambiar la tensión).
3. Se mide antes y después con el mismo tablero y el mismo guion.

### Lo que no se propone ahora

| Opción | Por qué no, por ahora |
|---|---|
| **Juntar los cambios seguidos**: recalcular y dibujar una vez al final de una ráfaga | Ayuda justo en el caso de la auditoría (varios cambios que se encolan), pero el teclado (Enter, flechas, Esc deshace) supone que el dibujo llega con cada evento. Si después de las fases 1 y 2 todavía se congela en un equipo lento, se revisa. |
| **Compilar AOT** | .NET correría de 3 a 10 veces más rápido sin tocar código, pero la descarga crece (choca con I-152) y la publicación tarda más. Es una decisión aparte, con su propia medición. |

## Preguntas para David

1. ¿Fase 1 y fase 2 juntas, o la fase 1 primero y medir? **Recomendación: juntas**, en una sola rama, cada fase
   en su commit y medida por separado.
2. ¿Lo de juntar ráfagas y lo de AOT se quedan para después, como dice arriba? **Recomendación: sí.**

## Respuestas de David (2026-10-05)

«sí, las dos fases juntas; por ahora»: las dos fases en una rama, cada una en su commit; juntar ráfagas y AOT
se quedan para después.

## Lo implementado (2026-10-05)

| Commit | Qué |
|---|---|
| `02a71ca` | Fase 1. `CuadroDeCarga.AlimentadorRecordado`: el cálculo del alimentador se recuerda por su entrada, como los derivados; la lista de corrientes por fase se compara por contenido. `InteriorDelGabinete.ShouldRender` con una firma de todo lo que dibuja. |
| `71ecdab` | Fase 2. `RenglonMemorizado` envuelve el marcado de cada circuito sin cambiarlo; `FirmaDeDibujo` (en el modelo) lleva todas las propiedades del circuito y de sus líneas, y los datos del tablero salvo la identificación. `FirmaDeDibujoTests` cambia cada propiedad por reflexión y exige que la firma cambie; se probó quitando propiedades a propósito. Con el desplegable abierto, el renglón se dibuja siempre. |
| `b49763f` | Lo del navegador (abajo): `--alto-ayuda` solo si cambió y registrada sin herencia; sin transición en los campos. |

**Lo que salió al medir.** Con la fase 2, al cambiar una longitud se dibuja 1 renglón de 42 y el dibujo en .NET
baja de ~96 a ~59 ms (las firmas cuestan ~11 ms), pero el total casi no se movía. Una traza de Chrome mostró que
el navegador gastaba más que .NET, y no por el cálculo:

- **Un recálculo de estilos de toda la página, ~60 ms, en cada cambio de campo.** `teclado.js` escribía
  `--alto-ayuda` en el `<body>` cada vez que mostraba la ayuda; una variable de CSS se hereda, así que la página
  entera (~6 600 elementos) recalculaba su estilo. Pasaba también al moverse con Tab sin cambiar nada.
- **La página entera repintada unas 10 veces por cambio, ~70 ms.** La transición de 120 ms del borde de los
  campos (I-176) pinta en cada cuadro, y la página es una sola capa de ~5 000 px: cada cuadro la repintaba toda.

Corregidos los dos, por un cambio de longitud: estilos ~1 ms, un pintado de ~7 ms.

**Medido** con el mismo tablero (42 espacios, 26 circuitos) y el mismo guion, publicación en Release, mediana de 9
(la ráfaga, una corrida):

| Cambio | Antes (`39a28d2`) | Fase 1 | Fases 1 y 2 | Con lo del navegador |
|---|---|---|---|---|
| Longitud de un circuito | 329 ms | 272 ms | 272 ms | **158 ms** |
| Temperatura ambiente (todo se recalcula) | 274 ms | 220 ms | 287 ms | **192 ms** |
| Abrir o cerrar un desplegable | 186 ms | 172 ms | 185 ms | **112 ms** |
| Carga de una línea del desplegable | 277 ms | 251 ms | 299 ms | **181 ms** |
| Ráfaga de 8 longitudes | 2.9 s | 3.0 s | 3.1 s | **1.3 s** |

Las variaciones de ±30 ms entre corridas son ruido de la máquina; lo que se mantiene es la última columna, cerca
de la mitad. Lo que queda es de .NET: ~55 ms de recálculo y ~60 ms de dibujo por cambio. Bajarlo más es juntar
ráfagas o AOT, que se quedaron para después.

**Lo que se ve, igual.** Un guion de Playwright guarda el HTML de la captura y lo que enseña cada campo después de
32 pasos (abrir un archivo, longitud, descripción, temperatura, desplegables, motor con la Excepción 2 marcada y
desmarcada, quitar líneas, renombrar y cambiar canalización, optimizar y deshacer, arrastrar un circuito, serie,
tensión, fases, «Nuevo» y volver a abrir el mismo archivo, editar después de reabrir). En la versión anterior y en
esta: idénticos. El teclado (Enter, flechas, Esc, Ctrl+Enter, Alt+2) y la barra de ayuda, iguales.

