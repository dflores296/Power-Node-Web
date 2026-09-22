# Hallazgos

Cada fila es un defecto o un cabo suelto real, con **ID estable** que no cambia aunque el documento
se reordene. Patrón de `msa-toolkit` (`docs/auditoria-2026-08-31.md`): **el hash de commit es la
prueba de que se cerró**, no una palabra.

ID: `<letra>-<número>`. La letra dice el frente (`M` motor, `I` interfaz, `P` publicación,
`E` estructura). Prioridad `P0` (bloquea) → `P3` (cosmético).

| ID | Hallazgo | Prioridad | Estado | Commit |
|---|---|---|---|---|
| E-01 · El motor traía código de coordinación mezclado dentro de `TablasNom` | P2 | **Cerrado** | este commit |
| M-01 · No hay implementación de las tablas de la norma sin base de datos | P0 | **Cerrado** | este commit |
| I-01 · No existe ninguna pantalla todavía | P0 | **Cerrado** | este commit |
| P-01 · No hay workflow de despliegue a GitHub Pages | P1 | **Cerrado** (sin ejecutar) | este commit |
| I-02 · `NumeroFases` se pasaba del tablero, no del circuito | P0 | **Cerrado** | este commit |
| I-03 · El piso práctico de calibre no se aplicaba | P1 | **Cerrado** | este commit |
| P-02 · GitHub Pages no está activado en el repo | P1 | Pendiente (lo hace David) | — |
| I-04 · No hay resumen de carga ni balanceo por fase | P2 | Pendiente | — |
| I-05 · No se puede guardar ni abrir un proyecto | P1 | Pendiente | — |

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

### I-02 — `NumeroFases` era el del tablero, no el del circuito · este commit

**Bug real, encontrado corriendo la pantalla, no compilándola.** Un circuito de 1 polo con 720 VA en
un tablero de 220 V 3F daba **1.89 A** en vez de **5.67 A** — exactamente un tercio. La pantalla
pasaba `NumeroFases: 3` (las del tablero), y el motor repartía la carga entre las tres fases como si
fuera una carga trifásica.

`NumeroFases` es **cuántas barras toca el circuito**, o sea sus polos. El escritorio lo resuelve con
`circuito.Fase.Length` (`CalculoCircuitoDerivadoService.MapearEntrada`). Corregido pasando
`c.Polos`, y de paso se agregó la columna de polos a la pantalla — que además es fiel al Excel, que
lleva una columna `P × A`.

### I-03 — Faltaba el piso práctico de calibre · este commit

Con el bug anterior corregido, la web daba **14 AWG / 2.07 %** donde el escritorio da
**12 AWG / 1.34 %**, y los dos cumplen el límite de caída del 3 %. La diferencia no era del cálculo:
`ConfiguracionProyecto` en escritorio trae un **piso práctico** por omisión —12 AWG en Alumbrado,
10 AWG en Contactos— más estricto que el normativo (14 AWG, 210-19(a)(4)), y la pantalla pasaba
`PisoPracticoCalibreMm2: null`.

Es criterio de diseño, no norma, y por eso vive en la configuración y no en el motor. Ahora la
pantalla lo aplica según el tipo de carga, con los mismos valores por omisión. **Equipo se queda sin
piso a propósito**: un aparato se dimensiona por su consumo de placa.

### M-01 — Las tablas de la norma, sin base de datos · este commit

`PowerNode.DesignSuite.Normativa` implementa las trece interfaces de `Calculo/TablasNom` leyendo un
JSON de **39 KB** en vez de SQL Server. Detalle en
[`../conocimiento/tablas-de-la-norma.md`](../conocimiento/tablas-de-la-norma.md).

**Hallazgo lateral, que quedó fijado con una prueba:** la lista de valores estandarizados de
240-6(a) de la **NOM incluye los valores IEC** —16, 32, 63— que el NEC no tiene. La primera versión
de la prueba esperaba que el inmediato superior de 15.1 A fuera 20 A, por costumbre del NEC, y falló:
son **16 A**. El código estaba bien; la prueba estaba mal.
