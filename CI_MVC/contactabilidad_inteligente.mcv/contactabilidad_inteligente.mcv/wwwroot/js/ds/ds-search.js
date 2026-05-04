/* =====================================================
   DS.search — Módulo de búsqueda global de clientes
   dash_client_Searching · v1.0
   Depende de: ds-core.js, ds-api.js
   ===================================================== */

(function () {
    'use strict';

    /* ──────────────────────────────────────────────────
       Estado del módulo
    ────────────────────────────────────────────────── */
    const state = {
        query:       '',
        filter:      'todos',          // 'todos' | 'por-verificar' | 'verificados'
        results:     [],
        focusIndex:  -1,
        isOpen:      false,
        isLoading:   false,
    };

    /* ──────────────────────────────────────────────────
       Selectores (cacheados)
    ────────────────────────────────────────────────── */
    let $widget, $input, $clear, $dropdown, $list, $empty,
        $filters, $countBadge, $pager, $listSearch, $listBody;

    /* ──────────────────────────────────────────────────
       Init
    ────────────────────────────────────────────────── */
    function init() {
        $widget   = document.getElementById('ds-search-widget');
        if (!$widget) return;                          // componente no montado

        $input    = document.getElementById('ds-search-input');
        $clear    = document.getElementById('ds-search-clear');
        $dropdown = document.getElementById('ds-search-dropdown');
        $list     = document.getElementById('ds-search-results');
        $empty    = document.getElementById('ds-search-empty');
        $filters  = $widget.querySelectorAll('.ds-search-filters__pill');

        // Client list (aside) – puede no estar en la misma página
        $countBadge = document.getElementById('ds-client-list-count');
        $pager      = document.getElementById('ds-client-list-pager');
        $listSearch = document.getElementById('ds-client-list-search');
        $listBody   = document.getElementById('ds-client-list-body');

        _bindSearchBar();
        _bindFilterPills();
        _bindListSearch();
        _bindKeyboard();
        _bindOutsideClick();

        // Carga inicial de la lista lateral (si existe)
        if ($listBody) _loadClientList();
    }

    /* ──────────────────────────────────────────────────
       Búsqueda global (dropdown)
    ────────────────────────────────────────────────── */
    function _bindSearchBar() {
        if (!$input) return;

        $input.addEventListener('input', DS.utils.debounce(async (e) => {
            state.query = e.target.value.trim();
            _toggleClear(state.query.length > 0);

            if (state.query.length < 2) {
                _closeDropdown();
                return;
            }
            await _fetchSuggestions(state.query);
        }, 300));

        $input.addEventListener('focus', () => {
            if (state.results.length > 0) _openDropdown();
        });

        if ($clear) {
            $clear.addEventListener('click', () => {
                $input.value = '';
                state.query  = '';
                _toggleClear(false);
                _closeDropdown();
                $input.focus();
            });
        }
    }

    async function _fetchSuggestions(query) {
        state.isLoading = true;
        _renderSkeleton();

        const { ok, data } = await DS.api.get(DS.endpoints.clients.search(query, 10));

        state.isLoading = false;

        if (!ok) {
            DS.notify.error('No se pudo realizar la búsqueda');
            _closeDropdown();
            return;
        }

        state.results   = data ?? [];
        state.focusIndex = -1;
        _renderResults(state.results);
        _openDropdown();
    }

    function _renderResults(items) {
        if (!$list || !$empty) return;

        if (!items.length) {
            $list.innerHTML = '';
            $empty.hidden   = false;
            return;
        }

        $empty.hidden  = false && true; // keep hidden
        $empty.hidden  = true;
        $list.innerHTML = items.map((c, i) => _buildResultHTML(c, i)).join('');

        // Bind click/keyboard actions on each row
        $list.querySelectorAll('.ds-search-result').forEach((row) => {
            const id = row.dataset.id;

            row.addEventListener('click', () => _handleSelectClient(id));
            row.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    _handleSelectClient(id);
                }
            });

            const addBtn = row.querySelector('.ds-search-result__add');
            if (addBtn) {
                addBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    _handleAddClient(id);
                });
            }
        });
    }

    function _buildResultHTML(cliente, index) {
        // Compute yaCargado client-side: store (real-time) con fallback a DS_FILTER_IDS inicial
        const yaCargado = DS.clientListStore?.has(String(cliente.id))
            ?? (Boolean(
                window.DS_FILTER_IDS &&
                window.DS_FILTER_IDS.split(',').map(s => s.trim()).includes(String(cliente.id))
            ) || Boolean(cliente.yaCargado));

        const resultId   = `ds-sr-${index}`;
        const initials   = _getInitials(cliente.nombreCompleto);
        const avatarType = cliente.tipo === 'empresa' ? 'company' : 'person';
        const avatarHTML = cliente.fotoUrl
            ? `<img src="${DS.utils.escapeHtml(cliente.fotoUrl)}" alt="${DS.utils.escapeHtml(cliente.nombreCompleto)}" />`
            : initials;

        const actionHTML = yaCargado
            ? `<span class="ds-search-result__loaded-badge" aria-label="Cliente ya cargado en la lista">YA CARGADO</span>`
            : `<button class="ds-search-result__add" aria-label="Agregar ${DS.utils.escapeHtml(cliente.nombreCompleto)}" title="Agregar cliente">
                   <span class="material-symbols-outlined" aria-hidden="true">add</span>
               </button>`;

        return `
            <li id="${resultId}"
                class="ds-search-result"
                role="option"
                tabindex="0"
                data-id="${DS.utils.escapeHtml(String(cliente.id))}"
                data-index="${index}"
                aria-selected="false">
                <div class="ds-search-result__avatar ds-search-result__avatar--${avatarType}"
                     aria-hidden="true">
                    ${avatarHTML}
                </div>
                <div class="ds-search-result__info">
                    <div class="ds-search-result__name">${DS.utils.escapeHtml(cliente.nombreCompleto)}</div>
                    <div class="ds-search-result__meta">
                        ${DS.utils.escapeHtml(cliente.identificacion)} · ${DS.utils.escapeHtml(cliente.email ?? '')}
                    </div>
                </div>
                ${actionHTML}
            </li>`;
    }

    function _renderSkeleton() {
        if (!$list) return;
        $empty.hidden   = true;
        $list.innerHTML = Array.from({ length: 3 }, () => `
            <li class="ds-search-result" style="pointer-events:none" aria-hidden="true">
                <div class="ds-search-result__avatar" style="background:#e5e7eb"></div>
                <div class="ds-search-result__info">
                    <div class="ds-skeleton-line" style="height:12px;width:120px;margin-bottom:6px"></div>
                    <div class="ds-skeleton-line" style="height:10px;width:80px"></div>
                </div>
            </li>`).join('');
        _openDropdown();
    }

    function _openDropdown() {
        if (!$dropdown || state.isOpen) return;
        $dropdown.removeAttribute('hidden');
        $input?.setAttribute('aria-expanded', 'true');
        state.isOpen = true;
    }

    function _closeDropdown() {
        if (!$dropdown) return;
        $dropdown.setAttribute('hidden', '');
        $input?.setAttribute('aria-expanded', 'false');
        $input?.removeAttribute('aria-activedescendant');
        state.isOpen    = false;
        state.focusIndex = -1;
    }

    function _toggleClear(visible) {
        if ($clear) $clear.style.display = visible ? 'flex' : 'none';
    }

    /* ──────────────────────────────────────────────────
       Filter Pills
    ────────────────────────────────────────────────── */
    function _bindFilterPills() {
        $filters.forEach((pill) => {
            pill.addEventListener('click', () => {
                $filters.forEach(p => {
                    p.classList.remove('ds-search-filters__pill--active');
                    p.setAttribute('aria-selected', 'false');
                });
                pill.classList.add('ds-search-filters__pill--active');
                pill.setAttribute('aria-selected', 'true');
                state.filter = pill.dataset.filter ?? 'todos';

                // Re-fetch if there's an active query
                if (state.query.length >= 2) _fetchSuggestions(state.query);

                // Also filter the sidebar list
                if ($listBody) _filterLocalList(state.filter);

                DS.events.emit('search:filterChanged', { filter: state.filter });
            });
        });
    }

    /* ──────────────────────────────────────────────────
       Client List Aside
    ────────────────────────────────────────────────── */
    async function _loadClientList(filter) {
        if (!$listBody) return;

        _showListLoading(true);

        const { ok, data } = await DS.api.get(
            DS.endpoints.clients.list(window.DS_FILTER_IDS || '', state.filter)
        );
        _showListLoading(false);

        if (!ok) { DS.notify.error('No se pudo cargar la lista de clientes'); return; }

        const clientes = data?.items ?? data ?? [];
        _setListCounts(data);
        _renderClientList(clientes);
    }

    function _renderClientList(items) {
        if (!$listBody) return;

        if (!items.length) {
            $listBody.innerHTML = `
                <div class="ds-client-list__loading-msg">Sin resultados</div>`;
            return;
        }

        $listBody.innerHTML = items.map(c => _buildArticleHTML(c)).join('');
        // Click / keyboard selection is handled via event delegation in the
        // consuming page (e.g. dashboard.js._bindClientRowSelection).
    }

    /**
     * Builds a single contact-type icon span using the ds-ci component.
     * @param {string} materialIcon - Material Symbols icon name (e.g. "phone")
     * @param {string} state        - "verified" | "pending" | "error"
     * @param {string} label        - Accessible label (e.g. "Teléfono")
     */
    function _buildContactIconHTML(materialIcon, state, label, iconGroup) {
        const STATE_CLASS = {
            verified: 'ds-ci--verified',
            pending:  'ds-ci--pending',
            error:    'ds-ci--error',
        };
        const STATE_LABEL = {
            verified: 'verificado',
            pending:  'pendiente',
            error:    'con error',
        };
        const cls        = STATE_CLASS[state]  ?? 'ds-ci--pending';
        const stateLabel = STATE_LABEL[state]  ?? 'pendiente';
        const safeLabel  = DS.utils.escapeHtml(label);
        const groupAttr  = iconGroup ? ` data-icon-group="${iconGroup}"` : '';

        return `<span class="material-symbols-outlined ds-client-article__contact-icon ds-ci ${cls}"
                      title="${safeLabel}"
                      aria-label="${safeLabel} - ${stateLabel}"
                      role="img"${groupAttr}>${materialIcon}</span>`;
    }

    /**
     * Builds the full contact-icons row HTML for a client object.
     * Consumes per-contact-type state fields when the API provides them
     * (estadoTelefono / estadoEmail / estadoDireccion); falls back to
     * "pending" (gray) until the API wires up those fields.
     * @param {object} c - cliente DTO from the API
     */
    function _buildContactIconsHTML(c) {
        const icons = [];
        if (c.hasPhone)   icons.push(_buildContactIconHTML('phone',       c.phoneStatus   ?? 'pending', 'Teléfono',  'phone'));
        if (c.hasEmail)   icons.push(_buildContactIconHTML('mail',         c.emailStatus   ?? 'pending', 'Email',     'email'));
        if (c.hasAddress) icons.push(_buildContactIconHTML('location_on',  c.addressStatus ?? 'pending', 'Dirección', 'address'));
        return icons.join('');
    }

    function _buildArticleHTML(c) {
        const initials   = _getInitials(c.fullName);
        const avatarType = c.type === 'empresa' ? 'company' : 'person';
        const avatarHTML = c.photoUrl
            ? `<img src="${DS.utils.escapeHtml(c.photoUrl)}" alt="" />`
            : initials;
        const hasRing    = !!c.photoUrl;
        const estado     = c.verificationStatus ?? (c.isVerified ? 'verificado' : 'por-verificar');

        return `
            <article class="ds-client-article"
                     role="button"
                     tabindex="0"
                     data-id="${DS.utils.escapeHtml(String(c.identification ?? c.id))}"
                     data-estado="${estado}"
                     aria-label="Seleccionar cliente ${DS.utils.escapeHtml(c.fullName)}">
                <div class="ds-client-article__avatar-wrap">
                    <div class="ds-client-article__avatar ds-client-article__avatar--${c.fotoUrl ? 'photo' : avatarType}"
                         aria-hidden="true">
                        ${avatarHTML}
                    </div>
                    ${hasRing ? '<div class="ds-client-article__avatar-ring" aria-hidden="true"></div>' : ''}
                </div>
                <div class="ds-client-article__info">
                    <div class="ds-client-article__top">
                        <span class="ds-client-article__name">${DS.utils.escapeHtml(c.fullName)}</span>
                        <span class="ds-client-article__id">${DS.utils.escapeHtml(c.identification)}</span>
                    </div>
                    <div class="ds-client-article__icons" aria-label="Tipos de contacto">
                        ${_buildContactIconsHTML(c)}
                    </div>
                </div>
                <div class="ds-client-article__actions">
                    <button class="ds-client-article__remove-btn" type="button"
                            aria-label="Eliminar cliente ${DS.utils.escapeHtml(c.fullName)}">
                        <span class="material-symbols-outlined" aria-hidden="true">close</span>
                    </button>
                </div>
            </article>`;
    }

    function _filterLocalList(filter) {
        if (!$listBody) return;
        const articles = $listBody.querySelectorAll('.ds-client-article');
        articles.forEach((art) => {
            const estado = art.dataset.estado ?? '';
            const show   = filter === 'todos'
                || (filter === 'por-verificar'  && estado === 'por-verificar')
                || (filter === 'verificados'     && estado === 'verificado');
            art.style.display = show ? '' : 'none';
        });
    }

    function _showListLoading(loading) {
        const msg = $listBody?.querySelector('.ds-client-list__loading-msg');
        if (!$listBody) return;
        if (loading) {
            $listBody.innerHTML = `<div class="ds-client-list__loading-msg">Preparando lista...</div>
                ${_skeletonRows(6)}`;
        }
    }

    function _skeletonRows(n) {
        return Array.from({ length: n }, (_, i) => `
            <article class="ds-client-article ds-client-article--skeleton"
                     aria-hidden="true"
                     style="opacity:${1 - i * 0.15}">
                <div class="ds-client-article__avatar-wrap">
                    <div class="ds-client-article__avatar"></div>
                </div>
                <div class="ds-client-article__info">
                    <div class="ds-client-article__top">
                        <span class="ds-skeleton-line" style="height:12px;width:${100 + (i % 3) * 16}px"></span>
                        <span class="ds-skeleton-line" style="height:8px;width:48px"></span>
                    </div>
                    <span class="ds-skeleton-line" style="height:8px;width:${60 + (i % 2) * 20}px;margin-top:6px"></span>
                </div>
            </article>`).join('');
    }

    function _setListCounts(meta) {
        if (!meta) return;
        if ($countBadge) $countBadge.textContent = meta.totalFiltrado ?? meta.total ?? '';
        if ($pager)      $pager.textContent = `Mostrando ${meta.count ?? 0} de ${meta.totalFiltrado ?? meta.total ?? 0} resultados`;

        // Update pill counts
        $filters.forEach(pill => {
            const f = pill.dataset.filter;
            const count = f === 'todos'
                ? meta.total
                : f === 'por-verificar'
                    ? meta.porVerificar
                    : meta.verificados;
            if (count !== undefined) {
                pill.textContent = '';
                const label = {
                    'todos':         'Todos',
                    'por-verificar': 'Por Verificar',
                    'verificados':   'Verificados',
                }[f] ?? f;
                pill.textContent = `${label} (${count})`;
            }
        });
    }

    /* ──────────────────────────────────────────────────
       Client List internal search
    ────────────────────────────────────────────────── */
    function _bindListSearch() {
        if (!$listSearch) return;
        $listSearch.addEventListener('input', DS.utils.debounce((e) => {
            const q = e.target.value.toLowerCase().trim();
            if (!$listBody) return;
            $listBody.querySelectorAll('.ds-client-article').forEach((art) => {
                const name = (art.querySelector('.ds-client-article__name')?.textContent ?? '').toLowerCase();
                const id   = (art.querySelector('.ds-client-article__id')?.textContent ?? '').toLowerCase();
                art.style.display = (!q || name.includes(q) || id.includes(q)) ? '' : 'none';
            });
        }, 250));
    }

    /* ──────────────────────────────────────────────────
       Actions
    ────────────────────────────────────────────────── */
    function _handleSelectClient(id) {
        _closeDropdown();
        $input.value  = '';
        state.query   = '';
        _toggleClear(false);
        DS.events.emit('cliente:seleccionado', { id });
    }

    function _handleAddClient(id) {
        DS.clientListStore?.add(id);
        DS.events.emit('cliente:agregar', { id });
        _closeDropdown();
        $input.value = '';
        _toggleClear(false);
        DS.notify.success('Cliente agregado a la lista');
    }

    /* ──────────────────────────────────────────────────
       Keyboard navigation
    ────────────────────────────────────────────────── */
    function _bindKeyboard() {
        if (!$input) return;
        $input.addEventListener('keydown', (e) => {
            if (!state.isOpen) return;

            const rows = Array.from($list?.querySelectorAll('.ds-search-result') ?? []);
            if (!rows.length) return;

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                state.focusIndex = Math.min(state.focusIndex + 1, rows.length - 1);
                _updateFocus(rows);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                state.focusIndex = Math.max(state.focusIndex - 1, 0);
                _updateFocus(rows);
            } else if (e.key === 'Enter' && state.focusIndex >= 0) {
                e.preventDefault();
                const focused = rows[state.focusIndex];
                if (focused) _handleSelectClient(focused.dataset.id);
            } else if (e.key === 'Escape') {
                _closeDropdown();
            } else if (e.key === 'Tab') {
                // Let focus move naturally; close dropdown so screen reader
                // doesn't maintain stale listbox context
                _closeDropdown();
            }
        });
    }

    function _updateFocus(rows) {
        rows.forEach((r, i) => {
            r.classList.toggle('ds-search-result--focused', i === state.focusIndex);
            r.setAttribute('aria-selected', i === state.focusIndex ? 'true' : 'false');
        });
        const focused = rows[state.focusIndex];
        if (focused) {
            $input?.setAttribute('aria-activedescendant', focused.id);
            focused.scrollIntoView({ block: 'nearest' });
        } else {
            $input?.removeAttribute('aria-activedescendant');
        }
    }

    /* ──────────────────────────────────────────────────
       Outside click
    ────────────────────────────────────────────────── */
    function _bindOutsideClick() {
        document.addEventListener('click', (e) => {
            if (!$widget?.contains(e.target)) _closeDropdown();
        });
    }

    /* ──────────────────────────────────────────────────
       Utils
    ────────────────────────────────────────────────── */
    function _getInitials(name) {
        if (!name) return '?';
        const parts = name.trim().split(/\s+/);
        return parts.length >= 2
            ? (parts[0][0] + parts[1][0]).toUpperCase()
            : parts[0].slice(0, 2).toUpperCase();
    }

    /* ──────────────────────────────────────────────────
       Public API
    ────────────────────────────────────────────────── */
    window.DS = window.DS ?? {};
    DS.search = {
        init,
        reload: () => _loadClientList(state.filter),
    };

    // Escuchar eventos de otros módulos
    document.addEventListener('DOMContentLoaded', () => {
        init();

        // Recargar lista cuando se crea/edita un cliente desde otro módulo
        DS.events?.on('cliente:creado',   () => DS.search.reload());
        DS.events?.on('cliente:editado',  () => DS.search.reload());
        DS.events?.on('cliente:eliminado',() => DS.search.reload());

        // Limpia el badge "YA CARGADO" del dropdown cuando un cliente es eliminado de la lista
        DS.events?.on('cliente:removido', ({ id }) => {
            const badge = $list?.querySelector(
                `.ds-search__result-item[data-id="${CSS.escape(String(id))}"] .ds-search__badge--loaded`
            );
            badge?.remove();
        });
    });

})();
