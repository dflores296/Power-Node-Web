# Los hallazgos se rastrean en markdown, dentro del repo

**CONFIRMADA · David · 2026-09-22**

## La decisión

`docs/estado/HALLAZGOS.md`, no GitHub Issues. Patrón de `msa-toolkit`: ID estable, prioridad,
estado, **hash del commit que cierra** — el hash es la prueba, no una palabra.

## Por qué

Se consideraron tres opciones: markdown solo, GitHub Issues + Projects, o los dos combinados.
David eligió markdown solo. Razones a favor que se discutieron:

- Se versiona junto con el código — un `git log` del archivo **es** el historial de la auditoría.
- Cualquier sesión de Claude Code lo lee y lo escribe sin fricción de API ni permisos aparte.
- Sobrevive a clonar el repo en otra máquina u otra cuenta, sin depender de que GitHub esté
  accesible.

Lo que se cede: sin notificaciones automáticas, sin enlace `Fixes #12` que cierre algo solo, sin
tablero visual de arrastrar tarjetas. Si algún día hace falta ese nivel de seguimiento, se puede
migrar — pero no se empieza ahí.
