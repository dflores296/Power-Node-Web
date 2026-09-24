# Sin piso práctico de calibre: el conductor sale del cálculo, no de una costumbre

**CONFIRMADA · David · 2026-09-22**

## La decisión

Esta web **no aplica ningún piso de calibre por tipo de carga**. El conductor de cada circuito sale
de lo que manda el cálculo: ampacidad corregida (310-15(b)(16) con sus factores), la protección que
le toca, y el límite de caída de tensión capturado. Nada más.

## Qué se quitó

La versión de escritorio trae **encendido por omisión** un piso más estricto que el normativo, en
`ConfiguracionProyecto` y capturable ahí:

| Tipo de carga | Piso en escritorio |
|---|---|
| Alumbrado | 3.31 mm² — 12 AWG |
| Contactos | 5.26 mm² — 10 AWG |
| Equipo | *(ninguno)* |

La web los traía **fijos, sin poder apagarlos** — se agregaron el 2026-09-22 como el hallazgo I-03,
precisamente para dar los mismos números que el escritorio.

## Por qué se quita

Palabras de David, con el caso enfrente:

> *«me recomiendas calibre 10 para contactos pero 12 para aire acondicionado con la misma carga por
> fase»*

Y tiene razón: esa diferencia **no la produce ningún artículo de la norma**, la producía el piso.
Medido antes de tocar nada, con 1 500 VA a 20 m en un 3F-4H de 220 V:

| Tipo | In | Protección | Conductor **antes** | Conductor **ahora** |
|---|---|---|---|---|
| Alumbrado | 11.81 A | 15 A | 12 AWG | 12 AWG |
| Contactos | 11.81 A | 20 A | **10 AWG** | **12 AWG** |
| Equipo | 11.81 A | 15 A | 12 AWG | 12 AWG |

El 12 AWG del equipo nunca fue un piso: lo pedía la **caída de tensión** (2.24 % con 12; con 14 se
pasaba del 3 %). O sea que el único número inventado era el 10 AWG de contactos.

**Un entregable que se firma no debería llevar cobre que ningún artículo pide.** Si el criterio del
despacho es no bajar de cierto calibre, eso se decide circuito por circuito y se captura, no se
aplica a escondidas.

## Lo que NO se quitó, y hay que saberlo

**El piso de PROTECCIÓN de 15 A (Alumbrado) y 20 A (Contactos) sigue ahí**, y con cargas chicas
vuelve a separar a contactos de los demás — pero ya no por una costumbre, sino por la cadena
protección → ampacidad mínima del conductor. Con 500 VA a 10 m:

| Tipo | In | Protección | Conductor |
|---|---|---|---|
| Alumbrado | 3.94 A | 15 A | 14 AWG |
| Contactos | 3.94 A | **20 A** | **12 AWG** |

Ese piso **vive dentro del motor copiado** (`CalculadoraCircuitoDerivadoNoMotor`) y viene del propio
Excel, que hacía `MAX(20, …)` para contactos y `MAX(15, …)` para alumbrado. Por la regla de la casa
—*el motor se copia, no se reescribe*— **no se toca desde aquí**: si algún día se quiere quitar o
hacer configurable, se decide y se prueba en `PowerNode-DesignSuite` y esta web lo recibe al
recopiar el motor.

## Consecuencia que conviene tener presente

**La web y el escritorio ya no dan el mismo calibre para el mismo circuito.** Con la configuración
por omisión de escritorio, un circuito de alumbrado de 720 VA a 20 m sale ahí en **12 AWG** y aquí
en **14 AWG**; los dos cumplen la norma y los dos cumplen el 3 % de caída. No es un defecto de
ninguno de los dos: es que allá el criterio del despacho está encendido y aquí no existe.

Si esto estorba, hay dos caminos, y los dos son de David:

1. **Apagar el piso en el escritorio** (Configuración del proyecto), y entonces los dos coinciden.
2. **Traerlo a la web como captura** —un campo por proyecto, apagado por omisión— en vez de una
   constante escondida.

---

**Actualización · 2026-09-24:** el piso de **protección** de 15/20 A también se quitó — ver
[`minimo-de-proteccion-por-uso.md`](minimo-de-proteccion-por-uso.md). Con 500 VA a 10 m, alumbrado y
contactos salen iguales: 15 A, 14 AWG. Solo los contactos de vivienda de cocina, lavadora y baño
llevan 20 A, por 210-11(c).
