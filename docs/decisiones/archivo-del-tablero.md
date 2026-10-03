# Guardar y abrir el tablero en un archivo

**PROPUESTA · Claude · 2026-09-25** — hallazgo I-05 (`../estado/HALLAZGOS.md`). La pidió David:
*«que te permita exportar el avance en un archivo y lea archivos de vuelta que puedas cargar con
circuitos, configuración, tablero, etc.»* y *«que tenga el nombre del tablero - Power Node como nombre
de cabecera»*. Falta que David la confirme.

## La decisión

- **1 archivo = 1 cuadro de carga**, como el Excel ([`alcance-v1-un-tablero.md`](alcance-v1-un-tablero.md)).
  Sin servidor ni base de datos ([`blazor-webassembly-sin-backend.md`](blazor-webassembly-sin-backend.md)):
  el archivo vive en el equipo de quien lo captura.
- **Varios tableros a la vez = varias pestañas.** Cada pestaña del navegador es su propia aplicación,
  con su tablero; **la pestaña lleva el nombre del tablero**: «Tablero cocina — Power Node». Sin
  nombre, la clave; sin clave, «Tablero sin nombre». No hay una lista de tableros dentro de la
  aplicación: eso sería un proyecto de varios tableros, y el alcance de v1 es uno.
- **Formato: JSON legible**, `<nombre del tablero>.powernode.json`:
  - `"formato": "power-node/cuadro-de-carga"` y `"version": 1`, para reconocerlo y poder cambiarlo;
  - la fecha de guardado, con su zona horaria, solo informativa;
  - los tipos por su nombre (`"Contactos"`, `"Watts"`), no por número; acentos tal cual.
- **Solo lo capturado, nunca los resultados.** Al abrir, el cuadro se recalcula con el motor de la
  versión que lo abre. Si una regla se corrige, el archivo viejo sale con el cálculo corregido, no con
  los números que tenía al guardarse. Son los mismos datos que David tecleó, no una foto del cálculo.
- **Solo los renglones con algo capturado.** Un espacio vacío abre vacío de todas formas.
- **Lo que falta en el archivo se queda como en un tablero nuevo.** Cada campo es opcional al leer;
  un archivo de antes de que existiera un campo abre con su valor por omisión.
- **Lo que no cabe se ajusta y se avisa**: un gabinete que no se ofrece para el sistema pasa al más
  cercano, y un circuito fuera de los espacios se omite; los dos con aviso al abrir.
- **Lo que no es de Power Node, o está dañado, o es de una versión más nueva, no se abre**, con el
  motivo. El tablero de la pestaña no cambia.

## Cómo se usa

- **Guardar** (barra superior, en la captura). En Chrome y Edge pregunta dónde, con el nombre del
  tablero ya puesto: es un «Guardar como» **cada vez, a propósito**. Si la aplicación recordara el
  archivo y lo sobrescribiera sola, abrir «Cocina», convertirla en «Baño» y guardar borraría la cocina.
  En Firefox y Safari, que no tienen ese diálogo, el archivo se descarga.
- **Abrir**: el selector de archivos del sistema. Si el tablero de la pestaña tiene cambios sin
  guardar, pregunta antes de reemplazarlo.
- **Cerrar o recargar la pestaña** con cambios sin guardar: el navegador pregunta.

## Lo que no se hizo, y por qué

- **Guardado automático en el navegador** (`localStorage`): no se hizo. Varias pestañas
  compartirían el mismo almacén y se pisarían, y lo que vive solo en un navegador se pierde al borrar
  sus datos. **La copia por pestaña (`sessionStorage`) sí se hizo** (2026-09-30, I-142, a pedido de la
  auditoría NOM del 2026-09-29): lo que no está en un archivo se copia en la pestaña y se recupera al
  recargarla, con aviso; sigue sin guardar, así que al cerrar el navegador pregunta. No reemplaza al
  archivo: se pierde al cerrar la pestaña.
- **Preguntar con `window.confirm`**: se cambió por un diálogo dentro de la página (I-142). El nativo
  detiene la página hasta que se contesta, y una herramienta que no lo ve la toma por congelada.
- **Varios tableros dentro de la aplicación**: ver arriba, alcance de v1.

## Formato 12 — M-20 (2026-10-03)

El circuito guarda `criterioProteccion` (`Conductor`, `Maximo430_52`, `Manual`; sin el campo, automático)
y `proteccionElegida` (amperes, solo con `Manual`). **Un motor de formato 11 o anterior abre en el
máximo**: la memoria de un tablero entregado no cambia al abrirlo. Un valor fijo fuera del rango abre con
el más cercano y se avisa; «manual» sin valor abre en el máximo. Ver
[`proteccion-de-motores-por-rango.md`](proteccion-de-motores-por-rango.md).

## Para cambiar el formato después

- Agregar un campo: no pide nada; los archivos viejos abren con el valor por omisión.
- **Renombrar un valor de un tipo** (por ejemplo, `CategoriaDeCarga.CalefaccionFija`) **rompe los
  archivos guardados**. Pide subir `ArchivoDelCuadro.Version` y leer el nombre viejo.

Código: `src/PowerNode.Web.Modelo/Archivo/` (el formato, sin Blazor, con sus pruebas en
`tests/PowerNode.Web.Tests/ArchivoDelCuadroTests.cs`), `Servicios/ProyectoActual.cs`,
`Layout/BarraSuperior.razor` y `wwwroot/js/archivo.js`.
