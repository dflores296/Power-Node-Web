# Bitácora

Registro de acciones por sesión, con hallazgo y commit.

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
