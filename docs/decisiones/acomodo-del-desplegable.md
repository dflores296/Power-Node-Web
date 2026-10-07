# El acomodo del desplegable: un renglón, sus columnas y el mínimo de la carga

**PROPUESTA · Claude · 2026-10-07** — las preguntas las armó Claude; David las va contestando una por una.
Lo contestado va abajo con sus palabras. Falta que David la marque CONFIRMADA. Nada de esto está
implementado todavía: primero se cierran las decisiones, luego un mockup, luego el código.

Es un tema de **acomodo, no de cálculo** (David). Sigue a
[`captura-en-el-desplegable.md`](captura-en-el-desplegable.md), que puso el equipo solo como un renglón
especial; D1 lo cambia.

## Las dos reglas (David, 2026-10-07)

- **R1.** «Todo lo que sea casilla, marcable, combo box, entrada, selector, que afecte el cálculo va en
  columna propia, porque son propiedades a definir de la carga que irá en esa salida.» Nada de controles
  en el pie del desplegable: «esa zona inferior no lleva controles».
- **R2.** «El orden de las columnas va de lo general a lo específico, de izquierda a derecha.»

## D1 · Qué es un renglón — CONTESTADA (David, 2026-10-07)

**Antes:** dos significados en la misma tabla. En cargas, un renglón son N salidas idénticas (Cant. ×
Carga c/u). Con un solo equipo (motor, variador, motocompresor, A/A, de habitación), el renglón azul era
el circuito mismo, con Cant. fija en 1.

**Decidido:** un solo significado. **Cada renglón es un aparato definido una vez, y «Cant.» dice cuántos
de esos aparatos van en el circuito**; cada uno en su propia salida. El renglón azul especial desaparece.

- **D1.1 · Motores de uso general:** como los contactos (David: «Sí»). Tres motores idénticos: un renglón
  con Cant. 3. Tres idénticos y uno más grande o más chico: dos renglones, Cant. 3 y Cant. 1. Nunca más de
  un motor por salida; sí varios en el mismo circuito (430-53).
- **D1.2 · Variador:** uno por renglón, Cant. fija en 1, aunque sean idénticos (David: «un variador solo
  puede ser una salida; si tienes más de un variador aunque sean idénticos tendría que ir cada uno en su
  renglón»). Es regla de captura: el cálculo no lo exige.
- **D1.3 · Motocompresor y acondicionador de habitación:** Cant. libre «siempre que sean idénticos y que
  la norma lo permita» (David). Lo que la norma permite ya lo revisa el cálculo del grupo: 430-53(a), (b) y
  (c), 440-22(b), y los límites de 440-62 para los de habitación.
- **D1.4 · Qué es «idéntico»:** no se vigila. David: «defino el aparato una vez y defino la cantidad de
  esos aparatos que voy a usar. No creo que necesites vigilar cada copia porque no son copias: es un solo
  renglón que cambia de cantidad y hace la suma.» Dos renglones iguales se calculan igual que uno con la
  suma de sus cantidades.

**El cálculo ya lo soporta** (revisado el 2026-10-07): en el grupo, el conductor lleva la suma de todas las
unidades más el 25 % de una sola, la mayor (`CalculadoraCircuitoDerivadoGrupo`, 430-24 y 430-53); en el
alimentador cada unidad entra por separado (`Enumerable.Repeat` por la cantidad). Lo que cambia es la
pantalla.

## Pendientes, en orden

Detalle de cada una (cómo está hoy, opciones, recomendación) en la conversación del 2026-10-07; aquí, lo
que hay que decidir:

- **D2.** Cant. de 1 a 2 o más en un motor: el cálculo pasa de motor solo (430-22, 430-52) a grupo
  (430-53). ¿Cambio automático visible en el renglón del circuito, con confirmación, o fija?
- **D3.** ¿Siguen con Cant. bloqueada el contacto del refrigerador, el A/A «Carga combinada» y otro
  tablero?
- **D4.** ¿Una columna aparece solo si alguna línea del circuito la usa, o siempre?
- **D5.** Una celda, un dato: separar «Entrada · Máx.», «Nominal · Selección», «Ampac. · Máx.» y «HP +
  Placa A» en columnas propias.
- **D6.** ¿Columnas compartidas por concepto («Corriente de placa (A)», «Protección máxima (A)») o una por
  equipo?
- **D7.** «Unidad» en variador, motocompresor y A/A: ¿«—» o «A» como texto?
- **D8.** Bypass y HP del motor del variador: columnas propias; ¿HP apagada hasta marcar el bypass?
- **D9.** «No arranca con la Tabla 430-52 (400 %)» y «Terminal 75 °C»: columnas propias, solo con motor
  solo.
- **D10.** «Arranque 225 %» del motocompresor: columna propia; en grupo, ¿solo activa en el mayor?
- **D11.** «No simultáneo con» (es del circuito, no de una carga): ¿columna del cuadro, primera línea del
  desplegable, o banda encima?
- **D12.** Los textos fijos de «Servicio» («motor mayor · 125 %», «440-62», «—»): ¿se quedan?
- **D13.** El orden de las columnas (R2).
- **D14 a D19.** El mínimo de 220-14 en «Carga c/u»: llenarlo al escoger el subtipo; qué pasa si se escribe
  menos; W o A; la secadora al cambiar el inmueble; anuncios (mínimo por circuito); archivos con 0.
- **El scroll del desplegable:** David lo probó y está mal; se ve después del acomodo.
