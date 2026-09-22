# El motor se copia, no se enlaza con `git submodule`

**CONFIRMADA · David · 2026-09-22**

## La decisión

`Calculo/` y `Domain/` viven **copiados** en este repo, como archivos propios — no como un
`git submodule` apuntando a `PowerNode-DesignSuite`.

## Por qué

Se probó primero el submodule y se revirtió el mismo día. Dos problemas reales, no preferencia:

1. **`Power-Node-Web` es público y `PowerNode-DesignSuite` es privado.** Un submodule de un repo
   privado dentro de uno público funciona clonado a mano, pero **GitHub Actions no puede clonarlo**
   para construir el sitio sin darle una credencial — y esa credencial, en un repo público, es una
   llave de acceso al repo privado expuesta en la configuración de CI.
2. David quería que `Power-Node-Web` fuera **independiente** del escritorio, no un espejo en vivo.

## El costo que esto acepta

Un arreglo al motor en `PowerNode-DesignSuite` **no llega solo** a este repo. Hay que traerlo a
mano, revisando qué cambió desde el commit de origen (`29f660f` a la fecha de esta decisión).

**Mitigación:** los namespaces se dejaron idénticos (`PowerNode.DesignSuite.Calculo`,
`PowerNode.DesignSuite.Domain`) justo para que comparar contra el repo de origen sea un diff de
carpetas, no una reescritura.

## Qué se copió, y con qué ajustes

Detalle completo en [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md).
