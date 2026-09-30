# Cargas y clases de circuito: el tipo es de la carga, la clase es del circuito

**PROPUESTA · Claude · 2026-09-29** — a pedido de David: «la columna tipo describe el tipo de carga, y no
tenemos tipo de carga para los aparatos; no tenemos clasificación para circuitos individuales ni con
cargas combinadas». David contestó las preguntas el mismo día («Decisiones de David», abajo). Falta que
la marque CONFIRMADA.

Amplía [`tipos-de-carga.md`](tipos-de-carga.md) (los seis tipos, I-74) y
[`motores-y-equipos-en-grupo.md`](motores-y-equipos-en-grupo.md). Texto leído del repo
`dflores296/NOM-001-SEDE-2012` (`data/definiciones.json`, `data/corpus.json`, `data/tablas_revisadas.json`).

## Lo que se midió

- **El tipo (seis opciones) es del circuito y decide todo**: el F.D. del alimentador, la calculadora
  (210, 430 o 440), si se permite 240-4(b) (se niega a todo «Contactos»), el uso de vivienda (210-11(c)),
  que la calefacción sea continua y en qué renglón del resumen va la carga.
- **Dos capturas que no se hablan**: los totales del renglón (continua y no continua) —el circuito es
  un aparato sin aparato en el desplegable— y el desglose, cuyos aparatos no tienen tipo: heredan el del
  circuito.
- **Consecuencias**: un circuito con lámparas y contactos tiene que escoger un tipo, y todo entra al
  alimentador con un solo F.D.; los 180 VA de 220-14(i) salen de si la descripción empieza con
  «contacto»; un contacto sencillo en su propio circuito pierde 240-4(b) por ser «Contactos»; el
  refrigerador en su circuito es un «uso» y no un circuito individual.
- El escritorio y el Excel capturan por elementos con un tipo por circuito
  (`Domain/Proyectos/ElementoCircuito.cs`); la web agregó los totales. Ninguno separa el tipo de la carga
  de la clase del circuito.

## Lo que dice la norma

Art. 100:

| Término | Definición |
|---|---|
| Circuito derivado individual | «Circuito que alimenta a un solo equipo de utilización.» |
| Circuito derivado de uso general | «Circuito que alimenta a dos o más salidas para alumbrado y aparatos.» |
| Circuito derivado para aparatos | «…una o más salidas a las que se conectan aparatos; tales circuitos no deben contener elementos de alumbrado conectados permanentemente…» |
| Alimentador | Los conductores hasta «el dispositivo final de protección contra sobrecorriente del circuito derivado». |
| Salida | «Punto en un sistema de alambrado en donde se toma corriente para alimentar a un equipo de utilización.» |
| Aparato | Equipo de utilización de tamaños y tipos normalizados que se conecta como una unidad. |
| Factor de demanda | «Relación entre la demanda máxima… y la carga total conectada.» |

La carga de cada salida sale de 220-14(a) a (l) y el F.D. se aplica **por tipo de carga** en 220 Parte C
(220-42, 220-44, 220-50 a 220-56). «Mixto» no está en la norma: dice **cargas combinadas** (220-18(a),
440-34, 430-110(c); 440-4(b), «equipos con varios motores y carga combinada»).

## La propuesta

**Principio: todo lo que cuelga de un circuito es una carga, en una sola lista; cada carga tiene su
tipo, y la clase del circuito sale de sus cargas.**

### 1. Tres formas de capturar una carga

| Forma | Qué se sabe | Qué se captura | Base |
|---|---|---|---|
| Por salida o equipo | Cuántos y su placa | Tipo · subtipo · cantidad · valor unitario (VA, W, A, HP) | 220-14(a)–(l), 430-6, 440-6 |
| Carga total | Solo el total | Tipo · VA (W, A) · continua o no | Declarada por el proyectista; la memoria lo dice |
| Por superficie | m² y uso del local | Alumbrado general: m² × VA/m² de la Tabla 220-12 | 220-12 — en su propia fase, con M-14 |

**La captura del renglón no se quita** (David: sirve para aproximar y para cargas de las que solo se
sabe el total, como otro tablero). Escribir en el renglón crea o edita una carga, y esa carga aparece en
el desplegable como una línea más. Qué crea, según el tipo: en Alumbrado, Contactos y Calefacción, una
**carga total** (son grupos de salidas); en Aparatos, Motores y A/A, **un equipo** (circuito
individual, como hoy). Se cambia en el desplegable.

### 2. Otro tablero es un alimentador

Un interruptor de este tablero que alimenta un tablero de alumbrado y control es un **alimentador**
(Art. 100, Art. 215). Se captura la **carga calculada** del otro tablero —continua y no continua, ya con
sus F.D.— y se calcula con 215-2(a)(1) y 215-3. **Sin F.D. aquí**: 220-40 dice que la carga del
alimentador es la suma «después de aplicar cualquier factor de demanda»; ya se aplicó allá. No es
cascada: es un número, no un vínculo con otro archivo
([`alcance-v1-un-tablero.md`](alcance-v1-un-tablero.md)).

### 3. La clase del circuito sale de sus cargas

Lo que cambia reglas es si el circuito alimenta **un solo equipo** o **dos o más salidas**:

| Cargas | Clase | Reglas |
|---|---|---|
| Un equipo, cantidad 1 | Individual | 210-21(b)(1) (contacto ≥ circuito), 422-10(a), 422-11(e), 240-4(b) |
| Carga total, o dos o más salidas, con alumbrado o contactos de uso general | Uso general | 210-21(b)(2)(3), 210-23, 210-24, 240-4(b)(1) |
| Solo aparatos, o contactos de vivienda para aparatos pequeños y lavadora | Para aparatos | Lo mismo; vivienda: 210-11(c), 210-52(b), 220-52 |
| Otro tablero | Alimentador | 215, 220-40 |

### 4. El tipo es de cada carga

Cada carga lleva tipo y subtipo; el F.D. del alimentador se aplica por el tipo de cada carga y el
resumen por tipo suma cargas, no circuitos. La columna «Tipo» del cuadro muestra el tipo si todas las
cargas son de uno, **«Combinadas»** si no, y abajo la clase: «Individual», «Uso general · 10 salidas»,
«Carga total». Varios motores en un circuito son varias cargas de motor: **«Varios» se retira**.

### 5. Nombres cortos basados en la NOM

David: «te permito usar nombres basados en la NOM pero más cortos». El corto va en el selector y la
tabla; el de la NOM, en la ayuda, la guía, el documento y la memoria.

**Tipos de carga**

| Corto | Nombre NOM | Artículos |
|---|---|---|
| Alumbrado | Alumbrado | 220-12, 220-14(d), 220-18(b), 220-42 |
| Contactos | Salidas para contactos | 220-14(h)(i)(j)(k), 210-52, 220-44 |
| Aparatos | Aparatos y cargas específicas | 220-14(a), Art. 422, 220-53 a 220-56 |
| Motores | Motores | Art. 430, 220-50 |
| A/A y refrig. | Aire acondicionado y refrigeración | Art. 440 |
| Calefacción | Calefacción eléctrica fija de ambiente | Art. 424, 220-51 |
| Tablero | Alimentador a tablero | Art. 215, 220-40 |

**Clases de circuito y condiciones**

| Corto | Nombre NOM | Dónde |
|---|---|---|
| Individual | Circuito derivado individual | Art. 100 |
| Uso general | Circuito derivado de uso general | Art. 100 |
| Para aparatos | Circuito derivado para aparatos | Art. 100 |
| Multiconductor | Circuito derivado multiconductor | Art. 100, 210-4 |
| Alimentador | Alimentador | Art. 100, 215 |
| Combinadas | Cargas combinadas | 220-18(a), 440-34, 430-110(c) |

**Lo demás**

| Hoy | Corto | Nombre NOM | Dónde |
|---|---|---|---|
| Desglose · «Aparato del circuito» | Salidas y cargas | Salida; carga | Art. 100 |
| Aparato con motor | Con motor | Aparato operado por motor | 430-6(a)(1) Exc. 3, 422-10(a) |
| «Hab.» · A/C de cuarto | De habitación | Acondicionador de aire para habitación | 440 Parte G |
| «VFD» | Velocidad ajustable | Equipo de conversión de potencia | 430 Parte J |
| RLA · «Sel.» | Nominal · Selección | Corriente de carga nominal · corriente de selección del circuito derivado | 440-6(a) |
| MCA · MOCP | Ampacidad · Prot. máx. | Ampacidad de los conductores · valor nominal máximo de la protección | 440-4(b) |
| Uso: Cocina · Lavadora · Baño | Aparatos pequeños · Lavadora · Baño | Circuitos para aparatos pequeños, para lavadora, para cuartos de baño | 210-11(c) |
| Uso: Refrigerador | Refrigerador | Circuito derivado individual del refrigerador | 210-52(b)(1) Exc. 2 |
| «Neutro comp.» | Multiconductor | Circuito derivado multiconductor | 210-4 |
| Carga instalada | Carga conectada | Carga total conectada | Art. 100 («Factor de demanda») |
| Carga demandada | Demanda | Demanda máxima | Art. 100 («Factor de demanda») |

Lo que la norma no define (la canalización T1, el interruptor principal) se queda con su nombre común.

### 6. Guía de cargas

Una página en la barra: árbol que se despliega por ramas —tipo → subtipo → cómo se captura → artículos,
secciones y tablas del circuito y del alimentador—, otro para las clases de circuito y el glosario.
Imprimible, y con un «?» desde el tipo que lleva a su rama. Sale de **un solo árbol de datos en el
modelo**, el mismo que llenará el selector de tipo de cada carga: la guía y el cálculo no se pueden
contradecir.

### 7. Fases

Cada commit, con su fase y su verificación:
[`../conocimiento/trazabilidad-cargas-y-circuitos.md`](../conocimiento/trazabilidad-cargas-y-circuitos.md).

| Fase | Qué | Hallazgos |
|---|---|---|
| A | Guía de cargas y glosario (sin tocar el cálculo) | I-126 |
| B | Modelo: carga con tipo, subtipo y forma; el renglón como carga; clase del circuito; alimentador a tablero; F.D. por carga; archivo formato 5 | I-123, I-125 |
| C | Cuadro, desplegable, documento y memoria con los nombres cortos; se retira «Varios» | I-127 |
| D | Reglas por clase: 210-21(b)(1), 210-23(a), 422-11(e), 240-4(b)(1) | I-124 |
| E | Mínimo de alumbrado general por superficie (220-12) y contactos de vivienda dentro de él (220-14(j)) | M-14 |

## Decisiones de David (2026-09-29)

1. **F.D. por el tipo de cada carga.**
2. **Tablero alimentado sin F.D.** — «solo apliquemos FD a las cargas que la NOM ya establece; si no,
   estaríamos duplicando factores».
3. **La captura por renglón se queda**: para aproximar y para lo que solo se sabe en total.
4. **Se retira «Varios».**
5. **Guía**: árbol desplegable, imprimible, con «?» desde el tipo.
6. **Nombres basados en la NOM, más cortos.**
7. **Implementar toda la propuesta**, con una tabla de commits en el repo para seguir el cambio.

La clase del circuito sin preguntarla (sección 3) y 220-12 en su propia fase (E) son recomendación de
Claude, dentro de «implementa toda la propuesta».

8. **Plan aprobado** (2026-09-30), después de ver el mockup de la captura: «Apruebo el plan,
   implementalo». Con un ajuste de Claude a la tabla de la sección 3: los contactos de uso general dan
   un circuito de uso general, como se lee en la práctica; «para aparatos» queda para los de solo
   aparatos y los de aparatos pequeños y lavadora de vivienda. Un circuito de solo motores (430-53)
   no tiene nombre de clase en el Art. 100: la pantalla dice «Grupo».

## Lo que queda fuera

- Calcular las Tablas 220-42, 220-44, 220-54, 220-55 y 220-56: el F.D. sigue a criterio del proyectista,
  con su justificación (R-12). La guía las cita.
- La cascada: el tablero alimentado es un número, no otro archivo.
- Llevar esto al escritorio: se anota en [`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md)
  cuando la fase B toque el modelo.
