# Factor de potencia por circuito, heredado del tablero

**PROPUESTA · Claude · 2026-09-23** — hallazgos I-26 e I-27 (`../estado/HALLAZGOS.md`).

## La decisión que se propone

`CircuitoDelCuadro` lleva su propio `FactorPotencia`, **vacío por omisión**. Mientras esté vacío, el
circuito usa el del tablero (`DatosDelTablero.FactorPotencia`), así que nada de lo ya capturado
cambia. Cuando se captura uno, ese es el que recibe el motor para ese circuito, en los dos lugares
donde hoy entra el del tablero:

1. **La conversión W → VA** (`ConsumoDePlaca.AVoltAmperes`, ya en uso desde I-25).
2. **La caída de tensión del derivado** (`DatosEntradaCircuitoDerivadoNoMotor.FactorPotencia`).

Y el renglón «Total (kW)» del resumen (I-27) deja de ser `InstaladaVA × FP del tablero`: pasa a ser
**la suma de la potencia activa de cada circuito** —`VA × FP del circuito`, que para un renglón
capturado en W regresa exactamente los W de la placa—.

## Por qué

- **Un refrigerador y una air fryer no tienen el mismo FP.** En la prueba del 2026-09-22 el
  refrigerador se capturó a 750 VA suponiendo FP 0.8 y la air fryer, que es resistiva (≈ 1), se
  calculó con 0.9. Con un solo FP, la conversión W → VA que trajo I-25 hereda el mismo error.
- **La caída de tensión se calcula con cos θ del tablero.** El error es chico y del lado
  conservador, pero es el mismo problema de fondo.
- **El «Total (kW)» del caso daba 3.42 kW** (3800 VA × 0.9) cuando la suma real es
  600 + 1500 + 1550 = **3.65 kW**.
- **El escritorio ya lo tiene por circuito.** No es una regla nueva del motor: es darle a la web el
  dato que el motor ya recibe por circuito.

## Qué no cambia

- **El alimentador sigue con el FP del tablero** para su caída de tensión. Un FP ponderado del
  alimentador es otra decisión (¿ponderado por VA? ¿por la fase que gobierna?) y no hace falta para
  cerrar I-26.
- **El 220-40 y el 215-3 no se tocan**: se dimensiona en VA, no en W.

## Qué hace falta para cerrarla

David confirma o descarta. Si se confirma:

- Columna «F.P.» en el cuadro de captura, vacía por omisión, que muestre el heredado en gris.
- La memoria de cada circuito imprime el FP con el que se calculó (ya lo hace: hoy imprime el del
  tablero).
- Prueba de regresión con el caso de los tres aparatos: refrigerador 600 W a FP 0.8 → 750 VA; air
  fryer 1550 W a FP 1.0 → 1550 VA; «Total (kW)» = 3.65.
