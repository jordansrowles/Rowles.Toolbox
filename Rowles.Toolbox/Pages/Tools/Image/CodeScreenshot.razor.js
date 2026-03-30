const KEYWORDS = new Set([
    'abstract', 'as', 'async', 'await', 'base', 'bool', 'break', 'byte',
    'case', 'catch', 'char', 'class', 'const', 'continue', 'decimal',
    'default', 'delegate', 'do', 'double', 'else', 'enum', 'event',
    'extern', 'false', 'finally', 'fixed', 'float', 'for', 'foreach',
    'from', 'function', 'get', 'goto', 'if', 'implements', 'import',
    'in', 'int', 'interface', 'internal', 'is', 'let', 'lock', 'long',
    'namespace', 'new', 'null', 'object', 'operator', 'out', 'override',
    'params', 'partial', 'private', 'protected', 'public', 'readonly',
    'record', 'ref', 'return', 'sealed', 'set', 'short', 'sizeof',
    'static', 'string', 'struct', 'switch', 'task', 'this', 'throw',
    'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe',
    'ushort', 'using', 'value', 'var', 'virtual', 'void', 'volatile',
    'where', 'while', 'with', 'yield',
    'def', 'elif', 'except', 'exec', 'global', 'lambda', 'nonlocal',
    'not', 'or', 'pass', 'print', 'raise', 'self', 'super',
    'export', 'extends', 'constructor', 'undefined', 'NaN',
]);

const COLOURS = {
    dark: {
        keyword: '#569cd6',
        string: '#ce9178',
        comment: '#6a9955',
        number: '#b5cea8',
        default: '#d4d4d4',
        lineNumber: '#858585',
        windowBg: '#1e1e1e',
        titleText: '#cccccc',
        titleBar: '#323233',
    },
    light: {
        keyword: '#0000ff',
        string: '#a31515',
        comment: '#008000',
        number: '#098658',
        default: '#1e1e1e',
        lineNumber: '#999999',
        windowBg: '#ffffff',
        titleText: '#333333',
        titleBar: '#e8e8e8',
    },
};

function tokenise(line) {
    const tokens = [];
    let i = 0;
    while (i < line.length) {
        // Comments
        if (line[i] === '/' && line[i + 1] === '/') {
            tokens.push({ type: 'comment', text: line.slice(i) });
            return tokens;
        }
        if (line[i] === '#' && (i === 0 || line.slice(0, i).trim() === '')) {
            tokens.push({ type: 'comment', text: line.slice(i) });
            return tokens;
        }

        // Strings
        if (line[i] === '"' || line[i] === "'" || line[i] === '`') {
            const quote = line[i];
            let j = i + 1;
            while (j < line.length && line[j] !== quote) {
                if (line[j] === '\\') j++;
                j++;
            }
            j = Math.min(j + 1, line.length);
            tokens.push({ type: 'string', text: line.slice(i, j) });
            i = j;
            continue;
        }

        // Numbers
        if (/[0-9]/.test(line[i]) && (i === 0 || /[\s(,=+\-*/<>[\]{}:;!&|^~%]/.test(line[i - 1]))) {
            let j = i;
            while (j < line.length && /[0-9.xXa-fA-F_eEbBuUlLdDfFnN]/.test(line[j])) j++;
            tokens.push({ type: 'number', text: line.slice(i, j) });
            i = j;
            continue;
        }

        // Words (identifiers/keywords)
        if (/[a-zA-Z_$@]/.test(line[i])) {
            let j = i;
            while (j < line.length && /[a-zA-Z0-9_$]/.test(line[j])) j++;
            const word = line.slice(i, j);
            const type = KEYWORDS.has(word) ? 'keyword' : 'default';
            tokens.push({ type, text: word });
            i = j;
            continue;
        }

        // Whitespace runs
        if (/\s/.test(line[i])) {
            let j = i;
            while (j < line.length && /\s/.test(line[j])) j++;
            tokens.push({ type: 'default', text: line.slice(i, j) });
            i = j;
            continue;
        }

        // Punctuation / operators
        tokens.push({ type: 'default', text: line[i] });
        i++;
    }
    return tokens;
}

function parseGradient(background) {
    if (!background || background === 'transparent') return null;
    if (background.startsWith('#') && !background.includes('→')) return { type: 'solid', colour: background };
    const parts = background.split('→').map(s => s.trim());
    return { type: 'gradient', stops: parts };
}

function drawRoundedRect(ctx, x, y, w, h, r) {
    r = Math.min(r, w / 2, h / 2);
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);
    ctx.quadraticCurveTo(x + w, y, x + w, y + r);
    ctx.lineTo(x + w, y + h - r);
    ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
    ctx.lineTo(x + r, y + h);
    ctx.quadraticCurveTo(x, y + h, x, y + h - r);
    ctx.lineTo(x, y + r);
    ctx.quadraticCurveTo(x, y, x + r, y);
    ctx.closePath();
}

export function renderScreenshot(code, options) {
    const {
        background = '#1e1e1e',
        padding = 64,
        borderRadius = 12,
        fontSize = 14,
        lineNumbers = true,
        windowControls = true,
        title = 'Untitled',
        theme = 'dark',
        width = 680,
    } = options || {};

    const colours = COLOURS[theme] || COLOURS.dark;
    const font = `${fontSize}px "Cascadia Code", "Fira Code", "JetBrains Mono", "Source Code Pro", "Consolas", monospace`;
    const lineHeight = Math.round(fontSize * 1.6);
    const titleBarHeight = windowControls || title ? 40 : 0;

    const lines = (code || '').replace(/\r\n/g, '\n').split('\n');

    // Measure text width
    const measureCanvas = document.createElement('canvas');
    const measureCtx = measureCanvas.getContext('2d');
    measureCtx.font = font;

    const lineNumWidth = lineNumbers ? measureCtx.measureText(String(lines.length).padStart(3, ' ')).width + 24 : 0;

    let maxLineWidth = 0;
    for (const line of lines) {
        const w = measureCtx.measureText(line).width;
        if (w > maxLineWidth) maxLineWidth = w;
    }

    const codePadding = 16;
    const windowWidth = Math.max(
        width,
        maxLineWidth + lineNumWidth + codePadding * 2 + 8
    );
    const codeHeight = lines.length * lineHeight + codePadding * 2;
    const windowHeight = titleBarHeight + codeHeight;

    const canvasWidth = windowWidth + padding * 2;
    const canvasHeight = windowHeight + padding * 2;

    const canvas = document.createElement('canvas');
    canvas.width = canvasWidth;
    canvas.height = canvasHeight;
    const ctx = canvas.getContext('2d');

    // Background
    const bg = parseGradient(background);
    if (bg) {
        if (bg.type === 'solid') {
            ctx.fillStyle = bg.colour;
            ctx.fillRect(0, 0, canvasWidth, canvasHeight);
        } else {
            const gradient = ctx.createLinearGradient(0, 0, canvasWidth, canvasHeight);
            bg.stops.forEach((stop, i) => {
                gradient.addColorStop(i / (bg.stops.length - 1), stop);
            });
            ctx.fillStyle = gradient;
            ctx.fillRect(0, 0, canvasWidth, canvasHeight);
        }
    }
    // If background is transparent/none, leave the canvas transparent

    // Window rectangle
    const wx = padding;
    const wy = padding;
    drawRoundedRect(ctx, wx, wy, windowWidth, windowHeight, borderRadius);
    ctx.fillStyle = colours.windowBg;
    ctx.fill();

    // Drop shadow
    ctx.save();
    ctx.shadowColor = 'rgba(0, 0, 0, 0.35)';
    ctx.shadowBlur = 40;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 12;
    drawRoundedRect(ctx, wx, wy, windowWidth, windowHeight, borderRadius);
    ctx.fillStyle = colours.windowBg;
    ctx.fill();
    ctx.restore();

    // Re-draw window on top (shadow may have bled)
    drawRoundedRect(ctx, wx, wy, windowWidth, windowHeight, borderRadius);
    ctx.fillStyle = colours.windowBg;
    ctx.fill();

    // Title bar background
    if (titleBarHeight > 0) {
        ctx.save();
        drawRoundedRect(ctx, wx, wy, windowWidth, titleBarHeight, borderRadius);
        // Clip bottom corners of title bar to be square
        ctx.rect(wx, wy + borderRadius, windowWidth, titleBarHeight - borderRadius);
        ctx.clip('evenodd');
        ctx.fillStyle = colours.titleBar;
        ctx.fill();
        ctx.restore();

        // Simpler approach: fill title bar, then redraw top rounded corners
        ctx.save();
        ctx.beginPath();
        ctx.rect(wx, wy, windowWidth, titleBarHeight);
        ctx.clip();
        drawRoundedRect(ctx, wx, wy, windowWidth, windowHeight, borderRadius);
        ctx.fillStyle = colours.titleBar;
        ctx.fill();
        ctx.restore();
    }

    // Window controls (macOS dots)
    if (windowControls && titleBarHeight > 0) {
        const dotY = wy + titleBarHeight / 2;
        const dotX = wx + 20;
        const dotRadius = 6;
        const dotSpacing = 20;
        const dotColours = ['#FF5F57', '#FEBC2E', '#28C840'];
        dotColours.forEach((colour, i) => {
            ctx.beginPath();
            ctx.arc(dotX + i * dotSpacing, dotY, dotRadius, 0, Math.PI * 2);
            ctx.fillStyle = colour;
            ctx.fill();
        });
    }

    // Title text
    if (title && titleBarHeight > 0) {
        ctx.font = `13px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif`;
        ctx.fillStyle = colours.titleText;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(title, wx + windowWidth / 2, wy + titleBarHeight / 2);
        ctx.textAlign = 'left';
    }

    // Code area - clip to rounded rect
    ctx.save();
    const codeAreaY = wy + titleBarHeight;
    drawRoundedRect(ctx, wx, wy, windowWidth, windowHeight, borderRadius);
    ctx.clip();

    ctx.font = font;
    ctx.textBaseline = 'top';

    const textStartX = wx + codePadding + lineNumWidth;
    const textStartY = codeAreaY + codePadding;

    lines.forEach((line, index) => {
        const y = textStartY + index * lineHeight + (lineHeight - fontSize) / 2;

        // Line number
        if (lineNumbers) {
            ctx.fillStyle = colours.lineNumber;
            const numText = String(index + 1).padStart(String(lines.length).length, ' ');
            ctx.fillText(numText, wx + codePadding, y);
        }

        // Syntax-highlighted tokens
        const tokens = tokenise(line);
        let x = textStartX;
        for (const token of tokens) {
            ctx.fillStyle = colours[token.type] || colours.default;
            ctx.fillText(token.text, x, y);
            x += ctx.measureText(token.text).width;
        }
    });

    ctx.restore();

    return canvas.toDataURL('image/png');
}

export function downloadImage(dataUrl, filename) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = filename || 'code-screenshot.png';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

export async function copyImageToClipboard(dataUrl) {
    try {
        const response = await fetch(dataUrl);
        const blob = await response.blob();
        await navigator.clipboard.write([
            new ClipboardItem({ 'image/png': blob })
        ]);
        return true;
    } catch {
        return false;
    }
}
