# Dónde va el interruptor principal dentro del gabinete

**PROPUESTA · Claude · 2026-09-25** — ajustada con las respuestas de David del mismo día (abajo). La pidió al ver el gabinete en multifilar (I-67):
*«en un gabinete siempre los polos del interruptor principal es igual a su número de barras»*;
*«[en 2 barras] puede tener un zócalo especial para él abajo, o puede ir dentro de un espacio […] por
lo general […] los últimos espacios del lado derecho. Aunque en realidad ese valor es más un
default»*; *«[en 1 barra] no tiene espacio para interruptor principal, eso es seguro»*. Falta que David
la confirme.

**Respuestas de David, 2026-09-25**: 1F-2H arranca con zapatas; en 2 y 3 barras, el principal va
por omisión en zócalo; en 1F-2H, con principal, en el primer espacio.

## Lo que ya está y no cambia

- **Polos del principal = número de barras**: 3 barras, 3 polos; 2, bipolar; 1, un polo. Ya sale así
  del motor; no se captura.
- **«Acometida»** sigue siendo el dato que decide si hay principal: *zapatas principales* (no hay
  principal en el tablero) o *interruptor principal*. El cálculo del principal (215-3, 240-6(a)) no
  cambia.

## Lo nuevo: el montaje

Con interruptor principal, un dato más, **«Montaje del principal»**:

| Barras | Opciones | Por omisión |
|---|---|---|
| 2 o 3 | **Zócalo propio**, abajo: no ocupa espacios numerados. **En espacios del gabinete**: se come tantos espacios como polos, como un derivado. | **Zócalo.** Al pasarlo a espacios, arranca en los últimos pares: 20-22-24 en un tablero de 24 y 3 barras; 22-24 con 2 barras. Si ahí hay un circuito, sube por la misma columna (`AcomodoEnGabinete.UltimoHuecoDeLaColumnaPar`, la regla del escritorio; I-69). |
| 1 (1F-2H) | Solo **en un espacio**. No hay zócalo. | El espacio 1, o el primero libre. |

- **La posición en espacios es un valor por omisión, no una regla**: se puede mover al espacio que
  sea, con las mismas reglas de un derivado (que quepa, que no pise otro interruptor, que caiga en
  barras distintas).
- **Los espacios del principal no admiten circuito.** El renglón del cuadro de carga dice
  «Interruptor principal» y no se captura carga ahí. Si al elegir el montaje ya había un circuito en
  los espacios que se eligieron, no se borra: se avisa y el principal no se monta hasta que se mueva uno
  de los dos. Se arrastra como un circuito (I-69).
- **Las cuentas del gabinete lo incluyen**: «16 de 24 espacios ocupados» cuenta los del principal.
- **1F-2H arranca con zapatas principales** (David): un centro de carga de una barra casi siempre
  viene protegido desde otro tablero. Los demás sistemas siguen arrancando con interruptor principal.
  Cambiar a 1F-2H pone zapatas; salir de 1F-2H regresa al interruptor principal.

## Cómo se dibuja (multifilar, I-67)

- **Zapatas principales**: abajo de las barras, donde iría el zócalo: un borne por barra con la
  acometida entrando por abajo. Sin interruptor. En 1F-2H, el borne va al inicio de la barra.
- **Zócalo propio (2 o 3 barras)**: abajo de la tapa, el principal **de frente**, con la pinza hacia
  arriba: a 1.2, un polo mide 24, lo mismo que la separación entre barras, así que cada polo cae justo
  debajo de su barra y la alimenta. La acometida entra por abajo. Sustituye a la franja «Interruptor
  principal» de hoy.
- **En espacios**: el principal en sus espacios, como un derivado, pero **distinguible**: tapa con el
  relleno del acento más fuerte, directorio en negritas «Interruptor principal · 225 A», y sus
  conexiones con una flecha **hacia** la barra (alimenta; los derivados toman de ella). La acometida
  llega por afuera, al directorio.
- **1F-2H**: igual que «en espacios», en su columna, con la flecha hacia la barra horizontal.

## Lo que toca

- `DatosDelTablero`: el montaje y el espacio inicial del principal; el archivo `.powernode.json` los
  guarda y un archivo viejo abre con el valor por omisión (`archivo-del-tablero.md`).
- `CuadroDeCarga`: reserva los espacios del principal y valida como a un derivado. Es de la web; el
  motor copiado no cambia (`motor-copiado-no-enlazado.md`).
- `InteriorDelGabinete.razor` y el cuadro de carga impreso: dibujan el principal donde va.
