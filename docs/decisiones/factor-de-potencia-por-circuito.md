# El factor de potencia es de cada carga, no del tablero

**CONFIRMADA · David · 2026-09-23** — hallazgos I-26 e I-27 (`../estado/HALLAZGOS.md`).
Propuesta original: `PROPUESTA · Claude · 2026-09-23`, corregida en la conversación antes de confirmarse.

## La decisión

- **El tablero ya no tiene factor de potencia.** Se quitó el campo «F.P.» de la ficha.
- **Cada circuito lleva el suyo**, prellenado en **0.9 para todos** (palabras de David: *«prellenado
  0.9 para todo»*). Es un valor supuesto; se cambia con el dato de placa.
- **El del alimentador no se captura: resulta de sus cargas.** Se combinan las de la fase que
  gobierna (la más cargada), porque es la corriente de esa fase la que entra a su caída de tensión.
- **El «Total (kW)» es la suma de la potencia activa de cada circuito**, y el resumen enseña el
  **F.P. resultante** del tablero.

## Por qué: lo que dice la NOM

La pregunta de David fue la correcta: *«un tablero no tiene FP, las cargas sí»*. El texto de la
NOM-001-SEDE-2012 lo confirma:

| Dónde | Qué dice | Consecuencia |
|---|---|---|
| 220-12 | Las cargas unitarias de alumbrado «se basan en… un factor de potencia del 100 por ciento». | La norma calcula en VA; no supone 0.9. |
| 220-14(a) | Un aparato «se debe calcular con base en la corriente del aparato». | Protección y calibre salen de A o VA de placa. |
| 220-18(b) | Alumbrado con balastro o LED: «con base en el valor nominal de corriente… y no en el total de watts». | La norma evita convertir desde W. |
| 220-14(c), 220-18(a) | Motores y aire acondicionado por los Art. 430 y 440. | Corriente de tablas o de placa; el F.P. no entra. |
| 220-54, 220-55 | Secadoras y estufas: «los kVA se deben considerar equivalentes a los kW». | Resistivas: F.P. = 1. |
| Tabla 9, nota 2 | La impedancia eficaz usa «el ángulo del factor de potencia **del circuito**»; para un FP distinto de 0.85, Ze = R × FP + XL × sen(arccos FP). | El F.P. es de cada circuito. |

**La NOM no fija 0.9 en ningún lado.** Es costumbre de diseño; hasta donde se sabe viene de las
tarifas de CFE (recargo por F.P. menor a 0.9), no de esta norma.

## Qué mueve y qué no

- **Mueve:** la conversión de W a VA (`ConsumoDePlaca.AVoltAmperes`) y la caída de tensión.
- **No mueve la protección ni el calibre por ampacidad** de una carga capturada en VA o en A.
- **El alimentador se sigue dimensionando con la suma de VA** (la fase más cargada), como la NOM:
  sumar VA aritméticamente es igual o mayor que la suma vectorial, así que queda del lado seguro.

## Cómo se combinan

Los W se suman directo y los VAR también; los VA **no**. F.P. = P / √(P² + Q²). No es el promedio
de los F.P.: 1000 VA a 1.0 más 1000 VA a 0.8 dan **0.9487**, no 0.9
(`FactorPotenciaCombinado.De`).

## Lo que salió al probarlo

**Suponer 0.9 en una carga resistiva subestimaba la caída de tensión, no la sobrestimaba.** En
calibres chicos manda la resistencia: en 12 AWG, Ze = 6.04 Ω/km con F.P. 0.9 y 6.60 con 1.0. La air
fryer de la prueba pasa de 2.31 % a 2.54 %. El reporte original decía que el error era «del lado
conservador»; no lo era.
