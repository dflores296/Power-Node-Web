# Tablero — cómo vamos

**Actualizado:** 2026-09-23: los hallazgos de la prueba de los tres aparatos, y el F.P. por carga.

> **Cómo se leen los porcentajes.** No son una encuesta: cada uno dice qué queda, y ese "qué queda"
> está enlazado. Un número sin su lista es una opinión — regla de `AbaSuite`.

---

## Qué es esto (para no repetir la deriva de alcance de hoy)

Una calculadora web de un **solo cuadro de carga**, como `docs/referencia/CC_NOM_2012_nuevo_nube.xlsx`
en el repo de escritorio: un tablero, hasta 42 espacios, nones/pares (tipo NQ/NF). **No** cascada de
varios tableros, **no** catálogo de equipos, **no** coordinación de protecciones — eso es v2, y sólo
si se decide perseguirlo.

Corre 100 % en el navegador (Blazor WebAssembly), sin servidor, publicada en GitHub Pages. El
proyecto se guarda abriendo/descargando un archivo, no en una base con la que hay que hablar por red.

## Los tres frentes

| Frente | Avance | Qué decide |
|---|---|---|
| **Motor** | **✅ 100%** | Si el cálculo de un circuito da el número correcto |
| **Interfaz** | **🟨 ~75%** | Si se puede capturar y ver el cuadro de carga |
| **Publicación** | **🟨 ~80%** | Si se puede abrir desde una pestaña, sin instalar nada |

### Motor — 100%

`Calculo` + `Domain`, copiados de `dflores296/PowerNode-DesignSuite` (commit `29f660f`), recortados
al alcance de un solo tablero. **Compila solo, sin `Data`, sin EF Core, sin SQL Server.** Detalle
completo de qué se trajo y qué se dejó fuera en
[`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md).

**Las tablas ya funcionan sin base de datos (M-01 cerrado).** `PowerNode.DesignSuite.Normativa`
implementa las trece interfaces leyendo 39 KB de JSON extraído de `NOM-001-SEDE-2012`. Las
implementaciones son las mismas de escritorio con un solo cambio (`IFuenteTablas` en vez de
`DbContext`); la lógica de interpretación de cada tabla no se tocó. **6/6 pruebas verdes** contra
los mismos valores que ya se verificaron a mano contra el PDF del DOF.

### Interfaz — ~75%

**El tablero está completo y el entregable se imprime.** Tres pantallas de trabajo en dos rutas:

- **`/` — captura.** La ficha del tablero con los campos del Excel (identificación, sistema,
  gabinete, condiciones de cálculo), los espacios en **un solo cuadro a todo lo ancho —nones
  arriba, pares abajo—** como la hoja, con el balanceo por fase renglón por renglón y las nueve
  columnas de conductor (hilos, mm², AWG/kcmil de fase, neutro y tierra); **el dibujo del interior
  del gabinete**, con una celda por interruptor y las barras que toca; el resumen de carga con su
  factor de demanda y el desbalanceo; y el alimentador con el interruptor principal.
- **`/documento` — el cuadro de carga impreso**, con las mismas 24 columnas que emite el exportador
  de escritorio, más el renglón del alimentador y el resumen.
- **`/documento` → memoria** — las **nueve secciones** del Excel original por circuito y por
  alimentador, con las fórmulas sustituidas y las citas del cálculo.

**Las reglas del tablero ya no viven en el `.razor`.** `PowerNode.Web.Modelo` (sin Blazor) tiene la
geometría de las barras, la ocupación de un multipolar, el balanceo, el alimentador y la memoria —
**46 pruebas verdes**, y ahí es donde el compilador impide que una regla se escape a una pantalla.

**Qué llegó del escritorio esta sesión, sin reescribirse:** `DistribucionBarras` y
`SistemaDelTablero` (qué barra toca cada espacio, y cuántas barras hay de verdad),
`AcomodoEnGabinete` (si el interruptor cabe), `CalculadoraAlimentador` y
`CalculadoraProteccionAlimentador` (el alimentador y el principal), `CalculadoraDesbalanceo`,
`Verificacion408_36`, y la plantilla de nueve secciones de `Exportacion.Word.SeccionesDeMemoria`.

**Desde el 2026-09-23:** el alimentador se dimensiona con **la fase más cargada** (M-02, era un
bug del lado inseguro), hay aviso cuando el principal queda **debajo** del derivado mayor (M-03), y
cada renglón acepta la carga **en VA, W o A** como viene en la placa (I-25) y lleva **su propio
F.P.** —el tablero ya no tiene uno; el del alimentador resulta de sus cargas— (I-26, I-27).

**Lo que falta:** guardar y abrir el proyecto como archivo (I-05) · circuitos de **Fuerza** (Art.
430: el motor está copiado, la pantalla no los ofrece) · condiciones de cálculo por circuito (hoy
son del tablero entero, como en el Excel, salvo el F.P., que ya es por circuito) · que David
decida las propuestas de abajo.

### Publicación — ~80%

`.github/workflows/deploy.yml` escrito, con las tres trampas de Blazor en Pages ya cubiertas:
`.nojekyll`, `base href` al subdirectorio del repo, y `404.html` para que recargar una ruta no
devuelva 404. CI corre `--check` sobre las tablas y las pruebas antes de publicar.

**Lo que falta:** que David active Pages en la configuración del repo (Settings → Pages → Source:
GitHub Actions) y que el workflow corra una vez de verdad. **Escrito, nunca ejecutado.**

## Decisiones que están esperando

**Activar GitHub Pages en el repo** (Settings → Pages → Source: GitHub Actions). Es lo único que
separa el sitio de estar en línea, y no lo puede hacer una sesión de Claude.

**Dos propuestas de la prueba del 2026-09-22**, `PROPUESTA · Claude · 2026-09-23`:

- **I-28 / M-03** — cómo redactar los avisos del principal sin «el Excel original», y si el principal
  menor que un derivado es aviso o bloqueo:
  [`../decisiones/interruptor-principal-criterios-del-excel.md`](../decisiones/interruptor-principal-criterios-del-excel.md).
- **I-29** — opción «sin valores IEC (16, 32, 63 A)»:
  [`../decisiones/serie-de-interruptores-sin-valores-iec.md`](../decisiones/serie-de-interruptores-sin-valores-iec.md).

**Reportar M-02 en `PowerNode-DesignSuite`.** El texto está listo en `HALLAZGOS.md` §M-02; la sesión
del 2026-09-23 no tuvo acceso a ese repo.
