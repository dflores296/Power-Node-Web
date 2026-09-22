# El Excel original, leído celda por celda

Lo que sigue salió de **abrir `CC_NOM_2012_nuevo_nube.xlsx`** (el que David subió el 2026-09-22 a la
sesión; el mismo archivo vive en `docs/referencia/` del repo de escritorio) y leer sus fórmulas, no
de suponer qué debería traer un cuadro de carga. Es la **referencia de forma** de este repo: el
formato del entregable no se inventa, se reproduce.

> El Excel dependía de un libro externo con las tablas de la norma, **`BD NOM 2012 - nuevo.xls`**,
> cuyo vínculo está roto: todas sus fórmulas dicen `[1]Hoja1!$K$344`. Ese archivo ya no importa —
> era la base de tableros Square D, y las tablas que sí hacen falta (240-6(a), 310-15(b)(16),
> 310-15(b)(2)(a), Tabla 8, Tabla 9, 250-122) están en `wwwroot/datos/tablas-nom.json`, verificadas
> contra el PDF del DOF.

---

## Las dos hojas

| Hoja | Qué es | Áreas de impresión |
|---|---|---|
| **Cuadro de Carga** | La tabla que se pega como imagen en el plano | `B17:CA80` (lo presentable) y `CC17:EK80` (la maquinaria) |
| **Memoria de Cálculo** | El documento que se entrega impreso | `C3:O73`, `C78:O148`, `Q3:AC73` — una hoja por alimentador |

**Lo que vive de `CC` a `EK` es el cálculo intermedio** —consideraciones, factores de corrección,
selección de conductor, caída de tensión, puesta a tierra— y es exactamente **lo que aquí ya no se
expone**: eso hoy es el motor, con sus citas. La hoja tenía que llevarlo a la vista porque una hoja
de cálculo no tiene otro lugar donde ponerlo.

## Los renglones

| Renglones | Circuitos | Qué son |
|---|---|---|
| 34–54 (21) | 1, 3, 5 … 41 | los **nones** — la columna izquierda del gabinete |
| 55 | — | **renglón en blanco de 6 pt**: el único corte entre los dos bloques |
| 56–76 (21) | 2, 4, 6 … 42 | los **pares** — la columna derecha |
| 78 | `TOTAL UNIDADES` | — |
| 79 | `TOTAL (VA)` y **la fila del alimentador** | — |

> **Los bloques no llevan rótulo, y va un bloque encima del otro.** Se buscó «LADO», «IZQUIERDO»,
> «DERECHO», «NONES» y «PARES» en las 4 022 celdas con contenido de la hoja: no aparecen. Lo único
> que separa los nones de los pares es ese renglón delgado de la fila 55 — **el número de circuito
> ya dice de qué columna del gabinete es**. Por eso aquí tampoco se rotulan, y la pantalla de captura
> los apila igual que la hoja en vez de ponerlos lado a lado.

O sea **42 espacios**. Para un tablero más chico se ocultaban renglones a mano, de los dos bloques
por igual. **Ese paso manual desaparece aquí:** el cuadro se dibuja con los espacios declarados.

## El encabezado (filas 19 a 26)

| Celdas | Campo | Aquí |
|---|---|---|
| `H19`, `B22`, `B25` | TABLERO, CLAVE, UBICACIÓN | ✅ se capturan |
| `BN19`, `BM21`–`BM26` | PROYECTO, CLIENTE, DISEÑÓ, REVISÓ, APROBÓ, FECHA, REVISIÓN | ✅ se capturan |
| `J23`, `J24`, `J25` | MONTAJE, MAT/BARRAS, GABINETE NEMA | ✅ se capturan |
| `T22`, `T24`, `T25`, `T26` | TENSIÓN F-F, FRECUENCIA, No. DE FASES, No. DE HILOS | ✅ se capturan |
| `T23` | TENSIÓN F-N = `ROUND(T22/SQRT(3),1)` | ⚠️ **ver abajo** |
| `T21` | INT. PPAL. = `CZ79` | ✅ sale del cálculo |
| `J21`, `J22`, `O21`–`O26`, `J26` | CATÁLOGO, MARCA, INTERIOR, CAJA, FRENTE, DIMENSIONES, CAPACIDAD | ❌ ocho `INDEX/MATCH` contra la base Square D — ver `../decisiones/sin-catalogo-square-d.md` |

> ⚠️ **La tensión fase-neutro del Excel es siempre ÷√3, y eso solo vale para una estrella.** En un
> 1F-3H (derivación central) son ÷2 —240 da 120, no 138.6— y en un 1F-2H la tensión capturada *ya
> es* la fase-neutro. Aquí lo resuelve `SistemaDelTablero.TensionFaseNeutro`, que es donde el motor
> guarda esa regla. **Es la primera diferencia deliberada con el Excel**, y va a favor del Excel solo
> en el caso trifásico, que es el que el formato asume.

## Cómo reparte las fases

Las columnas `CO34:CT34` son un `IF(OR(...))` de 48 comparaciones por celda. Traducido:

| Sistema (`T25`) | Fase A | Fase B | Fase C |
|---|---|---|---|
| 3 fases | 1, 2, 7, 8, 13, 14, 19, 20… | 3, 4, 9, 10, 15, 16… | 5, 6, 11, 12, 17, 18… |
| 2 fases | 1, 2, 5, 6, 9, 10… | 3, 4, 7, 8, 11, 12… | — |
| 1 fase | todo | — | — |

**Es la convención NEMA por pares**, la misma que ya implementaba `DistribucionBarras` en el
escritorio: los dos espacios a la misma altura muerden la misma barra. El Excel la confirma de forma
independiente. Un multipolar toma `N`, `N+2`, `N+4` —del mismo lado— y su carga se divide entre las
fases que toca (`CJ34/CI34`).

## La fila 79: el alimentador

**Tiene exactamente las mismas fórmulas que un renglón de circuito**, con la carga total en vez de la
de un circuito. Por eso aquí se calcula con `CalculadoraAlimentador` (Art. 215) y no con algo propio:
el Excel ya trataba el alimentador como «un circuito grande», que es lo que dice el motor copiado.

Dos cosas de esa fila **no salen de la norma** y quedaron anotadas en
[`../decisiones/interruptor-principal-criterios-del-excel.md`](../decisiones/interruptor-principal-criterios-del-excel.md):
el piso de 30 A del principal y el «si empata con el derivado más grande, sube un tamaño».

`CT79` es el **desbalanceo**: `(max - min) / max × 100` sobre los VA por fase.

## Las columnas del entregable (B a CA)

| Col. | Qué es |
|---|---|
| `B` | Número de circuito = espacio de la barra |
| `C`–`AV` | Descripción (banda ancha; las filas 31 y 32 llevaban VA continua/no continua por tipo de carga) |
| `AW`–`BA` | CARGA (VA): continua, F.D., no continua, F.D., instalada |
| `BB`–`BD` | BALANCEO DE FASES: A, B, C |
| `BJ`–`BL` | PROTECCIÓN: P × A |
| `BM`–`BP` | FASE: hilos, mm², AWG/kcmil |
| `BQ`–`BT` | NEUTRO: hilos, mm², AWG/kcmil |
| `BU`–`BX` | PUESTA A TIERRA: hilos, mm², AWG/kcmil |
| `BY` | In (A) |
| `BZ` | L (m) |
| `CA` | **e (%)** — la última columna del entregable |

Son las mismas 24 columnas que emite el exportador de escritorio
(`Exportacion/CuadroDeCarga/ColumnasCuadroDeCarga.cs`), y las mismas que imprime `/documento`.

## Los dos factores de demanda por renglón (`AX`, `AZ`)

El Excel los lleva **por circuito** y otra vez en el total. Aquí se capturan **solo en el total**, y
el renglón imprime `1.00`, que es la verdad literal de lo que se calculó: **220-42 prohíbe
expresamente** aplicar factor de demanda al circuito derivado, y **220-40** lo pone en el
alimentador, sobre la carga acumulada. La versión de escritorio llegó a la misma conclusión el
2026-08-20.
