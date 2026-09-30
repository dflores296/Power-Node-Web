# El F.P. y la continuidad por omisión, según el subtipo de la carga

**PROPUESTA · Claude · 2026-09-30** — auditoría NOM del 2026-09-29, P2-4 y riesgo 3 (I-149, I-153).
No implementada: cambia [`factor-de-potencia-por-circuito.md`](factor-de-potencia-por-circuito.md),
**CONFIRMADA · David · 2026-09-23** («prellenado 0.9 para todo»). Falta que David decida.

## El problema

**F.P. (P2-4).** Toda carga nace con F.P. 0.90. En una carga resistiva capturada en W eso infla la
corriente un 11 %:

| Carga | Placa | Hoy (F.P. 0.90) | Con F.P. 1.00 |
|---|---|---|---|
| Calentador de agua | 4 500 W | 5 000 VA · 22.73 A a 220 V | 4 500 VA · 20.45 A |
| Secadora | 5 000 W | 5 556 VA | 5 000 VA |
| Estufa | 12 000 W | 13 333 VA · 60.6 A → 70 A, 4 AWG | 12 000 VA · 54.5 A → 60 A, 6 AWG |

La propia decisión de 2026-09-23 ya lo anota: la NOM calcula en VA, 220-54 y 220-55 dicen que **«los
kVA se deben considerar equivalentes a los kW»** para secadoras y aparatos de cocción, y suponer 0.9 en
una resistiva **subestima** la caída de tensión en calibres chicos (en 12 AWG, Ze = 6.04 Ω/km con 0.9
y 6.60 con 1.0).

**Continuidad (riesgo 3).** Las luminarias nacen «no continua». En comercio y oficina el alumbrado
casi siempre opera 3 h o más (Art. 100, «carga continua»), y sin el 125 % de 210-19(a)(1) y 210-20(a)
el circuito puede quedar un tamaño abajo. El mínimo de 220-12 fuera de vivienda ya entra como
continuo (M-14); lo capturado no.

## La propuesta

Que el valor **inicial** dependa del subtipo; el ingeniero lo cambia con el dato de placa, como hoy.

| Subtipo | F.P. inicial | Continua inicial | Por qué |
|---|---|---|---|
| Calentador de agua | 1.00 | Sí (ya) | Resistiva; continua por 422-13 |
| Cocción (estufa, horno, parrilla) | 1.00 | No | Resistiva; 220-55: kVA = kW |
| Secadora | 1.00 | No | 220-54: kVA = kW; la de placa si se captura |
| Calefacción por resistencia | 1.00 | Sí (ya) | Resistiva; continua por 424-3(b) |
| Otra carga, capturada en W | 1.00 | No | En W sin F.P. de placa, no hay de dónde suponer 0.9 |
| Luminarias, **fuera de vivienda** | 0.90 | **Sí** | Opera 3 h o más en comercio, oficina, escuela |
| Luminarias, en vivienda | 0.90 | No | Como hoy |
| Todo lo demás | 0.90 | Como hoy | Sin cambio |

- Solo el valor con que **nace** la línea: un archivo abre con lo que guardó.
- Luminarias LED o de descarga: el F.P. de su placa (220-18(b) pide la corriente, no los watts). No se
  propone otro valor inicial sin dato.

## Lo que se pierde

- El «0.9 para todo» es fácil de explicar y es la costumbre (tarifa de CFE). Con esta propuesta el
  valor inicial cambia según lo que se capture.
- Con «Continua» marcada por omisión fuera de vivienda, un alumbrado que de verdad no opera 3 h sale
  un tamaño arriba hasta que se desmarca: del lado seguro.

## Preguntas para David

1. ¿F.P. 1.00 por omisión en las resistivas de la tabla, o se queda 0.90 para todo?
2. ¿Luminarias continuas por omisión fuera de vivienda?
