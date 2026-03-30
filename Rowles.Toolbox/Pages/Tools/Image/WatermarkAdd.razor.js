export function applyWatermark(imageDataUrl, options) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = img.naturalWidth;
            canvas.height = img.naturalHeight;
            const ctx = canvas.getContext('2d');

            ctx.drawImage(img, 0, 0);

            ctx.globalAlpha = options.opacity;
            ctx.fillStyle = options.color;
            ctx.font = `${options.fontSize}px sans-serif`;
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';

            if (options.position === 'Tile') {
                drawTiledWatermark(ctx, canvas.width, canvas.height, options);
            } else {
                drawPositionedWatermark(ctx, canvas.width, canvas.height, options);
            }

            ctx.globalAlpha = 1.0;
            resolve(canvas.toDataURL('image/png'));
        };
        img.onerror = () => reject('Failed to load image for watermarking');
        img.src = imageDataUrl;
    });
}

function drawPositionedWatermark(ctx, w, h, options) {
    const padding = options.fontSize;
    let x, y;

    switch (options.position) {
        case 'Center':
            x = w / 2;
            y = h / 2;
            break;
        case 'Bottom-Right':
            ctx.textAlign = 'right';
            x = w - padding;
            y = h - padding;
            break;
        case 'Bottom-Left':
            ctx.textAlign = 'left';
            x = padding;
            y = h - padding;
            break;
        case 'Top-Right':
            ctx.textAlign = 'right';
            x = w - padding;
            y = padding;
            break;
        case 'Top-Left':
            ctx.textAlign = 'left';
            x = padding;
            y = padding;
            break;
        default:
            x = w / 2;
            y = h / 2;
            break;
    }

    ctx.fillText(options.text, x, y);
}

function drawTiledWatermark(ctx, w, h, options) {
    const measured = ctx.measureText(options.text);
    const textWidth = measured.width;
    const textHeight = options.fontSize;
    const spacingX = textWidth + options.fontSize * 2;
    const spacingY = textHeight * 3;

    const radians = (options.rotation * Math.PI) / 180;

    const diagonal = Math.sqrt(w * w + h * h);
    const startX = -diagonal / 2;
    const startY = -diagonal / 2;
    const endX = diagonal / 2;
    const endY = diagonal / 2;

    ctx.save();
    ctx.translate(w / 2, h / 2);
    ctx.rotate(radians);

    for (let y = startY; y < endY; y += spacingY) {
        for (let x = startX; x < endX; x += spacingX) {
            ctx.fillText(options.text, x, y);
        }
    }

    ctx.restore();
}

export function downloadDataUrl(dataUrl, filename) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
