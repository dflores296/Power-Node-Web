# Tamaños de interruptor: centro de carga, riel DIN o la NOM completa

**CONFIRMADA · David · 2026-09-23** — hallazgo I-29 (`../estado/HALLAZGOS.md`).
Propuesta original: `PROPUESTA · Claude · 2026-09-23` («NOM completa / sin valores IEC»); David pidió
nombrar la distinción real —centro de carga contra riel DIN— y de ahí salió una tercera opción.

## El problema

La lista de valores normalizados de la 240-6(a) de la NOM-001-SEDE-2012 **mezcla dos familias**:

> 15, **16**, 20, 25, 30, **32**, 35, 40, 45, 50, 60, **63**, 70, 80, 90, 100, 110, 125, 150…

| | Centro de carga (enchufable, NEMA) | Riel DIN (modular, IEC) |
|---|---|---|
| Ejemplos | QO, NQ, QP, BR | Acti9, S200, 5SL |
| Origen | UL 489 | IEC 60898 |
| Tamaños | 15, 20, 25, 30, 35, 40, 45, 50, 60, 70… | 6, 10, 13, 16, 20, 25, 32, 40, 50, 63, 80, 100, 125 |

Con la lista completa, la air fryer de la prueba (15.25 A) salía en 16 A, que no existe en un QO/NQ.
Y 15, 30, 35 o 45 A no existen en riel DIN, que ya se usa en México con otras marcas.

## La decisión

Un dato del tablero, **«Interruptores»**, con tres opciones:

| Opción | Lista | Air fryer |
|---|---|---|
| **Centro de carga (NEMA)** — *por omisión* | La 240-6(a) sin 16, 32 ni 63 A | 20 A |
| **Riel DIN (IEC)** | Solo 16, 20, 25, 32, 40, 50, 63, 80, 100 y 125 A | 16 A |
| **NOM completa (centro de carga y riel DIN)** | La 240-6(a) tal cual | 16 A |

Aplica a los derivados y al principal. La memoria dice qué lista se usó.

**Por omisión, centro de carga** (David): es lo que más se instala. Consecuencia: lo que antes salía
en 16, 32 o 63 A ahora sale en 20, 35 o 70.

## Por qué no rompe `sin-catalogo-square-d.md`

No es catálogo: no dice qué modelos existen ni de qué marca. Son subconjuntos de la lista de la
propia norma, y la regla para elegir sigue siendo la del 240-6(a).

## Tres cosas que hubo que cuidar

1. **La excepción 240-4(b) se sigue leyendo contra la NOM completa.** Dice «el siguiente valor
   nominal **estándar** superior», o sea el de la norma. Si se leyera contra la serie, en riel DIN
   se aceptaría 63 A sobre un conductor de 55 A, cuando la NOM solo deja subir a 60. Para eso hubo
   que tocar el motor copiado: `ITablaProteccionEstandar` trae `ValoresDeLaNorma` y
   `SiguienteDeLaNorma` (por omisión, la misma lista), y `SeleccionConductor` los usa. Hay prueba que
   falla sin el cambio.
2. **En riel DIN, 14 AWG deja de alcanzar para alumbrado**: el más chico es 16 A y 240-4(d) no
   permite proteger un 14 AWG con más de 15 A. El motor sube solo a 12 AWG.
3. **Riel DIN se acaba en 125 A.** Arriba de eso se toma el siguiente de la NOM y se avisa, porque
   ese interruptor ya no es de riel DIN.
