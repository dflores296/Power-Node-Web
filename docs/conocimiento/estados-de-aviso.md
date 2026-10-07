# Componente: el estado del circuito (error y advertencia) — propuesta

**PROPUESTA · Claude · 2026-10-07** — pedida por David: «necesitamos una forma clara de detectar warnings y
errors en la tabla». Vista: [`../mockups/desglose-por-bloques.html`](../mockups/desglose-por-bloques.html)
(capturas `-1900.png` y `-1440.png`), con las tres opciones lado a lado. Qué dice cada aviso:
[`../decisiones/redaccion-de-avisos.md`](../decisiones/redaccion-de-avisos.md) y
[`avisos-de-la-captura.md`](avisos-de-la-captura.md). Falta que David escoja.

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

## Por decidir (David)

1. ¿C, o A, o B?
2. ¿El contador arriba del cuadro?
3. ¿La tarjeta «Avisos» del pie se queda (todos juntos) o sale ahora que cada circuito lo enseña?
