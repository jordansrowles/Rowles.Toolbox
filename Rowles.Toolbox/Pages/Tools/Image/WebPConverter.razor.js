export function readFileAsDataUrl(inputElement) {
    return new Promise((resolve, reject) => {
        const file = inputElement.files?.[0];
        if (!file) { reject('No file selected'); return; }
        const reader = new FileReader();
        reader.onload = () => resolve({ dataUrl: reader.result, size: file.size, name: file.name, type: file.type });
        reader.onerror = () => reject('Failed to read file');
        reader.readAsDataURL(file);
    });
}

export function convertImage(dataUrl, targetFormat, quality) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = img.naturalWidth;
            canvas.height = img.naturalHeight;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0);

            const mimeType = targetFormat === 'png' ? 'image/png' : 'image/webp';
            const q = targetFormat === 'png' ? undefined : quality / 100;
            const outputDataUrl = canvas.toDataURL(mimeType, q);

            const base64 = outputDataUrl.split(',')[1];
            const byteLength = Math.ceil(base64.length * 3 / 4);

            resolve({ dataUrl: outputDataUrl, size: byteLength });
        };
        img.onerror = () => reject('Failed to load image for conversion');
        img.src = dataUrl;
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
