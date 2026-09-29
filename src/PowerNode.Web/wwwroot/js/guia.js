// LA GUÍA DE CARGAS — I-126 (David, 2026-09-29): el árbol se despliega por ramas con <details>. Aquí,
// lo que Blazor no hace solo:
//  · llegar a una rama («guia#rama-motores», desde el «?» del tipo): abrirla, con sus antecesoras, y
//    llevarla a la vista;
//  · los enlaces del mapa: con <base href>, «#rama-…» apuntaría a la captura, así que van a
//    «guia#rama-…» y se atienden aquí, sin pasar por el enrutador ni llenar el historial;
//  · al imprimir, todo desplegado —un <details> cerrado no sale en papel— y, al terminar, como estaba.
(() => {
    function abrirHasta(el) {
        for (let d = el.closest('details'); d; d = d.parentElement?.closest('details'))
            d.open = true;
    }

    function ir(hash, suave) {
        const id = decodeURIComponent((hash || '').replace(/^#/, ''));
        const el = id && document.getElementById(id);
        if (!el || !el.closest('.guia'))
            return;
        abrirHasta(el);
        if (el.tagName === 'DETAILS')
            el.open = true;
        el.scrollIntoView({ block: 'start', behavior: suave ? 'smooth' : 'auto' });
        el.classList.remove('resaltada');
        void el.offsetWidth; // reinicia la animación si se vuelve a la misma rama
        el.classList.add('resaltada');
    }

    function desplegar(abrir) {
        document.querySelectorAll('.guia details').forEach(d => d.open = abrir);
    }

    addEventListener('click', e => {
        const a = e.target.closest?.('.guia a[data-rama]');
        if (!a || e.button !== 0 || e.ctrlKey || e.metaKey || e.shiftKey || e.altKey)
            return;
        e.preventDefault();
        e.stopPropagation();
        const url = new URL(a.href);
        history.replaceState(history.state, '', url.href);
        ir(url.hash, true);
    }, true);

    // Con la guía abierta, un «#rama-…» escrito en la dirección no recarga la página.
    addEventListener('hashchange', () => ir(location.hash, false));

    let cerradas = [];
    addEventListener('beforeprint', () => {
        cerradas = [...document.querySelectorAll('.guia details:not([open])')];
        cerradas.forEach(d => d.open = true);
    });
    addEventListener('afterprint', () => {
        cerradas.forEach(d => d.open = false);
        cerradas = [];
    });

    (window.powerNode ??= {}).guia = {
        alLlegar: () => ir(location.hash, false),
        desplegar,
    };
})();
