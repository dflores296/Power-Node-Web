# El motor copiado: qué se trajo, qué se dejó, y los cinco ajustes

Origen: `dflores296/PowerNode-DesignSuite`, commit `29f660f`, 2026-09-22. Decisión de copiar (no
enlazar) en [`../decisiones/motor-copiado-no-enlazado.md`](../decisiones/motor-copiado-no-enlazado.md).
Decisión de alcance en [`../decisiones/alcance-v1-un-tablero.md`](../decisiones/alcance-v1-un-tablero.md).

**Verificado compilando, no sólo copiado:** `dotnet build` limpio en este repo, sin `Data`, sin EF
Core, sin SQL Server, sin ningún paquete NuGet — 119 archivos, 10 719 líneas.

## Qué se copió completo

| De | Qué es |
|---|---|
| `Calculo/Casos/` | El cálculo de un circuito: protección, conductor, caída de tensión |
| `Calculo/Tableros/` | `DistribucionBarras` — nones izquierda / pares derecha (NEMA por pares) |
| `Calculo/Simbologia/` | Qué símbolo NMX-J-136-ANCE le toca a cada carga (lógica, no el dibujo) |
| `Calculo/Unidades/`, `Magnitudes/`, `Validaciones/`, `Derivaciones/`, `Diagramas/` | Tipos y reglas base |
| `Calculo/TablasNom/` | Las **interfaces** de las tablas de la norma (`ITablaAmpacidad`, etc.) — sin implementación |
| `Domain/Proyectos/` (recortado, ver abajo) | `Tablero`, `CircuitoDerivado`, `ElementoCircuito`, `CalculoCircuitoDerivado`, `EspacioTablero`, `ConfiguracionProyecto`, `DatosMotor`... |
| `Domain/Norma/` | `Cita(Referencia, Descripcion)` — la razón por la que la memoria puede decir "según Tabla 310-15(b)(16)" |
| `Domain/Advertencias/`, `Entregables/`, `Referencia/` | El resto del soporte, sin recorte |

## Qué NO se copió, y por qué

| Excluido | Por qué |
|---|---|
| `Calculo/Coordinacion/` (curvas de disparo, selectividad) | Fuera del alcance v1 — ver decisión de alcance |
| `Domain/Catalogo/` (formas de `ModeloTablero`, `ModeloInterruptor`...) | Aunque son clases vacías sin datos de Schneider, no hacen falta sin catálogo |
| `Domain/Proyectos/AjustesDisparoCapturados.cs` | Ajustes LSIG — coordinación |
| `Domain/Proyectos/CoberturaDeCoordinacion.cs` | Coordinación |
| `Domain/Proyectos/CoordinacionDeProtecciones.cs` | Coordinación |
| `Domain/Proyectos/Proteccion.cs` + `CalculoProteccion.cs` | Nodo de topología para derivación 240-21(b) entre tableros encadenados — no aplica a un tablero aislado |
| `Domain/Advertencias/AvisosDeCoordinacion.cs` | Coordinación |
| `Data/`, `Exportacion/`, `Importer/`, `UI/` | Ver más abajo |

**`Data/` no se copia — se reescribe delgado, después.** Ahí viven las implementaciones EF Core de
las interfaces de `Calculo/TablasNom/`, contra SQL Server. La web necesita SU PROPIA implementación
de esas mismas interfaces, leyendo el JSON de la norma (de `NOM-001-SEDE-2012`, público) directo en
memoria, sin base de datos. **Es trabajo nuevo, pendiente — ver `M-01` en `HALLAZGOS.md`.**

**`Exportacion/` (Word, Excel) no se copia todavía.** Es multiplataforma (`net8.0` puro), así que en
teoría podría portarse sin fricción — pero no hay nada que exportar hasta que exista una pantalla.
Candidato natural para después.

**`Importer/` no se copia como proyecto** (trae referencia a `Data`, con EF Core). Lo que sí hace
falta de ahí son los archivos `DatosNom/*.json.gz` — el corpus de la norma comprimido. Pendiente de
traer cuando se escriba la implementación de `M-01`.

**`UI/` (WPF) no se copia — no aplica.** Se usa como referencia visual (qué captura cada pantalla,
en qué orden, los colores por fase) al construir la interfaz Blazor nueva, nunca como código fuente.

## Los cinco ajustes que hizo falta hacer

Copiar dos carpetas grandes y excluir otras dos no es una operación limpia — quedan roturas de
compilación en la frontera. Los cinco, en el orden en que aparecieron:

1. **`ITablaAislamiento.cs` (en `TablasNom`, sí es del alcance) dependía de `FamiliaAislamiento`
   (definido dentro de `Coordinacion/CurvaDanioConductor.cs`, que no lo es).** Se extrajo el enum a
   su propio archivo, `TablasNom/FamiliaAislamiento.cs`. Ver `HALLAZGOS.md` E-01 — vale la pena
   reportarlo también en el repo de escritorio, donde la misma mezcla de responsabilidades no rompe
   nada porque `Coordinacion` siempre está presente, pero sigue siendo la misma mezcla.
2. **`Tablero.cs` tenía una propiedad `AjustesDisparoPrincipal` de tipo `AjustesDisparoCapturados`**
   (coordinación, excluida). Se quitó la propiedad y su docstring.
3. **`Proyectos/CalculoProteccion.cs` dependía del tipo `Proteccion`**, que se había excluido por ser
   un nodo de topología encadenada. Se eliminó el archivo completo — es el resultado calculado de
   una entidad que ya no existe aquí.
4. **`Advertencias/AdvertenciasDelProyecto.cs` tenía dos ramas de `switch` que mencionaban
   `Proteccion`** (una para armar el texto del aviso, otra para el orden de despliegue). Se quitaron
   esas dos líneas; el resto del archivo —que recorre `Tablero`, `Transformador`, `CentroControlMotores`—
   quedó intacto.

No se tocó ninguna regla de cálculo. Los cinco ajustes son de **plomería entre archivos**, no de
lógica de la norma.

## Cambios posteriores a la copia

Llevar estos cambios a `PowerNode-DesignSuite`.

| Fecha | Archivo | Cambio | ¿Cambia resultados? | Hallazgo |
|---|---|---|---|---|
| 2026-09-23 | `Validaciones/CalculadoraDesbalanceo.cs` | Extraer `CorrientePorFase` de `Porcentaje`. | No | M-02 |
| 2026-09-23 | `TablasNom/ITablaProteccionEstandar.cs`, `Casos/SeleccionConductor.cs` | Agregar `ValoresDeLaNorma` y `SiguienteDeLaNorma`; verificar 240-4(b) contra la lista completa de 240-6(a). | No (por omisión) | I-29 |
| 2026-09-23 | `Casos/SeleccionConductor.cs` | Cambiar el texto de la cita 310-15(b)(16): «capacidad mínima». | No | I-33 |
| 2026-09-23 | `Casos/SeleccionConductor.cs`, `CalculadoraCircuitoDerivadoNoMotor.cs`, `CalculadoraAlimentador.cs` | Agregar `CalibrePorDosRevisiones` y `cargaAl100PctA`: 125 % contra tabla sin factores, 100 % contra ampacidad corregida. Excluir motores y derivaciones 240-21(b). | Sí, con factores | M-05 |
| 2026-09-23 | `Casos/TemperaturaTerminales.cs`, `DatosEntradaCircuitoDerivadoNoMotor.cs`, `DatosEntradaAlimentador.cs`, calculadoras | Agregar `TerminalesMarcadas75C` y `Para(protección, marcado75C, aislamiento)`. | No (por omisión) | M-06 |
| 2026-09-23 | `Casos/CalculadoraCircuitoDerivadoNoMotor.cs` | Cambiar `permiteExcepcion2404b` a `TipoCarga != Contactos`. | Sí, en Equipo | M-04 |
| 2026-09-24 | `Casos/CalculadoraCircuitoDerivadoNoMotor.cs`, `DatosEntradaCircuitoDerivadoNoMotor.cs` | Quitar el mínimo de 15 A (alumbrado) y 20 A (contactos). Agregar `ProteccionMinimaA` y `ReferenciaProteccionMinima`, vacíos por omisión. | Sí, en contactos | R-14 |
| 2026-09-24 | `Domain/Proyectos/CircuitoDerivado.cs` | Corregir la cita de `CargaLineal`: «310-15(b)(5)(3)», no «Tabla 310-15(b)(5)(3)». | No | R-13 |
