# Los dos criterios del Excel para el interruptor principal: se avisan, no se aplican

**PROPUESTA · Claude · 2026-09-22**

## La decisión que se propone

El interruptor principal del tablero sale **del motor** (`CalculadoraAlimentador`, o sea 215-3 +
240-6(a)), tal cual. Los dos ajustes que el Excel original le hacía encima **se reportan como aviso
junto al resultado, y no se aplican solos**.

## Cuáles son

La celda `CZ79` del Excel, traducida:

```
protección = siguiente tamaño estándar ≥ capacidad mínima
si protección < 30      → 30
si protección == max(protecciones de los derivados) → siguiente tamaño estándar
```

1. **Piso de 30 A.** Ningún tablero salía con principal de menos de 30 A.
2. **No empatar con el derivado más grande.** Si el principal daba lo mismo que el derivado mayor,
   subía un tamaño.

## Por qué no se aplican solos

**Ninguno de los dos está en la NOM.** El 240-6(a) da la lista de tamaños estándar y el 215-3 dice
cómo entrar a ella; ni uno ni otro habla de un piso de 30 A ni de separar el principal del derivado
mayor. El segundo es además un pariente empobrecido de la coordinación selectiva, que es **v2** y
que un tamaño de diferencia no resuelve: dos interruptores termomagnéticos consecutivos se disparan
juntos ante una falla franca, y lo que decide si hay selectividad son las curvas, no los amperes.

Y hay una razón de regla de casa: **el motor se copia, no se reescribe**
(`motor-copiado-no-enlazado.md`). Meterle a la web una regla de selección que el escritorio no tiene
haría que los dos programas dieran números distintos para el mismo tablero — y el que está mal
sería el de escritorio, sin que nadie lo hubiera decidido.

## Por qué tampoco se callan

Son **criterio de diseño de David**, no un error del Excel: si el tablero que se va a comprar nunca
baja de 30 A de principal, el número calculado no es el que se compra. Callarlo obligaría a
acordarse. Así que la pantalla dice, por ejemplo:

> El interruptor principal calculado es de 25 A. El Excel original nunca bajaba de 30 A. Es criterio
> de diseño, no de la NOM.

## Qué hace falta para cerrarla

**David decide una de tres**, y según cuál sea el código cambia:

- **Se quedan como avisos** (lo propuesto) — no hay que tocar nada.
- **Se aplican, y se aplican también en el escritorio** — entonces la regla entra a `Calculo` en
  `PowerNode-DesignSuite`, se prueba ahí, y este repo la recibe al recopiar el motor.
- **Se aplican solo aquí, como opción de captura** — una casilla «piso de 30 A» en la ficha del
  tablero, apagada por omisión, y la memoria tiene que decir que ese número es criterio de diseño y
  no resultado del 240-6(a).

---

## Actualización — los textos de los avisos, y un tercer aviso que sí es error

**PROPUESTA · Claude · 2026-09-23** — hallazgos I-28 y M-03 (`../estado/HALLAZGOS.md`).

### I-28 · Los avisos citan «el Excel original» en la pantalla

Los textos de arriba se imprimen tal cual en la captura y en el documento. **El usuario de la web no
sabe qué Excel es**, y el archivo ni siquiera vive en este repo
(`PowerNode-DesignSuite/docs/referencia/`). Es contexto de desarrollo filtrado a la interfaz: el
origen del criterio tiene que quedarse **en este archivo**, no en la pantalla.

**No se cambió el texto todavía**: es cómo se le habla al usuario, y lo decide David. Dos opciones:

- **A. Reescribirlos como criterio de diseño, sin mencionar el Excel.**
  - «El interruptor principal calculado es de 15 A. La NOM no fija un mínimo; es práctica común no
    bajar de 30 A en el principal para dejar margen de crecimiento. Criterio del proyectista.»
  - «El interruptor principal quedó igual que el derivado más grande (X A). La 240-6(a) lo permite;
    subirlo un tamaño mejora la selectividad. Criterio del proyectista.»
- **B. Convertirlo en un dato del tablero.** Un campo «Mínimo del interruptor principal (A)», vacío
  por omisión, y un aviso solo cuando el calculado quede debajo de lo que el proyectista pidió.

**Lo que propone Claude: B para el piso de 30 A y A para el empate.** El piso es un número que cada
proyectista tiene distinto —y con B deja de salir un aviso en *todos* los tableros chicos—; el
empate no es un número, es una observación, y basta con redactarla sin el Excel.

### M-03 · Principal menor que el derivado más grande

Distinto de los dos de arriba: **esto no es criterio de diseño, es un error de coordinación
básico** —el principal se dispara con una carga que el derivado sí admite—. Se agregó el
2026-09-23 como **aviso**, sin mencionar el Excel:

> El interruptor principal (15 A) es menor que el derivado más grande (20 A, circuito 3). El
> principal se dispararía con una carga que ese derivado sí admite: revisa la carga capturada o sube
> el principal.

**Queda por decidir si es aviso o bloqueo.** Claude propone que se quede en aviso: el número que se
imprime sigue saliendo del motor (215-3 + 240-6(a)), y bloquear obligaría a la pantalla a decidir
qué hacer con él. Si David prefiere que el principal suba solo al derivado mayor, es la misma
situación que el segundo criterio de arriba y se resuelve igual (en `Calculo`, en los dos repos, o
como opción de captura).
