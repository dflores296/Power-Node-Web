# Por verificar

Supuestos del cálculo pendientes de confirmar en campo o por David. Ordenados por riesgo.

| Supuesto | Riesgo | Estado |
|---|---|---|
| «Terminales marcadas 75 °C» aplica a todo el tablero y a ambas puntas de cada circuito. | Calibre menor al requerido en un circuito con equipo no marcado. | Declaración del proyectista — 110-14(c)(1)a.(3). |
| Cada circuito inicia con F.P. 0.9. | VA y caída de tensión distintos si no se captura el F.P. de placa. | Confirmado por David. |
| Interruptores por omisión: centro de carga (NEMA). | 16, 32 y 63 A pasan a 20, 35 y 70 A. | Confirmado por David. |
| En riel DIN, arriba de 125 A se usa el tamaño siguiente de la NOM. | Interruptor fuera de la familia riel DIN. | Aviso junto al alimentador. |
| El mínimo del principal solo avisa. | El documento imprime el principal calculado, no el mínimo. | Confirmado por David. |
| El alimentador se dimensiona con la suma aritmética de las corrientes de su fase; la caída, con la suma fasorial. | Con cargas de F.P. muy distintos en una fase, la protección queda un poco de más. | Del lado seguro — R-04, R-02. |
| Las corrientes van atrasadas según el F.P. de cada circuito, en secuencia ABC (A 0°, B −120°, C 120°). | Con cargas capacitivas o secuencia inversa, cambia qué fase cae más. | Supuesto de cálculo — R-02. |
| Las condiciones de cálculo son del tablero, no del circuito (material, canalización, temperatura, agrupamiento, aislamiento, terminales). | Calibre incorrecto en un circuito con condiciones distintas. | Igual que el Excel. |
| El uso de los contactos (cocina, lavadora, baño) lo declara el proyectista. | Circuito de vivienda en 15 A si queda en General. No se verifica que solo alimente esas salidas — 210-52(b)(2). | Decisión de David — [`../decisiones/minimo-de-proteccion-por-uso.md`](../decisiones/minimo-de-proteccion-por-uso.md). |
| Los 1500 VA de 220-52 entran al alimentador como carga no continua. | Principal y alimentador de más si el proyectista los considera continuos, de menos al revés. | 220-52 no dice continua ni no continua. |
| La excepción de vivienda popular de hasta 60 m² no se aplica sola. | 20 A y 1500 VA donde no se exigen. | Pendiente del tipo de inmueble (R-11, R-12). |
| El desbalanceo se calcula en corriente; el balanceo se imprime en VA. | Los dos valores no coinciden entre sí. | Igual que el motor y el Excel. |
| El entregable se imprime desde el navegador. | No editable antes de entregar. | Propuesta — [`../decisiones/documento-imprimible-en-vez-de-archivo.md`](../decisiones/documento-imprimible-en-vez-de-archivo.md). |
| Impresión revisada solo en PDF de Chromium. | Márgenes o cortes distintos en impresora. | Imprimir una vez en físico. |
