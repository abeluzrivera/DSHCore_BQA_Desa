/**
 * DS Modal — Gestión de modales dinámicos
 * Requiere: ds-core.js
 *
 * API:
 *   // Abrir modal declarativo (id del elemento en DOM)
 *   DS.modal.open('modal-crear-cliente')
 *
 *   // Crear modal dinámico por JS
 *   DS.modal.create({
 *     title: 'Crear cliente',
 *     size: 'lg',           // sm | md (default) | lg | xl
 *     body: '<p>...</p>',   // HTML o Element
 *     footer: [             // botones opcionales
 *       { text: 'Cancelar', variant: 'ghost', action: 'cancel' },
 *       { text: 'Guardar',  variant: 'primary', action: 'confirm', id: 'btn-save' }
 *     ],
 *     onConfirm: () => {},
 *     onCancel:  () => {},
 *   })
 *
 *   DS.modal.close()          // cierra el modal más reciente
 *   DS.modal.closeAll()
 */
DS.modules.register('modal', function () {

    const stack = [];

    function buildFooterBtns(buttons, onConfirm, onCancel) {
        return buttons.map(btn => {
            const { text, variant = 'ghost', action, id = '' } = btn;
            return `<button class="ds-btn ds-btn--${variant}" data-action="${action}" ${id ? `id="${id}"` : ''}>
                        ${DS.utils.escapeHtml(text)}
                    </button>`;
        }).join('');
    }

    function create(options = {}) {
        const {
            title = '',
            size = '',
            body = '',
            footer,
            onConfirm,
            onCancel,
            id,
            closable = true,
        } = options;

        const backdrop = document.createElement('div');
        backdrop.className = 'ds-modal-backdrop';
        if (id) backdrop.id = id;

        const sizeClass = size ? `ds-modal--${size}` : '';
        const footerHtml = footer
            ? `<div class="ds-modal__footer">${buildFooterBtns(footer, onConfirm, onCancel)}</div>`
            : '';

        backdrop.innerHTML = `
            <div class="ds-modal ${sizeClass}" role="dialog" aria-modal="true">
                <div class="ds-modal__header">
                    <h2 class="ds-modal__title">${DS.utils.escapeHtml(title)}</h2>
                    ${closable ? `<button class="ds-modal__close" aria-label="Cerrar" data-action="cancel">
                        <span class="material-symbols-outlined" style="font-size:1.2rem">close</span>
                    </button>` : ''}
                </div>
                <div class="ds-modal__body"></div>
                ${footerHtml}
            </div>
        `;

        // Insertar body (puede ser HTML string o Element)
        const bodyEl = backdrop.querySelector('.ds-modal__body');
        if (body instanceof Element) {
            bodyEl.appendChild(body);
        } else {
            bodyEl.innerHTML = body;
        }

        document.body.appendChild(backdrop);
        stack.push(backdrop);
        document.body.style.overflow = 'hidden';

        DS.events.emit('modal:opened', { title });
        focusFirst(backdrop);

        // Eventos
        backdrop.addEventListener('click', function (e) {
            const action = e.target.closest('[data-action]')?.dataset.action;
            if (action === 'cancel' || (e.target === backdrop && closable)) {
                DS.modal.close();
                if (typeof onCancel === 'function') onCancel();
            } else if (action === 'confirm') {
                if (typeof onConfirm === 'function') onConfirm(backdrop);
            }
        });

        // Esc para cerrar
        const onEsc = (e) => {
            if (e.key === 'Escape' && closable) {
                DS.modal.close();
                if (typeof onCancel === 'function') onCancel();
                document.removeEventListener('keydown', onEsc);
            }
        };
        document.addEventListener('keydown', onEsc);
        backdrop._dsEscHandler = onEsc;

        return backdrop;
    }

    function open(modalId) {
        const el = document.getElementById(modalId);
        if (!el) { console.warn(`[DS.modal] No encontrado: #${modalId}`); return; }
        el.style.display = 'flex';
        stack.push(el);
        document.body.style.overflow = 'hidden';
        DS.events.emit('modal:opened', { id: modalId });
    }

    function close() {
        const modal = stack.pop();
        if (!modal) return;
        if (modal._dsEscHandler) document.removeEventListener('keydown', modal._dsEscHandler);
        modal.classList.add('is-leaving');
        
        // Si es modal declarativo (tiene ID), solo ocultarlo; si es dinámico, removerlo
        const isDeclarative = modal.id;
        
        setTimeout(() => {
            if (isDeclarative) {
                // Modal declarativo: solo ocultar (preservar en DOM para reutilizar)
                modal.style.display = 'none';
            } else {
                // Modal dinámico: remover del DOM
                modal.remove();
            }
            // Restaurar scroll solo si no hay más modales en la pila
            if (stack.length === 0) {
                document.body.style.overflow = '';
            }
        }, 300);
        DS.events.emit('modal:closed');
    }

    function closeAll() {
        while (stack.length) close();
    }

    function focusFirst(container) {
        const focusable = container.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
        if (focusable) setTimeout(() => focusable.focus(), 100);
    }

    DS.modal = { create, open, close, closeAll };
});
