# Cómo se redactan los avisos de la captura

**Guía de David · 2026-10-07** — escrita por David en la conversación del 2026-10-07; aquí, con sus palabras.
Reemplaza la R3 que propuso Claude en [`acomodo-del-desplegable.md`](acomodo-del-desplegable.md). Lo que
Claude preguntó sobre ella, al final. Falta que David la marque CONFIRMADA.

Se aplica a todo lo que la tabla le dice al usuario: errores, advertencias y ayudas al pasar el cursor.
Inventario de lo que hay hoy: [`../conocimiento/avisos-de-la-captura.md`](../conocimiento/avisos-de-la-captura.md).

## Objetivo

Los avisos no son documentación técnica, comentarios de código, decisiones de arquitectura ni justificación
normativa. Su única función es:

- informar una condición detectada,
- indicar el impacto,
- indicar la acción requerida.

## Taxonomía

| Clase | Qué es | Formato | Ejemplos |
|---|---|---|---|
| **Error** | Impide realizar el cálculo. | Falta [dato requerido]. | Falta la tensión de placa. · Falta la corriente de placa. · El grupo no tiene motores. |
| **Advertencia** | El cálculo existe, pero hay una condición que debe revisarse. | [Condición detectada]. [Acción recomendada]. | Contactos de 15 o 20 A en un circuito de 30 A. Divídelos en otro circuito. · Caída de tensión combinada de 6.2 %. Reduce la longitud o aumenta el conductor. · Ningún tubo admite todos los conductores. Reparte los circuitos. |
| **Información** | Aclara un valor o comportamiento. | [Qué significa el dato]. | La clase del circuito se determina por sus cargas. · Las cargas continuas se consideran al 125 %. · Los circuitos de la misma canalización se agrupan. |

Evitar: «El cálculo no puede realizarse porque el artículo 430-22(e) requiere…», «Conforme a la NOM…».

## Lenguaje

- **De usuario.** Hablar de circuitos, motores, conductores, protecciones, tableros y canalizaciones. No de
  algoritmos, motores de cálculo, reglas internas, decisiones de diseño, validaciones del sistema ni modelos
  de datos. ✅ «Falta la tensión de placa.» ❌ «No fue posible resolver el cálculo porque la entidad no
  contiene información suficiente.»
- **Presente.** ✅ «El grupo no tiene motores.» ✅ «La caída de tensión supera el límite recomendado.»
  ❌ «El grupo fue configurado sin motores.» ❌ «Se detectó que la caída de tensión ha excedido.»
- **Voz activa.** ✅ «Escoge los minutos del servicio.» ✅ «Agrega los HP del motor.» ❌ «Los HP del motor
  deben ser capturados.»
- **Longitud.** Una oración; máximo dos. Ideal, 5 a 15 palabras; aceptable, hasta 25. Sin párrafos.

## Referencias normativas

Los avisos no justifican: describen. La referencia normativa no forma parte del mensaje principal.

- Correcto: «Falta la corriente de placa.» Y al pasar el cursor: «Corriente usada para motores de servicio
  no continuo · 430-22(e)».
- Incorrecto: «Falta la corriente de placa conforme al artículo 430-22(e) de la NOM-001-SEDE-2012.»

## Estructura

- **Errores:** Falta + dato. «Falta la tensión de placa.» «Falta la corriente de placa.» «Faltan los HP del
  motor.»
- **Advertencias:** condición detectada + acción. «El circuito supera el 5 % de caída. Revísalo.» «Ningún
  tubo admite esta instalación. Reparte los circuitos.» «El contacto debe ir en un circuito dedicado.
  Sepáralo.»
- **Ayudas emergentes (al pasar el cursor):** qué es el campo o qué representa el valor · artículo.
  «Corriente usada para seleccionar el conductor. · 210-19(a)(1)» «Longitud usada para calcular la caída de
  tensión. · Tabla 9» «Protección máxima permitida por el fabricante. · 110-3(b)»

## Lo que no se escribe

«Este comportamiento existe porque…», «La aplicación calcula…», «El sistema determina…», «Por diseño…»,
«Durante el cálculo…», «Se implementó…», «Se decidió…», «La arquitectura considera…», «Internamente…».
Convierten el aviso en un comentario de desarrollo.

## Lo que preguntó Claude y contestó David (2026-10-07)

1. **Errores por conflicto** — aceptado. Además de «Falta [dato].», un error puede ser un conflicto entre
   datos: **[Conflicto]. [Acción].** «El contacto del refrigerador va solo. Pasa las demás cargas a otro
   circuito.»
2. **El artículo de una advertencia (el ⚠ del renglón)** — A: en el mismo cuadro de ayuda, en una segunda
   línea apartada: «· Tabla 210-21(b)(3)». Con la opción A de [`../conocimiento/estados-de-aviso.md`](../conocimiento/estados-de-aviso.md) el ⚠ sale:
   ese cuadro de ayuda es el de la celda enmarcada.
3. **La acción tiene que poderse hacer en la pantalla.** Si no hay una acción concreta, «Revísalo.»
4. **Los mensajes del motor de cálculo** — A: la pantalla traduce los casos conocidos al formato de esta
   guía, sin tocar el motor (se copia, no se reescribe); los desconocidos dicen «No se puede calcular el
   circuito. Revisa sus datos.» y el mensaje completo va a la memoria.
5. **Alcance** — B: toda la página de captura (ficha, cuadro, desplegable, canalizaciones, resumen,
   alimentador, tarjeta «Avisos», avisos flotantes). La memoria de cálculo no: ahí va la justificación.
6. **Ayuda de una casilla** — «qué es» es la condición que marca: «El variador tiene bypass a la línea ·
   430-122(b)».

David, sobre las acciones: «estandarizar las recomendaciones igual que todos los avisos».

## Las acciones, estandarizadas (propuesta de Claude; aprobada por David el 2026-10-07)

Cada acción empieza con uno de estos verbos, en imperativo y de tú, y dice dónde se hace si no es en el mismo
renglón.

| Situación | Verbo | Forma | Ejemplo |
|---|---|---|---|
| Falta un dato | — | Falta / Faltan + dato. El campo, en rojo. | Faltan los HP del motor. |
| Un valor de una lista no vale | Escoge | Escoge + qué. | Escoge otra tensión. |
| Una carga no puede ir con las demás | Pasa | Pasa + qué + a otro circuito. | Pasa las demás cargas a otro circuito. |
| Muchas salidas en un circuito que no las admite | Divide | Divide + qué + en circuitos de N A. | Divide los contactos en circuitos de 20 A. |
| Canalizaciones | Reparte | Reparte + qué + en más canalizaciones. | Reparte los circuitos en más canalizaciones. |
| Algo sobra | Quita | Quita + qué. | Quita las líneas sin carga. |
| Un ajuste de otra sección | Cambia / Baja / Sube | Verbo + qué + en + sección. | Baja la caída permitida en Condiciones de cálculo. |
| Nada en la pantalla lo resuelve | Revisa | Revísalo + con quién, si aplica. | Revísalo con el fabricante. |

La condición nombra el elemento (circuito, contactos, motor, tubo) y el número que importa: «Caída combinada
de 6.20 %.», «Contactos de 15 o 20 A en un circuito de 30 A.».
