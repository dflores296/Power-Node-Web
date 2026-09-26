# Índice de la documentación

**Actualizado:** 2026-09-26.

La portada del proyecto —qué es, para qué sirve, cómo se usa— es [`../README.md`](../README.md).
Este índice dice qué hay en `docs/` y, sobre todo, **qué es estado vigente y qué es referencia**.

## `estado/` — estado vigente (máximo 4 archivos)

| Documento | Contenido |
|---|---|
| [`estado/TABLERO.md`](estado/TABLERO.md) | Frentes, avance y pendientes. Leer primero. |
| [`estado/HALLAZGOS.md`](estado/HALLAZGOS.md) | Defectos con ID, prioridad y commit de cierre. |
| [`estado/BITACORA.md`](estado/BITACORA.md) | Acciones por sesión. |
| [`estado/POR-VERIFICAR.md`](estado/POR-VERIFICAR.md) | Supuestos pendientes de confirmar. |

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
| [`decisiones/interruptor-principal-criterios-del-excel.md`](decisiones/interruptor-principal-criterios-del-excel.md) | CONFIRMADA · David · 2026-09-23 (avisos); 2026-09-24 (M-03: aviso; fuera el mínimo capturado, entra 230-79) |
| [`decisiones/factor-de-potencia-por-circuito.md`](decisiones/factor-de-potencia-por-circuito.md) | CONFIRMADA · David · 2026-09-23 |
| [`decisiones/serie-de-interruptores.md`](decisiones/serie-de-interruptores.md) | CONFIRMADA · David · 2026-09-23 |
| [`decisiones/minimo-de-proteccion-por-uso.md`](decisiones/minimo-de-proteccion-por-uso.md) | CONFIRMADA · David · 2026-09-24 |
| [`decisiones/canalizaciones-y-agrupamiento.md`](decisiones/canalizaciones-y-agrupamiento.md) | CONFIRMADA · David · 2026-09-24 |
| [`decisiones/archivo-del-tablero.md`](decisiones/archivo-del-tablero.md) | PROPUESTA · Claude · 2026-09-25 |
| [`decisiones/montaje-del-interruptor-principal.md`](decisiones/montaje-del-interruptor-principal.md) | PROPUESTA · Claude · 2026-09-25 |
| [`decisiones/documento-imprimible-en-vez-de-archivo.md`](decisiones/documento-imprimible-en-vez-de-archivo.md) | PROPUESTA · Claude · 2026-09-22 |

## `conocimiento/` — referencia técnica

| Documento | Contenido |
|---|---|
| [`conocimiento/requisitos.md`](conocimiento/requisitos.md) | Requisitos con su referencia NOM y la prueba que los verifica. |
| [`conocimiento/seleccion-conductor-y-proteccion.md`](conocimiento/seleccion-conductor-y-proteccion.md) | Matriz de trazabilidad: selección de conductor y protección contra la NOM. |
| [`conocimiento/motor-copiado.md`](conocimiento/motor-copiado.md) | Alcance copiado de `PowerNode-DesignSuite`, ajustes y cambios posteriores. |
| [`conocimiento/tablas-de-la-norma.md`](conocimiento/tablas-de-la-norma.md) | Lectura de la NOM desde JSON y verificación `--check`. |
| [`conocimiento/cuadro-de-carga-excel.md`](conocimiento/cuadro-de-carga-excel.md) | Estructura del Excel original. |
| [`conocimiento/marca.md`](conocimiento/marca.md) | Logo, colores y tipografía. |

## `historico/` — documentos cerrados

Vacío.

## Binarios

| Archivo | Qué es |
|---|---|
| `portada.png` | Captura que encabeza el README: la vista «Cuadro de carga» con un tablero de ocho circuitos, a 1600 × 900 en tema claro. Se regenera con Playwright contra `http://127.0.0.1:5199`. |

---

## Convenciones de esta carpeta

- **Nombres en minúsculas y con guiones.** Sin espacios, acentos ni paréntesis. Excepción: los cuatro
  de `estado/`, en mayúsculas para que se vean primero.
- **Una decisión por archivo**, con su estado en la primera línea.
- **Los enlaces entre documentos son relativos** y dentro de `docs/` van sin el prefijo `docs/`.
