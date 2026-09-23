# Selección de conductor y protección: auditoría del 2026-09-23

> **Estado de las propuestas.** Aprobadas por David el 2026-09-23, una por una. **1 (aislamiento):
> hecha** — ver I-32 en `../estado/HALLAZGOS.md`. 2 a 6: pendientes.

## Contexto

David pidió acotar el alcance a **seleccionar conductor y protección con la Tabla 310-15(b)(16)** y
dominar ocho temas. Pidió tres cosas: (1) si el programa ya los contempla, (2) qué falta, siempre con
fundamento en la NOM (con enlace) o en el Excel, y (3) propuestas ordenadas por prioridad, **sin
saturar el cuadro con columnas nuevas**.

Todo lo de abajo se revisó leyendo el código del motor
(`src/PowerNode.DesignSuite.Calculo/Casos/CalculadoraCircuitoDerivadoNoMotor.cs`,
`SeleccionConductor.cs`, `TemperaturaTerminales.cs`, `CargaContinua100Pct.cs`) y **corriendo el
motor** con casos concretos. El texto de la NOM se tomó del corpus del repo
`dflores296/NOM-001-SEDE-2012`; los enlaces van a su guía publicada.

**Sobre el Excel:** en este repo solo está documentado el encabezado, el reparto de fases y la fila
del alimentador (`docs/conocimiento/cuadro-de-carga-excel.md`). Las columnas de cálculo (`CC` a `EK`:
factores, selección de conductor) **no se levantaron**, y el archivo vive en el repo de escritorio, al
que esta sesión no tiene acceso. Por eso no cito el Excel en estos temas. Si lo subes, lo comparo.

---

## 1. ¿El programa ya contempla cada tema?

| # | Tema | Fundamento NOM | ¿Lo contempla? | Dónde se ve hoy |
|---|---|---|---|---|
| 1 | **Corriente de diseño** | [210-19(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-19), [220-14(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/220/#220-14) | ✅ Sí. In = VA ÷ tensión del circuito, sin factor de demanda ([220-42](https://dflores296.github.io/NOM-001-SEDE-2012/art/220/#220-42)). | Columna In (A) |
| 2 | **Factor de carga continua (125 %)** | Definición de carga continua (3 h o más): [Art. 100](https://dflores296.github.io/NOM-001-SEDE-2012/glosario/). Protección: [210-20(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-20). Conductor: [210-19(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-19). Alimentador: [215-2(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/215/#215-2) y [215-3](https://dflores296.github.io/NOM-001-SEDE-2012/art/215/#215-3) | ✅ En la protección. ⚠️ En el conductor lo aplica **junto con** los factores de corrección, y la NOM dice «**antes** de la aplicación de cualquier factor». Ver hallazgo B. | Solo en la memoria, sección 3 |
| 3 | **Tabla 310-15(b)(16) por temperatura y tipo de aislamiento** | [Tabla 310-15(b)(16)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-16), [Tabla 310-104(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-104-a) | ⚠️ El motor sí sabe elegir columna por aislamiento, pero **la web lo deja fijo en THHN, lugar seco**. No se puede capturar. Ver hallazgo A. | Memoria, secciones 2 y 5 |
| 4 | **Factores de corrección** por temperatura y por agrupamiento | [310-15(b)(2)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#310-15) con [Tabla 310-15(b)(2)(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-2-a). [310-15(b)(3)(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#310-15) con [Tabla 310-15(b)(3)(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-3-a). Qué conductores se cuentan: 310-15(b)(5) neutro y (b)(6) tierra | ✅ Los dos, con las tablas de la NOM y en la columna del aislamiento. El número de agrupados se captura a mano, **sin ayuda** para contar neutro y tierra. | Memoria, sección 4 |
| 5 | **Protección de calibres pequeños** | [240-4(d)](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-4): 14 AWG → 15 A, 12 AWG → 20 A, 10 AWG → 30 A (cobre), «después de que se ha aplicado cualquier factor». Mínimo 14 AWG: [210-19(a)(4)](https://dflores296.github.io/NOM-001-SEDE-2012/art/210/#210-19) | ✅ Sí. Si la protección pasa del tope, el cable sube. | Memoria (cita 240-4(d)) |
| 6 | **Límite por temperatura de terminales** | [110-14(c)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/110/#110-14): hasta 100 A, 60 °C; más de 100 A, 75 °C; conductor de mayor temperatura, con la ampacidad de la terminal (a.(2), b.(2)); **equipo aprobado y marcado para otra temperatura** (a.(3)) | ✅ 60/75 °C por amperes y el tope a la columna de la terminal. ⚠️ **No se puede decir que el equipo está marcado 75 °C** (a.(3)). Siempre usa 60 °C hasta 100 A. Ver hallazgo C. | Memoria, sección 2 |
| 7 | **Inmediata superior (protección)** | [240-6(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-6) + 210-20(a): la protección no menor a la capacidad mínima | ✅ Sí, el primer tamaño estándar que alcanza. | Columna Protec. (A) |
| 8 | **Inmediata superior sobre la ampacidad del conductor / choque entre reglas** | [240-4](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-4): el conductor se protege según su ampacidad. 240-4(b): se permite el estándar inmediato superior si la ampacidad no es estándar, ≤ 800 A y **no** es circuito de varios contactos para cordón y clavija. Si no aplica 240-4(b), la protección no puede pasar de la ampacidad | ✅ El choque se resuelve **subiendo el conductor**, que es la salida válida: la protección no puede bajar de la capacidad mínima. ⚠️ Solo concede 240-4(b) en **Alumbrado**; en Equipo y en contacto individual sube el cable aunque la norma lo permitiría. Ver hallazgo D. | Memoria (cita 240-4 / 240-4(b)) |

**Visibilidad:** todo lo anterior solo se ve en la **memoria de cálculo**. En el cuadro de captura
se ven In, protección y calibre, pero no el porqué. Además, la memoria tiene dos textos que
confunden (hallazgo E).

---

## 2. Lo que no se contempla o se contempla mal, con evidencia

Todos los casos se corrieron en el motor: 1 polo a 127 V, cobre, PVC, 5 m, centro de carga.

**A. El aislamiento está fijo en THHN (90 °C). Puede dejar el cable corto.** El motor recibe
`TipoAislamiento` y `LugarInstalacionSeco`, pero la web nunca los pasa.

| 20 A continuos, 9 agrupados (factor 0.7) | Calibre |
|---|---|
| THHN (lo que supone hoy) | **10 AWG** |
| THW-LS | **8 AWG** |
| TW | **8 AWG** |

Si en obra se instala THW-LS, el 10 AWG que imprime el programa **no cumple**: 35 A × 0.7 = 24.5 A,
menor que los 25 A que pide la carga. Es el único hallazgo **del lado inseguro**. Fundamento:
[110-14(c)](https://dflores296.github.io/NOM-001-SEDE-2012/art/110/#110-14),
[Tabla 310-104(a)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-104-a),
[Tabla 310-15(b)(16)](https://dflores296.github.io/NOM-001-SEDE-2012/art/310/#tabla-310-15-b-16).

**B. El 125 % se aplica junto con los factores, no «antes».** 210-19(a)(1), textual: *«el tamaño
mínimo del conductor, antes de la aplicación de cualquier factor de ajuste o de corrección, deberá
tener una ampacidad permisible no menor que la carga no-continua más el 125 por ciento de la carga
continua»*. Y la primera oración: *«ampacidad no menor que la correspondiente a la carga máxima»*.
Son **dos revisiones**, y el cable tiene que pasar las dos:
1. La ampacidad de tabla, sin factores y en la columna de la terminal, debe cubrir el 125 %.
2. La ampacidad corregida debe cubrir la carga al 100 %.

Además, [240-4](https://dflores296.github.io/NOM-001-SEDE-2012/art/240/#240-4) exige que la ampacidad
corregida quede protegida por el interruptor.

El programa exige que la ampacidad **corregida** cubra el **125 %**. Con 32 A continuos de
Alumbrado y 9 agrupados:
- **Programa:** 6 AWG.
- **NOM:** 8 AWG pasa las tres revisiones. 40 A ≤ 40 A (tabla a 60 °C); 32 A ≤ 55 × 0.7 = 38.5 A;
  y 240-4(b) permite 40 A sobre 38.5 A.

**Sobredimensiona, no es inseguro.** Con 30 °C y 3 agrupados los factores valen 1 y da lo mismo. Por
eso no apareció en tus pruebas. Mismo texto en
[215-2(a)(1)](https://dflores296.github.io/NOM-001-SEDE-2012/art/215/#215-2) para el alimentador.

**C. No se puede declarar terminales marcadas a 75 °C.** 110-14(c)(1)a.(3) permite usar la columna
del aislamiento si el equipo está *«aprobado e identificado para tales conductores»*. Hoy siempre
usa 60 °C hasta 100 A. **Sobredimensiona, no es inseguro.**

**D. 240-4(b) solo se concede en Alumbrado.** 240-4(b)(1) solo excluye el circuito *«que alimenta
más de un contacto para cargas portátiles conectadas con cordón y clavija»*. Un circuito de Equipo, o
de Contactos con un solo contacto, sí califica. **Sobredimensiona, no es inseguro.**

**E. La memoria confunde, aunque el calibre sea correcto:**
- La sección 4 dice «Icm = In / (FT × FA)» pero sustituye la capacidad mínima (125 %), no In.
- Con 32 A continuos y 6 agrupados imprime «= 50 A» y luego elige un 8 AWG de 40 A. No cuadra.
- La cita de 310-15(b)(16) dice «Corriente de diseño 40 A por conductor» cuando 40 A es la
  capacidad mínima.

**F. Agrupados sin ayuda.** La Tabla 310-15(b)(3)(a), nota 1, dice que el número se ajusta según
310-15(b)(5) (el neutro de un circuito 1F+N no cuenta; en 2 fases + neutro de una estrella, sí) y
310-15(b)(6) (la tierra no cuenta). El campo no lo dice.

Fuera del alcance que marcaste, no los toco: Tabla 310-15(b)(17) al aire libre,
310-15(b)(3)(c) azoteas, y la excepción del ensamble al 100 % (existe en el modelo pero no en la
pantalla).

---

## 3. Propuestas, por prioridad

**1. Capturar el aislamiento y el lugar (hallazgo A).** Es el único que puede dejar el cable corto.
- En «Condiciones de cálculo», dos listas cortas:
  - **Aislamiento:** THHN, THHW-LS, THW-LS, THWN, THWN-2, XHHW-2, TW. Por omisión THHN, que es lo
    que calcula hoy.
  - **Lugar:** seco / húmedo / mojado.
- Se pasa a `DatosEntradaCircuitoDerivadoNoMotor.TipoAislamiento` / `LugarInstalacionSeco` y a
  `DatosEntradaAlimentador`. **No toca el motor.**
- Archivos: `src/PowerNode.Web.Modelo/DatosDelTablero.cs`, `CuadroDeCarga.cs` (las dos llamadas al
  motor), `src/PowerNode.Web/Pages/Captura.razor`, `Documento.razor` (que se imprima).
- Si el aislamiento no vale para el lugar, el motor ya lanza `AislamientoIncompatibleException`, y
  la pantalla lo muestra en el renglón.

**2. Que se vea el porqué, sin columnas nuevas (tu pedido + hallazgo E).**
- **Tooltip sobre la protección** de cada renglón:
  «In 32.00 A · continua 125 % → capacidad mínima 40.00 A (210-20(a)) → 40 A, primer estándar
  (240-6(a))».
- **Tooltip sobre el calibre de fase:**
  «THHN 90 °C · terminal 60 °C (110-14(c)(1)) · FT 1.00 · FA 0.70 · tabla 60 °C 40 A · corregida
  38.5 A · 240-4(b) permite 40 A · caída 0.6 %». Si subió, dice por qué: 240-4(d), 240-4 o caída.
- **Memoria, sección 4 corregida:** muestra las revisiones con sus números, en vez de la fórmula
  mezclada.
- **Cita de 310-15(b)(16):** que diga «capacidad mínima», no «corriente de diseño».
- Archivos: `Captura.razor`, `Memoria/MemoriaDeCalculo.cs`. Los datos ya vienen en
  `DetalleDelCalculo` y en las citas; si falta alguno (la ampacidad de tabla sin factores), se
  agrega a `DetalleDelCalculo`.

**3. Separar las dos revisiones del 125 % (hallazgo B).**
- En `SeleccionConductor.DeterminarCalibreBase` / `CalibrePorAmpacidadUtilizable`, el calibre base
  es el menor que cumple **las dos**: tabla de la columna de la terminal ≥ 125 %, y corregida ≥
  carga al 100 %.
- Luego sigue igual: 240-4 contra la corregida, 240-4(d) y caída.
- Toca el motor copiado. Queda anotado para el escritorio; dijiste que eso por ahora no importa.
- `SeleccionConductor.Seleccionar` necesita recibir también la carga al 100 % (hoy solo recibe la
  capacidad mínima y la corriente para caída). Aplica igual al alimentador (215-2(a)(1)).

**4. Temperatura de terminales marcada (hallazgo C).**
- Campo del tablero **«Terminales del equipo»**: «Según la NOM (60 °C hasta 100 A)», por omisión y
  como hoy, o «Marcadas 75 °C».
- `TemperaturaTerminales.Para` recibe esa declaración; el tope sigue siendo el aislamiento.

**5. 240-4(b) en Equipo y en contacto individual (hallazgo D).**
- `permiteExcepcion2404b` pasa a ser «no es circuito de varios contactos».
- Con el tipo de carga actual: verdadero en Alumbrado y Equipo; falso en Contactos, porque no
  sabemos cuántos contactos hay.
- Toca una línea del motor.

**6. Ayuda en «Agrupados» (hallazgo F).** Solo un tooltip con la regla de 310-15(b)(5) y (b)(6).

Cada punto se hace, se prueba y se te muestra antes de pasar al siguiente. Si prefieres otro orden
o dejar alguno fuera, lo ajusto.

---

## Verificación (por cada punto)

- **Prueba de regresión** en `tests/PowerNode.Web.Tests/CuadroDeCargaTests.cs` con los mismos casos
  de arriba:
  - A: 20 A continuos, 9 agrupados. THHN → 10 AWG; THW-LS → 8 AWG.
  - B: 32 A continuos de Alumbrado, 9 agrupados → 8 AWG.
  - C: con terminales 75 °C, la columna cambia.
  - D: Equipo de 32 A, 9 agrupados → 8 AWG.
- **Sin cambio en el caso base:** los tres aparatos a 30 °C y 3 agrupados siguen dando lo mismo, y
  las 54 pruebas actuales siguen verdes.
- `dotnet build src/PowerNode.Web` sin advertencias; `dotnet test` de los dos proyectos.
- **En el navegador:** Playwright contra `dotnet run`, con los tooltips leídos en pantalla y la
  memoria revisada.
- Se anota en `docs/estado/HALLAZGOS.md` con su hash, y la regla del motor en
  `docs/conocimiento/motor-copiado.md`.
