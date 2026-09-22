# Bitácora

Qué se hizo cada sesión. Del 2026-09-22 en adelante (nace el repo).

---

## 2026-09-22 (segunda parte) — Primera pantalla viva, calculando de verdad

**M-01, I-01 y P-01 cerrados.** El sitio ya calcula un cuadro de carga en el navegador.

- **Las tablas de la norma, sin base de datos.** Proyecto `PowerNode.DesignSuite.Normativa`: las
  trece implementaciones copiadas de `Data/TablasNom` del escritorio, con un solo cambio mecánico
  (`IFuenteTablas` en vez de `DbContext`). **La lógica de interpretación no se tocó.** Los datos
  salen de `tools/extraer_tablas.py`, que extrae 14 tablas y una sección del repo público de la
  norma: **39 KB** en vez de ~9 MB. Detalle en `../conocimiento/tablas-de-la-norma.md`.
- **6/6 pruebas verdes**, contra los mismos valores que el escritorio ya verificó a mano contra el
  PDF del DOF — no contra lo que la implementación devuelve hoy.
- **Primera pantalla** (`Pages/CuadroDeCarga.razor`): dos bloques nones/pares como el Excel,
  captura de tipo/VA/longitud/polos, fase por la convención NEMA de pares, y la memoria con las
  citas de norma de cada paso.
- **CI y despliegue** escritos, con el `--check` que rompe el build si las tablas se despegan de la
  norma, y las tres trampas de Blazor en Pages cubiertas (`.nojekyll`, `base href`, `404.html`).

**Dos bugs reales, los dos encontrados CORRIENDO la pantalla en el navegador, no compilándola**
(I-02 y I-03 en `HALLAZGOS.md`): `NumeroFases` se pasaba del tablero en vez del circuito —un circuito
de 1 polo daba un tercio de su corriente— y faltaba el piso práctico de calibre. Después de
corregirlos, los tres circuitos de prueba dan **exactamente** los mismos números que la app de
escritorio.

**Queda pendiente y no lo puede hacer una sesión de Claude:** activar GitHub Pages en el repo
(Settings → Pages → Source: GitHub Actions). El workflow está escrito pero **nunca se ha ejecutado**.

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
