/**
 * dashboard-file-modal.js
 * Manages drag-drop, file selection, validation and state rendering
 * for the lote-tab-archivo pane inside the Nueva Lote modal.
 *
 * Exposes:  window.LOTE_FILE_MODAL
 * Requires: #lote-upload-body, #lote-dropzone, #lote-file-input,
 *           #lote-loading, #lote-file-card, #lote-file-banner,
 *           #lote-error-alert, #lote-file-remove
 */
(function () {
    'use strict';

    /* ─── Allowed extensions / max size ─── */
    const ALLOWED_EXTENSIONS = ['.xlsx', '.xls', '.csv'];
    const MAX_BYTES           = 10 * 1024 * 1024; // 10 MB

    /* ─── State values (mirrors data-state attribute) ─── */
    const STATE = { EMPTY: 'empty', LOADING: 'loading', SUCCESS: 'success', ERROR: 'error' };

    class LoteFileModal {
        constructor() {
            this.validationResult = null; // { isValid, recordCount, validIds, identifierColumn, errorMessage }
            this._bound = false;
        }

        /** Bind all events. Called on DOMContentLoaded or on first tab activation. */
        bind() {
            if (this._bound) return;
            this._bound = true;

            this._body      = document.getElementById('lote-upload-body');
            this._dropzone  = document.getElementById('lote-dropzone');
            this._input     = document.getElementById('lote-file-input');
            this._loading   = document.getElementById('lote-loading');
            this._fileCard  = document.getElementById('lote-file-card');
            this._banner    = document.getElementById('lote-file-banner');
            this._bannerTxt = document.getElementById('lote-file-banner-text');
            this._errorBox  = document.getElementById('lote-error-alert');
            this._errorTxt  = document.getElementById('lote-error-text');
            this._removeBtn = document.getElementById('lote-file-remove');

            if (!this._body) {
                console.warn('[LoteFileModal] #lote-upload-body no encontrado — bind() abortado');
                return;
            }

            console.log('[LoteFileModal] bind() OK — elementos encontrados');
            this._bindDragDrop();
            this._bindFileInput();
            this._bindRemove();
        }

        /* ── Drag & Drop ── */
        _bindDragDrop() {
            const dz = this._dropzone;
            if (!dz) return;

            // Abrir diálogo de archivo al hacer clic en cualquier parte del dropzone
            dz.addEventListener('click', (e) => {
                if (!e.target.closest('label') && e.target !== this._input) {
                    this._input?.click();
                }
            });

            dz.addEventListener('dragenter', (e) => { e.preventDefault(); e.stopPropagation(); dz.classList.add('is-dragover'); });
            dz.addEventListener('dragover',  (e) => { e.preventDefault(); e.stopPropagation(); dz.classList.add('is-dragover'); });
            dz.addEventListener('dragleave', (e) => { e.preventDefault(); e.stopPropagation(); dz.classList.remove('is-dragover'); });
            dz.addEventListener('drop', (e) => {
                e.preventDefault();
                e.stopPropagation();
                dz.classList.remove('is-dragover');
                const file = e.dataTransfer?.files?.[0];
                if (file) this._handleFileSelect(file);
            });
        }

        /* ── File input <input type="file"> ── */
        _bindFileInput() {
            this._input?.addEventListener('change', (e) => {
                const file = e.target?.files?.[0];
                if (file) this._handleFileSelect(file);
                // reset so same file can be re-selected
                if (this._input) this._input.value = '';
            });
        }

        /* ── Remove / reset ── */
        _bindRemove() {
            this._removeBtn?.addEventListener('click', () => this._resetFile());
        }

        /* ── Handle a selected file ── */
        async _handleFileSelect(file) {
            console.log('[LoteFileModal] archivo seleccionado:', file?.name, file?.size);

            // Client-side pre-validation
            const ext = '.' + file.name.split('.').pop().toLowerCase();
            if (!ALLOWED_EXTENSIONS.includes(ext)) {
                this._showError(`Formato no permitido: ${ext}. Use ${ALLOWED_EXTENSIONS.join(', ')}`);
                return;
            }
            if (file.size > MAX_BYTES) {
                this._showError(`El archivo supera el límite de 10 MB.`);
                return;
            }

            this._setState(STATE.LOADING);
            console.log('[LoteFileModal] estado → loading, enviando al servidor…');

            try {
                const result = await this._uploadForValidation(file);
                console.log('[LoteFileModal] respuesta del servidor:', result);
                this.validationResult = result;

                if (!result.isValid) {
                    this._showError(result.errorMessage || 'No se pudo procesar el archivo.');
                    return;
                }

                this._showFileSuccess(file, result);
            } catch (err) {
                this._showError('Error de conexión al procesar el archivo.');
                console.error('[LoteFileModal] upload error', err);
            }
        }

        /* ── POST file to server for extraction ── */
        async _uploadForValidation(file) {
            const form = new FormData();
            form.append('file', file);

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';

            const response = await fetch('/dashboard?handler=ValidateFileExtract', {
                method: 'POST',
                headers: { 'RequestVerificationToken': token },
                body: form
            });

            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return await response.json();
        }

        /* ── Show success state ── */
        _showFileSuccess(file, result) {
            // Populate file card
            const nameEl = document.getElementById('lote-file-name');
            const metaEl = document.getElementById('lote-file-meta');
            if (nameEl) nameEl.textContent = file.name;
            if (metaEl) metaEl.textContent = `${result.recordCount} ID${result.recordCount !== 1 ? 's' : ''} encontrado${result.recordCount !== 1 ? 's' : ''} · ${this._formatBytes(file.size)}`;

            // Populate banner
            if (this._bannerTxt) {
                const parts = [`${result.recordCount} identificador${result.recordCount !== 1 ? 'es' : ''} válido${result.recordCount !== 1 ? 's' : ''}`];
                if (result.identifierColumn) parts.push(`columna "${result.identifierColumn}"`);
                this._bannerTxt.textContent = parts.join(' · ');
            }
            if (this._banner) this._banner.hidden = false;

            this._setState(STATE.SUCCESS);
            document.dispatchEvent(new CustomEvent('lote:file-ready', { detail: { hasFile: true } }));
        }

        /* ── Show error state ── */
        _showError(message) {
            if (this._errorTxt) this._errorTxt.textContent = message;
            if (this._errorBox) this._errorBox.hidden = false;
            if (this._banner)   this._banner.hidden   = true;
            this.validationResult = null;
            this._setState(STATE.ERROR);
            document.dispatchEvent(new CustomEvent('lote:file-ready', { detail: { hasFile: false } }));
        }

        /* ── Reset back to empty ── */
        _resetFile() {
            this.validationResult = null;
            if (this._banner)   this._banner.hidden   = true;
            if (this._errorBox) this._errorBox.hidden = true;
            if (this._bannerTxt) this._bannerTxt.textContent = '';
            if (this._errorTxt)  this._errorTxt.textContent  = '';
            this._setState(STATE.EMPTY);
            document.dispatchEvent(new CustomEvent('lote:file-ready', { detail: { hasFile: false } }));
        }

        /* ── Update data-state on upload body (drives CSS) ── */
        _setState(state) {
            if (this._body) this._body.dataset.state = state;
        }

        /* ── Utility: human-readable file size ── */
        _formatBytes(bytes) {
            if (bytes < 1024)        return `${bytes} B`;
            if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
            return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
        }

        /** Returns true when a file has been validated successfully */
        get hasValidFile() {
            return this.validationResult?.isValid === true && (this.validationResult?.validIds?.length ?? 0) > 0;
        }
    }

    /* ── Bootstrap ── */
    function init() {
        window.LOTE_FILE_MODAL = new LoteFileModal();
        window.LOTE_FILE_MODAL.bind();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
