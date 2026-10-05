# La descarga en Brotli

**CONFIRMADA · David · 2026-10-05** («P-3: 1 (medir con «Slow 4G» antes y después; descartar
InvariantGlobalization por los formatos de número)»). Propuesta por Claude el 2026-09-30 en I-152 (riesgo 7 de la
auditoría NOM del 2026-09-29) y en el documento de decisiones del 2026-10-05. Afecta AM-9 / I-187 (la carga
inicial de 40 s a 3 min por pestaña de la auditoría de motores). Commit: el de esta decisión.

## El problema

La publicación deja en `_framework` cada recurso en tres formas: tal cual, `.gz` y `.br`. GitHub Pages sirve
archivos estáticos y no negocia la compresión con esos archivos: el navegador baja los de tal cual. Con la red
lenta de la auditoría, la primera carga de cada pestaña tardó de 40 s a 3 min.

## Lo decidido

- **Opción 1, sí:** pedir los `.br` y descomprimirlos en el navegador con el decodificador de Brotli de Google, el
  camino que documenta Microsoft para Blazor WebAssembly en un hospedaje sin compresión.
- **InvariantGlobalization, no:** cambia la cultura de toda la app y con ella los formatos de número.

## Cómo quedó

- `wwwroot/js/brotli.js`: `decode.min.js` de [google/brotli](https://github.com/google/brotli), commit
  `42a2ed4`, licencia MIT (va en su cabecera). Un solo cambio, para cargarlo como script clásico y que la
  publicación le ponga su `?v=` como a los demás: `export let BrotliDecode=` → `window.BrotliDecode=`.
- `index.html`: `blazor.webassembly.js` con `autostart="false"` y `Blazor.start({ loadBootResource })`.
  - Solo los binarios (`assembly`, `pdb`, `dotnetwasm`, `globalization`): .NET 8 solo acepta una URL para sus
    módulos de JS y para `blazor.boot.json`.
  - .NET busca primero en su caché (Cache Storage, solo en https) y lo que recibe lo guarda ahí con el hash del
    recurso. Por eso lo descomprimido se compara con ese hash (SHA-256) antes de entregarlo: un `.br` viejo o
    roto se quedaría guardado.
  - Si el `.br` falta, no se descomprime o no coincide con su hash, se pide el recurso normal, con su integridad,
    como lo haría .NET. La página nunca deja de arrancar por esto.
  - En `localhost` y `127.0.0.1`, como siempre: `dotnet run` no genera los `.br`.
- El aviso de avance de la pantalla de carga (I-59) sigue igual: .NET publica el avance por recurso, venga de
  donde venga.

## Medido

La publicación en Release de esta rama, antes (`93951eb`) y después, servida por HTTPS desde un servidor estático
sin compresión (en `127.0.0.2`: https, como Pages, para que .NET use su caché; y no es «localhost»). Chromium con
el perfil «Slow 4G» de DevTools (562.5 ms de latencia, 1.44 Mbps de bajada, 675 kbps de subida), perfil nuevo
en cada corrida. «Frío»: la primera visita; «recarga»: la segunda.

| | Antes | Después |
|---|---|---|
| Primera carga, Slow 4G | **54.2 s**, 8.35 MB | **24.7 s**, 3.30 MB |
| Recarga, Slow 4G | 3.8 s, 0.09 MB | 3.8 s, 0.10 MB |
| Primera carga, sin límite (2 corridas) | 2.8 y 2.6 s | 2.8 y 3.0 s |
| Recarga, sin límite | 1.6 s | 1.6 y 1.8 s |

- Los 3.30 MB incluyen `brotli.js` sin comprimir (150 KB; Pages lo manda en gzip, ≈ 67 KB).
- Descomprimir en el navegador no se nota con la red sin límite: la diferencia está dentro de lo que varía
  de una corrida a otra.
- La recarga no cambia: .NET la sirve de su caché, sin llamar a `loadBootResource`.
- El respaldo, probado: sin el `.br` de `dotnet.native.wasm` (404) y con el de `PowerNode.Web.wasm` cambiado por
  otro (hash que no coincide), la consola lo dice y la página arranca y calcula igual.
- El avance de la pantalla de carga sale igual: de 2 % a 100 %, en 38 a 40 pasos.

## Lo que no se pudo verificar desde aquí

El entorno de Claude no llega a `github.io`. Dos cosas a revisar en la versión publicada, en DevTools → Network:

1. Que los `.br` lleguen sin `Content-Encoding: br`. Si Pages los marcara así, el navegador los descomprimiría solo,
   el decodificador fallaría y cada recurso se pediría otra vez sin comprimir (se ve un aviso «Brotli: se descarga
   sin comprimir» en la consola). Funcionaría, pero más lento que antes.
2. Si Pages ya comprimía los `.wasm` con gzip. Si lo hacía, la ganancia real es la diferencia entre gzip y
   Brotli (≈ 4.0 → 3.1 MB), no la de la tabla de arriba.
