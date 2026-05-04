/**
 * ds-client-list-store.js
 * Persiste la lista de IDs de clientes cargados en el dashboard usando sessionStorage.
 *
 * Ciclo de vida:
 *   - Los datos sobreviven recargas de página dentro de la misma pestaña.
 *   - Al cerrar la pestaña o el navegador, sessionStorage se limpia automáticamente.
 *   - TTL de 8 horas: si la pestaña queda abierta sin actividad, la lista expira.
 *   - Cada escritura (add, remove, setAll) renueva el TTL (sliding window).
 *
 * Expone: DS.clientListStore
 */
(() => {
    'use strict';

    const STORAGE_KEY = 'ci_dash_client_ids';
    const TTL_MS      = 8 * 60 * 60 * 1000; // 8 horas

    function _load() {
        try {
            const raw = sessionStorage.getItem(STORAGE_KEY);
            if (!raw) return [];
            const parsed = JSON.parse(raw);
            if (!parsed || typeof parsed !== 'object') return [];
            if (Date.now() > parsed.expiresAt) {
                sessionStorage.removeItem(STORAGE_KEY);
                return [];
            }
            return Array.isArray(parsed.ids) ? parsed.ids : [];
        } catch {
            return [];
        }
    }

    function _save(ids) {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify({
            ids:       [...new Set(ids)],
            expiresAt: Date.now() + TTL_MS
        }));
    }

    window.DS = window.DS ?? {};

    DS.clientListStore = {
        /** Devuelve los IDs actualmente persistidos. */
        getIds() {
            return _load();
        },

        /** Retorna true si el ID está en la lista. */
        has(id) {
            if (!id) return false;
            return _load().includes(String(id));
        },

        /** Agrega un ID a la lista; renueva el TTL. No duplica. */
        add(id) {
            if (!id) return;
            const ids = _load();
            const sid = String(id);
            if (!ids.includes(sid)) {
                ids.push(sid);
            }
            _save(ids);
        },

        /** Elimina un ID de la lista; renueva el TTL. */
        remove(id) {
            if (!id) return;
            _save(_load().filter(i => i !== String(id)));
        },

        /** Reemplaza toda la lista (usado en lote batch); renueva el TTL. */
        setAll(ids) {
            _save((ids ?? []).map(String).filter(Boolean));
        },

        /** Limpia la lista completamente. */
        clear() {
            sessionStorage.removeItem(STORAGE_KEY);
        },
    };
})();
