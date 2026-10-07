# Bitácora

Registro de acciones por sesión, con hallazgo y commit.

## 2026-10-07

**Estado del repo y observaciones visuales** (David):

- Revisión del rojo en `main` («3 / 6»): los dos checks cancelados de `088c5c9` nunca tuvieron máquina (cancelados a
  los 15 min, sin pasos); el CI del mismo commit y la publicación relanzada a mano, en verde. Corre además «pages
  build and deployment» (Jekyll) en cada push: revisar que Pages publique desde «GitHub Actions».
- I-198, `15c2f1b`: el desplegable reparte el ancho según lo que lleva cada circuito; la descripción, fija en 25 %.
- I-199, `c21d2af`: la tensión de placa, en su columna de la línea (estaba en el pie); el desplegable, con su propio
  scroll de lado en lugar de apretar columnas.
- I-200, `6fff8a6`: la descripción del desplegable termina en la misma línea que la del circuito (`js/desglose.js`).
- El acomodo del desplegable, deliberado con David pregunta por pregunta (R1, R2, D1 a D19):
  [`../decisiones/acomodo-del-desplegable.md`](../decisiones/acomodo-del-desplegable.md); la guía de redacción de
  avisos de David, [`../decisiones/redaccion-de-avisos.md`](../decisiones/redaccion-de-avisos.md); los estados de
  error y advertencia (opción A), [`../conocimiento/estados-de-aviso.md`](../conocimiento/estados-de-aviso.md); el
  mockup, [`../mockups/desglose-por-bloques.html`](../mockups/desglose-por-bloques.html).
- I-201, `8782a79`: el desplegable por bloques, primera de tres partes («Sí a todo, ya implementa en main»). Faltan
  los avisos con la opción A y el mínimo de 220-14 en «Carga c/u» (D14 a D19).

Pruebas: 641 en `PowerNode.Web.Tests` y 52 en `PowerNode.Normativa.Tests`, sin cambio.

## 2026-10-05

**Las decisiones de David sobre la auditoría de motores y los tres puntos de proyecto** («AM-11: B … AM-4: B …
AM-5: B … AM-7: A2 … P-1: sí · P-2: aceptar · P-3: 1») — en este orden:

- P-1: `main` en avance rápido a `6d7de14` (la rapidez, I-188), con build y pruebas antes; publicación y CI en verde.
- AM-5 (M-22), `3ff5157`: la cita de 430-62(b) dice que los 70 A son la ampacidad del 4 AWG y lo protegen (240-4) y
  que no hay tamaño estándar entre 60.93 y 69.21 A. El caso A de la auditoría, armado como prueba: 60.93 A, 69.21 A,
  70 A, 4 AWG, igual que ella.
- AM-4 (M-21), `b78b251`: la columna del motor, la más baja de las dos terminales, con la casilla «Motor diseño B a E y
  arrancador marcado 75 °C». 25 HP a 220 V y 50 HP a 440 V, 3 AWG; 100 HP a 440 V, 2/0. Cambian, por la misma regla,
  el 5 HP con tablero marcado (de 14 a 12 AWG) y el 30 HP con prioridad al conductor y tablero marcado (1 AWG con
  110 A). En el navegador, 3 → 4 → 3 AWG con la casilla.
- AM-7 (M-23), `72a2d48`: el variador con bypass: conductor con los dos 125 %, el rango topado con la Tabla 430-52
  del motor, aviso si queda vacío, «OL» por 430-124(b), desconexión al 115 % de la mayor. 30 HP a 440 V: sin cambio.
- AM-11 (I-189), `93951eb`: la tensión de placa obligatoria en A/C y variador, sin valor por omisión; los polos salen
  de ella; los archivos anteriores abren con la de sus polos y avisan. Las 36 pruebas anteriores que armaban un A/C o un variador
  ahora escogen la tensión de sus polos (`Placas.cs`). Agregada 575/600 V 3F para los tableros de 600 V, a confirmar.
  En el mismo commit, la hoja de la memoria del variador con bypass, que faltaba en `72a2d48`.
- P-3 (I-152), `ef26a2f`: los `.br` con el decodificador de Brotli, verificando el hash. Medido con «Slow 4G» por
  HTTPS: la primera carga, de 54.2 a 24.7 s (8.35 → 3.30 MB); la recarga y la red sin límite, iguales. El respaldo
  (`.br` faltante o con otro hash) y el avance de la pantalla de carga, probados.
- P-2: aceptado el borde de los campos sin fundido (`marca.md`).
- Decisiones: [`../decisiones/criterio-de-la-auditoria-de-motores.md`](../decisiones/criterio-de-la-auditoria-de-motores.md)
  y [`../decisiones/descarga-en-brotli.md`](../decisiones/descarga-en-brotli.md), CONFIRMADAS · David.

Pruebas: 641 en `PowerNode.Web.Tests` (30 nuevas, `CriterioDeMotores20261005Tests`), 52 en `PowerNode.Normativa.Tests`.
A `main` en avance rápido (`6d7de14..a6cab41`, David: «quiero todo en main»), con build y las pruebas en Release antes.

**I-188, el cambio estructural** (David: «sí, las dos fases juntas; por ahora») — `02a71ca`, `71ecdab`, `b49763f`
y este commit:

- Antes de tocar nada, una línea base con dos guiones de Playwright sobre la publicación en Release: tiempos por
  tipo de cambio con un tablero de 42 espacios, y el HTML y los valores de la captura después de 32 pasos.
- Fase 1: el alimentador recordado por su entrada (se calculaba siete veces por recálculo) y el gabinete que solo
  se dibuja si cambia. Fase 2: `RenglonMemorizado` y `FirmaDeDibujo`; al cambiar una longitud se dibuja 1 renglón
  de 42.
- La fase 2 casi no movía el total. Una traza de Chrome mostró el porqué: `--alto-ayuda` escrita en el `<body>` en
  cada cambio de campo (recálculo de estilos de toda la página, ~60 ms) y la transición del borde de los campos
  (la página repintada ~10 veces por Tab, ~70 ms). Corregidos los dos.
- Una longitud, de 329 a 158 ms; ocho seguidas, de 2.9 a 1.3 s. Lo que se ve, idéntico a la base en los 32 pasos;
  el teclado (Enter, flechas, Esc, Ctrl+Enter, Alt+2) y la barra de ayuda, iguales; el foco, visto en oscuro.
- El fundido del borde de los campos se quitó (piel Linear, I-176): anotado en `marca.md`.

Pruebas: 604 en `PowerNode.Web.Tests` (6 nuevas, `FirmaDeDibujoTests`), 52 en `PowerNode.Normativa.Tests`.

**El riesgo de datos rezagados, medido** (David: «¿hay forma de que se queden datos rezagados en un análisis de
instalación?») — `e3dcde1` y este commit:

- El cálculo: `LoRecordadoTests`, 600 cambios al azar; después de cada uno, lo recordado contra calcular de cero:
  idéntico. Probada rompiendo la llave del alimentador y la de los derivados: las dos se detectan.
- La pantalla: 600 acciones de usuario al azar en la versión anterior y en esta, la página comparada paso a paso:
  idénticas. Probada con una firma rota (sin el tamaño de la canalización): se ve en el paso 58.
- `LoQueLeeElDibujoTests`, guarda de lo que lee el marcado del renglón y del gabinete.
- El cuadro de carga y la memoria no pasan por la firma del dibujo. Lo que queda, en `dibujo-por-renglon.md`.

Pruebas: 611 en `PowerNode.Web.Tests` (7 nuevas), 52 en `PowerNode.Normativa.Tests`.

## 2026-10-03

**A `main` y el cambio estructural explicado** (David: «haz merge a main de los bugs que corregiste… explícame el
cambio estructural, y cuando acabemos… nos vamos con puntos de criterio») — este commit:

- `claude/new-session-ghrrb6` a `main` en avance rápido (`d9a1ff8..2dfcd78`): build sin advertencias, 52 + 598
  pruebas; publicación, CI y Pages en verde. Lleva todo `585a857`: los bugs y lo de usabilidad, que van en el
  mismo commit.
- I-188 medido por partes en una compilación de prueba (no publicada): recálculo ~50 ms (el alimentador se calcula
  siete veces), tabla ~33 ms y gabinete ~23 ms de dibujo en .NET, navegador ~100 ms. Propuesta en dos fases:
  [`../decisiones/dibujo-por-renglon.md`](../decisiones/dibujo-por-renglon.md) (PROPUESTA · Claude).

**Auditoría de motores, Art. 430** (David; caja negra contra `d9a1ff8`, 25 circuitos en cinco tableros): el
cálculo coincidió en todos; 18 hallazgos de interfaz y de criterio — `585a857` y este commit:

- Los dos bugs, reproducidos con Playwright antes de tocarlos. I-184: la celda decía 175 A con el cálculo en
  350 A (opciones sin `@key`, como I-45). I-185: quitar la única línea dejaba sus VA como «carga total»; ahora el
  circuito queda vacío (`QuitarCarga`), y la lavadora ya no baja a 15 A.
- I-186: «Nuevo» en la barra, con confirmación; la barra, con cortes nuevos (1320 y 840 px). I-187 e I-193: los
  mensajes de la carga y de la copia recuperada.
- I-190, I-196: la tensión en cada HP y la columna de la tabla en el rótulo (600 V: 575 V). I-191: sin descripción,
  el nombre de la carga. I-192: aviso de centro de carga arriba de 240 V. I-194: la memoria con 3.50 A y «fases
  A-B». I-195: `for`/`id`. I-197 y M-21: textos de 430-62(a) y de 110-14(c)(1) (motor de cálculo: solo textos).
- Por decidir (David), con opciones y recomendación en HALLAZGOS: I-188 (congelamientos: medidos, ~300 ms por
  cambio aquí, todo se vuelve a dibujar; recomendación, un componente por renglón), I-189 (A/C y variador en
  1 polo), M-21 (el criterio de la columna de terminales), M-22 (430-62(b) con el conductor mínimo), M-23 (variador
  con bypass, 430-122(b)).
- Visto en el navegador: claro, oscuro, 360 a 1920 px; el 100 HP marcando y desmarcando dos veces, con y sin
  fijada; la lavadora quitada; «Nuevo» con y sin cambios; cuadro y memoria con la lavadora, el refrigerador de
  3.5 A y un bipolar de 2F-3H.

Pruebas: 598 en `PowerNode.Web.Tests` (17 nuevas, `AuditoriaMotores20261003Tests`), 52 en `PowerNode.Normativa.Tests`.

**I-182 e I-183 implementados** (plan aprobado por David; sin el botón ↺: «¿no puedes simplemente volver a
seleccionar el antiguo?»; «OL» también en A/C, «de fábrica») — `b2b5f51` (confirmación), `230d16f` y este commit:

- La lista de «Protec. (A)» solo con el rango; la fijada en el acento; la calculada la regresa. La fijada guarda
  la huella del rango y se borra si cambia (equipo, Excepción 2, serie), con aviso una vez; solo ese circuito se
  recalcula. «cond.» y «máx.» de formato 12 abren fijados, sin formato nuevo.
- `SobrecargaRequerida` por equipo; «OL» junto a la clase en el renglón y en el documento, con leyenda; desglose,
  memoria y guía con el texto por equipo y el alcance de motores. Motor de cálculo: solo textos de citas.
- De paso, dos comentarios que M-20 dejó fuera de su método (`CambiarCanalizacion`, `ProteccionDelMotor`).
- Visto en el navegador: claro, oscuro, 390 px; bomba de 1/2 HP 15 → 20 fijado → 15; a 1 1/2 HP, aviso y 45 A;
  «OL» en motor, A/C y variador, no en un intermitente; documento con leyenda; memoria; archivo de formato 12.

Pruebas: 581 en `PowerNode.Web.Tests` (18 nuevas), 52 en `PowerNode.Normativa.Tests`.

**I-183: la sobrecarga del motor se especifica, no se pregunta** — propuesta, sin implementar; solo documentos:

- David pidió la clasificación de motores del Art. 430 (uso general y velocidad ajustable), sacada del texto de
  la NOM (corpus de `NOM-001-SEDE-2012`): por potencia y arranque (430-32), por servicio (430-22(e), 430-33),
  por tipo (Tabla 430-52), construcciones especiales (430-4, 430-6, 430-22), Parte J y los casos sin sobrecarga.
- De ahí la propuesta: todo circuito de motor de la app exige sobrecarga, así que se dice como regla del circuito
  (como el ICFT). También el porqué del automático de 1 HP (es criterio) y el alcance de motores en la ayuda.

**I-182, a pregunta de David: para qué es el rango y por qué un fijado quedaría fuera** — solo documentos:

- La física del rango, en la decisión de M-20: en el motor el relevador da la sobrecarga y el interruptor solo
  la falla, así que el interruptor se separa del conductor. El techo es disparar rápido ante una falla. El piso
  es no disparar al arrancar: rotor bloqueado (Tabla 430-251(a)), aceleración y curva del interruptor, que la
  NOM no puede fijar.
- Un fijado solo queda fuera del rango si cambia el equipo, la Excepción 2 o la serie: la propuesta ahora lo
  borra en esos casos, con aviso.

**I-182: el selector de la protección, solo con el rango** (David: «auto», «cond.» y «máx.» se entienden
por ser el desarrollador) — propuesta, sin implementar; solo documentos:

- Medido en el navegador qué separa «Criterio» de «Valor fijo»: el mismo número hoy; al cambiar el circuito
  (bomba de 1/2 a 1 1/2 HP) el criterio se recalcula (45, 30, 45 A) y el fijo se queda, o salta en silencio al
  más cercano si se sale del rango (15 → 25 A).
- Propuesta en la decisión de M-20, como el tamaño del tubo (I-44): solo el rango, calculado o fijado (acento y
  ↺), el porqué en la ayuda, aviso si un fijado se sale del rango. Dos preguntas para David.

**El automático de A/C y variador (el máximo), CONFIRMADO por David** («confírmala»), después de revisarlo contra
el texto de la NOM a su pedido. Solo documentos, sin cambio de cálculo:

- A/C: 440-52(a) exige sobrecarga en **todo** motocompresor y 440-52(b) protege con ella al conductor; la Tabla
  240-4(g) nombra 440 Partes C y F. No hay el hueco del motor de 1 HP o menos (430-32(d)(2)a.).
- Variador: el techo es 110-3(b); pasar la ampacidad llega a 240-4(g) por 430-120 (Parte D), porque la tabla no
  nombra la Parte J y la NOM 2012 no trae 430-130. Anotado en la decisión.
- Antes, `claude/new-session-jmt82u` (M-20 y su fase 2) a `main` a pedido de David, en avance rápido: build sin
  advertencias, 52 + 563 pruebas y el selector visto en el navegador (bomba de 1/2 HP: «15 auto», 15/20/25).

**M-20: CONFIRMADA por David y fase 2 con A/C y variador** («confirma»; «rífate la fase 2 con A/C y
variador»; lo del escritorio, «omítelo por ahora») — `0ec17fe`, `7bf7bf2` y el commit de docs:

- La lógica del rango sale del derivado del motor a `ProteccionDentroDelRango`; la usan el motor, el A/C
  y el variador.
- A/C por corriente nominal: de 125 % (440-32) al de 175 % o 225 % (440-22(a)); por placa, de la MCA a la
  MOCP (440-4(b)). Variador: de 125 % de la entrada a la máxima del fabricante (110-3(b)). Habitación y
  grupos de motores, sin rango.
- Su automático es el máximo: decisión de Claude, anotada en POR-VERIFICAR para que David la revise (la confirmó el mismo día).
- 430-62(a) con el máximo permitido también para A/C y variador.
- Visto en el navegador, claro y oscuro: cuatro selectores (motor, dos A/C, variador), ninguno en el de
  habitación.

Pruebas: 563 en `PowerNode.Web.Tests` (8 nuevas), 52 en `PowerNode.Normativa.Tests`.

**M-20 implementado** (David contestó: 1C, 2A, 3B solo en motores y explicando por qué, 5 y 6 las
recomendadas) — `3b1b689` y el commit de docs:

- Motor de cálculo: el rango de 430-52(c)(1) (del menor de la serie ≥ 125 % de la FLC al máximo) y tres
  criterios: máximo, prioridad al conductor (240-4, 240-4(b), 240-4(d); tope de 100 A con terminal de 60 °C)
  y manual. Tierra con la protección escogida.
- Automático en los circuitos nuevos: prioridad al conductor hasta 1 HP, máximo arriba. La bomba de 1/2 HP:
  15 A sobre 14 AWG.
- La celda «Protec. (A)» de un motor solo es un selector («15 auto», «25 máx.», valores fijos), con el porqué
  en la ayuda. Desglose y memoria con rango, criterio y nota de arranque (430-52(b)).
- 430-62(a) y 430-63 con el valor máximo permitido; A-4 con la instalada. Formato 12; un motor de formato 11
  abre en el máximo.
- Visto en el navegador (claro, oscuro, 390 px). Pregunta 4 (fase 2) sin contestar: David preguntó qué
  fases eran; venían en la sección 8 de la propuesta.

Pruebas: 555 en `PowerNode.Web.Tests` (36 nuevas), 52 en `PowerNode.Normativa.Tests`.

**Protección del derivado de motor por rango** (caso de David: bomba de cisterna de 1/2 HP a 127 V con 25 A
sobre 14 AWG; en campo, 15 A) — propuesta M-20, sin implementar; solo documentos:

- La propuesta entra a `docs/decisiones/proteccion-de-motores-por-rango.md` (PROPUESTA · Claude): rango de
  ≥ 125 % de la FLC al techo de 430-52, tres criterios (prioridad al conductor, máximo 430-52, manual),
  formato 12 con el máximo si falta el campo.
- Verificada contra `CalculadoraCircuitoDerivadoMotor` en `e0eb887`: los cinco ejemplos, la tierra del
  100 HP y los dos casos de agrupamiento cuadran.
- Dos correcciones al algoritmo: en riel DIN, un motor de FLC ≤ 6 A tenía Pmín 16 A sobre un techo de 15 A
  (ahora Pmín ≤ Pmáx); la iteración de terminales oscilaba con 30 HP a 220 V (1 AWG con 110 A ↔ 3 AWG con
  100 A; ahora P ≤ 100 A con la columna de 60 °C). De ahí, la pregunta 5 para David.
- M-20 en HALLAZGOS (P2, por decidir); TABLERO, índice y POR-VERIFICAR al día. P-5 de `requisitos.md` no se
  toca hasta que David decida.

Pruebas: sin cambio — 519 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

Después, a pedido de David («revisa la norma y dame opciones para cada pregunta»), contra el corpus de
`dflores296/NOM-001-SEDE-2012`:

- Cada pregunta con opciones, lo que dice la NOM y una recomendación, en la decisión.
- 430-62(a) dice «[con base en el valor máximo permitido … de acuerdo con 430-52]»: el techo del principal
  va con Pmáx, no con la protección elegida, como decía la propuesta. Corregido; pregunta 6, nueva.
- 430-52(b) obliga a que la protección soporte el arranque, y 430-31 pone la protección del cable contra
  sobrecarga en el dispositivo de 430-32. La recomendación de la pregunta 1 pasa a C: prioridad al
  conductor hasta 1 HP (el corte de 430-32), máximo 430-52 arriba.
- 110-14(c)(1)a.(4) (motor de diseño B–E) y la casilla de terminales 75 °C dan otra salida a la pregunta 5.

**El conductor visto de frente** (pedido de David) — `c572985`, I-181: en el alimentador, en lugar de las cajitas de
color; forro de la NOM y 7 hilos, de aluminio si el conductor lo es, y la tierra sin forro si va desnuda.
Luego, a pedido de David, las fases lado a lado y con brillo, cobre o aluminio — `22589e0`.
Y la misma orilla para los cinco: el neutro se veía con marco — `30f6332`.

**Iconos de la ficha** (pedido de David) — `4f3b49a`, `c053993` y el commit de docs:

- I-178: un icono por campo de Identificación, Sistema, Gabinete y Condiciones de cálculo; 30 nuevos en
  `Icono.razor`, del juego duotono, revisados en una hoja de contacto antes de entrar.
- I-179: la barra de ayuda decía «!» en tres campos (un comentario de Blazor antes del rótulo).
- I-180: «Conductor» redibujado — parecía la llave del SIM. De seis propuestas, David eligió la F: el cable
  con el corte del aislamiento y el cobre saliendo (`544e083`, `d1f023f`).

Pruebas: 519 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

**La piel Linear** (pedido de David: el `DESIGN.md` de Linear y la skill ui-ux-pro-max como referencia,
sin tocar funcionalidad) — `3261de1` y el commit de docs, I-176:

- David eligió: los dos temas, lima para la acción y la página activa, líneas finas en vez del relieve
  (I-64) e Inter dentro de la app en vez de Segoe UI (I-62). Las cuatro, en `marca.md`.
- Inter 4.1 recortada con `tools/fuente_inter.py` (76 KB, OFL). Sin el cero cruzado de Linear: «1/0 AWG»
  salía «1/Ø».
- El impreso, idéntico pixel por pixel; sin cortes ni desplazamiento lateral de 360 a 1920 px.
- Ramas: las tres `claude/*` ya estaban completas en `main`; se le dijo a David cuáles borrar.
- Visto por David ya publicado: el número de un espacio libre lo atravesaba su conexión punteada — I-177,
  `9f3716e`.
- Pendiente: regenerar `docs/portada.png` (sigue con el diseño anterior).

Pruebas: 519 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

**Verificación NOM, ronda 4** (caja negra contra `ef49373`): lo probado, correcto; cinco observaciones —
`d74c16a` y el commit de docs:

- R4-1: error si ningún tubo alcanza (alimentador) y aviso en los derivados — I-171. Al probarlo, David vio que
  el error recomendaba una opción cuyo llenado no se verificó (1250 kcmil sin diámetro): ahora lo dice — `1b325da`.
- R4-2: una sola tierra con «Paralelos en un tubo» — I-172.
- R4-3: N fijado abajo de 1/0 sube a 1/0 en vez de rechazarse — I-173.
- R4-4: 250-24(c)(2) con el área total, lectura literal; decisión CONFIRMADA por David — I-174.
- R4-5: caída con dos decimales en la memoria y en las citas del motor — I-175.

Pruebas: 519 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

**Revisión de cabos sueltos de las rondas 2 y 3** (`3f53a3f..246dccc`), cerrada a petición de David, que
confirmó todo — `44ba372` y el commit de docs:

- I-166: en acometida, el neutro reducido no baja del 12.5 % del área de las fases arriba de 1100 kcmil —
  250-24(c)(1) y (c)(2). 708.57 A: 2 × 250 kcmil en vez de 2 × 3/0.
- I-167 a I-170: neutro en «Comparar opciones», `pointercancel` en la barra de ayuda, citas de 310-10(h) y
  300-3(b)(1), y la memoria dice cuándo R y X del neutro son de otro calibre. Fuera el BOM de seis archivos.
- Decisiones: conductores por fase, CONFIRMADA; neutro del alimentador por 220-61, nueva y CONFIRMADA.
- TABLERO y HALLAZGOS al día; `motor-copiado.md` con la cita de I-169.

Pruebas: 516 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

**Verificación NOM, ronda 3** (caja negra contra `f960bcb`): M-19, I-158, I-160, I-161 e I-162 correctos;
tres observaciones aplicadas — `1175c18`:

- R3-1: el menú regresa también con la pestaña oculta — I-163.
- R3-2: con el neutro reducido, la caída lleva la Z del neutro real y la memoria lo dice; si pasa del
  límite, el neutro sube — I-164.
- R3-3: el piso del neutro cita 215-2(a)(2) — I-165. Decidido por David: en paralelo, el neutro cumple
  250-122 con el área del juego, cada uno de 1/0 o mayor — `1105651`.

Pruebas: 514 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

## 2026-10-02

**Auditoría NOM, ronda 2** (caja negra contra `3f53a3f`): las 12 correcciones de la ronda 1 se verificaron
en la app; tres hallazgos nuevos y dos observaciones — `4fad887`:

- N-1: el alimentador sube a conductores en paralelo cuando la ampacidad no alcanza (2 × 900 kcmil con
  708.57 A) — M-19.
- N-2: aviso de 210-3 en un circuito de varias salidas de más de 50 A — I-158.
- N-3: no se reprodujo con Playwright; `restablecer` refuerza el regreso del menú — I-159.
- Observaciones, aceptadas por David: aviso de 1 polo arriba de 70 A en centro de carga — I-160; casilla
  para reducir el neutro del alimentador por 220-61 — I-161, `b259e88`.
- Opcional de la auditoría, con la propuesta aprobada por David: «Conductores por fase» en el alimentador,
  automático o fijado de 1 a 6, con la tabla para comparar las opciones que cumplen — I-162, `224d97b`.

Pruebas: 509 en `PowerNode.Web.Tests` (25 nuevas en `Auditoria20261002Tests`), 52 en `PowerNode.Normativa.Tests`.

## 2026-09-30 (tercera parte)

**«Pendiente de probar» de la auditoría NOM del 2026-09-29.** Cada escenario, recalculado a mano con la
NOM y fijado como regresión en `PendientesDeProbar20260930Tests` (34) — `4caba1f`:

- 1F-2H y 1F-3H (neutro de solo las cargas F-N; corrimiento del neutro), motores monofásicos, tablero
  alimentado, A/A en un alimentador (440-33), PVC, niple, ducto metálico, multiconductor, tierra común y
  desnuda, azotea, vivienda popular, media (bomba, secadora) y grande (estufa, minisplits), restaurante
  (Tabla 220-56), tienda, taller, «NOM completa» (32 y 63 A) y la Tabla 430-22(e). La charola sigue
  fuera: segunda entrega de canalizaciones.
- El aviso de principal menor que un motor citaba 430-53(c)(4) para un motor solo del desplegable —
  I-154, `4caba1f`.
- 220-56: aviso cuando el F.D. deja la cocina abajo de los dos equipos más grandes — I-155, `4caba1f`.
- 600-5: el circuito de anuncios, continuo y de 20 A; avisos con otras cargas o más de 20 A — I-156,
  `4caba1f`.
- 220-12 sin anuncios, aparadores ni portalámparas de trabajo pesado — I-157, `4caba1f`.
- Del lado seguro, a POR-VERIFICAR: la excepción de 220-52 para vivienda de 60 m² o menos (no solo
  popular) y la lectura de 430-24 Excepción 1 con un motor continuo.

Pruebas: 484 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

## 2026-09-30 (segunda parte)

**Auditoría NOM-001-SEDE-2012 del 2026-09-29** (caja negra contra `55c120b`). Cada hallazgo, primero con
su prueba o en el navegador; donde la causa o la norma resultaron otras, se dice en `HALLAZGOS.md`.

- P1-1: la Excepción 1 de 430-52(c)(1) contra 240-6(a), no contra la serie (riel DIN: 35 → 32 A, 70 →
  63 A); se cita solo con redondeo — M-15, `8b4d893`. La Excepción 2(3), solo declarada — M-15, `60a4f91`.
- P1-2: lugar seco, húmedo y mojado; temperatura de la Tabla 310-104(a), permiso de 310-10(b) y (c)(2)
  (THWN, THW-2 y THWN-2 sí van en mojado; RHH, XHH y THHN no); formato 11 — M-16, `a3fded5`.
- P2-2: un calibre sin R ni X en la Tabla 9 se acota con el menor con datos (900 kcmil se queda, tierra
  de 2 AWG) — M-17, `751b3a1`.
- P1-3: no había ciclo; era `window.confirm` sin contestar. Pregunta dentro de la página y copia del
  tablero en la pestaña — I-142, `87b5856`.
- P2-1: 3F-3H sin circuitos de 1 polo ni tensión F-N — I-143, `443b8cb`.
- P2-3: fuera de rango, la casilla regresa al valor vigente aunque el valor llegue sin foco; mínimos de
  longitud y carga — I-144, `3a5c708`.
- «Lo que ya cumple»: 21 casos como regresión, con los números de la auditoría — `2084aae`.
- P3-4 y riesgo 6: ICFT de 210-8 en baño y cocina; contactos a 277 V (210-6) — I-145, I-146, `c2ba046`.
  210-12 (ICFA) es «se podrán» en la NOM: no se agrega.
- Riesgos 1, 4 y 5: aviso en el renglón; tierra en paralelo «3 × …»; «Corriente de carga» y
  «Capacidad mínima» del alimentador — I-147, `0e73a84`.
- Riesgo 7: la pantalla de carga dice cuánto falta — I-148, `31831fb`.
- P3-3: conductor del electrodo (Tabla 250-66) y puente de unión principal (250-28(d)(1)) del equipo
  de acometida; la Tabla 250-66 entra al JSON (21 tablas) — M-18, `a600dc9`.
- Propuestas, por decidir (David): F.P. y continuidad por omisión según el subtipo (P2-4, riesgo 3) —
  I-149, I-153, `valores-por-omision-de-la-carga.md`; factores de demanda del Art. 220 como sugerencia
  (P3-1, riesgo 2) — I-150, `factores-de-demanda-del-articulo-220.md`; capacidad interruptiva (P3-2) —
  I-151, `capacidad-interruptiva.md`; descarga más ligera (riesgo 7) — I-152.

Pruebas: 450 en `PowerNode.Web.Tests`, 52 en `PowerNode.Normativa.Tests`.

## 2026-09-30

**La carga se captura en el desplegable** (David: «el desplegable es la fuente de verdad»; aprobó la
propuesta y «una línea con dos cantidades, arranca»).

- Modelo: el uso de vivienda como subtipo de Contactos; lo del renglón pasa a su línea; los circuitos
  que la norma pide dedicados no admiten otra línea; clase «Grupo de motores»; formato 7 — I-128, `bb3caa8`.
- Pantalla: la columna Tipo dice la clase del circuito; sin columna Unidad; continua, no continua y
  F.P. de solo lectura; el motor, variador, A/A y tablero se capturan en su línea — I-128, `8b13b80`.
- Documento y memoria con la clase — I-128, `51a2164`.

- Revisión de David: columnas de carga centradas y parejas, desplegable como tabla, línea del tablero — I-129, `8e4de59`.
- Varios tableros en un mismo alimentador (David: «Sí, hazlo») — I-130, `34e6b74`.
- «No simultáneo con» abajo y solo donde aplica — I-131, `afc8998`.
- Variadores: varios en un circuito como grupo y varios motores por variador (David: «haz la 1 y la 2») — I-132, `5291d19`.
- Sin la casilla «Motores» del variador: la sobrecarga de cada motor queda fuera del alcance — I-132, `a0cdaa8`.
- El bote del equipo del renglón ya lo quita — I-133, `041cf90`.
- Campos del desplegable a la misma altura; Total centrada — I-134, `fc6aea4`.
- Columna «Servicio»: el servicio del motor en su línea, sin selector arriba — I-135, `39b1071`. I-136 (servicio por motor en un grupo), pendiente hasta un caso real (David), con su propuesta de pantalla y cálculo en `HALLAZGOS.md`.
- El equipo del renglón con su propio nombre, editable, aparte del del espacio — I-137, `8fc6d3b`.
- «Continua / No continua» en la columna Servicio; nombres genéricos por subtipo — I-138, `50a51ed`.
- Campos que dependen de otro, solo cuando aplican — I-139, `4aa48e3`.
- Renglón sin estirarse de más; desplegable con anchos fijos — I-140, `8203883`.
- Flechas de subir y bajar en la cantidad — I-141, `a44533b`.

Pruebas: 399 en `PowerNode.Web.Tests`, 24 en `PowerNode.Normativa.Tests`.

## 2026-09-29

**Revisión de David de los P1 publicados**: los cuatro bien; un ajuste en I-79.

- M-11 e I-98 a `main` por avance rápido en `73d8e13`; CI y publicación en verde.
- El motivo de un cambio de polos rechazado, en el aviso flotante y no en un renglón de la tabla,
  que se quedaba aunque se borraran los circuitos — I-79, `7943415`.

Pruebas: 274 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

**Los P2 de la auditoría** (pedido de David: «¿por qué solo I-81? aviéntate P2»). I-82 y M-11 ya
estaban.

- Avisar en el archivo cargas, longitudes y capacidad de barra negativas y frecuencia menor que 50 Hz
  — I-84, `e52ad3c`.
- Nombrar en la confirmación los multipolares que pierden polos al bajar fases o espacios; recordar
  los polos elegidos y regresarlos al crecer, si hay lugar; `@key` en las opciones de HP — I-83,
  `15aa643`.
- Leer la coma igual en todos los navegadores: números como texto (`inputmode="decimal"`), coma de
  miles y coma decimal con aviso — I-81, `b8c497b`.
- Compactar la barra por escalones, desplazar las tablas del documento dentro de la hoja y hacer que
  el cuadro quepa a 1920 con «Descripción» fija — I-85, I-86, I-87, `6c0e8aa`.
- Verificar en el navegador lo nuevo y todo lo anterior: P1, I-79, diámetros, arrastre, guardar y
  abrir sin diferencias, impresión, y sin desplazamiento de página de 320 a 1280 px.

Pruebas: 278 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

P2 a `main` por avance rápido en `d63f290` (pedido de David); CI y publicación en verde.

**Verificado por David en la versión publicada:** I-77, I-78, I-80, M-11 e I-98. **Por verificar:** el
aviso flotante de I-79 y los P2 (I-81 a I-87). Sigue: los P3, I-88 a I-97 y M-10.

**Los P3 de la auditoría** (pedido de David: «termina P3»; y una tabla de pruebas breve para que
verifique P2, entregada aparte).

- Decir en la memoria la frecuencia capturada — I-96, `f731945`.
- Quitar «Configuración» del mensaje de caída inalcanzable del motor; anotado para el escritorio —
  M-10, `663f1b9`.
- Rechazar dos canalizaciones con el mismo nombre, en pantalla y en el archivo — I-94, `2882900`.
- Un aviso junto a Aislamiento y Lugar cuando el aislamiento no vale en el lugar, y un error corto en
  cada renglón — I-95, `08eb2b5`.
- Alinear el uso bajo el tipo y quitar el texto cortado de los selectores — I-89, I-90, `0988e99`.
- Barra de ayuda en dos renglones hasta 1100 px y con su alto real reservado al pie — I-88, `aaf5139`.
- Separar los mm² del subrayado del calibre — I-91, `78162ee`.
- Documento impreso: «Canal.» en dos renglones, «Tierra», placa como se capturó; flecha del principal
  sobre la barra — I-92, I-93, `f216b33`.
- Teclear en la descripción y la ficha sin redibujar la página: de ~175 a ~20 ms por tecla — I-97,
  `b8ac355`.
- Verificar en el navegador cada uno; barrido de 320 a 1920 px sin selectores cortados ni
  desplazamiento de página.

Pruebas: 283 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

**Verificado por David en la versión publicada:** el aviso flotante de I-79 y los P2 (I-81 a I-87),
con la tabla de pruebas de 17 puntos: todos bien.

P3 a `main` por avance rápido (pedido de David).

**Lo que quedaba de I-97** (pedido de David: «atácalo para no tener cabos sueltos»).

- Medir un cambio de carga con 42 espacios: cálculo ~65 ms, dibujo ~150, huella de «sin guardar» ~25
  (desarrollo).
- Reusar el resultado de los derivados cuya entrada no cambió, armar el desglose una vez por cálculo y
  revisar la huella al quedar quieta la pantalla — I-97, `1932e36`. Release: de ~110 a ~95 ms.
- Descartar congelar renglones (riesgo de enseñar un resultado viejo) y recordar canalizaciones (no
  ahorra).

Pruebas: 284 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

I-92: los VA del motor se quedan con la regla de I-49 (decisión de David). I-97 a `main` por avance
rápido (pedido de David). **Auditoría del 2026-09-28 cerrada** (David). Siguen los puntos de David.

**Puntos de David.**

- Quitar la columna «Barras» del cuadro y pintar el N.º con el color de su fase — I-99, `568d88f`.
- Alinear el renglón del cuadro: borde del N.º de un multipolar (I-100), la primera línea de cada
  celda a la misma altura (I-101), la carga por fase centrada (I-102) y las notas del uso en una línea
  (I-103) — `bceaec3`.

I-99 a I-103 a `main` por avance rápido (pedido de David).

**Revisión de David de I-99 a I-103 publicados**: el N.º con contorno «se ve horrible» y la alineación no
era la que pedía. Con sus reglas:

- El N.º como los rótulos de fase: relleno del color y número blanco en negritas — I-99, `5125dbd`.
- Todo centrado; todos los campos de 28 px; lo de abajo, notas que no mueven el campo; tipo y uso
  repartidos centrados — I-101 rehecho, `5125dbd`.
- Las columnas de resultados parejas, de 76 px — I-104, `5125dbd`.

A `main` por avance rápido (pedido de David). Sin listas de revisión hasta que el resultado sea el que busca
(David).

- La columna N.º fija otra vez al desplazar de lado (I-101 le había quitado el sticky) — I-105, `aa9e585`.
- 6 px menos por columna para que el cuadro quepa; sin muestras de color en Fase, Neutro y Tierra —
  I-106, `aa9e585`. A `main` por avance rápido.
- La casilla de «+N» dentro de la celda en 2 polos — I-107, `2d2d621`. A `main` por avance rápido.
- El contorno de enfoque separado de la nota; renglones con nota de 66 px y de 2 polos de 2 × 34 —
  I-108, `bda5e9d`. A `main` por avance rápido.
- El texto de ejemplo se va al enfocar el campo — I-109, `8013249`. A `main` por avance rápido.
- La pantalla de carga centrada desde el principio: dos animaciones con el mismo nombre — I-110, `e5e13b5`.
  A `main` por avance rápido.
- El tipo se elige: «—» por omisión y sin calcular hasta elegirlo — I-111; sin icono en el cuadro — I-112,
  `1472560`. A `main` por avance rápido. Sigue: varios motores en un circuito (430-53), por platicar.

**Motores y A/C contra la norma** (pedido de David: medir todo, flujo del ingeniero, la norma en todo,
propuesta en tabla). Texto de la NOM leído del corpus de `dflores296/NOM-001-SEDE-2012`.

- Propuesta y hallazgos abiertos — [`../decisiones/motores-y-equipos-en-grupo.md`](../decisiones/motores-y-equipos-en-grupo.md),
  `b2fec52`. David: las cinco fases; interruptor de grupo = mayor estándar ≤ límite; MCA al 100 %.
- Fase 0: el F.D. no reduce al motor mayor y la MCA entra al 100 % — M-12, M-13, `9a3c7f0`; la memoria
  sin aparatos que no cuentan — I-113, `ee6f3ac`; alimentación y tabla bajo el HP, HP de cualquier
  alimentación, sin «+N» en trifásico, guía de clasificación — I-114, `4e24e18`. A `main`.
- Fase 1: varios motores, o motores y otras cargas, en un circuito — unidad «Varios», desglose con
  motores, calculadora de grupo (430-24, 430-53(c)(4), 240-4(b)), cada motor por separado en el
  alimentador, archivo formato 3 — I-115, `3b7b4d2`.
- Fase 3 (antes que la 2: toca el mismo desglose): aparato con motor en el desglose de carga, el mayor
  al 125 % — 220-18(a), I-118, `502b7b4`. Fases 1 y 3 a `main` por avance rápido.
- Fase 2: A/C «Varios» (440-22(b), 440-33/440-34) y «Hab.» (440 Parte G); A/C de cuarto en el
  desglose de contactos con el aviso de 440-62(b)/(c) — I-116, I-117, `940ddb9`. Archivo formato 4:
  el 3 ya estaba publicado y no conoce los valores nuevos. A `main`.
- Fase 4: motor «VFD» (430-122(a), 110-3(b)); servicio no continuo con la Tabla 430-22(e), extraída del
  repo de la norma; «No simultáneo con» (220-60, 430-24 Exc. 3); medio de desconexión en la memoria —
  I-119 a I-122, `c571d1b`. El botón de detalle abre en cualquier circuito.

Pruebas: 326 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

**Cargas y clases de circuito** (David: «la columna tipo describe el tipo de carga y no tenemos tipo
de carga para los aparatos… no tenemos clasificación para circuitos dedicados ni mixtos»). Medido el
modelo y leído el Art. 100 (`definiciones.json`): circuito derivado individual, de uso general y para
aparatos; «cargas combinadas» en lugar de «mixto».

- Propuesta con las decisiones de David: F.D. por el tipo de cada carga; la captura por renglón se
  queda; otro tablero es un alimentador sin F.D.; se retira «Varios»; guía de cargas; nombres cortos
  basados en la NOM — [`../decisiones/cargas-y-clases-de-circuito.md`](../decisiones/cargas-y-clases-de-circuito.md).
  Hallazgos abiertos: I-123 a I-127 y M-14 (el mínimo de 220-12, que el alimentador no aplica).
  Cada commit del cambio, en [`../conocimiento/trazabilidad-cargas-y-circuitos.md`](../conocimiento/trazabilidad-cargas-y-circuitos.md)
  (pedido de David). `0c988d6`.
- Fase A: página «Guía de cargas» —tipos, subtipos, clases de circuito y glosario, con sus citas—
  desde un solo árbol en el modelo; «?» desde el encabezado Tipo; la barra se compacta antes para que
  quepa la cuarta página — I-126, `38e2c24`.

Pruebas: 334 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

**Cargas y clases, fases B y C** (David, 2026-09-30, después del mockup: «Apruebo el plan,
implementalo»).

- `AparatoDelCircuito` pasa a `CargaDelCircuito` — `dc7cdd2`.
- Modelo: el tipo es de cada carga (subtipo), F.D. carga por carga, clase del circuito, grupo por lo
  que lleva, mínimos de 220-14, tipo Tablero con 215 y sin F.D., archivo formato 5 — I-123, I-125,
  `72bee16`.
- Pantalla, documento y memoria: «Salidas y cargas» con tipo y subtipo, «Combinadas», la clase bajo la
  descripción, sin «Varios», nombres de la NOM — I-123, I-127, `0e48028`.

Pruebas: 359 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

**Fases D y E.**

- Reglas de la clase del circuito: 240-4(b)(1) por lo que alimenta, 210-19(a)(3), 210-21(b), 210-23,
  422-11(e) — I-124, `d029be3`.
- Mínimo de alumbrado general por superficie: Tabla 220-12 extraída, área servida, 220-14(j)(k),
  archivo formato 6 — M-14, `71abc56`.

Pruebas: 369 en `PowerNode.Web.Tests`, 24 en `PowerNode.Normativa.Tests`. Las cinco fases del cambio
«cargas y clases de circuito» hechas.

## 2026-09-28

**Auditoría de interfaz** (pedido de David: botones desalineados, texto cortado o chueco, funcionalidad).
Completa la del 2026-09-27, que se quedó sin límite: reproducir en el navegador sus catorce puntos,
revisar lo que quedó sin ver y registrar todo. Sin correcciones.

- Reproducir los catorce puntos de la sesión anterior: todos se confirman. El de la coma resultó
  distinto: Chrome no la toma como 0, la borra al teclear (0,9 → 9.00) — I-81.
- Barrer la captura a 1920, 1366, 1024, 768, 390 y 360 px, en claro y oscuro, midiendo texto cortado
  en campos y selectores, desbordes y desplazamiento de la página; el documento y la memoria, igual.
- Nuevos: barra superior sin lugar de 761 a 1120 px (I-85), documento que arrastra la página en
  pantallas chicas (I-86), barra de ayuda que tapa el pie (I-88), selector de uso desalineado (I-89),
  selectores cortados (I-90), subrayado que tacha los mm² (I-91), documento impreso (I-92), flecha del
  principal (I-93).
- Revisar lo que quedó pendiente: guardar y abrir un archivo real (sin diferencias en campos ni
  resultados; archivos ajenos o vacíos se rechazan con aviso), arrastrar en el gabinete y en la tabla
  (mover, espacio ocupado, 3 polos, deshacer, principal al espacio: bien) e imprimir en PDF (cabe en
  carta y A4 horizontal; papel físico sigue sin revisar).
- Medir la latencia en la versión publicada (Release): 65 ms por tecla con 42 espacios — I-97.
- Registrar I-77 a I-97 y M-10, abiertos.

Pruebas: 269 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests` (sin cambios de código).

**Los cuatro P1 de la auditoría** (pedido de David: revisar el registro, llevarlo a `main` y empezar
por los P1). El registro pasó a `main` por avance rápido en `22ccaca`, con tres precisiones.

- Rechazar tensión menor que 100 V y F.P. fuera de 0.1 a 1: el campo regresa a su valor con un aviso
  que dice por qué (`js/teclado.js`, con el `min`/`max` de cada campo); el archivo los avisa — I-77,
  I-80, `42a4957`. De paso, F.D. = 2 ya no se queda escrito (I-82) y no entran negativos en pantalla
  (I-84, falta el archivo).
- Contar los circuitos de 1 polo con captura al ampliar los polos; el selector regresa si no se
  puede — I-79, `42a4957`.
- Preguntar antes de borrar circuitos al reducir espacios o pasar a 1F-2H, nombrándolos; recordar
  los espacios elegidos — I-78, `42a4957`.
- Verificar en el navegador tecleando como usuario: los cuatro, más I-55 y Esc sin cambios.

Pruebas: 273 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

P1 a `main` por avance rápido en `e1ef93d` (pedido de David); CI y publicación en verde.

**El diámetro del fabricante con el aislamiento** (pedido de David).

- Pedir el diámetro exterior del fabricante en «Condiciones de cálculo», con un aviso, y quitarlo
  de la tarjeta de canalizaciones; general para cualquier aislamiento y calibre fuera de la Tabla 5
  — I-98, `71ff9d9`. THHW sí está en la Tabla 5; los que no: THHW-LS, THW-LS, USE, USE-2.
- Leer los renglones de la Tabla 5 cuya celda de tipo viene en blanco: THHN 4/0 a 300 y XHHW de 250
  en adelante se perdían — M-11, `7ffac7a`.

Pruebas: 274 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

## 2026-09-27

**Revisión de I-15 con David** — comparar en el navegador la versión anterior (`22641fd`) con la del
Art. 430 (`8099fb1`), el mismo tablero con dos motores trifásicos.

- Quitar el neutro de la memoria en un multipolar sin «+N» y del alimentador en 3F-3H — I-73, `458508d`.
- Motor / A/C: la unidad decidía el artículo (HP, 430; VA, W o A, carga de placa). David aceptó seis
  tipos de carga, con Motor (430) y A/C y refrigeración (440) por separado — I-74, propuesta en
  [`../decisiones/tipos-de-carga.md`](../decisiones/tipos-de-carga.md), `1f4bea4`.
- Implementar los seis tipos con las respuestas de David: Motor en HP o en A (interpolado, 430-6(a)(1));
  A/C por MCA y MOCP o por corriente nominal (440-4(b), 440-6(a), 440-22(a), 440-32); los dos en un
  grupo del alimentador (430-24, 440-33); «Al alimentador» en el resumen; archivo en formato 2 — I-74,
  `ef619d4`. Motor copiado: `FlcMarcadaEnAmperesA` y `CalculadoraCircuitoDerivado440`.
- Contar el neutro en «Fases / hilos» solo si el derivado lo lleva — I-75, `5bab8a9`.
- Refrigerador: en los contactos de la cocina va en Cocina; en su propio circuito, uso «Refrigerador» — I-76, `dc339f3`.

Pruebas: 269 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

## 2026-09-26

**Ordenar el repositorio como `msa-toolkit` y `NOM-001-SEDE-2012`** (pedido de David) — `c92aafe`

- Agregar `LICENSE`: código visible, no abierto, como `msa-toolkit`; las tablas conservan CC BY-SA 4.0.
- Reescribir `README.md` como portada: captura, insignias, enlace a la herramienta, para qué sirve,
  cómo se usa, estructura, documentación, licencia y marcas.
- Mover la tabla de requisitos a `docs/conocimiento/requisitos.md`.
- Renombrar `docs/LEEME.md` a `docs/README.md` (GitHub lo muestra al abrir la carpeta); agregar las
  tres decisiones que faltaban en el índice, los binarios y las convenciones.
- Agregar `docs/portada.png` (Playwright, vista «Cuadro de carga»).
- Agregar `.claude/hooks/session-start.sh`: instala .NET 8 y `wasm-tools` en las sesiones remotas.

**Diagrama de arquitectura** (entregado por David) — `docs/arquitectura.webp` en el README, sección
«Arquitectura», con crédito a [GitDiagram](https://github.com/ahmedkhaleel2004/gitdiagram) — `22641fd`.

**Motores en HP, Art. 430** (pedido de David: «vamos con I-15») — I-15, M-09, `8099fb1`

- Capturar un motor por sus HP en Motor / A/C: FLC de tabla, conductor al 125 %, protección de la
  Tabla 430-52; desglose, documento y memoria por el Art. 430.
- Sumar los motores del alimentador por fase (430-24), con su techo (430-62(a), 430-63) y su FLC en
  la caída fasorial.
- Motor copiado: motores por fase en `CorrienteDeFaseAlimentador`, `TerminalesMarcadas75C` en el
  derivado de motor, techo de 430-63 con 215-3 (M-09) — registrado en
  [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md).
- Propuesta: [`../decisiones/motores-art-430.md`](../decisiones/motores-art-430.md), con dos
  preguntas para David.
- Agregar `.claude/launch.json` (la app en `http://127.0.0.1:5199`, como pide CLAUDE.md).

Pruebas: 237 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

## 2026-09-23

**Prueba de los tres aparatos (refrigerador, microondas, air fryer; 3F-4H 220/127 V)**

- Dimensionar el alimentador con la fase más cargada — M-02, `b37de00`.
- Avisar principal menor que el derivado más grande — M-03, `b37de00`.
- Capturar la carga en VA, W o A — I-25, `b37de00`.
- Corregir el tooltip de «Tipo» — I-30, `b37de00`.
- Capturar el F.P. por circuito; calcular kW reales y F.P. resultante — I-26, I-27, `2ebf20f`.
- Explicar en tooltip el efecto del F.P. según la unidad — `3321f64`.
- Redactar los avisos del principal sin el Excel; agregar «Mínimo del principal (A)» — I-28, `e7fdf0a`.
- Seleccionar la familia de interruptores (centro de carga, riel DIN, NOM completa) — I-29, `b9406ab`.
- Mostrar completos «Acometida» e «Interruptores» — I-31, `b9406ab`.

**Auditoría de selección de conductor y protección** — [`../conocimiento/seleccion-conductor-y-proteccion.md`](../conocimiento/seleccion-conductor-y-proteccion.md)

- Capturar aislamiento y lugar de instalación — I-32, `b1a84d6`.
- Mostrar el desglose de protección y conductor; corregir la sección 4 de la memoria — I-33, `13b3093`.
- Verificar el 125 % antes de factores y la carga al 100 % después — M-05, `74d6783`.
- Declarar terminales marcadas 75 °C — M-06, `3aa1c3a`.
- Aplicar 240-4(b) en Equipo — M-04, `3aa1c3a`.
- Agregar la regla de conteo en «Agrupados» — I-34, `3aa1c3a`.
- Redactar tooltips, README y documentación de estado en infinitivo, sin explicaciones.

Pruebas: 66 en `PowerNode.Web.Tests`, 6 en `PowerNode.Normativa.Tests`.

**Revisión de David** — R-01 a R-13 validados contra la NOM y registrados en [`HALLAZGOS.md`](HALLAZGOS.md).

- Correr `PowerNode.Web.Tests` al publicar — R-03, `986c9d7`.
- Agregar las 20 pruebas del documento de pruebas — R-07, `986c9d7`. 4.2 con F.P. 0.8 espera 2.00 % — R-05.
- Limitar la caída del alimentador a 3 % por omisión, capturable; avisar la caída combinada mayor que 5 % por circuito; corregir la cita de la memoria, sección 7 (decía «310-15, NOTA 4») — R-01, `2a973ea`.
- Corregir la cita de `CargaLineal` en `CircuitoDerivado.cs`: numeral 310-15(b)(5)(3), no tabla — R-13, `69a5ed6`.
- Redactar el aviso de riel DIN > 125 A en singular o plural, con 240-6(a) — R-06, `de855ee`.
- Quitar el mínimo de 15/20 A por tipo de carga; capturar el uso de los contactos de vivienda (20 A por 210-11(c), 1500 VA por 220-52) — R-14. Confirmar el aviso de M-03 — R-08. `ab9c785`.
- Citar el neutro portador en 2 fases + neutro de estrella, en la memoria y en el tooltip del neutro — R-09, `04c3df3`.
- Fijar con pruebas que en 2F-3H 220Y/127 no se aplican 220-61(a) excepción ni 310-15(b)(7) — R-10, `c942ccb`.
- Llevar al motor el cálculo por fase del alimentador y calcular su caída fase por fase con el neutro (fasorial) — R-04, R-02, `d6cd1c6`.
- Límites de caída por omisión 2 % + 3 % = 5 %; aviso si los límites suman más; avisos de caída combinada fuera del cuadro — R-15, `6ca60a1`.
- Casilla «Equipo de acometida» e «Inmueble»: el principal sube al mínimo de 230-79; fuera el «Mínimo del principal (A)» — R-11, `62fc7f9`. Registrar R-16.
- Justificación del factor de demanda, selección múltiple del Art. 220, en la memoria; aviso si falta — R-12, `eb50462`.
- Cinco tipos de carga y factor de demanda por tipo; motores, A/C y calefacción fija al 100 %; justificación por tipo — R-17, `8e28f86`.
- Revisar 240-4(b) en cada calibre, no solo en el de la carga: 60 A en 6 AWG, no 4 AWG — R-16, `2da832f`.
- Factor de demanda también en motores y A/C (430-26) y calefacción (220-51 Excepción); calefacción a continua; tooltips con ejemplos — R-18, `b3020f0`.
- «Inmueble» global de nueve opciones para 230-79 y para filtrar las justificaciones del F.D. — R-19, `be9985f`.
- Desglose opcional de aparatos por circuito, debajo del último espacio; la carga es la suma — I-35, `e429cbb`.
- Conductor en una celda (calibre y mm² debajo), sin «Hilos»; fases en rectángulos negro, rojo y azul; neutro blanco y tierra verde — 200-6, 250-119; pedido de David, `fb47979`, `48615e2` (AWG o kcmil en la celda).
- Resumen con título «Tipo de carga» y encabezados centrados; balanceo de fases en tarjeta propia con barras por fase — pedido de David, `db71f23`.
- Desglose de aparatos como lista: tarjeta blanca a todo el ancho del renglón, línea entre aparatos — pedido de David, `4f45202`.
- Calibre con su unidad en el alimentador; documento impreso sin «Hilos»; columnas A/B/C de igual ancho — pedido de David, `f3a762b`.
- Quitar del gabinete la nota del acomodo de barras — pedido de David, `d9c2f39`.
- Alimentador: protección seleccionada primero, cables con su color; avisos del principal y de caída combinada en una tarjeta «Avisos» al final — pedido de David, `2c4b2b4`.
- Tarjetas del cierre al mismo alto — pedido de David, `10de2c0`.
- Ficha en tres columnas (Identificación y Sistema apiladas) con campos que llenan la tarjeta — pedido de David, `362c3a7`.
- Ficha con un solo formato de campo (rótulo en columna fija, mismo alto de renglón); en tarjetas angostas, todos apilados — pedido de David, `911c719`.
- Carga con decimales sin cortarse; hilos según las fases — I-36, I-37, `8235454`.
- Móvil: el cierre ya no ensancha la página — I-38, `6d7eb4b`.
- Canalizaciones (plan aprobado por David): tablas 1, 4, 5 y 310-15(b)(3)(c) del Capítulo 10 y módulo `Canalizaciones` en el motor — `b526db8`; agrupamiento por canalización, neutro por circuito y tamaño en el cuadro — I-39, I-40, I-41, M-07, `6e88585`; documento y memoria — `04a9f9f`. Propuesta: `97b1555`.
- Cuadro con su ancho natural y desplazamiento lateral, N.º fijo — pedido de David, `5639aa0`.
- Selector de aislamiento con los 17 tipos del motor; THW válido en lugar seco (310-10(a)) — I-42, M-08, `23da7ce`.
- Canalizaciones en dos tablas (derivados y alimentador), propias sin plegar, tamaño en mm e in — pedido de David, `c2d7094`.
- Quitar «Canalización» y «Tubo» de Condiciones: toda canalización nace EMT y se configura en su renglón; la propia se guarda con el circuito; nombres editables — I-43, `58febd2`.
- Una sola lista de tubos: cada circuito con carga nace en el suyo (T1, T2…), sin «Propia»; opciones en columnas; tamaño en combo mm/in con el calculado ya elegido; sin columna vacía — I-44, `c7ab778`.
- El selector «Canal.» mostraba otro tubo que el del circuito al quitarse uno de la lista (opciones reutilizadas por posición): `@key` — I-45, `2ddc0b5`.

**Revisión de la cocina de David (1F-2H, 127 V, inmueble «Otro»)** — el caso se reprodujo en el motor con los mismos números.

- Aplicar el uso de contactos (210-11(c), 220-52) solo en vivienda de más de 60 m² — I-46, `1cebf41`.
- F.P. del alimentador con demanda y 220-52, el de la caída; sin renglón de neutro en 1F-2H — I-47, `1cebf41`.
- «1 polo» en el interruptor principal — I-48, `1cebf41`.
- Mostrar como máximo dos decimales (el cálculo conserva todos); F.D. y F.P. siempre con dos; VA iguales en todas partes — I-49, `9fe6995`.
- «÷ 1000» en la fórmula de caída de la memoria — I-50, `9fe6995`.
- Alinear tipo y uso de contactos en una columna centrada — I-51; F.P. de dos decimales sin cortarse — I-52, `5cdc8bb`.
- Tensión nominal por configuración (110-4), «Tensión F-N» en 1F-2H y aviso de tensión no nominal; la tierra no es hilo — I-53, `317110e`.
- 1F-2H: renglones en orden sin nones y pares, gabinete en una columna, de 1 a 8 espacios — I-54, `e7af563`.
- 1F-2H solo con 1, 2, 4, 6 y 8 espacios — I-54, `5f74680`.
- Campos numéricos: 0 como marca de agua en cargas, vaciar un obligatorio recupera su valor, seleccionar todo al entrar — I-55, `dbc8f70`.
- Navegación con teclado: Enter/Shift+Enter, ↑↓ de renglón, Esc, Ctrl+Enter, Alt+1…5, foco tras acciones, barra de ayuda, aria-label — I-56, `a5e5b3d`.
- Logo nuevo: carta de Smith (variante B), dibujada con sus ecuaciones; SVG, PNG, .ico y firma generados — `docs/conocimiento/marca.md`, `7e0a36d`.
- Tema oscuro (Automático → Claro → Oscuro, impreso siempre en claro) e iconos de la pestaña renombrados a `powernode-carta-*` — I-57, `f84d575`.
- Barra superior fija con el patrón de la NOM y de msa-toolkit: Captura · Cuadro de carga · Memoria de cálculo, tema Sistema | Claro | Oscuro, Imprimir en el documento — I-58, `af8e83b`.
- Logo más grande (80 px en la carga, 40 en la barra) y la carta que se dibuja: con el avance real de la descarga en la pantalla de carga, al pasar el cursor en la barra; sin tooltip — I-59, `04a2e56`.
- La carta de la carga se ve completa aunque la carga sea rápida (capa encima de la app, 1.6 s mínimo, sin brincos tras la congelación del arranque); encabezado del documento sin hueco; resumen a 360 px; tema e Imprimir con símbolos en el celular — I-60, `da669de`.
- Guardar y abrir el tablero en un archivo `.powernode.json` (lo capturado, se recalcula al abrir); la pestaña lleva el nombre del tablero; varios tableros, uno por pestaña. Decisión propuesta: `archivo-del-tablero.md` — I-05, `0169b78`.
- Botón de GitHub con las estrellas en la barra, como el de gitdiagram.com — I-61, `005be3c`.
- Propuestas de tipografía e iconos en un canvas; David eligió Segoe UI y duotono: iconos duotono en toda la aplicación, firma en Segoe UI — I-62, `5518836`.
- Iconos de tipo de carga en el resumen y junto al selector «Tipo»; el resumen ya no se sale de su tarjeta en pantallas anchas — I-63, `a7e6219`.
- Relieve marcado y más contraste (propuesta B del canvas) — I-64, `3ed4e44`.
- Líneas de tabla más firmes; relieve visible en oscuro, como gitdiagram — I-65, `7ade4dc`.
- Iconos de interruptor termomagnético NEMA y DIN de 1, 2 y 3 polos con su valor; los NEMA, dentro del gabinete — I-66, `4782c6e`.
- El interruptor NEMA rehecho como un QO genérico de frente, con una manija ancha en 2 y 3 polos y sin rótulo; en el gabinete, acostado con la pinza hacia la barra; DIN retirado — I-66, `f850580`.
- El interruptor NEMA con una manija de ancho normal en su polo (único, derecho o central), tapa lisa en los demás y más esbelto — I-66, `3a70aea`.
- El interruptor NEMA a 20 por polo (proporción del QO), centrado en su celda; renglones de 30 px mínimo en la rejilla — I-66, `ba636bd`.
- Gabinete en multifilar: barras con su fase, conexión y número por espacio, directorio afuera; 1F-2H con la barra horizontal — I-67, `eb6d52c`.
- El interruptor principal en el gabinete: zócalo, espacios o zapatas; 1F-2H arranca con zapatas. Decisión propuesta: `montaje-del-interruptor-principal.md` — I-68, `4db6997`.
- Arrastrar circuitos en la tabla y en el gabinete, deshacer y «Optimizar acomodo» (con el desempate por dispersión en `BalanceoDeFases`) — I-69, `1c573d1`.
- Arrastrar el principal entre el zócalo y los espacios; la franja se vuelve el zócalo al arrastrarlo; deshacer regresa el montaje — I-70, `ddffb03`.
- El gabinete en su propio renglón; rótulos del principal y la acometida en una línea; en el celular, todo en la lista de abajo — I-71, `6eabed1`.
- Los números de circuito, centrados en su recuadro con cualquier fuente — I-72, `bd5ee08`.

Pruebas: 218 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

## 2026-09-22 (tercera parte)

- Levantar el Excel celda por celda — [`../conocimiento/cuadro-de-carga-excel.md`](../conocimiento/cuadro-de-carga-excel.md).
- Crear `PowerNode.Web.Modelo` (sin Blazor): `DatosDelTablero`, `CircuitoDelCuadro`, `CuadroDeCarga`, `Memoria/`.
- Resolver la geometría con `DistribucionBarras`, `SistemaDelTablero` y `AcomodoEnGabinete` — I-10, I-11, `cc92ad5`.
- Calcular alimentador e interruptor principal; aplicar el factor de demanda en el total — I-12, `cc92ad5`.
- Emitir `/documento`: cuadro de 24 columnas y memoria de nueve secciones — I-13, `cc92ad5`.
- Calcular V F-N según el sistema — I-14, `cc92ad5`.
- Apilar nones y pares en una sola tabla — `19b4214`.
- Corregir encabezados, columnas de conductor y dibujo del gabinete — I-16 a I-18, `c2bb19c`.
- Corregir color de barras, unidades y líneas — I-19 a I-21, `4730182`.
- Acotar la clase `grupo` — I-22, `228478d`.
- Combinar las celdas de un multipolar — I-23, `acdcf42`.
- Corregir el borde del renglón de continuación — I-24, `73cb16b`.
- Retirar el piso práctico de calibre (decisión de David) — `1529a4f`.

## 2026-09-22 (segunda parte)

- Leer las tablas de la NOM desde JSON sin base de datos — M-01, `c46954f`.
- Crear la primera pantalla de captura — I-01, `c46954f`.
- Escribir CI y despliegue a Pages — P-01, `c46954f`.
- Pasar los polos del circuito como `NumeroFases` — I-02, `c46954f`.
- Aplicar la marca Power Node y los estilos de arranque — I-06, `7cf69c5`.
- Versionar CSS e iconos con `?v=<commit>` — P-03, `dfc674a`.
- Agregar `favicon.ico` — I-07, `c19cca6`.
- Ampliar el selector de tipo — I-08, `1b53276`.

## 2026-09-22

- Crear el repositorio `dflores296/Power-Node-Web` (David).
- Copiar `Calculo` y `Domain` de `PowerNode-DesignSuite` (commit `29f660f`), sin coordinación ni catálogo — `bfc9d47`.
- Mover `FamiliaAislamiento` a `TablasNom` — E-01, `bfc9d47`.
- Estructurar `docs/`: `estado/`, `decisiones/`, `conocimiento/`, `historico/`.
