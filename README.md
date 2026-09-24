# Power Node Web

Calcular el cuadro de carga de un tablero según la **NOM-001-SEDE-2012**, en el navegador, sin
instalación ni servidor.

## Requisitos

Cada requisito se relaciona con su referencia normativa y con su verificación en
`tests/PowerNode.Web.Tests`.

### Tablero

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| T-1 | Distribuir hasta 42 espacios: nones a la izquierda, pares a la derecha, fase por renglón (convención NEMA). | — | `LasBarrasRotanPorPares_…` |
| T-2 | Asignar a un interruptor de 2 o 3 polos los espacios N, N+2 y N+4 del mismo lado. | — | `UnTrifasicoOcupaTresEspacios…` |
| T-3 | Calcular la tensión fase-neutro según el sistema: estrella ÷√3, 1F-3H ÷2, 1F-2H igual. | — | `LaTensionFaseNeutroNoEsSiempreEntreRaizDeTres` |

### Carga

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| C-1 | Capturar la carga en VA, W o A y convertirla a VA. | 220-14(a) | `I25_…` |
| C-2 | Capturar el factor de potencia por carga (valor inicial 0.9). | Tabla 9, nota 2 | `FP_…` |
| C-3 | Separar la carga continua (3 h o más) de la no continua. | Art. 100 | `ElMotorDistingueContinuaDeNoContinua_…` |
| C-4 | Calcular la corriente de diseño sin factor de demanda en el derivado. | 210-19(a)(1), 220-42 | `UnCircuitoDeAlumbrado…` |
| C-5 | Desglosar los aparatos de un circuito (opcional): la carga es la suma, el F.P. el combinado; «Contacto» sin carga, 180 VA. | 220-14(i), 424-3(b) | `I35_…` |

### Protección

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| P-1 | Calcular la capacidad mínima: 125 % de la continua + 100 % de la no continua. | 210-20(a), 215-3 | `ElDesgloseDeLaProteccion…` |
| P-2 | Seleccionar el primer tamaño normalizado mayor o igual a la capacidad mínima. | 240-6(a) | `Serie_…` |
| P-3 | Seleccionar la familia de interruptores: centro de carga (NEMA), riel DIN (IEC) o NOM completa. | 240-6(a) | `Serie_…` |
| P-4 | Sin mínimo por tipo de carga. Aplicar 20 A a los contactos de vivienda de cocina (aparatos pequeños), lavadora y baño. | 210-11(c) | `SinMinimoPorTipo_…`, `Vivienda_ElUsoPide20A…` |

### Conductor

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| K-1 | Seleccionar la columna de ampacidad por aislamiento y lugar de instalación. | Tabla 310-104(a), Tabla 310-15(b)(16) | `Aislamiento_…` |
| K-2 | Aplicar los factores por temperatura ambiente y por agrupamiento en la columna del aislamiento. | 310-15(b)(2)(a), 310-15(b)(3)(a) | `DosRevisiones_…` |
| K-3 | Limitar la ampacidad a la temperatura de la terminal: 60 °C hasta 100 A, 75 °C arriba de 100 A, o 75 °C con equipo marcado. | 110-14(c)(1) | `Terminales_…` |
| K-4 | Verificar el 125 % contra la ampacidad de tabla sin factores y la carga al 100 % contra la ampacidad corregida. | 210-19(a)(1), 215-2(a)(1) | `DosRevisiones_…` |
| K-5 | Proteger el conductor según su ampacidad; permitir el estándar inmediato superior salvo en circuitos de contactos. | 240-4, 240-4(b) | `Excepcion240_4b_…`, `R16_…` |
| K-6 | Limitar la protección de 14, 12 y 10 AWG de cobre a 15, 20 y 30 A. | 240-4(d) | `Serie_EnRielDinNoHay15A_…` |
| K-7 | Verificar la caída de tensión con la impedancia eficaz. | Tabla 9, 210-19(a)(1) nota 4 | `LasFormulasVienenConSusNumerosSustituidos` |
| K-8 | Seleccionar el conductor de puesta a tierra con ajuste proporcional. | 250-122, 250-122(b) | — |
| K-9 | En 2 fases + neutro de estrella, citar el neutro como portador y darle el calibre de la fase. | 310-15(b)(5)(2), 220-61(c)(1) | `R09_…` |
| K-10 | En 2F-3H 220Y/127, no aplicar la excepción de 220-61(a) (× 140 %) ni la Tabla 310-15(b)(7). | 220-61(a), 310-15(b)(7) | `R10_…` |
| K-11 | Contar los portadores de cada canalización con los circuitos que van por ella: neutro de 1 polo sí, de 2 fases + N de estrella sí, de 3 fases + N solo con carga no lineal, tierra nunca; neutro compartido. | 310-15(b)(3)(a), 310-15(b)(5), 310-15(b)(6), 210-4 | `CanalizacionesTests`, `I39_…` |
| K-12 | Aplicar el ajuste por agrupamiento según el tipo de canalización: tubo, niple, ductos, canales auxiliares, superficiales. Sumar la temperatura de azotea al sol. | 310-15(b)(3)(a)(2), 376-22(b), 378-22, 366-23, 386-22, 388-22, 310-15(b)(3)(c) | `Ajuste_…`, `DuctoMetalico_…`, `Azotea_…` |
| K-13 | Dimensionar la canalización con todos sus conductores; 20 % en ductos y canales. | Capítulo 10, Tablas 1, 4, 5 y 8, Notas 2 a 5; 376-22(a), 366-22 | `Llenado_…` |
| K-14 | Llevar neutro solo en 1 polo, o en 2 y 3 polos con carga F-N. | 310-15(b)(5) | `I41_…` |

### Alimentador

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| A-1 | Dimensionar el alimentador con la fase de mayor capacidad requerida, en el motor. | 215-2(a)(1), 215-3 | `M02_…`, `R04_…` |
| A-2 | Aplicar el factor de demanda por tipo de carga (alumbrado, contactos, equipo, motores y A/C, calefacción), a criterio del ingeniero y con justificación del Art. 220. Justificaciones filtradas por inmueble. Calefacción fija como carga continua. | 220-40, 220-50, 220-51, 430-26, 424-3(b) | `ElFactorDeDemanda…`, `R12_…`, `R17_…`, `R18_…`, `R19_…` |
| A-3 | Calcular el factor de potencia del alimentador con las cargas de la fase que gobierna. | Tabla 9, nota 2 | `FP_ElDelAlimentador…` |
| A-4 | Avisar si el principal es menor que el derivado más grande. | — | `M03_…` |
| A-5 | Si el tablero es equipo de acometida, subir el principal al mínimo del inmueble: 30 A (vivienda popular), 60 A (todos los que no son vivienda unifamiliar). | 230-79(c), 230-79(d) | `R11_…`, `R19_…` |
| A-6 | Verificar la protección contra la capacidad de la barra. | 408-36 | `ElAvisoDel408_36…` |
| A-7 | Limitar la caída de tensión del alimentador: 2 % por omisión (con el 3 % del derivado, 5 %), capturable. Avisar si los límites suman más de 5 %. | 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4 | `R01_…`, `R15_…` |
| A-11 | Calcular la caída del alimentador fase por fase, con la caída del neutro (suma fasorial); limitar con la peor fase. | Tabla 9 | `R02_…` |
| A-8 | Avisar por circuito si la caída del alimentador en su fase más la del derivado excede 5 %. | 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4 | `R01_…` |
| A-9 | Cargar al alimentador 1500 VA por circuito de aparatos pequeños y de lavadora. | 220-52(a), 220-52(b) | `Vivienda_AparatosPequenosYLavadora…` |
| A-10 | Avisar si hay un solo circuito de aparatos pequeños. | 210-11(c)(1) | `Vivienda_UnSoloCircuito…` |

### Entregable

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| E-1 | Emitir el cuadro de carga con 24 columnas y el resumen de carga. | — | Navegador |
| E-2 | Emitir la memoria de cálculo con nueve secciones y fórmulas sustituidas. | — | `LasNueveSecciones…` |
| E-3 | Mostrar el desglose de la protección y del conductor en tooltip y en la memoria, sección 4. | — | `LaSeccion4CuadraConElConductorElegido` |

## Datos

Tablas tomadas de [NOM-001-SEDE-2012](https://github.com/dflores296/NOM-001-SEDE-2012), verificadas
contra el PDF del DOF. La integración continua compara ambas copias en cada compilación.

> Los resultados no sustituyen el criterio del ingeniero responsable del proyecto.

## Ejecutar

```bash
dotnet run --project src/PowerNode.Web
```

Blazor WebAssembly sobre .NET 8. Índice de documentación: [`docs/LEEME.md`](docs/LEEME.md).
