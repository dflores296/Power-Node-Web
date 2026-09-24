# Las tablas de la norma, sin base de datos

Cómo el motor de cálculo lee la NOM-001-SEDE-2012 en un navegador, donde no hay SQL Server ni
ninguna otra base con la que hablar.

## La costura que lo hizo posible

Al auditar `Data/TablasNom` del repo de escritorio salió algo que no estaba documentado y que decide
toda la arquitectura de este repo: **de las trece implementaciones de las interfaces de
`Calculo/TablasNom`, doce tocan la base de datos en un solo punto** —
`TablaGridReader.LeerFilasDatos(db, tablaId)` — y la número trece (`TablaProteccionEstandar`) en
otro, igual de acotado: el texto de la sección 240-6(a).

Todo lo demás —interpretar qué columna es cuál, qué significa una celda vacía, cómo se leen los
rangos de calibre— es lógica pura sobre un tipo intermedio, `FilaTabla`.

**Esa es la costura.** Cambiar de dónde salen las filas no le toca una línea a la lógica de lectura:

```
                     ┌──────────────────────────┐
  SQL Server  ─────► │                          │
  (escritorio)       │  FilaTabla               │ ───►  las 13 implementaciones
                     │  .Texto(col)             │       (lógica sin tocar)
  JSON 39 KB  ─────► │  .RowSpan(col)           │
  (web, aquí)        └──────────────────────────┘
```

## Qué se copió, y el único cambio

Las trece implementaciones vienen de `Data/TablasNom` del repo de escritorio. El cambio es mecánico:

| Antes (escritorio) | Ahora (web) |
|---|---|
| `TablaAmpacidadEf(DesignSuiteDbContext db, …)` | `TablaAmpacidadJson(IFuenteTablas fuente, …)` |
| `TablaGridReader.LeerFilasDatos(db, TablaId)` | `fuente.FilasDatos(TablaId)` |
| `db.Secciones…Select(s => s.Texto)` | `fuente.TextoDeSeccion(SeccionId)` |

**Ninguna regla de lectura de la norma se tocó.** Si un número sale distinto aquí que en escritorio,
es un defecto de la fuente de datos, no de la interpretación — y eso es exactamente lo que la suite
de pruebas está para atrapar.

## De dónde salen los datos

`tools/extraer_tablas.py` lee el repo público `dflores296/NOM-001-SEDE-2012` y emite
`wwwroot/datos/tablas-nom.json` con **18 tablas y una sección** (14 hasta el 2026-09-24; las del Capítulo 10, abajo): las que el cálculo de un cuadro de
carga realmente toca, y nada más. Son **39 KB** contra los ~9 MB del corpus completo.

El JSON viaja **crudo**, con las celdas tal como las publica la norma (`t`/`rs`/`cs`). La expansión
de rowspan/colspan a índices de columna se hace en C#, en `FuenteTablasJson.ResolverGrid`, con el
algoritmo copiado verbatim de `NomDataImporter.ResolverGrid` del escritorio.

> **Por qué la expansión no se hace en Python.** Sería más corto, pero habría **dos
> implementaciones del mismo algoritmo en dos lenguajes**, y el día que una cambiara, la otra leería
> la misma tabla distinto sin que nada avisara. Un solo intérprete, un solo lenguaje.

Cada tabla viaja con su campo `verificada`: la fecha en que se cotejó celda por celda contra el PDF
del DOF. **Sin esa fecha, un número aquí no se distingue de uno tecleado a mano**, y hay una prueba
que falla si alguna tabla llega sin ella.

## El `--check` que rompe el build

```bash
python3 tools/extraer_tablas.py <ruta a NOM-001-SEDE-2012/data> <destino.json> --check
```

No escribe: **falla si el JSON generado se despegó de la norma publicada**. Corre en CI y en el
despliegue, que clonan el repo de la norma en cada corrida. Si una tabla cambia allá, el build se
rompe aquí en vez de publicar números viejos en silencio. Es el mismo mecanismo que
`build_cifras.py --check` en el repo de la norma.

## Agregar una tabla

1. Agrégala a `TABLAS` en `tools/extraer_tablas.py`, con un comentario de quién la usa.
2. Regenera los dos JSON (el de `wwwroot/datos/` y el de las pruebas).
3. Si no existe su implementación, cópiala de `Data/TablasNom` del escritorio y aplica el cambio
   mecánico de la tabla de arriba.

## Las pruebas

`tests/PowerNode.Normativa.Tests` verifica contra los **mismos valores que el escritorio ya
comprobó a mano** contra el PDF del DOF — no contra lo que la implementación devuelve hoy. 12 AWG:
3.31 mm², 6.5/6.73 Ω/km, 20 A a 60 °C. 1/0: 53.49 mm². Tabla 9: R = 6.6, X = 0.177 en PVC.

> ⚠ **La NOM no es el NEC.** La lista de 240-6(a) incluye los valores IEC —**16, 32, 63**— que el
> NEC no tiene. La prueba del inmediato superior se escribió primero esperando 20 A para 15.1 A, por
> costumbre, y falló: son **16 A**. Queda fijado a propósito.

## Las tablas del Capítulo 10 (2026-09-24)

Para canalizaciones se agregaron la Tabla 1 (ocupación), la 4 (tubo conduit), la 5 (conductores
aislados) y la 310-15(b)(3)(c) (azoteas). La 8 ya estaba. Lo que hay que saber al leerlas:

- **Tabla 4:** el primer bloque (EMT) no trae renglón de título en los datos: su título cae en los
  renglones de encabezado de la tabla y se reconoce por posición. El DOF lo rotula «Tubo conduit no
  metálico (EMT)»; el Art. 358 es «metálico ligero». Hay **dos** bloques «Cédula 80»: el segundo da
  56.40 mm interiores en 53 (2), más que la cédula 40, y no se ofrece. «––» = ese tamaño no existe.
- **Tabla 5:** se lee por la designación AWG/kcmil, nunca por los mm²: en el bloque THHN la columna
  de mm² está corrida. **Errata:** TW/THHW/THW/THW-2 de 10 AWG publica 55.68 mm² con 4.470 mm de
  diámetro; se calcula con 15.68 (`ErratasDeLaNorma.AreaTw10Awg`). Se comparó área contra diámetro en
  toda la Tabla 5 y la Tabla 4: es la única.
- **THW-LS y THHW-LS no están en la Tabla 5.** Se captura el diámetro del fabricante (Nota 5).
- **Oráculo:** el Apéndice C (Tabla C-1, EMT) da cuántos conductores iguales caben; el llenado
  calculado con las Tablas 1, 4 y 5 coincide.

