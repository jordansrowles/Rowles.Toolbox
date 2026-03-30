export function renderSvgToPng(svgMarkup, width, height) {
    return new Promise((resolve, reject) => {
        const canvas = document.createElement('canvas');
        canvas.width = width || 800;
        canvas.height = height || 600;
        const ctx = canvas.getContext('2d');

        const blob = new Blob([svgMarkup], { type: 'image/svg+xml;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const img = new Image();

        img.onload = () => {
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
            URL.revokeObjectURL(url);
            const dataUrl = canvas.toDataURL('image/png');
            resolve(dataUrl);
        };
        img.onerror = () => {
            URL.revokeObjectURL(url);
            reject('Failed to render SVG');
        };
        img.src = url;
    });
}

export function downloadDataUrl(dataUrl, filename) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

export function readFileAsText(inputElement) {
    return new Promise((resolve, reject) => {
        const file = inputElement.files?.[0];
        if (!file) { reject('No file selected'); return; }
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject('Failed to read file');
        reader.readAsText(file);
    });
}
