# Bitácora

Registro de acciones por sesión, con hallazgo y commit.

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
- Llevar al motor el cálculo por fase del alimentador y calcular su caída fase por fase con el neutro (fasorial) — R-04, R-02.

Pruebas: 119 en `PowerNode.Web.Tests`, 6 en `PowerNode.Normativa.Tests`.

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
