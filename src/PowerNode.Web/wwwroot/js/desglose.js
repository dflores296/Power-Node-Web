// LA DESCRIPCIÓN DEL DESPLEGABLE, ALINEADA CON LA DEL CIRCUITO — I-200 (David, 2026-10-07).
//
// La columna «Descripción» del cuadro la mide el navegador (table-layout automático: según lo
// capturado y el ancho de la ventana), así que CSS no la conoce. Aquí se mide y se deja como la
// fracción del desplegable que le toca a su descripción (--desc-fraccion), para que las dos terminen
// en la misma línea. Fracción y no píxeles: con table-layout: fixed, el ancho que sobra se reparte
// entre las columnas con ancho en píxeles —la descripción se pasaba de la línea— y no entre las de
// porcentaje. --desc-ancho, en píxeles, es para el ancho mínimo del desplegable antes del scroll.
(() => {
    let observador = null;

    function medir(cuadro, th) {
        const d = th.getBoundingClientRect();
        const t = cuadro.getBoundingClientRect();
        // El desplegable va en la celda que empieza en la descripción y llega al final del renglón;
        // su sangría es la de las celdas del cuadro. Su borde izquierdo (1 px) va antes de la columna.
        const celda = cuadro.querySelector(':scope > tbody > tr > td');
        const estilo = celda ? getComputedStyle(celda) : null;
        const izq = estilo ? parseFloat(estilo.paddingLeft) : 0;
        const der = estilo ? parseFloat(estilo.paddingRight) : 0;
        const ancho = d.width - izq - 1;
        const disponible = t.right - d.left - izq - der - 2;
        if (ancho <= 0 || disponible <= 0)
            return;
        const raiz = document.documentElement.style;
        raiz.setProperty('--desc-fraccion', (ancho / disponible).toFixed(5));
        raiz.setProperty('--desc-ancho', `${ancho}px`);
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
        observador = new ResizeObserver(() => medir(cuadro, th));
        observador.observe(th);
        observador.observe(cuadro);
        medir(cuadro, th);
    }

    function desconectar() {
        observador?.disconnect();
        observador = null;
        document.documentElement.style.removeProperty('--desc-fraccion');
        document.documentElement.style.removeProperty('--desc-ancho');
    }

    (window.powerNode ??= {}).desglose = { conectar: () => conectar(), desconectar };
})();
