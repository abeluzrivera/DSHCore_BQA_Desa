/**
 * DS Form — Helpers de formularios
 * Requiere: ds-core.js, ds-api.js, ds-notifications.js
 *
 * API:
 *   // Validación manual
 *   DS.form.validate('#mi-form')  // retorna true/false y marca campos
 *
 *   // Serializar formulario como objeto
 *   DS.form.serialize('#mi-form')  // => { nombre: 'juan', ... }
 *
 *   // Submit con fetch automático
 *   DS.form.submitAjax('#mi-form', {
 *     onSuccess: (data) => { DS.modal.close(); DS.events.emit('cliente:creado', data); },
 *     onError: (err) => {}
 *   })
 *
 *   // Poblar formulario desde objeto
 *   DS.form.fill('#mi-form', { nombre: 'Pedro', email: 'p@p.cl' })
 *
 *   // Limpiar y reiniciar validaciones
 *   DS.form.reset('#mi-form')
 *
 *   // Deshabilitar/habilitar todos los inputs
 *   DS.form.setLoading('#mi-form', true)
 */
DS.modules.register('form', function () {

    /**
     * Valida un formulario según atributos HTML5 (required, minlength, pattern, etc.)
     * y agrega/quita clases ds-input.is-invalid.
     */
    function validate(formSelector) {
        const form = typeof formSelector === 'string'
            ? document.querySelector(formSelector)
            : formSelector;

        if (!form) return false;

        let valid = true;

        form.querySelectorAll('[required], [data-ds-validate]').forEach(field => {
            clearError(field);

            let fieldValid = true;
            const value = field.value?.trim() ?? '';

            if (field.hasAttribute('required') && !value) {
                fieldValid = false;
                setError(field, field.dataset.errorRequired ?? 'Este campo es obligatorio');
            } else if (field.type === 'email' && value && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
                fieldValid = false;
                setError(field, field.dataset.errorEmail ?? 'Email inválido');
            } else if (field.hasAttribute('minlength') && value.length < +field.getAttribute('minlength')) {
                fieldValid = false;
                setError(field, field.dataset.errorMin ?? `Mínimo ${field.getAttribute('minlength')} caracteres`);
            } else if (field.hasAttribute('pattern') && value && !new RegExp(field.getAttribute('pattern')).test(value)) {
                fieldValid = false;
                setError(field, field.dataset.errorPattern ?? 'Formato inválido');
            }

            if (!fieldValid) valid = false;
        });

        return valid;
    }

    function setError(field, message) {
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
        let errorEl = field.parentElement?.querySelector('.ds-field-error');
        if (!errorEl) {
            errorEl = document.createElement('span');
            errorEl.className = 'ds-field-error';
            field.after(errorEl);
        }
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    }

    function clearError(field) {
        field.classList.remove('is-invalid', 'is-valid');
        const errorEl = field.parentElement?.querySelector('.ds-field-error');
        if (errorEl) errorEl.style.display = 'none';
    }

    /** Serializa FormData como objeto plano */
    function serialize(formSelector) {
        const form = typeof formSelector === 'string'
            ? document.querySelector(formSelector)
            : formSelector;

        if (!form) return {};
        const data = {};
        new FormData(form).forEach((value, key) => {
            // Soporte de campos múltiples (checkboxes)
            if (key in data) {
                data[key] = [].concat(data[key], value);
            } else {
                data[key] = value;
            }
        });
        return data;
    }

    /**
     * Envía un formulario via AJAX, validando antes.
     * Lee action y method del <form>.
     */
    async function submitAjax(formSelector, options = {}) {
        const form = typeof formSelector === 'string'
            ? document.querySelector(formSelector)
            : formSelector;

        if (!form) return;

        const { onSuccess, onError, silent = false } = options;

        if (!validate(form)) {
            if (!silent) DS.notify?.warning('Por favor corrige los campos marcados');
            return;
        }

        const url    = form.action || window.location.href;
        const method = (form.getAttribute('method') ?? 'POST').toUpperCase();
        const body   = serialize(form);

        setLoading(form, true);

        const result = method === 'GET'
            ? await DS.api.get(`${url}?${DS.utils.toQueryString(body)}`)
            : await DS.api[method.toLowerCase()](url, body);

        setLoading(form, false);

        if (result.ok) {
            if (typeof onSuccess === 'function') onSuccess(result.data);
        } else {
            if (typeof onError === 'function') onError(result.error);
        }

        return result;
    }

    /** Rellena un formulario con un objeto de datos */
    function fill(formSelector, data) {
        const form = typeof formSelector === 'string'
            ? document.querySelector(formSelector)
            : formSelector;

        if (!form) return;

        Object.entries(data).forEach(([key, value]) => {
            const field = form.querySelector(`[name="${key}"]`);
            if (!field) return;

            if (field.type === 'checkbox') {
                field.checked = Boolean(value);
            } else if (field.type === 'radio') {
                const radio = form.querySelector(`[name="${key}"][value="${value}"]`);
                if (radio) radio.checked = true;
            } else {
                field.value = value ?? '';
            }
        });
    }

    /** Resetea form y limpia errores de validación */
    function reset(formSelector) {
        const form = typeof formSelector === 'string'
            ? document.querySelector(formSelector)
            : formSelector;

        if (!form) return;
        form.reset();
        form.querySelectorAll('.is-invalid, .is-valid').forEach(el => {
            el.classList.remove('is-invalid', 'is-valid');
        });
        form.querySelectorAll('.ds-field-error').forEach(el => {
            el.style.display = 'none';
        });
    }

    /** Deshabilita (loading=true) o habilita todos los controles del form */
    function setLoading(formSelector, loading) {
        const form = typeof formSelector === 'string'
            ? document.querySelector(formSelector)
            : formSelector;

        if (!form) return;

        form.querySelectorAll('input, select, textarea, button').forEach(el => {
            el.disabled = loading;
        });

        // Loading state en el botón submit
        const submitBtn = form.querySelector('[type="submit"], button[data-submit]');
        if (submitBtn) submitBtn.classList.toggle('is-loading', loading);
    }

    DS.form = { validate, serialize, submitAjax, fill, reset, setLoading };
});
