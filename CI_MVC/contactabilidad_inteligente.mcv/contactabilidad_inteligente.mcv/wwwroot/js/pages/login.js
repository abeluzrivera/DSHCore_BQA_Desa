/**
 * Login Form Handler
 * Maneja interactividad del formulario de login
 * Frame: Login / Inital_Template (Data Smart Hub)
 */

(function () {
    'use strict';

    const form = document.querySelector('form');
    const passwordInput = document.getElementById('passwordInput');
    const togglePasswordBtn = document.getElementById('togglePasswordBtn');
    const submitBtn = document.querySelector('button[type="submit"]');

    // ─── Toggle visibilidad de contraseña ───────────────────────────
    if (togglePasswordBtn && passwordInput) {
        togglePasswordBtn.addEventListener('click', function (e) {
            e.preventDefault();

            const isPassword = passwordInput.type === 'password';
            passwordInput.type = isPassword ? 'text' : 'password';

            const icon = this.querySelector('.material-icons');
            if (icon) {
                icon.textContent = isPassword ? 'visibility_off' : 'visibility';
            }

            this.setAttribute('aria-label', isPassword ? 'Ocultar contraseña' : 'Mostrar contraseña');
        });
    }

    // ─── Enter en campo contraseña envía el formulario ───────────────
    if (passwordInput) {
        passwordInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter' && form) {
                form.submit();
            }
        });
    }

    // ─── Submit: deshabilitar botón para evitar doble envío ──────────
    if (form) {
        form.addEventListener('submit', function () {
            if (submitBtn) {
                submitBtn.disabled = true;
                const buttonText = submitBtn.querySelector('#buttonText');
                if (buttonText) {
                    buttonText.textContent = 'Procesando...';
                }
            }
        });
    }

    // ─── Feedback visual en blur/focus de inputs ─────────────────────
    const inputs = document.querySelectorAll('input[type="email"], input[type="password"]');
    inputs.forEach(function (input) {
        input.addEventListener('blur', function () {
            if (this.value.trim()) {
                const error = this.closest('.lp-field')?.querySelector('.lp-error');
                if (error) {
                    error.style.display = 'none';
                }
            }
        });

        input.addEventListener('focus', function () {
            const error = this.closest('.lp-field')?.querySelector('.lp-error');
            if (error && error.textContent.trim()) {
                error.style.display = 'block';
            }
        });
    });

})();
