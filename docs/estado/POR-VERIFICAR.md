# Por verificar

Lo que se escribió sin verse corriendo, y espera que David lo confirme. Ordenado por riesgo.

| Qué | Riesgo si está mal | Estado |
|---|---|---|
| **El mínimo del principal solo avisa**, no sube el principal (decisión de David) | El cuadro impreso dice el calculado aunque se vaya a comprar el mínimo | CONFIRMADA — ver `../decisiones/interruptor-principal-criterios-del-excel.md` |
| **El alimentador se dimensiona con la fase más cargada** (M-02), entregándole al motor la carga equivalente de esa fase (3 × sus VA en un 3F-4H) | Las citas del motor (215-2, 215-3) hablan de la corriente de la fase, no de la carga total | Verificado en el caso de los tres aparatos; la memoria explica la fase que gobierna en la sección 3, y la cita del 220-40 se reescribe con la carga del tablero |
| **Cada circuito nace con F.P. 0.9** (decisión de David) | Un aparato resistivo o un motor capturado en W sale con otros VA, y su caída de tensión se mueve, si no se cambia el 0.9 | Es un valor supuesto; la columna F.P. lo dice en su tooltip |
| **El entregable se imprime desde el navegador**, no se descarga `.xlsx` ni `.docx` | Si David edita la hoja antes de entregar, hoy no puede | PROPUESTA — ver `../decisiones/documento-imprimible-en-vez-de-archivo.md` |
| **Las condiciones de cálculo son del tablero entero** (material, canalización, temperatura, agrupamiento, f.p.), no por circuito | Un circuito en condiciones distintas sale con el calibre de los demás | Es lo que hace el Excel, donde esas columnas se llenan iguales en todos los renglones |
| **El desbalanceo se mide en corriente y el balanceo se imprime en VA** | Los dos números del pie parecen no cuadrar entre sí | Deliberado: el % sale de `CalculadoraDesbalanceo` (motor), las columnas A/B/C son las del Excel |
| **El cuadro impreso se vio en pantalla y en PDF de Chromium**, no en la impresora de David | Márgenes o cortes de página distintos | Falta imprimirlo de verdad una vez |
