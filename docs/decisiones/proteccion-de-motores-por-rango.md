# Protección del derivado de motor: rango permitido y criterio del proyectista

**PROPUESTA · Claude · 2026-10-03** — hallazgo **M-20**, de la auditoría NOM del 2026-10-02/03. No
implementada. Cambia P-5 de [`../conocimiento/requisitos.md`](../conocimiento/requisitos.md):
**decisión de David**. Verificada contra el motor en `e0eb887`; dos correcciones al algoritmo (pasos 2,
7 y 8) — ver [Verificación](#verificación-contra-el-código-claude--2026-10-03). Revisada contra el texto
de la NOM a pedido de David: 430-62(a) corregido y opciones para cada pregunta — ver
[Preguntas](#preguntas-para-david-con-opciones).

## El problema

La protección del derivado de un motor **no se puede escoger**. La app toma siempre el **máximo** que
permite la Tabla 430-52 y lo pone como «la protección seleccionada». La casilla «Protec. (A)» del
renglón solo muestra ese valor.

**Caso que lo destapó** (David, 2026-10-03): bomba de cisterna de 1/2 HP, 1 polo, 127 V, 15 m.

| Concepto | Valor | Referencia |
|---|---|---|
| FLC | 8.90 A | Tabla 430-248, columna de 127 V |
| Conductor | 125 % × 8.90 = 11.13 A → **14 AWG** (15 A a 60 °C) | 430-22, 110-14(c)(1) |
| Techo | 250 % × 8.90 = 22.25 A → **25 A** (inmediato superior) | Tabla 430-52, 430-52(c)(1) Exc. 1 |
| La app pone | **25 A** sobre 14 AWG | — |
| En campo se pone | **15 A** sobre 14 AWG | — |

Los 25 A **cumplen**, pero no son la única protección que cumple. Presentarlos como el resultado
tiene tres problemas:

1. **Se lee como mínimo.** Quien revisa la memoria cree que 15 A no cumple, y sí cumple.
2. **Deja el conductor sin protección contra sobrecarga.** 14 AWG con 25 A solo es válido por
   240-4(g): la norma lo permite porque supone que la sobrecarga la cuida otro dispositivo (430-32).
   Una bomba de cisterna arranca sola, con flotador o presostato. Si no trae protector térmico
   integrado ni relevador, con 25 A nada protege al motor ni al cable contra sobrecarga.
   *Matiz (revisión del texto, 2026-10-03):* 430-31 dice que el dispositivo contra sobrecarga de la
   Parte C (430-32) protege «a los motores, aparatos de control y **conductores** de los circuitos
   derivados de motores». Con 430-32 cumplido, 25 A sobre 14 AWG cumple y el cable sí queda protegido;
   el problema es la instalación que no lo trae, que ya no cumple 430-32(b).
3. **Arrastra todo lo que depende de esa protección.** Con el máximo sale más grande:
   - la tierra, que se entra a la Tabla 250-122 con la protección del derivado (250-122(d)(1));
   - el techo del principal por 430-62(a), según cómo se lea (pregunta 6);
   - el aviso de «principal menor que el derivado» (A-4).

## Lo que dice la NOM

| Sección | Texto (resumido) | Consecuencia |
|---|---|---|
| 430-52(b) | La protección «debe ser capaz de soportar la corriente de arranque del motor» | **Obligación**, en todo motor. Es el único piso literal, y la app no lo puede verificar (no tiene la curva del interruptor) |
| 430-52(c)(1) | La protección debe tener un valor «que **no exceda**» el de la Tabla 430-52 | Es un **techo**, no un valor obligatorio |
| 430-52(c)(1) Exc. 1 | Si el techo no es normalizado, «se **permitirá**» el inmediato superior | Permiso para subir, no obligación |
| 430-52(c)(1) Exc. 2 | Si el motor no arranca, se permite subir (hasta 400 %, o 300 % arriba de 100 A, con interruptor de tiempo inverso) | Ya está: la casilla «No arranca con la Tabla 430-52» |
| 430-22 | Conductor al 125 % de la FLC | **Piso** práctico para la protección: abajo de eso puede dispararse en operación normal |
| 240-4 | El conductor se protege según su ampacidad | La regla general, que el motor puede seguir si el proyectista quiere |
| 240-4(g) | Los conductores de motor se pueden proteger arriba de su ampacidad según el Art. 430 | **Permiso**, no exigencia |
| 430-32 | El motor lleva protección contra sobrecarga propia (relevador o protector térmico) | (a) arriba de 1 HP, siempre; (b) de 1 HP o menos con arranque automático, también. El interruptor del tablero no la da |
| 430-31 | La sobrecarga de la Parte C protege motores, controles **y conductores** del derivado | Con 430-32 cumplido, el conductor está protegido aunque P pase su ampacidad |
| 430-55 | Se permite que un solo dispositivo dé cortocircuito y sobrecarga si su valor cumple 430-32 | El interruptor sería la sobrecarga solo a ≤ 115–125 % de la corriente de **placa** (140 % con 430-32(c)); con los tamaños normalizados casi nunca cabe, y la app no captura la placa de un motor en HP |
| 110-14(c)(1)a.(4) | Motor de diseño B, C, D o E: se permiten conductores de 75 °C aunque el circuito sea de 100 A o menos | La terminal del motor deja de forzar 60 °C; la del interruptor sigue pidiendo su marca (a.(3), la casilla que ya existe) |
| 430-62(a) | El alimentador no excede la mayor protección de derivado + las FLC de los demás, «[con base en el **valor máximo permitido** para el tipo específico de uno de los dispositivos protectores de acuerdo con 430-52…]» | Leído literal, entra **Pmáx**, no la protección elegida — corrige la versión anterior de esta tabla; pregunta 6 |
| 430-63(1) | Alimentador con motor y otra carga: para un solo motor, «el valor nominal **permitido** en 430-52» | Igual que 430-62(a): Pmáx |
| 250-122(d)(1) | Tierra «con base en el valor nominal del dispositivo de protección … del circuito derivado» | Usa la protección **elegida**. Contrasta con (d)(2), que cuando quiere el máximo lo dice: «valor nominal máximo permitido» |

**Conclusión normativa:** la protección del derivado de un motor es un **rango**:

```
Pmín (criterio práctico: ≥ 125 % FLC)  ≤  P  ≤  Pmáx (Tabla 430-52, Exc. 1; o Exc. 2 si se declara)
```

Dentro del rango decide el ingeniero. La NOM no exige el máximo.

## La propuesta

### 1. Tres criterios por circuito de motor

| Criterio | Cómo sale P | Cuándo conviene |
|---|---|---|
| **Prioridad al conductor** | El mayor valor de la serie que protege al conductor por 240-4, sin bajar de Pmín ni pasar de Pmáx | Motores chicos, bombas, ventiladores; cuando no se sabe si trae protector térmico |
| **Máximo 430-52** (lo de hoy) | El mayor valor de la serie que no excede Pmáx | Motores que arrancan con carga pesada; cuando ya se probó que el otro no aguanta el arranque |
| **Manual** | El que escoja el proyectista, de una lista con solo los valores de la serie dentro del rango | Cuando el ingeniero tiene la curva del interruptor o el dato del fabricante |

### 2. El cálculo de «Prioridad al conductor»

Hoy `CalculadoraCircuitoDerivadoMotor` escoge la protección (paso 3) **antes** del conductor
(pasos 6–8), y la temperatura de terminales (paso 4) sale de esa protección. Con prioridad al
conductor el orden cambia:

1. **FLC y capacidad mínima**, igual que hoy (430-6(a), 430-22 o 430-22(e)).
2. **Pmín**: el menor valor de la serie que sea ≥ 125 % de la FLC. Con servicio no continuo, ≥ la
   capacidad mínima de 430-22(e). **Nunca arriba de Pmáx**: si ese valor excede Pmáx, Pmín = Pmáx.
   Pasa en riel DIN con un motor de FLC ≤ 6 A: 1/4 HP a 127 V (5.30 A) tiene Pmáx = 15 A de la lista
   de 240-6(a), fuera de la serie (como hoy), y la serie empieza en 16 A. El rango es solo 15 A.
3. **Pmáx**: igual que hoy (`ProteccionDeLaTabla430_52`), con la Excepción 2 si está marcada.
4. **Columna de terminales**: la de Pmín (110-14(c)(1)), o 75 °C si se declararon terminales
   marcadas 75 °C. No cambia después (paso 8).
5. **Calibre base**: el de hoy por ampacidad (125 % FLC contra la columna de terminales, con
   factores). Es el calibre **antes** de subir por caída de tensión o piso práctico.
6. **P** = el mayor valor de la serie que protege al calibre base por 240-4:
   - P ≤ ampacidad utilizable del calibre base; o
   - el inmediato superior por 240-4(b), si la ampacidad no es valor normalizado y P ≤ 800 A;
   - sin pasar el tope de 240-4(d) (14 AWG → 15 A, 12 AWG → 20 A, 10 AWG → 30 A, cobre).
7. **Recorte al rango** (Pmín ≤ Pmáx por el paso 2, así que el orden no altera el resultado):
   - Si P < Pmín (la ampacidad corregida quedó por debajo, por temperatura o agrupamiento), entonces
     P = Pmín y se sube el calibre hasta que quede protegido por 240-4, citándolo.
   - Si P > Pmáx, entonces P = Pmáx. **Pmáx manda siempre**: es la NOM; Pmín es criterio práctico.
8. **P se queda en la regla de 110-14(c)(1) de su columna, sin iterar.** Con la columna de 60 °C
   (Pmín ≤ 100 A, sin terminales marcadas 75 °C), P no pasa de 100 A: el mayor valor de la serie
   ≤ 100 A. Con Pmín > 100 A, P ya pasa de 100 A y la columna es la de 75 °C desde el principio.

   *Por qué no se itera* (la versión anterior recalculaba con la columna nueva hasta 3 vueltas): el
   ciclo no termina. 30 HP, 3 polos, 220 V: FLC 80 A, 125 % = 100 A, Pmín = 100 A → 60 °C → 1 AWG
   (110 A) → P = 110 A → 75 °C → 3 AWG (100 A) → P = 100 A → 60 °C → 1 AWG… Con el tope, P = 100 A
   con 1 AWG. Además, sin agrupamiento, una P > 100 A nunca protege al calibre base de 75 °C cuando
   Pmín ≤ 100 A: ese calibre lleva a lo más 100 A (3 AWG). Con agrupamiento sí puede (3/0 con factor
   0.50 lleva 112.5 A); el tope lo deja en 100 A, que sigue cumpliendo y es menor. Ver pregunta 5.
9. **Caída de tensión**, piso práctico y paralelos: igual que hoy, con `corrienteParaCaidaA` = FLC.
   Subir el calibre por caída **no** sube P: el conductor más grueso sigue protegido.
10. **Tierra**: 250-122(d)(1) con **P**. 250-122(b) y (a), igual que hoy.

### 3. Manual

- El selector ofrece solo los valores de la serie del tablero (centro de carga, riel DIN o NOM
  completa) entre Pmín y Pmáx. Con la Excepción 2 marcada, hasta su techo. Si Pmáx está fuera de la
  serie (riel DIN, paso 2), ofrece solo Pmáx.
- La columna de terminales sale de P, como hoy: con P > 100 A, 75 °C.
- Si P protege al conductor por 240-4: nota «Conductor protegido por el interruptor — 240-4».
- Si P pasa la ampacidad del conductor: nota «Permitido por 240-4(g): la sobrecarga debe darla el
  arrancador o el protector térmico del motor — 430-32».
- Si un archivo trae un valor fuera de la serie o del rango, se abre con el más cercano dentro del
  rango y se avisa en la apertura (el patrón del F.P. fuera de rango, I-80 —
  `CircuitoJson.FueraDeRango`).

### 4. Pantalla

- **Renglón**: la celda «Protec. (A)» de un circuito de motor muestra el valor elegido y, abajo en
  chico, `máx. 25` (o `25 — máx. 430-52` cuando es el máximo).
- **Desplegable del motor**, junto a la casilla «No arranca con la Tabla 430-52»:
  - selector **Criterio de protección**: `Prioridad al conductor · Máximo 430-52 · Manual`;
  - con *Manual*, un selector de valor con la lista del rango.
- **Tooltip / desglose** (`DesgloseDeSeleccion`): rango completo, criterio y por qué salió P.
- **Encabezado de la columna**: el título de hoy («primer tamaño normalizado mayor o igual a la
  capacidad mínima») no describe al motor; agregar «en motores, dentro del rango de 430-52».

### 5. Memoria (sección 3 del circuito de motor)

Reemplazar «Protección seleccionada — 430-52(c)(1) Excepción 1: 25 A, el valor inmediato superior:
22.25 A no es valor normalizado de 240-6(a)» por:

```
Rango permitido — 430-52(c)(1)   15 A (≥ 125 % × 8.90 A = 11.13 A) a 25 A (250 % × 8.90 A = 22.25 A → Exc. 1)
Criterio                          Prioridad al conductor: el mayor valor que protege 14 AWG (15 A) — 240-4, 240-4(d)
Protección seleccionada           15 A
Sobrecarga del motor              Relevador o protector térmico — 430-32. El interruptor no la da.
Arranque                          Verificar con la curva del interruptor que el motor arranca. Si no: subir de criterio
                                  (hasta 25 A con este conductor, 240-4(g)) o marcar la Excepción 2.
```

### 6. Lo que cambia aguas abajo

- **250-122(d)(1)**: tierra con P. El 100 HP a 480 V (columna de 460 V) pasa de 2 AWG (con 350 A) a
  6 AWG (con 175 A).
- **430-62(a) y 430-63**: según la pregunta 6. Leído literal, entra Pmáx y el techo del principal no
  cambia con el criterio (`AgregadoMotores` / `CuadroDeCarga`, `MayorProteccionDerivadoA`; hoy recibe
  la protección instalada, que con el máximo casi siempre es lo mismo — salvo en riel DIN: 32 A
  instalados contra 35 A de máximo).
- **A-4**: el aviso de principal menor que el derivado compara contra P: son los dos interruptores que
  se instalan.
- **Gabinete y documento**: el interruptor dibujado e impreso es P.

### 7. Archivo — formato 12

| Campo nuevo en `CircuitoJson` | Valores | Ausente |
|---|---|---|
| `criterioProteccion` | `Conductor` · `Maximo430_52` · `Manual` | `Maximo430_52` |
| `proteccionElegida` | amperes, solo con `Manual` | — |

**Ausente = `Maximo430_52`**, para que un tablero ya guardado (formato ≤ 11) abra con los **mismos
números**: la memoria de un tablero entregado no debe cambiar al abrirlo (mismo criterio que en
[`factores-de-demanda-del-articulo-220.md`](factores-de-demanda-del-articulo-220.md)).

El criterio de un **circuito nuevo** lo decide David (pregunta 1).

### 8. Después, con el mismo patrón (fase 2)

La misma situación —un techo presentado como la selección— existe en:

| Caso | Techo | Hoy |
|---|---|---|
| A/C por corriente nominal | 175 % (225 % si no arranca) — 440-22(a), «no exceda» | El mayor que no excede |
| A/C por placa | MOCP — 440-4(b) | Igual a la MOCP |
| Grupo de motores | 430-53(c)(4) | El mayor que no excede |
| Variador | Protección máxima del fabricante — 430-122, 110-3(b) | El mayor que no excede |

Se hace después de que David confirme la de motores. Es el mismo selector y la misma lógica de
rango.

## Ejemplos (para las pruebas)

Cobre, THHN, lugar seco, 30 °C, sin agrupamiento, centro de carga NEMA, longitud corta (sin subir
por caída), terminales por la regla general.

| Motor | FLC | Pmín | Calibre base | Prioridad conductor | Máximo 430-52 (hoy) | Tierra con prioridad |
|---|---|---|---|---|---|---|
| 1/2 HP, 1 polo, 127 V | 8.90 A | 15 A | 14 AWG (15 A) | **15 A** | 25 A | 14 AWG |
| 1 HP, 1 polo, 127 V | 14.00 A | 20 A | 12 AWG (20 A) | **20 A** | 35 A | 12 AWG |
| 5 HP, 3 polos, 220 V | 15.20 A | 20 A | 12 AWG (20 A) | **20 A** | 40 A | 12 AWG |
| 10 HP, 3 polos, 220 V | 28.00 A | 35 A | 8 AWG (40 A) | **40 A** | 70 A | 10 AWG |
| 30 HP, 3 polos, 220 V | 80.00 A | 100 A | 1 AWG (110 A a 60 °C) | **100 A** (tope del paso 8) | 200 A, con 3 AWG a 75 °C | 8 AWG |
| 100 HP, 3 polos, 480 V (NOM completa) | 124.00 A | 175 A | 2/0 AWG (175 A a 75 °C) | **175 A** | 350 A | 6 AWG |

**Ojo con el 30 HP:** con prioridad al conductor sale **más cobre** que hoy (1 AWG contra 3 AWG). Con
P ≤ 100 A el circuito cae en la regla de 60 °C de 110-14(c)(1)a.; hoy, con 200 A, va a 75 °C. Es
correcto, pero sorprende: pregunta 5.

Casos de borde:

- **Agrupamiento que tumba la ampacidad.** 1/2 HP a 127 V en un tubo con 12 portadores a 40 °C:
  14 AWG corregido = 25 × 0.91 × 0.50 = 11.38 A. Cumple 430-22 (≥ 11.13 A), pero queda abajo de
  Pmín (15 A) y ningún valor de la serie es ≤ 11.38 A. Por 240-4(b) se permite 15 A (11.38 A no es
  valor normalizado): P = 15 A con 14 AWG, citando 240-4(b).
- **Agrupamiento con un motor más grande.** 1 HP a 127 V en el mismo tubo: 12 AWG corregido =
  30 × 0.91 × 0.50 = 13.65 A, no cumple 430-22 (17.5 A). El calibre base sube a 10 AWG (18.2 A), y
  por 240-4(b) P = 20 A = Pmín.
- **P abajo de Pmín sin 240-4(b).** Si la ampacidad corregida del calibre base es un valor
  normalizado menor que Pmín, 240-4(b) no aplica: se sube el calibre hasta que proteja Pmín, con
  cita 240-4. Armar la prueba con una ampacidad corregida que dé exacto un valor de la serie.
- **Tope de 100 A.** 30 HP a 220 V: la ampacidad del calibre base (1 AWG, 110 A a 60 °C) dejaría
  P = 110 A, pero Pmín = 100 A pone la columna en 60 °C: P = 100 A con 1 AWG, sin iterar (paso 8).
- **Rango sin valores de la serie.** Riel DIN, 1/4 HP a 127 V: Pmáx = 15 A de la NOM (fuera de la
  serie), Pmín = 15 A (no 16): P = 15 A con 14 AWG en los tres criterios.
- **Manual fuera de la serie.** En riel DIN, un archivo con 35 A abre con el valor más cercano del
  rango y avisa.
- **Archivo de formato 11.** Abre en `Maximo430_52` con los mismos números de antes (regresión).

## Pruebas a agregar (`tests/PowerNode.Web.Tests`)

| Prueba | Verifica |
|---|---|
| `M20_ElRangoVaDel125DeLaFlcAlTechoDe430_52` | Pmín y Pmáx de la tabla de ejemplos |
| `M20_PrioridadAlConductor_LaBombaDeMedioHpVaEn15A` | 1/2 HP 127 V → 15 A, 14 AWG |
| `M20_PrioridadAlConductor_DaLaTablaDeEjemplos` | Teoría con los cinco renglones |
| `M20_ConAgrupamientoSubeElCalibreYNoBajaDePmin` | El caso de borde de agrupamiento |
| `M20_ConPminHasta100ALaProteccionNoPasaDe100A` | 30 HP a 220 V → 100 A con 1 AWG, sin ciclo |
| `M20_ElPisoNoPasaDelTecho` | Riel DIN, 1/4 HP a 127 V → 15 A, no 16 |
| `M20_ManualSoloOfreceValoresDeLaSerieDentroDelRango` | NEMA sin 16/32/63; riel DIN |
| `M20_ManualArribaDeLaAmpacidadCita240_4gY430_32` | La nota de sobrecarga |
| `M20_LaTierraUsaLaProteccionElegida` | 250-122(d)(1) con P |
| `M20_El430_62aUsa…` | El techo del principal, con Pmáx o con P según la pregunta 6 |
| `M20_UnArchivoDeFormato11AbreConElMaximo` | Compatibilidad: mismos números |
| `M20_ConLaExcepcion2ElRangoLlegaA400` | La casilla actual sigue funcionando en los tres criterios |

## Documentos a actualizar

- **`requisitos.md`**: reescribir **P-5** y agregar el criterio de protección.

  > P-5. Calcular el rango de la protección del derivado de un motor: del menor valor de la serie ≥
  > 125 % de la FLC al techo de la Tabla 430-52 (Excepción 1; Excepción 2 si se declara). Escoger
  > dentro del rango por el criterio del circuito: prioridad al conductor (240-4), máximo 430-52 o
  > manual. Tierra con la protección elegida; 430-62(a) según la pregunta 6.

- **`HALLAZGOS.md`**: M-20 con prioridad P2.
- **`POR-VERIFICAR.md`**: el supuesto «Pmín = 125 % de la FLC es criterio práctico, no de la NOM»,
  y «la app no verifica el arranque contra la curva del interruptor».
- **`TABLERO.md`**: el frente de motores.
- **`motores-art-430.md`**: nota de que esta decisión reemplaza la selección «el mayor que no
  excede el máximo».
- **`archivo-del-tablero.md`**: el formato 12.

## Lo que se pierde

- **El arranque no se verifica.** La app no tiene la curva del interruptor ni la corriente de
  arranque del motor. Con prioridad al conductor, un motor grande o con arranque pesado puede
  disparar el interruptor. La memoria lo dice y el proyectista sube de criterio si hace falta.

  A futuro: con la letra de código de la placa y la Tabla 430-7(b), que ya está en `tablas-nom.json`,
  se podría estimar la corriente de rotor bloqueado y avisar cuando P sea muy chica frente a ella.

- **Un selector más** en el desplegable del motor.

## Verificación contra el código (Claude · 2026-10-03)

Corrida con `CalculadoraCircuitoDerivadoMotor` en `e0eb887` (cobre, THHN, seco, 30 °C, PVC, 5 m):

- **Cuadran:** FLC, «Máximo 430-52 (hoy)» y el calibre de hoy de los cinco renglones originales; la
  tierra de hoy del 100 HP (2 AWG con 350 A; la Tabla 250-122 de la NOM da 2 AWG para 400 A, no 3 AWG
  como el NEC); los dos casos de agrupamiento (14 AWG con 11.38 A; 10 AWG con 18.2 A); la bomba a
  15 m (14 AWG, 1.95 %).
- **Corregido en este documento:**
  1. Pmín podía quedar arriba de Pmáx (riel DIN, motor de FLC ≤ 6 A), y el recorte del paso 7, en
     el orden en que estaba, dejaba P = 16 A sobre un techo de 15 A. Ahora Pmín ≤ Pmáx (paso 2) y
     Pmáx manda (paso 7).
  2. La iteración de terminales del paso 8 no terminaba con el 30 HP a 220 V (oscilaba entre 1 AWG
     con 110 A y 3 AWG con 100 A). Ahora P se topa en 100 A cuando la columna es de 60 °C.
  3. Detalles: 100 HP «a 460 V» → a 480 V (columna de 460 V), como en la tabla de ejemplos; el texto
     de la memoria que se reemplaza es el de la Excepción 1, no «igual al máximo»; el aviso al abrir
     un archivo sigue el patrón de I-80, no el de I-144 (que es de la casilla).
- **Revisión del texto de la NOM** (a pedido de David, el mismo día): 430-62(a) y 430-63 dicen «valor
  máximo permitido» y «valor permitido en 430-52», no la protección elegida: la tabla de la NOM decía lo
  contrario y se corrigió (pregunta 6, nueva). Se agregaron 430-52(b) (soportar el arranque es
  obligación), 430-31 (la sobrecarga de 430-32 también protege al conductor), 430-55 y
  110-14(c)(1)a.(4). Con eso, la recomendación de la pregunta 1 pasa de A a C.

## Preguntas para David, con opciones

Revisadas contra el texto de la NOM (`dflores296/NOM-001-SEDE-2012`, corpus del DOF) el 2026-10-03.
Cada pregunta trae sus opciones, lo que dice la norma y una recomendación; decide David.

### 1. ¿Qué criterio llevan los circuitos nuevos?

| Opción | Lo que dice la NOM | A favor | En contra |
|---|---|---|---|
| **A. Prioridad al conductor**, todos | 240-4 es la regla general; 430-52(c)(1) lo permite («no exceda») | La práctica de campo (bomba: 15 A). Tierra más chica. El cable queda protegido aunque falte 430-32 | 430-52(b) obliga a que soporte el arranque y la app no lo verifica. Un motor grande arrancando a tensión plena puede disparar: 100 HP con 175 A (141 % de la FLC) |
| **B. Máximo 430-52**, como hoy | Los porcentajes de la Tabla 430-52 son los que dan por bueno el arranque de 430-52(b); 430-31 pone la sobrecarga del cable en el dispositivo de 430-32 | Sin riesgo de arranque. Los mismos números que el escritorio y que los archivos guardados | El caso de la bomba sigue: 25 A se lee como mínimo. Tierra más grande. Depende de que se instale 430-32 |
| **C. Por tamaño, con el corte que ya trae la NOM**: prioridad al conductor hasta 1 HP, máximo 430-52 arriba | 430-32 parte en 1 HP: (a) arriba, (b) y (d) de 1 HP o menos. 430-32(d)(2)(a) Exc. ya acepta un motor de 1 HP o menos (arranque manual, no fijo) en un circuito de 120 V de hasta 20 A | Arregla el caso común (bombas, ventiladores, extractores) sin el riesgo de arranque de los grandes | El corte es un criterio: una bomba de 1.5 HP sale con el máximo. Dos reglas que explicar en la memoria |

En las tres, el proyectista puede cambiar el criterio del circuito, y cuando P < Pmáx la memoria pide
verificar 430-52(b) con la curva del interruptor.

**Recomendación: C.** Cambia la de la versión anterior (A): al revisar el texto, 430-52(b) es
obligación y no solo un aviso, y 430-31 confirma que la NOM cuenta con 430-32 para proteger el cable.
Donde el arranque pesa (motores grandes), conviene el máximo. Donde no pesa y el problema se ve en campo
(motores de 1 HP o menos), conviene prioridad al conductor.

### 2. ¿Qué piso lleva el rango (Pmín)?

| Opción | Lo que dice la NOM | A favor | En contra |
|---|---|---|---|
| **A. 125 % de la FLC** | No hay piso literal fuera de 430-52(b). Se apoya en 430-22 (conductor al 125 %), 430-32(a)(1) (relevador a ≤ 115–125 % de la placa) y, por analogía, en 210-20(a) (carga continua al 125 %) | El relevador dispara antes que el interruptor: los dos se coordinan. Coincide con el calibre | Es criterio, no norma; va a `POR-VERIFICAR.md` |
| **B. 100 % de la FLC** | Ninguna sección | Más valores en el selector manual | El interruptor carga al motor a su plena corriente todo el tiempo; una sobrecarga moderada o un arranque largo lo dispara antes que al relevador (se pierde la coordinación) |
| **C. Sin piso; aviso abajo del 125 %** | — | Libertad total en manual | Deja escoger 15 A para un 10 HP (28 A): el aviso tendría que salir casi siempre |
| **D. A, más un aviso de arranque** con la letra de código | Tabla 430-7(b) (ya en `tablas-nom.json`) da los kVA de rotor bloqueado por HP | Es lo único que se acerca a 430-52(b) | Sin la curva del interruptor (sin catálogo, por decisión) solo puede avisar, no decidir; pide capturar la letra de código |

El piso solo pesa en el selector manual y en el recorte del paso 7: prioridad al conductor escoge el
mayor valor que protege al calibre, que casi siempre está arriba del piso.

**Recomendación: A**, y D como fase posterior.

### 3. ¿Dónde va el selector de valor manual?

| Opción | A favor | En contra |
|---|---|---|
| **A. En el desplegable del motor**, junto a «No arranca con la Tabla 430-52» | Sigue [`captura-en-el-desplegable.md`](captura-en-el-desplegable.md): el desplegable captura y el renglón resume. Junta todo lo de la protección del motor | Un clic más para llegar |
| **B. En la celda «Protec. (A)» del renglón** | Más rápido; se ve donde está el resultado | La celda es de resultado (`col-res`), con el desglose en su tooltip. Rompe la regla del renglón |
| **C. A, con un atajo**: clic en la celda abre el desplegable en el selector | La rapidez de B sin romper la regla | Un detalle más de interfaz que probar |

**Recomendación: A** (C, si en uso se siente lento).

### 4. ¿Hacemos ya la fase 2 (A/C, grupos de motores, variador)?

| Opción | A favor | En contra |
|---|---|---|
| **A. Esperar** a usar la de motores | Se corrige el patrón con un solo caso antes de copiarlo cuatro veces | Más adelante, otro formato de archivo (13) |
| **B. Todo junto** | Un solo cambio de formato (12) y un solo selector | Cuatro reglas con texto distinto. 440-22(a) trae su propia excepción («no se exigirá menor a 15 A»); 430-53(c)(4) permite subir por 240-4(b); la MOCP de placa (440-4(b)) y el variador (110-3(b)) son del fabricante. Más que probar de una vez |
| **C. Motores, y A/C por corriente nominal** | 440-22(a) tiene la misma estructura que 430-52: «debe ser capaz de conducir la corriente de arranque», techo de 175 % (225 % si no arranca). Es el gemelo más cercano | Deja grupos, placa y variador para después |

**Recomendación: A.** El formato extra no cuesta mucho: ya va en 11.

### 5. Cerca de 100 A, ¿qué columna de terminales?

Caso: 30 HP, 220 V, FLC 80 A, 125 % = 100 A. Hoy: 3 AWG con 200 A, tierra 6 AWG.

| Opción | Lo que dice la NOM | Resultado | A favor | En contra |
|---|---|---|---|---|
| **A. Quedarse en 60 °C**: P ≤ 100 A (paso 8) | 110-14(c)(1)a.: 100 A o menos, ampacidad a 60 °C | **1 AWG con 100 A**, tierra 8 AWG | Simple, sin iterar, P menor | Más cobre que hoy |
| **B. El calibre menor que quede protegido**, en 60 °C o en 75 °C | a. hasta 100 A; b. arriba de 100 A, 75 °C | **2 AWG con 110 A**, tierra 6 AWG | Menos cobre que A | Hay que calcular las dos columnas y comparar; P mayor |
| **C. A, con la salida a 75 °C que ya existe** | a.(3): «Terminales marcadas 75 °C» (la casilla del tablero). a.(4): motor de diseño B, C, D o E, para la terminal del motor | **3 AWG con 100 A**, tierra 8 AWG | Lo menos de cobre y la P menor, si las terminales lo permiten. La mayoría de los motores de uso general son de diseño B | La terminal del interruptor también debe estar marcada 75 °C; a.(4) sola no basta. Si se agrega, es una casilla más por motor (diseño B–E) |

**Recomendación: A, y recordar en la memoria la salida de C**: con la casilla de terminales 75 °C, el
30 HP baja a 3 AWG con 100 A. La casilla de diseño B–E del motor puede esperar.

### 6. (Nueva, de la revisión del texto) ¿Con qué protección se calcula el techo del principal, 430-62(a) y 430-63?

| Opción | Lo que dice la NOM | A favor | En contra |
|---|---|---|---|
| **A. Pmáx de 430-52**, como lo dice el corchete | 430-62(a): «[con base en el valor máximo permitido … de acuerdo con 430-52]»; 430-63(1): «el valor nominal permitido en 430-52». 250-122(d)(1), en cambio, dice «el valor nominal del dispositivo»: la norma distingue los dos | Lectura literal. El techo del principal no cambia al escoger otro criterio en un derivado | Con P chica, el principal puede quedar mucho arriba del derivado (cumple, pero se ve raro); hay que explicarlo en la memoria |
| **B. P elegida** (lo que decía la versión anterior) | Contra el corchete | Techo más bajo, del lado conservador | Avisaría «excede 430-62(a)» en un principal que la NOM permite |

En las dos, el aviso A-4 (principal menor que el derivado) compara contra P: son los interruptores que
se instalan. Hoy la app usa la protección instalada; con A, en riel DIN cambiaría de 32 A a 35 A.

**Recomendación: A.**
