# Trazabilidad: cargas y clases de circuito

Un cambio grande y en varias fases: el tipo pasa del circuito a cada carga y la clase del circuito sale
de sus cargas. Aquí va **cada commit** con qué hizo, qué hallazgo toca y cómo se verificó, para poder
seguirlo de punta a punta — pedido de David, 2026-09-29.

Decisión: [`../decisiones/cargas-y-clases-de-circuito.md`](../decisiones/cargas-y-clases-de-circuito.md).
Hallazgos: I-123 a I-127 y M-14 en [`../estado/HALLAZGOS.md`](../estado/HALLAZGOS.md).

## Fases

| Fase | Qué | Hallazgos | Estado |
|---|---|---|---|
| — | Propuesta y hallazgos abiertos | — | Hecha |
| A | Guía de cargas y glosario | I-126 | Hecha |
| B | Modelo: carga con tipo, subtipo y forma; clase del circuito; alimentador a tablero; F.D. por carga; archivo formato 5 | I-123, I-125 | Pendiente |
| C | Pantalla, documento y memoria con los nombres cortos; se retira «Varios» | I-127 | Pendiente |
| D | Reglas por clase de circuito | I-124 | Pendiente |
| E | Mínimo de alumbrado general por superficie (220-12, 220-14(j)(k)) | M-14 | Pendiente |

## Commits

Un renglón por commit, en orden. El commit de documentos que pone el hash de uno de código va en el
renglón siguiente.

| # | Commit | Fase | Qué | Hallazgos | Verificación |
|---|---|---|---|---|---|
| 1 | `0c988d6` | — | Propuesta con las decisiones de David; hallazgos abiertos; esta tabla | I-123 a I-127, M-14 | — |
| 2 | `38e2c24` | A | Página «Guía de cargas» (`/guia`): mapa y árbol de los tipos con sus subtipos, clases de circuito, glosario; «?» en el encabezado Tipo; barra compacta desde 1040 y 800 px. Modelo: `GuiaDeCargas`, `SubtipoDeCarga` | I-126 | `GuiaDeCargasTests` (8); 334 + 23 pruebas; navegador 360–1920 px, claro y oscuro, mapa, dirección con `#rama-…`, impresión |
| 3 | *(este)* | A | I-126 con su hash; requisito E-7; bitácora; esta tabla | I-126 | — |
