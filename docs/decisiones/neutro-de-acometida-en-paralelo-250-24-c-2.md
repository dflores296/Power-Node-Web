# Neutro de acometida en paralelo: el 12.5 % se toma del área total de las fases

**CONFIRMADA · David · 2026-10-03** — propuesta por Claude el mismo día. Sale de la verificación NOM, ronda 4
(R4-4, I-174), que pidió dejar la lectura actual y registrarla; David la confirmó en el hilo («confirmo»).

## La decisión

Con «Equipo de acometida», «Reducir el neutro — 220-61» y fases en paralelo de más de 557 mm² (1100 kcmil)
de cobre en total, **cada neutro en paralelo** es no menor que el 12.5 % del **área total** de las fases en
paralelo — 250-24(c)(1) y (c)(2). Es la lectura literal de la NOM:

> «El tamaño del conductor puesto a tierra en cada canalización deberá estar basado en el área total de los
> conductores de fase en paralelo en las canalizaciones» — 250-24(c)(2).

Con 708.57 A (2 × 900 kcmil por fase): 12.5 % de 1800 kcmil = 225 kcmil → **2 × 250 kcmil**.

La cita de 220-61 en la tarjeta y en la memoria lo dice: «cada uno en paralelo, con el área total de las
fases en paralelo, como lo dice la NOM — 250-24(c)(2)».

## Por qué

Es lo que dice el texto oficial (`dflores296/NOM-001-SEDE-2012`, `data/corpus.json`) y queda del lado
seguro: el neutro sale igual o mayor que con la otra lectura.

## Lo que se descartó

- **La lectura del NEC** (250-24(C)(2), de donde viene el artículo): la base es el área de las fases **en esa
  canalización** («in the raceway»). Con 708.57 A serían 900 kcmil por tubo → abajo de 1100 kcmil, Tabla
  250-66 → 2 × 2/0. Más chico y probablemente la intención original, pero no es lo que dice la NOM; si David
  la prefiere, el cambio es tomar el área de un juego en `PuestaTierraDeAcometida` para el neutro.

## Dónde está

`CuadroDeCarga.ReducirNeutro` (piso `acometida.PuenteDeUnion`), `CuadroDeCarga.CadaUnoEnParalelo`; hallazgos
I-166 e I-174. Relacionada: [`neutro-del-alimentador-por-220-61.md`](neutro-del-alimentador-por-220-61.md).
