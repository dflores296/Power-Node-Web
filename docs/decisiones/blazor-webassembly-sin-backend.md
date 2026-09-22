# Blazor WebAssembly, sin servidor

**CONFIRMADA · David · 2026-09-22**

## La decisión

La app corre **100 % en el navegador** (Blazor WebAssembly, no Blazor Server), publicada como
sitio estático en GitHub Pages. Ningún servidor que mantener encendido, ninguna base de datos con la
que hablar por red.

## Por qué

El primer intento (revertido, ver `motor-copiado-no-enlazado.md`) fue Blazor **Server**: el C# corre
en un servidor y el navegador sólo recibe HTML por WebSocket. Eso resuelve "no reescribir en
JavaScript", pero no resuelve **"cómo lo veo desde el trabajo"**: alguien tiene que mantener ese
servidor prendido y alcanzable, y la red de la empresa de David podría no permitir un túnel a una
máquina de casa (VPN/Tailscale) — no confirmado, pero suficiente para no apostarle el diseño.

Con WebAssembly no hay ese problema: es HTML/JS/WASM estático, igual que `NOM-001-SEDE-2012` y
`msa-toolkit`. Funciona detrás de cualquier firewall corporativo porque no es distinto de abrir
cualquier página web.

## El costo que esto acepta

- **Primera carga ~3.4 MB** (verificado: `Calculo`+`Domain` publicados y comprimidos). Después queda
  en caché del navegador.
- **Sin SQL Server, ni siquiera del lado servidor** — no hay servidor. Persistencia = abrir/guardar
  un archivo desde el navegador.
- Un puente mínimo de JavaScript (~10 líneas) para descargar/subir archivos y leer `localStorage` —
  Blazor WASM no evita el 100 % del JS, sólo casi todo.

## Trampa ya conocida, anotada para no repetirla

GitHub Pages usa Jekyll por default, que **ignora carpetas que empiezan con `_`** — y Blazor publica
sus archivos en `_framework/`. Sin un archivo `.nojekyll` en la raíz del sitio publicado, el
despliegue "funciona" pero la página no carga nada, sin error visible. `msa-toolkit` ya lo tiene;
aquí hay que agregarlo también.
