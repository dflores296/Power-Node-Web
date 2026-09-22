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
