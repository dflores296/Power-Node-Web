# Tablero

**Actualizado:** 2026-10-03.

## Alcance

Calcular un solo cuadro de carga: un tablero, hasta 42 espacios, nones y pares (tipo NQ/NF). Sin
cascada, sin catálogo de equipos, sin coordinación de protecciones —
[`../decisiones/alcance-v1-un-tablero.md`](../decisiones/alcance-v1-un-tablero.md).

Ejecutar en el navegador (Blazor WebAssembly), sin servidor, publicado en GitHub Pages.

## Frentes

| Frente | Avance | Pendiente |
|---|---|---|
| Motor | 100 % | Llevar al escritorio los cambios listados en [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md). |
| Interfaz | ~90 % | **Auditoría NOM, ronda 2, del 2026-10-02** ([`HALLAZGOS.md`](HALLAZGOS.md)): M-19 (paralelos por ampacidad), I-158 (210-3), I-159, I-160 (1 polo en centro de carga), I-161 (neutro por 220-61, opción) e I-162 (conductores por fase del alimentador, automático o fijado) cerrados. **Verificación, ronda 3** (2026-10-02): I-163 a I-165 cerrados; en paralelo, el neutro cumple 250-122 con el área del juego (David, 2026-10-03). **Revisión de cabos sueltos** (2026-10-03): I-166 (neutro de acometida al 12.5 %, 250-24(c)) a I-170 cerrados; decisiones de conductores por fase y del neutro por 220-61, CONFIRMADAS. **Verificación, ronda 4**: I-171 a I-175 (ningún tubo alcanza, una tierra en un tubo, N fijado a 1/0, dos decimales); lectura literal de 250-24(c)(2), CONFIRMADA (David): [`../decisiones/neutro-de-acometida-en-paralelo-250-24-c-2.md`](../decisiones/neutro-de-acometida-en-paralelo-250-24-c-2.md). **Protección del derivado de motor por rango** (caso de David, 2026-10-03: bomba de 1/2 HP con 25 A sobre 14 AWG): M-20 cerrado y CONFIRMADO (David) — automático (prioridad al conductor hasta 1 HP, máximo arriba), selector en la celda «Protec. (A)», 430-62(a) con el máximo permitido, formato 12; fase 2 con A/C y variador (su automático es el máximo: CONFIRMADO, David); el selector de la celda, solo con el rango (I-182), y la sobrecarga del motor especificada, no preguntada (I-183): propuestas, por decidir (David); grupos de motores fuera; sin llevar al escritorio por ahora: [`../decisiones/proteccion-de-motores-por-rango.md`](../decisiones/proteccion-de-motores-por-rango.md). **Auditoría NOM del 2026-09-29** ([`HALLAZGOS.md`](HALLAZGOS.md), M-15 a M-18, I-142 a I-153): P1 y P2 cerrados salvo P2-4; P3-3, P3-4 y los riesgos 1, 4, 5, 6 y 7 cerrados. Por decidir (David): F.P. y continuidad por omisión (I-149, I-153), factores de demanda del Art. 220 (I-150), capacidad interruptiva (I-151), descarga más ligera (I-152). Lo que la auditoría dejó para después, probado y fijado como regresión (`PendientesDeProbar20260930Tests`); salieron I-154 a I-157 (anuncios por 600-5, 220-12 sin alumbrado que no es general, piso de 220-56, cita del principal), cerrados. Auditoría del 2026-09-28: cerrada (David, 2026-09-29). Puntos de David: I-99 a I-112 hechos. Motores y A/C contra la norma: las cinco fases hechas (M-12, M-13, I-113 a I-122); falta que David las revise y confirme [`../decisiones/motores-y-equipos-en-grupo.md`](../decisiones/motores-y-equipos-en-grupo.md). **Cargas y clases de circuito: las cinco fases hechas** — el tipo es de cada carga, la clase del circuito sale de sus cargas, otro tablero es alimentador, reglas por clase y mínimo por superficie (I-123 a I-127, M-14); falta que David la revise y confirme — [`../decisiones/cargas-y-clases-de-circuito.md`](../decisiones/cargas-y-clases-de-circuito.md), commits en [`../conocimiento/trazabilidad-cargas-y-circuitos.md`](../conocimiento/trazabilidad-cargas-y-circuitos.md). **La carga se captura solo en el desplegable; el renglón resume y dice la clase** (I-128) — falta que David la marque CONFIRMADA: [`../decisiones/captura-en-el-desplegable.md`](../decisiones/captura-en-el-desplegable.md). Después: ejercicios de cálculo (derivados, alimentadores, motores) y la rapidez de captura. Pendiente hasta un caso real: servicio no continuo por motor en un grupo, 430-24 Excepción 1 (I-136, con su propuesta en [`HALLAZGOS.md`](HALLAZGOS.md)). Confirmar las propuestas de motores (I-15) y de los seis tipos de carga (I-74). Modelar centros de carga de una barra por lado. |
| Publicación | 100 % | — |

## Motor

- Copiar `Calculo` y `Domain` de `PowerNode-DesignSuite` (commit `29f660f`), sin `Data`, EF Core ni
  SQL Server.
- Leer 21 tablas y 3 secciones de la NOM desde JSON (81 KB) — `PowerNode.DesignSuite.Normativa`. Lugar seco,
  húmedo o mojado con 310-10(b) y (c)(2) (M-16); Tabla 250-66 (M-18).
- Canalizaciones (nacido en la web): portadores, ajuste por tipo de canalización y tamaño —
  `Calculo/Canalizaciones/`. Charola, en una segunda entrega.
- Pruebas: 52 en `PowerNode.Normativa.Tests`.

## Interfaz

- `/`: capturar la ficha del tablero, los circuitos, el resumen de carga y el alimentador. Dibujar el
  interior del gabinete en multifilar: barras, conexión y número de cada espacio, interruptores NEMA tipo QO,
  directorio y el principal en zócalo, en espacios o zapatas (I-68).
- Mover circuitos arrastrándolos, en la tabla (del número) o en el gabinete (del interruptor), con
  deshacer; «Optimizar acomodo» para el menor desbalanceo — I-69. El principal, entre el zócalo y los
  espacios — I-70.
- Seis tipos de carga — I-74, [`../decisiones/tipos-de-carga.md`](../decisiones/tipos-de-carga.md). Motor
  (Art. 430), en HP o en A interpolando en la tabla: FLC de tabla, conductor al 125 %, protección de la
  Tabla 430-52 — I-15, [`../decisiones/motores-art-430.md`](../decisiones/motores-art-430.md). A/C y
  refrigeración (Art. 440), por su placa: MCA y MOCP, o corriente nominal (175 % / 225 %). En el
  alimentador, los dos en un grupo por fase: 430-24, 440-33 y 430-62(a). Varios motores, o motores y
  otras cargas, en un circuito: Motor «Varios», con desglose — 430-24, 430-53(c)(4) (I-115). Aparato con
  motor en el desglose de carga: el mayor al 125 % — 220-18(a) (I-118). A/C «Varios» — 440-22(b)
  (I-116) — y de habitación — 440-62 (I-117). Motor con variador (I-119), servicio no continuo (I-120),
  cargas no simultáneas (I-121) y medio de desconexión en la memoria (I-122).
- `/documento`: emitir el cuadro de carga (24 columnas) y la memoria de cálculo (nueve secciones).
- Consultar en tooltip el desglose de la protección y del conductor de cada renglón.
- Teclado (`wwwroot/js/teclado.js`): Enter/Shift+Enter bajan y suben, ↑↓ cambian de renglón, Esc deshace,
  Ctrl+Enter abre el desglose, Alt+1…5 cambian de sección; barra de ayuda al pie con la ayuda del campo.
- Guardar y abrir el tablero en un archivo (`.powernode.json`); la pestaña lleva el nombre del tablero;
  varios tableros a la vez, uno por pestaña — I-05.
- Piel Linear (David, 2026-10-03 — I-176): Inter dentro de la app, superficies con línea de 1 px, lima
  en «Imprimir / PDF» y en la página activa; impreso sin cambios — [`../conocimiento/marca.md`](../conocimiento/marca.md).
  Falta regenerar `docs/portada.png`.
- Un icono por campo de la ficha (Identificación, Sistema, Gabinete, Condiciones de cálculo) — I-178.
- Barra superior fija (patrón de la NOM y de msa-toolkit): Captura · Cuadro de carga · Memoria de
  cálculo, tema Sistema | Claro | Oscuro (`wwwroot/js/tema.js`) e Imprimir en el documento; impreso,
  siempre en claro.
- Canalizaciones: cada circuito con carga nace en su tubo (T1, T2…, EMT); agrupar en la columna
  «Canal.»; configurar nombre, tipo, opciones y tamaño en la tarjeta «Canalizaciones». El diámetro del
  fabricante de un aislamiento fuera de la Tabla 5 (THHW-LS, THW-LS, USE), en «Condiciones de cálculo» — I-98.
- Protección del motor, del A/C y del variador dentro de su rango (430-52(c)(1), 440-22(a), 440-4(b),
  110-3(b)): automático, prioridad al conductor, máximo o un valor fijo, escogido en la celda «Protec. (A)» — M-20,
  [`../decisiones/proteccion-de-motores-por-rango.md`](../decisiones/proteccion-de-motores-por-rango.md).
- Pruebas: 563 en `PowerNode.Web.Tests` (44 de M-20 y su fase 2; 35 de la auditoría del 2026-10-02, sus rondas 3 y 4 y la revisión de cabos sueltos; 20 del documento de pruebas del 2026-09-23; 30 de la auditoría NOM
  del 2026-09-29, 21 de su «Lo que ya cumple» y 34 de su «Pendiente de probar»).

Requisitos con su referencia NOM y su prueba: [`../conocimiento/requisitos.md`](../conocimiento/requisitos.md).

## Publicación

- Publicar en cada push a `main` con `.github/workflows/deploy.yml`.
- Verificar las tablas contra el repo de la norma (`--check`) antes de publicar.
- Correr `PowerNode.Normativa.Tests` y `PowerNode.Web.Tests` antes de publicar.

## Decisiones abiertas

| Decisión | Documento |
|---|---|
| Entregable impreso desde el navegador | [`../decisiones/documento-imprimible-en-vez-de-archivo.md`](../decisiones/documento-imprimible-en-vez-de-archivo.md) |
| Guardar y abrir el tablero en un archivo | [`../decisiones/archivo-del-tablero.md`](../decisiones/archivo-del-tablero.md) |
| Dónde va el interruptor principal dentro del gabinete | [`../decisiones/montaje-del-interruptor-principal.md`](../decisiones/montaje-del-interruptor-principal.md) |
| Motores en HP: qué se fija y cómo entran al alimentador | [`../decisiones/motores-art-430.md`](../decisiones/motores-art-430.md) |
| Seis tipos de carga: Motor (430) y A/C y refrigeración (440) — contestada, por confirmar | [`../decisiones/tipos-de-carga.md`](../decisiones/tipos-de-carga.md) |
| Motores, A/C y aparatos con motor en grupo — contestada, por confirmar | [`../decisiones/motores-y-equipos-en-grupo.md`](../decisiones/motores-y-equipos-en-grupo.md) |
| F.P. y continuidad por omisión según el subtipo — auditoría P2-4 y riesgo 3 | [`../decisiones/valores-por-omision-de-la-carga.md`](../decisiones/valores-por-omision-de-la-carga.md) |
| Factores de demanda del Art. 220: sugerir, sin quitar el criterio — auditoría P3-1 y riesgo 2 | [`../decisiones/factores-de-demanda-del-articulo-220.md`](../decisiones/factores-de-demanda-del-articulo-220.md) |
| Capacidad interruptiva contra la falla disponible — auditoría P3-2 | [`../decisiones/capacidad-interruptiva.md`](../decisiones/capacidad-interruptiva.md) |
