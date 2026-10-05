# Lo de criterio de la auditoría de motores (Art. 430, 2026-10-03)

**CONFIRMADA · David · 2026-10-05** — las cuatro decisiones de abajo. Las propuso Claude el 2026-10-05, con
opciones y recomendación, en el documento que David revisó con un agente experto
([Claude Docs](https://claude.ai/code/artifact/c9f5919d-6c48-47aa-8371-5c1709b8ca18)); David regresó con una
letra por decisión y lo que cada una pedía además. Hallazgos M-21, M-22, M-23 e I-189 de
[`../estado/HALLAZGOS.md`](../estado/HALLAZGOS.md). Pruebas: `CriterioDeMotores20261005Tests`.

| AM | Hallazgo | Decisión | Commit |
|---|---|---|---|
| 4 | M-21 · la columna de terminales de un motor | **B**, con la regla de David | `b78b251` |
| 5 | M-22 · 430-62(b) con el conductor mínimo | **B**, y la cita dice 240-4 | `3ff5157` |
| 7 | M-23 · variador con dispositivo de desviación | **A2**, como límite del rango | `72a2d48`, `93951eb` |
| 11 | I-189 · A/C y variador nacían en 1 polo | **B**, sin valor por omisión | `93951eb` |

Las cuatro tocan el motor copiado de PowerNode-DesignSuite (salvo AM-11, que es de la web): anotadas en
[`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md) para llevarlas al escritorio.

---

## AM-4 · M-21 — la columna es la más baja de las dos terminales

**Lo que decidió David:** «B — con base 60 °C en el extremo del motor: columna = la más baja entre
(interruptor: >100 A → 75 °C; ≤100 A → 60 °C o 75 °C si el tablero está marcado) y (motor/arrancador:
conductor >1 AWG → 75 °C; 14–1 AWG → 60 °C salvo casilla). Casilla por motor «Motor diseño B a E y
arrancador marcado 75 °C» — 110-14(c) («la más baja de cualquier terminal»), 110-14(c)(1)a.(3) y a.(4);
desmarcada por omisión.»

**Cómo quedó** (`CalculadoraCircuitoDerivadoMotor`, paso 5.5):

- La terminal del interruptor, como antes: más de 100 A, 75 °C — 110-14(c)(1)b.; 100 A o menos, 60 °C, o
  75 °C con «Terminales marcadas 75 °C» del tablero — a.(3).
- La del motor y su arrancador: sin la casilla, 60 °C con conductor de 14 a 1 AWG — a.; 75 °C mayor que
  1 AWG — b. Con la casilla, 75 °C — a.(3) y a.(4).
- El calibre decide la terminal y la terminal el calibre. Se resuelve con el conductor que pide la ampacidad
  en la columna de 60 °C: si es de 1 AWG o menor, esa es la columna; si pasa de 1 AWG, la del motor es de
  75 °C y en esa columna el conductor no baja de 1/0 AWG (con 1 AWG volvería a ser de 60 °C y no alcanzaría).
  `SeleccionConductor` acepta para eso un calibre mínimo opcional.
- El tope de 100 A de «prioridad al conductor» (M-20, pregunta 5) sigue siendo el de la terminal del
  interruptor.
- Citas: la de 110-14(c)(1) del interruptor y una de «Terminal del motor y del arrancador…»; el desglose de
  la celda dice cuándo manda la del motor. La casilla está en el pie del desplegable del motor, junto a la de
  la Excepción 2; se guarda (formato 13).

**Las pruebas de la decisión:** 25 HP a 220 V → 3 AWG (antes 4); 50 HP a 440 V → 3 AWG (antes 4); 100 HP a
440 V → 2/0, sin cambio. Además, 40 HP a 220 V (130 A) → 1/0 (antes 1), el caso del mínimo.

**Lo que cambia sin que se pidiera, por la misma regla:** con el tablero marcado 75 °C, un motor de 100 A o
menos con conductor de 14 a 1 AWG baja a la columna de 60 °C si la casilla del motor no está marcada
(`MotoresTests.I15`: 5 HP, de 14 a 12 AWG). Y con «prioridad al conductor», 30 HP a 220 V en tablero marcado
pasa de 3 AWG con 100 A a 1 AWG con 110 A: ya no topa en 100 A (la terminal del interruptor es de 75 °C) y el
conductor va en la columna de 60 °C del motor. Con la casilla, lo de antes.

**Archivos anteriores:** abren con la casilla desmarcada y calculan con la regla nueva: un motor de más de
100 A con conductor de 1 AWG o menor sale un calibre arriba. No se avisa al abrir: la memoria lo dice en la
cita.

## AM-5 · M-22 — 430-62(b) con el conductor mínimo

**Lo que decidió David:** «B — agregar además «70 A = ampacidad del 4 AWG: el conductor queda protegido —
240-4».»

**Cómo quedó** (`CalculadoraAlimentador`, cita de 430-62(b)): la lectura literal se queda, y la cita dice
además, en el caso A de la auditoría: «70 A = ampacidad del 4 AWG: el conductor queda protegido — 240-4. No
hay tamaño estándar entre 60.93 A y el techo de 69.21 A: el anterior, 60 A, queda debajo de la capacidad que
pide 430-24.» Si la protección queda debajo de la ampacidad, «70 A < 85 A, la ampacidad del 3 AWG…». La frase
del tamaño intermedio solo sale cuando el anterior tamaño de la serie bajo el techo queda debajo de la
capacidad mínima. Solo texto.

## AM-7 · M-23 — el variador con bypass

**Lo que decidió David:** «A2 — como límite del rango, no solo aviso: con bypass, protección entre 125 % de
la entrada y el menor de (máx. del fabricante, máx. Tabla 430-52 del motor); rango vacío → aviso. Fundamento:
la NOM-2012 no tiene 430-130 → 430-120 manda a la Parte D. Con bypass: marcar OL (430-124(b)) y desconexión
≥115 % del mayor de entrada y corriente a plena carga del motor (430-128, 430-110(a)).»

**Cómo quedó:**

- Dos datos del variador, en el pie del desplegable: «HP del motor» (opcional; obligatorio con bypass) y
  «Con bypass». La FLC es la de tabla a la tensión del circuito: con el bypass el motor va directo a la línea.
- Conductor: el mayor de 125 % de la entrada y 125 % de la FLC — 430-122(b).
- Protección: del 125 % de la entrada al menor de la máxima del fabricante y la de la Tabla 430-52 para el
  motor (con su Excepción 1, en la serie). Rango vacío: se usa el máximo y se avisa (cita «⚠» y aviso del
  renglón). El máximo que entra a 430-62(a), el menor de los dos.
- Sobrecarga: «OL» con la del bypass en el arrancador del circuito de desviación — 430-124(b).
- Desconexión: 115 % del mayor de la entrada y la FLC — 430-128, 430-110(a), en la cita y en la memoria.
- Los dos datos se guardan (formato 13) y pasan con el variador al desplegable y de regreso; en un grupo
  (430-53) no entran al cálculo, y se avisa.

**La prueba de la decisión:** 30 HP a 440 V, entrada 42 A, máxima 70 A → sin cambio: 6 AWG, de 60 a 70 A.
La hoja de la memoria del variador con bypass llegó en `93951eb` (en `72a2d48` seguía diciendo que la FLC del
motor y la Tabla 430-52 no se usan).

## AM-11 · I-189 — la tensión de placa

**Lo que decidió David:** «B — sin valor por omisión: el campo «Tensión de placa» es obligatorio (aviso rojo
como la MOCP); opciones según el tablero: 127 V 1F, 208/230 V 1F (2P), 220 V 3F (3P, solo tablero 3F),
440/460 V 3F; en variador es la tensión de ENTRADA; aplica también a «De habitación».»

**Cómo quedó** (`TensionDePlaca`, web; el motor de cálculo no cambia):

- La piden un equipo de A/C con ampacidad de placa, un motocompresor, un acondicionador de habitación y un
  variador («Tensión de entrada»). Un grupo no: va a la tensión de su circuito.
- Sin ella el circuito no se calcula: error del renglón, como la MOCP, y el selector en rojo.
- Opciones según el tablero: 127 V 1F (1 polo, con neutro y 100 a 140 V a neutro), 208/230 V 1F (2 polos,
  200 a 250 V entre fases), 220 V 3F (3 polos, trifásico de 200 a 250 V), 440/460 V 3F (400 a 500 V).
  **Agregada por Claude:** 575/600 V 3F (550 a 620 V), porque la app calcula tableros de 600 V (I-196) y sin
  ella un equipo de A/C ahí no tendría tensión que escoger. Si David no la quiere, se quita.
- Escogerla cambia los polos; si no caben, no cambia y dice por qué. Cambiar los polos en la columna P cambia
  la tensión a la de esos polos (si ya había una; si faltaba, sigue faltando). Si el tablero cambia y la placa
  ya no es de él, error.
- Se guarda (formato 13) y pasa con el equipo al desplegable y de regreso. **Un archivo de formato 12 o
  anterior** abre con la tensión de sus polos —calcula igual que antes— y avisa: «se tomó la de sus 2 polos,
  208/230 V 1F. Verifícala con la placa del equipo».
- La memoria dice la tensión en la descripción del equipo.

**La prueba:** el minisplit de 1 TR del caso A (MCA 10.5 A, MOCP 15 A, 20 m): con 127 V 1F, 12 AWG por caída;
con 208/230 V 1F, 2 polos y 14 AWG.
