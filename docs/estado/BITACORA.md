# Bitácora

Qué se hizo cada sesión. Del 2026-09-22 en adelante (nace el repo).

---

## 2026-09-22 — Nace el repo, motor copiado y verificado

**Contexto:** venía de una sesión larga en `PowerNode-DesignSuite` (el escritorio) intentando
resolver "no puedo ver el WPF desde el trabajo". Pasó por tres planteamientos antes de aterrizar en
éste — quedan registrados para no repetirlos:

1. Blazor **Server** + SQLite, dentro del mismo repo de escritorio. **Revertido**: nunca fue la idea
   del usuario que el escritorio fuera portable con SQLite — su idea era archivos con base propia
   estilo AutoCAD/Revit, pero con SQL Server. Ver la nota del 2026-09-22 en
   `PowerNode-DesignSuite/docs/estado/00-RETOMAR-AQUI.md`.
2. Repo aparte, Blazor **WebAssembly**, **motor completo** (incluido catálogo Square D y
   coordinación) enlazado por `git submodule`. Descartado en dos partes: el submodule ata un repo
   público a uno privado (CI no podría clonarlo sin credenciales) y el catálogo Square D no debe
   viajar a un repo público — es material con derechos de autor de Schneider Electric.
3. **Esta:** alcance recortado a lo que el Excel original ya hacía — un cuadro de carga, un tablero,
   sin cascada ni catálogo ni coordinación — con el motor **copiado** (no enlazado) desde
   `PowerNode-DesignSuite`.

**Lo que se hizo:**

- Repo `dflores296/Power-Node-Web` creado (David), público.
- `Calculo/` y `Domain/` copiados de `PowerNode-DesignSuite` commit `29f660f`, excluyendo
  `Calculo/Coordinacion/`, `Domain/Catalogo/`, y las entidades de coordinación de `Domain/Proyectos`
  (`AjustesDisparoCapturados`, `CoberturaDeCoordinacion`, `CoordinacionDeProtecciones`,
  `Proteccion`, `CalculoProteccion`) y `Domain/Advertencias/AvisosDeCoordinacion`.
- Cinco ajustes quirúrgicos para que compilara solo — el más interesante es E-01 (ver
  `HALLAZGOS.md`): `FamiliaAislamiento` estaba mezclado dentro del código de coordinación aunque lo
  usa una tabla de ampacidad común.
- **Verificado compilando**, no sólo copiado: `dotnet build` limpio, 119 archivos, 10 719 líneas,
  cero referencia a EF Core / SQL Server / Square D.
- Estructura de `docs/` armada: `estado/` (tope de 4 archivos), `decisiones/` (con autor y estado),
  `conocimiento/`, `historico/` — patrón tomado de `AbaSuite` y `msa-toolkit`.

**Queda para la próxima sesión:** implementar las tablas de la norma sin base de datos (M-01), y
empezar la primera pantalla.
