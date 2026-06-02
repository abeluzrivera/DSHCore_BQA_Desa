/**
 * ds-endpoints.js — Centralización de URLs de la API
 *
 * ÚNICO lugar donde se definen las rutas. Para cambiar el base path o
 * mover endpoints basta con editar aquí — ningún módulo JS hardcodea URLs.
 *
 * Uso:
 *   DS.endpoints.clients.verifyContact(numericId, contactId)
 *   DS.endpoints.users.generatePassword()
 *   DS.endpoints.catalogs.errorCodes()
 */
(function () {
    'use strict';

    window.DS = window.DS || {};

    // Cambiar BASE para despliegues en subdirectorio o detrás de un reverse proxy.
    const BASE = '';

    DS.endpoints = {

        clients: {
            search:          (q, top = 10)                   => `${BASE}/api/clientes/search?q=${encodeURIComponent(q)}&top=${top}`,
            list:            (ids, estado)                   => {
                if (!ids) return `${BASE}/api/clientes`;
                const q = [`ids=${ids}`];
                if (estado && estado !== 'todos') q.push(`estado=${encodeURIComponent(estado)}`);
                return `${BASE}/api/clientes?${q.join('&')}`;
            },
            detail:          (id)                            => `${BASE}/api/clientes/${encodeURIComponent(id)}`,
            verifyContact:   (numericId, contactId)          => `${BASE}/api/clientes/${numericId}/contacts/${contactId}/verify`,
            unverifyContact: (numericId, contactId, code)    => `${BASE}/api/clientes/${numericId}/contacts/${contactId}/unverify/${encodeURIComponent(code)}`,
            verifyAddress:   (numericId, addressId)          => `${BASE}/api/clientes/${numericId}/address/${addressId}/verify`,
            unverifyAddress: (numericId, addressId, code)    => `${BASE}/api/clientes/${numericId}/address/${addressId}/unverify/${encodeURIComponent(code)}`,
        },

        users: {
            list:             ()    => `${BASE}/api/users`,
            detail:           (id)  => `${BASE}/api/users/${id}`,
            create:           ()    => `${BASE}/api/users`,
            update:           (id)  => `${BASE}/api/users/${id}`,
            toggleStatus:     (id)  => `${BASE}/api/users/${id}/toggle-status`,
            generatePassword: ()    => `${BASE}/api/users/generate-password`,
        },

        catalogs: {
            errorCodes:    () => `${BASE}/api/catalogos/error-codes`,
            contactTypes:  () => `${BASE}/api/catalogos/contact-types`,
            contactStates: () => `${BASE}/api/catalogos/contact-states`,
            roles:         () => `${BASE}/api/catalogos/roles`,
        },

    };

})();
