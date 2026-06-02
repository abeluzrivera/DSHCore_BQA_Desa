/**
 * DS Notifications — Sistema de toasts y alertas
 * Requiere: ds-core.js
 *
 * API:
 *   DS.notify.success('Guardado correctamente')
 *   DS.notify.error('No se pudo guardar', { duration: 6000 })
 *   DS.notify.warning('Revisa los datos')
 *   DS.notify.info('Procesando...')
 *   DS.notify.confirm('¿Confirmar eliminar?', { onConfirm: () => borrar() })
 */
DS.modules.register('notifications', function () {

    // Crear contenedor si no existe
    let container = document.getElementById('ds-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'ds-toast-container';
        document.body.appendChild(container);
    }

    const ICONS = {
        success: 'check_circle',
        error:   'error',
        warning: 'warning',
        info:    'info',
    };

    /**
     * Muestra un toast.
     * @param {'success'|'error'|'warning'|'info'} type
     * @param {string} message
     * @param {{ duration?: number, title?: string }} options
     */
    function show(type, message, options = {}) {
        const { duration = 4000, title } = options;

        const toast = document.createElement('div');
        toast.className = `ds-toast ds-toast--${type}`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <span class="ds-toast__icon material-symbols-outlined">${ICONS[type] ?? 'notifications'}</span>
            <div class="ds-toast__message">
                ${title ? `<strong>${DS.utils.escapeHtml(title)}</strong><br>` : ''}
                ${DS.utils.escapeHtml(message)}
            </div>
        `;

        container.appendChild(toast);
        DS.events.emit('notify:shown', { type, message });

        // Auto-dismiss
        if (duration > 0) {
            setTimeout(() => dismiss(toast), duration);
        }

        return toast;
    }

    function dismiss(toast) {
        toast.classList.add('is-leaving');
        toast.addEventListener('animationend', () => toast.remove(), { once: true });
    }

    // Hacer clic en toast lo cierra
    container.addEventListener('click', function (e) {
        const toast = e.target.closest('.ds-toast');
        if (toast) dismiss(toast);
    });

    /**
     * Modal de confirmación nativo del DS (no window.confirm).
     * @param {string} message
     * @param {{ title?: string, confirmText?: string, cancelText?: string,
     *            type?: 'danger'|'warning', onConfirm: Function, onCancel?: Function }} options
     */
    function confirm(message, options = {}) {
        const {
            title = '¿Confirmar acción?',
            confirmText = 'Confirmar',
            cancelText = 'Cancelar',
            type = 'danger',
            onConfirm,
            onCancel,
        } = options;

        // Si ya existe un confirm DS, no apilar
        const existing = document.getElementById('ds-confirm-modal');
        if (existing) existing.remove();

        const backdrop = document.createElement('div');
        backdrop.id = 'ds-confirm-modal';
        backdrop.className = 'ds-modal-backdrop';
        backdrop.innerHTML = `
            <div class="ds-modal ds-modal--sm" role="dialog" aria-modal="true" aria-labelledby="ds-confirm-title">
                <div class="ds-modal__header">
                    <h2 class="ds-modal__title" id="ds-confirm-title">${DS.utils.escapeHtml(title)}</h2>
                </div>
                <div class="ds-modal__body">
                    <p style="margin:0; color: var(--color-text-secondary);">${DS.utils.escapeHtml(message)}</p>
                </div>
                <div class="ds-modal__footer">
                    <button class="ds-btn ds-btn--ghost" data-action="cancel">${DS.utils.escapeHtml(cancelText)}</button>
                    <button class="ds-btn ds-btn--${type}" data-action="confirm">${DS.utils.escapeHtml(confirmText)}</button>
                </div>
            </div>
        `;

        document.body.appendChild(backdrop);

        backdrop.querySelector('[data-action="confirm"]').addEventListener('click', () => {
            backdrop.remove();
            if (typeof onConfirm === 'function') onConfirm();
        });

        backdrop.querySelector('[data-action="cancel"]').addEventListener('click', () => {
            backdrop.remove();
            if (typeof onCancel === 'function') onCancel();
        });

        backdrop.addEventListener('click', (e) => {
            if (e.target === backdrop) {
                backdrop.remove();
                if (typeof onCancel === 'function') onCancel();
            }
        });

        document.addEventListener('keydown', function esc(e) {
            if (e.key === 'Escape') { backdrop.remove(); document.removeEventListener('keydown', esc); }
        });
    }

    // Exponer en namespace DS
    DS.notify = {
        success: (msg, opts) => show('success', msg, opts),
        error:   (msg, opts) => show('error',   msg, opts),
        warning: (msg, opts) => show('warning', msg, opts),
        info:    (msg, opts) => show('info',    msg, opts),
        confirm,
    };
});
