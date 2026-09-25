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

    (window.powerNode ??= {}).archivo = {
        guardar,
        abrir,
        marcarSinGuardar: valor => { sinGuardar = valor; },
        confirmar: mensaje => window.confirm(mensaje),
    };
})();
