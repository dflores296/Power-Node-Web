# Los artículos y avisos de la tabla de captura — inventario del 2026-10-07

Todo lo que la tabla de captura (el cuadro y su desplegable) dice con un artículo de la NOM, sacado del
código (`Pages/Captura.razor`, `CuadroDeCarga.cs`, `CircuitoDelCuadro.cs`) el 2026-10-07. Es para que David
decida cada uno — [`../decisiones/acomodo-del-desplegable.md`](../decisiones/acomodo-del-desplegable.md), R3.

Fuera de este inventario (no son la tabla): la tarjeta «Avisos» al pie de la página, la tarjeta del
alimentador, la de canalizaciones y la ficha del tablero.

## Cómo se ve cada cosa hoy

- **Visible**: está escrito en la tabla, siempre.
- **Cursor**: aparece al pasar el cursor (ayuda).
- **Ícono ⚠**: el triángulo junto a la descripción del circuito; el texto, al pasar el cursor.
- **Fila roja**: un renglón rojo bajo el circuito; el circuito no se calculó.
- **Flotante**: un aviso arriba de la página, que se va solo.

## La propuesta (columna «Propuesta»)

Tres niveles, de lo discreto a lo completo, y un puente entre la tabla y la memoria:

| Nivel | Dónde | Qué lleva |
|---|---|---|
| 1 · A la vista | La tabla | Valores, estados y lo que hay que corregir. Sin artículos. |
| 2 · Al pasar el cursor | La ayuda del campo | Una frase: qué se captura o qué significa el valor, y la referencia al final («· 430-122(a)»). Nada de explicaciones largas. |
| 3 · La memoria de cálculo | `/documento` | El porqué completo, la cuenta y todos los artículos. |
| Puente | Clic en el valor de Protec., en el conductor o en el ⚠ | Abre la memoria en la hoja de ese circuito. |

Las claves de la columna «Propuesta»:

- **Queda**: sin cambio.
- **Corta**: se queda donde está, en una frase; la referencia al final, sin explicación.
- **Sin artículo**: se queda a la vista, pero sin el artículo.
- **Sale**: se quita de la tabla. Lo dice otra cosa (una columna nueva, el valor del campo) o la memoria.
- **Memoria**: sale de la tabla y queda en la memoria, con el puente.
- **Rojo**: error que impide calcular: el campo que falta, en rojo; en la fila roja, qué hacer, sin
  artículo.

---

## A · Encabezados del cuadro (cursor)

| # | Columna | Hoy dice (resumido) | Artículos | Propuesta |
|---|---|---|---|---|
| A1 | Tipo | La clase sale de sus cargas; lista de clases | Art. 100, 430-53, Art. 215 | Corta: «La clase del circuito: sale de sus cargas.» |
| A2 | Carga | Suma del desplegable; continua al 125 %, no continua al 100 %; motor y A/C con su corriente | 210-19(a)(1), 210-20(a), 430-24, 440-33 | Corta |
| A3 | Continua | Entran al 125 % | 210-19(a)(1), 210-20(a) | Corta |
| A4 | L (m) | Determina la caída | Tabla 9 | Corta |
| A5 | Canal. | Cómo se agrupan los circuitos y se cuentan portadores | 310-15(b)(3)(a), 310-15(a)(2) | Corta: «Escoger la canalización; los de la misma se agrupan.» |
| A6 | In (A) | Corriente de diseño | 210-19(a)(1) | Corta |
| A7 | Protec. (A) | Primer tamaño normalizado; en motores, solo el techo; cómo se fija | 240-6(a), 210-20(a), 430-52(c)(1), 440-22(a), 440-4(b), 110-3(b) | Corta: «La protección. En motores y A/C, escoger dentro del rango.» |
| A8 | Fase | AWG y mm²; paralelo; colores | Tabla 8, 210-5(c)(2) | Corta (los colores, a la memoria) |
| A9 | Neutro | Blanco o gris | 200-6 | Corta |
| A10 | Tierra | Tamaño y color | Tabla 250-122, 250-122(b), 250-119 | Corta |
| A11 | e (%) | Límite 3 % y 5 % | 210-19(a)(1) nota 4 | Corta |

## B · El renglón del circuito

| # | Qué | Cómo se ve | Hoy dice | Artículos | Propuesta |
|---|---|---|---|---|---|
| B1 | Detalle bajo la clase, alimentador a tableros | Visible | «sin F.D. (220-40)» | 220-40 | Sin artículo: «sin F.D.» |
| B2 | «ICFT» bajo la clase | Visible | «ICFT» | (210-8 en el desglose) | Queda |
| B3 | «OL» bajo la clase | Visible + cursor | «Protección contra sobrecarga — 430-32(a): …» | 430-32(a), 430-32(b), 430-33, 430-124(a), (b), 430-126, 430-53, 440-52 (según el equipo) | Queda (OL a la vista; el cursor, corto) |
| B4 | «+N» de los polos | Cursor | Cuándo lleva neutro; neutro como portador | 310-15(b)(5)(2) | Corta |
| B5 | Selector de Protec. (motor, A/C, variador) | Cursor | Fijada o calculada; el rango; **y el desglose completo** | 430-52, 440-22(a), 110-3(b) y los del desglose | Corta: «Se permite de 15 a 25 A» y si está fijada. El desglose: Memoria |
| B6 | Valor de Protec. y del conductor (todos los circuitos) | Cursor | El desglose completo del cálculo, con sus citas | Varios por circuito | Memoria, con el puente |
| B7 | ⚠ Reglas de la clase que no se cumplen | Ícono ⚠ | Por ejemplo: «Los contactos de uso general (15 o 20 A) no van en un circuito de 30 A… — Tabla 210-21(b)(3)» | Tabla 210-21(b)(3), 210-23(a)(2), 210-23(b), 210-23(c), 210-23(d), 210-6(a)(2) | Queda el ⚠. El texto, corto y sin artículo: «Contactos de 15 o 20 A en un circuito de 30 A: pártelos.» El artículo: Memoria |
| B8 | ⚠ Caída combinada > 5 % | Ícono ⚠ | La cuenta completa | 215-2(a)(4) nota 2, 210-19(a)(1) nota 4 | Ídem: «Caída combinada de 6.2 %: pasa del 5 %.» |
| B9 | ⚠ A/A de habitación pasa del 80 % o 50 % | Ícono ⚠ | Corriente, porcentaje y circuito | 440-62(b), (c) | Ídem |
| B10 | ⚠ Bypass: ningún tamaño en el rango | Ícono ⚠ | Las dos corrientes y «revisar con el fabricante» | 430-122(b), 110-3(b) | Ídem |
| B11 | ⚠ Variador con bypass en un grupo | Ícono ⚠ | El bypass no entra al grupo | 430-53, 430-122(b) | Ídem |
| B12 | ⚠ Ningún tubo alcanza | Ícono ⚠ | Reparte los circuitos | Cap. 10 Tabla 1, Tabla 4 | Ídem |

## C · Errores del renglón (fila roja)

| # | Cuándo | Hoy dice (resumido) | Artículos | Propuesta |
|---|---|---|---|---|
| C1 | Falta la tensión de placa, o no es de este tablero, o no va con los polos | «Falta la tensión de placa del equipo: escógela en el desplegable…» | — | Rojo: la columna «Tensión de placa» en rojo; fila: «Falta la tensión de placa.» |
| C2 | Motor de servicio no continuo sin corriente de placa | «430-22(e): el conductor… va sobre la corriente de placa…» | 430-22(e) | Rojo: «Falta la corriente de placa.» |
| C3 | Servicio de corta duración con tiempo «continuo» | «Tabla 430-22(e): un motor de servicio de corta duración no se especifica…» | Tabla 430-22(e) | Rojo: «Escoge los minutos del servicio.» |
| C4 | Grupo sin motores | «430-53: el grupo no tiene motores…» | 430-53 | Rojo: «El grupo no tiene motores.» |
| C5 | Variador con bypass sin HP, o HP fuera de tabla | «430-122(b): con bypass el conductor lleva también el 125 %…» | 430-122(b) | Rojo: la columna «HP del motor» en rojo; fila: «Con bypass, faltan los HP del motor.» |
| C6 | Desglose con solo motores y líneas sin carga | «220-18(a): el circuito solo alimenta motores… (430-53)» | 220-18(a), 430-53 | Rojo, sin artículo |
| C7 | Alimentador a tableros con otras cargas | «…solo lleva tableros — Art. 100, 215» | Art. 100, 215 | Rojo, sin artículo |
| C8 | Contacto del refrigerador con más líneas o Cant. > 1 | «…va solo en su circuito y es uno — 210-52(b)(1) Excepción 2» | 210-52(b)(1) Exc. 2 | Rojo, sin artículo |
| C9 | Aparatos pequeños, lavadora o baño con otras cargas | «…el circuito solo alimenta esas salidas — 210-52(b)(2) / 210-11(c)(2) / (3)» | 210-52(b)(2), 210-11(c)(2), (3) | Rojo, sin artículo |
| C10 | Un motor que la tabla no trae (HP o amperes) | Mensaje de la tabla de motores | Tabla 430-248 / 430-250 | Rojo, sin artículo |
| C11 | Lo que no puede calcular el motor de cálculo (sin calibre, sin protección…) | El mensaje del motor tal cual | Varios | **Por ver**: el motor se copia y no se reescribe; se puede acortar en la pantalla sin tocarlo |

## D · Encabezados del desplegable (cursor)

| # | Columna | Artículos | Propuesta |
|---|---|---|---|
| D1 | Tipo | 220 Parte C | Corta |
| D2 | Subtipo | 220-14 | Corta |
| D3 | Unidad | 430-6(a), 430-6(a)(1), 440-6(a), 440-62 | Corta (con las columnas nuevas, casi todo esto sale) |
| D4 | No continua (tablero) | 220-40 | Corta |
| D5 | Servicio | 210-19(a)(1), 424-3(b), 422-13, 430-22(e), Tabla 430-22(e) | Corta |

## E · Celdas del desplegable, por tipo de carga

### Alumbrado, contactos, aparatos, calefacción

| # | Qué | Cómo se ve | Hoy dice | Propuesta |
|---|---|---|---|---|
| E1 | Mínimo por salida bajo «Carga c/u» | Visible | «mín. 180 VA — 220-14(i)» (también 90 VA, 600 VA, 180 VA (h), 5000 VA 220-54) | Sale (con D14 el campo ya trae el mínimo). En el cursor del campo: «No menos de 180 VA · 220-14(i)» |
| E2 | Mínimo de anuncios | Visible + cursor | «mín. 1,200 VA por circuito — 220-14(f)»; 600-5(a) | Según D18 |
| E3 | Servicio de la carga | Cursor | «Continua: opera 3 h o más y entra al 125 % — 210-19(a)(1), 215-2(a)(1)»; apagada: «Siempre continua — 422-13 / 424-3(b) / 600-5(b)» | Corta |
| E4 | Total (VA) con mínimo | Cursor | «Con el mínimo de 220-14(i)» | Queda |
| E5 | Aparato con motor (el mayor) en «Servicio» | Visible + cursor | «motor mayor · 125 %» — 220-18(a) | Queda visible; cursor corto |
| E6 | Aparato con motor (los demás) | Cursor sobre «—» | «al 100 %; solo el mayor va al 125 % — 220-18(a)» | Corta |

### Motor de uso general

| # | Qué | Cómo se ve | Hoy dice | Propuesta |
|---|---|---|---|---|
| E7 | Unidad HP / A | Cursor | Interpolación — 430-6(a)(1) | Corta |
| E8 | Selector de HP | Cursor | La corriente sale de la tabla — 430-6(a); 2 polos a 220 V es monofásico — Tabla 430-248 | Corta |
| E9 | Nota bajo los HP | Visible | «Monofásico 127 V · Tabla 430-248 · FLC 9.80 A» | Sin artículo: «127 V · FLC 9.80 A». La tabla, en el cursor |
| E10 | Motor en A: nota bajo la corriente | Visible + cursor | «1.5 HP · Monofásico 127 V»; interpolación — 430-6(a)(1) | Sin artículo; cursor corto |
| E11 | «Placa A» (servicio no continuo) | Cursor | 430-22(e) | Corta (pasa a su columna, D5) |
| E12 | Servicio del motor | Cursor | 430-33, Tabla 430-22(e), 430-22(e) | Corta |
| E13 | «No arranca con la Tabla 430-52: hasta 400 %» | Visible (pie) + cursor largo | 430-52(c)(1) Excepción 2(3) | Pasa a columna (D9). Encabezado sin artículo; cursor corto |
| E14 | «Motor diseño B a E y arrancador marcado 75 °C» | Visible (pie) + cursor largo | 110-14(c)(1)a.(3), a.(4), 110-14(c) | Ídem |
| E15 | En grupo: «Continuo» en Servicio | Visible + cursor | 430-24 Excepción 1 no se captura en grupo | Queda visible; cursor corto |

### Variador

| # | Qué | Cómo se ve | Hoy dice | Propuesta |
|---|---|---|---|---|
| E16 | Corriente de entrada | Cursor | 430-122(a) (solo); 430-122(a) en grupo | Corta |
| E17 | Protección máxima | Cursor | 110-3(b); en grupo 430-53(c)(2), 110-3(b) | Corta |
| E18 | Nota «Entrada · Prot. máx.» | Visible + cursor | 430-122(a), 110-3(b) | Sale (cada una en su columna, D5) |
| E19 | «—» en Servicio | Cursor | 430-22(e), 430-122, Art. 440 | Corta: «No aplica al variador.» |
| E20 | «Con bypass» | Visible (pie) + cursor largo | 430-122(b), 430-120, Parte D, 430-124(b), 430-128, 430-110(a) | Pasa a columna (D8). Cursor: «El motor también trabaja directo de la línea; pide los HP del motor.» El resto: Memoria |
| E21 | «HP del motor» | Visible (pie) + cursor | 430-6(a) | Pasa a columna (D8); cursor corto |
| E22 | En grupo: «variador» en Servicio | Visible + cursor | 430-122(a), 430-53 | Queda visible; cursor corto |

### A/A y refrigeración

| # | Qué | Cómo se ve | Hoy dice | Propuesta |
|---|---|---|---|---|
| E23 | Carga combinada: nota | Visible | «Ampacidad · Prot. máx. — 440-4(b)» | Sale (columnas, D5) |
| E24 | MCA | Cursor | 440-4(b) | Corta |
| E25 | MOCP | Cursor | 440-4(b) | Corta |
| E26 | Motocompresor: nota | Visible | «Nominal · Selección — 440-6(a)» | Sale (columnas, D5) |
| E27 | Corriente nominal (RLA) | Cursor | 440-6(a), 440-32, 440-22(a) | Corta |
| E28 | Corriente de selección | Cursor | 440-6(a) Excepción 1 | Corta |
| E29 | «225 %» | Visible + cursor | 440-22(a); en grupo, 440-22(b)(1) | Pasa a columna (D10); cursor corto |
| E30 | Motocompresor en grupo, «—» en Servicio | Cursor | 440-6(a), 440-33 | Corta |
| E31 | De habitación: nota | Visible | «Corriente total — 440 Parte G» | Sale (la columna dice «Corriente de placa (A)») |
| E32 | De habitación: corriente | Cursor | 440-62 | Corta |
| E33 | De habitación: «440-62» en Servicio | **Visible** | «440-62» y en el cursor 80 % / 50 % — 440-62(b), (c) | Sale el artículo visible: «—»; cursor corto |

### Otro tablero

| # | Qué | Cómo se ve | Hoy dice | Propuesta |
|---|---|---|---|---|
| E34 | Continua / No continua | Cursor | 220-40 | Corta |
| E35 | «Agregar tablero» | Cursor | 215-2(a)(1), 408-36 | Corta |

### Del desplegable en general

| # | Qué | Cómo se ve | Hoy dice | Propuesta |
|---|---|---|---|---|
| E36 | «Agregar carga» con un equipo solo | Cursor | «…el circuito se calcula como grupo — 430-53, 440-22(b)» | Sale (con D1 ya no hay renglón azul): «Agregar una salida o una carga.» |
| E37 | «No simultáneo con» | Visible (pie) + cursor | 220-60, 430-24 Exc. 3, 440-33 Exc. 1 | Según D11; cursor corto |

## F · Avisos flotantes de la tabla

| # | Cuándo | Hoy dice | Artículos | Propuesta |
|---|---|---|---|---|
| F1 | Una protección fijada regresa al cálculo (I-182) | El circuito y los valores | Según el caso | Con D2, en el cambio a grupo ya no sale. En los demás casos: queda, sin artículo |
| F2 | Subtipo que va solo, con otras líneas | «…va solo en su circuito: quita primero las demás líneas» | — | Queda |
| F3 | Polos que no se pueden cambiar | El motivo | — | Queda |
