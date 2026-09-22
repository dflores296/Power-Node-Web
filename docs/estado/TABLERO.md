# Tablero — cómo vamos

**Actualizado:** 2026-09-22, sesión de arranque.

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
| **Interfaz** | **⬜ 0%** | Si se puede capturar y ver el cuadro de carga |
| **Publicación** | **⬜ 0%** | Si se puede abrir desde una pestaña, sin instalar nada |

### Motor — 100%

`Calculo` + `Domain`, copiados de `dflores296/PowerNode-DesignSuite` (commit `29f660f`), recortados
al alcance de un solo tablero. **Compila solo, sin `Data`, sin EF Core, sin SQL Server.** Detalle
completo de qué se trajo y qué se dejó fuera en
[`../conocimiento/motor-copiado.md`](../conocimiento/motor-copiado.md).

**Lo que falta para que el motor *funcione*, no sólo compile:** las tablas de la norma
(`ITablaAmpacidad`, `ICatalogoCalibres`, etc.) hoy son interfaces sin implementación — en escritorio
las resuelve EF Core contra SQL Server. Aquí hace falta una implementación ligera que lea el JSON de
`NOM-001-SEDE-2012` directo en memoria, sin base de datos. **No empezado.**

### Interfaz — 0%

Las pantallas de captura y el cuadro de carga en sí (columnas B→CA del Excel, ver
`docs/referencia/cuadro-de-carga.md` en el repo de escritorio). Mirando `TableroCircuitosView.xaml`
y `GabineteView.xaml` de la app de escritorio como referencia de forma — no se copia código WPF, se
reconstruye en Blazor.

**No empezado.**

### Publicación — 0%

`.github/workflows/deploy.yml`, calcado del de `NOM-001-SEDE-2012`, con el `.nojekyll` que Blazor
necesita (GitHub Pages ignora `_framework/` sin él — trampa conocida, ver
`msa-toolkit/docs/despliegue.md` para el patrón hermano).

**No empezado.**

## Decisiones que están esperando

Ninguna pendiente ahora mismo. Todo lo de hoy quedó `CONFIRMADA` — ver `../decisiones/`.
