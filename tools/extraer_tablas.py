#!/usr/bin/env python3
"""Extrae de NOM-001-SEDE-2012 sólo las tablas que el calculador necesita.

Por qué existe
--------------
El repo público `dflores296/NOM-001-SEDE-2012` publica las 245 tablas de la norma
(`data/tablas_revisadas.json`, 1.1 MB) y el corpus completo (`data/corpus.json`, 8 MB).
Mandarle eso a un navegador para calcular un circuito sería absurdo: el cálculo de un
cuadro de carga toca **18 tablas y una sección**, y nada más.

Este script produce `wwwroot/datos/tablas-nom.json` con exactamente esas, en la MISMA forma
cruda que publica el repo de origen (celdas con `t`/`rs`/`cs`). **No interpreta nada**: la
expansión de rowspan/colspan y la lectura de valores la hace C# en tiempo de ejecución, con
el mismo algoritmo que ya usa la versión de escritorio. Un solo intérprete, un solo lenguaje,
cero riesgo de que la versión Python y la de C# lean la misma tabla distinto.

Uso
---
    python3 tools/extraer_tablas.py <ruta a NOM-001-SEDE-2012/data> <destino.json>
    python3 tools/extraer_tablas.py <ruta> <destino.json> --check   # no escribe; falla si cambió

El `--check` corre en CI: si el archivo generado se despegó de la norma publicada, el build
falla. Es el mismo mecanismo que `build_cifras.py --check` en el repo de la NOM.
"""

import json
import sys
from pathlib import Path

# Las 18 tablas que toca el cálculo de un cuadro de carga, con quién las usa.
# Si agregas una aquí, hay una implementación de interfaz en Normativa/ que la pide.
TABLAS = {
    "8": "Propiedades de los conductores (área, resistencia) — ICatalogoCalibres",
    "9": "Resistencia y reactancia en CA — ITablaImpedancia",
    "310-15(b)(16)": "Ampacidad en canalización — ITablaAmpacidad",
    "310-15(b)(17)": "Ampacidad al aire libre — ITablaAmpacidad",
    "310-15(b)(2)(a)": "Corrección por temperatura ambiente — ITablaCorreccionTemperatura",
    "310-15(b)(3)(a)": "Ajuste por agrupamiento — ITablaAgrupamiento",
    "310-104(a)": "Conductores y aislamientos — ITablaAislamiento",
    "250-122": "Conductor de puesta a tierra de equipo — ITablaPuestaTierra",
    "430-247": "FLC motores de corriente continua — ITablaFlcMotor",
    "430-248": "FLC motores monofásicos — ITablaFlcMotor",
    "430-249": "FLC motores de dos fases — ITablaFlcMotor",
    "430-250": "FLC motores trifásicos — ITablaFlcMotor",
    "430-52": "Protección de circuitos de motor — ITablaProteccionMotor",
    "430-7(b)": "Letras de código de rotor bloqueado — ITablaRotorBloqueado",
    # Canalizaciones (decisión canalizaciones-y-agrupamiento, 2026-09-24).
    "1": "Porcentaje de ocupación de tubo conduit (Capítulo 10) — ITablaOcupacion",
    "4": "Dimensiones y área de tubo conduit (Capítulo 10) — ITablaTuboConduit",
    "5": "Dimensiones de conductores aislados (Capítulo 10) — ITablaDimensionesConductor",
    "310-15(b)(3)(c)": "Sumador de temperatura en azoteas al sol — ITablaTemperaturaAzotea",
}

# 240-6(a) no es una tabla: es un renglón de prosa con los valores estandarizados
# ("15, 16, 20, 25, 30..."). ITablaProteccionEstandar lo parsea del texto.
SECCIONES = {
    "240-6(a)": "Capacidades estandarizadas de protecciones — ITablaProteccionEstandar",
}


def buscar_seccion(corpus, seccion_id):
    """Busca una sección por id, descendiendo por el árbol de hijos de cada artículo."""

    def descender(nodos):
        for n in nodos:
            if n.get("id") == seccion_id:
                return n.get("text", "")
            hallado = descender(n.get("children", []))
            if hallado is not None:
                return hallado
        return None

    for articulo in corpus["articles"]:
        hallado = descender(articulo.get("sections", []))
        if hallado is not None:
            return hallado
    return None


def construir(data_dir: Path) -> dict:
    tablas_src = json.loads((data_dir / "tablas_revisadas.json").read_text(encoding="utf-8"))
    corpus = json.loads((data_dir / "corpus.json").read_text(encoding="utf-8"))

    tablas = {}
    for tid in TABLAS:
        if tid not in tablas_src:
            raise SystemExit(f"FALTA la tabla '{tid}' en tablas_revisadas.json del repo de la norma.")
        t = tablas_src[tid]
        # Sólo lo que el lector necesita. `verificada` viaja porque es la fecha en que esa
        # tabla se cotejó celda por celda contra el PDF del DOF: es la procedencia del dato,
        # y sin ella un número aquí no se distingue de uno tecleado a mano.
        tablas[tid] = {
            "verificada": t.get("verificada"),
            "header_rows": t.get("header_rows", 1),
            "rows": t["rows"],
        }

    secciones = {}
    for sid in SECCIONES:
        texto = buscar_seccion(corpus, sid)
        if not texto:
            raise SystemExit(f"FALTA la sección '{sid}' en corpus.json del repo de la norma.")
        secciones[sid] = texto

    return {
        "_leeme": (
            "GENERADO por tools/extraer_tablas.py desde dflores296/NOM-001-SEDE-2012. "
            "No editar a mano: se regenera y CI falla si se despegó."
        ),
        "tablas": tablas,
        "secciones": secciones,
    }


def main():
    if len(sys.argv) < 3:
        raise SystemExit(__doc__)

    data_dir = Path(sys.argv[1])
    destino = Path(sys.argv[2])
    check = "--check" in sys.argv

    nuevo = construir(data_dir)
    # Compacto y con orden estable: así el diff de un cambio real es legible, y --check no
    # falla por un reordenamiento de claves.
    texto = json.dumps(nuevo, ensure_ascii=False, sort_keys=True, separators=(",", ":"))

    if check:
        if not destino.exists():
            raise SystemExit(f"--check: no existe {destino}. Corre el script sin --check.")
        if destino.read_text(encoding="utf-8") != texto:
            raise SystemExit(
                f"--check: {destino} se despegó de la norma publicada.\n"
                f"Regenéralo con: python3 {sys.argv[0]} {data_dir} {destino}"
            )
        print(f"--check OK: {destino} está al día ({len(nuevo['tablas'])} tablas).")
        return

    destino.parent.mkdir(parents=True, exist_ok=True)
    destino.write_text(texto, encoding="utf-8")
    kb = len(texto.encode("utf-8")) / 1024
    print(f"Escrito {destino}: {len(nuevo['tablas'])} tablas, "
          f"{len(nuevo['secciones'])} secciones, {kb:.0f} KB.")


if __name__ == "__main__":
    main()
