# Tablero — cómo vamos

**Actualizado:** 2026-09-22, segunda sesión: primera pantalla viva.

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
| **Interfaz** | **🟨 ~35%** | Si se puede capturar y ver el cuadro de carga |
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

### Interfaz — ~35%

**La primera pantalla existe y calcula de verdad** (`Pages/CuadroDeCarga.razor`): dos bloques
—nones izquierda, pares derecha, como el Excel—, captura de descripción/tipo/VA/longitud/polos,
fase derivada por la convención NEMA de pares, y la memoria con las citas de norma de cada paso.
Verificada en el navegador con Playwright: los tres circuitos de prueba dan **exactamente** los
mismos números que la app de escritorio (5.67 A → 15 A → 12 AWG → 1.34 %).

**Lo que falta:** el resumen de carga por fase y el balanceo · circuitos de Fuerza (Art. 430, el
motor ya está copiado pero la pantalla no lo ofrece) · guardar y abrir el proyecto como archivo ·
los datos de identificación (tablero, clave, ubicación, proyecto) que el Excel lleva arriba del
cuadro · exportar.

### Publicación — ~80%

`.github/workflows/deploy.yml` escrito, con las tres trampas de Blazor en Pages ya cubiertas:
`.nojekyll`, `base href` al subdirectorio del repo, y `404.html` para que recargar una ruta no
devuelva 404. CI corre `--check` sobre las tablas y las pruebas antes de publicar.

**Lo que falta:** que David active Pages en la configuración del repo (Settings → Pages → Source:
GitHub Actions) y que el workflow corra una vez de verdad. **Escrito, nunca ejecutado.**

## Decisiones que están esperando

**Activar GitHub Pages en el repo** (Settings → Pages → Source: GitHub Actions). Es lo único que
separa el sitio de estar en línea, y no lo puede hacer una sesión de Claude.
