# Alcance de v1: un solo cuadro de carga

**CONFIRMADA · David · 2026-09-22**

## La decisión

La versión web calcula **un tablero, no varios encadenados**. Nada de cascada, catálogo de equipos
ni coordinación de protecciones — eso es lo que `PowerNode-DesignSuite` (escritorio) ya tiene y a
donde "se fue más allá" del alcance de esta versión.

## Por qué

Es exactamente lo que hacía `CC_NOM_2012_nuevo_nube.xlsx`, el Excel original que dio origen a todo
el proyecto: **1 archivo = 1 cuadro de carga**, hasta 42 espacios (21 nones a la izquierda, 21 pares
a la derecha), tipo NQ/NF. Documentado celda por celda en
`PowerNode-DesignSuite/docs/referencia/cuadro-de-carga.md`.

Antes de esta decisión se había planteado llevar el motor completo (incluido el catálogo Square D y
la coordinación de protecciones) a la web. David lo cortó: mejor construir primero lo que el Excel
ya resolvía —con la ventaja de la cita automática de norma que el Excel nunca tuvo— y avanzar desde
ahí, no partir de lo más grande que ya existe en escritorio.

## Lo que se sigue de aquí

- `Calculo/Coordinacion/` no se copia.
- `Domain/Catalogo/` no se copia (también por razón legal, ver
  [`sin-catalogo-square-d.md`](sin-catalogo-square-d.md)).
- De `Domain/Proyectos/`, tampoco viajan las entidades de coordinación
  (`AjustesDisparoCapturados`, `CoberturaDeCoordinacion`, `CoordinacionDeProtecciones`) ni el nodo
  `Proteccion` (topología encadenada — no aplica a un tablero aislado).
- El captador de datos es manual: sin modelo de catálogo, todo se teclea (calibre, protección,
  longitud) — el mismo camino que ya soporta y prueba el motor cuando no hay modelo elegido.

## Qué no cambia

`Calculo/Casos` (el cálculo de un circuito), `Tableros` (distribución de barras nones/pares),
`Simbologia`, y las entidades `Tablero`/`CircuitoDerivado`/`ElementoCircuito` viajan completas — son
justo lo que un cuadro de carga necesita.
