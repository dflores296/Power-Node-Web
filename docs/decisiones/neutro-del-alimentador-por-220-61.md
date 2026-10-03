# Neutro del alimentador: reducido a su carga de desbalance, como opción

**CONFIRMADA · David · 2026-10-03** — propuesta por Claude el mismo día. David aceptó la opción en la ronda 2
(I-161), decidió el área del juego en paralelo (I-165) y confirmó este registro en el hilo de la revisión de
cabos sueltos («si confirmo todo»).

## La decisión

Una casilla en la tarjeta del alimentador, **«Reducir el neutro a su carga de desbalance — 220-61»**, apagada
por omisión. Sin marcar, el neutro es del calibre de la fase (del lado seguro). Marcada:

- **La carga del neutro** es la mayor carga neta entre el neutro y una fase — 220-61(a). Lo que pase de 200 A,
  al 70 % — 220-61(b)(2). Los circuitos de 2 polos con neutro en 3F-4H no se reducen — 220-61(c)(1). No se
  aplica la reducción de estufas y secadoras de vivienda de 220-61(b)(1).
- **No se ofrece** en 1F-2H, en 2 fases + neutro de estrella (220-61(c)(1)) ni con «Carga no lineal»
  (220-61(c)(2), aplicado a todo el neutro, del lado seguro).
- **Pisos del calibre**, el mayor de:
  - la ampacidad para esa carga, con la columna y los factores de la fase;
  - la tierra de equipos de 250-122 — 215-2(a)(2). **En paralelo, con el área de los N neutros**, porque
    215-2(a)(2) excluye 250-122(f) para los conductores puestos a tierra en paralelo; cada uno de 1/0 AWG o
    mayor — 310-10(h)(1);
  - en un **equipo de acometida**, la Tabla 250-66 y, con más de 1100 kcmil de cobre (1750 de aluminio) en
    las fases, el 12.5 % de su área — 250-24(c)(1); en paralelo, cada neutro — 250-24(c)(2).
  - Nunca mayor que la fase.
- **La caída de tensión** se calcula con la R y X del neutro reducido (Tabla 9); si así pasa del límite, el
  neutro sube hasta que no pase.

## Por qué

220-61 permite el neutro a su carga de desbalance, y en alimentadores grandes la diferencia es de varios
calibres. Pero reducirlo es decisión del proyectista (y de la carga real, que la captura no siempre sabe):
por eso es opción, no lo que hace el programa solo.

El área del juego en paralelo es interpretación: la NOM no dice «área combinada» con esas palabras para el
neutro; es lo que queda al quitar 250-122(f), igual que el área combinada que 250-122(a) admite para
conductores seccionados. Con 708.57 A da 2 × 1/0 en vez de 2 × 2/0.

## Lo que se descartó

- **Reducir el neutro siempre**: escondería una decisión que es del proyectista.
- **Pedir que cada neutro en paralelo sea no menor que la tierra de la tabla**: es aplicar 250-122(f), que
  215-2(a)(2) excluye.
- **Solo la Tabla 250-66 en acometida**: le faltaba el 12.5 % de 250-24(c)(1); 708.57 A daba 2 × 3/0 en vez
  de 2 × 250 kcmil (I-166).

## Dónde está

`DatosDelTablero.NeutroReducido220_61`, `CuadroDeCarga.ReducirNeutro`, `CuadroDeCarga.PorQueNoSeReduceElNeutro`,
`CuadroDeCarga.NeutroReducido`; la casilla en `Captura.razor`. Hallazgos I-161, I-164, I-165 e I-166.
