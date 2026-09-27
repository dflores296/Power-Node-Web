# Seis tipos de carga: Motor (Art. 430) y A/C y refrigeración (Art. 440), cada uno por su lado

**PROPUESTA · Claude · 2026-09-27** — I-74, al revisar I-15 con David. David ya aceptó separar los
tipos (abajo, textual); falta que confirme lo que está en «Por decidir».

## El problema

«Motor / A/C» junta tres cosas que la NOM calcula distinto, y desde I-15 **la unidad decide el
artículo**: en HP, Art. 430; en VA, W o A, carga de placa, como Equipo. La norma no decide por la
unidad sino por el equipo:

| Captura | Hoy | La NOM |
|---|---|---|
| Bomba de 5 HP (3 polos, 220 V) en HP | Art. 430: 40 A, 12 AWG | Art. 430 |
| La misma bomba en A (15.2 A) | Carga de placa: 20 A, 12 AWG | Art. 430 — un motor marcado en amperes se toma con los HP de la tabla, 430-6(a)(1) |
| Minisplit en VA, W o A | Carga de placa | Art. 440 — corriente de placa, 440-6(a) |
| Minisplit en HP | Art. 430: FLC de tabla y 250 % | Art. 440 |
| Cualquier motor o A/C en VA, W o A, en el alimentador | Continua o no continua (215-3) | 430-24 / 440-33, 220-50 |

- **Motores → Art. 430.** La corriente sale de las Tablas 430-247 a 430-250, no de la placa
  (430-6(a)(1)).
- **A/C y refrigeración con motocompresor hermético → Art. 440**, que modifica al 430 (440-1,
  440-3(a)). La corriente sale de la placa (440-6(a)); conductor al 125 % (440-32); protección que no
  exceda 175 %, o 225 % si no arranca (440-22(a)); un equipo con varios motores trae en placa la
  ampacidad del conductor y la protección máxima (440-4(b)).
- **Aparatos con motor → Art. 422** (422-3, 440-3(c)): su valor de placa (422-10(a)).
- **En el alimentador, 220-50:** motores por 430-24, 430-25 y 430-26; motocompresores por 440-6. Los
  dos en un solo grupo: 125 % del mayor más la suma de los demás (440-33); el mayor es el de mayor
  corriente (440-7).

## Lo que David ya dijo (2026-09-27)

> «5 tipos es una decisión documentada pero no es inquebrantable siempre que este justificado
> ampliar los tipos de carga que debe considerar el programa»

> «estoy completamente de acuerdo que separemos así los tipos de carga»

## La propuesta

**La regla para lo que viene:** si cambia el factor de demanda del Art. 220, es un **tipo**; si solo
cambia el cálculo del circuito, es un **selector dentro del tipo**, como el uso de los contactos
(cocina, lavadora y baño llevan el mismo F.D.; el uso solo cambia el mínimo del circuito).

| # | Tipo | Circuito derivado | Se captura | F.D. (Art. 220) |
|---|---|---|---|---|
| 1 | Alumbrado | Art. 210 | VA, W o A | 220-42 |
| 2 | Contactos (con su uso en vivienda) | Art. 210 | VA, W o A | 220-44, 220-52 |
| 3 | Equipo (aparatos, también los que traen motor y placa) | Art. 422 | VA, W o A de placa | 220-53 a 220-56 |
| 4 | **Motor** | **Art. 430** | HP | 220-50 → 430-24, 430-26 |
| 5 | **A/C y refrigeración** | **Art. 440** | Corriente de placa (Por decidir 1) | 220-50 → 440-6; 220-60 |
| 6 | Calefacción | Art. 424 | VA, W o A | 220-51 |

**Seis, no siete.** «Aparato con motor» sería el mismo Equipo: el mismo artículo (422), el mismo
cálculo (el valor de placa, 422-10(a)) y el mismo F.D. (220-53 excluye estufas, secadoras,
calefacción y aire acondicionado, no los aparatos con motor).

### Motor — Art. 430

Lo de I-15, con dos cambios: es un tipo, no una unidad, y el selector muestra la FLC junto a cada
tamaño («5 HP — 15.20 A»), para que un motor marcado solo en amperes se capture con los HP que le
corresponden en la tabla (430-6(a)(1)). Lo demás no cambia: los polos dicen monofásico o
trifásico, la tensión del circuito elige la columna, interruptor de tiempo inverso, protección de la
Tabla 430-52 y conductor al 125 % (430-22).

### A/C y refrigeración — Art. 440

Se captura lo que trae la placa del equipo, no los HP. El motor copiado ya trae el cálculo:
`CalculadoraCarga440` — corriente base (la de carga nominal o la de selección del circuito, la
mayor, 440-6(a)), conductor al 125 % (440-32), protección: el mayor tamaño estándar que no pasa de
175 % (440-22(a) no permite redondear hacia arriba, a diferencia de 430-52), 225 % si se declara que
no arranca, y nunca menos de 15 A. Canalización, longitud, F.P. y caída, como cualquier renglón.

### En el alimentador y en el resumen

- Motor y A/C y refrigeración entran al mismo grupo, por fase, como en I-15: 125 % del mayor y 100 %
  de los demás (430-24, 440-33), con el techo de 430-62(a), que nombra la protección de 440-22(a).
- Alumbrado, Contactos, Equipo y Calefacción: continua al 125 % y no continua al 100 % (215-3), igual
  que hoy.
- El resumen lleva seis renglones por tipo, cada uno con su F.D. Debajo, aparte, **cómo entra al
  alimentador**: continua, no continua, y motores y A/C (430-24, 440-33). Los tres suman el total.
  Desaparece «Motores en HP», que se leía como un tipo más.

### Factor de demanda

Motor y A/C y refrigeración ofrecen las justificaciones que hoy tiene Motor / A/C (430-26, 220-60,
«Otra»). 220-60 queda donde se usa: entre el renglón de A/C y el de Calefacción.

## Lo que cambia en otras decisiones

- **R-17** (`../estado/HALLAZGOS.md`): la lista pasa de cinco a seis tipos. El F.D. por tipo sigue.
- **[`motores-art-430.md`](motores-art-430.md)**: HP deja de ser unidad de Motor / A/C, y «Art. 440,
  fuera» deja de valer. El cálculo del motor, 430-24 por fase y el techo de 430-62(a) siguen, y sus
  dos preguntas siguen abiertas.
- **Motor copiado**: `CalculadoraCarga440` se usa tal cual. Si se captura también la ampacidad mínima
  y la protección máxima (Por decidir 1), ese cálculo nace en la web y se anota en
  [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md): el escritorio
  (`RegimenCarga.AireAcondicionado440`) captura la corriente de carga nominal y la de selección.

## Lo que queda fuera

- **440-22(b)**: varios motocompresores, o un motocompresor con otras cargas, en un circuito (remite
  a 430-53). Igual que en el escritorio.
- **Acondicionador de cuarto con clavija (440, Parte G)**: se captura como A/C con su corriente de
  placa; los topes de 80 % y 50 % del circuito (440-62) no se revisan.
- **Motores que se calculan con la placa** (430-6(a)(1): baja velocidad, alto par, varias
  velocidades; Excepciones 2 y 3): se capturan en HP.
- **Sobrecarga** (430-32, 440-52): va en el arrancador o en el equipo; la memoria la dice, no la
  dimensiona.

## Por decidir

1. **A/C: qué se captura.** (a) La corriente de carga nominal y, si viene, la de selección del
   circuito, como el escritorio; (b) la ampacidad mínima del conductor y la protección máxima de la
   placa (440-4(b)); (c) las dos, según lo que traiga la placa. Recomendación: (c). Un minisplit o un
   equipo paquete traen (b); un compresor o una cámara, (a). Con (b), al alimentador entra la
   ampacidad mínima, del lado seguro: ya trae el 25 % de su motor mayor.
2. **Motor: solo HP**, con la FLC a la vista en el selector (recomendación), o también amperes,
   interpolando en la tabla como permite 430-6(a)(1).
3. **Archivos guardados.** Motor / A/C con HP pasa a Motor. Sin HP: (a) a A/C y refrigeración, con
   su carga convertida a amperes como corriente de placa y un aviso en el renglón para revisarla
   (recomendación); o (b) a Equipo, que conserva el número pero cambia de F.D.
4. **Nombres.** En el selector del renglón (108 px): «Motor» y «A/C y refrig.». En el resumen, el
   documento y la memoria: «Motores» y «Aire acondicionado y refrigeración».
