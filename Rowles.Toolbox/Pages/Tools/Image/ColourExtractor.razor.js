export function readFileAsDataUrl(inputElement) {
    return new Promise((resolve, reject) => {
        const file = inputElement.files?.[0];
        if (!file) { reject('No file selected'); return; }
        const reader = new FileReader();
        reader.onload = () => resolve({ dataUrl: reader.result, size: file.size, name: file.name });
        reader.onerror = () => reject('Failed to read file');
        reader.readAsDataURL(file);
    });
}

export function extractColours(dataUrl, bucketSize, maxColours) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            const canvas = document.createElement('canvas');
            const maxDim = 200;
            const scale = Math.min(1, maxDim / Math.max(img.naturalWidth, img.naturalHeight));
            canvas.width = Math.round(img.naturalWidth * scale);
            canvas.height = Math.round(img.naturalHeight * scale);
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

            const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
            const data = imageData.data;
            const buckets = {};
            let totalPixels = 0;

            for (let i = 0; i < data.length; i += 4) {
                const a = data[i + 3];
                if (a < 128) continue;

                const r = Math.round(data[i] / bucketSize) * bucketSize;
                const g = Math.round(data[i + 1] / bucketSize) * bucketSize;
                const b = Math.round(data[i + 2] / bucketSize) * bucketSize;
                const key = `${Math.min(r, 255)},${Math.min(g, 255)},${Math.min(b, 255)}`;

                buckets[key] = (buckets[key] || 0) + 1;
                totalPixels++;
            }

            const sorted = Object.entries(buckets)
                .sort((a, b) => b[1] - a[1])
                .slice(0, maxColours);

            const colours = sorted.map(([key, count]) => {
                const [r, g, b] = key.split(',').map(Number);
                const hex = '#' + [r, g, b].map(v => v.toString(16).padStart(2, '0')).join('');
                const percentage = ((count / totalPixels) * 100).toFixed(1);
                return { r, g, b, hex, percentage: parseFloat(percentage), count };
            });

            resolve(colours);
        };
        img.onerror = () => reject('Failed to load image for colour extraction');
        img.src = dataUrl;
    });
}

export async function copyText(text) {
    await navigator.clipboard.writeText(text);
}
