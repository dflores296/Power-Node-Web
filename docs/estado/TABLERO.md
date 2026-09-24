# Tablero

**Actualizado:** 2026-09-24.

## Alcance

Calcular un solo cuadro de carga: un tablero, hasta 42 espacios, nones y pares (tipo NQ/NF). Sin
cascada, sin catálogo de equipos, sin coordinación de protecciones —
[`../decisiones/alcance-v1-un-tablero.md`](../decisiones/alcance-v1-un-tablero.md).

Ejecutar en el navegador (Blazor WebAssembly), sin servidor, publicado en GitHub Pages.

## Frentes

| Frente | Avance | Pendiente |
|---|---|---|
| Motor | 100 % | Llevar al escritorio los cambios listados en [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md). |
| Interfaz | ~85 % | Revisión del 2026-09-23: R-01 a R-13 en [`HALLAZGOS.md`](HALLAZGOS.md). Guardar y abrir el proyecto (I-05). Capturar circuitos de Fuerza, Art. 430 (I-15). Modelar centros de carga de una barra por lado. |
| Publicación | 100 % | — |

## Motor

- Copiar `Calculo` y `Domain` de `PowerNode-DesignSuite` (commit `29f660f`), sin `Data`, EF Core ni
  SQL Server.
- Leer 18 tablas de la NOM desde JSON (77 KB) — `PowerNode.DesignSuite.Normativa`.
- Canalizaciones (nacido en la web): portadores, ajuste por tipo de canalización y tamaño —
  `Calculo/Canalizaciones/`. Charola, en una segunda entrega.
- Pruebas: 22 en `PowerNode.Normativa.Tests`.

## Interfaz

- `/`: capturar la ficha del tablero, los circuitos, el resumen de carga y el alimentador. Dibujar el
  interior del gabinete.
- `/documento`: emitir el cuadro de carga (24 columnas) y la memoria de cálculo (nueve secciones).
- Consultar en tooltip el desglose de la protección y del conductor de cada renglón.
- Asignar cada circuito a su canalización (columna «Canal.») y consultar la tarjeta «Canalizaciones».
- Pruebas: 166 en `PowerNode.Web.Tests` (20 del documento de pruebas del 2026-09-23).

Requisitos con su referencia NOM y su prueba: [`../../README.md`](../../README.md).

## Publicación

- Publicar en cada push a `main` con `.github/workflows/deploy.yml`.
- Verificar las tablas contra el repo de la norma (`--check`) antes de publicar.
- Correr `PowerNode.Normativa.Tests` y `PowerNode.Web.Tests` antes de publicar.

## Decisiones abiertas

| Decisión | Documento |
|---|---|
| Entregable impreso desde el navegador | [`../decisiones/documento-imprimible-en-vez-de-archivo.md`](../decisiones/documento-imprimible-en-vez-de-archivo.md) |
