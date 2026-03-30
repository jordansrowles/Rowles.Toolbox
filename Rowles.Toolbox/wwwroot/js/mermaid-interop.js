let loadPromise = null;

export function isDarkMode() {
    return document.documentElement.classList.contains('dark');
}

export async function loadMermaid(theme) {
    if (!loadPromise) {
        loadPromise = new Promise((resolve, reject) => {
            if (window.mermaid) { resolve(); return; }
            const s = document.createElement('script');
            s.src = 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js';
            s.onload = () => resolve();
            s.onerror = () => reject(new Error('Failed to load Mermaid.js'));
            document.head.appendChild(s);
        });
    }
    await loadPromise;
    window.mermaid.initialize({
        startOnLoad: false,
        theme: theme,
        securityLevel: 'loose',
        fontFamily: 'ui-monospace, monospace'
    });
}

let renderCounter = 0;

export async function renderDiagram(definition) {
    renderCounter++;
    const id = 'mmd-' + renderCounter;
    try {
        const { svg } = await window.mermaid.render(id, definition);
        return svg;
    } catch (e) {
        const el = document.getElementById('d' + id);
        if (el) el.remove();
        throw e;
    }
}
