# Índice de la documentación

**Actualizado:** 2026-10-05.

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
| [`decisiones/motores-art-430.md`](decisiones/motores-art-430.md) | PROPUESTA · Claude · 2026-09-26 (en parte reemplazada por `tipos-de-carga.md`) |
| [`decisiones/tipos-de-carga.md`](decisiones/tipos-de-carga.md) | PROPUESTA · Claude · 2026-09-27 (David aceptó los tipos y contestó las cuatro preguntas; implementada) |
| [`decisiones/motores-y-equipos-en-grupo.md`](decisiones/motores-y-equipos-en-grupo.md) | PROPUESTA · Claude · 2026-09-29 (David eligió el alcance y contestó tres preguntas) |
| [`decisiones/cargas-y-clases-de-circuito.md`](decisiones/cargas-y-clases-de-circuito.md) | PROPUESTA · Claude · 2026-09-29 (el tipo es de la carga, la clase es del circuito; David contestó seis preguntas) |
| [`decisiones/captura-en-el-desplegable.md`](decisiones/captura-en-el-desplegable.md) | PROPUESTA · Claude · 2026-09-30 (la carga se captura en el desplegable; el renglón solo resume; David contestó cuatro dudas) |
| [`decisiones/valores-por-omision-de-la-carga.md`](decisiones/valores-por-omision-de-la-carga.md) | PROPUESTA · Claude · 2026-09-30 (auditoría NOM P2-4 y riesgo 3; cambia la de F.P., por decidir) |
| [`decisiones/factores-de-demanda-del-articulo-220.md`](decisiones/factores-de-demanda-del-articulo-220.md) | PROPUESTA · Claude · 2026-09-30 (auditoría NOM P3-1 y riesgo 2; cambia R-12, por decidir) |
| [`decisiones/capacidad-interruptiva.md`](decisiones/capacidad-interruptiva.md) | PROPUESTA · Claude · 2026-09-30 (auditoría NOM P3-2; por decidir) |
| [`decisiones/proteccion-de-motores-por-rango.md`](decisiones/proteccion-de-motores-por-rango.md) | CONFIRMADA · David · 2026-10-03 (M-20; fase 2: A/C y variador) |
| [`decisiones/documento-imprimible-en-vez-de-archivo.md`](decisiones/documento-imprimible-en-vez-de-archivo.md) | PROPUESTA · Claude · 2026-09-22 |
| [`decisiones/conductores-por-fase-del-alimentador.md`](decisiones/conductores-por-fase-del-alimentador.md) | CONFIRMADA · David · 2026-10-03 |
| [`decisiones/neutro-del-alimentador-por-220-61.md`](decisiones/neutro-del-alimentador-por-220-61.md) | CONFIRMADA · David · 2026-10-03 (opción; en paralelo, área del juego; en acometida, 250-24(c)) |
| [`decisiones/neutro-de-acometida-en-paralelo-250-24-c-2.md`](decisiones/neutro-de-acometida-en-paralelo-250-24-c-2.md) | CONFIRMADA · David · 2026-10-03 (12.5 % del área total, lectura literal) |
| [`decisiones/dibujo-por-renglon.md`](decisiones/dibujo-por-renglon.md) | PROPUESTA · Claude · 2026-10-03 (I-188; David contestó el 2026-10-05: las dos fases, implementadas) |
| [`decisiones/criterio-de-la-auditoria-de-motores.md`](decisiones/criterio-de-la-auditoria-de-motores.md) | CONFIRMADA · David · 2026-10-05 (AM-4 terminales del motor, AM-5 430-62(b), AM-7 variador con bypass, AM-11 tensión de placa) |
| [`decisiones/acomodo-del-desplegable.md`](decisiones/acomodo-del-desplegable.md) | PROPUESTA · Claude · 2026-10-07 (R1 y R2 de David; D1: un renglón es un aparato con su cantidad; D2 a D19 pendientes) |
| [`decisiones/descarga-en-brotli.md`](decisiones/descarga-en-brotli.md) | CONFIRMADA · David · 2026-10-05 (P-3, I-152: los `.br` con su decodificador, sin InvariantGlobalization) |

## `conocimiento/` — referencia técnica

| Documento | Contenido |
|---|---|
| [`conocimiento/requisitos.md`](conocimiento/requisitos.md) | Requisitos con su referencia NOM y la prueba que los verifica. |
| [`conocimiento/seleccion-conductor-y-proteccion.md`](conocimiento/seleccion-conductor-y-proteccion.md) | Matriz de trazabilidad: selección de conductor y protección contra la NOM. |
| [`conocimiento/motor-copiado.md`](conocimiento/motor-copiado.md) | Alcance copiado de `PowerNode-DesignSuite`, ajustes y cambios posteriores. |
| [`conocimiento/trazabilidad-cargas-y-circuitos.md`](conocimiento/trazabilidad-cargas-y-circuitos.md) | Cada commit del cambio «cargas y clases de circuito», con su fase, hallazgo y verificación. |
| [`conocimiento/tablas-de-la-norma.md`](conocimiento/tablas-de-la-norma.md) | Lectura de la NOM desde JSON y verificación `--check`. |
| [`conocimiento/cuadro-de-carga-excel.md`](conocimiento/cuadro-de-carga-excel.md) | Estructura del Excel original. |
| [`conocimiento/marca.md`](conocimiento/marca.md) | Logo, colores y tipografía. |

## `historico/` — documentos cerrados

Vacío.

## Binarios

| Archivo | Qué es |
|---|---|
| `portada.png` | Captura que encabeza el README: la vista «Cuadro de carga» con un tablero de ocho circuitos, a 1600 × 900 en tema claro. Se regenera con Playwright contra `http://127.0.0.1:5199`. |
| `arquitectura.webp` | Diagrama de arquitectura del README: aplicación web, modelo eléctrico, cálculo, datos normativos y entregables, con el archivo de cada bloque. Generado con [GitDiagram](https://github.com/ahmedkhaleel2004/gitdiagram) (Ahmed Khaleel) y entregado por David el 2026-09-25. |

---

## Convenciones de esta carpeta

- **Nombres en minúsculas y con guiones.** Sin espacios, acentos ni paréntesis. Excepción: los cuatro
  de `estado/`, en mayúsculas para que se vean primero.
- **Una decisión por archivo**, con su estado en la primera línea.
- **Los enlaces entre documentos son relativos** y dentro de `docs/` van sin el prefijo `docs/`.
