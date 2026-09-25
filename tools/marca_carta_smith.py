#!/usr/bin/env python3
"""
El logo de Power Node: una carta de Smith (David, 2026-09-25).

La carta se DIBUJA CON SUS ECUACIONES, no se calca de ninguna imagen: es geometría y queda libre de
derechos de terceros. Todas las curvas son tangentes en el punto (1, 0) del borde:

  - resistencia constante r: círculo con centro en (r/(1+r), 0) y radio 1/(1+r);
  - reactancia constante x: arco con centro en (1, 1/x) y radio 1/|x|, recortado al círculo unidad,
    que corta el borde en ((x²-1)/(x²+1), 2x/(x²+1)).

Variante B («media», la que eligió David): r = 0.5, 1, 2 y x = ±0.5, ±1, ±2, más el eje real. El punto
azul va al centro, z = 1: la adaptación perfecta — el nodo.

Uso:
    python3 tools/marca_carta_smith.py            # escribe los SVG de wwwroot/marca
    python3 tools/marca_carta_smith.py --ico      # además arma favicon.ico con los PNG de 16, 32 y 48

Los PNG salen de estos SVG renderizados con Playwright (tools/marca_png.mjs); ver
docs/conocimiento/marca.md.
"""
import struct
import sys
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent / "src" / "PowerNode.Web" / "wwwroot"
MARCA = RAIZ / "marca"

TINTA, TINTA_OSCURA, AZUL, GRIS, GRIS_OSCURO = "#101418", "#E8ECEF", "#0B6E99", "#5A6570", "#9AA6B2"
RESISTENCIAS = [0.5, 1, 2]
REACTANCIAS = [0.5, 1, 2]
C, R = 32.0, 26.0  # centro y radio de la carta en el lienzo de 64 × 64


def _p(u, v):
    return C + R * u, C - R * v


def _n(x):
    return f"{x:.3f}".rstrip("0").rstrip(".")


def _circulo_r(r):
    cx, _ = _p(r / (1 + r), 0)
    return f'<circle cx="{_n(cx)}" cy="{_n(C)}" r="{_n(R / (1 + r))}"/>'


def _arco_x(x):
    x2 = x * x
    ix, iy = _p(1, 0)
    fx, fy = _p((x2 - 1) / (x2 + 1), 2 * x / (x2 + 1))
    radio = _n(R / abs(x))
    giro = 1 if x > 0 else 0  # por dentro del círculo unidad: en SVG la y crece hacia abajo
    return f'<path d="M{_n(ix)},{_n(iy)} A{radio},{radio} 0 0 {giro} {_n(fx)},{_n(fy)}"/>'


def carta(tinta, azul, borde, fino, punto):
    """Los trazos de la carta en el lienzo de 64 × 64: finos adentro, borde grueso encima, punto al centro."""
    finos = "".join(_circulo_r(r) for r in RESISTENCIAS) + "".join(
        _arco_x(s * x) for x in REACTANCIAS for s in (1, -1))
    eje = f'<path d="M{_n(C - R)},{_n(C)} L{_n(C + R)},{_n(C)}"/>'
    return (
        f'<g {tinta} fill="none" stroke-linecap="round" stroke-linejoin="round">'
        f'<g stroke-width="{borde if fino is None else fino}">{finos}{eje}</g>'
        f'<circle cx="{_n(C)}" cy="{_n(C)}" r="{_n(R)}" stroke-width="{borde}"/></g>'
        f'<circle cx="{_n(C)}" cy="{_n(C)}" r="{punto}" fill="{azul}"/>'
    )


NOTA = ("Carta de Smith dibujada con sus ecuaciones (tools/marca_carta_smith.py): "
        "r = 0.5, 1, 2; x = ±0.5, ±1, ±2. El punto azul es el centro, z = 1: la adaptación.")

# Grosores: el normal, para 38 px o más; el de tamaños chicos, para la pestaña y el .ico.
NORMAL = dict(borde=3.5, fino=1.8, punto=4.5)
CHICO = dict(borde=5, fino=2.6, punto=6)


def svg(contenido, ancho=64, alto=64, estilo=""):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{ancho}" height="{alto}" '
            f'viewBox="0 0 {ancho} {alto}">\n  <!-- {NOTA} -->\n{estilo}  {contenido}\n</svg>\n')


def firma(tinta, gris):
    return svg(
        carta(f'stroke="{tinta}"', AZUL, **NORMAL)
        + f'\n  <text x="76" y="30" font-family="IBM Plex Sans, Segoe UI, sans-serif" font-size="21" '
          f'font-weight="600" fill="{tinta}" letter-spacing="-0.2">Power Node</text>'
          f'\n  <text x="76" y="47" font-family="IBM Plex Sans, Segoe UI, sans-serif" font-size="11" '
          f'font-weight="500" fill="{gris}" letter-spacing="1.6">DESIGN SUITE</text>',
        ancho=260)


def escribir_svg():
    archivos = {
        "powernode-icono.svg": svg(carta(f'stroke="{TINTA}"', AZUL, **NORMAL)),
        # Tema oscuro de la aplicación: la tinta clara. Un <img> no sabe del tema que se eligió en la
        # página (solo del sistema), por eso son dos archivos y la hoja de estilos muestra uno u otro.
        "powernode-icono-oscuro.svg": svg(carta(f'stroke="{TINTA_OSCURA}"', AZUL, **NORMAL)),
        "powernode-icono-16.svg": svg(carta(f'stroke="{TINTA}"', AZUL, **CHICO)),
        "powernode-icono-mono.svg": svg(carta('stroke="currentColor"', "currentColor", **NORMAL)),
        # La tinta cambia con el tema de la pestaña; el azul no, tiene contraste en los dos.
        # Con «carta» en el nombre: el navegador guarda el icono de la pestaña por URL, y con el
        # nombre anterior seguía mostrando el unifilar (I-07).
        "powernode-carta-favicon.svg": svg(
            carta('class="tinta"', AZUL, **CHICO),
            estilo=(f"  <style>.tinta {{ stroke: {TINTA}; }} @media (prefers-color-scheme: dark) "
                    f"{{ .tinta {{ stroke: {TINTA_OSCURA}; }} }}</style>\n")),
        "powernode-firma.svg": firma(TINTA, GRIS),
        "powernode-firma-oscura.svg": firma(TINTA_OSCURA, GRIS_OSCURO),
    }
    for nombre, contenido in archivos.items():
        (MARCA / nombre).write_text(contenido, encoding="utf-8")
        print("escrito", MARCA / nombre)


def escribir_ico():
    """
    El .ico con PNG adentro (16, 32, 48), como el anterior: lo aceptan todos desde Vista. Dos copias:
    marca/powernode-carta.ico, el que declara index.html, y favicon.ico en la raíz, el que el navegador
    pide solo (I-07).
    """
    pngs = [(t, (MARCA / f"powernode-carta-{t}.png").read_bytes()) for t in (16, 32, 48)]
    cabecera = struct.pack("<HHH", 0, 1, len(pngs))
    desplazamiento = 6 + 16 * len(pngs)
    entradas, datos = b"", b""
    for tamano, png in pngs:
        entradas += struct.pack("<BBBBHHII", tamano, tamano, 0, 0, 1, 32, len(png), desplazamiento + len(datos))
        datos += png
    for destino in (MARCA / "powernode-carta.ico", RAIZ / "favicon.ico"):
        destino.write_bytes(cabecera + entradas + datos)
        print("escrito", destino)


if __name__ == "__main__":
    escribir_svg()
    if "--ico" in sys.argv:
        escribir_ico()
