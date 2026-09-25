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
- Gabinete en multifilar: barras con su fase, conexión y número por espacio, directorio afuera; 1F-2H con la barra horizontal — I-67, `HASH`.

Pruebas: 194 en `PowerNode.Web.Tests`, 23 en `PowerNode.Normativa.Tests`.

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
