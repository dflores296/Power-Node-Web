# Hallazgos

Cada fila es un defecto o un cabo suelto real, con **ID estable** que no cambia aunque el documento
se reordene. Patrón de `msa-toolkit` (`docs/auditoria-2026-08-31.md`): **el hash de commit es la
prueba de que se cerró**, no una palabra.

ID: `<letra>-<número>`. La letra dice el frente (`M` motor, `I` interfaz, `P` publicación,
`E` estructura). Prioridad `P0` (bloquea) → `P3` (cosmético).

| ID | Hallazgo | Prioridad | Estado | Commit |
|---|---|---|---|---|
| E-01 · El motor traía código de coordinación mezclado dentro de `TablasNom` | P2 | **Cerrado** | este commit |
| M-01 · No hay implementación de las tablas de la norma sin base de datos | P0 | Pendiente | — |
| I-01 · No existe ninguna pantalla todavía | P0 | Pendiente | — |
| P-01 · No hay workflow de despliegue a GitHub Pages | P1 | Pendiente | — |

---

### E-01 — `FamiliaAislamiento` vivía dentro de `Coordinacion/CurvaDanioConductor.cs` · este commit

Al copiar `Calculo/` excluyendo `Coordinacion/` (decisión: coordinación es v2), la compilación
reventó porque `ITablaAislamiento.cs` — que **sí** es del alcance base, decide qué columna de
ampacidad usar — dependía de un enum, `FamiliaAislamiento`, que físicamente vivía adentro del
archivo de verificación de daño térmico del conductor contra la curva de disparo. Dos
responsabilidades sin relación (la ampacidad tabulada de un circuito común, y la coordinación
tiempo-corriente contra una protección) compartían un archivo por conveniencia de cuándo se escribió,
no por diseño.

**Corregido aquí** moviendo el enum a su propio archivo en `TablasNom/FamiliaAislamiento.cs`, donde
ya vive el resto de lo que usa `ITablaAislamiento`.

**Vale la pena reportarlo en `PowerNode-DesignSuite`** (el repo de escritorio, de donde salió) —
ahí el acoplamiento no rompe nada porque `Coordinacion` siempre está presente, pero es la misma
mezcla de responsabilidades. No se tocó ese repo desde aquí.
