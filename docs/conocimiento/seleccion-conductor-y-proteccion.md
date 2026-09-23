# Selección de conductor y protección

**Actualizado:** 2026-09-23. Alcance: seleccionar conductor y protección con la Tabla 310-15(b)(16).

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

## Casos de verificación

| Caso | Condición | Resultado |
|---|---|---|
| 32 A continuos, Alumbrado, 9 agrupados, THHN | 125 % sin factores y 100 % con factores | 8 AWG, 40 A (240-4(b)) |
| 32 A continuos, Contactos, 9 agrupados, THHN | Sin 240-4(b) | 6 AWG |
| 26 A no continuos, 9 agrupados | THHN / THW-LS | 10 AWG / 8 AWG |
| 45 A no continuos, Equipo | Terminales 60 °C / marcadas 75 °C | 6 AWG / 8 AWG |
| 53 A, Alumbrado, 5 m | NOM completa / riel DIN | 60 A 6 AWG / 63 A 4 AWG |

## Fuera de alcance

- Tabla 310-15(b)(17) (conductores al aire libre).
- 310-15(b)(3)(c) (canalizaciones en azoteas).
- Excepción del ensamble al 100 % en la pantalla (existe en el modelo).
