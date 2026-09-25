// Los PNG de la marca, renderizados de los SVG con Playwright (Chromium) — docs/conocimiento/marca.md.
//
//   node tools/marca_png.mjs
//
// Tamaños chicos (16, 32, 48 y favicon.png) desde powernode-icono-16.svg, de trazo grueso; los
// grandes (180, 192, 512) desde powernode-icono.svg. favicon.png va sobre blanco, como el anterior;
// los demás, transparentes. Después: python3 tools/marca_carta_smith.py --ico
//
// Playwright no es dependencia del repo: se usa el que tenga instalado la máquina (NODE_PATH, o la
// instalación global).
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { createRequire } from 'node:module';
import { readFileSync } from 'node:fs';

const require = createRequire(import.meta.url);
const { chromium } = require('playwright');

const aqui = path.dirname(fileURLToPath(import.meta.url));
const raiz = path.resolve(aqui, '..', 'src', 'PowerNode.Web', 'wwwroot');
const marca = path.join(raiz, 'marca');

const trabajos = [
    ['powernode-icono-16.svg', 16, path.join(marca, 'powernode-16.png'), null],
    ['powernode-icono-16.svg', 32, path.join(marca, 'powernode-32.png'), null],
    ['powernode-icono-16.svg', 48, path.join(marca, 'powernode-48.png'), null],
    ['powernode-icono-16.svg', 32, path.join(raiz, 'favicon.png'), '#ffffff'],
    ['powernode-icono.svg', 180, path.join(marca, 'powernode-180.png'), null],
    ['powernode-icono.svg', 192, path.join(marca, 'powernode-192.png'), null],
    ['powernode-icono.svg', 512, path.join(marca, 'powernode-512.png'), null],
];

const navegador = await chromium.launch();
for (const [fuente, tamano, destino, fondo] of trabajos) {
    const pagina = await navegador.newPage({ viewport: { width: tamano, height: tamano } });
    // Incrustado como dato: una página en blanco no puede cargar un file://.
    const url = 'data:image/svg+xml;base64,' + readFileSync(path.join(marca, fuente)).toString('base64');
    await pagina.setContent(
        `<html><body style="margin:0;background:${fondo ?? 'transparent'}">` +
        `<img src="${url}" width="${tamano}" height="${tamano}" style="display:block"></body></html>`);
    await pagina.waitForLoadState('load');
    await pagina.screenshot({ path: destino, omitBackground: fondo === null });
    await pagina.close();
    console.log('escrito', destino);
}
await navegador.close();
