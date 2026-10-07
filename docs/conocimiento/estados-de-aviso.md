# Componente: el estado del circuito (error y advertencia)

**DECIDIDA · David · 2026-10-07: opción A** (abajo). **Implementada** en `1f97c39` (I-202). Propuesta de Claude del mismo día — pedida por David: «necesitamos una forma clara de detectar warnings y
errors en la tabla». Vista: [`../mockups/desglose-por-bloques.html`](../mockups/desglose-por-bloques.html)
(capturas `-1900.png` y `-1440.png`), con las tres opciones lado a lado. Qué dice cada aviso:
[`../decisiones/redaccion-de-avisos.md`](../decisiones/redaccion-de-avisos.md) y
[`avisos-de-la-captura.md`](avisos-de-la-captura.md).

## El problema

Hoy un error es un renglón rojo aparte que cruza toda la tabla, saltándose las columnas. Una advertencia
es un ⚠ chico junto a la descripción, que no se ve al recorrer un tablero de 42 circuitos.

## Lo que ya hay

| Pieza | Qué hace | Por qué no basta |
|---|---|---|
| `tr.fila-error` | Un renglón rojo bajo el circuito | Rompe la tabla: cruza todas las columnas |
| `.aviso-renglon` (⚠) | Ícono junto a la descripción | Chico, solo; no se ve al recorrer |
| `.excede` en e (%) | La caída en rojo | Solo para la caída |
| Tarjeta «Avisos» al pie | Todos los avisos juntos | Lejos del circuito |

## La propuesta (opción C)

| Severidad | Cuándo | El renglón del circuito | El mensaje | En el desplegable |
|---|---|---|---|---|
| **Error** | No se calcula | Tinte `--alerta-fondo` en todo el renglón; barra de 3 px `--alerta` a la izquierda; ícono ● con «!» antes de la descripción | En las celdas de resultado (In, Protec., Fase, e %), que sin cálculo quedan vacías: «Falta la tensión de entrada.» | El campo que falta, con borde `--alerta` |
| **Advertencia** | Se calcula; hay que revisar | Tinte `--aviso-fondo`; barra de 3 px `--aviso-borde`; ícono ▲ con «!» | Al pasar el cursor por el ícono; la referencia, en otra línea. El valor que la causa, marcado en `--aviso-texto` con subrayado punteado | El campo que la causa, si es uno, con borde `--aviso-borde` |
| Normal | — | Sin tinte ni barra | — | — |

- **Un error tapa las advertencias** del mismo circuito: primero se corrige lo que impide calcular.
- **Un contador** arriba del cuadro: «1 error · 2 advertencias». Un clic lleva al siguiente.
- **Colores:** solo los tokens que ya existen, con su pareja en el tema oscuro (`--alerta`,
  `--alerta-fondo`, `--aviso-borde`, `--aviso-fondo`, `--aviso-texto`). Ninguno nuevo.
- **No depende del color:** el círculo es error, el triángulo es advertencia; la barra y el texto también lo dicen.
- **Contraste:** `--alerta` sobre `--alerta-fondo`, 5.6 : 1; `--aviso-texto` sobre `--aviso-fondo`, ~6 : 1.
- **Lector de pantalla:** el renglón lleva `aria-invalid="true"` en error; el ícono, `role="img"` con
  `aria-label` = el mensaje.

## Las otras dos opciones (en la vista)

- **A · Enmarcar solo el aviso.** Discreta, pero no se ve al recorrer: el borde es chico y está lejos de la
  descripción.
- **B · Pintar el renglón en color fuerte.** Se ve, pero con tres o cuatro avisos la tabla se vuelve un
  semáforo y el texto pierde contraste.

## Lo que decidió David (2026-10-07)

1. **Opción A · enmarcar el aviso: solo la celda, con borde.** Sin tinte del renglón, sin barra, sin ícono.
   - **Error:** el mensaje en las celdas de resultado (In, Protec., Fase, e %), enmarcado en `--alerta`. En el
     desplegable, el campo que falta con borde `--alerta`.
   - **Advertencia:** la celda del valor que la causa (la protección, la caída…), enmarcada en `--aviso-borde`
     y su valor en `--aviso-texto`. El mensaje, al pasar el cursor por esa celda, con la referencia en otra
     línea. (Antes iba en el ⚠ junto a la descripción: con A, el ⚠ sale.)
2. **El contador arriba del cuadro: sí.** «1 error · 2 advertencias»; un clic lleva al siguiente.
3. **La tarjeta «Avisos» del pie: se queda**, con todos juntos.

La recomendación de Claude era C; queda en la vista como referencia.

### Qué celda enmarca cada advertencia (propuesta de Claude)

Con A, cada advertencia necesita la celda del valor que la causa. Las de
[`avisos-de-la-captura.md`](avisos-de-la-captura.md):

| Advertencia | Celda |
|---|---|
| B7 Contactos de 15 o 20 A en un circuito de más de 20 A | Protec. (A) |
| B8 Equipo fijo de más del 50 % del circuito | Protec. (A) |
| B9 a B11 Alumbrado en un circuito de 30 A, 40 o 50 A, o más | Protec. (A) |
| B12 Contactos de vivienda a más de 120 V | P (los polos dan la tensión) |
| B13 Caída combinada de más de 5 % | e (%) |
| B14, B15 Acondicionador de habitación de más del 80 % o 50 % | Protec. (A) |
| B16 Con bypass, ninguna protección cabe | Protec. (A) |
| B18 Ningún tubo admite los conductores | Canal. |

Dos advertencias en la misma celda: un solo marco, y las dos en su cuadro de ayuda, una por línea.
