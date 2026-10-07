// LOS ANCHOS DEL DESPLEGABLE POR BLOQUES — acomodo-del-desplegable.md (David, 2026-10-07); antes, I-200.
//
// La columna «Descripción» del cuadro la mide el navegador (table-layout automático: según lo capturado y
// el ancho de la ventana), así que CSS no la conoce. Aquí se mide y, en cada desplegable abierto:
//   - la descripción de cada bloque termina en la misma línea que la del circuito (I-200);
//   - Tipo, Subtipo, Cant., F.P., Total y el bote (data-g) miden su base en todos los bloques: quedan
//     una debajo de otra (David: «los botes de basura también se pueden alinear»);
//   - lo que sobra se reparte entre las columnas del medio, en proporción a su base (data-w);
//   - todos los bloques del mismo largo: el del desplegable o, si no cabe, el del bloque más ancho. El
//     desplegable se recorre de lado; los campos no se aprietan (David: «existe el scroll»).
// Se vuelve a medir cuando cambia el ancho del cuadro o de su descripción, y cuando Blazor dibuja otros
// bloques (un desplegable que se abre, una línea que cambia de juego de columnas).
//
// La línea se toma del borde izquierdo de «Tipo», no del derecho de «Descripción»: la descripción del cuadro
// es fija al recorrer el cuadro de lado (sticky) y el desplegable no; «Tipo» se mueve con él.
(() => {
    let tamano = null;
    let cambios = null;
    let pendiente = 0;

    function medir() {
        pendiente = 0;
        const cuadro = document.querySelector('#sec-cuadro > .cuadro');
        const tipo = cuadro?.querySelector(':scope > thead th[data-col="tipo"]');
        if (!tipo)
            return;
        const linea = tipo.getBoundingClientRect().left;
        for (const recorre of cuadro.querySelectorAll('.desglose-scroll')) {
            const tablas = [...recorre.querySelectorAll(':scope > table.desglose')];
            if (tablas.length === 0)
                continue;
            const desc = Math.max(160, linea - recorre.getBoundingClientRect().left);
            const datos = tablas.map(t => {
                const cols = [...t.querySelectorAll(':scope > colgroup > col')];
                const base = cols.reduce((s, c) => s + (+c.dataset.w || 0), 0);
                const repartible = cols.filter(c => !c.hasAttribute('data-g')).reduce((s, c) => s + (+c.dataset.w || 0), 0);
                return { t, cols, base, repartible };
            });
            const ancho = Math.max(recorre.clientWidth, ...datos.map(d => d.base + desc));
            for (const { t, cols, base, repartible } of datos) {
                t.style.width = `${ancho}px`;
                const sobra = ancho - base - desc;
                for (const c of cols) {
                    const w = +c.dataset.w || 0;
                    c.style.width = c.dataset.col === 'Desc' ? `${desc}px`
                        : c.hasAttribute('data-g') || repartible === 0 ? `${w}px`
                        : `${w + sobra * w / repartible}px`;
                }
            }
        }
    }

    function pedir() {
        if (!pendiente)
            pendiente = requestAnimationFrame(medir);
    }

    function conectar(intentos = 20) {
        desconectar();
        const cuadro = document.querySelector('#sec-cuadro > .cuadro');
        const th = cuadro?.querySelector(':scope > thead th.descripcion');
        if (!th) {
            // La primera vez el cuadro puede llegar un cuadro después de la página.
            if (intentos > 0) requestAnimationFrame(() => conectar(intentos - 1));
            return;
        }
        tamano = new ResizeObserver(pedir);
        tamano.observe(th);
        tamano.observe(cuadro);
        // Solo los renglones que entran o salen: los anchos que se ponen aquí son atributos, no cuentan.
        cambios = new MutationObserver(pedir);
        cambios.observe(cuadro, { childList: true, subtree: true });
        pedir();
    }

    function desconectar() {
        tamano?.disconnect();
        cambios?.disconnect();
        tamano = cambios = null;
        if (pendiente) cancelAnimationFrame(pendiente);
        pendiente = 0;
    }

    (window.powerNode ??= {}).desglose = { conectar: () => conectar(), desconectar };
})();
