# El entregable se imprime desde el navegador, no se descarga como `.xlsx` ni `.docx`

**PROPUESTA · Claude · 2026-09-22**

## La decisión que se propone

`/documento` dibuja el **cuadro de carga** y la **memoria de cálculo** como páginas con hoja de
estilos de impresión, y el botón «Imprimir / PDF» llama al diálogo del navegador. No se genera
ningún archivo.

## Por qué no un `.xlsx` como en escritorio

La versión de escritorio emite un libro de Excel con `ClosedXML` y un Word con `DocumentFormat.
OpenXml`. Las dos bibliotecas **sí** corren en `net8.0`, pero aquí no conviene, por tres razones en
orden de peso:

1. **El peso llega al navegador.** Esta app se descarga entera la primera vez (~3.4 MB hoy).
   `ClosedXML` arrastra `SixLabors.Fonts`, `ExcelNumberFormat` y `DocumentFormat.OpenXml`; sumarlos
   es duplicar la descarga para producir un archivo que, en el flujo real, **termina pegado como
   imagen en un plano de AutoCAD** — no se vuelve a editar.
2. **El PDF del navegador es lo que se quiere firmar.** La memoria se entrega impresa. Un PDF que
   sale del diálogo de impresión es exactamente eso, y sin biblioteca de por medio.
3. **Se puede verificar aquí.** Una sesión de nube puede abrir la página con Playwright, emular
   `media: print` y mirar el resultado. Un `.xlsx` generado se puede volver a leer, pero **cómo se
   ve impreso no se puede ver** sin Excel, que es justo el problema que la nota de escritorio dejó
   anotado sobre las opciones B y C.

## Lo que se pierde, dicho claro

- **No hay archivo para retocar.** Si David quiere mover una columna antes de entregar, hoy no
  puede: tendría que ser en el navegador.
- **El «pegar como imagen en AutoCAD» sigue siendo manual**, igual que con el Excel — el PDF o una
  captura de pantalla entran al plano por el mismo camino.

## Cuándo se reabre

Si David dice que necesita el `.xlsx` de vuelta —porque lo edita antes de entregar, o porque el
plano se arma desde la hoja— esto se rehace con `ClosedXML` reusando `ColumnasCuadroDeCarga` del
escritorio, que ya existe y ya está probado. El modelo (`PowerNode.Web.Modelo`) está separado de la
pantalla justamente para que el exportador sea un consumidor más, no una reescritura.
