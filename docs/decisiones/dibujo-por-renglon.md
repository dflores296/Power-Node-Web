# Rapidez de la captura: recalcular y dibujar solo lo que cambió

**PROPUESTA · Claude · 2026-10-03** — hallazgo **I-188** (auditoría de motores del 2026-10-03, AM-10: la
página se congeló cuatro veces, más de 45 s, al capturar varios campos seguidos). No implementada: falta que
David decida. No cambia ningún número: es cómo se dibuja la pantalla.

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
