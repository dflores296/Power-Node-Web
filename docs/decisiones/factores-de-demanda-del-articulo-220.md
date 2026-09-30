# Factores de demanda del Art. 220: sugerirlos, sin quitar el criterio

**PROPUESTA · Claude · 2026-09-30** — auditoría NOM del 2026-09-29, P3-1 y riesgo 2 (I-150). No
implementada: cambia R-12, **decisión de David** (el factor de demanda sigue a criterio del ingeniero,
con justificación; sin automatizar). Falta que David decida.

## Lo que pide la auditoría

- **P3-1.** Que el F.D. salga solo según el inmueble: Tabla 220-42 (alumbrado), 220-44 (contactos fuera
  de vivienda), 220-53 (cuatro o más aparatos fijos en vivienda), 220-54 (secadoras), Tabla 220-55
  (cocción, con sus notas; la Nota 4 también para el derivado: una estufa de 12 kW con 8 kW y 40 A),
  220-56 (cocina comercial), 220-60 (no coincidentes). Hoy el F.D. es 1.00 en cualquier inmueble.
- **Riesgo 2.** Un F.D. manual no tiene tope: 0.10 en motores y alumbrado con solo escoger una
  justificación bajó el principal de 175 A a 125 A.

## Lo que dice la NOM

| Sección | Verbo | Consecuencia |
|---|---|---|
| 220-42 | «se **deben** aplicar» los factores de la Tabla 220-42 al alumbrado general | Obliga al método; 1.00 da una carga mayor, del lado seguro |
| 220-44 | «se **permite**» calcular los contactos con la Tabla 220-42 o la 220-44 | Opcional |
| 220-53 | «se **permite** aplicar un factor de demanda del 75 %» | Opcional |
| 220-54 | la carga «**debe ser** de 5 000 VA» o la de placa, la mayor; factores de la Tabla 220-54 «se **permite**» | El mínimo ya se aplica (C-11); el factor es opcional |
| 220-55 | «Se **permite** aplicar los factores de demanda de la Tabla 220-55» | Opcional, también su Nota 4 para el derivado |
| 220-56 | «Se **permitirá** calcular … de acuerdo con la Tabla 220-56» | Opcional |
| 220-60 | «se **puede** omitir la más pequeña» | Ya está, por par de circuitos (I-121) |
| 430-26 | los conductores «**pueden** tener una ampacidad menor» | Opcional. **La NOM no pide autorización** (la auditoría la toma del NEC, 430.26) |

Salvo 220-42, la norma **permite** reducir; no aplicar el factor da una carga igual o mayor, que cumple.
La auditoría lo lista como «la NOM lo pide y la app no lo hace»; es más exacto decir que la app no
ofrece una reducción que la NOM permite, y que no dice cuánto se aleja un F.D. manual de la tabla.

## La propuesta

1. **Sugerir, no imponer.** En «Resumen de carga», junto a cada F.D., el valor que da la tabla de su
   tipo para el inmueble y la carga capturada —con 7 000 VA de alumbrado en vivienda, «Tabla 220-42:
   0.63 (3 000 VA al 100 %, el resto al 35 %)»— con un botón «Usar». El valor por omisión sigue en 1.00 y la justificación se llena sola al
   usar la sugerencia.
2. **Avisar cuando el F.D. manual queda abajo del de la tabla** (riesgo 2): «0.10 es menor que el 0.63 de
   la Tabla 220-42 para vivienda». Con «Otra» como justificación, pedir el texto (ya se pide).
3. **Motores (430-26):** avisar cuando el F.D. deja el alimentador abajo de la suma de 430-24 sin el
   motor mayor; recordar que la ampacidad debe alcanzar «la carga máxima determinada … con sus ciclos de
   servicio». Sin mencionar autorización: la NOM no la pide.
4. **220-55 Nota 4 en el derivado:** ofrecerla como opción de la línea de cocción («Calcular el derivado
   con la Tabla 220-55, Nota 4»), apagada por omisión. Con ella, la estufa de 12 kW va con 8 kW:
   36.4 A → 40 A (210-19(a)(3)), 8 AWG.

## Lo que se pierde

- Con sugerencias, el F.D. sigue siendo decisión del ingeniero (R-12); un tablero capturado sin
  tocarlos sale igual que hoy, del lado seguro.
- Automatizarlo por completo, como pide la auditoría, cambiaría el resultado de todos los tableros
  guardados al abrirlos: la memoria de un tablero ya entregado dejaría de coincidir.

## Preguntas para David

1. ¿Sugerencia con botón (propuesta) o F.D. automático por omisión (auditoría)?
2. ¿El aviso del F.D. manual abajo de la tabla?
3. ¿La Nota 4 de la Tabla 220-55 como opción del derivado de cocción?
