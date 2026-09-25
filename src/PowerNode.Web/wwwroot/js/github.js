// LAS ESTRELLAS DEL REPOSITORIO — I-61 (David, 2026-09-25): el botón de GitHub de la barra, como el de
// gitdiagram.com. La API pública de GitHub da 60 consultas por hora sin sesión, por dirección IP (una
// oficina entera sale por la misma): la cuenta se guarda una hora en localStorage. Si la consulta
// falla —límite, red, firewall—, el botón sale sin número; nunca estorba.
(() => {
    const repositorio = 'dflores296/Power-Node-Web';
    const clave = 'powernode.estrellas';
    const vigenciaMs = 60 * 60 * 1000;

    async function estrellas() {
        try {
            const guardada = JSON.parse(localStorage.getItem(clave) ?? 'null');
            if (guardada && Date.now() - guardada.cuando < vigenciaMs && Number.isInteger(guardada.estrellas))
                return guardada.estrellas;
        } catch { }
        try {
            const respuesta = await fetch(`https://api.github.com/repos/${repositorio}`,
                { headers: { Accept: 'application/vnd.github+json' } });
            if (!respuesta.ok)
                return null;
            const n = (await respuesta.json()).stargazers_count;
            if (!Number.isInteger(n))
                return null;
            try { localStorage.setItem(clave, JSON.stringify({ estrellas: n, cuando: Date.now() })); } catch { }
            return n;
        } catch {
            return null;
        }
    }

    (window.powerNode ??= {}).github = { estrellas };
})();
