# Los avisos de la tabla de captura, reescritos con la guía — 2026-10-07

Cada aviso y cada ayuda de la tabla de captura (el cuadro y su desplegable), con su texto final según la
guía de David: [`../decisiones/redaccion-de-avisos.md`](../decisiones/redaccion-de-avisos.md). Sacado del
código el 2026-10-07 (`Pages/Captura.razor`, `CuadroDeCarga.cs`, `CircuitoDelCuadro.cs`, y los mensajes del
motor de cálculo que llegan a un circuito). Nada está implementado.

**APROBADA · David · 2026-10-07 — por verificar.** David: «lo apruebo todo, pero falta revisar los casos; ya
lo haremos con un agente automático más adelante que haga de usuario y directo abra en el navegador. Lo
apruebo, déjalo por verificar.» Cada caso se verifica provocándolo en el navegador, después de implementar.

El resto de la página de captura (ficha, canalizaciones, resumen, alimentador, tarjeta «Avisos», avisos
flotantes que no son de la tabla) va en una segunda entrega: alcance B de la guía.

**Cómo leer las tablas.**
- **Clase:** Error (no calcula), Conflicto (error por datos que chocan), Advertencia (calcula, pero hay que
  revisar), Información (ayuda al pasar el cursor).
- **Mensaje:** lo que se lee en la tabla o en el ⚠.
- **Cursor:** la ayuda que sale al pasar el cursor; la referencia va al final, tras «·».
- `{…}` es un valor que pone el cálculo.
- Si una celda dice **Sale**, ese texto se quita de la tabla. Lo dice otra cosa: una columna nueva, el
  valor del campo o la memoria de cálculo.

---

## A · Encabezados del cuadro

| # | Columna | Clase | Cursor (hoy, resumido → propuesta) |
|---|---|---|---|
| A1 | Tipo | Información | Las clases y el «?» → **Clase del circuito, según sus cargas · Art. 100** |
| A2 | Carga | Información | Cómo se suma, 125 %, motores → **Suma de las cargas del desplegable, en VA.** |
| A3 | Continua | Información | → **Cargas que operan 3 h o más; se consideran al 125 % · 210-19(a)(1)** |
| A4 | No continua | Información | → **Cargas que operan menos de 3 h; se consideran al 100 %.** |
| A5 | F.P. | Información | Fórmula → **Factor de potencia del conjunto de cargas del circuito.** |
| A6 | L (m) | Información | → **Longitud usada para calcular la caída de tensión · Tabla 9** |
| A7 | Canal. | Información | Cómo se agrupan y el tramo más desfavorable → **Canalización del circuito; los de la misma canalización se agrupan · 310-15(b)(3)(a)** |
| A8 | P | Información | → **Polos del interruptor.** |
| A9 | In (A) | Información | → **Corriente usada para seleccionar el conductor y la protección · 210-19(a)(1)** |
| A10 | Protec. (A) | Información | Tamaño normalizado, rango de motores, cómo se fija → **Protección del circuito: el tamaño normalizado que cubre la corriente · 240-6(a)** |
| A11 | Fase | Información | AWG, mm², paralelo y colores → **Conductor de fase: calibre y sección en mm² · Tabla 8** |
| A12 | Neutro | Información | → **Conductor de neutro; blanco o gris · 200-6** |
| A13 | Tierra | Información | → **Conductor de puesta a tierra de equipos · Tabla 250-122** |
| A14 | e (%) | Información | → **Caída de tensión del circuito; se recomienda hasta 3 % · 210-19(a)(1) nota 4** |
| A15 | A, B, C | Información | → **Carga del circuito en esta fase, en VA.** |

## B · El renglón del circuito

| # | Qué | Clase | Mensaje (a la vista) | Cursor |
|---|---|---|---|---|
| B1 | Alimentador a otro tablero, bajo la clase | Información | «sin F.D. (220-40)» → **sin F.D.** | **Carga del otro tablero, ya con sus factores de demanda · 220-40** |
| B2 | «ICFT» bajo la clase | Información | **ICFT** (queda) | **El circuito lleva protección por falla a tierra · 210-8** |
| B3 | «OL» bajo la clase | Información | **OL** (queda) | **El equipo pide protección contra sobrecarga aparte del interruptor · {430-32(a), 430-124, 440-52…}** |
| B4 | «+N» | Información | **+N** (queda) | **La carga va entre fase y neutro: el circuito lleva neutro · 310-15(b)(5)** |
| B5 | Selector de Protec. (motor, A/C, variador) | Información | (el valor) | **Se permite de {mín.} a {máx.} A · {430-52(c)(1)}**; si está fijada, segunda oración: **Fijada en {x} A; el cálculo da {y} A.** |
| B6 | Valor de Protec. y del conductor | Información | (el valor) | El desglose completo **sale** a la memoria. Cursor: **Clic para ver el cálculo en la memoria.** (requiere el puente: la memoria con un punto de entrada por circuito) |

### Advertencias del ⚠

Al pasar el cursor por el ⚠ sale el mensaje. La referencia va en una segunda línea apartada: «· {ref}» (guía, punto 2).

| # | Cuándo | Mensaje | Ref. |
|---|---|---|---|
| B7 | Contactos de uso general en un circuito de más de 20 A | **Contactos de 15 o 20 A en un circuito de {P} A. Divide los contactos en circuitos de 20 A.** | Tabla 210-21(b)(3) |
| B8 | Equipo fijo de más del 50 % con alumbrado o contactos | **El equipo fijo usa más del 50 % del circuito. Pasa el equipo a otro circuito.** | 210-23(a)(2) |
| B9 | Circuito de 30 A con alumbrado común | **Alumbrado común en un circuito de 30 A. Divide el alumbrado en circuitos de 20 A.** | 210-23(b) |
| B10 | Circuito de 40 o 50 A con alumbrado común | **Alumbrado común en un circuito de {P} A. Divide el alumbrado en circuitos de 20 A.** | 210-23(c) |
| B11 | Circuito de más de 50 A con alumbrado | **Alumbrado en un circuito de {P} A. Pasa el alumbrado a otro circuito.** | 210-23(d) |
| B12 | Contactos de vivienda a más de 120 V | **Contactos de vivienda a {V} V. Pasa los contactos a un circuito de 127 V.** | 210-6(a)(2) |
| B13 | Caída combinada de más de 5 % | **Caída combinada de {x} %. Baja la caída permitida en Condiciones de cálculo.** | 215-2(a)(4) nota 2 |
| B14 | A/A de habitación solo, más del 80 % | **El acondicionador usa más del 80 % del circuito. Revísalo.** | 440-62(b) |
| B15 | A/A de habitación con otras cargas, más del 50 % | **El acondicionador usa más del 50 % del circuito. Pasa las demás cargas a otro circuito.** | 440-62(c) |
| B16 | Con bypass, ninguna protección cabe | **Con bypass, ninguna protección cabe entre {mín.} y {máx.} A. Revísalo con el fabricante.** | 430-122(b) |
| B17 | Variador con bypass dentro de un grupo | **Sale** (D8: en un grupo, «Bypass» se ve apagada y no se puede marcar) | — |
| B18 | Ningún tubo alcanza | **Ningún tubo admite todos los conductores. Reparte los circuitos en más canalizaciones.** | Cap. 10, Tabla 1 |

## C · Errores del renglón (fila roja)

El dato que falta se marca en rojo en su columna. La fila roja dice:

| # | Cuándo | Clase | Mensaje |
|---|---|---|---|
| C1 | Sin tensión de placa | Error | **Falta la tensión de placa.** (variador: **Falta la tensión de entrada.**) |
| C2 | La tensión de placa no es de este tablero | Conflicto | **La tensión de placa no es de este tablero. Escoge otra tensión.** |
| C3 | La tensión de placa no va con los polos | Conflicto | **La tensión de placa pide {n} polos. Escoge otra tensión o cambia los polos.** |
| C4 | Motor de servicio no continuo sin corriente de placa | Error | **Falta la corriente de placa.** |
| C5 | Corta duración con tiempo «continuo» | Error | **Faltan los minutos del servicio.** |
| C6 | Grupo sin motores | Error | **El grupo no tiene motores.** |
| C7 | Variador con bypass sin HP | Error | **Faltan los HP del motor.** |
| C8 | HP que la tabla no trae a esa tensión | Conflicto | **La tabla no trae un motor de {hp} HP a {V} V. Escoge otros HP.** |
| C9 | Motor en amperes que la tabla no alcanza | Conflicto | **La tabla no trae un motor de {A} A a {V} V. Revisa la corriente de placa.** |
| C10 | Desglose con motores y líneas sin carga | Conflicto | **El circuito lleva motores y líneas sin carga. Quita las líneas sin carga.** |
| C11 | Alimentador a tableros con otras cargas | Conflicto | **Un alimentador a tableros solo lleva tableros. Pasa las demás cargas a otro circuito.** |
| C12 | Contacto del refrigerador con otras líneas | Conflicto | **El contacto del refrigerador va solo. Pasa las demás cargas a otro circuito.** |
| C13 | Contacto del refrigerador con Cant. mayor que 1 | Conflicto | **El contacto del refrigerador es uno. Deja la cantidad en 1.** |
| C14 | Contactos de aparatos pequeños, lavadora o baño con otras cargas | Conflicto | **Los contactos de {uso} van solos. Pasa las demás cargas a otro circuito.** |

### Lo que el motor de cálculo no puede calcular (guía, punto 4)

La pantalla traduce estos casos; el motor no se toca. El mensaje original va a la memoria.

| # | El motor dice hoy (resumido) | Clase | Mensaje |
|---|---|---|---|
| C15 | «440-6(a): falta la corriente de carga nominal…» | Error | **Falta la corriente de placa.** |
| C16 | «440-4(b): falta la ampacidad mínima…» | Error | **Falta la ampacidad mínima (MCA).** |
| C17 | «440-4(b): falta la protección máxima…» | Error | **Falta la protección máxima.** |
| C18 | «440-62(a)(3): falta la corriente total…» | Error | **Falta la corriente de placa.** |
| C19 | «430-122(a): falta la corriente nominal de entrada…» | Error | **Falta la corriente de entrada.** |
| C20 | «110-3(b): falta la protección máxima que marca el fabricante…» | Error | **Falta la protección máxima.** |
| C21 | «440-60: un acondicionador de habitación trifásico o de más de 250 V no es de la Parte G…» | Conflicto | **Un acondicionador trifásico o de más de 250 V no es de habitación. Escoge otro subtipo de A/A.** |
| C22 | «440-62(a)(2): {x} A pasa de los 40 A de un acondicionador de habitación…» | Conflicto | **Un acondicionador de más de 40 A no es de habitación. Escoge otro subtipo de A/A.** |
| C23 | «440-4(b) / 110-3(b): la protección máxima ({x} A) es menor que el tamaño estándar más chico» | Conflicto | **La protección máxima de {x} A es menor que cualquier interruptor. Revisa la placa.** |
| C24 | «430-53(c)(2): la protección máxima de un variador no lleva la corriente del grupo…» | Conflicto | **La protección del variador no alcanza la corriente del grupo. Pasa el variador a su propio circuito.** |
| C25 | «{regla}: el límite de la protección no deja un interruptor que lleve la corriente del grupo…» | Conflicto | **Ninguna protección cabe para la corriente del grupo. Divide los motores en más circuitos.** |
| C26 | «Las Tablas 430-247/248/249/250 no traen una fila para…» | Conflicto | Igual que C8. |
| C27 | «Ni subiendo hasta {n} conductores en paralelo alcanza la ampacidad…» | Conflicto | **Ningún conductor alcanza la corriente del circuito. Divide la carga en más circuitos.** |
| C28 | «Ni con el calibre más grande… baja la caída de tensión a {x} %…» | Conflicto | **Ningún conductor baja la caída a {x} %. Acorta el circuito o sube la caída permitida en Condiciones de cálculo.** |
| C29 | «'{aislamiento}' no se reconoce, o no es válido para el lugar…» | Conflicto | **El aislamiento {x} no vale en lugar {lugar}. Cambia el aislamiento en Condiciones de cálculo.** |
| C30 | «La Tabla 310-15(b)(2)(a) no cubre {t} °C…» | Conflicto | **La temperatura ambiente de {t} °C está fuera de tabla. Cambia la temperatura en Condiciones de cálculo.** |
| C31 | Cualquier otro | Error | **No se puede calcular el circuito. Revisa sus datos.** |

## D · Encabezados del desplegable

| # | Columna | Cursor |
|---|---|---|
| D1 | Tipo | **Familia de la carga; define su factor de demanda en el alimentador · 220 Parte C** |
| D2 | Subtipo | **Qué es la carga dentro de su tipo; define su carga mínima · 220-14** |
| D3 | Cant. | **Cuántas salidas iguales lleva el circuito.** |
| D4 | Unidad | **Unidad de la carga: VA, W o A; en motores, HP o A.** |
| D5 | Carga c/u | **Carga de cada salida.** |
| D6 | Servicio | **Tiempo de trabajo de la carga: continua (3 h o más) o no continua · 210-19(a)(1)** |
| D7 | F.P. | **Factor de potencia de la carga.** |
| D8 | Total (VA) | **Carga de todas las salidas del renglón, en VA.** |
| D9 | Continua / No continua (tableros) | **Carga continua (o no continua) del otro tablero, ya con sus factores de demanda · 220-40** |

Las columnas nuevas de [`../decisiones/acomodo-del-desplegable.md`](../decisiones/acomodo-del-desplegable.md) (tensión de placa, corrientes,
bypass, HP del motor, casillas del motor solo, 225 %) llevan su ayuda en E.

## E · Las celdas del desplegable, por tipo de carga

### Alumbrado, contactos, aparatos y calefacción

| # | Qué | Hoy | Propuesta |
|---|---|---|---|
| E1 | Mínimo bajo «Carga c/u» | Visible: «mín. 180 VA — 220-14(i)» | **Sale** (con D14 el campo ya trae el mínimo). Cursor del campo: **Carga mínima por salida: {180} VA · {220-14(i)}** |
| E2 | Mínimo de anuncios | Visible: «mín. 1,200 VA por circuito — 220-14(f)» | Según D18. Cursor: **Carga mínima por circuito de anuncios: 1,200 VA · 220-14(f)** |
| E3 | Servicio de la carga | Cursor largo | **Continua: opera 3 h o más · 210-19(a)(1)**. Apagada: **Siempre continua · {422-13 / 424-3(b) / 600-5(b)}** |
| E4 | Total con el mínimo | Cursor «Con el mínimo de 220-14(i)» | **Incluye la carga mínima · 220-14(i)** |
| E5 | Aparato con motor, el mayor | Visible «motor mayor · 125 %» | Queda visible. Cursor: **El motor mayor del circuito se considera al 125 % · 220-18(a)** |
| E6 | Aparato con motor, los demás | Cursor sobre «—» | **Se considera al 100 % · 220-18(a)** |

### Motor de uso general

| # | Qué | Hoy | Propuesta |
|---|---|---|---|
| E7 | Unidad HP / A | Cursor | **HP: corriente de tabla. A: corriente de placa · 430-6(a)** |
| E8 | Selector de HP | Cursor largo | **Potencia del motor, con su corriente de tabla · 430-6(a)** |
| E9 | Nota bajo los HP | Visible «Monofásico 127 V · Tabla 430-248 · FLC 9.80 A» | **127 V · FLC 9.80 A** (sin la tabla). Cursor: **Corriente de plena carga de tabla · Tabla 430-248** |
| E10 | Motor en A: nota | Visible «1.5 HP · Monofásico 127 V» | **1.5 HP · 127 V**. Cursor: **HP que le corresponden por su corriente · 430-6(a)(1)** |
| E11 | Corriente de placa (servicio no continuo) | Cursor | **Corriente usada para motores de servicio no continuo · 430-22(e)** (el ejemplo de la guía) |
| E12 | Servicio del motor | Cursor largo | **Tiempo de trabajo del motor · Tabla 430-22(e)** |
| E13 | Casilla «no arranca con la tabla» (columna nueva) | Visible + cursor largo | Encabezado: **No arranca**. Cursor: **El motor no arranca con la protección de tabla · 430-52(c)(1)** |
| E14 | Casilla «terminal 75 °C» (columna nueva) | Visible + cursor largo | Encabezado: **Terminal 75 °C**. Cursor: **Motor de diseño B a E con arrancador marcado 75 °C · 110-14(c)** |
| E15 | Columnas del motor solo, apagadas en grupo | — | Cursor: **Solo con un motor solo.** (D2) |
| E16 | En grupo: «Continuo» en Servicio | Visible + cursor | Queda visible. Cursor: **En un grupo, cada motor se considera continuo.** |

### Variador

| # | Qué | Hoy | Propuesta |
|---|---|---|---|
| E17 | Tensión de entrada | Cursor | **Tensión de entrada de la placa del variador.** |
| E18 | Corriente de entrada | Cursor | **Corriente de entrada de la placa del variador · 430-122(a)** |
| E19 | Protección máxima | Cursor | **Protección máxima permitida por el fabricante · 110-3(b)** (el ejemplo de la guía) |
| E20 | Nota «Entrada · Prot. máx.» | Visible | **Sale** (cada dato en su columna) |
| E21 | «—» en Servicio | Cursor | **No aplica al variador.** |
| E22 | Bypass (columna nueva) | Visible + cursor largo | Cursor: **El variador tiene bypass a la línea · 430-122(b)** |
| E23 | HP del motor (columna nueva) | Visible + cursor | **Potencia del motor que mueve el variador · 430-6(a)**. Apagada sin bypass: **Solo con bypass.** |
| E24 | En grupo: «variador» en Servicio | Visible + cursor | Queda visible. Cursor: **Cuenta con su corriente de entrada · 430-122(a)** |

### A/A y refrigeración

| # | Qué | Hoy | Propuesta |
|---|---|---|---|
| E25 | Tensión de placa | Cursor | **Tensión de la placa del equipo.** |
| E26 | Nota «Ampacidad · Prot. máx. — 440-4(b)» | Visible | **Sale** (columnas) |
| E27 | Ampacidad mínima (MCA) | Cursor | **Ampacidad mínima de los conductores, de la placa · 440-4(b)** |
| E28 | Protección máxima (MOCP) | Cursor | **Protección máxima de la placa · 440-4(b)** |
| E29 | Nota «Nominal · Selección — 440-6(a)» | Visible | **Sale** (columnas) |
| E30 | Corriente nominal del motocompresor | Cursor | **Corriente de carga nominal de la placa · 440-6(a)** |
| E31 | Corriente de selección | Cursor | **Corriente de selección de la placa, si la trae · 440-6(a)** |
| E32 | 225 % (columna nueva) | Visible + cursor | Encabezado: **Arranque 225 %**. Cursor: **No arranca con la protección al 175 % · 440-22(a)** |
| E33 | Motocompresor en grupo, «—» en Servicio | Cursor | **El mayor se considera al 125 % · 440-33** |
| E34 | Nota «Corriente total — 440 Parte G» | Visible | **Sale** |
| E35 | Corriente total (de habitación) | Cursor | **Corriente total de la placa del acondicionador · 440-62** |
| E36 | «440-62» en Servicio | **Visible** | **Sale**: «—». Cursor: **No aplica al acondicionador de habitación.** |

### Otro tablero

| # | Qué | Propuesta (cursor) |
|---|---|---|
| E37 | Continua / No continua | Como D9 |
| E38 | «Agregar tablero» | **Agrega otro tablero al mismo alimentador.** |

### Del desplegable en general

| # | Qué | Propuesta |
|---|---|---|
| E39 | «Agregar carga» | **Agrega una salida o una carga.** (con D1 ya no hay renglón azul ni «pasa a grupo») |
| E40 | «No simultáneo con» | Según D11. Cursor: **Circuito que no funciona a la vez que este; al alimentador va el mayor · 220-60** |
| E41 | «No entra al alimentador: es el menor del par.» | Queda (ya cumple) |

## F · Avisos flotantes de la tabla

| # | Cuándo | Clase | Mensaje |
|---|---|---|---|
| F1 | Una protección fijada regresa al cálculo | Advertencia | **La protección fijada del circuito {n} ya no está en su rango. Regresa a {x} A.** |
| F2 | Un subtipo que va solo, con otras líneas | Conflicto | **{Subtipo} va solo. Quita primero las demás líneas del circuito {n}.** |
| F3 | Polos que no se pueden cambiar | Conflicto | **El circuito {n} no cabe con {p} polos. {motivo}** (el motivo, con la misma guía) |
