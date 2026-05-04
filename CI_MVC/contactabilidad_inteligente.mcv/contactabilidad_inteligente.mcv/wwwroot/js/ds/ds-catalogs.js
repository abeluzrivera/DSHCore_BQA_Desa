/* =====================================================
   ds-catalogs.js — Catálogos centralizados del sistema
   Carga los catálogos desde el backend (SSOT) en lugar de hardcodearlos.
   ===================================================== */

(function () {
    'use strict';

    /**
     * Namespace global para catálogos.
     * Se inicializa de forma asíncrona al cargar la página.
     */
    window.DS = window.DS || {};
    window.DS.catalogs = {
        errorCodes: {},
        contactTypes: [],
        contactStates: [],
        isLoaded: false,

        /**
         * Inicializa los catálogos cargándolos desde el backend.
         * Debe llamarse una vez al inicio de la aplicación.
         */
        async init() {
            try {
                // Cargar todos los catálogos en paralelo
                const [errorCodesRes, contactTypesRes, contactStatesRes] = await Promise.all([
                    DS.api.get(DS.endpoints.catalogs.errorCodes()),
                    DS.api.get(DS.endpoints.catalogs.contactTypes()),
                    DS.api.get(DS.endpoints.catalogs.contactStates())
                ]);

                if (errorCodesRes.ok && errorCodesRes.data) {
                    this.errorCodes = errorCodesRes.data;
                }

                if (contactTypesRes.ok && contactTypesRes.data) {
                    this.contactTypes = contactTypesRes.data;
                    // Crear índice por código para búsqueda rápida
                    this._contactTypeMap = {};
                    this.contactTypes.forEach(t => {
                        this._contactTypeMap[t.code] = t;
                    });
                }

                if (contactStatesRes.ok && contactStatesRes.data) {
                    this.contactStates = contactStatesRes.data;
                    // Crear índice por código para búsqueda rápida
                    this._stateMap = {};
                    this.contactStates.forEach(s => {
                        this._stateMap[s.code] = s;
                    });
                }

                this.isLoaded = true;
                console.log('[DS.catalogs] Catálogos cargados correctamente');
            } catch (error) {
                console.error('[DS.catalogs] Error al cargar catálogos:', error);
                // Fallback a valores vacíos - la aplicación funcionará con labels por defecto
            }
        },

        /**
         * Obtiene el catálogo de errores para un tipo de contacto específico.
         * @param {string} contactType - Tipo de contacto ('EMAIL', 'CEL', etc.)
         * @returns {Array<{code: string, label: string}>}
         */
        getErrorsForType(contactType) {
            // Mapear tipos de contacto a categorías de error
            const categoryMap = {
                'EMAIL': 'email',
                'TEL_C': 'phone',
                'CEL': 'phone',
                'WAPP': 'phone',
                'DIR_D': 'address',
                'DIR_T': 'address'
            };

            const category = categoryMap[contactType];
            return this.errorCodes[category] || [];
        },

        /**
         * Obtiene el label amigable de un tipo de contacto.
         * @param {string} type - Código del tipo ('EMAIL', 'CEL', etc.)
         * @returns {string}
         */
        getContactLabel(type) {
            const metadata = this._contactTypeMap?.[type];
            return metadata?.label || type;
        },

        /**
         * Obtiene el label de un código de error específico.
         * @param {string} errorCode - Código de error ('ERR_MAIL_FMT', etc.)
         * @returns {string}
         */
        getErrorLabel(errorCode) {
            for (const errors of Object.values(this.errorCodes)) {
                const found = errors.find(e => e.code === errorCode);
                if (found) return found.label;
            }
            return errorCode; // Fallback al código crudo
        },

        /**
         * Obtiene la metadata completa de un tipo de contacto.
         * @param {string} type - Código del tipo ('EMAIL', 'CEL', etc.)
         * @returns {{code, icon, iconType, label, order, shouldTruncate}|null}
         */
        getContactMetadata(type) {
            return this._contactTypeMap?.[type] || null;
        },

        /**
         * Obtiene el ícono de un tipo de contacto.
         * @param {string} type - Código del tipo
         * @returns {string}
         */
        getContactIcon(type) {
            const metadata = this._contactTypeMap?.[type];
            return metadata?.icon || 'contact_page';
        },

        /**
         * Obtiene el tipo de ícono (phone, email, location, link).
         * @param {string} type - Código del tipo
         * @returns {string}
         */
        getContactIconType(type) {
            const metadata = this._contactTypeMap?.[type];
            return metadata?.iconType || 'phone';
        },

        /**
         * Verifica si un tipo de contacto debe truncarse en la UI.
         * @param {string} type - Código del tipo
         * @returns {boolean}
         */
        shouldTruncate(type) {
            const metadata = this._contactTypeMap?.[type];
            return metadata?.shouldTruncate || false;
        },

        /**
         * Obtiene la metadata visual de un estado de contactabilidad.
         * @param {string} stateCode - Código del estado ('Verificado', 'Pendiente', etc.)
         * @returns {{code, bgClass, borderClass, textClass, state}|null}
         */
        getStateMetadata(stateCode) {
            return this._stateMap?.[stateCode] || null;
        },

        /**
         * Obtiene las clases CSS de un estado.
         * @param {string} stateCode - Código del estado
         * @returns {{bg: string, border: string, text: string, state: string}}
         */
        getStateClasses(stateCode) {
            const metadata = this._stateMap?.[stateCode];
            if (metadata) {
                return {
                    bg: metadata.bgClass,
                    border: metadata.borderClass,
                    text: metadata.textClass,
                    state: metadata.state
                };
            }
            // Fallback
            return {
                bg: 'tw-bg-bg-subtle',
                border: 'tw-border-border',
                text: 'tw-text-text-body-muted',
                state: 'unknown'
            };
        }
    };

    // Auto-inicializar al cargar la página
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => DS.catalogs.init());
    } else {
        DS.catalogs.init();
    }

})();
