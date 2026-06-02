/**
 * DS Core — Núcleo del Design System
 * Expone el namespace global `window.DS`
 *
 * Incluye:
 *   DS.events  — EventBus desacoplado entre módulos
 *   DS.utils   — Utilidades generales (debounce, formateo, DOM)
 *   DS.state   — Estado reactivo simple
 */
(function (global) {
    'use strict';

    // ─── EventBus ─────────────────────────────────────────────
    // Permite comunicación entre módulos sin acoplamiento directo.
    // Uso:
    //   DS.events.on('cliente:creado', (data) => { ... })
    //   DS.events.emit('cliente:creado', { id: 5 })
    //   DS.events.off('cliente:creado', handler)
    const EventBus = (function () {
        const listeners = {};

        return {
            on(event, fn) {
                if (!listeners[event]) listeners[event] = [];
                listeners[event].push(fn);
                return () => this.off(event, fn); // retorna unsub fn
            },
            off(event, fn) {
                if (!listeners[event]) return;
                listeners[event] = listeners[event].filter(h => h !== fn);
            },
            emit(event, data) {
                (listeners[event] || []).forEach(fn => {
                    try { fn(data); }
                    catch (e) { console.error(`[DS.events] Error en handler '${event}':`, e); }
                });
            },
            once(event, fn) {
                const wrapper = (data) => { fn(data); this.off(event, wrapper); };
                this.on(event, wrapper);
            }
        };
    })();

    // ─── Utils ────────────────────────────────────────────────
    const utils = {
        /**
         * Debounce: retrasa la ejecución mientras sigan llegando llamadas.
         * Uso: DS.utils.debounce(() => buscar(), 300)
         */
        debounce(fn, delay = 300) {
            let timer;
            return function (...args) {
                clearTimeout(timer);
                timer = setTimeout(() => fn.apply(this, args), delay);
            };
        },

        /**
         * Throttle: limita a 1 ejecución por intervalo.
         */
        throttle(fn, limit = 200) {
            let last = 0;
            return function (...args) {
                const now = Date.now();
                if (now - last >= limit) {
                    last = now;
                    fn.apply(this, args);
                }
            };
        },

        /** Formatea número como moneda CLP o según locale */
        formatCurrency(value, currency = 'CLP', locale = 'es-CL') {
            return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(value);
        },

        /** Formatea número con separadores */
        formatNumber(value, locale = 'es-CL') {
            return new Intl.NumberFormat(locale).format(value);
        },

        /** Formatea fecha */
        formatDate(dateStr, opts = { day: '2-digit', month: '2-digit', year: 'numeric' }) {
            if (!dateStr) return '—';
            return new Intl.DateTimeFormat('es-CL', opts).format(new Date(dateStr));
        },

        /** Trunca texto con ellipsis */
        truncate(str, max = 40) {
            if (!str) return '';
            return str.length > max ? str.slice(0, max - 1) + '…' : str;
        },

        /** Escapa HTML para evitar XSS al insertar texto */
        escapeHtml(str) {
            const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
            return String(str).replace(/[&<>"']/g, c => map[c]);
        },

        /**
         * Selector seguro — lanza warning si no encuentra el elemento.
         * @returns {Element|null}
         */
        qs(selector, context = document) {
            const el = context.querySelector(selector);
            if (!el) console.warn(`[DS.utils.qs] No se encontró: "${selector}"`);
            return el;
        },

        qsAll(selector, context = document) {
            return Array.from(context.querySelectorAll(selector));
        },

        /** Lee el anti-forgery token de ASP.NET para fetch POST */
        getAntiForgeryToken() {
            const el = document.querySelector('input[name="__RequestVerificationToken"]');
            return el ? el.value : '';
        },

        /** Construye query string desde un objeto */
        toQueryString(params) {
            return new URLSearchParams(
                Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== '')
            ).toString();
        },

        /** Sleep (para tests o UX deliberada) */
        sleep(ms) {
            return new Promise(resolve => setTimeout(resolve, ms));
        }
    };

    // ─── Estado reactivo simple ────────────────────────────────
    // Permite que componentes reaccionen a cambios de estado global.
    // Uso:
    //   DS.state.set('filtroActivo', 'pendiente')
    //   DS.state.get('filtroActivo')   // => 'pendiente'
    //   DS.events.on('state:filtroActivo', (val) => ...)
    const state = (function () {
        const _store = {};
        return {
            get(key) { return _store[key]; },
            set(key, value) {
                const prev = _store[key];
                _store[key] = value;
                if (prev !== value) EventBus.emit(`state:${key}`, value);
            },
            getAll() { return { ..._store }; }
        };
    })();

    // ─── Contact Filter ───────────────────────────────────────────────────────
    // Filtro genérico de contactabilidad por estado (verified / pending / error).
    // Funciona con items que tienen [data-contact-state="verified|pending|error"].
    // El Dashboard usa su propia implementación; este módulo centraliza la lógica
    // para páginas como Clientes donde el filter bar es ds-contact-filter.
    //
    // Uso:
    //   DS.contactFilter.init(document.querySelector('.cp-content'));
    //   DS.contactFilter.reapply(scopeEl);  // tras verify / reject
    const contactFilter = (() => {
        const BAR_SEL     = '.ds-contact-filter';
        const ITEM_SEL    = '[data-contact-state]';
        const SECTION_SEL = '.cp-section';

        const EMPTY_LABEL = {
            pendiente:  'Sin contactos pendientes',
            verificado: 'Sin contactos verificados',
            error:      'Sin contactos con error',
        };
        const EMPTY_ICON = {
            pendiente:  'hourglass_empty',
            verificado: 'verified',
            error:      'error_outline',
        };

        function buildBarHTML() {
            return `<div class="ds-contact-filter" role="group" aria-label="Filtrar contactos por estado">
                <button class="ds-contact-filter__btn ds-contact-filter__btn--active" data-filter="todos" type="button" aria-pressed="true">
                    <span class="material-symbols-outlined" aria-hidden="true">filter_list</span>Todos
                </button>
                <button class="ds-contact-filter__btn" data-filter="pendiente" type="button" aria-pressed="false">
                    <span class="material-symbols-outlined" aria-hidden="true">hourglass_empty</span>Pendientes
                    <span class="ds-contact-filter__count" aria-hidden="true"></span>
                </button>
                <button class="ds-contact-filter__btn" data-filter="verificado" type="button" aria-pressed="false">
                    <span class="material-symbols-outlined" aria-hidden="true">verified</span>Verificados
                    <span class="ds-contact-filter__count" aria-hidden="true"></span>
                </button>
                <button class="ds-contact-filter__btn" data-filter="error" type="button" aria-pressed="false">
                    <span class="material-symbols-outlined" aria-hidden="true">error_outline</span>Con error
                    <span class="ds-contact-filter__count" aria-hidden="true"></span>
                </button>
            </div>`;
        }

        function _apply(scopeEl, filter, sectionSelector = SECTION_SEL) {
            const items = scopeEl.querySelectorAll(ITEM_SEL);
            items.forEach(item => {
                const st   = item.dataset.contactState ?? 'pending';
                const show =
                    filter === 'todos'       ||
                    (filter === 'verificado' && st === 'verified')           ||
                    (filter === 'error'      && st === 'error')              ||
                    (filter === 'pendiente'  && st !== 'verified' && st !== 'error');
                item.style.display = show ? '' : 'none';
            });
            // Ocultar secciones cuyas items estén todos ocultos
            if (sectionSelector) {
                scopeEl.querySelectorAll(sectionSelector).forEach(sec => {
                    const hasVisible = Array.from(sec.querySelectorAll(ITEM_SEL))
                        .some(it => it.style.display !== 'none');
                    sec.style.display = hasVisible ? '' : 'none';
                });
            }
            // Vacío: mostrar mensaje si el filtro no tiene resultados
            let emptyEl = scopeEl.querySelector('.ds-contact-filter__empty');
            const anyVisible = Array.from(items).some(it => it.style.display !== 'none');
            if (!anyVisible && filter !== 'todos') {
                if (!emptyEl) {
                    emptyEl = document.createElement('div');
                    emptyEl.className = 'ds-contact-filter__empty';
                    scopeEl.appendChild(emptyEl);
                }
                emptyEl.innerHTML =
                    `<span class="material-symbols-outlined">${EMPTY_ICON[filter] ?? 'filter_list'}</span>` +
                    `<span>${EMPTY_LABEL[filter] ?? 'Sin resultados'}</span>`;
                emptyEl.hidden = false;
            } else if (emptyEl) {
                emptyEl.hidden = true;
            }
        }

        function _updateCounts(scopeEl) {
            const filterBar = scopeEl?.querySelector(BAR_SEL);
            if (!filterBar) return;
            const items = Array.from(scopeEl.querySelectorAll(ITEM_SEL));
            const cnt = {
                pendiente:  items.filter(it => it.dataset.contactState !== 'verified' && it.dataset.contactState !== 'error').length,
                verificado: items.filter(it => it.dataset.contactState === 'verified').length,
                error:      items.filter(it => it.dataset.contactState === 'error').length,
            };
            Object.entries(cnt).forEach(([f, n]) => {
                const el = filterBar.querySelector(`[data-filter="${f}"] .ds-contact-filter__count`);
                if (el) el.textContent = n > 0 ? String(n) : '';
            });
        }

        /** Inicializa el filter bar dentro de scopeEl y computa los conteos.
         *  @param {Element} scopeEl
         *  @param {{ sectionSelector?: string|null }} options
         *    sectionSelector: selector CSS de secciones a ocultar cuando todos sus items
         *    están filtrados. Null = sin ocultado de secciones.
         *    Default '.cp-section'. Dashboard usa '.dash-detail__section'.
         */
        function init(scopeEl, { sectionSelector = SECTION_SEL } = {}) {
            if (!scopeEl) return;
            const filterBar = scopeEl.querySelector(BAR_SEL);
            if (!filterBar) return;
            // Clonar para limpiar listeners anteriores (re-init tras section reload)
            const newBar = filterBar.cloneNode(true);
            filterBar.replaceWith(newBar);
            // Persistir sectionSelector en el bar para que reapply pueda leerlo
            newBar.dataset.sectionSel = sectionSelector ?? '';
            newBar.addEventListener('click', e => {
                const btn = e.target.closest('.ds-contact-filter__btn');
                if (!btn) return;
                const filter = btn.dataset.filter ?? 'todos';
                const secSel = newBar.dataset.sectionSel || null;
                newBar.querySelectorAll('.ds-contact-filter__btn').forEach(b => {
                    b.classList.remove('ds-contact-filter__btn--active');
                    b.setAttribute('aria-pressed', 'false');
                });
                btn.classList.add('ds-contact-filter__btn--active');
                btn.setAttribute('aria-pressed', 'true');
                _apply(scopeEl, filter, secSel);
            });
            _updateCounts(scopeEl);
        }

        /** Re-aplica el filtro activo y actualiza conteos. Llamar tras verify / reject / reload. */
        function reapply(scopeEl) {
            if (!scopeEl) return;
            const bar = scopeEl.querySelector(BAR_SEL);
            const secSel = bar?.dataset.sectionSel || null;
            const activeFilter = bar?.querySelector('.ds-contact-filter__btn--active')?.dataset.filter ?? 'todos';
            _apply(scopeEl, activeFilter, secSel);
            _updateCounts(scopeEl);
        }

        return { buildBarHTML, init, reapply };
    })();

    // ─── Registro de módulos ───────────────────────────────────
    // Cada módulo DS se registra aquí para auto-inicialización.
    const _modules = {};
    const modules = {
        register(name, initFn) {
            _modules[name] = initFn;
        },
        initAll() {
            Object.entries(_modules).forEach(([name, fn]) => {
                try { fn(); }
                catch (e) { console.error(`[DS] Error al init módulo '${name}':`, e); }
            });
        }
    };

    // ─── Namespace público ─────────────────────────────────────
    // ✅ Preserva DS.endpoints (y cualquier otra propiedad preexistente)
    global.DS = Object.assign(global.DS || {}, { events: EventBus, utils, state, contactFilter, modules });

    // Auto-inicializar módulos cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => DS.modules.initAll());
    } else {
        DS.modules.initAll();
    }

})(window);
