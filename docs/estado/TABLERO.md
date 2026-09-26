# Tablero

**Actualizado:** 2026-09-26.

## Alcance

Calcular un solo cuadro de carga: un tablero, hasta 42 espacios, nones y pares (tipo NQ/NF). Sin
cascada, sin catálogo de equipos, sin coordinación de protecciones —
[`../decisiones/alcance-v1-un-tablero.md`](../decisiones/alcance-v1-un-tablero.md).

Ejecutar en el navegador (Blazor WebAssembly), sin servidor, publicado en GitHub Pages.

## Frentes

| Frente | Avance | Pendiente |
|---|---|---|
| Motor | 100 % | Llevar al escritorio los cambios listados en [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md). |
| Interfaz | ~90 % | Revisión del 2026-09-23: R-01 a R-13 en [`HALLAZGOS.md`](HALLAZGOS.md). Confirmar la propuesta de motores (I-15). Modelar centros de carga de una barra por lado. |
| Publicación | 100 % | — |

## Motor

- Copiar `Calculo` y `Domain` de `PowerNode-DesignSuite` (commit `29f660f`), sin `Data`, EF Core ni
  SQL Server.
- Leer 18 tablas de la NOM desde JSON (77 KB) — `PowerNode.DesignSuite.Normativa`.
- Canalizaciones (nacido en la web): portadores, ajuste por tipo de canalización y tamaño —
  `Calculo/Canalizaciones/`. Charola, en una segunda entrega.
- Pruebas: 23 en `PowerNode.Normativa.Tests`.

## Interfaz

- `/`: capturar la ficha del tablero, los circuitos, el resumen de carga y el alimentador. Dibujar el
  interior del gabinete en multifilar: barras, conexión y número de cada espacio, interruptores NEMA tipo QO,
  directorio y el principal en zócalo, en espacios o zapatas (I-68).
- Mover circuitos arrastrándolos, en la tabla (del número) o en el gabinete (del interruptor), con
  deshacer; «Optimizar acomodo» para el menor desbalanceo — I-69. El principal, entre el zócalo y los
  espacios — I-70.
- Motores en HP (Motor / A/C): FLC de tabla, conductor al 125 %, protección de la Tabla 430-52; en el
  alimentador, 430-24 y 430-62(a) por fase — I-15,
  [`../decisiones/motores-art-430.md`](../decisiones/motores-art-430.md).
- `/documento`: emitir el cuadro de carga (24 columnas) y la memoria de cálculo (nueve secciones).
- Consultar en tooltip el desglose de la protección y del conductor de cada renglón.
- Teclado (`wwwroot/js/teclado.js`): Enter/Shift+Enter bajan y suben, ↑↓ cambian de renglón, Esc deshace,
  Ctrl+Enter abre el desglose, Alt+1…5 cambian de sección; barra de ayuda al pie con la ayuda del campo.
- Guardar y abrir el tablero en un archivo (`.powernode.json`); la pestaña lleva el nombre del tablero;
  varios tableros a la vez, uno por pestaña — I-05.
- Barra superior fija (patrón de la NOM y de msa-toolkit): Captura · Cuadro de carga · Memoria de
  cálculo, tema Sistema | Claro | Oscuro (`wwwroot/js/tema.js`) e Imprimir en el documento; impreso,
  siempre en claro.
- Canalizaciones: cada circuito con carga nace en su tubo (T1, T2…, EMT); agrupar en la columna
  «Canal.»; configurar nombre, tipo, opciones y tamaño en la tarjeta «Canalizaciones».
- Pruebas: 237 en `PowerNode.Web.Tests` (20 del documento de pruebas del 2026-09-23).

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
