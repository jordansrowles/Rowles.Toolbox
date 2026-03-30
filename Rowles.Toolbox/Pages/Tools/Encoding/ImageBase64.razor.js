export async function readFileAsDataUrl(inputElement) {
    const file = inputElement.files?.[0];
    if (!file) return null;

    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve({
            dataUrl: reader.result,
            name: file.name,
            size: file.size,
            type: file.type
        });
        reader.onerror = () => reject('Failed to read file');
        reader.readAsDataURL(file);
    });
}
