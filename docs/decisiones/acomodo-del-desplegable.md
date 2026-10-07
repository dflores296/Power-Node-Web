# El acomodo del desplegable: un renglón, sus columnas y el mínimo de la carga

**PROPUESTA · Claude · 2026-10-07** — las preguntas las armó Claude; David las va contestando una por una.
Lo contestado va abajo con sus palabras. Falta que David la marque CONFIRMADA. Implementado todo: R1, R2 y D1 a D13
(I-201, `8782a79`), D14 a D19 (I-203, `e52fd74`) y los avisos (I-202, `1f97c39`).

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

## D2 · Cant. de un motor de 1 a 2 o más — CONTESTADA (David, 2026-10-07)

David: «¿por qué saturar de avisos y de artículos la tabla, si puedes ir a la ventana de memoria y ver
exactamente con qué artículo se está calculando ese circuito de acuerdo a su carga?»

**Decidido** («Sí»): al cambiar la cantidad, el cálculo se ajusta solo, sin aviso, confirmación ni cita. La
tabla enseña el resultado; el porqué y su artículo, la memoria de cálculo. Lo capturado no se pierde: una
protección fijada en «Protec. (A)» del motor solo se guarda y vuelve al regresar a Cant. 1; las dos casillas
del motor solo se ven apagadas mientras sea grupo, con su marca, y con una ayuda corta al pasar el cursor
(David: «ayuda corta está bien»), sin artículo: «Solo con un motor solo». La celda «Tipo» del circuito
cambia como hoy («Grupo de motores · 3 salidas»).

## D3 · Los renglones con Cant. fija en 1 — CONTESTADA (David, 2026-10-07)

David: «1 sí, 2 sí, 3 A». Con Cant. fija en 1, cada uno en su renglón:

- **El contacto del refrigerador** — va solo en su circuito y es uno (210-52(b)(1) Excepción 2). Si no:
  «El contacto del refrigerador va solo. Pasa las demás cargas a otro circuito.» / «… es uno. Deja la
  cantidad en 1.» (C12, C13 de los avisos).
- **El A/A «Carga combinada»** — la ampacidad mínima y la protección máxima de su placa son de un equipo.
- **Otro tablero** — cada tablero con su nombre, su carga y su protección (408-36).
- **El variador** — ya en D1.2.

Todos los demás aparatos, Cant. libre (D1).

## D4, D5, D6 y D7 · Las columnas: un bloque por juego de columnas — CONTESTADAS (David, 2026-10-07)

David: «Hay una fila de encabezados hasta arriba; si todos los tipos de carga en los renglones son los mismos,
comparten esa fila de encabezados de columna. Si un renglón tiene sus propias columnas, no vas a atiborrar
todas: vas a agregar entre cada carga un nuevo renglón de encabezados correspondiente a cada carga.»

- **El desplegable se arma por bloques.** Cada bloque es una fila de encabezados y los renglones que llevan
  ese mismo juego de columnas. Con un solo juego, un solo encabezado, como hoy. Nada de columnas de otra
  carga con «—».
- **Qué comparte bloque** (1 A): el mismo juego de columnas, no el mismo «Tipo». Un motor de uso general y
  un variador son «Motores», pero van en bloques distintos.
- **Orden** (2 A): los renglones se agrupan solos por juego de columnas; un bloque y un encabezado por
  juego, aunque se capturen intercalados.
- **Alineación** (3 A): Descripción, Tipo, Subtipo y Cant. van primero en todos los bloques y miden lo mismo,
  una debajo de otra; Descripción, alineada con la del circuito (I-200). Lo que cambia de bloque a bloque son
  las columnas de la derecha.
- **D5 (una celda, un dato):** lo resuelve R1. Las celdas de dos casillas se separan en columnas.
- **D6 (nombres):** cada bloque nombra sus columnas con precisión: Corriente de placa (A) del motor,
  Corriente nominal (A) y Corriente de selección (A) del motocompresor, Corriente total (A) del de
  habitación, Corriente de entrada (A) y Prot. máx. del fabricante (A) del variador, Ampacidad mínima (A) y
  Prot. máx. de placa (A) del A/A «Carga combinada». Las siglas de placa (FLA, RLA, MCA, MOCP), en la ayuda.
- **D7 («Unidad»):** solo la lleva el bloque de un subtipo con algo que escoger: cargas (VA / W / A) y motor
  de uso general (HP / A). Variador, motocompresor y A/A no la llevan: la unidad va en el encabezado.
- Dentro de un bloque, una columna que no aplica por otro dato se ve **apagada**, con su ayuda corta
  («HP del motor» sin bypass, «Corriente de placa» en un motor continuo).

Antes se había contestado D4 como «la unión de las columnas de los subtipos del circuito»; los bloques la
reemplazan.

## D8 · Bypass y HP del motor del variador — CONTESTADA (David, 2026-10-07: «A, A»)

- Las dos, columnas del bloque del variador. «HP del motor» se ve **apagada** hasta marcar «Bypass» (ayuda:
  «Solo con bypass.»); al marcarlo se enciende y, vacía, se pone en rojo: «Faltan los HP del motor.».
- **Un variador dentro de un grupo:** «Bypass» y «HP del motor» se ven apagadas (ayuda: «Solo con el variador
  solo en su circuito.»). Lo que no cuenta no se deja marcar; la advertencia B17 de los avisos («El bypass no
  cuenta en un grupo…») ya no hace falta.

## D9 y D10 · Las casillas del motor solo y el 225 % — CONTESTADAS (David, 2026-10-07: «D9 sí, D10 A»)

- **D9:** «No arranca con la protección de tabla» y «Terminal 75 °C», columnas del bloque de motor de uso
  general; apagadas cuando el circuito no es un motor solo, con su marca guardada (D2). El cálculo no cambia:
  la de 75 °C sigue siendo solo del motor solo.
- **D10:** «Arranque 225 %», columna del bloque del motocompresor; en un grupo, solo activa en el renglón del
  mayor (440-22(b)(1)); en los demás, apagada: «Solo en el motocompresor mayor.».

## D11 a D19 — CONTESTADAS (David, 2026-10-07: «Sí a todo»): las recomendaciones de Claude

- **D11 · «No simultáneo con»:** una columna en el cuadro principal, en el renglón del circuito (es dato del
  circuito, no de una carga). Sale del pie del desplegable.
- **D12 · Los textos fijos de «Servicio»** («Continuo» en los motores de un grupo, «motor mayor · 125 %»,
  «variador»): se quedan.
- **D13 · El orden de las columnas:** el del mockup. Descripción · Tipo · Subtipo · Cant. · [las del bloque,
  de lo general a lo específico: tensión, unidad, carga o HP, corrientes, protección máxima, servicio, casillas
  del equipo] · F.P. · Total · quitar.
- **D14 · El mínimo de 220-14 en «Carga c/u»:** se llena al escoger el subtipo, y no se puede dejar debajo.
- **D15 · Si se escribe menos:** sube solo al mínimo, con un aviso: «Contacto: no menos de 180 VA.»
- **D16 · W o A:** contactos, portalámparas de servicio pesado y ensamble de salidas, solo en VA; la secadora
  deja escoger y el mínimo se convierte.
- **D17 · La secadora y el inmueble:** si su carga sigue siendo el mínimo que se llenó solo, se actualiza al
  cambiar el inmueble; si se escribió otra, no se toca.
- **D18 · Anuncios (mínimo por circuito):** se llena con 1,200 VA si es la única línea de anuncios del circuito;
  con varias, no se llena y queda el piso del cálculo.
- **D19 · Archivos con la carga en 0:** al abrirlos, el campo enseña el mínimo; el resultado no cambia.

## R3 · Los artículos en la tabla — REEMPLAZADA por la guía de David

David escribió cómo se redactan los avisos: [`redaccion-de-avisos.md`](redaccion-de-avisos.md) (2026-10-07).
Lo de abajo queda como historia.

### Antes (en discusión)

David no la quiere como «sin artículos»: «tampoco podemos dejar pelona la tabla, hay que ayudar pero ser
discretos, que la tabla y la memoria hagan sinergia». Pidió el inventario de todo lo que la tabla dice con un
artículo, para decidir cada uno: [`../conocimiento/avisos-de-la-captura.md`](../conocimiento/avisos-de-la-captura.md),
con la propuesta de Claude (tres niveles y un puente a la memoria) en cada renglón.

## El mockup

[`../mockups/desglose-por-bloques.html`](../mockups/desglose-por-bloques.html) (capturas a 1900 y 1440 px), 2026-10-07,
segunda versión con las observaciones de David: el título del desplegable arriba y el nombre de cada bloque
en su primera celda («Cargas», «Motores», «Motocompresores», «Variadores»); el sobrante repartido entre las
columnas de la derecha (Descripción, Tipo, Subtipo y Cant., iguales en todos los bloques); todos los bloques
de un desplegable del mismo largo, hasta el borde (David: «¿por qué tienen diferente longitud y les faltan
bordes?» — el tope de 40 % los dejaba cortos);
si no caben, el bloque se recorre de lado; el error, dentro del renglón del circuito; las notas, fuera de la
tabla. Los estados de error y advertencia: [`../conocimiento/estados-de-aviso.md`](../conocimiento/estados-de-aviso.md).

## Pendientes, en orden

- **Los avisos de la tabla:** el texto final de cada uno, aprobado por David (2026-10-07), por verificar
  con un agente que haga de usuario en el navegador —
  [`../conocimiento/avisos-de-la-captura.md`](../conocimiento/avisos-de-la-captura.md). Falta la segunda entrega
  (el resto de la página de captura).
- **El scroll del desplegable:** David lo probó y está mal; se ve después del acomodo.
