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
    // Los números son campos de texto (data-numero, inputmode="decimal"), no type="number" — I-81: Chrome
    // borraba la coma al teclear («0,9» quedaba 9) y Firefox la entregaba vacía. Aquí se leen igual en
    // todos los navegadores (ver leerNumero).
    const esNumero = el => el instanceof HTMLInputElement && 'numero' in el.dataset;
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
    //
    // I-77, I-80: un número fuera de su min/max tampoco entra —0 V dividía entre cero, un F.P. de 1.5
    // tumbaba el alimentador—. Regresa al valor que tenía al entrar y un aviso dice por qué: si se
    // quedara escrito, la casilla diría una cosa y el cálculo seguiría con otra (I-82). El min y el
    // max son los del propio campo; la página los toma del modelo.
    //
    // I-81: la coma. «1,500» es coma de miles (así se escribe en México) y queda 1500; «0,9» o «2,5»
    // es coma decimal y queda 0.9 o 2.5, con un aviso de cómo se leyó. Lo que no es número regresa.
    document.addEventListener('change', e => {
        const el = e.target;
        if (!esNumero(el))
            return;
        ultimo = el;
        delete el.dataset.rechazo;
        const tecleado = el.value.trim();
        if (tecleado === '') {
            el.value = '';
            if (el.hasAttribute('data-requerido') && el.dataset.previo)
                el.value = el.dataset.previo;
            return;
        }
        const leido = leerNumero(tecleado);
        if (leido === null) {
            regresar(el, tecleado, 'no es un número');
            return;
        }
        el.value = leido.texto;
        const min = el.min === '' ? -Infinity : Number(el.min);
        const max = el.max === '' ? Infinity : Number(el.max);
        if (leido.valor < min || leido.valor > max) {
            const rango = Number.isFinite(min) && Number.isFinite(max) ? `va de ${el.min} a ${el.max}`
                : Number.isFinite(min) ? `el mínimo es ${el.min}` : `el máximo es ${el.max}`;
            regresar(el, tecleado, `no se admite; ${rango}`);
            return;
        }
        if (leido.comaDecimal)
            avisar(`${ayudaDe(el).nombre}: «${tecleado}» se leyó como ${leido.texto}. El separador decimal es el punto.`, '');
    }, true);

    /**
     * «1500», «1,500», «12,000.5» → 1500, 1500, 12000.5; «0,9», «2,5», «1.500,50» → 0.9, 2.5, 1500.50
     * (coma decimal). null si no es un número. Tres cifras después de la coma, y no empieza en 0: miles.
     */
    function leerNumero(texto) {
        const t = texto.replace(/\s+/g, '');
        let limpio = null, comaDecimal = false;
        if (/^-?(\d+\.?\d*|\.\d+)$/.test(t))
            limpio = t;
        else if (/^-?[1-9]\d{0,2}(,\d{3})+(\.\d+)?$/.test(t))
            limpio = t.replace(/,/g, '');
        else if (/^-?\d*,\d+$/.test(t))
            [limpio, comaDecimal] = [t.replace(',', '.'), true];
        else if (/^-?[1-9]\d{0,2}(\.\d{3})+,\d+$/.test(t))
            [limpio, comaDecimal] = [t.replace(/\./g, '').replace(',', '.'), true];
        if (limpio === null)
            return null;
        const valor = Number(limpio);
        return Number.isFinite(valor) ? { valor, texto: limpio, comaDecimal } : null;
    }

    /** El campo regresa al valor que tenía al entrar: «Circuito 1 · F.P.: 1.5 no se admite; va de 0.1 a 1. Se regresó a 0.90.» */
    function regresar(el, tecleado, porque) {
        if (!('previo' in el.dataset)) {
            // Sin valor de antes —llegó sin foco, p. ej. escrito por otro programa—, la página lo
            // rechaza y lo regresa con regresarUltimo; aquí se guarda el porqué (auditoría 2026-09-29, P2-3).
            el.dataset.rechazo = `${tecleado} ${porque}`;
            return;
        }
        el.value = el.dataset.previo;
        const queda = el.value === '' ? 'Se dejó vacío.' : `Se regresó a ${el.value}.`;
        avisar(`${ayudaDe(el).nombre}: ${tecleado} ${porque}. ${queda}`, 'mal');
        el.classList.add('rechazado');
        setTimeout(() => el.classList.remove('rechazado'), 1500);
    }

    let avisoFlotante;

    /** El último campo numérico que cambió: al que la página regresa un valor que no aceptó (P2-3). */
    let ultimo;

    function avisar(texto, clase) {
        avisoFlotante?.remove();
        const aviso = avisoFlotante = document.createElement('div');
        aviso.className = `aviso-flotante ${clase}`.trim();
        aviso.setAttribute('role', clase === 'mal' ? 'alert' : 'status');
        aviso.textContent = texto;
        document.body.appendChild(aviso);
        setTimeout(() => aviso.remove(), 5500);
    }

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
        pendiente = false;
        barra.hidden = false;
        document.body.classList.add('con-ayuda');
        reservarAlto();
    }

    // El pie de la página se recorre hasta arriba de la barra, mida lo que mida — I-88.
    function reservarAlto() {
        if (barra && !barra.hidden)
            document.body.style.setProperty('--alto-ayuda', `${barra.offsetHeight}px`);
    }

    // Con el botón del ratón abajo, la barra se quita hasta soltarlo — I-162. Al quitarla se quita también
    // el espacio que le reservaba el pie: abajo de la página todo baja de golpe y el clic se soltaba en otra
    // cosa («Comparar opciones» no se abría).
    let pulsando = false;
    let pendiente = false;
    document.addEventListener('pointerdown', () => { pulsando = true; }, true);
    // pointercancel: en pantalla táctil, un deslizamiento cancela el toque sin pointerup; sin esto la barra se
    // quedaba puesta hasta el siguiente toque — revisión de cabos sueltos del 2026-10-03.
    const alSoltar = () => {
        pulsando = false;
        setTimeout(() => { if (pendiente) ocultarAyuda(); });
    };
    addEventListener('pointerup', alSoltar, true);
    addEventListener('pointercancel', alSoltar, true);

    function ocultarAyuda() {
        pendiente = pulsando;
        if (pulsando)
            return;
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
        new ResizeObserver(reservarAlto).observe(b);
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

    // ---- Para la página: regresar un selector que el modelo no aceptó (I-78, I-79) ---------------
    // Blazor no toca el DOM si el valor del modelo no cambió: el selector se quedaba diciendo «3
    // polos» con el circuito en 1. La página lo regresa aquí, y también lo que Esc recuerda.
    //
    // N-3 (auditoría del 2026-10-02): al cancelar «Reducir espacios» el menú se quedó en «6» en una
    // prueba por script. No se reprodujo ni con clic ni por script, pero el valor se vuelve a poner
    // en el cuadro siguiente: si un dibujo de Blazor o el propio cambio llegan después, gana el vigente.
    //
    // R3-1 (ronda 3): con la pestaña oculta el cuadro siguiente no llega (requestAnimationFrame no corre
    // hasta que se ve) y el menú se quedó en «6». Se pone también en la siguiente vuelta del ciclo
    // (setTimeout) y, si la pestaña está oculta, otra vez al volver a verse.
    const alVolver = new Map();
    window.powerNode.restablecer = (selector, valor) => {
        const poner = () => {
            const el = document.querySelector(selector);
            if (!el)
                return;
            el.value = valor;
            el.dataset.previo = valor;
        };
        poner();
        setTimeout(poner);
        requestAnimationFrame(poner);
        if (document.visibilityState === 'hidden')
            alVolver.set(selector, poner);
    };
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState !== 'visible')
            return;
        alVolver.forEach(poner => poner());
        alVolver.clear();
    });
    // Si el ingeniero cambia ese campo antes de volver (no puede con la pestaña oculta, pero por si
    // acaso), ya no se le regresa: manda lo que eligió.
    document.addEventListener('change', e => {
        for (const selector of alVolver.keys())
            if (e.target instanceof Element && e.target.matches(selector))
                alVolver.delete(selector);
    }, true);

    // ---- Para la página: regresar el último número que el modelo no aceptó (P2-3) -----------------
    // Lo mismo, para un campo numérico: si el valor llegó sin foco, el filtro de arriba no sabía a qué
    // regresarlo y la casilla se quedaba con «1.5» mientras el cálculo seguía con 0.90 (auditoría del
    // 2026-09-29). La página, que sí sabe el valor vigente, lo manda aquí; se marca y se dice por qué.
    window.powerNode.regresarUltimo = valor => {
        const el = ultimo;
        if (!el)
            return;
        const tecleado = el.dataset.rechazo ?? `${el.value} no se admite`;
        delete el.dataset.rechazo;
        el.value = valor;
        el.dataset.previo = valor;
        avisar(`${ayudaDe(el).nombre}: ${tecleado}. ${valor === '' ? 'Se dejó vacío.' : `Se regresó a ${valor}.`}`, 'mal');
        el.classList.add('rechazado');
        setTimeout(() => el.classList.remove('rechazado'), 1500);
    };
})();
