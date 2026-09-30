# La carga se captura en el desplegable; el renglón solo resume

**PROPUESTA · Claude · 2026-09-30** · implementada el 2026-09-30 (`bb3caa8`, `8b13b80`, `51a2164`) — a pedido de David: «el desplegable es la fuente de verdad; el
renglón del circuito ya no captura carga». David contestó las dudas el mismo día (abajo). Falta que la
marque CONFIRMADA.

Sigue a [`cargas-y-clases-de-circuito.md`](cargas-y-clases-de-circuito.md) (implementada el
2026-09-30).

## El problema

Después de las fases B y C hay dos lugares para capturar la misma carga: las casillas del renglón
(unidad, continua, no continua, F.P.) y las líneas del desplegable. El tipo se escoge dos veces: en el
renglón y en cada línea. Un refrigerador solo pide «Tipo» arriba y «Tipo» abajo.

## La propuesta

**Una sola captura: las líneas del desplegable.** Un circuito tiene carga si tiene al menos una línea
con tipo.

| Columna del renglón | Hoy | Queda |
|---|---|---|
| Descripción | Etiqueta del espacio | Igual: la escribe el ingeniero |
| Tipo | Tipo de carga, seleccionable | **Clase del circuito, solo lectura**: Individual, Uso general, Para aparatos, Grupo de motores, Alimentador; «—» sin carga |
| Unidad | VA, W, A, HP, Ampac.… | **Se quita**: el renglón va en VA |
| Continua · No continua | Casillas | **Solo lectura**: la suma de las líneas, en VA |
| F.P. | Casilla | **Solo lectura**: el F.P. del conjunto |
| L, Canal., P, In, Protec., conductores, e | Igual | Igual |

**Cada línea del desplegable** lleva tipo, subtipo, cantidad, unidad, carga por unidad, la casilla
«Continua» y su F.P. La casilla se queda (David, respuesta 1). El calentador de agua y la calefacción
la llevan marcada siempre.

### Circuitos que la norma pide dedicados

Un subtipo que por norma va solo en su circuito no admite otra línea ni cantidad mayor que 1 (David,
respuesta 2):

| Subtipo | Por qué | Regla |
|---|---|---|
| Motor con velocidad ajustable | Un equipo de conversión de potencia con su protección de fabricante — 430-122, 110-3(b) | Solo, cantidad 1 |
| A/A con ampacidad de placa (MCA, MOCP) | La placa ya suma sus motores — 440-4(b) | Solo, cantidad 1 |
| Tablero alimentado | Es un alimentador — Art. 100, 215 | Solo con otros tableros, cada uno en su línea (David, 2026-09-30; 215-2(a)(1), 408-36, 240-21(b)) |
| Contactos · Refrigerador | Circuito derivado individual — 210-52(b)(1) Exc. 2 | Solo, cantidad 1 |
| Contactos · Ap. pequeños, Lavadora, Baño | Solo esas salidas — 210-52(b)(2), 210-11(c)(2), (3) | Solo líneas del mismo subtipo |

El tablero alimentado es la única línea con **dos cantidades**: continua y no continua del otro
tablero, ya con sus factores de demanda; aquí no lleva otro (220-40). Un alimentador puede llevar
varios tableros; su carga es la suma.

### El uso de vivienda pasa a subtipo de Contactos

Aparatos pequeños, Lavadora, Baño y Refrigerador dejan de ser un selector del renglón y son subtipos de
Contactos (David, respuesta 3). Siguen dando 20 A (210-11(c)), los 1500 VA del alimentador (220-52) y
el circuito individual del refrigerador.

### Lo que no cambia

- El desplegable se abre con la flecha, como hoy; no se abre solo (David, respuesta 4). La rapidez de
  captura se revisa después.
- El cálculo: las mismas calculadoras (210, 215, 430, 440) según las líneas.
- Los archivos guardados abren igual: lo capturado en el renglón pasa a ser su línea del desplegable,
  con los mismos números. No hay que hacer nada.

## Fases

| Fase | Qué |
|---|---|
| 1 | Modelo: subtipos de uso de vivienda; la captura del renglón como su primera línea; reglas de los circuitos dedicados; la clase «Grupo de motores»; archivo formato 7 |
| 2 | Pantalla: renglón de solo lectura, columna de clase, sin unidad; las formas de motor, variador, A/A y tablero en la línea del desplegable |
| 3 | Documento y memoria con la clase en lugar del tipo |

## Respuestas de David (2026-09-30)

1. La casilla «Continua» se queda en cada línea; el renglón solo suma.
2. Los circuitos que por norma son dedicados no admiten más líneas ni cantidad mayor que 1.
3. El uso de vivienda, subtipo de Contactos: «lo más correcto».
4. El desplegable no se abre solo; la rapidez, después.
5. El tablero alimentado, una línea con dos cantidades: «Sí, una línea con dos cantidades, arranca».
