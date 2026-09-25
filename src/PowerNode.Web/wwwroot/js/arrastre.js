// ARRASTRAR CIRCUITOS — I-69 (David, 2026-09-25).
//
// Se agarra de un asa —la celda del número del circuito en el cuadro, o el interruptor en el
// gabinete— y se suelta sobre un espacio: otra celda de número o un renglón del gabinete. Aquí solo
// se lleva el puntero; si cabe o no lo decide el modelo (CuadroDeCarga.MoverCircuito), y la tabla y
// el gabinete se redibujan juntos porque salen del mismo cuadro.
//
//   [data-arrastre="n"]   asa: el interruptor del espacio n (data-etiqueta, lo que dice el fantasma).
//   [data-destino="n"]    donde se puede soltar: el espacio n.
//
// Eventos de puntero y no el arrastre nativo del navegador: el nativo no funciona con el dedo en la
// mayoría de los celulares, y se lleva mal con los redibujados de Blazor. Esc cancela.
(() => {
    const UMBRAL = 5;          // px antes de que un clic se vuelva arrastre
    const ORILLA = 56;         // px de la orilla de la ventana en que se desplaza sola
    let dotnet = null;
    let sesion = null;

    const destinoEn = (x, y) => {
        for (const el of document.elementsFromPoint(x, y))
            if (el instanceof HTMLElement && el.dataset.destino)
                return el;
        return null;
    };

    function empezar() {
        const f = document.createElement('div');
        f.className = 'arrastre-fantasma';
        f.textContent = sesion.etiqueta;
        document.body.appendChild(f);
        sesion.fantasma = f;
        sesion.asa.classList.add('arrastre-origen');
        document.body.classList.add('arrastrando');
    }

    function seguir(x, y) {
        sesion.fantasma.style.transform = `translate(${x + 14}px, ${y + 10}px)`;
        const d = destinoEn(x, y);
        if (d !== sesion.destino) {
            sesion.destino?.classList.remove('destino-activo');
            d?.classList.add('destino-activo');
            sesion.destino = d;
        }
        // Cerca de la orilla, la página se recorre sola: el cuadro es más alto que la pantalla.
        if (y < ORILLA) window.scrollBy(0, -12);
        else if (y > window.innerHeight - ORILLA) window.scrollBy(0, 12);
    }

    function terminar() {
        if (!sesion) return;
        sesion.fantasma?.remove();
        sesion.destino?.classList.remove('destino-activo');
        sesion.asa.classList.remove('arrastre-origen');
        document.body.classList.remove('arrastrando');
        sesion = null;
    }

    document.addEventListener('pointerdown', e => {
        if (e.button !== 0 || !dotnet) return;
        const asa = e.target instanceof Element ? e.target.closest('[data-arrastre]') : null;
        if (!asa) return;
        sesion = {
            origen: Number(asa.dataset.arrastre),
            etiqueta: asa.dataset.etiqueta || `Circuito ${asa.dataset.arrastre}`,
            asa, id: e.pointerId, x0: e.clientX, y0: e.clientY, activo: false, destino: null,
        };
    });

    document.addEventListener('pointermove', e => {
        if (!sesion || e.pointerId !== sesion.id) return;
        if (!sesion.activo) {
            if (Math.hypot(e.clientX - sesion.x0, e.clientY - sesion.y0) < UMBRAL) return;
            sesion.activo = true;
            empezar();
        }
        e.preventDefault();
        seguir(e.clientX, e.clientY);
    }, { passive: false });

    document.addEventListener('pointerup', e => {
        if (!sesion || e.pointerId !== sesion.id) return;
        const { activo, destino, origen } = sesion;
        terminar();
        if (!activo) return;
        // Soltar fuera de un espacio es cancelar: todo queda como estaba.
        if (destino) dotnet.invokeMethodAsync('Soltar', origen, Number(destino.dataset.destino));
    });

    document.addEventListener('pointercancel', terminar);
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape' && sesion?.activo) { e.stopPropagation(); terminar(); }
    }, true);

    window.powerNode = window.powerNode || {};
    window.powerNode.arrastre = {
        conectar: ref => { dotnet = ref; },
        desconectar: () => { dotnet = null; terminar(); },
    };
})();
