# Conductores por fase del alimentador: automático, o fijado por el proyectista

**PROPUESTA · Claude · 2026-10-03** — David aprobó la propuesta el 2026-10-03 («me gusta, impleméntalo»);
falta que la marque CONFIRMADA.

## La decisión

El alimentador tiene un selector **«Conductores por fase»**: **Automático** (por omisión) o un número de 1 a 6.

- **Automático**: el N más chico que cumple la ampacidad, 240-4 y la caída de tensión, como hasta ahora
  (M-19). El selector dice cuál da: «Automático · 2».
- **Fijado**: se calcula con ese N exacto. Si no cumple, el alimentador queda sin resultado y se dice por qué
  y qué números sí cumplen. Nunca se cambia el N por su cuenta.
- **Comparar**: una tabla con cada N que cumple (fase, tierra, ampacidad del juego, caída, canalización y
  cobre), para elegir con números y no a ojo. Se ofrece cuando hay algo que comparar: dos o más opciones, un
  conductor de 250 kcmil o más, o un N fijado.

Solo el alimentador. Los derivados siguen en automático: un derivado en paralelo es raro y ya avisa si
comparte canalización (310-10(h)(3)).

## Por qué

La NOM permite el paralelo de 1/0 AWG en adelante (310-10(h)(1)) pero no lo exige: con más de un N que
cumple, elegir es del proyectista. Con 708.57 A cumplen 2 × 900 kcmil y 6 × 2/0 AWG, y el segundo lleva 47 %
menos cobre a cambio de cuatro canalizaciones más. El programa tomaba siempre el primero.

## Lo que se descartó

- **Que un N que no cumple se suba solo**: escondería que lo que se capturó no sirve. Se prefirió el error,
  con las opciones que sí cumplen enfrente.
- **Un tope distinto de 6**: es criterio del programa, no de la norma; se quedó el del automático.
- **Elegir por costo**: sin precios no hay costo; el cobre en mm² sirve para comparar.

## Dónde está

`DatosDelTablero.ConductoresPorFaseAlimentador`, `CuadroDeCarga.OpcionesDeParalelo`,
`CuadroDeCarga.ConductoresPorFaseAutomatico`; la tarjeta del alimentador en `Captura.razor`. Hallazgo I-162.
