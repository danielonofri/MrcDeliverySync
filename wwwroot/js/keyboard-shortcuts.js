// Captura global de atajos de teclado para Mrc Delivery Sync.
let dotNetHelperReference = null;

window.registerKeyboardShortcuts = function (dotNetHelper) {
    dotNetHelperReference = dotNetHelper;

    // Evitar registrar múltiples event listeners
    if (!window.isKeyboardListenerRegistered) {
        window.isKeyboardListenerRegistered = true;

        document.addEventListener('keydown', function (e) {
            // 1. Validar que e.key exista para evitar la excepción
            if (!e.key) return;

            // 2. Ignorar si el usuario está tipeando en un input, textarea, select o elemento editable
            const activeEl = document.activeElement;
            if (activeEl) {
                const tag = activeEl.tagName ? activeEl.tagName.toUpperCase() : '';
                const isContentEditable = activeEl.isContentEditable;

                if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || isContentEditable) {
                    return;
                }
            }

            // 3. Conversión segura a mayúsculas
            const key = e.key.toUpperCase();
            if (['A', 'R', 'E'].includes(key)) {
                e.preventDefault();
                if (dotNetHelperReference) {
                    dotNetHelperReference.invokeMethodAsync('OnShortcutKeyPressed', key);
                }
            }
        });
    }
};