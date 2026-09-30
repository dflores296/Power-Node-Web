# Trazabilidad: cargas y clases de circuito

Un cambio grande y en varias fases: el tipo pasa del circuito a cada carga y la clase del circuito sale
de sus cargas. Aquí va **cada commit** con qué hizo, qué hallazgo toca y cómo se verificó, para poder
seguirlo de punta a punta — pedido de David, 2026-09-29.

Decisión: [`../decisiones/cargas-y-clases-de-circuito.md`](../decisiones/cargas-y-clases-de-circuito.md).
Hallazgos: I-123 a I-127 y M-14 en [`../estado/HALLAZGOS.md`](../estado/HALLAZGOS.md).

## Fases

| Fase | Qué | Hallazgos | Estado |
|---|---|---|---|
| — | Propuesta y hallazgos abiertos | — | Hecha |
| A | Guía de cargas y glosario | I-126 | Hecha |
| B | Modelo: carga con tipo, subtipo y forma; clase del circuito; alimentador a tablero; F.D. por carga; archivo formato 5 | I-123, I-125 | Hecha |
| C | Pantalla, documento y memoria con los nombres cortos; se retira «Varios» | I-127 | Hecha |
| D | Reglas por clase de circuito | I-124 | En curso |
| E | Mínimo de alumbrado general por superficie (220-12, 220-14(j)(k)) | M-14 | En pausa |

## Commits

Un renglón por commit, en orden. El commit de documentos que pone el hash de uno de código va en el
renglón siguiente.

| # | Commit | Fase | Qué | Hallazgos | Verificación |
|---|---|---|---|---|---|
| 1 | `0c988d6` | — | Propuesta con las decisiones de David; hallazgos abiertos; esta tabla | I-123 a I-127, M-14 | — |
| 2 | `38e2c24` | A | Página «Guía de cargas» (`/guia`): mapa y árbol de los tipos con sus subtipos, clases de circuito, glosario; «?» en el encabezado Tipo; barra compacta desde 1040 y 800 px. Modelo: `GuiaDeCargas`, `SubtipoDeCarga` | I-126 | `GuiaDeCargasTests` (8); 334 + 23 pruebas; navegador 360–1920 px, claro y oscuro, mapa, dirección con `#rama-…`, impresión |
| 3 | `4eb02ea` | A | I-126 con su hash; requisito E-7; bitácora; esta tabla | I-126 | — |
| 4 | `e20d9dc` | — | Plan de las fases B a E y cambios a la UI, en pausa (David) | — | — |
| 5 | `dc7cdd2` | B | `AparatoDelCircuito` → `CargaDelCircuito`, `Aparatos` → `Cargas`; el archivo no cambia | I-123 | 334 pruebas sin cambio |
| 6 | `72bee16` | B | Subtipo y tipo de cada carga; porciones por tipo y F.D. por carga en resumen, alimentador, motores y no simultáneos; clase del circuito; grupo por contenido; mínimos por subtipo (220-14, 220-54); calentador y calefacción continuos; tipo Tablero con 215 y F.D. 1; archivo formato 5; `Tramo` en el motor (motor-copiado.md) | I-123, I-125 | `CargasYClasesTests` (25); 359 + 23 pruebas |
| 7 | `0e48028` | C | Pantalla: «Salidas y cargas» con tipo, subtipo y «?»; «Combinadas»; clase bajo la descripción; sin «Varios»; nombres de la NOM; unidad de 80 px. Documento y memoria: tipo, clase, cargas combinadas, 215 del tablero | I-123, I-127 | 359 + 23 pruebas; navegador a 1920: cuadro de 1676 px, ningún selector cortado, circuito combinado, grupo de motores, tablero de 30.5 kVA (90 A), documento y memoria |
| 8 | *(este)* | B, C | Hallazgos con su hash; decisión: plan aprobado y ajuste de la clase; bitácora; esta tabla | I-123, I-125, I-127 | — |

## Plan de las fases B a E — en pausa

**En pausa · David · 2026-09-29**: «deliberar por partes»; David estudia la guía de cargas antes de
seguir. Es el plan, no lo decidido: nada de B a E se ejecuta hasta que David lo revise.

### Fases

| Fase | Estado | Qué hace | Hallazgos | Archivos principales |
|---|---|---|---|---|
| A | **Hecha** (`38e2c24`) | Página «Guía de cargas» (`/guia`): mapa + árbol de 7 tipos y 22 subtipos, clases de circuito, glosario; «?» en el encabezado Tipo; imprimible | I-126 | `GuiaDeCargas.cs`, `SubtipoDeCarga.cs`, `Pages/Guia.razor`, `js/guia.js` |
| B | Siguiente | **Modelo**: `AparatoDelCircuito` → `CargaDelCircuito` con `Tipo` y `Subtipo`; el renglón es la «carga del renglón» (se queda); porciones de carga por tipo en cada circuito → F.D. por carga en resumen, alimentador, motores del alimentador y no simultáneos; `CategoriaDeCarga.Tablero` (F.D. fijo 1, cálculo con 215-2(a)(1)/215-3 vía `Tramo` en `DatosEntradaCircuitoDerivadoNoMotor`); `FormaDeCalculo` y `ClaseDeCircuito` derivadas; grupo por contenido (se retiran `CapturaDeMotor.Grupo` y `PlacaAire.Grupo`); mínimos por subtipo (180, 90, 600 VA; anuncios 1200 VA; secadora 5000 VA en vivienda; calentador y calefacción continuos); archivo formato 5 con migración desde ≤4 | I-123, I-125 | `CircuitoDelCuadro.cs`, `CuadroDeCarga.cs`, `DatosDelTablero.cs`, `ArchivoDelCuadro.cs`/`ArchivoJson.cs`, `CalculadoraCircuitoDerivadoNoMotor.cs`, `motor-copiado.md` |
| C | Después | **Pantalla, documento y memoria** con los nombres cortos (tabla de abajo); se retira «Varios» de la pantalla | I-127 | `Captura.razor`, `Documento.razor`, `MemoriaDeCalculo.cs`, `app.css` |
| D | Después | **Reglas por clase**: 210-21(b)(1) contacto ≥ circuito en individual; Tabla 210-21(b)(3); 210-23(a)(1)(2) avisos 80 %/50 %; 422-11(e) un aparato no operado por motor; 240-4(b)(1) por número de contactos (ya no «todo Contactos»); 210-19(a)(3) estufa ≥ 8.75 kW → 40 A | I-124 | `CuadroDeCarga.cs`, engine NoMotor (`permiteExcepcion2404b`) |
| E | Después | **Mínimo de alumbrado general por superficie**: extraer Tabla 220-12; área servida y uso del local en Datos; ajuste en el alimentador; vivienda: contactos de uso general dentro de los VA/m² (220-14(j)); oficinas/bancos: mayor entre 180 VA y 11 VA/m² (220-14(k)) | M-14 | `tools/extraer_tablas.py`, `DatosDelTablero.cs`, `CuadroDeCarga.cs`, `Captura.razor` |

Clase del circuito (fase B, ajuste mío a la tabla de la sección 3 de la propuesta, se anota ahí):
Individual = una sola carga de equipo, cantidad 1 (o contactos con uso Refrigerador); Uso general =
carga total o varias salidas con alumbrado o contactos de uso general; Para aparatos = solo aparatos,
o contactos de vivienda para aparatos pequeños y lavadora; Alimentador = tipo Tablero.

### Cambios a la UI (Fase C, salvo donde se dice)

| Dónde | Hoy | Con el cambio | Por qué |
|---|---|---|---|
| Barra | 3 páginas | «Guía de cargas» — **hecho (A)** | Guía |
| Cuadro · columna Tipo | Selector de 6 tipos, el tipo es del circuito | Mismo selector + «Tablero»; si las cargas son de tipos distintos muestra **«Combinadas»** (cambiarlo pone ese tipo a todas las cargas); nombres cortos: Aparatos, Motores, A/A y refrig. | El tipo es de la carga (I-123) |
| Cuadro · bajo la descripción | — | Nota corta de la clase: «Individual», «Uso general · 10 salidas», «Para aparatos», «Carga total», «Alimentador» | La clase sale de las cargas |
| Cuadro · «?» | En el encabezado Tipo — **hecho (A)** | Además, en cada línea del desplegable junto a su tipo/subtipo, a su rama | Decisión 5 de David |
| Cuadro · unidad (Motor) | HP · A · VFD · Varios | HP · A · **Vel. aj.**; «Varios» desaparece: varios motores = abrir el desplegable | Se retira «Varios» (decisión 4) |
| Cuadro · unidad (A/C) | MCA · RLA · Hab. · Varios | **Ampac.** · **Nominal** · **De hab.** (verificar que quepa a 1920) ; notas «Ampacidad / Prot. máx.» en lugar de MCA/MOCP | Nombres NOM cortos |
| Cuadro · Tablero | No existe | Tipo «Tablero»: captura continua y no continua **ya calculadas** del otro tablero; sin F.D.; resultado con 215-2(a)(1)/215-3 | I-125 |
| Desplegable (desglose) | «Aparatos del circuito», sin tipo; clase Carga/Motor/Motocompresor/Hab. | **«Salidas y cargas del circuito»**: cada línea con selector de **tipo y subtipo** (optgroups por tipo), cantidad, valor unitario y unidad según el subtipo (VA/W/A, HP/A, Nominal); continua forzada en calentador y calefacción; mínimos de 220-14 mostrados como nota («180 VA mín.») | Una sola lista de cargas con tipo |
| Desplegable · renglón | Al abrirlo, lo capturado se vuelve la primera línea | Igual, con el tipo del circuito y el subtipo por omisión (Luminarias, Uso general, Fijo, Uso general, Motocompresor, Resistencia) | La captura por renglón se queda (decisión 3) |
| Uso de contactos (vivienda) | Cocina · Lavadora · Baño · Refrigerador | **Aparatos pequeños** · Lavadora · Baño · Refrigerador | Nombre NOM corto |
| Neutro | «Neutro comp.» | **Multiconductor** | Nombre NOM |
| Resumen de carga | Instalada / Demandada por tipo de circuito | **Carga conectada / Demanda** por tipo de **carga** (un circuito combinado reparte su carga en varios renglones); renglón «Tablero» con F.D. 1 fijo, no editable | F.D. por carga (decisión 1 y 2) |
| Documento (cuadro impreso) | Tipo del circuito | Tipo o «Combinadas» y la clase; encabezados «Carga conectada», «Demanda» | Mismos nombres que la pantalla |
| Memoria | Aparatos del circuito; citas 210 | «Salidas y cargas» con tipo y subtipo de cada una; clase del circuito con su artículo (Art. 100); en Tablero, 215-2(a)(1)/215-3 y 220-40 («sin otro F.D.») | Trazabilidad a la NOM |
| Fase D en pantalla | 240-4(b) negado a todo Contactos | Avisos en el renglón: 210-21(b)(1), 210-23(a), 422-11(e); 240-4(b)(1) según contactos reales | I-124 |
| Fase E en pantalla | — | En Datos del tablero: área servida (m²) y uso del local; renglón «Mínimo 220-12» en el resumen, como el de 220-52 | M-14 |

Lo que no se toca: la geometría del cuadro, el gabinete, canalizaciones, alimentador principal
(salvo el F.D. por carga), y el ancho de 1920 px (cada cambio se mide).

### Verificación (cada fase)

- `dotnet build src/PowerNode.Web` sin advertencias; `dotnet test` en `PowerNode.Normativa.Tests` y
  `PowerNode.Web.Tests`; pruebas nuevas con números a mano (p. ej. circuito con 1000 VA de alumbrado
  y 900 VA de contactos, F.D. 0.5 solo en alumbrado → demanda 1400 VA; tablero de 30 kVA con F.D.
  de alumbrado 0.5 → entra 30 kVA; archivo formato 4 abre igual en formato 5).
- Navegador (dev server en 127.0.0.1:5199 + Playwright): casos de cada fase, 360–1920 px, claro y
  oscuro, guardar/abrir, impresión; que el cuadro siga midiendo ≤ 1676 px a 1920.
- Commit por cambio, hash en `HALLAZGOS.md` y en la tabla de trazabilidad; `main` por avance rápido
  al cerrar cada fase.
