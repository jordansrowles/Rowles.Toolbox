export function generateFavicon(dataUrl, size) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = size;
            canvas.height = size;
            const ctx = canvas.getContext('2d');
            ctx.imageSmoothingEnabled = true;
            ctx.imageSmoothingQuality = 'high';
            ctx.drawImage(img, 0, 0, size, size);

            const outputDataUrl = canvas.toDataURL('image/png');
            const base64 = outputDataUrl.split(',')[1];
            const byteLength = Math.ceil(base64.length * 3 / 4);

            resolve({ dataUrl: outputDataUrl, size: byteLength });
        };
        img.onerror = () => reject('Failed to load image for favicon generation');
        img.src = dataUrl;
    });
}

export function getImageDimensions(dataUrl) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve({ width: img.naturalWidth, height: img.naturalHeight });
        img.onerror = () => reject('Failed to load image');
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

export async function downloadAllAsZip(favicons) {
    // favicons is an array of { dataUrl, filename }
    // We'll trigger individual downloads with a short delay between each
    for (let i = 0; i < favicons.length; i++) {
        const { dataUrl, filename } = favicons[i];
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        if (i < favicons.length - 1) {
            await new Promise(r => setTimeout(r, 300));
        }
    }
}

export async function copyToClipboard(text) {
    await navigator.clipboard.writeText(text);
}
