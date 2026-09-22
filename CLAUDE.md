# Power Node Web — punto de entrada

Calculadora web de cuadros de carga según la NOM-001-SEDE-2012. Blazor WebAssembly, sin servidor,
publicada en GitHub Pages. Hermana de `dflores296/PowerNode-DesignSuite` (escritorio, SQL Server) —
**no** su reemplazo, y **no** enlazada a ella: el motor se copia, no se referencia en vivo. Ver
`docs/decisiones/motor-copiado-no-enlazado.md`.

---

## Antes de trabajar, leer en este orden

1. **[`docs/estado/TABLERO.md`](docs/estado/TABLERO.md)** — cómo vamos, con lo que falta enlazado.
   Empezar siempre aquí.
2. **[`docs/estado/HALLAZGOS.md`](docs/estado/HALLAZGOS.md)** — defectos con ID y commit de cierre.
3. **[`docs/estado/BITACORA.md`](docs/estado/BITACORA.md)** — qué se hizo cada sesión.
4. **[`docs/LEEME.md`](docs/LEEME.md)** — índice completo de todo lo demás.

## Este archivo se queda corto, a propósito

El `CLAUDE.md` de `PowerNode-DesignSuite` creció a 300+ líneas mezclando historia, decisiones y
estado — y una sesión llegó a leer "156 tests" cuando el repo ya iba en 1306, porque nadie lo podó.
Aquí la regla es la de `AbaSuite`: este archivo dice **las reglas** y **a dónde ir**, nada más. El
estado vive en `docs/estado/`, con tope de 4 archivos.

## Las reglas que no se negocian

- **Toda decisión de arquitectura lleva autor y fecha**, en `docs/decisiones/`. Una sesión de Claude
  propone (`PROPUESTA · Claude · <fecha>`); sólo David confirma (`CONFIRMADA · David · <fecha>`).
  Nunca al revés.
- **El motor (`Calculo`/`Domain`) se copia de `PowerNode-DesignSuite`, no se reescribe.** Si algo del
  cálculo se ve mal, el defecto puede estar en el original — repórtalo ahí también.
- **Sin catálogo Square D en este repo.** Es público; el catálogo tiene derechos de autor de
  Schneider Electric. Ver `docs/decisiones/sin-catalogo-square-d.md`.
- **Alcance v1: un tablero, como el Excel original.** Sin cascada, sin coordinación, sin catálogo de
  equipos. Ver `docs/decisiones/alcance-v1-un-tablero.md` antes de proponer algo que ya se descartó.
- **Ningún hallazgo se cierra sin commit.** `docs/estado/HALLAZGOS.md` lleva el hash, no una palabra.

## Antes de dar algo por terminado

```bash
dotnet build src/PowerNode.Web
dotnet test tests/PowerNode.Normativa.Tests
dotnet test tests/PowerNode.Web.Tests
```

**Sin advertencias, y las pruebas en verde.** `dotnet build src/PowerNode.Web` arrastra los cinco
proyectos; no hace falta construirlos uno por uno.

**Y si se tocó una pantalla, se abre en el navegador antes de darla por buena.** Los dos únicos bugs
de cálculo que ha tenido este repo (I-02 e I-03) compilaban y pasaban las pruebas: se vieron
corriendo. `dotnet run --project src/PowerNode.Web` y Playwright contra `http://127.0.0.1:5199`.
