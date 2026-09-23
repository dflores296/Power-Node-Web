# Índice de la documentación

Organizado por **para qué lo buscas**, no por quién lo escribió. Mismo patrón que
`dflores296/msa-toolkit` y `dflores296/AbaSuite`.

Última revisión: **22 de septiembre de 2026**.

---

## 🔴 `estado/` — lo vivo. Máximo 4 archivos, a propósito

Si algún día son más de 4, algo se poda a `historico/` — no se agrega un quinto. Es la regla que
`AbaSuite` tuvo que aprender a la mala (10 archivos, ~6 000 líneas, más de la mitad ya cerrado).

| Documento | Para qué |
|---|---|
| [`estado/TABLERO.md`](estado/TABLERO.md) | **Los frentes con su %, y la lista que lo justifica.** Empezar aquí. |
| [`estado/HALLAZGOS.md`](estado/HALLAZGOS.md) | Cada defecto encontrado, con ID estable. No se cierra sin commit. |
| [`estado/BITACORA.md`](estado/BITACORA.md) | Qué se hizo cada sesión. |
| [`conocimiento/seleccion-conductor-y-proteccion.md`](conocimiento/seleccion-conductor-y-proteccion.md) | Qué cubre el programa de la selección de conductor y protección, contra la NOM. |
| [`estado/POR-VERIFICAR.md`](estado/POR-VERIFICAR.md) | Lo escrito sin verse corriendo, esperando confirmación. |

## 🟢 `decisiones/` — una por archivo, con autor y estado

Cada decisión dice **quién** la tomó y **cuándo**. Sin eso no cuenta:

| Estado | Significa |
|---|---|
| `PROPUESTA · <quién> · <fecha>` | La sugirió alguien. No es válida todavía. |
| `CONFIRMADA · <quién> · <fecha>` | La confirmó David. Manda. |
| `HEREDADA · origen desconocido` | Estaba antes de este esquema, sin autor claro. Sospechosa hasta revisarse. |

**Una sesión de Claude nunca escribe `CONFIRMADA`** — eso lo hace únicamente David, o él le
confirma a Claude que lo escriba.

| Documento | Estado |
|---|---|
| [`decisiones/alcance-v1-un-tablero.md`](decisiones/alcance-v1-un-tablero.md) | CONFIRMADA · David |
| [`decisiones/motor-copiado-no-enlazado.md`](decisiones/motor-copiado-no-enlazado.md) | CONFIRMADA · David |
| [`decisiones/sin-catalogo-square-d.md`](decisiones/sin-catalogo-square-d.md) | CONFIRMADA · David |
| [`decisiones/blazor-webassembly-sin-backend.md`](decisiones/blazor-webassembly-sin-backend.md) | CONFIRMADA · David |
| [`decisiones/hallazgos-en-markdown.md`](decisiones/hallazgos-en-markdown.md) | CONFIRMADA · David |
| [`decisiones/sin-piso-practico-de-calibre.md`](decisiones/sin-piso-practico-de-calibre.md) | CONFIRMADA · David · 2026-09-22 |
| [`decisiones/interruptor-principal-criterios-del-excel.md`](decisiones/interruptor-principal-criterios-del-excel.md) | CONFIRMADA · David · 2026-09-23 (los avisos); M-03 aviso-o-bloqueo sigue abierto |
| [`decisiones/factor-de-potencia-por-circuito.md`](decisiones/factor-de-potencia-por-circuito.md) | CONFIRMADA · David · 2026-09-23 |
| [`decisiones/serie-de-interruptores.md`](decisiones/serie-de-interruptores.md) | CONFIRMADA · David · 2026-09-23 |
| [`decisiones/documento-imprimible-en-vez-de-archivo.md`](decisiones/documento-imprimible-en-vez-de-archivo.md) | **PROPUESTA · Claude · 2026-09-22** |

## 📘 `conocimiento/` — lo que se aprendió, no cambia cada sesión

| Documento | Qué es |
|---|---|
| [`conocimiento/motor-copiado.md`](conocimiento/motor-copiado.md) | Exactamente qué se trajo de `PowerNode-DesignSuite`, qué se dejó fuera, y los cinco ajustes que hizo falta hacer para que compilara solo. |
| [`conocimiento/marca.md`](conocimiento/marca.md) | El logo, los colores reales, la escala de redondeo, y en qué se aparta del escritorio. |
| [`conocimiento/cuadro-de-carga-excel.md`](conocimiento/cuadro-de-carga-excel.md) | El Excel original leído celda por celda: los 42 espacios, el encabezado, cómo reparte las fases, la fila del alimentador y las 24 columnas del entregable. |
| [`conocimiento/tablas-de-la-norma.md`](conocimiento/tablas-de-la-norma.md) | Cómo el motor lee la NOM sin base de datos: la costura (`FilaTabla`), los 39 KB de JSON, el `--check` que rompe el build, y cómo agregar una tabla. |

## ⚪ `historico/` — lo cerrado. Nunca se borra, se mueve aquí.

Vacío todavía — es el primer día de este repo.
