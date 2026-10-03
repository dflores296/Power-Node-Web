"""La letra de la interfaz: Inter, recortada a lo que la aplicación escribe.

David, 2026-10-03: la piel de Linear (docs/conocimiento/marca.md, «La piel Linear») pide Inter con
sus variantes cv01 (el 1 sin base) y ss03 (comillas y comas redondas). La tercera de Linear, zero (el
cero cruzado), no: convierte «1/0 AWG» en «1/Ø», y la aplicación usa Ø para el diámetro del
conductor. Va dentro de la aplicación, no desde Google: así no depende de otro servidor.

De la publicación oficial (https://github.com/rsms/inter/releases, v4.1, licencia OFL 1.1):

- el eje de tamaño óptico se fija en 14 (el texto de la aplicación va de 9.5 a 24 px);
- el peso queda de 300 a 700 (la piel no pasa de 590);
- solo los caracteres que hay en la aplicación: latín con acentos, griego (Δ, Ω, θ, φ…), puntuación,
  fracciones, flechas y operadores (≤, ≥, ≈, √, −). Lo que Inter no trae lo pone la letra del sistema.

Queda en ~76 KB (la fuente completa pesa 352 KB). Uso:

    pip install fonttools brotli
    python3 tools/fuente_inter.py ruta/a/Inter-4.1/InterVariable.ttf

Un archivo nuevo se publica con otro nombre (inter-4.1.woff2 → inter-4.2.woff2), no con ?v=: es la
regla de los iconos (I-07), y el CSS que lo pide sí cambia de versión en cada publicación.
"""

import io
import pathlib
import sys

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

RAIZ = pathlib.Path(__file__).resolve().parent.parent
DESTINO = RAIZ / "src/PowerNode.Web/wwwroot/fuentes/inter-4.1.woff2"

CARACTERES = ",".join([
    "U+0000-00FF",  # latín básico y latín-1: acentos, ñ, ¿¡, °, ², ×, ·, «»
    "U+0100-017F",  # latín extendido A
    "U+0370-03FF",  # griego: Δ Σ Ω θ π ρ σ φ
    "U+2000-206F",  # puntuación: – — … ‑
    "U+2070-209F",  # superíndices y subíndices
    "U+20AC",
    "U+2100-218F",  # fracciones: ⅓ ⅛
    "U+2190-21FF",  # flechas
    "U+2200-22FF",  # operadores: − √ ≈ ≤ ≥ ∠
    "U+25A0-25FF", "U+2600-26FF", "U+2700-27BF",  # los que Inter tenga; los demás, del sistema
    "U+FEFF", "U+FFFD",
])

VARIANTES = ",".join([
    "kern", "liga", "calt", "ccmp", "locl", "mark", "mkmk",
    "cv01", "ss03",                  # las de Linear, sin zero (ver arriba)
    "tnum", "pnum", "case",          # cifras tabulares en el cuadro
    "frac", "numr", "dnom", "sups", "subs", "sinf", "ordn",
])


def main() -> None:
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    fuente = instancer.instantiateVariableFont(TTFont(sys.argv[1]), {"opsz": 14, "wght": (300, 700)})
    # Guardada y vuelta a leer antes de recortar: recortada en memoria, la tabla gvar de la instancia
    # todavía apunta a glifos de la original y el recorte truena (KeyError: 'uni200B').
    intermedio = io.BytesIO()
    fuente.save(intermedio)
    intermedio.seek(0)
    fuente = TTFont(intermedio)

    opciones = subset.Options()
    opciones.flavor = "woff2"
    opciones.layout_features = VARIANTES.split(",")
    recorte = subset.Subsetter(opciones)
    recorte.populate(unicodes=subset.parse_unicodes(CARACTERES))
    recorte.subset(fuente)

    DESTINO.parent.mkdir(parents=True, exist_ok=True)
    fuente.flavor = "woff2"
    fuente.save(DESTINO)
    print(f"{DESTINO.relative_to(RAIZ)}: {DESTINO.stat().st_size / 1024:.0f} KB")


if __name__ == "__main__":
    main()
