# `Magnitudes/` — qué es andamio y qué es deuda

Este módulo es la base de rigor de ingeniería del motor: tipos puros, sin estado, con sus pruebas.
La auditoría del 2026-08-19 (§6.2) lo marcó como **"biblioteca a medio consumir"** en su versión
benigna — la trampa de `docs/estado/04-trampas.md` — y pidió una de dos salidas honestas: conectar lo que
de verdad simplifique el motor, o **etiquetar** el resto como base de lo que viene.

> **Lo que no conviene es dejarlos sin etiqueta**, porque la próxima sesión no sabrá si son andamio
> o deuda, y acabará borrándolos "por muertos" o construyendo otra vez lo mismo al lado.

Esta tabla es esa etiqueta. **`MagnitudesEtiquetadasTests` falla si aparece un tipo que no está
aquí**, así que el inventario no se puede quedar atrás sin que alguien se entere.

## Estado

| Tipo | Estado | Por qué |
|---|---|---|
| `Impedancia` | **En uso** | Caída de tensión y cortocircuito. Ocho archivos del motor lo consumen. |
| `SistemaTrifasico` | **En uso** | Tensiones y corrientes de línea/fase, conexión estrella/delta. |
| `TrianguloPotencias` | **En uso desde 2026-08-19** | `SenoDelAngulo` es el `sen θ` de la fórmula exacta de caída de tensión. Se conectó al cerrar §6.2: estaba calculado a mano en el motor y en la memoria, **con guardas distintas**, así que un factor de potencia mal capturado daba `NaN` en el cálculo y un número en el papel. |
| `Fasor` | **Base, a propósito** | Es el tipo con el que se escribe un análisis de flujos o de fallas asimétricas. Hoy el motor resuelve todo con magnitudes reales y ángulo implícito, que alcanza para v1. |
| `Admitancia` / `Conductividad` | **Base, a propósito** | La forma en paralelo de la impedancia. La necesita el análisis nodal; el motor de v1 solo recorre ramas en serie. |
| `Resistividad` | **Base, a propósito** | Hoy solo la usa `Admitancia`. Su consumidor natural es calcular R desde el material y la geometría, en vez de leerla de la Tabla 9 — que es lo que v1 hace, y hace bien: la tabla **es** la referencia normativa. |
| `PrefijoSI` + extensiones | **Base, a propósito** | Presentación de magnitudes (kA, MVA, mΩ). Su lugar es la UI y los entregables; hoy cada uno formatea con `:N2` y su unidad escrita a mano. |
| `Reactancia` | **Base, a propósito** | Helpers de X<sub>L</sub>/X<sub>C</sub> desde L, C y frecuencia. El motor lee la reactancia de la Tabla 9 a 60 Hz, así que no la deriva. |

## La regla

**"Base, a propósito" no es permiso para crecer.** Un tipo nuevo aquí sin consumidor necesita una
razón escrita en esta tabla — la misma vara que `TablaRotorBloqueadoEf`, que existe antes que su
consumidor porque su artículo (430-52(c)(1) Excepción 2) está fuera de v1 **por decisión**, no por
olvido.

Y al revés: si uno de estos tipos se conecta, se mueve a **"En uso"** con la fecha y el porqué. Es
el registro que evita la pregunta "¿esto sirve para algo?" en la sesión número doce.
