# Motores en HP: circuito de motor por el Art. 430, y en el alimentador por fase

**PROPUESTA · Claude · 2026-09-26** — I-15, a pedido de David («vamos con I-15»). Implementada;
pendiente de confirmar.

## El problema

«Motor / A/C» se calculaba como carga de placa (Art. 210): VA, W o A, 125 % si es continua. Un
motor no se dimensiona así. Su corriente sale de la tabla y no de la placa (430-6(a)), el conductor
va al 125 % de ella (430-22), la protección se elige para que arranque (Tabla 430-52) y en el
alimentador pesa el mayor al 125 % y los demás al 100 % (430-24). El motor copiado ya traía todo eso
(`CalculadoraCircuitoDerivadoMotor`, `AgregadoMotores`); faltaba capturarlo.

## La propuesta

- **HP es una unidad más de Motor / A/C.** El selector de unidad ofrece VA, W, A y, solo en ese tipo,
  HP. Con HP, las celdas de continua y no continua se vuelven un selector de caballos. En VA, W o A
  el renglón sigue siendo carga de placa: un minisplit o un refrigerador no traen HP en la placa, y
  obligar a inventarlos es lo que el escritorio ya descartó (`TipoCarga.Equipo`).
- **Nada más se captura.** Lo demás sale del tablero:

  | Dato | De dónde sale | Por qué |
  |---|---|---|
  | Monofásico o trifásico | Polos: 1 y 2, monofásico; 3, trifásico. | La Tabla 430-249 (dos fases a 90°) no aplica a estos sistemas. |
  | Columna de la tabla | La tensión del circuito, en volts enteros. | Las tablas se permiten para sistemas de 110-120 V y 220-240 V: 220 V se lee en 230 V; 127 V tiene columna propia en la 430-248. |
  | Dispositivo | Interruptor automático de tiempo inverso. | Es lo que se monta en un tablero de derivados. |
  | Tipo de motor | Monofásico, o jaula de ardilla en 3 polos. | Con interruptor de tiempo inverso dan 250 %, igual que diseño B y síncrono. Rotor devanado y CD (150 %) no se alimentan de un centro de carga. |

- **Solo los HP que trae la tabla.** El selector ofrece los de la tabla a esa tensión y esos polos
  (un monofásico llega a 10 HP). Si se cambian los polos y el motor ya no está, el renglón lo dice.
- **Un motor en HP no se desglosa.** Es un circuito de un motor; para varios aparatos, VA, W o A.
- **En el cuadro, VA = FLC × tensión (× √3).** Es la carga del balanceo, del resumen y de los kW. No
  es continua ni no continua: el resumen la lleva en su propio renglón, «Motores en HP».
- **En el alimentador, por fase.** Cada barra lleva sus motores: 125 % de la FLC del mayor que la
  toca y 100 % de los demás, sumados a su 125 % de continua y 100 % de no continua. Gobierna la fase
  que pide más. Tres motores de 1 HP a 127 V, uno por fase, piden 17.5 A por fase (20 A), no 45.5 A
  (50 A) como si todos colgaran de la misma.
- **430-62(a) sobre la fase que gobierna.** Máximo de la protección: la mayor protección de motor de
  esa fase, más la FLC de los demás motores, más lo que 215-3 pide para la otra carga (430-63). Si
  el principal sale menor que la protección de un motor, el aviso de M-03 dice hasta cuánto permite
  subirlo 430-62(a); si lo excede, se avisa.
- **El factor de demanda de motores (430-26) reduce su FLC en el alimentador**, no el derivado.

## Cambios al motor copiado

Se registran en [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md) para llevarlos
al escritorio:

- `CorrienteDeFaseAlimentador` lleva sus motores (`AgregadoMotores`) y su fasor; `CalculadoraAlimentador`
  los usa para la fase que gobierna, 430-24, 430-62(a) y la caída fasorial.
- `DatosEntradaCircuitoDerivadoMotor.TerminalesMarcadas75C`: el derivado de motor ignoraba la
  declaración de 110-14(c)(1)a.(3) (M-06 solo la había llevado al no-motor y al alimentador).
- M-09: el techo de 430-63 sumaba la otra carga al 100 %; 215-3 le pide 125 % de la continua.

## Lo que queda fuera

- **Art. 440** (motocompresores herméticos, con MCA y MOCP de placa). Un A/C se captura en VA, W o
  A, como carga de placa, o en HP si la placa los trae.
- **Varios motores en un circuito** (430-53) y la corriente de rotor bloqueado (430-52(c)(1)
  Excepción 2): el derivado usa el tamaño inmediato superior y nada más.
- **430-32**: la sobrecarga del motor va en el arrancador; la memoria lo dice, no la dimensiona.

## Por decidir

1. ¿El tipo de motor y el dispositivo quedan fijos, o se capturan (fusible, rotor devanado)?
2. ¿430-62(a) por fase —como se implementó— o con todos los motores del tablero?
