# Sin mínimo de protección por tipo de carga: 20 A solo donde lo pide 210-11(c)

**CONFIRMADA · David · 2026-09-24**

## La decisión

- **Se quita el mínimo de 15 A en alumbrado y 20 A en contactos.** La protección de un circuito
  derivado sale de su carga (210-20(a)) y del primer tamaño de 240-6(a). Sin casilla para
  encenderlo: David no lo quiere como opción.
- **Los contactos de vivienda llevan un «Uso»**: General (por omisión), Cocina (aparatos pequeños),
  Lavadora o Baño. Los tres últimos llevan **20 A**, con la cita de la norma.
- **Aparatos pequeños y lavadora cuentan 1500 VA en el alimentador**, aunque la carga capturada sea
  menor. El derivado se calcula con su carga; los 1500 VA son carga de alimentador.
- **El principal menor que un derivado (M-03) avisa, no bloquea** (R-08). Con el mínimo por tipo
  fuera, solo pasa con un circuito de vivienda de 20 A y poca carga, o con factor de demanda menor
  que 1.

## Por qué

El mínimo venía del Excel (`MAX(20, …)` en contactos, `MAX(15, …)` en alumbrado) y de la práctica
de despacho, no de la NOM:

| Mínimo | Qué dice la NOM |
|---|---|
| Alumbrado 15 A | No hace falta: 15 A ya es el primer tamaño de 240-6(a) (16 A en riel DIN). Nunca actuaba. |
| Contactos 20 A | 210-3 y 210-21(b)(3) permiten circuitos de contactos de 15 A. Solo 210-11(c) exige 20 A, y solo en tres usos de vivienda. |

Aplicado a todos los contactos, el mínimo subía el conductor a 12 AWG sin artículo que lo pidiera —
el mismo problema que llevó a quitar el piso de calibre
([`sin-piso-practico-de-calibre.md`](sin-piso-practico-de-calibre.md)) — y producía la mayoría de los
avisos de M-03.

Quitarlo sin más dejaba sin cubrir lo que la norma sí exige: un circuito de cocina de 300 VA habría
salido de 15 A. Por eso el «Uso» entra en el mismo cambio.

## Lo que exige la norma, por uso

| Uso | Protección | Carga para el alimentador | Solo esas salidas |
|---|---|---|---|
| Cocina (aparatos pequeños) | 20 A — 210-11(c)(1) | 1500 VA — 220-52(a) | 210-52(b)(2) |
| Lavadora | 20 A — 210-11(c)(2) | 1500 VA — 220-52(b) | 210-11(c)(2) |
| Baño | 20 A — 210-11(c)(3) | La capturada | 210-11(c)(3), salvo su Excepción 2 |

- **Dos o más circuitos de aparatos pequeños**, 210-11(c)(1). Con uno solo capturado, el programa
  avisa; con ninguno, calla (el tablero puede no ser de vivienda).
- **Vivienda popular de hasta 60 m²:** exenta de 210-11(c) y 220-52. El programa todavía no sabe el
  tipo de inmueble (R-11, R-12); mientras tanto, el proyectista deja el uso en General.
- **«Solo esas salidas»** no se verifica: el programa no conoce las salidas. Lo declara quien captura.

## En el motor

El mínimo vivía en `CalculadoraCircuitoDerivadoNoMotor` (motor copiado). Se quitó y se agregó
`ProteccionMinimaA` + `ReferenciaProteccionMinima` a `DatosEntradaCircuitoDerivadoNoMotor`, vacíos
por omisión. **Cambia resultados en el escritorio también**: queda en la lista de
[`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md).
