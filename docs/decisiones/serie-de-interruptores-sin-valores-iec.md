# Serie de interruptores: la 240-6(a) completa, u opcionalmente sin los valores IEC

**PROPUESTA · Claude · 2026-09-23** — hallazgo I-29 (`../estado/HALLAZGOS.md`).

## El problema

La lista de valores normalizados de la 240-6(a) **de la NOM-001-SEDE-2012** incluye 16, 32 y 63 A,
que el NEC no tiene (quedó fijado con prueba desde M-01). El motor la usa completa, así que en la
prueba del 2026-09-22 la air fryer (15.25 A de capacidad mínima) salió con **16 A** — correcto según
la norma, pero en un centro de carga tipo QO/NQ no existe un interruptor de 16 A: esos valores son de
interruptores DIN (tipo IEC).

## La decisión que se propone

Un dato del tablero, **«Serie de interruptores»**, con dos valores:

| Valor | Qué hace |
|---|---|
| **NOM completa** *(por omisión)* | La lista de la 240-6(a) tal cual. No cambia nada de lo que ya se calcula. |
| **Sin valores IEC (16, 32, 63)** | La misma lista sin esos tres. La air fryer quedaría en **20 A**, que también cumple 240-6(a). |

Se implementa como un decorador de `ITablaProteccionEstandar` en `PowerNode.Web.Modelo` que filtra
`ValoresEstandar` y resuelve `SiguienteEstandar`/`AnteriorEstandar` sobre la lista filtrada, y
`MotorNom` arma las calculadoras con el que corresponda. **El motor no se toca**: recibe la
interfaz, como siempre.

## Por qué no rompe `sin-catalogo-square-d.md`

No es catálogo: no dice qué modelos existen ni de qué marca. Es un **subconjunto de la lista de la
propia norma**, y la regla para elegir dentro de ella sigue siendo la del 240-6(a).

## Lo que hay que cuidar

- **La memoria tiene que decirlo.** Si se eligió «sin valores IEC», la cita del 240-6(a) debe
  decir que se omitieron 16, 32 y 63 A por criterio del proyectista — si no, quien revisa verá un
  salto de 15.25 A a 20 A que la norma no explica.
- **Afecta derivados y principal por igual.** Un principal de 16 A tampoco existe en un QO.

## Qué hace falta para cerrarla

David confirma o descarta. Si se confirma, prueba de regresión con el caso de los tres aparatos:
con «sin valores IEC», air fryer y principal en 20 A.
