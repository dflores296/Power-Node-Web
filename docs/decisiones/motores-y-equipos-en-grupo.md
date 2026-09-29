# Motores, A/C y aparatos con motor: varios en un circuito, y lo que faltaba de los Arts. 430 y 440

**PROPUESTA · Claude · 2026-09-29** — a pedido de David: medir motores y A/C contra la norma, ver
cómo los captura un ingeniero y proponer. David eligió el alcance y contestó tres preguntas el mismo
día («Decisiones de David», abajo). Falta que la marque CONFIRMADA.

Amplía [`motores-art-430.md`](motores-art-430.md) y [`tipos-de-carga.md`](tipos-de-carga.md): lo que
ahí quedó fuera —varios motores en un circuito (430-53), varios motocompresores (440-22(b)), el
acondicionador de habitación (Parte G)— entra aquí.

Texto de la norma leído del corpus de `dflores296/NOM-001-SEDE-2012` (`data/corpus.json`), no de
memoria: 430-6, 430-7(d), 430-22, 430-24, 430-25, 430-26, 430-32, 430-42, 430-52, 430-53, 430-62,
430-63, 430-109, 430-110, 430-122 a 430-128; 440-1 a 440-8, 440-12, 440-22, 440-31 a 440-35, 440-60
a 440-65; 422-10, 422-11, 422-62; 424-3; 220-14, 220-18, 220-50, 220-53, 220-60; 210-23; 110-3.

## Lo que se midió

Un motor solo y un A/C solo se calculan bien: FLC de las Tablas 430-248/430-250 por la tensión del
circuito (430-6(a)(1)), conductor al 125 % (430-22), 250 % de la Tabla 430-52 con el tamaño siguiente
(430-52(c)(1) Exc. 1); A/C por MCA/MOCP (440-4(b)) o por corriente de carga nominal (440-32, 440-22(a)).
Un motor de 2 polos a 220 V es monofásico (Tabla 430-248); la Tabla 430-249 es de sistemas de dos
fases y no aplica. Estrella-delta no cambia la FLC ni el tramo tablero→arrancador (125 %); el 72 % de
430-22(c) es del arrancador al motor.

| # | Hallazgo | Norma | Veredicto |
|---|---|---|---|
| A8, A9 | Un circuito de Motor admite un solo motor; la salida sugerida («usar Equipo») los calcula como carga de placa, sin 430-24 ni 430-53, y fuera del grupo de motores del alimentador | 430-53, 430-24, 220-18(a) | Falta, y el camino sugerido da resultados incorrectos |
| A17 | El F.D. de motores reduce también al motor mayor antes del 125 %: con F.D. 0.5 y un motor de 5 hp el alimentador pide 9.5 A y el motor consume 15.2 | 430-26 | Error del lado inseguro — M-12 |
| A18 | Un A/C con MCA recibe otro 25 % en el alimentador si es el mayor | 440-4(b), 440-33 | Sobrado — M-13 |
| A22 | Un circuito que tuvo aparatos y pasa a Motor o A/C los imprime en la memoria | — | Error — I-113 |
| A7 | Motor con variador: se calcula como motor (250 %) | 430-122(a), 110-3(b), 430-128 | No aplica a variadores |
| A12, A13 | Varios motocompresores sin MCA de conjunto; acondicionador de habitación | 440-22(b), 440-33, 440 Parte G | Falta |
| A15 | Aparato con motor mezclado con otras cargas | 220-18(a), 422-10 | Falta el 125 % del motor mayor |
| A6, A19, A21 | Servicio no continuo; medio de desconexión; cargas no simultáneas | 430-22(e); 430-110, 440-12, 430-128; 430-24 Exc. 3, 440-33 Exc. 1, 220-60 | Falta |

## Cómo se clasifica

Regla de [`tipos-de-carga.md`](tipos-de-carga.md): **si cambia el factor de demanda del Art. 220 es
un tipo; si solo cambia el cálculo del circuito, es un selector dentro del tipo.** Los seis tipos
alcanzan; se agregan formas de captura y miembros en el desglose.

| Carga | Artículo | Tipo | Cómo se captura |
|---|---|---|---|
| Motor de uso general, 1F 127 V, 1F 220 V, 3F | 430 | Motor | HP o A |
| Varios motores, o motores y otras cargas | 430-53, 430-24 | Motor, con desglose | Un miembro por motor (HP o A) y por carga (VA) |
| Motor con variador | 430 Parte J | Motor · Variador | Corriente de entrada y protección máxima del variador |
| Motor de servicio no continuo | 430-22(e) | Motor · servicio | Clase de servicio, minutos y corriente de placa |
| Motocompresor hermético (minisplit, paquete, condensadora, cámara) | 440 | A/C y refrig. | MCA/MOCP o corriente de carga nominal |
| Varios motocompresores, o compresor y ventiladores sin MCA de conjunto | 440-22(b), 440-33 | A/C, con desglose | Un miembro por compresor (RLA, BCSC) y por motor |
| Acondicionador de habitación con clavija | 440 Parte G | A/C · Habitación | Corriente total de placa |
| Refrigerador y congelador domésticos, enfriador de agua | 422 (440-3(c)), 440-22(b) Exc. 2 | Contactos (uso Refrigerador) o Equipo | Placa |
| Aparato con motor (lavadora, lavavajillas, triturador) | 422, 430-6(a)(1) Exc. 3 | Equipo; en el desglose, miembro con motor | A o HP de placa |
| Equipo de A/C sin motocompresor (manejadora, condensador remoto) | 440-3(b) | Motor o Equipo | HP/A o placa |

## La propuesta

| # | Cambio | Norma | Fase |
|---|---|---|---|
| 1 | El F.D. de motores no reduce al motor mayor: conserva su 125 % completo; el F.D. reduce a los demás | 430-26, 430-24 | 0 |
| 2 | Un A/C con MCA entra al grupo del alimentador al 100 % y no compite por el mayor | 440-4(b), 440-33, 440-7 | 0 |
| 3 | La memoria de un Motor o A/C no imprime aparatos que no cuentan | — | 0 |
| 4 | Bajo el HP, la alimentación y la tabla («Monofásico 127 V · Tabla 430-248»); sin «+N» en un motor trifásico | 430-6(a)(1) | 0 |
| 5 | Guía de clasificación en la ayuda de Tipo | 440-3(b)/(c), 422 | 0 |
| 6 | Desglose en Motor: motores y otras cargas del circuito | 430-53, 430-42 | 1 |
| 7 | Cálculo de grupo: conductor por 430-24; interruptor = mayor estándar ≤ [% de 430-52 del mayor × su FLC + Σ demás + otras cargas]; hasta 240-4(b) si queda bajo la ampacidad | 430-53(c)(4), 430-24, 240-4(b) | 1 |
| 8 | En el alimentador, cada motor del grupo por separado; el interruptor del grupo, para 430-62(a) | 430-24, 430-62(a) | 1 |
| 9 | Nota 430-53(a) para motores de hasta 1 hp y 6 A en circuitos de 20 A a 127 V | 430-53(a), 430-42 | 1 |
| 10 | Desglose en A/C: motocompresores, ventiladores y otras cargas | 440-22(b), 440-33, 440-34, 440-7 | 2 |
| 11 | A/C · Habitación: una unidad; ≤ 80 % del circuito, ≤ 50 % con otras cargas | 440-62 | 2 |
| 12 | Miembro con motor en el desglose de Equipo, Contactos y Alumbrado: 125 % del motor mayor | 220-18(a), 422-10 | 3 |
| 13 | Motor · Variador: 125 % de la corriente de entrada; protección máxima del fabricante | 430-122(a), 110-3(b) | 4 |
| 14 | Servicio no continuo con la Tabla 430-22(e) | 430-22(e), 430-24 Exc. 1 | 4 |
| 15 | «No simultáneo con el circuito N»: del par cuenta el mayor | 430-24 Exc. 3, 440-33 Exc. 1, 220-60 | 4 |
| 16 | Medio de desconexión mínimo en la memoria (115 %) | 430-110, 440-12(a)(1), 430-128 | 4 |
| 17 | Tabla 430-22(e) desde el repo de la norma | — | 4 |
| 18 | Archivo formato 3 | — | 1 |

Todos los miembros de un grupo van a la tensión y las fases del circuito (sus polos).

## Decisiones de David (2026-09-29)

1. **Alcance: las cinco fases** (0 a 4).
2. **Interruptor de un grupo (430-53(c)(4)): el mayor estándar que no pase del límite** —lectura
   literal de «no exceda»—, y hasta 240-4(b) solo si queda bajo la ampacidad del conductor.
3. **A/C con MCA en el alimentador: al 100 %, sin otro 25 %.** El 25 % lo toma el motor mayor sin MCA.

## Cómo quedó (2026-09-29)

Las cinco fases, en `main`. Lo que la pantalla agrega, para revisarlo junto:

| Dónde | Qué | Hallazgo |
|---|---|---|
| Unidad de Motor | HP · A · **Varios** (grupo con desglose) · **VFD** (entrada y protección máxima del variador) | I-115, I-119 |
| Unidad de A/C | MCA · A · **Varios** (motocompresores, ventiladores y otras cargas) · **Hab.** (acondicionador de habitación) | I-116, I-117 |
| Desglose de cualquier tipo de carga | La unidad dice la clase: carga (VA, W, A), aparato con motor (HP, A), A/C de cuarto (A); la celda «Continua» de una máquina dice «motor» o «motor · 125 %» | I-118, I-117 |
| Detalle del circuito (la flecha junto a la descripción, ahora en todos) | «No simultáneo con»; en un motor solo, su servicio (430-22(e)) | I-120, I-121 |
| Memoria, sección 3 | Medio de desconexión mínimo | I-122 |

El archivo pasó a formato 4 (el 3 duró una publicación: I-115). Una Fase 3 se hizo antes que la 2
porque tocaba el mismo desglose.

## Lo que queda fuera

- **430-52(c)(1) Excepción 2** (subir la protección por la corriente de arranque) y las Tablas
  430-251: sin datos de rotor bloqueado en el cuadro.
- **Motores de alto par, baja velocidad y velocidades múltiples** (430-6(b), 430-22(b)).
- **Fusibles, disparo instantáneo, rotor devanado**: el interruptor de un tablero es de tiempo inverso.
- **Centros de control de motores** (Parte H) y **elevadores** (Art. 620).
- **Sobrecarga** (430-32, 440-52): va en el arrancador; la memoria la menciona.
- **430-53(c)(1)–(3) y (5)**: que controladores y relevadores estén aprobados para instalación en
  grupo no se puede verificar desde el cuadro; la memoria lo pide.
