/**
 * users.js — Gestión de Usuarios
 *
 * Arquitectura: Client-Side / Frontend JS (DS-Back Layer 6)
 * Responsabilidades:
 *   - Dar interactividad a la página de usuarios.
 *   - Consumir /api/users/* (API Layer) con DS.api.
 *   - Delegad presentación visual al propio JS (clases CSS, DOM).
 *
 * Restricciones:
 *   ❌ NO llama a ?handler=* (Razor Pages handlers).
 *   ❌ NO hardcodea valores de catálogo — los roles vienen del SSR (Model.RolesList).
 *   ❌ NO procesa reglas de negocio — solo orquesta llamadas al backend.
 */

(() => {
    'use strict';

    // ─── Bootstrap ──────────────────────────────────────────────────────────────

    document.addEventListener('DOMContentLoaded', () => {
        _initSearch();
        _initCheckboxes();
        _initRefreshButton();
        _initProfileImage();
        _initPasswordGenerator();
        _initPasswordStrength();
        _initCopyPassword();
        _initCreateUserForm();
        _initEditUserForm();
        _initModalEvents();
        _injectSpinKeyframe();
    });

    // ─── Search (client-side filter) ────────────────────────────────────────────

    function _initSearch() {
        const input = document.getElementById('searchUsersInput');
        const rows  = document.querySelectorAll('.user-row');
        if (!input || !rows.length) return;

        input.addEventListener('input', DS.utils.debounce(e => {
            const term = e.target.value.toLowerCase().trim();
            rows.forEach(row => {
                const name  = row.querySelector('.user-name-cell')?.textContent.toLowerCase()  ?? '';
                const email = row.querySelector('.user-email-cell')?.textContent.toLowerCase() ?? '';
                const role  = row.querySelector('.role-badge')?.textContent.toLowerCase()      ?? '';
                row.style.display = (!term || name.includes(term) || email.includes(term) || role.includes(term))
                    ? '' : 'none';
            });
        }, 200));
    }

    // ─── Select All ─────────────────────────────────────────────────────────────

    function _initCheckboxes() {
        const selectAll = document.getElementById('selectAll');
        if (!selectAll) return;
        selectAll.addEventListener('change', function () {
            document.querySelectorAll('.user-row .checkbox-input')
                .forEach(cb => { cb.checked = this.checked; });
        });
    }

    // ─── Refresh button ─────────────────────────────────────────────────────────

    function _initRefreshButton() {
        const btn = document.querySelector('.refresh-btn');
        if (!btn) return;
        btn.addEventListener('click', () => {
            const icon = btn.querySelector('.material-icons');
            if (icon) icon.style.animation = 'spin 1s ease-in-out';
            setTimeout(() => window.location.reload(), 1000);
        });
    }

    // ─── Profile image preview ──────────────────────────────────────────────────

    function _initProfileImage() {
        const input     = document.getElementById('profileImageInput');
        const container = document.querySelector('.profile-image-container');

        if (container && input) {
            container.addEventListener('click', e => {
                e.preventDefault();
                e.stopPropagation();
                DS.notify.confirm(
                    'En esta versión la imagen no se visualizará de inmediato. ¿Desea continuar?',
                    {
                        title: 'Foto de perfil',
                        confirmText: 'Seleccionar',
                        cancelText: 'Cancelar',
                        type: 'warning',
                        onConfirm: () => input.click()
                    }
                );
            });
        }

        if (input) {
            input.addEventListener('change', e => {
                const file = e.target.files[0];
                if (!file) return;

                if (file.size > 1 * 1024 * 1024) {
                    DS.notify.error(`La imagen supera 1 MB (${(file.size / 1024 / 1024).toFixed(2)} MB).`);
                    input.value = '';
                    return;
                }
                const valid = ['image/jpeg', 'image/png', 'image/gif', 'image/bmp'];
                if (!valid.includes(file.type)) {
                    DS.notify.error('Solo se permiten imágenes JPG, PNG, GIF o BMP.');
                    input.value = '';
                }
            });
        }
    }

    // ─── Password generator ──────────────────────────────────────────────────────

    function _initPasswordGenerator() {
        const btn      = document.getElementById('generatePasswordBtn');
        const pwInput  = document.getElementById('password');
        const strength = document.getElementById('passwordStrength');
        if (!btn || !pwInput) return;

        btn.addEventListener('click', async () => {
            const result = await DS.api.post(DS.endpoints.users.generatePassword());
            if (result.ok && result.data?.success) {
                pwInput.value = result.data.password;
                pwInput.dispatchEvent(new Event('input'));
                if (strength) strength.classList.remove('d-none');
            } else {
                DS.notify.error('No se pudo generar la contraseña. Intente nuevamente.');
            }
        });
    }

    // ─── Password strength indicator ─────────────────────────────────────────────

    function _initPasswordStrength() {
        const pwInput      = document.getElementById('password');
        const strengthBox  = document.getElementById('passwordStrength');
        const strengthLabel = document.getElementById('strengthLevel');
        if (!pwInput || !strengthBox || !strengthLabel) return;

        pwInput.addEventListener('input', () => {
            const pw = pwInput.value;
            if (!pw) { strengthBox.classList.add('d-none'); return; }

            const score = [
                /[A-Z]/.test(pw),
                /[a-z]/.test(pw),
                /[0-9]/.test(pw),
                /[!@#$%^&*()\-_=+[\]{}|;:,.<>?]/.test(pw),
                pw.length >= 8
            ].filter(Boolean).length;

            strengthBox.classList.remove('d-none');
            const div = strengthBox.querySelector('div');
            if (score === 5)     { strengthLabel.textContent = 'Alto';  div.className = 'd-flex align-items-center gap-2 p-2 rounded'; div.style.background = 'rgba(40,167,69,0.1)'; strengthLabel.style.color = '#28a745'; }
            else if (score >= 3) { strengthLabel.textContent = 'Medio'; div.className = 'd-flex align-items-center gap-2 p-2 rounded'; div.style.background = 'rgba(255,193,7,0.1)';  strengthLabel.style.color = '#ffc107'; }
            else                 { strengthLabel.textContent = 'Bajo';  div.className = 'd-flex align-items-center gap-2 p-2 rounded'; div.style.background = 'rgba(220,53,69,0.1)';  strengthLabel.style.color = '#dc3545'; }
        });
    }

    // ─── Copy password ────────────────────────────────────────────────────────────

    function _initCopyPassword() {
        const btn     = document.getElementById('copyPasswordBtn');
        const pwInput = document.getElementById('password');
        if (!btn || !pwInput) return;

        btn.addEventListener('click', async () => {
            if (!pwInput.value) return;
            try {
                await navigator.clipboard.writeText(pwInput.value);
                const icon = btn.querySelector('.material-icons');
                const orig = icon.textContent;
                icon.textContent = 'check';
                icon.style.color = '#28a745';
                setTimeout(() => { icon.textContent = orig; icon.style.color = '#6c757d'; }, 2000);
            } catch {
                DS.notify.error('No se pudo copiar la contraseña al portapapeles.');
            }
        });
    }

    // ─── Create User Form ─────────────────────────────────────────────────────────

    function _initCreateUserForm() {
        const form = document.getElementById('createUserForm');
        if (!form) return;

        form.addEventListener('submit', async e => {
            e.preventDefault();

            const payload = {
                fullName:      _val('fullName'),
                username:      _val('userCode'),
                email:         _val('email'),
                roleCode:      _val('role'),
                plainPassword: _val('password'),
                isActive:      document.getElementById('isActive')?.checked ?? true
            };

            if (!payload.fullName    || !payload.username ||
                !payload.email        || !payload.roleCode  || !payload.plainPassword) {
                DS.notify.warning('Por favor complete todos los campos requeridos.');
                return;
            }

            const submitBtn = form.querySelector('button[type="submit"]');
            const origHtml  = submitBtn.innerHTML;
            _setLoading(submitBtn, 'Guardando...');

            const result = await DS.api.post(DS.endpoints.users.create(), payload);

            if (result.ok && result.data?.success) {
                _closeModal('createUserModal');
                DS.notify.success(result.data.message ?? 'Usuario creado exitosamente.');
                setTimeout(() => window.location.reload(), 1500);
            } else {
                DS.notify.error(result.data?.message ?? 'Error al crear el usuario.');
                _resetBtn(submitBtn, origHtml);
            }
        });
    }

    // ─── Edit User Form ───────────────────────────────────────────────────────────

    function _initEditUserForm() {
        const form = document.getElementById('editUserForm');
        if (!form) return;

        form.addEventListener('submit', async e => {
            e.preventDefault();

            const id = parseInt(document.getElementById('editUserId')?.value ?? '0');
            if (!id) { DS.notify.warning('ID de usuario no válido.'); return; }

            const payload = {
                fullName:  _val('editFullName'),
                username:  _val('editUserCode'),
                email:     _val('editEmail'),
                roleCode:  _val('editRole'),
                isActive:  document.getElementById('editIsActive')?.checked ?? true
            };

            if (!payload.fullName || !payload.username || !payload.email || !payload.roleCode) {
                DS.notify.warning('Por favor complete todos los campos requeridos.');
                return;
            }

            const submitBtn = form.querySelector('button[type="submit"]');
            const origHtml  = submitBtn.innerHTML;
            _setLoading(submitBtn, 'Actualizando...');

            const result = await DS.api.put(DS.endpoints.users.update(id), payload);

            if (result.ok && result.data?.success) {
                _closeModal('editUserModal');
                DS.notify.success(result.data.message ?? 'Usuario actualizado exitosamente.');
                setTimeout(() => window.location.reload(), 1500);
            } else {
                DS.notify.error(result.data?.message ?? 'Error al actualizar el usuario.');
                _resetBtn(submitBtn, origHtml);
            }
        });
    }

    // ─── Modal events ─────────────────────────────────────────────────────────────

    function _initModalEvents() {
        // Reset create form on close
        const createModal = document.getElementById('createUserModal');
        if (createModal) {
            createModal.addEventListener('hidden.bs.modal', () => {
                document.getElementById('createUserForm')?.reset();
                document.getElementById('passwordStrength')?.classList.add('d-none');
            });
            createModal.addEventListener('shown.bs.modal', () => {
                document.getElementById('fullName')?.focus();
            });
        }

        // Focus first field on edit modal open
        const editModal = document.getElementById('editUserModal');
        if (editModal) {
            editModal.addEventListener('shown.bs.modal', () => {
                document.getElementById('editFullName')?.focus();
            });
        }
    }

    // ─── Keyframe spin (para botón refresh) ──────────────────────────────────────

    function _injectSpinKeyframe() {
        if (document.getElementById('ds-spin-style')) return;
        const style = document.createElement('style');
        style.id = 'ds-spin-style';
        style.textContent = '@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }';
        document.head.appendChild(style);
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────────

    function _val(id) { return document.getElementById(id)?.value?.trim() ?? ''; }

    function _setLoading(btn, text) {
        btn.disabled = true;
        btn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span>${text}`;
    }

    function _resetBtn(btn, html) {
        btn.disabled = false;
        btn.innerHTML = html;
    }

    function _closeModal(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const modal = bootstrap.Modal.getInstance(el);
        if (modal) modal.hide();
    }

    function _updateRowStatus(userId, data) {
        const row = document.querySelector(`.user-row:has(button[onclick*="toggleUserStatus(${userId})"])`);
        if (!row) return;

        const statusCell = row.querySelector('.status-cell');
        const statusText = row.querySelector('.status-text');
        if (statusCell && statusText) {
            statusCell.className = `status-cell ${data.statusClass}`;
            statusText.textContent = data.statusText;
        }

        const toggleBtn = row.querySelector('button[onclick*="toggleUserStatus"]');
        if (toggleBtn) {
            const icon = toggleBtn.querySelector('.material-icons');
            if (data.nuevoEstado) {
                icon.textContent       = 'block';
                toggleBtn.className    = 'action-btn text-danger';
                toggleBtn.title        = 'Desactivar';
            } else {
                icon.textContent       = 'check_circle';
                toggleBtn.className    = 'action-btn text-success';
                toggleBtn.title        = 'Activar';
            }
        }
    }

})();

// ─── Global functions (llamadas desde onclick en el HTML) ─────────────────────

/**
 * Abre el modal de edición cargando los datos del usuario desde la API.
 * @param {number} userId
 */
window.editUser = async function (userId) {
    if (!userId) { DS.notify.error('ID de usuario no válido.'); return; }

    const result = await DS.api.get(DS.endpoints.users.detail(userId));

    if (!result.ok || !result.data?.success) {
        DS.notify.error(result.data?.message ?? 'No se pudo cargar el usuario.');
        return;
    }

    const u = result.data.data;
    document.getElementById('editUserId').value      = u.id;
    document.getElementById('editFullName').value    = u.fullName   ?? '';
    document.getElementById('editUserCode').value    = u.username   ?? '';
    document.getElementById('editEmail').value       = u.email      ?? '';
    document.getElementById('editIsActive').checked  = u.isActive;

    // rolCodigo viene del endpoint GET /api/users/{id} (reverse lookup ya hecho en el controller)
    const roleSelect = document.getElementById('editRole');
    if (roleSelect && u.rolCodigo) roleSelect.value = u.rolCodigo;

    const modal = new bootstrap.Modal(document.getElementById('editUserModal'));
    modal.show();
};

/**
 * Invierte el estado activo/inactivo de un usuario sin recargar la página.
 * @param {number} userId
 */
window.toggleUserStatus = async function (userId) {
    if (!userId) { DS.notify.error('ID de usuario no válido.'); return; }

    DS.notify.confirm(
        '¿Está seguro de que desea cambiar el estado de este usuario? Esta acción afectará su acceso al sistema.',
        {
            title: 'Cambiar estado de usuario',
            confirmText: 'Sí, cambiar',
            cancelText: 'Cancelar',
            type: 'warning',
            onConfirm: async () => {
                const result = await DS.api.patch(DS.endpoints.users.toggleStatus(userId));

                if (result.ok && result.data?.success) {
                    const data = result.data.data; // UserStatusToggleDto
                    _updateRowFromToggle(userId, data);
                    DS.notify.success(result.data.message ?? 'Estado actualizado correctamente.');
                } else {
                    DS.notify.error(result.data?.message ?? 'No se pudo cambiar el estado.');
                }
            }
        }
    );
};

/**
 * Actualiza la fila de la tabla tras un toggle de estado (sin recargar la página).
 * @param {number} userId
 * @param {{ nuevoEstado: boolean, statusText: string, statusClass: string }} data
 */
function _updateRowFromToggle(userId, data) {
    const row = document.querySelector(`.user-row:has(button[onclick*="toggleUserStatus(${userId})"])`);
    if (!row) return;

    const statusCell = row.querySelector('.status-cell');
    const statusText = row.querySelector('.status-text');
    if (statusCell && statusText) {
        statusCell.className   = `status-cell ${data.statusClass}`;
        statusText.textContent = data.statusText;
    }

    const toggleBtn = row.querySelector('button[onclick*="toggleUserStatus"]');
    if (!toggleBtn) return;

    const icon = toggleBtn.querySelector('.material-icons');
    if (data.newStatus) {
        icon.textContent    = 'block';
        toggleBtn.className = 'action-btn text-danger';
        toggleBtn.title     = 'Desactivar';
    } else {
        icon.textContent    = 'check_circle';
        toggleBtn.className = 'action-btn text-success';
        toggleBtn.title     = 'Activar';
    }
}
