export function analyzeImage(imageDataUrl) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            const w = img.naturalWidth;
            const h = img.naturalHeight;

            // Source canvas
            const src = document.createElement('canvas');
            src.width = w;
            src.height = h;
            const srcCtx = src.getContext('2d');
            srcCtx.drawImage(img, 0, 0);
            const srcData = srcCtx.getImageData(0, 0, w, h);
            const pixels = srcData.data;

            // 1. High-contrast enhancement
            const enhancedDataUrl = buildEnhanced(pixels, w, h);

            // 2. Edge detection (Sobel)
            const edgeDataUrl = buildEdgeDetection(pixels, w, h);

            // 3. Channel separation
            const redChannelDataUrl = buildChannel(pixels, w, h, 'red');
            const greenChannelDataUrl = buildChannel(pixels, w, h, 'green');
            const blueChannelDataUrl = buildChannel(pixels, w, h, 'blue');

            // 4. Brightness histogram
            const histogramDataUrl = buildHistogram(pixels, w, h);

            // 5. Alpha / transparency check
            let hasAlpha = false;
            let alphaDataUrl = null;
            for (let i = 3; i < pixels.length; i += 4) {
                if (pixels[i] < 255) { hasAlpha = true; break; }
            }
            if (hasAlpha) {
                alphaDataUrl = buildAlphaChannel(pixels, w, h);
            }

            resolve({
                enhancedDataUrl,
                edgeDataUrl,
                redChannelDataUrl,
                greenChannelDataUrl,
                blueChannelDataUrl,
                histogramDataUrl,
                hasAlpha,
                alphaDataUrl,
                width: w,
                height: h
            });
        };
        img.onerror = () => reject('Failed to load image for analysis');
        img.src = imageDataUrl;
    });
}

function buildEnhanced(pixels, w, h) {
    const canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    const imgData = ctx.createImageData(w, h);
    const out = imgData.data;

    // Calculate mean brightness
    let sum = 0;
    const len = pixels.length;
    for (let i = 0; i < len; i += 4) {
        sum += (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
    }
    const mean = sum / (len / 4);

    const factor = 4.0; // aggressive contrast boost
    for (let i = 0; i < len; i += 4) {
        out[i]     = clamp(factor * (pixels[i] - mean) + 128);
        out[i + 1] = clamp(factor * (pixels[i + 1] - mean) + 128);
        out[i + 2] = clamp(factor * (pixels[i + 2] - mean) + 128);
        out[i + 3] = 255;
    }

    ctx.putImageData(imgData, 0, 0);
    return canvas.toDataURL('image/png');
}

function buildEdgeDetection(pixels, w, h) {
    const canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    const imgData = ctx.createImageData(w, h);
    const out = imgData.data;

    // Convert to grayscale buffer
    const gray = new Float32Array(w * h);
    for (let i = 0; i < gray.length; i++) {
        const p = i * 4;
        gray[i] = 0.299 * pixels[p] + 0.587 * pixels[p + 1] + 0.114 * pixels[p + 2];
    }

    // Sobel kernels
    for (let y = 1; y < h - 1; y++) {
        for (let x = 1; x < w - 1; x++) {
            const idx = y * w + x;
            const tl = gray[(y - 1) * w + (x - 1)];
            const tc = gray[(y - 1) * w + x];
            const tr = gray[(y - 1) * w + (x + 1)];
            const ml = gray[y * w + (x - 1)];
            const mr = gray[y * w + (x + 1)];
            const bl = gray[(y + 1) * w + (x - 1)];
            const bc = gray[(y + 1) * w + x];
            const br = gray[(y + 1) * w + (x + 1)];

            const gx = -tl + tr - 2 * ml + 2 * mr - bl + br;
            const gy = -tl - 2 * tc - tr + bl + 2 * bc + br;
            const mag = Math.min(255, Math.sqrt(gx * gx + gy * gy));

            const oi = idx * 4;
            out[oi] = mag;
            out[oi + 1] = mag;
            out[oi + 2] = mag;
            out[oi + 3] = 255;
        }
    }

    ctx.putImageData(imgData, 0, 0);
    return canvas.toDataURL('image/png');
}

function buildChannel(pixels, w, h, channel) {
    const canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    const imgData = ctx.createImageData(w, h);
    const out = imgData.data;

    const offset = channel === 'red' ? 0 : channel === 'green' ? 1 : 2;
    for (let i = 0; i < pixels.length; i += 4) {
        const val = pixels[i + offset];
        out[i]     = val;
        out[i + 1] = val;
        out[i + 2] = val;
        out[i + 3] = 255;
    }

    ctx.putImageData(imgData, 0, 0);
    return canvas.toDataURL('image/png');
}

function buildHistogram(pixels, w, h) {
    const bins = new Uint32Array(256);
    for (let i = 0; i < pixels.length; i += 4) {
        const brightness = Math.round(0.299 * pixels[i] + 0.587 * pixels[i + 1] + 0.114 * pixels[i + 2]);
        bins[brightness]++;
    }

    const maxBin = Math.max(...bins);
    const cw = 512;
    const ch = 200;
    const canvas = document.createElement('canvas');
    canvas.width = cw;
    canvas.height = ch;
    const ctx = canvas.getContext('2d');

    ctx.fillStyle = '#1f2937';
    ctx.fillRect(0, 0, cw, ch);

    const barWidth = cw / 256;
    for (let i = 0; i < 256; i++) {
        const barHeight = maxBin > 0 ? (bins[i] / maxBin) * (ch - 10) : 0;
        const x = i * barWidth;
        const y = ch - barHeight;

        // Gradient from blue to white based on brightness
        const t = i / 255;
        const r = Math.round(59 + t * 196);
        const g = Math.round(130 + t * 125);
        const b = Math.round(246 - t * 10);
        ctx.fillStyle = `rgb(${r},${g},${b})`;
        ctx.fillRect(x, y, Math.max(barWidth - 0.5, 1), barHeight);
    }

    return canvas.toDataURL('image/png');
}

function buildAlphaChannel(pixels, w, h) {
    const canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    const imgData = ctx.createImageData(w, h);
    const out = imgData.data;

    for (let i = 0; i < pixels.length; i += 4) {
        const a = pixels[i + 3];
        out[i]     = a;
        out[i + 1] = a;
        out[i + 2] = a;
        out[i + 3] = 255;
    }

    ctx.putImageData(imgData, 0, 0);
    return canvas.toDataURL('image/png');
}

function clamp(val) {
    return Math.max(0, Math.min(255, Math.round(val)));
}
