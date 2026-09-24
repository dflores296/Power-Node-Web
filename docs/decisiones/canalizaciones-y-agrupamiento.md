# Canalizaciones: el agrupamiento se cuenta por tubo y la canalización se calcula

**PROPUESTA · Claude · 2026-09-24** — con las decisiones de David de la misma fecha (abajo). Falta
que David la marque CONFIRMADA.

## El problema

«Agrupados» era un solo número para todo el tablero, y se aplicaba igual a cada derivado y al
alimentador. En la obra los circuitos se agrupan en tubos y el alimentador casi siempre va solo:
poner 9 porque tres circuitos comparten tubo le aplicaba 70 % también al alimentador, y dejar 3
dejaba sin ajuste a un tubo con seis portadores.

El programa no puede adivinar la instalación. Lo que sí sabe el diseñador es **qué circuitos van
juntos**. Se captura eso —«los circuitos 1, 2 y 7 van por T1»— y el programa cuenta los portadores,
aplica el factor que toca según el tipo de canalización y calcula su tamaño. «Agrupados» deja de
capturarse: es un resultado.

## Decisiones de David (2026-09-24)

- **Alcance, entrega 1:** tubo conduit (los bloques de la Tabla 4, más niple de 60 cm o menos y tubo
  en azotea al sol), ductos metálicos y no metálicos, canales auxiliares y canalizaciones
  superficiales. **Charola en la entrega 2**: necesita las dimensiones reales de cada cable y otra
  forma de calcular la ampacidad (392-80).
- **Neutro por circuito:** 1 polo siempre lleva neutro; 2 y 3 polos no, salvo la casilla «+N».
- **THW-LS y THHW-LS** no vienen en la Tabla 5: se captura el diámetro exterior del fabricante
  (Capítulo 10, Nota 5).
- **El código va en el motor** (`Calculo` y `Normativa`), para llevarlo al escritorio, que no calcula
  canalizaciones. Se registra en `docs/conocimiento/motor-copiado.md`.

## Portadores por circuito — 310-15(b)(3)(a), (b)(5), (b)(6)

| Circuito | Fases | ¿El neutro cuenta? |
|---|---|---|
| 1 polo (F-N) | 1 | Sí: regresa toda la corriente. (b)(5)(1) solo perdona al neutro que lleva el desbalance *de otros conductores del mismo circuito*. |
| 2 polos, carga F-F | 2 | No lleva. |
| 2 polos + N en 3F-4H | 2 | Sí — (b)(5)(2). |
| 2 polos + N en 1F-3H | 2 | No — (b)(5)(1). |
| 3 polos sin N | 3 | No lleva. |
| 3 polos + N en 3F-4H | 3 | No — (b)(5)(1); sí con carga mayormente no lineal — (b)(5)(3). |
| Neutro compartido (circuito multiconductor, 210-4) | suma | Como el multipolar equivalente. |
| Puesta a tierra | — | Nunca — (b)(6). Sí cuenta para el llenado (Nota 3 del Cap. 10). |

Conductores en paralelo: cada uno cuenta si van en el mismo tubo (310-15(b)(3)(a)). Por omisión, un
juego por tubo y tubos iguales — 310-10(h)(3).

El neutro compartido solo se acepta con los circuitos de 1 polo del tubo en barras distintas; se
avisa 210-4(b) (desconexión simultánea) y 210-4(d) (agrupar en el tablero).

## El ajuste según la canalización

| Canalización | Llenado | Ajuste de la Tabla 310-15(b)(3)(a) |
|---|---|---|
| Tubo conduit | Tabla 1: 53 / 31 / 40 % | Con más de 3 portadores. |
| Niple de 60 cm o menos | 60 % (Nota 4) | No — (b)(3)(a)(2). |
| Ducto metálico | 20 % — 376-22(a) | Solo con más de 30 — 376-22(b). |
| Ducto no metálico | 20 % — 378-22 | Sí. |
| Canal auxiliar metálico | 20 % — 366-22(a) | Solo con más de 30 — 366-23(a). |
| Canal auxiliar no metálico | 20 % — 366-22(b) | Sí — 366-23(b). |
| Superficial metálica | Lo que diga el fabricante — 386-22 | No, si pasa de 2500 mm², no pasa de 30 portadores ni del 20 %. |
| Superficial no metálica | Lo que diga el fabricante — 388-22 | Sí. |

Tubo en azotea al sol: la Tabla 310-15(b)(3)(c) suma de 14 a 33 °C a la temperatura ambiente. Un
circuito que pasa por varias canalizaciones toma la del tramo más desfavorable — 310-15(a)(2).

## El tamaño — Capítulo 10

Suma de áreas de **todos** los conductores (Tabla 5 aislados, Tabla 8 desnudos, π·d²/4 con el
diámetro del fabricante en LS); Tabla 1 por el número de conductores; el tamaño más chico de la
Tabla 4 que alcance. Con tres conductores y relación de diámetros entre 2.8 y 3.2, el tamaño
inmediato superior (Nota 2 de la Tabla 1). Tierra común por tubo: 250-122(c).

## Erratas del DOF encontradas

- **Tabla 4:** el bloque del Art. 358 dice «Tubo conduit no metálico (EMT)»; el Art. 358 es «Tubo
  conduit metálico ligero». Hay **dos** bloques «Cédula 80»: el segundo da 56.4 mm interiores en
  53 (2), más que Cédula 40; una Cédula 80 no puede tener más diámetro que la 40. Ese bloque no se
  ofrece.
- **Tabla 5:** en THHN la columna de mm² está corrida («6.63 | 8», «8.37 | 6»). Las áreas del
  conductor aislado son las correctas. Se lee por la designación AWG/kcmil.

## Fuera de alcance

Charola (entrega 2). Factores por diversidad del Apéndice (Tabla B.310.15(B)(2)(11)). Cables
directamente enterrados. Tabla 5A (aluminio compacto). Excepción de tramo de 310-15(a)(2). Radio de
curvatura (Tabla 2).
