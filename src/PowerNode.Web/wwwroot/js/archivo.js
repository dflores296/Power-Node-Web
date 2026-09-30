// ABRIR Y GUARDAR EL TABLERO — I-05 (David, 2026-09-25). Ver docs/decisiones/archivo-del-tablero.md.
//
// La aplicación no tiene servidor: el tablero vive en un archivo del equipo. Aquí solo se mueve texto
// entre el disco y la página; qué lleva el archivo y cómo se lee lo decide el modelo (ArchivoDelCuadro).
//
//   Guardar   Chrome y Edge preguntan dónde, con el nombre del tablero ya puesto (showSaveFilePicker):
//             es un «Guardar como» cada vez, a propósito. Si recordara el archivo y lo sobrescribiera
//             solo, abrir «Cocina», convertirlo en «Baño» y guardar borraba la cocina. Firefox y
//             Safari no tienen ese diálogo: el archivo se descarga.
//   Abrir     el selector de archivos del sistema.
//   Cerrar    con cambios sin guardar, el navegador pregunta antes de cerrar o recargar la pestaña.
//   Copia     lo que no está en un archivo se copia en sessionStorage, que es de esta pestaña y
//             sobrevive a recargarla: la alternativa que dejó escrita la decisión (auditoría del
//             2026-09-29, P1-3). No es localStorage: varias pestañas se pisarían.
//   Confirmar dentro de la página, con <dialog>. Antes era window.confirm: detiene la página hasta que
//             se contesta, y quien no ve el diálogo nativo —una herramienta de prueba— la ve congelada
//             (P1-3: los cuatro «congelamientos» de la auditoría eran esta pregunta sin contestar).
(() => {
    const tipos = [{ description: 'Tablero de Power Node', accept: { 'application/json': ['.json'] } }];
    let sinGuardar = false;

    async function guardar(nombre, texto) {
        if ('showSaveFilePicker' in window) {
            try {
                const manija = await window.showSaveFilePicker({ suggestedName: nombre, types: tipos });
                const escritor = await manija.createWritable();
                await escritor.write(texto);
                await escritor.close();
                return manija.name;
            } catch (e) {
                if (e.name === 'AbortError')
                    return null; // canceló el diálogo
                if (e.name !== 'SecurityError' && e.name !== 'NotAllowedError')
                    throw e;
                // Sin permiso para el diálogo (un marco, una política): se descarga.
            }
        }
        const enlace = document.createElement('a');
        enlace.href = URL.createObjectURL(new Blob([texto], { type: 'application/json' }));
        enlace.download = nombre;
        document.body.appendChild(enlace);
        enlace.click();
        enlace.remove();
        setTimeout(() => URL.revokeObjectURL(enlace.href), 10000);
        return nombre;
    }

    /** { nombre, texto } del archivo elegido, o null si se canceló. */
    function abrir() {
        return new Promise(resolve => {
            // En la página mientras se usa: Safari no abre el selector de un <input> suelto.
            const entrada = document.createElement('input');
            entrada.type = 'file';
            entrada.accept = '.json,application/json';
            entrada.hidden = true;
            document.body.appendChild(entrada);
            entrada.addEventListener('change', async () => {
                const archivo = entrada.files?.[0];
                entrada.remove();
                resolve(archivo ? { nombre: archivo.name, texto: await archivo.text() } : null);
            });
            entrada.addEventListener('cancel', () => { entrada.remove(); resolve(null); });
            entrada.click();
        });
    }

    window.addEventListener('beforeunload', e => {
        if (!sinGuardar)
            return;
        e.preventDefault();
        e.returnValue = ''; // los navegadores ponen su propio mensaje
    });

    /**
     * true si se aceptó. El mensaje respeta sus saltos de línea; el botón de aceptar dice lo que hace
     * («Reducir el gabinete»), y el foco empieza en Cancelar: aceptar borra o recorta circuitos.
     */
    function confirmar(mensaje, aceptar, cancelar) {
        return new Promise(resolve => {
            const dialogo = document.createElement('dialog');
            dialogo.className = 'confirmacion';
            const texto = document.createElement('p');
            texto.className = 'confirmacion-texto';
            texto.id = 'confirmacion-texto';
            texto.textContent = mensaje;
            dialogo.setAttribute('aria-describedby', texto.id);
            const botones = document.createElement('div');
            botones.className = 'confirmacion-botones';
            const no = boton(cancelar || 'Cancelar', 'boton', () => dialogo.close('no'));
            const si = boton(aceptar || 'Aceptar', 'boton primario', () => dialogo.close('si'));
            botones.append(no, si);
            dialogo.append(texto, botones);
            // Esc cierra con returnValue vacío: es Cancelar.
            dialogo.addEventListener('close', () => { dialogo.remove(); resolve(dialogo.returnValue === 'si'); });
            document.body.appendChild(dialogo);
            dialogo.showModal();
            no.focus();
        });
    }

    function boton(rotulo, clase, alPulsar) {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = clase;
        b.textContent = rotulo;
        b.addEventListener('click', alPulsar);
        return b;
    }

    // La copia de esta pestaña. sessionStorage puede no existir o negarse (modo privado, sin espacio):
    // entonces no hay copia y la aplicación sigue igual.
    const CLAVE_COPIA = 'powerNode.copiaDelTablero';
    const copia = {
        escribir: texto => {
            try {
                if (texto == null) sessionStorage.removeItem(CLAVE_COPIA);
                else sessionStorage.setItem(CLAVE_COPIA, texto);
            } catch { /* sin copia */ }
        },
        leer: () => {
            try { return sessionStorage.getItem(CLAVE_COPIA); } catch { return null; }
        },
    };

    (window.powerNode ??= {}).archivo = {
        guardar,
        abrir,
        marcarSinGuardar: valor => { sinGuardar = valor; },
        confirmar,
        copia,
    };
})();
