// EL TEMA — I-57 (David, 2026-09-25).
//
// Tres elecciones: «automatico» (en pantalla, «Sistema») sigue al sistema (prefers-color-scheme);
// «claro» y «oscuro» lo fijan y se recuerdan en localStorage. Aquí se resuelve cuál manda y se
// escribe en <html data-tema>, con "claro" u "oscuro": es lo único que lee la hoja de estilos. Así
// la paleta oscura se escribe una sola vez en app.css.
//
// Va en el <head> y sin defer: si corriera después, la página se pintaría un instante en claro.
(() => {
    const clave = 'powernode.tema';
    const elecciones = ['automatico', 'claro', 'oscuro'];
    const sistema = window.matchMedia('(prefers-color-scheme: dark)');

    // localStorage puede no estar (ventana privada, sitio bloqueado): entonces vale para esta visita.
    let eleccion = 'automatico';
    try {
        const guardada = localStorage.getItem(clave);
        if (elecciones.includes(guardada))
            eleccion = guardada;
    } catch { }

    function aplicar() {
        document.documentElement.dataset.tema =
            eleccion === 'automatico' ? (sistema.matches ? 'oscuro' : 'claro') : eleccion;
    }

    aplicar();
    sistema.addEventListener('change', aplicar); // en automático, el cambio del sistema se ve al momento

    (window.powerNode ??= {}).tema = {
        eleccion: () => eleccion,
        /** Fija la elección (automatico, claro u oscuro) y devuelve la que quedó. */
        fijar(nueva) {
            if (!elecciones.includes(nueva))
                return eleccion;
            eleccion = nueva;
            try {
                if (eleccion === 'automatico')
                    localStorage.removeItem(clave);
                else
                    localStorage.setItem(clave, eleccion);
            } catch { }
            aplicar();
            return eleccion;
        },
    };
})();
