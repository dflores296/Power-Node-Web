# Capacidad interruptiva contra la corriente de falla disponible (110-9, 110-10)

**PROPUESTA · Claude · 2026-09-30** — auditoría NOM del 2026-09-29, P3-2 (I-151). No implementada: pide
capturas nuevas y roza [`alcance-v1-un-tablero.md`](alcance-v1-un-tablero.md) (sin coordinación de
protecciones). Falta que David decida.

## El problema

No hay dónde capturar la corriente de cortocircuito disponible ni la capacidad interruptiva de los
interruptores. 110-9 pide que el equipo que interrumpe corriente de falla tenga capacidad suficiente
para la corriente disponible en sus terminales; 110-10, que las protecciones, la impedancia total y las
corrientes de cortocircuito permitan que la protección despeje una falla sin daño.

## Por qué cabe en un tablero

No es coordinación (qué interruptor dispara primero): es una comparación por aparato, falla disponible
contra capacidad interruptiva, en el principal y en los derivados. El motor copiado ya trae
`AnalisisCortocircuito.Verificar` (veredicto Suficiente / Insuficiente / No evaluable, sin inventar
datos) y la Tabla 9 para la impedancia del alimentador.

## La propuesta

1. **Capturas nuevas, opcionales**, en la tarjeta del alimentador:
   - corriente de cortocircuito disponible en el origen del alimentador (kA), la que da CFE o el estudio;
   - capacidad interruptiva del principal (kA) y una para los derivados (kA), con la opción de
     cambiarla en un circuito.
2. **La falla en las barras del tablero**, por el método punto a punto: la del origen atenuada por la
   impedancia del alimentador (Tabla 9, su longitud y sus conductores en paralelo). Sin la falla
   disponible no se calcula nada: «No evaluable», como en el escritorio.
3. **Veredicto por aparato**: el principal contra la falla del origen; cada derivado contra la de las
   barras. Insuficiente es un aviso rojo en el renglón, la tarjeta de avisos y la memoria (nueva
   sección del alimentador).
4. **Sin clasificaciones en serie** (240-86): pedirían datos de catálogo, que este repo no lleva
   ([`sin-catalogo-square-d.md`](sin-catalogo-square-d.md)).

## Lo que se pierde

- Cinco campos más; sin llenarlos todo sigue igual.
- El punto a punto supone fuente de impedancia despreciable arriba del origen: da una falla igual o
  mayor que la real (del lado seguro).

## Preguntas para David

1. ¿Entra a v1, o se queda en el escritorio?
2. ¿Una capacidad para todos los derivados, con excepción por circuito, o una por circuito?
