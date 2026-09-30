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
| 2026-09-24 | `Casos/CaidaPorFase.cs` (nuevo), `DatosEntradaAlimentador.cs`, `ResultadoAlimentador.cs`, `CalculadoraAlimentador.cs` | Agregar `CorrientesPorFase` y `ConNeutro`: la fase que gobierna y el factor de demanda en el motor; caída por fase con el neutro (suma fasorial). Resultado con `FaseQueGobierna`, `CaidaPorFase` y `CorrienteNeutro`. | No (por omisión); sí al capturar corrientes por fase | R-04, R-02 |
| 2026-09-24 | `Casos/DatosEntradaAlimentador.cs`, `CalculadoraAlimentador.cs` | Agregar `ProteccionMinimaA` y `ReferenciaProteccionMinima` al alimentador (230-79). | No (por omisión) | R-11 |
| 2026-09-24 | `Casos/SeleccionConductor.cs` | `DeterminarCalibreBase`: revisar 240-4(b) en cada calibre desde el de la carga, no solo en ese. | Sí, cuando la protección queda arriba de la carga | R-16 |
| 2026-09-24 | `Casos/SeleccionConductor.cs` | Agregar `caidaVoltsPorImpedancia`: la caída de un calibre la puede dar quien llama. | No (por omisión) | R-02 |
| 2026-09-24 | `Domain/Proyectos/CircuitoDerivado.cs` | Corregir la cita de `CargaLineal`: «310-15(b)(5)(3)», no «Tabla 310-15(b)(5)(3)». | No | R-13 |
| 2026-09-24 | `TablasNom/ErratasDeLaNorma.cs` | Agregar `AreaTw10Awg`: Tabla 5, TW/THHW/THW/THW-2 de 10 AWG, 55.68 → 15.68 mm². | Sí, en llenado de canalizaciones | M-07 |
| 2026-09-24 | `Normativa/TablaAislamientoJson.cs` (en escritorio: `Data/TablasNom`) | `TemperaturaMaxima`: en lugar seco sin renglón propio, la temperatura que da la tabla — 310-10(a). THW se rechazaba en seco. | Sí, THW en lugar seco | M-08 |
| 2026-09-25 | `Tableros/BalanceoDeFases.cs` | Desempatar por dispersión: a igual (máx − mín) / máx, gana el acomodo con las corrientes por barra más parejas. Con toda la carga en una barra, la búsqueda se detenía en 100 % aunque dos movimientos llegaban a 0 %. | Sí, propone más movimientos cuando antes no proponía | I-69 |
| 2026-09-26 | `Casos/DatosEntradaCircuitoDerivadoMotor.cs`, `CalculadoraCircuitoDerivadoMotor.cs` | Agregar `TerminalesMarcadas75C` y usar `TemperaturaTerminales.Para(protección, marcado75C, aislamiento)`, como M-06 en el no-motor. | No (por omisión) | I-15 |
| 2026-09-26 | `Casos/CaidaPorFase.cs` (`CorrienteDeFaseAlimentador`), `CalculadoraAlimentador.cs` | Agregar `Motores` y `FasorMotores` a cada fase: la fase que gobierna cuenta 430-24; 430-24 y 430-62(a) con los motores de esa fase; caída fasorial con sus FLC al 100 %. `CargaMotores` sin fase sigue igual. | No (por omisión); sí con motores por fase | I-15 |
| 2026-09-26 | `Casos/CalculadoraProteccionAlimentador.cs` | Techo de 430-63: la otra carga con lo que le pide 215-3 (125 % de la continua + no continua), no al 100 %. | Sí, con motores y carga continua: el techo sube | M-09 |
| 2026-09-26 | `Normativa/TablaFlcMotorJson.cs` (en escritorio: `Data/TablasNom`) | Agregar `TensionDeColumna` y `TablaDe`: la columna con la que se lee una tensión de sistema, para que la memoria la diga. | No | I-15 |
| 2026-09-27 | `Casos/DatosEntradaCircuitoDerivadoMotor.cs`, `CalculadoraCircuitoDerivadoMotor.cs` | Agregar `FlcMarcadaEnAmperesA`: un motor marcado en amperes y no en HP entra con esa corriente como FLC —la de los HP que le corresponden en la tabla, interpolando— y cita 430-6(a)(1). `Hp` lleva los HP interpolados, solo para la cita. | No (por omisión) | I-74 |
| 2026-09-29 | `Casos/SeleccionConductor.cs`, `CaidaTensionExcedidaException.cs` | El mensaje de caída inalcanzable dice «relajar el límite de caída de tensión», sin nombrar la pantalla «Configuración» del escritorio. | No | M-10 |
| 2026-09-29 | `Casos/DatosEntradaCircuitoDerivadoMotor.cs`, `CalculadoraCircuitoDerivadoMotor.cs` | Agregar `Servicio` (`ServicioNoContinuo`): con él, el conductor va al porcentaje de la Tabla 430-22(e) sobre la corriente de placa y cita 430-22(e); la protección sigue en 430-52. | No (por omisión) | I-120 |
| 2026-09-30 | `Casos/CalculadoraCircuitoDerivadoMotor.cs` (`ProteccionDeLaTabla430_52`, `ProteccionDeMotor`) | La Excepción 1 de 430-52(c)(1) se evalúa contra la lista de 240-6(a) (`ValoresDeLaNorma`), no contra la serie: máximo = techo si es normalizado, o el siguiente; de la serie, el mayor que no lo excede. La cita dice Excepción 1 solo si hubo redondeo. 250-122(d)(2) usa el mismo máximo. **Reportar al escritorio:** allá no hay serie, pero el texto de la cita dice «Excepción 1» aunque no redondee. | Sí: 35 → 32 A y 70 → 63 A en riel DIN; 16, 32 y 63 A de techo en NEMA | M-15 |
| 2026-09-30 | `Casos/DatosEntradaCircuitoDerivadoMotor.cs`, `CalculadoraCircuitoDerivadoMotor.cs` | Agregar `NoArrancaConLaTabla`: la Excepción 2(3) de 430-52(c)(1), declarada —hasta 400 % de la FLC, 300 % arriba de 100 A, el mayor estándar que no lo excede—, con su cita. | No (por omisión) | M-15 |
| 2026-09-30 | `Unidades/Enums.cs` (`LugarDeInstalacion`), `TablasNom/ITablaAislamiento.cs`, los `DatosEntrada*` y las calculadoras | `bool LugarInstalacionSeco` → `LugarDeInstalacion Lugar` (Seco, Húmedo, Mojado). **Reportar al escritorio:** «húmedo/mojado» tomaba la fila más caliente (XHHW mojado a 90 °C). | Sí, en mojado y húmedo | M-16 |
| 2026-09-30 | `Normativa/TablaAislamientoJson.cs` (en escritorio: `Data/TablasNom`) | La columna de aplicaciones con su combinación de celdas; húmedo: fila de húmedos, si no la de mojados, si no la que tenga si 310-10(b) lo nombra; mojado: fila de mojados, si no la de húmedos si 310-10(c)(2) lo nombra. Las dos listas se leen del texto de 310-10(b) y (c)(2), que ahora viajan en el JSON. | Sí (ver M-16) | M-16 |
| 2026-09-30 | `Casos/SeleccionConductor.cs` (`MenorConDatos`) | Un calibre sin R ni X en la Tabla 9 acota su caída con las del menor más cercano con datos, en vez de saltarlo como si fallara la caída. **Reportar al escritorio:** el salto subía la tierra por 250-122(b). | Sí: 900 kcmil ya no pasa a 1000 | M-17 |

**Visto en el escritorio, sin cambio aquí (I-15):** `CalculadoraCircuitoDerivadoMotor` calcula la caída
de un motor monofásico con `TensionFaseNeutroV`, también cuando el motor está entre fases (2 polos,
220 V): la caída sale ~1.7 veces mayor. La web le pasa la tensión del circuito en ese campo; en el
escritorio hay que revisar qué tensión le llega.

## Nacido en la web — llevar al escritorio

El escritorio no calcula canalizaciones. Todo esto es nuevo y está escrito con el estilo del motor
(interfaces en `TablasNom`, lectores en `Normativa`, cálculo en `Calculo`, citas en cada paso).
Decisión: [`../decisiones/canalizaciones-y-agrupamiento.md`](../decisiones/canalizaciones-y-agrupamiento.md).

| Archivo | Qué es |
|---|---|
| `Calculo/Canalizaciones/EnumsCanalizacion.cs` | `TipoCanalizacion`, `TipoTuboConduit` (11 bloques de la Tabla 4) y la columna de la Tabla 9 de cada una. |
| `Calculo/Canalizaciones/ConteoDePortadores.cs` | `ContadorDePortadores`: 310-15(b)(3)(a), (b)(5), (b)(6) y neutro compartido (210-4). |
| `Calculo/Canalizaciones/AjusteDeAgrupamiento.cs` | Si la Tabla 310-15(b)(3)(a) aplica según el tipo: niple, 376-22(b), 378-22, 366-23, 386-22, 388-22. |
| `Calculo/Canalizaciones/CalculadoraOcupacion.cs` | El tamaño: Tablas 1, 4, 5 y 8 del Capítulo 10, Notas 2 a 5; ductos al 20 %. `TamanoCalculado`: el que da el cálculo aunque el diseñador fije otro. |
| `Calculo/TablasNom/ITablasDeCanalizacion.cs` | `ITablaOcupacion`, `ITablaTuboConduit`, `ITablaDimensionesConductor`, `ITablaTemperaturaAzotea`. |
| `Normativa/TablasDeCanalizacionJson.cs` | Sus lectores. En el escritorio van en `Data/TablasNom` con `TablaGridReader`, igual que las demás. |

Los calculadores de derivado y alimentador **no cambiaron**: siguen recibiendo
`NumeroConductoresAgrupados`, `TemperaturaAmbienteC` y `MaterialCanalizacion`; lo que cambia es quién
los llena.

### El derivado del Art. 440 — I-74

El escritorio tiene `CalculadoraCarga440` (corriente base, conductor al 125 %, protección de
440-22(a)) y dimensiona el conductor en otra parte; no tiene la forma de placa con ampacidad mínima
y protección máxima (440-4(b)). Decisión: [`../decisiones/tipos-de-carga.md`](../decisiones/tipos-de-carga.md).

| Archivo | Qué es |
|---|---|
| `Calculo/Casos/DatosEntradaCircuitoDerivado440.cs` | La placa en una de dos formas: corriente de carga nominal y de selección (440-6(a)), o ampacidad mínima y protección máxima (440-4(b)); y las condiciones del tramo, como el derivado de motor. |
| `Calculo/Casos/CalculadoraCircuitoDerivado440.cs` | El derivado completo: protección con `CalculadoraCarga440` o con la máxima de placa (el mayor estándar que no la excede, `AnteriorEstandar`); terminales, aislamiento, factores, conductor, caída y tierra con el mismo camino que `CalculadoraCircuitoDerivadoMotor`; cita 240-4(g). |

### Varios motores en un circuito — I-115

El escritorio agrupa motores solo en el alimentador (430-24, `AgregadoMotores`); un derivado es de un
motor. Decisión: [`../decisiones/motores-y-equipos-en-grupo.md`](../decisiones/motores-y-equipos-en-grupo.md).

| Archivo | Qué es |
|---|---|
| `Calculo/Casos/DatosEntradaCircuitoDerivadoGrupo.cs` | `MiembroDelGrupo` (motor o motocompresor, cantidad, corriente por unidad, origen de la corriente), `MiembrosDelGrupo` (lista comparable por contenido), la entrada del derivado y `DetalleDelGrupo` (regla, mayor, porcentaje, límite, piso, 240-4(b)). |
| `Calculo/Casos/CalculadoraCircuitoDerivadoGrupo.cs` | Conductor por 430-24 (440-33/440-34); protección = mayor estándar ≤ límite de 430-53(c)(4) o 440-22(b)(1)/(2), subiendo hasta 240-4(b) solo si no lleva la corriente de operación; notas 430-53(a), (c), (c)(6), 240-4(g). |
| `Calculo/Casos/ResultadoCircuitoDerivado.cs` | Campo opcional `Grupo` (`DetalleDelGrupo`), null en todos los demás derivados. |

Con motocompresores (I-116): el mismo calculador aplica 440-33/440-34 y 440-22(b)(1)/(2); un solo
motocompresor sin nada más, 440-32 y 440-22(a). El acondicionador de aire para habitación (I-117) es
una tercera forma de `DatosEntradaCircuitoDerivado440` (`CorrienteTotalHabitacionA`): conductor al
125 %, circuito que deja la corriente en 80 % (440-62(b)) y conductor que cubre la protección, sin
240-4(b) (440-62(a)(4)); rechaza trifásico, más de 250 V o más de 40 A (440-60, 440-62(a)(2)).

### Variador y servicio no continuo — I-119, I-120

| Archivo | Qué es |
|---|---|
| `Calculo/Casos/CalculadoraCircuitoDerivadoVariador.cs` | `DatosEntradaCircuitoDerivadoVariador` y el derivado de un variador: conductor al 125 % de la corriente de entrada (430-122(a)), protección = mayor estándar que no excede la máxima del fabricante (110-3(b)), cita 430-128 para el desconectador. |
| `Calculo/TablasNom/ITablaServicioMotor.cs` | `ServicioDeMotor`, `EspecificacionDeTiempo` y la interfaz de la Tabla 430-22(e). |
| `Normativa/TablaServicioMotorJson.cs` | Su lector. La tabla entra a `tablas-nom.json` con `tools/extraer_tablas.py` (19 tablas). |

### Alimentador a otro tablero — I-125

| Archivo | Qué es |
|---|---|
| `Calculo/Casos/DatosEntradaCircuitoDerivadoNoMotor.cs` | Parámetro `Tramo` (`ClaseDeTramo?`, `null` por omisión): con `Alimentador`, el mismo cálculo del derivado no-motor —125 % de la continua más la no continua— se cita con 215-2(a)(1) y 215-3 en lugar de 210-19(a)(1) y 210-20(a). |
| `Calculo/Casos/CalculadoraCircuitoDerivadoNoMotor.cs` | Lee `Tramo` para elegir los artículos. Sin `Tramo`, idéntico al del escritorio. |

El escritorio ya tiene `ClaseDeTramo` para el alimentador del tablero; lo nuevo es usarlo en un
interruptor de este tablero que alimenta otro.

### Tabla 220-12 — M-14

| Archivo | Qué es |
|---|---|
| `Calculo/TablasNom/ITablaCargaUnitaria.cs` | La interfaz de la Tabla 220-12: VA/m² de alumbrado general por tipo de inmueble. |
| `Normativa/TablaCargaUnitariaJson.cs` | Su lector. La tabla entra a `tablas-nom.json` con `tools/extraer_tablas.py` (20 tablas); la llamada de nota («39 (b)») no se lee como número. |

### Variadores en un grupo — I-132

| Archivo | Qué es |
|---|---|
| `Calculo/Casos/DatosEntradaCircuitoDerivadoGrupo.cs` | `ClaseDeMiembro.Variador`; `MiembroDelGrupo.ProteccionMaximaA` (la del fabricante, 110-3(b)); `DetalleDelGrupo.TopeDelFabricante`. |
| `Calculo/Casos/CalculadoraCircuitoDerivadoGrupo.cs` | Un variador cuenta como motor del grupo con su corriente de entrada (430-120, 430-122(a)). Si el mayor es un variador, el límite de 430-53(c)(4) toma su protección máxima en lugar del porcentaje de la Tabla 430-52; ningún variador admite más que la suya (430-53(c)(2)), y ese tope no se sube por 240-4(b): si no lleva la corriente del grupo, error. Sin variadores, idéntico. |

El escritorio calcula un motor por circuito: no tiene grupo ni variador en grupo.

### Puesta a tierra del equipo de acometida — M-18

| Archivo | Qué es |
|---|---|
| `Calculo/TablasNom/ITablaElectrodoTierra.cs` | La interfaz de la Tabla 250-66. |
| `Normativa/TablaElectrodoTierraJson.cs` | Su lector: los intervalos de la acometida en texto («Más de 85.0 a 177»), con 0.5 % de holgura por el redondeo. La tabla entra a `tablas-nom.json` con `tools/extraer_tablas.py` (21 tablas). |
| `Calculo/Casos/PuestaTierraDeAcometida.cs` | El conductor del electrodo (250-66, 250-66(a)) y el puente de unión principal (250-28(d)(1)), por el mayor conductor de acometida o el área equivalente en paralelo. |

