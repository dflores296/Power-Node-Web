# Índice de la documentación

**Actualizado:** 2026-09-23.

## `estado/` — estado vigente (máximo 4 archivos)

| Documento | Contenido |
|---|---|
| [`estado/TABLERO.md`](estado/TABLERO.md) | Frentes, avance y pendientes. Leer primero. |
| [`estado/HALLAZGOS.md`](estado/HALLAZGOS.md) | Defectos con ID, prioridad y commit de cierre. |
| [`estado/BITACORA.md`](estado/BITACORA.md) | Acciones por sesión. |
| [`estado/POR-VERIFICAR.md`](estado/POR-VERIFICAR.md) | Supuestos pendientes de confirmar. |

Requisitos con referencia NOM y prueba: [`../README.md`](../README.md).

## `decisiones/` — una por archivo, con autor y fecha

| Estado | Significado |
|---|---|
| `PROPUESTA · <quién> · <fecha>` | Sugerida; no vigente. |
| `CONFIRMADA · <quién> · <fecha>` | Confirmada por David; vigente. |
| `HEREDADA · origen desconocido` | Anterior al esquema; revisar. |

Solo David confirma una decisión.

| Documento | Estado |
|---|---|
| [`decisiones/alcance-v1-un-tablero.md`](decisiones/alcance-v1-un-tablero.md) | CONFIRMADA · David |
| [`decisiones/motor-copiado-no-enlazado.md`](decisiones/motor-copiado-no-enlazado.md) | CONFIRMADA · David |
| [`decisiones/sin-catalogo-square-d.md`](decisiones/sin-catalogo-square-d.md) | CONFIRMADA · David |
| [`decisiones/blazor-webassembly-sin-backend.md`](decisiones/blazor-webassembly-sin-backend.md) | CONFIRMADA · David |
| [`decisiones/hallazgos-en-markdown.md`](decisiones/hallazgos-en-markdown.md) | CONFIRMADA · David |
| [`decisiones/sin-piso-practico-de-calibre.md`](decisiones/sin-piso-practico-de-calibre.md) | CONFIRMADA · David · 2026-09-22 |
| [`decisiones/interruptor-principal-criterios-del-excel.md`](decisiones/interruptor-principal-criterios-del-excel.md) | CONFIRMADA · David · 2026-09-23 (avisos). Abierto: aviso o bloqueo en M-03. |
| [`decisiones/factor-de-potencia-por-circuito.md`](decisiones/factor-de-potencia-por-circuito.md) | CONFIRMADA · David · 2026-09-23 |
| [`decisiones/serie-de-interruptores.md`](decisiones/serie-de-interruptores.md) | CONFIRMADA · David · 2026-09-23 |
| [`decisiones/documento-imprimible-en-vez-de-archivo.md`](decisiones/documento-imprimible-en-vez-de-archivo.md) | PROPUESTA · Claude · 2026-09-22 |

## `conocimiento/` — referencia técnica

| Documento | Contenido |
|---|---|
| [`conocimiento/seleccion-conductor-y-proteccion.md`](conocimiento/seleccion-conductor-y-proteccion.md) | Matriz de trazabilidad: selección de conductor y protección contra la NOM. |
| [`conocimiento/motor-copiado.md`](conocimiento/motor-copiado.md) | Alcance copiado de `PowerNode-DesignSuite`, ajustes y cambios posteriores. |
| [`conocimiento/tablas-de-la-norma.md`](conocimiento/tablas-de-la-norma.md) | Lectura de la NOM desde JSON y verificación `--check`. |
| [`conocimiento/cuadro-de-carga-excel.md`](conocimiento/cuadro-de-carga-excel.md) | Estructura del Excel original. |
| [`conocimiento/marca.md`](conocimiento/marca.md) | Logo, colores y tipografía. |

## `historico/` — documentos cerrados

Vacío.
