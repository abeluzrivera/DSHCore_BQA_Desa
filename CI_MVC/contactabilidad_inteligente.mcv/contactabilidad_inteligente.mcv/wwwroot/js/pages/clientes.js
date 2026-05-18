/**
 * clientes.js — Perfil de Cliente: Contactabilidad Handlers
 * Manages modals, contact verification, rejection flow, and form submissions
 */

(() => {
    'use strict';

    // ─────────────────────────────────────────────────────
    // Modal Initialization
    // ─────────────────────────────────────────────────────
    
    function _initModals() {
        // Modal open buttons
        document.querySelectorAll('.cp-nuevo-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const modalId = btn.dataset.modal;
                if (modalId) {
                    DS.modal.open(modalId);
                }
            });
        });

        // Modal close buttons
        document.querySelectorAll('[data-modal-close]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                DS.modal.close();
            });
        });

        // Close on backdrop click
        document.querySelectorAll('.ds-modal-backdrop').forEach(backdrop => {
            backdrop.addEventListener('click', (e) => {
                if (e.target === backdrop) {
                    DS.modal.close();
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Edit Toggle — Show/Hide Action Pills + Card Editing Mode
    // ─────────────────────────────────────────────────────
    
    function _initEditToggle() {
        document.querySelectorAll('.cp-edit-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const card = btn.closest('.cp-contact-card');
                if (!card) return;

                const pills = card.querySelector('.cp-action-pills');
                if (pills) {
                    const isNowEditing = pills.hidden;
                    pills.hidden = !isNowEditing;

                    // Update button and card appearance with CSS classes
                    btn.classList.toggle('cp-edit-btn--active', isNowEditing);
                    card.classList.toggle('cp-contact-card--editing', isNowEditing);
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Verify Contact — POST to 
    //          /api/clientes / ${ clientId } /address/${ contactId } /verify 
    //          /api / clientes / ${ clientId } /contacts/${ contactId } /verify
    // ─────────────────────────────────────────────────────
    
    function _initVerifyPills() {
        document.querySelectorAll('[data-action="verify"]').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.preventDefault();
                const card = btn.closest('.cp-contact-card');
                if (!card) return;

                const clientId = card.dataset.clientId;
                const contactType = card.dataset.contactType;
                const contactId = card.dataset.contactId;

                if (!clientId || !contactId) {
                    DS.notify?.error('Datos inválidos o faltantes en la tarjeta.');
                    return;
                }

                // Deshabilitar el botón temporalmente para evitar doble clic
                btn.disabled = true;

                try {
                    // 1. Determinar si es una dirección o un contacto (teléfono/email)
                    const isAddress = contactType === 'DIR_D' || contactType === 'DIR_T';

                    // 2. Construir la URL RESTful hacia el nuevo API Controller
                    const endpoint = isAddress
                        ? `/api/clientes/${clientId}/address/${contactId}/verify`
                        : `/api/clientes/${clientId}/contacts/${contactId}/verify`;

                    // 3. Usar nuestro DS.api (Axios) - Nota: Es un POST, aunque no mandemos body
                    const { ok, data } = await DS.api.post(endpoint, {});

                    // 4. Actualizar la UI si fue exitoso
                    if (ok) {
                        DS.notify?.success('Verificado correctamente');

                        // --- ACTUALIZACIÓN VISUAL DE LA TARJETA ---

                        // A. Clases de estado general de la tarjeta
                        card.classList.remove('cp-contact-card--pending', 'cp-contact-card--error');
                        card.classList.add('cp-contact-card--verified');

                        // B. Actualizar el icono de estado en la parte superior derecha
                        const stateIconBox = card.querySelector('.cp-state-icon');
                        if (stateIconBox) {
                            stateIconBox.classList.remove('cp-state-icon--pending', 'cp-state-icon--error');
                            stateIconBox.classList.add('cp-state-icon--verified');
                            const iconSpan = stateIconBox.querySelector('.material-symbols-outlined');
                            if (iconSpan) iconSpan.textContent = 'check_circle';
                        }

                        // C. Ocultar los botones de acción (Verificar, Validación, LOPDP)
                        const actionPills = card.querySelector('.cp-action-pills');
                        if (actionPills) actionPills.hidden = true;

                        // D. Ocultar mensajes de error previos (si los hubiera)
                        const errorHint = card.querySelector('.cp-error-hint');
                        if (errorHint) errorHint.style.display = 'none';

                        // E. Actualizar el icono grande principal (Ajuste para Tailwind)
                        const iconBox = card.querySelector('.cp-contact-card__icon-box');
                        if (iconBox) {
                            // Quitamos las clases de color pendientes/errores
                            iconBox.classList.remove(
                                'cp-contact-card__icon-box--pending', 'cp-contact-card__icon-box--error', // Viejas
                                'tw-bg-pending-bg', 'tw-border-pending-border', 'tw-text-pending-text',   // Tailwind Pending
                                'tw-bg-error-bg', 'tw-border-error-border', 'tw-text-error-text'          // Tailwind Error
                            );
                            // Agregamos las de verificado
                            iconBox.classList.add(
                                'tw-bg-verified-bg', 'tw-border-verified-border', 'tw-text-verified-text'
                            );
                        }

                        // E2. Actualizar la clase del value-row (fila con icono + valor)
                        const valueRow = card.querySelector('[class*="cp-contact-card__value-row--"]');
                        if (valueRow) {
                            valueRow.classList.remove(
                                'cp-contact-card__value-row--pending',
                                'cp-contact-card__value-row--error',
                                'cp-contact-card__value-row--unknown'
                            );
                            valueRow.classList.add('cp-contact-card__value-row--verified');
                        }

                        // F. Sincronizar data-contact-state → re-aplicar filtro activo
                        _syncCardFilter(card, 'verified');
                    } else {
                        // El error ya fue notificado por el interceptor de ds-api.js, 
                        // pero habilitamos el botón de nuevo por si quiere reintentar.
                        btn.disabled = false;
                    }
                } catch (err) {
                    console.error('Excepción al verificar:', err);
                    btn.disabled = false;
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Reject Zone Toggle
    // ─────────────────────────────────────────────────────
    
    function _initRejectToggle() {
        document.querySelectorAll('[data-action="reject-toggle"]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const card = btn.closest('.cp-contact-card');
                if (!card) return;

                const rejectZone = card.querySelector('.cp-reject-zone');
                if (rejectZone) {
                    const isHidden = rejectZone.hidden;
                    rejectZone.hidden = !isHidden;
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Reject Pill Selection — POST to
    //          /api/clientes / ${ clientId } /address/${ contactId } /Unverify / ${ errorCode }
    //          /api / clientes / ${ clientId } /contacts/${ contactId } /Unverify / ${ errorCode }
    // ─────────────────────────────────────────────────────
    
    function _initRejectPills() {
        document.querySelectorAll('.cp-reject-pill').forEach(pill => {
            pill.addEventListener('click', async (e) => {
                e.preventDefault();
                const card = pill.closest('.cp-contact-card');
                if (!card) return;

                const clientId = card.dataset.clientId;
                const contactType = card.dataset.contactType;
                // Usamos dataset.contactId porque en el código original usabas contactValue pero el Controller espera el ID
                const contactId = card.dataset.contactId;
                const errorCode = pill.dataset.errorCode;

                if (!clientId || !contactId || !errorCode) {
                    DS.notify?.error('Datos inválidos o faltantes en la tarjeta.');
                    return;
                }

                // Deshabilitar el botón temporalmente para evitar doble clic
                pill.disabled = true;

                try {
                    // 1. Determinar si es una dirección o un contacto (teléfono/email)
                    const isAddress = contactType === 'DIR_D' || contactType === 'DIR_T';

                    // 2. Construir la URL RESTful hacia el nuevo API Controller
                    // La ruta incluye el errorCode como parámetro de URL
                    const endpoint = isAddress
                        ? DS.endpoints.clients.unverifyAddress(clientId, contactId, errorCode)
                        : DS.endpoints.clients.unverifyContact(clientId, contactId, errorCode);

                    // 3. Usar nuestro DS.api (Axios) - Es un POST sin body
                    const { ok, data } = await DS.api.post(endpoint, {});

                    // 4. Actualizar la UI si fue exitoso
                    if (ok) {
                        DS.notify?.success('Motivo de rechazo registrado');

                        // --- ACTUALIZACIÓN VISUAL DE LA TARJETA ---

                        // A. Clases de estado general de la tarjeta
                        card.classList.remove('cp-contact-card--pending', 'cp-contact-card--verified');
                        card.classList.add('cp-contact-card--error');

                        // B. Actualizar el icono de estado en la parte superior derecha
                        const stateIconBox = card.querySelector('.cp-state-icon');
                        if (stateIconBox) {
                            stateIconBox.classList.remove('cp-state-icon--pending', 'cp-state-icon--verified');
                            stateIconBox.classList.add('cp-state-icon--error');
                            const iconSpan = stateIconBox.querySelector('.material-symbols-outlined');
                            if (iconSpan) iconSpan.textContent = 'cancel';
                        }

                        // C. Mostrar u ocultar controles
                        const pillsContainer = card.querySelector('.cp-action-pills');
                        if (pillsContainer) pillsContainer.hidden = true;

                        const rejectZone = card.querySelector('.cp-reject-zone');
                        if (rejectZone) rejectZone.hidden = true;

                        // D. Crear o actualizar el cartel de error (El recuadro rojo con el texto)
                        let errorHint = card.querySelector('.cp-error-hint');
                        if (!errorHint) {
                            // Lo creamos y lo añadimos al final de la tarjeta (banda inferior)
                            errorHint = document.createElement('div');
                            errorHint.className = 'cp-error-hint';
                            card.appendChild(errorHint);
                        } else {
                            errorHint.style.display = 'flex';
                        }

                        // Actualizamos el texto. _getErrorLabel devuelve el texto a partir de 'ERR_MAIL_DOM'
                        const labelText = typeof _getErrorLabel === 'function' ? _getErrorLabel(errorCode) : errorCode;
                        errorHint.innerHTML = `<span class="material-symbols-outlined">cancel</span> ${labelText}`;

                        // E. Actualizar el icono grande principal (Ajuste para Tailwind)
                        const iconBox = card.querySelector('.cp-contact-card__icon-box');
                        if (iconBox) {
                            // Quitamos clases de pendiente y verificado
                            iconBox.classList.remove(
                                'cp-contact-card__icon-box--pending', 'cp-contact-card__icon-box--verified', // Viejas
                                'tw-bg-pending-bg', 'tw-border-pending-border', 'tw-text-pending-text',   // Tailwind Pending
                                'tw-bg-verified-bg', 'tw-border-verified-border', 'tw-text-verified-text' // Tailwind Verified
                            );
                            // Agregamos las de error
                            iconBox.classList.add(
                                'tw-bg-error-bg', 'tw-border-error-border', 'tw-text-error-text'
                            );
                        }

                        // E2. Actualizar la clase del value-row (fila con icono + valor)
                        const valueRow = card.querySelector('[class*="cp-contact-card__value-row--"]');
                        if (valueRow) {
                            valueRow.classList.remove(
                                'cp-contact-card__value-row--pending',
                                'cp-contact-card__value-row--verified',
                                'cp-contact-card__value-row--unknown'
                            );
                            valueRow.classList.add('cp-contact-card__value-row--error');
                        }

                        // F. Sincronizar data-contact-state → re-aplicar filtro activo
                        _syncCardFilter(card, 'error');

                            // Opcional: Si quieres que el icono en sí (ej. el teléfono) se vuelva una X
                            // const mainIcon = iconBox.querySelector('.material-symbols-outlined');
                            // if(mainIcon) mainIcon.textContent = 'warning';
                    } else {
                        pill.disabled = false;
                    }
                } catch (err) {
                    console.error('Excepción al rechazar:', err);
                    pill.disabled = false;
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // LOPDP Placeholder Button
    // ─────────────────────────────────────────────────────
    
    function _initLopdpButtons() {
        document.querySelectorAll('[data-lopdp]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                DS.notify.info('LOPDP — Disponible próximamente');
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Form Submission: Add Email
    // ─────────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────
    // Form Submission: Add Email
    // ─────────────────────────────────────────────────────
    function _initAddEmailForm() {
        const form = document.getElementById('form-nuevo-email');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const submitBtn = form.querySelector('button[type="submit"]');
            const valor = form.querySelector('input[name="valor"]')?.value?.trim();

            if (!valor) {
                DS.notify?.error('Ingrese un correo electrónico');
                return;
            }

            const clientId = document.querySelector('[data-numeric-id]')?.dataset?.numericId;
            if (!clientId) {
                DS.notify?.error('No se pudo obtener el ID del cliente');
                return;
            }

            // Bloquear botón (UX)
            const originalBtnText = submitBtn ? submitBtn.innerHTML : '';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="material-symbols-outlined tw-animate-spin tw-mr-2">autorenew</span> Guardando...';
            }

            try {
                const enumEmailValue = parseInt(form.querySelector('input[name="tipo"]').value, 10);

                const payload = {
                    contactType: enumEmailValue,
                    contactValue: valor
                };

                const endpoint = `/api/clientes/${clientId}/contacts`;
                const { ok } = await DS.api.post(endpoint, payload);

                if (ok) {
                    DS.notify?.success('Correo agregado correctamente');
                    form.reset();
                    if (window.DS && DS.modal) DS.modal.close();
                    if (typeof _reloadContactSection === 'function') _reloadContactSection('emails-list');
                }
            } catch (err) {
                console.error('Error al agregar correo:', err);
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;
                }
            }
        });
    }

    // ─────────────────────────────────────────────────────
    // Form Submission: Add Phone
    // ─────────────────────────────────────────────────────
    function _initAddPhoneForm() {
        const form = document.getElementById('form-nuevo-telefono');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const submitBtn = form.querySelector('button[type="submit"]');
            const valor = form.querySelector('input[name="valor"]')?.value?.trim();
            const tipoValue = form.querySelector('select[name="tipo"]')?.value?.trim();

            if (!valor) {
                DS.notify?.error('Ingrese un número de teléfono');
                return;
            }

            // Parseamos el Enum (Ojo: asegúrate de que el HTML del <select> tenga values como "1", "3", "4")
            const tipoContactoInt = parseInt(tipoValue, 10);
            if (isNaN(tipoContactoInt)) {
                DS.notify?.error('Seleccione el tipo de teléfono válido');
                return;
            }

            const clientId = document.querySelector('[data-numeric-id]')?.dataset?.numericId;
            if (!clientId) {
                DS.notify?.error('No se pudo obtener el ID del cliente');
                return;
            }

            // Bloquear botón (UX)
            const originalBtnText = submitBtn ? submitBtn.innerHTML : '';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="material-symbols-outlined tw-animate-spin tw-mr-2">autorenew</span> Guardando...';
            }

            try {
                const payload = {
                    contactType: tipoContactoInt,
                    contactValue: valor
                };

                const endpoint = `/api/clientes/${clientId}/contacts`;
                const { ok } = await DS.api.post(endpoint, payload);

                if (ok) {
                    DS.notify?.success('Teléfono agregado correctamente');
                    form.reset();
                    if (window.DS && DS.modal) DS.modal.close();
                    if (typeof _reloadContactSection === 'function') _reloadContactSection('phones-list');
                }
            } catch (err) {
                console.error('Error al agregar teléfono:', err);
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;
                }
            }
        });
    }

    // ─────────────────────────────────────────────────────
    // Form Submission: Add Direccion
    // ─────────────────────────────────────────────────────
    
    function _initAddDireccionForm() {
        const form = document.getElementById('form-nueva-direccion');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            // 1. Obtener referencias
            const submitBtn = form.querySelector('button[type="submit"]');
            const direccion = form.querySelector('input[name="direccion"]')?.value?.trim();
            const tipoValue = form.querySelector('select[name="tipo"]')?.value?.trim();

            // 2. Validaciones locales (Fail Fast en el Frontend)
            if (!direccion) {
                DS.notify?.error('Ingrese una dirección completa.');
                return;
            }

            // OJO: Tu C# ahora espera un Enum (entero). Nos aseguramos de parsearlo.
            // Asume que tu HTML <select> tiene values como "5" (HomeAddress) o "6" (WorkAddress)
            const tipoContactoInt = parseInt(tipoValue, 10);
            if (isNaN(tipoContactoInt)) {
                DS.notify?.error('Seleccione un tipo de dirección válido.');
                return;
            }

            const clientId = document.querySelector('[data-numeric-id]')?.dataset?.numericId;
            if (!clientId) {
                DS.notify?.error('No se pudo obtener el ID del cliente.');
                return;
            }

            // 3. Bloquear el botón para evitar envíos duplicados (Mejor UX)
            const originalBtnText = submitBtn ? submitBtn.innerHTML : '';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="material-symbols-outlined tw-animate-spin tw-mr-2">autorenew</span> Guardando...';
            }

            try {
                // 4. Armar el Payload (coincide con AddAddressRequest en C#)
                const latRaw  = form.querySelector('input[name="latitud"]')?.value?.trim();
                const lngRaw  = form.querySelector('input[name="longitud"]')?.value?.trim();
                const latVal  = latRaw  !== '' && latRaw  != null ? parseFloat(latRaw)  : null;
                const lngVal  = lngRaw  !== '' && lngRaw  != null ? parseFloat(lngRaw)  : null;

                const payload = {
                    addressType: tipoContactoInt,
                    fullAddress: direccion,
                    city:        form.querySelector('input[name="ciudad"]')?.value?.trim()       || null,
                    province:    form.querySelector('input[name="provincia"]')?.value?.trim()    || null,
                    country:     form.querySelector('input[name="pais"]')?.value?.trim()         || null,
                    postalCode:  form.querySelector('input[name="codigoPostal"]')?.value?.trim() || null,
                    latitude:    !isNaN(latVal) ? latVal : null,
                    longitude:   !isNaN(lngVal) ? lngVal : null,
                };

                // 5. Llamada RESTful al Controller
                const endpoint = `/api/clientes/${clientId}/addresses`;
                const { ok, data } = await DS.api.post(endpoint, payload);

                // 6. Procesar Respuesta Exitosa
                if (ok) {
                    DS.notify?.success('Dirección agregada correctamente');
                    form.reset();

                    // Cerramos el modal usando tu Design System
                    if (window.DS && DS.modal) DS.modal.close();

                    // Recargamos la sección
                    if (typeof _reloadContactSection === 'function') {
                        _reloadContactSection('addresses-list');
                    }
                }
                // Nota: El 'else' de error ya no es necesario aquí porque tu interceptor de ds-api.js 
                // ya detecta los BadRequest (400) y lanza el DS.notify.error() automáticamente.

            } catch (err) {
                console.error('Excepción al agregar dirección:', err);
                // DS.api.post ya maneja el notify.error genérico para fallos de red
            } finally {
                // 7. Desbloquear el botón
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;
                }
            }
        });
    }

    // ─────────────────────────────────────────────────────
    // Sidebar — tab switching + seccion exclusiva por sub-item
    // ─────────────────────────────────────────────────────

    function _showContactSection(sub) {
        var panel = document.getElementById('tab-contactabilidad');
        if (!panel) return;
        panel.querySelectorAll(':scope > .cp-section').forEach(function(sec) {
            sec.hidden = sec.id !== 'section-' + sub;
        });
    }

    function _initSidebar() {
        var _mapPreselected = false;
        document.querySelectorAll('.cp-sidebar__subitem').forEach(function(item) {
            item.addEventListener('click', function() {
                var sub = item.dataset.sub;
                _showContactSection(sub);
                document.querySelectorAll('.cp-sidebar__subitem').forEach(function(s) { s.classList.remove('cp-sidebar__subitem--active'); });
                item.classList.add('cp-sidebar__subitem--active');
                // Carga diferida del mapa: sólo la primera vez que el usuario abre Direcciones
                if (sub === 'addresses' && !_mapPreselected) {
                    _mapPreselected = true;
                    _preselectFirstAddress();
                }
            });
        });
        var activeSubItem = document.querySelector('.cp-sidebar__subitem--active');
        if (activeSubItem && activeSubItem.dataset.sub) { _showContactSection(activeSubItem.dataset.sub); }
        document.querySelectorAll('.cp-sidebar__item').forEach(function(item) {
            item.addEventListener('click', function() {
                var tab = item.dataset.tab;
                document.querySelectorAll('.cp-sidebar__item').forEach(function(s) {
                    s.classList.remove('cp-sidebar__item--active');
                    s.setAttribute('aria-selected', 'false');
                });
                item.classList.add('cp-sidebar__item--active');
                item.setAttribute('aria-selected', 'true');
                document.querySelectorAll('.cp-tab-panel').forEach(function(p) {
                    var isActive = p.id === 'tab-' + tab;
                    p.hidden = !isActive;
                    p.classList.toggle('cp-tab-panel--active', isActive);
                });
                var subnav = document.querySelector('.cp-sidebar__subnav');
                if (subnav) subnav.hidden = (tab !== 'contactabilidad');
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Address Map Panel — selección + actualización del iframe
    // ─────────────────────────────────────────────────────

    function _initAddressMapPanel() {
        const mapFrame = /** @type {HTMLIFrameElement|null} */ (document.getElementById('cp-map-frame'));
        const addrList = document.getElementById('addresses-list');
        if (!addrList) return;

        // Click en tarjeta de dirección → actualizar panel de mapa
        addrList.addEventListener('click', e => {
            const card = e.target.closest('.cp-contact-card');
            if (!card) return;
            // Ignorar clics en zonas de acción y campos editables GPS
            if (e.target.closest('.cp-action-pills, .cp-reject-zone, [data-map-btn], .cp-edit-btn, [data-gps-lat], [data-gps-lng]')) return;

            const lat = card.dataset.lat;
            const lng = card.dataset.lng;
            if (lat && lng && mapFrame) {
                _updateMapPanel(lat, lng);
            }
            // Resaltar tarjeta seleccionada
            addrList.querySelectorAll('.cp-contact-card').forEach(c => c.classList.remove('cp-contact-card--selected'));
            card.classList.add('cp-contact-card--selected');
        });

        // Botón "open" → abrir en Google Maps en nueva pestaña
        document.querySelector('[data-map-ctrl="open"]')?.addEventListener('click', () => {
            if (!mapFrame?.src) return;
            const match = mapFrame.src.match(/q=([-\d.]+),([-\d.]+)/);
            if (match) window.open(`https://maps.google.com/?q=${match[1]},${match[2]}`, '_blank', 'noopener');
        });
    }

    /** Actualiza el iframe del panel de mapa con nuevas coordenadas. */
    function _updateMapPanel(lat, lng) {
        const panel = document.getElementById('cp-map-panel');
        if (!panel) return;
        // Si el panel muestra el estado vacío lo sustituimos con el iframe
        const emptyEl = panel.querySelector('.cp-map-panel__empty');
        if (emptyEl) {
            const iframe = document.createElement('iframe');
            iframe.id = 'cp-map-frame';
            iframe.className = 'cp-map-panel__iframe';
            iframe.title = 'Mapa de dirección';
            iframe.setAttribute('aria-label', 'Mapa de la dirección del cliente');
            iframe.loading = 'lazy';
            emptyEl.replaceWith(iframe);
        }
        const frame = /** @type {HTMLIFrameElement} */ (document.getElementById('cp-map-frame'));
        if (frame) {
            frame.src = `https://www.google.com/maps?q=${lat},${lng}&z=16&output=embed`;
        }
    }


    // Usa DS.contactFilter (ds-core.js) con [data-contact-state] como fuente de verdad.
    // ─────────────────────────────────────────────────────

    function _initContactFilter() {
        document.querySelectorAll('.cp-section').forEach(section => {
            DS.contactFilter?.init(section);
        });
    }

    /** Sincroniza data-contact-state en la tarjeta y re-aplica el filtro de su sección. */
    function _syncCardFilter(card, newState) {
        if (!card) return;
        card.dataset.contactState = newState;
        DS.contactFilter?.reapply(card.closest('.cp-section'));
        // Reordenar las tarjetas del contenedor, empujando la modificada al final de su grupo
        const container = card.closest('.cp-cards-grid, .cp-cards-grid--2col, .cp-cards-stack');
        if (container) _sortContactCards(container, card);
    }

    /**
     * Ordena las tarjetas de un contenedor por estado: verified → pending → error/unknown.
     * Si se pasa `movedCard`, se mueve primero al final del DOM para que quede
     * al final de su grupo tras el sort estable.
     */
    function _sortContactCards(container, movedCard = null) {
        const statePriority = { verified: 1, pending: 2, error: 3, unknown: 3 };
        const isAddresses = container.id === 'addresses-list';

        if (movedCard && container.contains(movedCard)) {
            container.appendChild(movedCard);
        }

        const cards = Array.from(container.querySelectorAll(':scope > .cp-contact-card'));
        cards
            .sort((a, b) => {
                const sp = c => statePriority[c.dataset.contactState] ?? 2;
                // Para direcciones: bloque 0–3 (con GPS) y bloque 4–6 (sin GPS)
                const gpsOffset = isAddresses
                    ? c => (c.dataset.lat && c.dataset.lng ? 0 : 3)
                    : () => 0;
                return (gpsOffset(a) + sp(a)) - (gpsOffset(b) + sp(b));
            })
            .forEach(c => container.appendChild(c));
    }


    
    function _getErrorLabel(errorCode) {
        // Leemos el catálogo inyectado desde C#
        const labels = window.DS?.catalogs?.errorLabels || {};

        // Si el código existe (ej. 'ERR_MAIL_DOM'), devuelve "Dominio inalcanzable"
        // Si no lo encuentra por alguna razón, devuelve el código crudo como respaldo
        return labels[errorCode] || errorCode;
    }

    // ─────────────────────────────────────────────────────
    // Helper: Reload contact section via AJAX
    // ─────────────────────────────────────────────────────
    
    async function _reloadContactSection(sectionId) {
        const clientId = document.querySelector('.cp-header__client-id')?.textContent?.trim();
        if (!clientId) return;

        // Buscamos la sección actual antes de hacer la petición
        const oldList = document.getElementById(sectionId);
        const oldSection = oldList?.closest('.cp-section');

        if (!oldSection) return;

        // 1. Feedback visual (Mejora de UX): Opacamos la sección y bloqueamos clics
        oldSection.style.opacity = '0.5';
        oldSection.style.pointerEvents = 'none';
        oldSection.style.transition = 'opacity 0.2s ease';

        try {
            // 2. Usamos nuestra instancia centralizada de Axios en lugar de fetch puro
            // Como el servidor devuelve HTML, Axios pondrá ese string en 'data'
            const { ok, data: htmlString } = await DS.api.get(`/Clientes?id=${clientId}`);

            if (ok && htmlString) {
                // 3. Parseamos el HTML
                const parser = new DOMParser();
                const doc = parser.parseFromString(htmlString, 'text/html');

                // 4. Buscamos la nueva sección
                const newList = doc.getElementById(sectionId);
                const newSection = newList?.closest('.cp-section');

                if (newSection) {
                    // 5. Reemplazamos el nodo en el DOM
                    oldSection.replaceWith(newSection);

                    // 6. ¡CRÍTICO! Reinicializar todos los eventos porque los botones son nuevos
                    if (typeof _initEditToggle === 'function') _initEditToggle();
                    if (typeof _initVerifyPills === 'function') _initVerifyPills();
                    if (typeof _initRejectToggle === 'function') _initRejectToggle();
                    if (typeof _initRejectPills === 'function') _initRejectPills();
                    if (typeof _initMapBtn === 'function') _initMapBtn();
                    if (typeof _initModals === 'function') _initModals();

                    // 7. Re-inicializar el filtro en la sección nueva (bind de eventos + conteos iniciales)
                    DS.contactFilter?.init(newSection);
                } else {
                    // Si por alguna razón la nueva vista no tiene la sección, restauramos la UI
                    _restoreSectionUI(oldSection);
                }
            } else {
                _restoreSectionUI(oldSection);
            }
        } catch (err) {
            console.error('Excepción al recargar la sección:', err);
            _restoreSectionUI(oldSection);
            // Nota: DS.api ya mostrará el toast de error de red automáticamente
        }
    }

    // Función auxiliar para no repetir código
    function _restoreSectionUI(section) {
        if (section) {
            section.style.opacity = '1';
            section.style.pointerEvents = 'auto';
        }
    }

    // ─────────────────────────────────────────────────────
    // GPS Save Button
    // ─────────────────────────────────────────────────────

    function _initMapBtn() {
        document.querySelectorAll('[data-map-btn]').forEach(btn => {
            btn.addEventListener('click', async function () {
                const card = btn.closest('.cp-contact-card');
                if (!card) return;

                const clientId = card.dataset.clientId;
                const addressId = card.dataset.contactId;

                const latEl = card.querySelector('[data-gps-lat]');
                const lngEl = card.querySelector('[data-gps-lng]');

                if (!clientId || !addressId || !latEl || !lngEl) {
                    DS.notify?.error('Datos de identificación incompletos para actualizar GPS.');
                    return;
                }

                const latStr = latEl.textContent.trim();
                const lngStr = lngEl.textContent.trim();
                const lat = latStr !== '' ? parseFloat(latStr) : null;
                const lng = lngStr !== '' ? parseFloat(lngStr) : null;

                // Loading state visual
                const iconEl = btn.querySelector('.material-symbols-outlined');
                if (iconEl) iconEl.textContent = 'hourglass_empty';
                btn.disabled = true;
                btn.style.pointerEvents = 'none';

                try {
                    // 1. URL RESTful hacia el Controller
                    const endpoint = `/api/clientes/${clientId}/address/${addressId}/update/GPS`;

                    // 2. Body (Payload) que coincide con tu clase SaveGpsRequest en C#
                    const payload = {
                        latitude: lat,
                        longitude: lng
                    };

                    // 3. Llamada mediante Axios (DS.api ya maneja errores de red y de servidor)
                    const { ok, data } = await DS.api.post(endpoint, payload);

                    if (ok) {
                        DS.notify?.success('Coordenadas GPS guardadas');

                        if (lat !== null && lng !== null) {
                            // Actualizar data-lat / data-lng en la tarjeta para que el selector de mapa funcione
                            card.dataset.lat = lat.toFixed(6);
                            card.dataset.lng = lng.toFixed(6);
                            // Actualizar el panel de mapa full-size
                            _updateMapPanel(String(lat), String(lng));
                        }
                    }

                } catch (err) {
                    console.error('Error procesando GPS:', err);
                } finally {
                    // Restore button state
                    if (iconEl) iconEl.textContent = 'my_location';
                    btn.disabled = false;
                    btn.style.pointerEvents = '';
                }
            });
        });
    }

    // ─────────────────────────────────────────────────────
    // Initialize All Handlers
    // ─────────────────────────────────────────────────────
    
    function _initAll() {
        _initModals();
        _initEditToggle();
        _initVerifyPills();
        _initRejectToggle();
        _initRejectPills();
        _initLopdpButtons();
        _initAddEmailForm();
        _initAddPhoneForm();
        _initAddDireccionForm();
        _initMapBtn();
        _initContactFilter();
        _initSidebar();
        _initMobileDrawer();
        _initAddressMapPanel();
        // Orden inicial: verified → pending → error/unknown
        ['phones-list', 'emails-list', 'addresses-list'].forEach(id => {
            const container = document.getElementById(id);
            if (container) _sortContactCards(container);
        });
    }

    /* ─────────────────────────────────────────────────────
       Mobile Drawer — off-canvas sidebar con focus trap
       ───────────────────────────────────────────────────── */
    function _initMobileDrawer() {
        const sidebar  = document.getElementById('cp-sidebar');
        const overlay  = document.getElementById('cp-sidebar-overlay');
        const toggle   = document.getElementById('cp-sidebar-toggle');
        const closeBtn = document.getElementById('cp-sidebar-close');
        const bottomMenuBtn = document.getElementById('cp-bottom-nav-menu');
        const mobileTitle   = document.getElementById('cp-mobile-section-title');
        const bottomNavBtns = document.querySelectorAll('.cp-bottom-nav__item[data-sub]');

        if (!sidebar || !overlay) return;

        // ── Open / Close helpers ──────────────────────────────
        function _openDrawer() {
            sidebar.classList.add('is-animating');
            sidebar.classList.add('is-open');
            overlay.style.display = 'block';
            requestAnimationFrame(() => overlay.classList.add('is-visible'));
            toggle?.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';

            // Move focus to first focusable element inside sidebar
            const firstFocusable = sidebar.querySelector('button:not([disabled]), [tabindex="0"]');
            firstFocusable?.focus();

            sidebar.addEventListener('transitionend', () => {
                sidebar.classList.remove('is-animating');
            }, { once: true });
        }

        function _closeDrawer() {
            sidebar.classList.add('is-animating');
            sidebar.classList.remove('is-open');
            overlay.classList.remove('is-visible');
            toggle?.setAttribute('aria-expanded', 'false');
            document.body.style.overflow = '';

            overlay.addEventListener('transitionend', () => {
                overlay.style.display = 'none';
                sidebar.classList.remove('is-animating');
            }, { once: true });

            toggle?.focus(); // return focus to trigger
        }

        // ── Triggers ─────────────────────────────────────────
        toggle?.addEventListener('click', () =>
            sidebar.classList.contains('is-open') ? _closeDrawer() : _openDrawer());

        bottomMenuBtn?.addEventListener('click', () =>
            sidebar.classList.contains('is-open') ? _closeDrawer() : _openDrawer());

        closeBtn?.addEventListener('click', _closeDrawer);
        overlay.addEventListener('click', _closeDrawer);

        // ── Escape key ────────────────────────────────────────
        document.addEventListener('keydown', e => {
            if (e.key === 'Escape' && sidebar.classList.contains('is-open')) {
                _closeDrawer();
            }
        });

        // ── Focus trap ────────────────────────────────────────
        sidebar.addEventListener('keydown', e => {
            if (e.key !== 'Tab' || !sidebar.classList.contains('is-open')) return;
            const focusable = Array.from(
                sidebar.querySelectorAll('button:not([disabled]), [href], input, [tabindex]:not([tabindex="-1"])')
            ).filter(el => !el.closest('[hidden]'));
            if (focusable.length === 0) return;
            const first = focusable[0];
            const last  = focusable[focusable.length - 1];
            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        });

        // ── Bottom nav section switching ──────────────────────
        bottomNavBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const sub = btn.dataset.sub;
                _showContactSection(sub);

                // Sync active state on bottom nav
                bottomNavBtns.forEach(b => b.classList.remove('cp-bottom-nav__item--active'));
                btn.classList.add('cp-bottom-nav__item--active');

                // Sync sidebar subitems
                document.querySelectorAll('.cp-sidebar__subitem').forEach(s => {
                    s.classList.toggle('cp-sidebar__subitem--active', s.dataset.sub === sub);
                });

                // Update mobile title
                if (mobileTitle) mobileTitle.textContent = btn.querySelector('span:last-child')?.textContent ?? '';

                // Load map on first address open
                if (sub === 'addresses') {
                    const firstAddr = document.querySelector('#addresses-list .cp-contact-card');
                    if (firstAddr) firstAddr.click();
                }
            });
        });

        // ── Sync bottom nav when sidebar subitems are clicked ─
        document.querySelectorAll('.cp-sidebar__subitem').forEach(item => {
            item.addEventListener('click', () => {
                const sub = item.dataset.sub;
                bottomNavBtns.forEach(b => {
                    b.classList.toggle('cp-bottom-nav__item--active', b.dataset.sub === sub);
                });
                if (mobileTitle) {
                    const label = { phones: 'Teléfonos', emails: 'Correos', addresses: 'Direcciones' };
                    mobileTitle.textContent = label[sub] ?? 'Contactabilidad';
                }
                // Close drawer after selecting a section on mobile
                if (window.innerWidth <= 768) _closeDrawer();
            });
        });
    
        // Orden inicial: verified → pending → error/unknown
        ['phones-list', 'emails-list', 'addresses-list'].forEach(id => {
            const container = document.getElementById(id);
            if (container) _sortContactCards(container);
        });
    }

    /** Selecciona automáticamente la primera tarjeta de dirección que tenga lat/lng. */
    function _preselectFirstAddress() {
        const addrList = document.getElementById('addresses-list');
        if (!addrList) return;
        const first = Array.from(addrList.querySelectorAll('.cp-contact-card'))
            .find(c => c.dataset.lat && c.dataset.lng);
        if (!first) return;
        addrList.querySelectorAll('.cp-contact-card').forEach(c => c.classList.remove('cp-contact-card--selected'));
        first.classList.add('cp-contact-card--selected', 'cp-contact-card--pulse');
        _updateMapPanel(first.dataset.lat, first.dataset.lng);
    }

    // ─────────────────────────────────────────────────────
    // Entry Point
    // ─────────────────────────────────────────────────────

    // Exponer init para modo embed (Dashboard llama DS.Clientes.init() tras inyectar el HTML)
    window.DS = window.DS || {};
    window.DS.Clientes = { init: _initAll };

    // Auto-inicialización en carga autónoma de la página de clientes
    document.addEventListener('DOMContentLoaded', _initAll);
})();
