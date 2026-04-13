window.getLocalStorage = (key) => localStorage.getItem(key);
window.setLocalStorage = (key, value) => localStorage.setItem(key, value);

window.registerUndoShortcut = function (dotNetRef) {
    const handler = function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'z' && !e.shiftKey) {
            const tag = document.activeElement?.tagName;
            if (tag === 'INPUT' || tag === 'TEXTAREA' || document.activeElement?.isContentEditable) return;
            e.preventDefault();
            dotNetRef.invokeMethodAsync('OnCtrlZ');
        }
    };
    document.addEventListener('keydown', handler);
    return handler;
};

window.unregisterUndoShortcut = function (handler) {
    if (handler) document.removeEventListener('keydown', handler);
};

window.attachImagePaste = function (textarea, uploadUrl) {
    if (!textarea || textarea._imagePasteAttached) return;
    textarea._imagePasteAttached = true;

    textarea.addEventListener('paste', async function (e) {
        const items = e.clipboardData?.items;
        if (!items) return;

        for (const item of items) {
            if (!item.type.startsWith('image/')) continue;

            e.preventDefault();
            const ext = item.type.split('/')[1].split('+')[0] || 'png';
            const file = item.getAsFile();
            const fd = new FormData();
            fd.append('file', file, `paste.${ext}`);

            try {
                const resp = await fetch(uploadUrl, { method: 'POST', body: fd });
                if (!resp.ok) { console.error('Image upload failed', resp.status); return; }
                const data = await resp.json();
                const md = `![](${data.url})`;
                const start = textarea.selectionStart;
                const end = textarea.selectionEnd;
                textarea.value = textarea.value.slice(0, start) + md + textarea.value.slice(end);
                textarea.selectionStart = textarea.selectionEnd = start + md.length;
                textarea.dispatchEvent(new Event('input', { bubbles: true }));
            } catch (err) {
                console.error('Image upload error', err);
            }
            break;
        }
    });
};
