/* =====================================================
   dashboard.js — Lógica del dashboard de contactabilidad
   Depende de: ds-core.js, ds-api.js, ds-notifications.js, ds-catalogs.js
   ===================================================== */

(function () {
    'use strict';

    /**
     * Maps each contact type to its semantic group. Populated from server via window.CONTACT_TYPE_TO_GROUP.
     */
    const CONTACT_TYPE_TO_GROUP = window.CONTACT_TYPE_TO_GROUP ?? {};

    const CONTACT_TYPE_TO_ICON = {
        'TEL_C': 'phone',       'CEL':   'phone',     'WAPP': 'phone',
        'EMAIL': 'mail',
        'DIR_D': 'location_on', 'DIR_T': 'location_on',
        'LINK':  'link',
    };

    /** Numeric priority for "worst-wins" aggregation (higher = worse). */
    const STATE_PRIORITY = { verified: 0, pending: 1, error: 2 };

    /** All possible ds-ci state classes for safe swap. */
    const DS_CI_STATE_CLASSES = ['ds-ci--verified', 'ds-ci--pending', 'ds-ci--error'];

    /* ──────────────────────────────────────────────────
       Init
    ────────────────────────────────────────────────── */
    function init() {
        _bindFilterPills();
        _bindListSearch();
        _bindClientRowSelection();
        _bindContactActions();
        _bindMobileBackBtn();
        _initDashboardPanelFilters();
        _bindLoteModal();
        _bindClientePerfilLinks();

        // History API: manejar Atrás / Adelante del navegador
        window.addEventListener('popstate', e => {
            if (e.state?.view === 'perfil' && e.state?.clientId) {
                // Navegó adelante a la vista de perfil
                _showClienteView(e.state.clientId, e.state.clientName ?? 'Perfil de Cliente', false);
            } else if (e.state?.clientId) {
                // Navegó al estado con panel de detalle abierto → cerrar vista perfil si estaba abierta
                _showDashboardView({ updateHistory: false });
                _selectClientNoHistory(e.state.clientId);
            } else {
                // Navegó atrás hasta la lista limpia
                _showDashboardView({ updateHistory: false });
                _closeDetailPanel({ scrollY: e.state?.listScrollY ?? 0, updateHistory: false });
                if (_asideIsEmpty()) {
                    const ids = DS.clientListStore?.getIds() ?? [];
                    if (ids.length) _apiSearchClientsByIds(ids);
                }
            }
        });

        // Handle client selection emitted by ds-search.js (search dropdown or external module).
        DS.events?.on('cliente:seleccionado', ({ id }) => {
            const alreadyActive = !!document.querySelector(
                `.ds-client-article--active[data-id="${CSS.escape(id)}"]`);
            if (!alreadyActive) _selectClient(id);
        });

        DS.events?.on('cliente:agregar', ({ id }) => _handleClienteAgregar(id));

        // Deep-link: si la URL ya trae ?cliente= al cargar, abrir ese cliente.
        const urlParams  = new URLSearchParams(window.location.search);
        const urlCliente = urlParams.get('cliente');
        const urlView    = urlParams.get('view');

        if (urlCliente && urlView === 'perfil') {
            // Deep-link a la vista perfil
            history.replaceState(
                { clientId: urlCliente, view: 'perfil', listScrollY: 0 },
                '',
                window.location.href
            );
            _showClienteView(urlCliente, 'Perfil de Cliente', false);
        } else if (urlCliente) {
            // Deep-link al panel de detalle
            history.replaceState(
                { clientId: urlCliente, listScrollY: 0, activeFilter: 'todos' },
                '',
                window.location.href
            );
            _selectClientNoHistory(urlCliente);
        } else if (window.innerWidth > 767) {
            const first = document.querySelector('#ds-client-list-body .ds-client-article');
            if (first) _selectClientNoHistory(first.dataset.id);
        }
    }

    /** Devuelve true si el aside de clientes no tiene ningún artículo en el DOM. */
    function _asideIsEmpty() {
        return !document.querySelector('#ds-client-list-body .ds-client-article');
    }

    /* ──────────────────────────────────────────────────
       View Orchestrator — Dashboard ↔ Cliente Perfil
    ────────────────────────────────────────────────── */

    /**
     * Muestra la vista embebida del perfil de cliente DENTRO del dashboard.
     * Descarga el HTML de /clientes?id=xxx&embed=1, lo inyecta y llama a DS.Clientes.init().
     * @param {string} id       — identificación del cliente
     * @param {string} name     — nombre para el breadcrumb
     * @param {boolean} [pushState=true] — si se debe apilar una entrada en el historial
     */
    async function _showClienteView(id, name = 'Perfil de Cliente', pushState = true) {
        const viewEl    = document.getElementById('ds-view-cliente');
        const contentEl = document.getElementById('ds-client-view-content');
        const loadingEl = document.getElementById('ds-client-view-loading');
        const breadEl   = document.getElementById('ds-client-view-breadcrumb-name');
        const dashEl    = document.querySelector('.dash-layout');

        if (!viewEl || !contentEl) return;

        // Actualizar breadcrumb
        if (breadEl) breadEl.textContent = name;

        // Mostrar vista y spinner, ocultar dashboard
        viewEl.hidden = false;
        viewEl.removeAttribute('hidden');
        if (dashEl) dashEl.hidden = true;
        if (loadingEl) loadingEl.hidden = false;

        // Apilar en historial antes de la carga (URL cambia de inmediato)
        if (pushState) {
            history.pushState(
                { clientId: id, clientName: name, view: 'perfil', listScrollY: window.scrollY },
                '',
                `/Dashboard?cliente=${encodeURIComponent(id)}&view=perfil`
            );
        }

        try {
            const resp = await fetch(`/clientes?id=${encodeURIComponent(id)}&embed=1`, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const html = await resp.text();

            // Inyectar contenido (reemplazar cualquier carga anterior)
            contentEl.innerHTML = html;

            // Re-inicializar módulo de clientes sobre el nuevo DOM
            DS.Clientes?.init();

            // Scroll al inicio del perfil
            viewEl.scrollTop = 0;
        } catch {
            contentEl.innerHTML = `
                <div style="padding:2rem;text-align:center;color:var(--color-danger)">
                    <span class="material-symbols-outlined" style="font-size:2.5rem">error</span>
                    <p style="margin-top:.5rem">No se pudo cargar el perfil del cliente.</p>
                </div>`;
        } finally {
            if (loadingEl) loadingEl.hidden = true;
        }
    }

    /**
     * Cierra la vista embebida del cliente y regresa al dashboard.
     * @param {boolean} [updateHistory=true] — si se debe actualizar la URL
     */
    function _showDashboardView({ updateHistory = true } = {}) {
        const viewEl  = document.getElementById('ds-view-cliente');
        const dashEl  = document.querySelector('.dash-layout');

        if (viewEl)  viewEl.hidden  = true;
        if (dashEl)  dashEl.hidden  = false;

        if (updateHistory) {
            // Volver a la URL del panel del cliente activo (sin view=perfil)
            const activeArt = document.querySelector('.ds-client-article--active');
            const clientId  = activeArt?.dataset.id;
            if (clientId) {
                history.replaceState(
                    { clientId, listScrollY: 0 },
                    '',
                    `/Dashboard?cliente=${encodeURIComponent(clientId)}`
                );
            } else {
                history.replaceState({ listScrollY: 0 }, '', '/Dashboard');
            }
        }
    }

    /**
     * Vincula el click en los botones/links "MÁS DETALLES" (data-cliente-perfil-id)
     * dentro del panel de detalle y del aside. Usa event delegation sobre document
     * para capturar tanto los panels server-rendered como los inyectados dinámicamente.
     */
    function _bindClientePerfilLinks() {
        document.addEventListener('click', e => {
            const link = e.target.closest('[data-cliente-perfil-id]');
            if (!link) return;
            e.preventDefault();
            const id   = link.dataset.clientePerfilId;
            const name = link.dataset.clientePerfilName ?? 'Perfil de Cliente';
            if (id) _showClienteView(id, name);
        });

        // Botón "Volver al Dashboard"
        document.getElementById('ds-client-view-back')?.addEventListener('click', () => {
            _showDashboardView();
        });
    }

    /* ──────────────────────────────────────────────────
       Filter pills (Todos / Por verificar / Verificados)
    ────────────────────────────────────────────────── */
    function _bindFilterPills() {
        const pills = document.querySelectorAll('.ds-search-filters__pill');
        const body  = document.getElementById('ds-client-list-body');
        const count = document.getElementById('ds-client-list-count');
        const pager = document.getElementById('ds-client-list-pager');

        pills.forEach(pill => {
            pill.addEventListener('click', () => {
                pills.forEach(p => {
                    p.classList.remove('ds-search-filters__pill--active');
                    p.setAttribute('aria-selected', 'false');
                });
                pill.classList.add('ds-search-filters__pill--active');
                pill.setAttribute('aria-selected', 'true');

                const filter   = pill.dataset.filter ?? 'todos';
                const articles = body ? Array.from(body.querySelectorAll('.ds-client-article')) : [];
                let visible    = 0;

                articles.forEach(art => {
                    const estado = (art.dataset.estado ?? '').toLowerCase();
                    const show =
                        filter === 'todos' ||
                        (filter === 'por-verificar' && estado !== 'verificado') ||
                        (filter === 'verificados'   && estado === 'verificado');
                    art.style.display = show ? '' : 'none';
                    if (show) visible++;
                });

                if (count) count.textContent = visible;
                if (pager) pager.textContent = `Mostrando ${visible} de ${articles.length} resultados`;
            });
        });
    }

    /* ──────────────────────────────────────────────────
       Internal list search
    ────────────────────────────────────────────────── */
    function _bindListSearch() {
        const input = document.getElementById('ds-client-list-search');
        const body  = document.getElementById('ds-client-list-body');
        const count = document.getElementById('ds-client-list-count');
        const pager = document.getElementById('ds-client-list-pager');

        input?.addEventListener('input', _debounce(e => {
            const q        = e.target.value.toLowerCase().trim();
            const articles = body ? Array.from(body.querySelectorAll('.ds-client-article')) : [];
            const total    = articles.length;
            let visible    = 0;

            articles.forEach(art => {
                const name = (art.querySelector('.ds-client-article__name')?.textContent ?? '').toLowerCase();
                const id   = (art.querySelector('.ds-client-article__id')?.textContent   ?? '').toLowerCase();
                const show = !q || name.includes(q) || id.includes(q);
                art.style.display = show ? '' : 'none';
                if (show) visible++;
            });

            if (count) count.textContent = visible;
            if (pager) pager.textContent = q
                ? `Mostrando ${visible} de ${total} resultados`
                : `Mostrando ${total} de ${total} resultados`;
        }, 250));
    }

    /* ──────────────────────────────────────────────────
       Client row selection → show matching detail panel
    ────────────────────────────────────────────────── */
    function _bindClientRowSelection() {
        const body = document.getElementById('ds-client-list-body');

        body?.addEventListener('click', e => {
            // Interceptar botón de eliminación antes de la selección del artículo
            const removeBtn = e.target.closest('.ds-client-article__remove-btn');
            if (removeBtn) {
                e.stopPropagation();
                const artToRemove = removeBtn.closest('.ds-client-article');
                if (artToRemove) _removeClientFromList(artToRemove.dataset.id);
                return;
            }

            const art = e.target.closest('.ds-client-article');
            if (art) {
                _selectClient(art.dataset.id);
                // Emit so ds-search.js listener (and other modules) can react.
                // _selectClient itself no longer emits to avoid re-entrancy.
                DS.events?.emit('cliente:seleccionado', { id: art.dataset.id });
            }
        });

        body?.addEventListener('keydown', e => {
            if (e.key !== 'Enter' && e.key !== ' ') return;
            const art = e.target.closest('.ds-client-article');
            if (art) { e.preventDefault(); _selectClient(art.dataset.id); }
        });
    }

    /**
     * Núcleo de selección de cliente — muestra el panel sin tocar el historial.
     * Usado internamente por _selectClient y por el handler de popstate.
     */
    function _selectClientNoHistory(id) {
        if (!id) return;

        // Update active article
        document.querySelectorAll('.ds-client-article--active')
            .forEach(a => a.classList.remove('ds-client-article--active'));
        const art = document.querySelector(`.ds-client-article[data-id="${CSS.escape(id)}"]`);
        art?.classList.add('ds-client-article--active');
        art?.scrollIntoView({ block: 'nearest' });

        // Si el panel no existe (e.g. restaurado desde sessionStorage sin recarga batch),
        // lo cargamos de forma lazy y retornamos — _selectClientNoHistory se llamará de nuevo
        // una vez que el panel esté inyectado.
        const activePanel = document.querySelector(`.dash-detail__panel[data-client-id="${CSS.escape(id)}"]`);
        if (!activePanel) {
            _fetchAndInjectDetailPanel(id);
            return;
        }

        // Show matching panel, hide others + empty state
        document.querySelectorAll('.dash-detail__panel').forEach(p => {
            p.hidden = (p.dataset.clientId !== id);
        });
        const empty = document.getElementById('dash-detail-empty');
        if (empty) empty.hidden = true;
        // Actualizar conteos del filtro activo para el panel que se va a mostrar
        DS.contactFilter?.reapply(activePanel);

        // Mobile: switch to detail view
        if (window.innerWidth <= 767) {
            document.querySelector('.dash-body')?.classList.add('dash-body--detail-open');
        }
    }

    /**
     * Selecciona un cliente y actualiza la URL con pushState.
     * Si el cliente ya está reflejado en la URL actual no apila una entrada duplicada.
     */
    function _selectClient(id) {
        if (!id) return;
        _selectClientNoHistory(id);

        // Actualizar URL solo si el cliente cambió respecto al state actual
        const url = new URL(window.location.href);
        if (url.searchParams.get('cliente') !== String(id)) {
            const activeFilter = document.querySelector('.ds-search-filters__pill--active')?.dataset.filter ?? 'todos';
            history.pushState(
                { clientId: id, listScrollY: window.scrollY, activeFilter },
                '',
                `/Dashboard?cliente=${encodeURIComponent(id)}`
            );
        }
    }

    /**
     * Carga el detalle completo de un cliente desde la API e inyecta su panel
     * en el DOM. Se usa como fallback cuando el panel no está presente
     * (ejemplo: lista restaurada desde sessionStorage tras recargar la página).
     * Al terminar llama a _selectClientNoHistory para que el panel sea visible
     * sin apilar una entrada duplicada en el historial.
     */
    async function _fetchAndInjectDetailPanel(id) {
        if (!id) return;
        try {
            const { ok, data } = await DS.api.get(DS.endpoints.clients.detail(id));
            if (!ok || !data?.success || !data?.data) {
                DS.notify.error('No se pudo cargar el detalle del cliente');
                return;
            }
            const clientViewModel = _mapDtoToViewModel(data.data);
            _injectDetailPanel(clientViewModel);
            // Sync aside article icons with the current verification state from the detail endpoint
            if (data.verificationState) {
                _applyVerificationState(clientViewModel.id, data.verificationState);
            }
            _selectClientNoHistory(clientViewModel.id);
        } catch {
            DS.notify.error('Error al cargar el detalle del cliente');
        }
    }

    /* ──────────────────────────────────────────────────
       Contact actions (verify / validate / edit)
       Uses event delegation on .dash-detail
    ────────────────────────────────────────────────── */
    function _bindContactActions() {
        const detail = document.querySelector('.dash-detail');

        detail?.addEventListener('click', e => {
            // 1. Quick-verify toggle
            const verifyBtn = e.target.closest('.contact-verify-btn');
            if (verifyBtn) {
                const row          = verifyBtn.closest('.dash-contact-row');
                if (!row) return;
                const icon         = verifyBtn.querySelector('.contact-verify-icon');
                const prevVerified = icon?.classList.contains('is-verified') ?? false;
                if (prevVerified) return;   // ya verificado — no hacer nada
                const newVerified  = true;

                // Limpiar estado de error si el row fue rechazado anteriormente
                row.classList.remove('dash-contact-row--error');
                row.classList.remove('dash-contact-row--error');
                row.classList.add('dash-contact-row--verified');
                row.dataset.contactState = 'verified';
                row.querySelector('.dash-contact-row__error-hint')?.remove();
                row.querySelector('.dash-rejection-zone')?.remove();

                _setIconVerified(icon, newVerified);
                _apiVerify({
                    clientId:        row.dataset.clientId,
                    clientNumericId: row.dataset.clientNumericId,
                    contactId:       row.dataset.contactId,
                    contactType:     row.dataset.contactType,
                    contactValue:    row.dataset.contactValue,
                    verified:        newVerified,
                }, icon, prevVerified, row.dataset.clientId);
                return;
            }

            // 2. Toggle inline rejection zone
            const validBtn = e.target.closest('.contact-reject-btn');
            if (validBtn) {
                const row = validBtn.closest('.dash-contact-row');
                if (row) _toggleRejectionZone(row);
                return;
            }

            // 3. Edit (placeholder)
            if (e.target.closest('.contact-edit-btn')) {
                DS.notify.info('Editar contacto — próximamente');
                return;
            }
        });
    }

    function _setIconVerified(icon, verified) {
        if (!icon) return;
        icon.classList.toggle('is-verified', verified);
        // Update the parent button classes and disabled state
        const btn = icon.closest('.contact-verify-btn');
        if (btn) {
            btn.classList.toggle('contact-verify-btn--verified', verified);
            btn.disabled = verified;
        }
        // Update the row verified class
        const row = btn?.closest('.dash-contact-row');
        if (row) {
            row.classList.toggle('dash-contact-row--verified', verified);
        }
    }

    /**
     * Returns the semantic state of a contact row based on its current CSS classes.
     * @param {Element} row - .dash-contact-row element
     * @returns {"verified"|"pending"|"error"}
     */
    function _getRowState(row) {
        if (row.classList.contains('dash-contact-row--error')) return 'error';
        const statusIcon = row.querySelector('.contact-verify-icon');
        if (statusIcon?.classList.contains('is-verified')) return 'verified';
        return 'pending';
    }

    /**
     * Syncs the client article icons and data-estado after any contact state change.
     * Uses category-based verification:
     *   clientVerified = AnyVerif(TEL) ∧ AnyVerif(EMAIL) ∧ AnyVerif(DIR)
     * where TEL = {CEL, TEL_C}, EMAIL = {EMAIL}, DIR = {DIR_D}.
     * Also applies "worst-wins" aggregation per icon group for individual icons.
     * @param {string} clientId
     */
    function _syncClientBadge(clientId) {
        if (!clientId) return;

        const panel = document.querySelector(`.dash-detail__panel[data-client-id="${CSS.escape(clientId)}"]`);
        const rows  = panel ? Array.from(panel.querySelectorAll('.dash-contact-row')) : [];

        // Group-based coverage: for every semantic group present, at least 1 must be verified.
        // CEL + TEL_C + WAPP share the "phone" group — one verified phone covers the group.
        const groupMap = {};
        rows.forEach(r => {
            const g = CONTACT_TYPE_TO_GROUP[r.dataset.contactType] ?? r.dataset.contactType;
            if (!groupMap[g]) groupMap[g] = [];
            groupMap[g].push(r);
        });
        const allVerif = rows.length > 0 && Object.values(groupMap).every(group =>
            group.some(r => r.querySelector('.contact-verify-icon')?.classList.contains('is-verified'))
        );

        // 1. Update detail panel header status pill
        const pill = panel?.querySelector('.dash-detail__status-pill');
        if (pill) {
            pill.className   = `dash-detail__status-pill ${allVerif ? 'dash-detail__status-pill--verified' : 'dash-detail__status-pill--pending'}`;
            pill.textContent = `ESTADO: ${allVerif ? 'VERIFICADO' : 'PENDIENTE'}`;
        }

        // 2. Update article data-estado + sync upload widget
        const art        = document.querySelector(`.ds-client-article[data-id="${CSS.escape(clientId)}"]`);
        const prevEstado = art?.dataset.estado;
        const newEstado  = allVerif ? 'verificado' : 'por-verificar';
        if (art) art.dataset.estado = newEstado;
        if (prevEstado !== newEstado) {
            _syncUploadWidget(newEstado === 'verificado' ? 1 : -1);
            _syncFilterPills();
        }

        // 3. Build per-contact-type state map, roll up to icon-group level, update .ds-ci icons
        const typeStateMap = {};

        rows.forEach(row => {
            const type  = row.dataset.contactType;
            if (!type) return;
            const state = _getRowState(row);
            const prev  = typeStateMap[type];
            if (prev === undefined || STATE_PRIORITY[state] > STATE_PRIORITY[prev]) {
                typeStateMap[type] = state;
            }
        });

        if (art) {
            // Roll up contactType → icon-group using CONTACT_TYPE_TO_GROUP (worst-wins per group)
            const groupStateMap = {};
            Object.entries(typeStateMap).forEach(([type, state]) => {
                const group = CONTACT_TYPE_TO_GROUP[type] ?? type;
                const prev  = groupStateMap[group];
                if (prev === undefined || STATE_PRIORITY[state] > STATE_PRIORITY[prev]) {
                    groupStateMap[group] = state;
                }
            });
            art.querySelectorAll('.ds-ci[data-icon-group]').forEach(icon => {
                const group = icon.dataset.iconGroup;
                const state = groupStateMap[group];
                if (state === undefined) return;
                DS_CI_STATE_CLASSES.forEach(c => icon.classList.remove(c));
                icon.classList.add(`ds-ci--${state}`);
            });
        }
    }

    /**
     * Recuenta los artículos del listado por data-estado y actualiza los contadores
     * de las pills de filtro (Todos / Por Verificar / Verificados).
     */
    function _syncFilterPills() {
        const articles    = Array.from(document.querySelectorAll('#ds-client-list-body .ds-client-article'));
        const total       = articles.length;
        const verificados = articles.filter(a => a.dataset.estado === 'verificado').length;
        const porVerif    = total - verificados;

        const elTodos    = document.getElementById('pill-count-todos');
        const elPorVerif = document.getElementById('pill-count-por-verificar');
        const elVerif    = document.getElementById('pill-count-verificados');

        if (elTodos)    elTodos.textContent    = total;
        if (elPorVerif) elPorVerif.textContent = porVerif;
        if (elVerif)    elVerif.textContent    = verificados;
    }

    /**
     * Actualiza el contador "#ds-client-list-count" con el número real de artículos
     * actualmente en el DOM (visibles o no).
     */
    function _updateListCount() {
        const count = document.getElementById('ds-client-list-count');
        if (!count) return;
        count.textContent = document.querySelectorAll('#ds-client-list-body .ds-client-article').length;
    }

    /**
     * Elimina un cliente del listado lateral, su panel de detalle y del store
     * de sesión. Si era el cliente activo, activa el siguiente de la lista o
     * muestra el estado vacío.
     * @param {string} id  Identificador del cliente (string, ej. "0012345678")
     */
    function _removeClientFromList(id) {
        if (!id) return;
        const art      = document.querySelector(`.ds-client-article[data-id="${CSS.escape(id)}"]`);
        const panel    = document.querySelector(`.dash-detail__panel[data-client-id="${CSS.escape(id)}"]`);
        const wasActive = art?.classList.contains('ds-client-article--active');

        if (art) {
            art.style.transition = 'opacity 0.2s';
            art.style.opacity    = '0';
            setTimeout(() => {
                art.remove();
                _syncFilterPills();
                _updateListCount();
            }, 200);
        }
        panel?.remove();

        if (wasActive) {
            // Excluir el artículo que aún está en el DOM (fade de 200ms) para evitar
            // que _selectClient lo tome como "siguiente" y re-inyecte su panel.
            const next = document.querySelector(`#ds-client-list-body .ds-client-article:not([data-id="${CSS.escape(id)}"])`);
            if (next) {
                _selectClient(next.dataset.id);
            } else {
                const empty = document.getElementById('dash-detail-empty');
                if (empty) empty.hidden = false;
            }
        }

        DS.clientListStore?.remove(id);
        DS.events?.emit('cliente:removido', { id });
    }

    /**
     * Actualiza el widget de progreso (barra, porcentaje, detalle) con un delta.
     * @param {number} delta  +1 al verificar, -1 al revertir
     */
    function _syncUploadWidget(delta) {
        const progress = document.getElementById('widget-progress');
        if (!progress) return;
        const total    = parseInt(progress.dataset.total,    10) || 0;
        const verified = Math.max(0, Math.min(total, parseInt(progress.dataset.verified, 10) + delta));
        progress.dataset.verified = verified;
        const pct = total > 0 ? Math.round(verified * 100 / total) : 0;
        progress.setAttribute('aria-valuenow', pct);
        const fill   = document.getElementById('widget-bar-fill');
        const pctEl  = document.getElementById('widget-pct');
        const detail = document.getElementById('widget-detail');
        if (fill)   fill.style.width   = `${pct}%`;
        if (pctEl)  pctEl.textContent  = `${pct}%`;
        if (detail) detail.textContent = `${verified} de ${total} Validados`;
    }

    /* ──────────────────────────────────────────────────
       API helpers
    ────────────────────────────────────────────────── */
    function _getToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? null;
    }

    async function _apiSearchClientsByIds(ids) {
        const token = _getToken();
        if (!token) { DS.notify.error('Token de seguridad no encontrado'); return null; }

        try {
            const { ok, data } = await DS.api.post(
                '/Dashboard?handler=SearchClientsByIds',
                { ids },
                { headers: { 'RequestVerificationToken': token } }
            );

            if (!ok) throw new Error('Server error');
            return data?.data ?? [];
        } catch (err) {
            DS.notify.error('Error al buscar clientes');
            return null;
        }
    }

    async function _apiVerify({ clientId, clientNumericId, contactId, contactType }, icon, prevVerified, syncClientId) {
        try {
            const isAddress = ['DIR_D', 'DIR_T'].includes(contactType);
            const url = isAddress
                ? DS.endpoints.clients.verifyAddress(clientNumericId, contactId)
                : DS.endpoints.clients.verifyContact(clientNumericId, contactId);

            const { ok, data } = await DS.api.post(url);

            if (!ok) throw new Error('Server error');

            // El servidor calcula el estado — el JS solo pinta
            if (data?.verificationState) {
                _applyVerificationState(syncClientId, data.verificationState);
            } else {
                _syncClientBadge(syncClientId); // fallback DOM-based
            }
            DS.notify.success('Contacto verificado');
        } catch {
            DS.notify.error('Error al actualizar el estado del contacto');
            _setIconVerified(icon, prevVerified); // revert optimistic UI
        }
    }

    async function _apiContactError({ clientId, clientNumericId, contactId, contactType, errorCode, errorLabel }, row) {
        try {
            const isAddress = ['DIR_D', 'DIR_T'].includes(contactType);
            const url = isAddress
                ? DS.endpoints.clients.unverifyAddress(clientNumericId, contactId, errorCode)
                : DS.endpoints.clients.unverifyContact(clientNumericId, contactId, errorCode);

            const { ok, data } = await DS.api.post(url);

            if (!ok) throw new Error('Server error');

            // Actualizar la fila visualmente
            const icon = row?.querySelector('.contact-verify-icon');
            _setIconVerified(icon, false);
            row?.classList.add('dash-contact-row--error');
            row?.classList.remove('dash-contact-row--verified');
            if (row) row.dataset.contactState = 'error';

            row?.querySelector('.dash-contact-row__error-hint')?.remove();
            if (row && errorLabel) {
                const hint = document.createElement('div');
                hint.className = 'dash-contact-row__error-hint';
                hint.innerHTML =
                    `<span class="material-symbols-outlined" aria-hidden="true">cancel</span>` +
                    `<span>${errorLabel}</span>`;
                row.appendChild(hint);
            }
            row?.querySelector('.contact-reject-btn')?.classList.add('is-active');

            // El servidor calcula el estado — el JS solo pinta
            if (data?.verificationState) {
                _applyVerificationState(row?.dataset.clientId, data.verificationState);
            } else {
                _syncClientBadge(row?.dataset.clientId); // fallback DOM-based
            }
            DS.notify.success('Motivo de rechazo registrado');
        } catch {
            DS.notify.error('Error al registrar el motivo de rechazo');
        }
    }

    /* ──────────────────────────────────────────────────
       Inline rejection zone (pill picker)
    ────────────────────────────────────────────────── */
    /* _applyVerificationState */
    function _applyVerificationState(clientId, state) {
        if (!clientId || !state) return;
        const newEstado  = state.isClientVerified ? 'verificado' : 'por-verificar';
        const art        = document.querySelector('.ds-client-article[data-id="' + CSS.escape(clientId) + '"]');
        const prevEstado = art?.dataset.estado;
        if (art) art.dataset.estado = newEstado;
        const panel = document.querySelector('.dash-detail__panel[data-client-id="' + CSS.escape(clientId) + '"]');
        const pill  = panel?.querySelector('.dash-detail__status-pill');
        if (pill) {
            const v = state.isClientVerified;
            pill.className   = 'dash-detail__status-pill ' + (v ? 'dash-detail__status-pill--verified' : 'dash-detail__status-pill--pending');
            pill.textContent = 'ESTADO: ' + (v ? 'VERIFICADO' : 'PENDIENTE');
        }
        const SC = { verified: 'ds-ci--verified', pending: 'ds-ci--pending', error: 'ds-ci--error', none: 'ds-ci--pending' };
        const gs = { phone: state.phoneState, email: state.emailState, address: state.addressState };
        if (art) {
            art.querySelectorAll('.ds-ci[data-icon-group]').forEach(icon => {
                const g = icon.dataset.iconGroup, st = gs[g];
                if (!st || st === 'none') return;
                DS_CI_STATE_CLASSES.forEach(c => icon.classList.remove(c));
                icon.classList.add(SC[st] ?? 'ds-ci--pending');
            });
        }
        if (prevEstado !== newEstado) { _syncUploadWidget(newEstado === 'verificado' ? 1 : -1); _syncFilterPills(); }

        // Re-aplicar filtro activo y actualizar conteos tras el cambio de estado
        if (panel) DS.contactFilter?.reapply(panel);

    }
    /**
     * Inicializa DS.contactFilter en todos los paneles ya presentes en el DOM
     * (renderizados por el servidor en el primer GET del Dashboard).
     * Los paneles inyectados dinámicamente se inicializan en _injectDetailPanel.
     */
    function _initDashboardPanelFilters() {
        document.querySelectorAll('.dash-detail__panel').forEach(panel => {
            DS.contactFilter?.init(panel, { sectionSelector: '.dash-detail__section' });
        });
    }

    function _toggleRejectionZone(row) {
        if (!row) return;

        // Toggle off if already open
        const existing = row.querySelector('.dash-rejection-zone');
        if (existing) {
            existing.remove();
            row.querySelector('.contact-reject-btn')?.classList.remove('is-active');
            return;
        }

        // Build inline zone usando catálogo centralizado
        const contactType = row.dataset.contactType;
        const errors      = DS.catalogs?.getErrorsForType(contactType) || [];

        const zone = document.createElement('div');
        zone.className = 'dash-rejection-zone';
        zone.innerHTML = `
            <p class="dash-rejection-zone__label" aria-label="Seleccione motivo de rechazo">
                <span class="dash-rejection-zone__label-dot" aria-hidden="true"></span>
                Seleccione motivo de rechazo:
            </p>
            <div class="dash-rejection-zone__pills"
                 role="group" aria-label="Motivos de rechazo">
                ${errors.map(err =>
                    `<button class="dash-rejection-pill"
                             type="button"
                             data-code="${err.code}">${err.label}</button>`
                ).join('')}
            </div>`;

        zone.querySelectorAll('.dash-rejection-pill').forEach(pill => {
            pill.addEventListener('click', () => {
                zone.remove();
                _apiContactError({
                    clientId:        row.dataset.clientId,
                    clientNumericId: row.dataset.clientNumericId,
                    contactId:       row.dataset.contactId,
                    contactType:     row.dataset.contactType,
                    contactValue:    row.dataset.contactValue,
                    errorCode:       pill.dataset.code,
                    errorLabel:      pill.textContent.trim(),
                }, row);
            });
        });

        row.appendChild(zone);
        row.querySelector('.contact-reject-btn')?.classList.add('is-active');
    }

    /* ──────────────────────────────────────────────────
       Mobile back button (Volver)
    ────────────────────────────────────────────────── */
    function _bindMobileBackBtn() {
        document.querySelector('.dash-detail')?.addEventListener('click', e => {
            if (e.target.closest('.dash-detail__back-btn')) _closeDetailPanel();
        });
    }

    /**
     * Cierra el panel de detalle, limpia la URL y restaura el scroll de la lista.
     * @param {object} [opts]
     * @param {number} [opts.scrollY=0] - Posición de scroll a restaurar.
     * @param {boolean} [opts.updateHistory=true] - false cuando ya lo gestiona popstate.
     */
    function _closeDetailPanel({ scrollY = 0, updateHistory = true } = {}) {
        document.querySelectorAll('.ds-client-article--active')
            .forEach(a => a.classList.remove('ds-client-article--active'));
        document.querySelectorAll('.dash-detail__panel').forEach(p => { p.hidden = true; });
        const empty = document.getElementById('dash-detail-empty');
        if (empty) empty.hidden = false;
        document.querySelector('.dash-body')?.classList.remove('dash-body--detail-open');

        if (updateHistory) {
            history.replaceState({ listScrollY: scrollY }, '', '/Dashboard');
        }
        if (scrollY) window.scrollTo({ top: scrollY, behavior: 'instant' });
    }

    /* ──────────────────────────────────────────────────
       Utility: debounce
    ────────────────────────────────────────────────── */
    function _debounce(fn, delay) {
        let timer;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delay);
        };
    }

    /* ──────────────────────────────────────────────────
       Modal: Nueva Lote de clientes
    ────────────────────────────────────────────────── */
    function _bindLoteModal() {
        const overlay    = document.getElementById('lote-modal-overlay');
        if (!overlay) return;

        const openBtn    = document.getElementById('btn-open-lote-modal');
        const closeBtn   = document.getElementById('lote-modal-close');
        const cancelBtn  = document.getElementById('lote-modal-cancel');
        const submitBtn  = document.getElementById('lote-modal-submit');
        const submitLabel = submitBtn?.querySelector('.lote-modal__submit-label');
        const textarea   = document.getElementById('lote-ids-textarea');
        const clearBtn   = document.getElementById('lote-clear-btn');
        const infoBanner = document.getElementById('lote-info-banner');
        const infoText   = document.getElementById('lote-info-text');
        const tabs       = overlay.querySelectorAll('.lote-modal__tab');

        /* ── open / close ── */
        function _open() {
            overlay.hidden = false;
            _updateSubmitLabel();
            textarea?.focus();
            document.addEventListener('keydown', _handleEsc);
        }

        function _close() {
            overlay.hidden = true;
            document.removeEventListener('keydown', _handleEsc);
        }

        function _handleEsc(e) {
            if (e.key === 'Escape') _close();
        }

        openBtn?.addEventListener('click', _open);
        closeBtn?.addEventListener('click', _close);
        cancelBtn?.addEventListener('click', _close);

        // Backdrop click closes modal
        overlay.addEventListener('click', e => {
            if (e.target === overlay) _close();
        });

        /* ── tabs ── */
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                tabs.forEach(t => {
                    t.classList.remove('lote-modal__tab--active');
                    t.setAttribute('aria-selected', 'false');
                });
                overlay.querySelectorAll('.lote-modal__pane')
                    .forEach(p => p.classList.remove('lote-modal__pane--active'));

                tab.classList.add('lote-modal__tab--active');
                tab.setAttribute('aria-selected', 'true');

                const pane = document.getElementById(tab.getAttribute('aria-controls'));
                if (pane) pane.classList.add('lote-modal__pane--active');

                _updateSubmitLabel();
            });
        });

        function _activeTabId() {
            return overlay.querySelector('.lote-modal__tab--active')?.id ?? '';
        }

        function _updateSubmitLabel() {
            if (!submitBtn || !submitLabel) return;
            const tab = _activeTabId();
            let enabled = false;
            if (tab === 'lote-tab-ids') {
                const { valid } = _parseIds(textarea?.value ?? '');
                enabled = valid.length > 0;
            } else if (tab === 'lote-tab-archivo') {
                enabled = window.LOTE_FILE_MODAL?.hasValidFile ?? false;
            }
            submitBtn.disabled = !enabled;
            submitBtn.style.opacity = enabled ? '1' : '0.5';
            submitBtn.style.cursor = enabled ? 'pointer' : 'not-allowed';
            submitLabel.textContent = 'Procesar y Buscar';
        }

        /* ── textarea → ID parsing ── */

        /**
         * Validates a single ID token:
         *  - Purely numeric  → must be exactly 9, 10 or 13 digits.
         *  - Alphanumeric    → at least 6 chars, only [a-zA-Z0-9], no spaces.
         */
        function _isValidId(id) {
            if (/^\d+$/.test(id)) {
                return id.length === 9 || id.length === 10 || id.length === 13;
            }
            // Alphanumeric: min 6 chars, MUST contain at least one letter AND one digit
            return /^[a-zA-Z0-9]{6,}$/.test(id)
                && /[a-zA-Z]/.test(id)
                && /\d/.test(id);
        }

        function _parseIds(value) {
            const all     = value.split(/[\n,;\s]+/).map(s => s.trim()).filter(s => s.length > 0);
            const unique  = [...new Set(all)];
            const valid   = unique.filter(_isValidId);
            const dupes   = all.length - unique.length;
            const invalid = unique.length - valid.length;
            return { all, unique, valid, dupes, invalid };
        }

        function _updateBanner() {
            const { all, valid, dupes, invalid } = _parseIds(textarea?.value ?? '');
            if (all.length === 0) {
                infoBanner.hidden = true;
                return;
            }
            infoBanner.hidden = false;
            const parts = [
                `${valid.length} ID${valid.length !== 1 ? 's' : ''} v\u00e1lido${valid.length !== 1 ? 's' : ''}`,
            ];
            if (dupes   > 0) parts.push(`${dupes}   duplicado${dupes   !== 1 ? 's' : ''} eliminado${dupes   !== 1 ? 's' : ''}`);
            if (invalid > 0) parts.push(`${invalid} inv\u00e1lido${invalid !== 1 ? 's' : ''} ignorado${invalid !== 1 ? 's' : ''}`);
            infoText.textContent = parts.join(' \u2022 ');
        }

        textarea?.addEventListener('input', () => {
            _updateBanner();
            _updateSubmitLabel();
        });

        /* ── clear ── */
        clearBtn?.addEventListener('click', () => {
            if (textarea) textarea.value = '';
            infoBanner.hidden = true;
            textarea?.focus();
            _updateSubmitLabel();
        });

        /* ── submit ── */
        submitBtn?.addEventListener('click', () => {
            let ids;
            if (_activeTabId() === 'lote-tab-archivo') {
                const modal = window.LOTE_FILE_MODAL;
                if (!modal?.hasValidFile) return;
                ids = modal.validationResult.validIds;
            } else {
                const { valid } = _parseIds(textarea?.value ?? '');
                if (valid.length === 0) { textarea?.focus(); return; }
                ids = valid;
            }

            // Deshabilitar para prevenir doble-submit
            submitBtn.disabled = true;

            // POST body — nunca en la URL
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Dashboard?handler=LoteBatch';

            const idsInput = document.createElement('input');
            idsInput.type = 'hidden';
            idsInput.name = 'ids';
            idsInput.value = ids.join(',');
            form.appendChild(idsInput);

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token;
            form.appendChild(tokenInput);

            document.body.appendChild(form);
            form.submit();
        });

        /* ── CustomEvent listener for file tab state changes ── */
        document.addEventListener('lote:file-ready', () => _updateSubmitLabel());
    }

    /* ──────────────────────────────────────────────────
       Dynamic client injection (search dropdown "Agregar")
    ────────────────────────────────────────────────── */

    /**
     * Fetches full client detail from the REST API and injects
     * the article (left list) + detail panel (right area) into the DOM,
     * then selects the client so its panel is immediately visible.
     * 
     * La API retorna un DTO puro - esta función aplica la lógica de presentación.
     */
    async function _handleClienteAgregar(id) {
        if (!id) return;

        // If already in list → just reveal
        if (document.querySelector(`.ds-client-article[data-id="${CSS.escape(id)}"]`)) {
            _selectClient(id);
            return;
        }

        try {
            const { ok, data } = await DS.api.get(DS.endpoints.clients.detail(id));

            if (!ok || !data?.success || !data?.data) {
                DS.notify.error('Cliente no encontrado');
                return;
            }

            const clientViewModel = _mapDtoToViewModel(data.data);

            _injectClientArticle(clientViewModel);
            _injectDetailPanel(clientViewModel);
            _selectClient(clientViewModel.id);
            _syncFilterPills();
            DS.clientListStore?.add(clientViewModel.id);
            if (data.verificationState) _applyVerificationState(clientViewModel.id, data.verificationState);
        } catch {
            DS.notify.error('Error al cargar el cliente');
        }
    }

    /**
     * Mapea el DTO crudo del API (sin propiedades UI) a un ViewModel visual
     * completo con clases CSS, iconos, labels, etc.
     * Esta función asume la responsabilidad de presentación que NO debe estar en la API.
     */
    function _mapDtoToViewModel(dto) {
        const initials = _getInitials(dto.fullName || dto.identification);
        const verified = dto.isVerified;

        // Mapear contactos y direcciones aplicando lógica visual
        const contacts = [
            ...(dto.contacts   || []).map(c => _mapContactToViewModel(c)),
            ...(dto.addresses  || []).map(d => _mapAddressToViewModel(d))
        ];

        return {
            numericId: dto.id,
            id: dto.identification,
            name: dto.fullName || dto.identification,
            initials: initials,
            avatarUrl: null,
            status: verified ? 'Verificado' : 'Pendiente',
            statusClass: verified ? 'bg-green-100 text-green-800' : 'bg-orange-50 text-alert-ochre',
            location: null,
            contacts: contacts
        };
    }

    /**
     * Mapea un contacto del DTO a ContactViewModel con propiedades visuales.
     * Usa las propiedades exactas de ContactoDto: TipoMedioContacto, ValorContacto, EstadoVerificacion
     */
    function _mapContactToViewModel(contactDto) {
        const tipo = contactDto.contactMediumType;
        const hasError = EstadosContactabilidad.EsError(contactDto.verificationStatus);
        const verified = contactDto.verificationStatus === EstadosContactabilidad.Verificado2 && !hasError;
        
        return {
            id: contactDto.id,
            icon: _getContactIcon(tipo),
            label: _getContactLabel(tipo),
            value: contactDto.contactValue,
            tipoContacto: tipo,
            verified: verified,
            bgClass: hasError ? 'bg-red-50' : 'bg-background-light/30',
            borderClass: hasError ? 'border-red-100' : 'border-forest-green/5',
            valueClass: _shouldTruncate(tipo) ? 'truncate' : '',
            state: hasError ? 'error' : (verified ? 'verified' : 'pending'),
            iconType: _getContactIconType(tipo),
            errorCode: hasError ? contactDto.verificationStatus : '',
            errorLabel: hasError ? _getErrorLabel(contactDto.verificationStatus) : ''
        };
    }

    /**
     * Mapea una dirección del DTO a ContactViewModel (direcciones se muestran como contactos).
     * Usa las propiedades exactas de DireccionDto: IdDireccionCliente, TipoDireccion, DireccionCompleta, EstadoVerificacion
     */
    function _mapAddressToViewModel(addressDto) {
        const tipo = addressDto.addressType === 'DIR_T' || addressDto.addressType?.toUpperCase() === 'TRABAJO' ? 'DIR_T' : 'DIR_D';
        const estado = addressDto.verificationStatus || EstadosContactabilidad.Pendiente;
        const hasError = EstadosContactabilidad.EsError(estado);
        const verified = estado === EstadosContactabilidad.Verificado2 && !hasError;

        return {
            id: addressDto.id,
            icon: _getContactIcon(tipo),
            label: _getContactLabel(tipo),
            value: addressDto.fullAddress || '',
            tipoContacto: tipo,
            verified: verified,
            bgClass: hasError ? 'bg-red-50' : 'bg-background-light/30',
            borderClass: hasError ? 'border-red-100' : 'border-forest-green/5',
            valueClass: 'truncate',
            state: hasError ? 'error' : (verified ? 'verified' : 'pending'),
            iconType: 'location',
            errorCode: hasError ? estado : '',
            errorLabel: hasError ? _getErrorLabel(estado) : ''
        };
    }

    // Helper para verificar si un estado es error
    const EstadosContactabilidad = {
        Verificado2: 'VERIF',
        Verificado: 'Verificado',
        Pendiente: 'Pendiente',
        EsError: (estado) => estado?.startsWith('ERR') || false
    };

    // Helpers para mapping visual usando DS.catalogs
    function _getInitials(name) {
        if (!name) return '??';
        const parts = name.trim().split(/\s+/);
        if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
        if (parts.length === 1 && parts[0].length >= 2) return parts[0].substring(0, 2).toUpperCase();
        return parts[0][0].toUpperCase();
    }

    function _getContactIcon(tipo) {
        return DS.catalogs?.getContactIcon(tipo) || 'contact_page';
    }

    function _getContactLabel(tipo) {
        return DS.catalogs?.getContactLabel(tipo) || tipo;
    }

    function _getContactIconType(tipo) {
        return DS.catalogs?.getContactIconType(tipo) || 'phone';
    }

    function _shouldTruncate(tipo) {
        return DS.catalogs?.shouldTruncate(tipo) || false;
    }

    const _GENERIC_STATUS_LABELS = {
        'ERROR':       'Error de validación',
        'NORES':       'Sin respuesta',
        'CADU':        'Información vencida',
        'PEND-LOPDP':  'Pendiente LOPDP',
        'Deny-LOPDP':  'Rechazado LOPDP',
    };

    function _getErrorLabel(errorCode) {
        return DS.catalogs?.getErrorLabel(errorCode)
            || _GENERIC_STATUS_LABELS[errorCode]
            || errorCode;
    }

    /**
     * Injects a client article into #ds-client-list-body from a ClientViewModel.
     * Mirrors the structure produced by ds-search.js._buildArticleHTML so that
     * dashboard.js click delegation and _syncClientBadge work correctly.
     */
    function _injectClientArticle(client) {
        const body = document.getElementById('ds-client-list-body');
        if (!body) return;
        if (body.querySelector(`.ds-client-article[data-id="${CSS.escape(client.id)}"]`)) return;

        const estado     = client.status === 'Verificado' ? 'verificado' : 'por-verificar';
        const avatarHTML = client.avatarUrl
            ? `<img src="${DS.utils.escapeHtml(client.avatarUrl)}" alt="" />`
            : DS.utils.escapeHtml(client.initials ?? '??');

        // Build one icon per visual group (phone / email / location) with worst-wins state
        const ICON_TO_MATERIAL = { phone: 'phone', email: 'mail', location: 'location_on' };
        const ICON_TO_LABEL    = { phone: 'Teléfono', email: 'Correo', location: 'Dirección' };
        const STATE_CLASS      = { verified: 'ds-ci--verified', pending: 'ds-ci--pending', error: 'ds-ci--error' };
        const STATE_LABEL      = { verified: 'verificado',      pending: 'pendiente',       error: 'con error' };
        const PRIORITY         = { verified: 0, pending: 1, error: 2 };
        const GROUP_MAP        = { phone: 'phone', email: 'email', location: 'location' };
        const ICON_GROUP_MAP   = { phone: 'phone', email: 'email', location: 'address' };

        const groupState   = {};
        const groupPresent = {};

        (client.contacts ?? []).forEach(c => {
            const g = GROUP_MAP[c.iconType] ?? c.iconType;
            if (!ICON_TO_MATERIAL[g]) return;
            groupPresent[g] = true;
            const cur = PRIORITY[groupState[g]] ?? -1;
            if ((PRIORITY[c.state] ?? 1) > cur) groupState[g] = c.state;
        });

        const iconsHTML = Object.keys(groupPresent).map(g => {
            const state  = groupState[g] ?? 'pending';
            const mat    = ICON_TO_MATERIAL[g];
            const label  = ICON_TO_LABEL[g];
            const cls    = STATE_CLASS[state] ?? 'ds-ci--pending';
            const stLbl  = STATE_LABEL[state] ?? 'pendiente';
            return `<span class="material-symbols-outlined ds-client-article__contact-icon ds-ci ${cls}"
                         title="${DS.utils.escapeHtml(label)}"
                         aria-label="${DS.utils.escapeHtml(label)} - ${stLbl}"
                         role="img" data-icon-group="${ICON_GROUP_MAP[g] ?? g}">${mat}</span>`;
        }).join('');

        const art = document.createElement('article');
        art.className = 'ds-client-article ds-client-article--highlight';
        art.setAttribute('role', 'button');
        art.setAttribute('tabindex', '0');
        art.dataset.id     = client.id;
        art.dataset.estado = estado;
        art.setAttribute('aria-label', `Seleccionar cliente ${DS.utils.escapeHtml(client.name)}`);
        art.innerHTML = `
            <div class="ds-client-article__avatar-wrap">
                <div class="ds-client-article__avatar ds-client-article__avatar--person" aria-hidden="true">
                    ${avatarHTML}
                </div>
            </div>
            <div class="ds-client-article__info">
                <div class="ds-client-article__top">
                    <span class="ds-client-article__name">${DS.utils.escapeHtml(client.name)}</span>
                    <span class="ds-client-article__id">${DS.utils.escapeHtml(client.id)}</span>
                </div>
                <div class="ds-client-article__icons" aria-label="Tipos de contacto">
                    ${iconsHTML}
                </div>
            </div>
            <div class="ds-client-article__actions">
                <button class="ds-client-article__remove-btn" type="button"
                        aria-label="Eliminar cliente ${DS.utils.escapeHtml(client.name)}">
                    <span class="material-symbols-outlined" aria-hidden="true">close</span>
                </button>
            </div>`;

        // Clear skeleton / loading placeholder on first real inject
        body.querySelector('.ds-client-list__loading-msg')?.remove();
        body.querySelectorAll('.ds-client-article--skeleton').forEach(s => s.remove());

        body.prepend(art);
        requestAnimationFrame(() =>
            setTimeout(() => art.classList.remove('ds-client-article--highlight'), 800));
    }

    /**
     * Injects the full detail panel into .dash-detail from a ClientViewModel.
     * HTML structure exactly matches the server-rendered panel in Dashboard.cshtml so
     * that _bindContactActions event delegation and _syncClientBadge work correctly.
     */
    function _injectDetailPanel(client) {
        const section = document.querySelector('.dash-detail');
        if (!section) return;
        if (section.querySelector(`#detail-${CSS.escape(client.id)}`)) return;

        const contacts = client.contacts ?? [];
        const phones   = contacts.filter(c => ['TEL_C', 'CEL', 'WAPP'].includes(c.tipoContacto));
        const emails   = contacts.filter(c => c.tipoContacto === 'EMAIL');
        const addrs    = contacts.filter(c => ['DIR_D', 'DIR_T'].includes(c.tipoContacto));

        const isVerif      = client.status === 'Verificado';
        const statusPilCls = `dash-detail__status-pill ${
            isVerif ? 'dash-detail__status-pill--verified' : 'dash-detail__status-pill--pending'}`;
        const statusText   = `ESTADO: ${isVerif ? 'VERIFICADO' : 'PENDIENTE'}`;
        const avatarHTML   = client.avatarUrl
            ? `<img src="${DS.utils.escapeHtml(client.avatarUrl)}" alt="" />`
            : DS.utils.escapeHtml(client.initials ?? '??');
        const locationHTML = client.location
            ? `<span class="dash-detail__location">
                   <svg xmlns="http://www.w3.org/2000/svg" width="11" height="13" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                       <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
                   </svg>
                   ${DS.utils.escapeHtml(client.location)}
               </span>` : '';
        const noContactsHTML = contacts.length === 0
            ? `<div class="dash-detail__empty">
                   <img src="/icons/article-open.svg" class="ds-icon" style="width:36px;height:36px;opacity:.4" aria-hidden="true" alt="" />
                   <p>Sin contactos registrados</p>
               </div>` : '';

        const panel = document.createElement('div');
        panel.className           = 'dash-detail__panel';
        panel.id                  = `detail-${client.id}`;
        panel.dataset.clientId        = client.id;
        panel.dataset.clientNumericId = String(client.numericId);
        panel.hidden              = true;
        panel.innerHTML = `
            <div class="dash-detail__header">
                <button class="dash-detail__back-btn" type="button" aria-label="Volver a la lista de clientes">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
                         fill="none" stroke="currentColor" stroke-width="2.5"
                         stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <path d="M19 12H5"/><path d="M12 19l-7-7 7-7"/>
                    </svg>
                    Volver
                </button>
                <div class="dash-detail__client-row">
                    <div class="dash-detail__avatar" aria-hidden="true">${avatarHTML}</div>
                    <div class="dash-detail__client-info">
                        <div class="dash-detail__name-row">
                            <h2 class="dash-detail__name">${DS.utils.escapeHtml(client.name)}</h2>
                            <a href="/clientes?id=${encodeURIComponent(client.id)}"
                               class="dash-detail__meta-btn"
                               title="Ver perfil completo del cliente"
                               data-cliente-perfil-id="${DS.utils.escapeHtml(client.id)}"
                               data-cliente-perfil-name="${DS.utils.escapeHtml(client.name)}">
                                <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24"
                                     fill="none" stroke="currentColor" stroke-width="2.5"
                                     stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                                    <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>
                                    <polyline points="15 3 21 3 21 9"/>
                                    <line x1="10" y1="14" x2="21" y2="3"/>
                                </svg>
                                MÁS DETALLES
                            </a>
                        </div>
                        <div class="dash-detail__meta-row">
                            <span class="${statusPilCls}">${statusText}</span>
                            <span class="dash-detail__meta-sep" aria-hidden="true"></span>
                            <span class="dash-detail__id-pill">Identificación:&nbsp;${DS.utils.escapeHtml(client.id)}</span>
                            ${locationHTML}
                        </div>
                    </div>
                </div>
            </div>
            ${DS.contactFilter.buildBarHTML()}
            <div class="dash-detail__body">
                ${_buildDashSection('Teléfonos',   'section-phone.svg',    phones, client.id, client.numericId)}
                ${_buildDashSection('Correos',      'section-email.svg',    emails, client.id, client.numericId)}
                ${_buildDashSection('Direcciones', 'section-location.svg', addrs,  client.id, client.numericId)}
                ${noContactsHTML}
            </div>`;

        section.appendChild(panel);
        DS.contactFilter.init(panel, { sectionSelector: '.dash-detail__section' });
    }

    function _buildDashSection(title, iconFile, contacts, clientId, numericId) {
        if (!contacts.length) return '';
        return `
            <div class="dash-detail__section">
                <h3 class="dash-detail__section-title">
                    <img src="/icons/${iconFile}" class="ds-icon ds-icon--sm" aria-hidden="true" alt="" />
                    ${title}
                </h3>
                ${contacts.map(c => _buildDashContactRow(c, clientId, numericId)).join('')}
            </div>`;
    }

    function _buildDashContactRow(contact, clientId, numericId) {
        const isVerified = contact.verified;
        const isError    = contact.state === 'error';
        const rowCls     = [
            'dash-contact-row',
            isVerified ? 'dash-contact-row--verified' : '',
            isError    ? 'dash-contact-row--error'    : '',
        ].filter(Boolean).join(' ');

        const ROW_ICONS  = { phone: '/icons/row-phone.svg', email: '/icons/row-email.svg', location: '/icons/row-location.svg' };
        const rowIcon    = ROW_ICONS[contact.iconType] ?? '/icons/row-phone.svg';
        const isEmail    = contact.tipoContacto === 'EMAIL';
        const valCls     = isEmail ? 'dash-contact-row__value dash-contact-row__value--truncate' : 'dash-contact-row__value';
        const titleAttr  = isEmail ? `title="${DS.utils.escapeHtml(contact.value)}"` : '';
        const badgeHTML  = (!isEmail && contact.label)
            ? `<span class="ds-badge ds-badge--neutral dash-contact-row__type-badge">${DS.utils.escapeHtml(contact.label)}</span>`
            : '';
        const hintHTML   = isError && contact.errorLabel
            ? `<div class="dash-contact-row__error-hint">
                   <span class="material-symbols-outlined" aria-hidden="true">cancel</span>
                   <span>${DS.utils.escapeHtml(contact.errorLabel)}</span>
               </div>` : '';

        return `
            <div class="${rowCls}"
                 data-contact-id="${DS.utils.escapeHtml(String(contact.id))}"
                 data-contact-value="${DS.utils.escapeHtml(contact.value)}"
                 data-contact-type="${DS.utils.escapeHtml(contact.tipoContacto)}"
                 data-client-id="${DS.utils.escapeHtml(clientId)}"
                 data-client-numeric-id="${DS.utils.escapeHtml(String(numericId))}"
data-contact-state="${isVerified ? 'verified' : isError ? 'error' : 'pending'}">
                <div class="dash-contact-row__info">
                    <img src="${rowIcon}" class="ds-icon dash-contact-row__icon" aria-hidden="true" alt="" />
                    <span class="${valCls}" ${titleAttr}>${DS.utils.escapeHtml(contact.value)}</span>
                    ${badgeHTML}
                </div>
                <div class="dash-contact-row__actions">
                    <button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm contact-verify-btn${isVerified ? ' contact-verify-btn--verified' : ''}"
                            ${isVerified ? 'disabled' : ''}
                            title="${isVerified ? 'Verificado' : 'Verificar'}"
                            aria-label="${isVerified ? 'Contacto verificado' : 'Verificar contacto'}">
                        <img src="/icons/verify-check.svg" class="ds-icon contact-verify-icon${isVerified ? ' is-verified' : ''}" aria-hidden="true" alt="" />
                    </button>
                    <button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm contact-reject-btn${isError ? ' is-active' : ''}"
                            title="Opciones de validación"
                            aria-label="Abrir opciones de validación">
                        <img src="/icons/note-remove.svg" class="ds-icon" aria-hidden="true" alt="" />
                    </button>
                    <button class="ds-btn ds-btn--ghost ds-btn--icon ds-btn--sm contact-edit-btn"
                            title="Editar"
                            aria-label="Editar dato de contacto">
                        <img src="/icons/edit.svg" class="ds-icon" aria-hidden="true" alt="" />
                    </button>
                </div>
                ${hintHTML}
            </div>`;
    }

    /* ──────────────────────────────────────────────────
       Bootstrap
    ────────────────────────────────────────────────── */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
