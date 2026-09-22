# Power Node Web

Calculadora de cuadros de carga según la **NOM-001-SEDE-2012**. Un tablero por vez, hasta 42
espacios — el mismo alcance que el Excel que dio origen al proyecto, con la ventaja de que cada
resultado cita el artículo de la norma del que sale.

Corre completo en el navegador. Sin instalar nada, sin servidor, sin base de datos con la que
hablar por red.

## Estado

**En construcción — no hay interfaz todavía.** El motor de cálculo ya compila y está verificado.
Ver [`docs/estado/TABLERO.md`](docs/estado/TABLERO.md) para el avance real.

## De dónde viene

Nace de dos proyectos hermanos:

- **[`NOM-001-SEDE-2012`](https://github.com/dflores296/NOM-001-SEDE-2012)** — la norma completa,
  estructurada y verificada artículo por artículo. Es la fuente de cada tabla que este calculador
  usa.
- **[`PowerNode-DesignSuite`](https://github.com/dflores296/PowerNode-DesignSuite)** — la versión de
  escritorio, con SQL Server, catálogo de equipos y coordinación de protecciones. Esta web reusa su
  motor de cálculo, recortado a lo que hacía el Excel original: un solo cuadro de carga.

## Desarrollo

```bash
dotnet build src/PowerNode.DesignSuite.Domain/PowerNode.DesignSuite.Domain.csproj
```

Documentación completa del proyecto en [`docs/LEEME.md`](docs/LEEME.md).
