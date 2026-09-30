# Requisitos

**Referencia viva.** Si algo aquí contradice al código o a las pruebas, es un defecto de este
documento.

Cada requisito se relaciona con su referencia normativa y con su verificación en
`tests/PowerNode.Web.Tests`.

## Tablero

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| T-1 | Distribuir hasta 42 espacios: nones a la izquierda, pares a la derecha, fase por renglón (convención NEMA). | — | `LasBarrasRotanPorPares_…` |
| T-2 | Asignar a un interruptor de 2 o 3 polos los espacios N, N+2 y N+4 del mismo lado. | — | `UnTrifasicoOcupaTresEspacios…` |
| T-3 | Calcular la tensión fase-neutro según el sistema: estrella ÷√3, 1F-3H ÷2, 1F-2H igual. | — | `LaTensionFaseNeutroNoEsSiempreEntreRaizDeTres` |
| T-4 | En 3F-3H (delta, sin neutro), no calcular un circuito de 1 polo con carga; no mostrar tensión F-N. | Art. 100, 200 | `P2_1_…` |

## Carga

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| C-1 | Capturar la carga en VA, W o A y convertirla a VA. | 220-14(a) | `I25_…` |
| C-2 | Capturar el factor de potencia por carga (valor inicial 0.9). | Tabla 9, nota 2 | `FP_…` |
| C-3 | Separar la carga continua (3 h o más) de la no continua. | Art. 100 | `ElMotorDistingueContinuaDeNoContinua_…` |
| C-4 | Calcular la corriente de diseño sin factor de demanda en el derivado. | 210-19(a)(1), 220-42 | `UnCircuitoDeAlumbrado…` |
| C-5 | Desglosar los aparatos de un circuito (opcional): la carga es la suma, el F.P. el combinado; «Contacto» sin carga, 180 VA. | 220-14(i), 424-3(b) | `I35_…` |
| C-6 | Capturar un motor (tipo Motor) por sus HP. Tomar la FLC de la tabla por los polos (1 y 2: monofásico; 3: trifásico) y la tensión del circuito; ofrecer solo los HP que trae la tabla, con su FLC. Cargar al cuadro FLC × tensión (× √3). | 430-6(a), Tablas 430-248 y 430-250 | `I15_…` |
| C-7 | Capturar un motor marcado en amperes y no en HP: tomar los HP de la tabla que le corresponden, interpolando (desde 0 HP y 0 A por debajo del más chico); su FLC es esa corriente. Rechazar la que pasa del más grande. | 430-6(a)(1) | `I74_…` |
| C-8 | Separar Motor (Art. 430) y A/C y refrigeración (Art. 440) en seis tipos de carga; el tipo decide el artículo, no la unidad. Un motor o un equipo de A/C no se desglosa. | 220-50, 440-3(a), 422-3 | `I74_…` |
| C-10 | Cada carga lleva su tipo y subtipo; el F.D. del alimentador se aplica por el tipo de cada carga, y la clase del circuito sale de sus cargas (individual, uso general, para aparatos, alimentador). | Art. 100, 220 Parte C | `I123_…` |
| C-11 | Llevar cada carga al mínimo de su subtipo: 180 VA por contacto, 90 VA por contacto de uno múltiple, 600 VA por portalámparas pesado, 180 VA por tramo de ensamble, 1200 VA por circuito de anuncios, 5000 VA por secadora en vivienda; calentador de agua y calefacción, continuos. | 220-14(e)(f)(h)(i), 220-54, 422-13, 424-3(b) | `I123_CadaSubtipoLlevaSuMinimo` |
| C-12 | Capturar otro tablero como alimentador: su carga calculada, sin F.D., con 215-2(a)(1) y 215-3. | Art. 215, 220-40 | `I125_…` |
| C-13 | Revisar las reglas de la clase: 210-21(b)(1), Tabla 210-21(b)(3), 210-23, 422-11(e); 240-4(b)(1) por lo que alimenta; 40 A para la estufa doméstica de 8.75 kW o más. | 210-19(a)(3), 210-21(b), 210-23, 240-4(b)(1), 422-11(e) | `I124_…` |
| C-14 | No bajar el alimentador del mínimo de alumbrado general por superficie; en vivienda, con los contactos de uso general dentro; en bancos y oficinas, contactos de 11 VA/m². | 220-12, Tabla 220-12, 220-14(j)(k) | `M14_…` |
| C-15 | Anotar que requieren ICFT los contactos de baño (210-8(a)(1) en vivienda, (b)(1) fuera) y los de la cubierta del mueble de cocina en vivienda ((a)(6)); en vivienda popular, con su excepción. 210-12 (ICFA) es «se podrán»: no se anota. | 210-8 | `P3_4_…` |
| C-16 | Contactos de 1 polo a 277 V: nota de 210-6(c)(6); en vivienda, aviso de 210-6(a)(2). | 210-6 | `R6_…` |
| C-9 | Capturar un equipo de A/C por su placa: ampacidad mínima y protección máxima (MCA, MOCP), o la corriente de carga nominal y la de selección del circuito, la mayor. | 440-4(b), 440-6(a) y su Excepción 1 | `I74_…` |

## Protección

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| P-1 | Calcular la capacidad mínima: 125 % de la continua + 100 % de la no continua. | 210-20(a), 215-3 | `ElDesgloseDeLaProteccion…` |
| P-2 | Seleccionar el primer tamaño normalizado mayor o igual a la capacidad mínima. | 240-6(a) | `Serie_…` |
| P-3 | Seleccionar la familia de interruptores: centro de carga (NEMA), riel DIN (IEC) o NOM completa. | 240-6(a) | `Serie_…` |
| P-4 | Sin mínimo por tipo de carga. Aplicar 20 A a los contactos de vivienda de cocina (aparatos pequeños), lavadora y baño; no fuera de vivienda ni en vivienda popular de hasta 60 m². El circuito individual del refrigerador, sin mínimo de 20 A, con su cita. | 210-11(c) y su Excepción 1, 210-52(b)(1) Excepción 2 | `SinMinimoPorTipo_…`, `Vivienda_ElUsoPide20A…`, `I46_…`, `I76_…` |
| P-5 | Proteger el derivado de un motor con el porcentaje de la Tabla 430-52 para interruptor de tiempo inverso (250 %). Máximo: el techo si es valor de 240-6(a); si no, el siguiente de 240-6(a) (Excepción 1, evaluada contra la lista de la NOM y citada solo si hubo redondeo). De la serie, el mayor que no excede el máximo. Con la casilla del proyectista, hasta 400 % (300 % arriba de 100 A) — Excepción 2(3), nunca sola. | 430-52(c)(1) y sus Excepciones 1 y 2, 240-6(a) | `I15_…`, `I74_…`, `P1_1_…` |
| P-6 | Proteger el derivado de un equipo de A/C con el mayor tamaño estándar que no pase de 175 % de su corriente —225 % si se declara que no arranca—, sin redondear hacia arriba, y no menos de 15 A; o con la protección máxima de placa (el mayor estándar que no la excede). | 440-22(a) y su Excepción, 440-4(b) | `I74_…` |

## Conductor

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| K-1 | Seleccionar la columna de ampacidad por aislamiento y lugar de instalación (seco, húmedo o mojado): la temperatura, de la fila de la Tabla 310-104(a) para ese lugar; el permiso, de 310-10(b) y 310-10(c)(2). | Tabla 310-104(a), 310-10, Tabla 310-15(b)(16) | `Aislamiento_…`, `Tabla310_104a_SecoHumedoYMojado`, `P1_2_…` |
| K-2 | Aplicar los factores por temperatura ambiente y por agrupamiento en la columna del aislamiento. | 310-15(b)(2)(a), 310-15(b)(3)(a) | `DosRevisiones_…` |
| K-3 | Limitar la ampacidad a la temperatura de la terminal: 60 °C hasta 100 A, 75 °C arriba de 100 A, o 75 °C con equipo marcado. | 110-14(c)(1) | `Terminales_…` |
| K-4 | Verificar el 125 % contra la ampacidad de tabla sin factores y la carga al 100 % contra la ampacidad corregida. | 210-19(a)(1), 215-2(a)(1) | `DosRevisiones_…` |
| K-5 | Proteger el conductor según su ampacidad; permitir el estándar inmediato superior salvo en circuitos de contactos. | 240-4, 240-4(b) | `Excepcion240_4b_…`, `R16_…` |
| K-6 | Limitar la protección de 14, 12 y 10 AWG de cobre a 15, 20 y 30 A. | 240-4(d) | `Serie_EnRielDinNoHay15A_…` |
| K-7 | Verificar la caída de tensión con la impedancia eficaz. Un calibre sin R ni X en la Tabla 9 (700, 800, 900 kcmil): acotar la caída con las del menor más cercano con datos; no atribuir a la caída un aumento que viene del hueco. | Tabla 9, 210-19(a)(1) nota 4 | `LasFormulasVienenConSusNumerosSustituidos`, `P2_2_…` |
| K-8 | Seleccionar el conductor de puesta a tierra con ajuste proporcional. | 250-122, 250-122(b) | — |
| K-9 | En 2 fases + neutro de estrella, citar el neutro como portador y darle el calibre de la fase. | 310-15(b)(5)(2), 220-61(c)(1) | `R09_…` |
| K-10 | En 2F-3H 220Y/127, no aplicar la excepción de 220-61(a) (× 140 %) ni la Tabla 310-15(b)(7). | 220-61(a), 310-15(b)(7) | `R10_…` |
| K-11 | Contar los portadores de cada canalización con los circuitos que van por ella: neutro de 1 polo sí, de 2 fases + N de estrella sí, de 3 fases + N solo con carga no lineal, tierra nunca; neutro compartido. | 310-15(b)(3)(a), 310-15(b)(5), 310-15(b)(6), 210-4 | `CanalizacionesTests`, `I39_…` |
| K-12 | Aplicar el ajuste por agrupamiento según el tipo de canalización: tubo, niple, ductos, canales auxiliares, superficiales. Sumar la temperatura de azotea al sol. | 310-15(b)(3)(a)(2), 376-22(b), 378-22, 366-23, 386-22, 388-22, 310-15(b)(3)(c) | `Ajuste_…`, `DuctoMetalico_…`, `Azotea_…` |
| K-13 | Dimensionar la canalización con todos sus conductores; 20 % en ductos y canales. | Capítulo 10, Tablas 1, 4, 5 y 8, Notas 2 a 5; 376-22(a), 366-22 | `Llenado_…` |
| K-14 | Llevar neutro solo en 1 polo, o en 2 y 3 polos con carga F-N. | 310-15(b)(5) | `I41_…` |
| K-15 | Dimensionar el conductor del derivado de un motor al 125 % de la FLC de tabla; la protección del motor no lo sube. Caída a la tensión del circuito (F-F en 2 polos). | 430-22, 240-4(g) | `I15_…` |
| K-16 | Dimensionar el conductor del derivado de un equipo de A/C al 125 % de su corriente, o a la ampacidad mínima de placa sin otro 125 %; la protección no lo sube. | 440-32, 440-4(b), 240-4(g) | `I74_…` |
| K-17 | Varios motores, o motores y otras cargas, en un circuito: conductor al 125 % de la máquina mayor + las demás + 125 % de la continua + la no continua; protección = mayor tamaño estándar que no excede el % de la Tabla 430-52 del motor mayor + la FLC de los demás + las otras cargas; hasta 240-4(b) solo si no lleva la corriente de operación y el límite queda bajo la ampacidad del conductor. Un grupo de un solo motor se calcula como motor. | 430-24, 430-53(c)(4), 240-4(b), 240-4(g) | `I115_…` |
| K-18 | Un aparato con motor fijo en su sitio (HP, o A de placa) junto con otras cargas: el motor mayor de más de ⅛ hp al 125 %, lo demás al 100 %. Un circuito de carga que solo alimenta motores se rechaza: va por el Art. 430. | 220-18(a), 430-6(a)(1) Exc. 3 | `I118_…` |
| K-19 | Varios motocompresores, o motocompresor y otros motores o cargas, sin MCA de conjunto: conductor por 440-33/440-34; protección por 440-22(b)(1) si el motocompresor es la carga más grande, si no por 440-22(b)(2); un solo motocompresor, 440-32 y 440-22(a). | 440-33, 440-34, 440-22(b) | `I116_…` |
| K-20 | Acondicionador de habitación con cordón y clavija en su circuito: conductor al 125 %, circuito que deja su corriente en 80 %, protección que no pasa la ampacidad del conductor; rechazar trifásico, > 250 V o > 40 A. En un circuito con otras cargas, avisar si pasa del 50 %; solo, del 80 %. | 440-60, 440-62 | `I117_…` |
| K-21 | Motor con variador: conductor al 125 % de la corriente de entrada del variador; protección = mayor tamaño estándar que no excede la máxima del fabricante. | 430-122(a), 110-3(b) | `I119_…` |
| K-22 | Motor de servicio no continuo: conductor al porcentaje de la Tabla 430-22(e) sobre la corriente de placa; protección por 430-52 con la FLC de tabla. | 430-22(e), Tabla 430-22(e) | `I120_…` |

## Alimentador

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| A-1 | Dimensionar el alimentador con la fase de mayor capacidad requerida, en el motor. | 215-2(a)(1), 215-3 | `M02_…`, `R04_…` |
| A-2 | Aplicar el factor de demanda por tipo de carga (alumbrado, contactos, equipo, motores, A/C y refrigeración, calefacción), a criterio del ingeniero y con justificación del Art. 220. Justificaciones filtradas por inmueble. Calefacción fija como carga continua. | 220-40, 220-50, 220-51, 430-26, 424-3(b) | `ElFactorDeDemanda…`, `R12_…`, `R17_…`, `R18_…`, `R19_…` |
| A-3 | Calcular el factor de potencia del alimentador con las cargas de la fase que gobierna. | Tabla 9, nota 2 | `FP_ElDelAlimentador…` |
| A-4 | Avisar si el principal es menor que el derivado más grande. Si ese derivado es de un motor o de un equipo de A/C, decir hasta cuánto permite subir el principal 430-62(a). | 430-62(a) | `M03_…`, `I15_…`, `I74_…` |
| A-5 | Si el tablero es equipo de acometida, subir el principal al mínimo del inmueble: 30 A (vivienda popular), 60 A (todos los que no son vivienda unifamiliar). | 230-79(c), 230-79(d) | `R11_…`, `R19_…` |
| A-6 | Verificar la protección contra la capacidad de la barra. | 408-36 | `ElAvisoDel408_36…` |
| A-7 | Limitar la caída de tensión del alimentador: 2 % por omisión (con el 3 % del derivado, 5 %), capturable. Avisar si los límites suman más de 5 %. | 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4 | `R01_…`, `R15_…` |
| A-11 | Calcular la caída del alimentador fase por fase, con la caída del neutro (suma fasorial); limitar con la peor fase. | Tabla 9 | `R02_…` |
| A-8 | Avisar por circuito si la caída del alimentador en su fase más la del derivado excede 5 %. | 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4 | `R01_…` |
| A-9 | Cargar al alimentador 1500 VA por circuito de aparatos pequeños y de lavadora, solo en vivienda de más de 60 m²; no al circuito individual del refrigerador. | 220-52(a) y su Excepción, 220-52(b) y su excepción | `Vivienda_AparatosPequenosYLavadora…`, `I46_…`, `I76_…` |
| A-10 | Avisar si hay un solo circuito de aparatos pequeños. | 210-11(c)(1) | `Vivienda_UnSoloCircuito…` |
| A-12 | Sumar los motores y los equipos de A/C por fase, en un solo grupo —los de un circuito con varios motores, cada uno por separado—: 125 % de la corriente del mayor + 100 % de los demás, con el F.D. de su tipo; su corriente en la caída fasorial. Máximo de la protección: la mayor de motor o A/C + la corriente de los demás + lo que 215-3 pide para la otra carga; avisar si se excede. En el resumen, «Al alimentador»: continua, no continua, y motores y A/C, que suman el total. | 430-24, 440-33, 440-7, 430-26, 430-62(a), 430-63 | `I15_…`, `M09_…`, `I74_…` |
| A-13 | Un motor de servicio no continuo entra al grupo de motores con el valor de 430-22(e), sin el 125 % del mayor. | 430-24 Excepción 1 | `I120_…` |
| A-15 | Si el tablero es equipo de acometida, dar el conductor del electrodo de puesta a tierra (Tabla 250-66, con el área equivalente de los conductores en paralelo) y el puente de unión principal (la misma tabla, o 12.5 % del área arriba de 1100 kcmil de cobre). | 250-66, 250-66(a), 250-28(d)(1) | `Tabla250_66_…`, `P3_3_…` |
| A-14 | De un par de circuitos que no funcionan a la vez, al alimentador va el mayor (su carga con F.D.); avisar cuál se omite, y si el par no tiene carga. | 220-60, 430-24 Excepción 3, 440-33 Excepción 1 | `I121_…` |

## Entregable

| ID | Requisito | Referencia | Verificación |
|---|---|---|---|
| E-1 | Emitir el cuadro de carga con 24 columnas y el resumen de carga. | — | Navegador |
| E-2 | Emitir la memoria de cálculo con nueve secciones y fórmulas sustituidas. | — | `LasNueveSecciones…` |
| E-3 | Mostrar el desglose de la protección y del conductor en tooltip y en la memoria, sección 4. | — | `LaSeccion4CuadraConElConductorElegido` |
| E-4 | Guardar el tablero en un archivo (`.powernode.json`) y abrirlo de vuelta: lo capturado, sin resultados; se recalcula al abrir. Rechazar lo que no es de Power Node o es de una versión más nueva. Formato 2: un «Motor / A/C» del formato 1 abre como Motor si trae HP, o como A/C y refrigeración con aviso. Formato 3: la clase de cada aparato (carga o motor) y el Motor «Varios»; la versión se lee antes que lo demás. Formato 4: el A/C «Varios» y «Hab.», el motocompresor y el A/C de cuarto en el desglose. Formato 11: el lugar seco, húmedo o mojado («húmedo o mojado» abre como mojado, con aviso) y la Excepción 2 del motor. | — | `I05_…`, `I74_…`, `I115_…`, `I116_…` |
| E-8 | Preguntar dentro de la página, no con un diálogo nativo que la detiene, antes de borrar o recortar circuitos al cambiar Espacios, Fases o Hilos, y al abrir un archivo sobre cambios sin guardar. Copiar en la pestaña (`sessionStorage`) lo que no está en un archivo y recuperarlo al recargar. | — | `P1_3_…`, navegador |
| E-9 | Rechazar un número fuera de su rango (longitud > 0, cargas ≥ 0, F.P. 0.1–1, F.D. 0.01–1…) y regresar la casilla al valor con que se calcula, con aviso, aunque el valor llegue sin foco. | — | Navegador |
| E-10 | Enseñar en el renglón del circuito, con el texto al pasar el cursor, lo que hay que revisar (reglas de su clase, caída combinada, 440-62). | — | `R1_…`, navegador |
| E-6 | Decir en la memoria de un motor, un grupo, un variador o un A/C el medio de desconexión mínimo. | 430-110, 430-128, 440-12, 440-63 | `I122_…` |
| E-5 | Nombrar la pestaña con el tablero: «Tablero cocina — Power Node». | — | Navegador |
| E-7 | Publicar la guía de cargas: cada tipo y subtipo de carga y cada clase de circuito con sus citas (la carga, el circuito derivado, el alimentador) y el glosario de la pantalla contra la norma, de la misma fuente que el selector de tipo. | Art. 100, Art. 220 Partes B y C | `GuiaDeCargasTests` |
