// EL TECLADO DE LA CAPTURA — I-55 e I-56 (David, 2026-09-25).
//
// Blazor no maneja el teclado más allá de lo que hace el navegador; todo lo de aquí es comportamiento
// de captura, como en una hoja de cálculo:
//
//   Tab / Shift+Tab   igual que siempre.
//   Enter             baja al mismo campo del renglón siguiente; Shift+Enter sube (data-col).
//   ↑ / ↓             cambian de renglón. Ya no suman o restan 1 en un número, ni cambian un
//                     selector: una flecha en «P» reacomodaba el tablero completo.
//   Alt+↓ o Espacio   abren la lista de un selector.
//   Esc               deshace la edición del campo: regresa al valor que tenía al entrar.
//   Ctrl+Enter        abre o cierra el desglose del circuito (el ▸ ya no es parada de Tab).
//   Alt+1 … Alt+5     Ficha, Cuadro, Canalizaciones, Resumen, Alimentador.
//
// «El mismo campo» lo dice data-col: el mismo valor en la misma tabla. Se escucha en captura
// (tercer argumento true) para correr antes que Blazor.
(() => {
    const esNumero = el => el instanceof HTMLInputElement && el.type === 'number';
    const esCampo = el => el instanceof HTMLInputElement || el instanceof HTMLSelectElement;
    const esSelector = el => el instanceof HTMLSelectElement;
    const visible = el => !el.disabled && el.offsetParent !== null;

    // ---- Al entrar: recordar el valor (Esc, obligatorios) y seleccionar el número completo -------
    // I-55: con el cursor a un lado del «20», teclear 5 daba «520» o «205». El mouseup del mismo
    // clic deshacía la selección: se cancela.
    document.addEventListener('focusin', e => {
        const el = e.target;
        if (esCampo(el) && el.type !== 'checkbox')
            el.dataset.previo = el.value;
        if (esNumero(el)) {
            el.select();
            el.addEventListener('mouseup', ev => ev.preventDefault(), { once: true });
        }
        mostrarAyuda(el);
    });

    document.addEventListener('focusout', e => {
        if (!(e.relatedTarget instanceof HTMLElement))
            ocultarAyuda();
    });

    // ---- I-55: un campo obligatorio (data-requerido) que se vacía recupera su valor --------------
    // Antes de que Blazor lea el cambio: sin esto el campo quedaba en blanco mientras el cálculo
    // seguía con el valor anterior. Las cargas no son obligatorias: vacío es 0 (lo resuelve la página).
    document.addEventListener('change', e => {
        const el = e.target;
        if (esNumero(el) && el.hasAttribute('data-requerido') && el.value.trim() === '' && el.dataset.previo)
            el.value = el.dataset.previo;
    }, true);

    // ---- Teclas ----------------------------------------------------------------------------------
    document.addEventListener('keydown', e => {
        const el = e.target;

        if (e.altKey && !e.ctrlKey && !e.shiftKey && !e.metaKey && /^Digit[1-5]$/.test(e.code)) {
            if (irASeccion(Number(e.code.slice(5))))
                e.preventDefault();
            return;
        }

        if (!esCampo(el))
            return; // botones y enlaces: Enter y Espacio hacen lo de siempre

        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
            if (alternarDesglose(el))
                e.preventDefault();
            return;
        }

        if (e.key === 'Escape') {
            if (deshacer(el))
                e.preventDefault();
            return;
        }

        const flecha = e.key === 'ArrowUp' ? -1 : e.key === 'ArrowDown' ? 1 : 0;
        if (flecha && !e.altKey && !e.ctrlKey && !e.metaKey) {
            // Un número no suma ni resta 1; un selector no cambia de opción.
            if (esNumero(el) || esSelector(el))
                e.preventDefault();
            if (el.dataset.col && mover(el, flecha))
                e.preventDefault();
            return;
        }

        if ((e.key === 'ArrowLeft' || e.key === 'ArrowRight') && esSelector(el) && !e.altKey) {
            e.preventDefault(); // algunos navegadores cambian la opción con ← →
            return;
        }

        if (e.key === 'Enter' && !e.altKey && el.dataset.col) {
            // En el último renglón no hay a dónde bajar: Enter guarda, como siempre.
            if (mover(el, e.shiftKey ? -1 : 1))
                e.preventDefault();
        }
    }, true);

    /** El mismo campo (data-col) del renglón anterior o siguiente, en la misma tabla. */
    function mover(el, direccion) {
        const tabla = el.closest('table');
        if (!tabla)
            return false;
        const lista = [...tabla.querySelectorAll(`[data-col="${el.dataset.col}"]`)]
            .filter(x => x.closest('table') === tabla && visible(x));
        const destino = lista[lista.indexOf(el) + direccion];
        if (!destino)
            return false;
        destino.focus();
        destino.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        return true;
    }

    /** Esc: el valor que tenía al entrar. Blazor se entera por los mismos eventos que al teclear. */
    function deshacer(el) {
        if (el.type === 'checkbox' || !('previo' in el.dataset) || el.value === el.dataset.previo)
            return false;
        el.value = el.dataset.previo;
        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));
        if (esNumero(el))
            el.select();
        return true;
    }

    /** Ctrl+Enter en cualquier campo del circuito o de su desglose. */
    function alternarDesglose(el) {
        const espacio = el.closest('table.desglose')?.dataset.circuito ?? el.closest('tr[data-espacio]')?.dataset.espacio;
        const boton = espacio && document.querySelector(`tr[data-espacio="${espacio}"] button.desglosar`);
        if (!boton)
            return false;
        boton.click(); // la página pone el foco en el primer aparato, o de regreso en la descripción
        return true;
    }

    // A dónde lleva cada atajo: el primer campo que exista de la lista, en ese orden.
    const secciones = {
        1: ['#sec-ficha', ['input, select']],
        2: ['#sec-cuadro', ['input[data-col="desc"]']],
        3: ['#sec-canalizaciones', ['input[data-col="cn-nombre"]', 'input, select', 'button']],
        4: ['#sec-resumen', ['input, select']],
        5: ['#sec-alimentador', ['input, select']],
    };

    function irASeccion(n) {
        const [seccion, opciones] = secciones[n];
        const destino = opciones
            .map(campos => [...document.querySelectorAll(`${seccion} :is(${campos})`)].find(visible))
            .find(Boolean);
        if (!destino)
            return false;
        destino.focus();
        destino.scrollIntoView({ block: 'center' });
        return true;
    }

    // ---- Barra de ayuda (I-56): la ayuda del campo con el foco, sin paradas de Tab extra -----------
    // Las ayudas de la pantalla van en `title` y solo se ven con el ratón. La barra toma la del
    // encabezado de la columna (data-col en el <th>), la de la etiqueta del campo o la del propio
    // campo, y recuerda las teclas.
    let barra;

    function mostrarAyuda(el) {
        if (!(el instanceof HTMLElement) || !el.matches('input, select, button'))
            return ocultarAyuda();
        const { nombre, ayuda } = ayudaDe(el);
        if (!nombre && !ayuda)
            return ocultarAyuda();
        barra ??= crearBarra();
        barra.querySelector('.campo').textContent = nombre;
        barra.querySelector('.texto').textContent = ayuda;
        barra.hidden = false;
        document.body.classList.add('con-ayuda');
    }

    function ocultarAyuda() {
        if (barra)
            barra.hidden = true;
        document.body.classList.remove('con-ayuda');
    }

    function ayudaDe(el) {
        let nombre = el.getAttribute('aria-label') ?? '';
        let ayuda = '';
        const tabla = el.closest('table');
        if (el.dataset.col && tabla) {
            const th = [...tabla.querySelectorAll(`thead [data-col~="${el.dataset.col}"]`)].find(x => x.closest('table') === tabla);
            if (th) {
                ayuda = (th.querySelector('.ayuda') ?? th).getAttribute('title') ?? '';
                nombre ||= th.textContent.trim();
            }
        }
        const etiqueta = el.closest('label');
        if (etiqueta) {
            const span = etiqueta.querySelector('.ayuda');
            nombre ||= (span?.textContent ?? etiqueta.firstChild?.textContent ?? '').trim();
            ayuda ||= span?.getAttribute('title') ?? etiqueta.getAttribute('title') ?? '';
        }
        const propia = el.getAttribute('title');
        if (propia && propia !== ayuda)
            ayuda = ayuda ? `${ayuda}\n${propia}` : propia;
        nombre ||= el.textContent.trim();
        return { nombre, ayuda };
    }

    function crearBarra() {
        const b = document.createElement('div');
        b.id = 'barra-ayuda';
        b.hidden = true;
        b.setAttribute('aria-hidden', 'true'); // el lector de pantalla ya lee aria-label y title del campo
        b.innerHTML =
            '<div class="ayuda-campo"><strong class="campo"></strong><span class="texto"></span></div>' +
            '<div class="teclas">Enter ↓ · Shift+Enter ↑ · ↑↓ renglón · Esc deshace · Alt+↓ abre lista · ' +
            'Ctrl+Enter desglose · Alt+1…5 secciones</div>';
        document.body.appendChild(b);
        return b;
    }

    // ---- Para la página: poner el foco en lo que acaba de crearse o abrirse (I-56) ---------------
    // window.powerNode ya existe: tema.js, en el <head>, puso ahí el tema (I-57).
    (window.powerNode ??= {}).enfocar = selector => {
        const el = document.querySelector(selector);
        if (!el)
            return;
        el.focus();
        el.scrollIntoView({ block: 'nearest', inline: 'nearest' });
    };
})();
