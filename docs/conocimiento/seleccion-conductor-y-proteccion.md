# Selección de conductor y protección

**Actualizado:** 2026-09-24. Alcance: seleccionar conductor y protección con la Tabla 310-15(b)(16).

Guía de la norma: [dflores296.github.io/NOM-001-SEDE-2012](https://dflores296.github.io/NOM-001-SEDE-2012).

## Matriz de trazabilidad

| # | Tema | Requisito | Referencia NOM | Estado | Prueba |
|---|---|---|---|---|---|
| 1 | Corriente de diseño | Calcular In = VA ÷ tensión del circuito, sin factor de demanda. | [210-19(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-19), [220-42](https://dflores296.github.io/NOM-001-SEDE-2012/art/220/#220-42) | Cumple | `UnCircuitoDeAlumbrado…` |
| 2 | Carga continua | Clasificar como continua la carga de 3 h o más. Aplicar 125 % a la continua y 100 % a la no continua. | [Art. 100](https://dflores296.github.io/NOM-001-SEDE-2012/glosario/), [210-20(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-20), [215-3](https://dflores296.github.io/NOM-001-SEDE-2012/art/215/#215-3) | Cumple | `ElDesgloseDeLaProteccion…` |
| 3 | 125 % y factores | Verificar el 125 % contra la ampacidad de tabla sin factores; verificar la carga al 100 % contra la ampacidad corregida. | [210-19(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-19), [215-2(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/215/#215-2) | Cumple (M-05) | `DosRevisiones_…` |
| 4 | Tabla 310-15(b)(16) | Seleccionar la columna por tipo de aislamiento y lugar. | [Tabla 310-15(b)(16)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-16), [Tabla 310-104(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-104-a) | Cumple (I-32) | `Aislamiento_…` |
| 5 | Factor por temperatura | Aplicar el factor de la Tabla 310-15(b)(2)(a) en la columna del aislamiento. | [310-15(b)(2)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#310-15), [Tabla 310-15(b)(2)(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-2-a) | Cumple | `DosRevisiones_…` |
| 6 | Factor por agrupamiento | Aplicar el factor de la Tabla 310-15(b)(3)(a). Excluir tierra; contar neutro según (b)(5). | [310-15(b)(3)(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#310-15), [Tabla 310-15(b)(3)(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-3-a) | Cumple (I-34) | `DosRevisiones_…` |
| 7 | Calibres pequeños | Limitar la protección de 14, 12 y 10 AWG de cobre a 15, 20 y 30 A. | [240-4(d)](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-4) | Cumple | `Serie_EnRielDinNoHay15A_…` |
| 8 | Temperatura de terminales | Limitar a 60 °C hasta 100 A y a 75 °C arriba de 100 A. Usar 75 °C con equipo marcado. Usar 60 °C con conductor de 60 °C. | [110-14(c)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/110/#110-14) | Cumple (M-06) | `Terminales_…` |
| 9 | Inmediata superior (protección) | Seleccionar el primer tamaño normalizado mayor o igual a la capacidad mínima, en la familia elegida. | [240-6(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-6) | Cumple (I-29) | `Serie_…` |
| 10 | Inmediata superior (conductor) | Permitir la protección estándar inmediata superior a la ampacidad no estándar, hasta 800 A, salvo circuitos de varios contactos. Comparar contra la lista completa de 240-6(a). | [240-4(b)](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-4) | Cumple (M-04) | `Excepcion240_4b_…`, `Serie_En240_4bManda_…` |
| 11 | Choque entre reglas | Aumentar el conductor cuando la protección supera su ampacidad y no aplica 240-4(b). | [240-4](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-4) | Cumple | `Excepcion240_4b_…` |
| 12 | Visibilidad | Mostrar el desglose de protección y conductor en tooltip y en la memoria, sección 4. | — | Cumple (I-33) | `LaSeccion4CuadraConElConductorElegido` |
| 13 | Neutro en 2 fases + neutro de estrella | Contar el neutro como portador; darle el calibre de la fase; no reducirlo. | [310-15(b)(5)(2)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#310-15), [220-61(c)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/220/#220-61) | Cumple (R-09) | `R09_…` |
| 14 | 2F-3H 220Y/127 | No aplicar la excepción de 220-61(a) (neutro × 140 %) ni la Tabla 310-15(b)(7). Ver abajo. | [220-61(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/220/#220-61), [310-15(b)(7)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#310-15) | Cumple (R-10) | `R10_…` |

## Casos de verificación

| Caso | Condición | Resultado |
|---|---|---|
| 32 A continuos, Alumbrado, 9 agrupados, THHN | 125 % sin factores y 100 % con factores | 8 AWG, 40 A (240-4(b)) |
| 32 A continuos, Contactos, 9 agrupados, THHN | Sin 240-4(b) | 6 AWG |
| 26 A no continuos, 9 agrupados | THHN / THW-LS | 10 AWG / 8 AWG |
| 45 A no continuos, Equipo | Terminales 60 °C / marcadas 75 °C | 6 AWG / 8 AWG |
| 53 A, Alumbrado, 5 m | NOM completa / riel DIN | 60 A 6 AWG / 63 A 4 AWG |
| Alimentador 2F-3H 220Y/127, 99.99 A por fase, terminales 75 °C | Sin 220-61(a) excepción ni Tabla 310-15(b)(7) | 100 A; fase y neutro 3 AWG (con × 140 %: neutro 1/0 AWG; con (b)(7): 4 AWG) |

## Lecturas de la norma

**220-61(a), Excepción — neutro × 140 %.** El texto dice «sistemas de 2 fases, 3 hilos o 2 fases 5
hilos». Viene del NEC 220.61(A) Exception, *2-phase, 3-wire or 2-phase, 5-wire systems*: sistemas
**bifásicos**, con dos tensiones a 90°. En México «2F-3H» nombra otra cosa: **dos fases de una
estrella 220Y/127** (120° entre fases). Ahí el neutro lleva ≈ la corriente de fase, 310-15(b)(5)(2),
y 220-61(c)(1) prohíbe reducirlo. No se multiplica por 1.4: se dimensiona igual que la fase.

**Tabla 310-15(b)(7).** Solo para alimentadores y acometidas **monofásicos de 3 hilos, 120/240 V**,
de vivienda. No aplica a 2F-3H 220Y/127 ni a ningún tablero trifásico. La tabla no se carga en
`tablas-nom.json`.

## Fuera de alcance

- Tabla 310-15(b)(17) (conductores al aire libre).
- 310-15(b)(3)(c) (canalizaciones en azoteas).
- Excepción del ensamble al 100 % en la pantalla (existe en el modelo).
