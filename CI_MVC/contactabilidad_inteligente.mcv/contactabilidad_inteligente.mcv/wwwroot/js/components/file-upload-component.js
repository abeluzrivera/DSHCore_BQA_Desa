/**
 * Section - Light Green Form Area Component
 * Maneja validación de archivos (drag & drop, click upload)
 * con soporte para eventos de validación estandarizados
 * 
 * @module fileUploadComponent
 * @requires No external dependencies
 */

class FileUploadComponent {
  constructor() {
    this.dropZone = document.getElementById('dropZone');
    this.fileInput = document.getElementById('fileInput');
    this.downloadTemplateBtn = document.getElementById('downloadTemplateBtn');
    this.deleteFileBtn = document.getElementById('deleteFileBtn');
    this.changeFileBtn = document.getElementById('changeFileBtn');
    this.cancelFileBtn = document.getElementById('cancelFileBtn');
    this.cancelProcessBtn = document.getElementById('cancelProcessBtn');
    this.processFileBtn = document.getElementById('processFileBtn');

    // Validaciones estandarizadas
    this.ALLOWED_EXTENSIONS = ['.xlsx', '.xls', '.csv'];
    this.MAX_FILE_SIZE = 25 * 1024 * 1024; // 25 MB

    this.init();
  }

  init() {
    if (!this.dropZone || !this.fileInput) {
      console.warn('FileUploadComponent: Required elements not found');
      return;
    }

    // Drag and drop events
    this.dropZone.addEventListener('click', () => this.fileInput.click());
    this.dropZone.addEventListener('dragover', (e) => this.handleDragOver(e));
    this.dropZone.addEventListener('dragleave', () => this.handleDragLeave());
    this.dropZone.addEventListener('drop', (e) => this.handleDrop(e));

    // File input change
    this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));

    // Download template button
    if (this.downloadTemplateBtn) {
      this.downloadTemplateBtn.addEventListener('click', (e) => this.handleDownloadTemplate(e));
    }

    // Delete file button (error state)
    if (this.deleteFileBtn) {
      this.deleteFileBtn.addEventListener('click', () => this.resetForm());
    }

    // Change file button (error state)
    if (this.changeFileBtn) {
      this.changeFileBtn.addEventListener('click', () => this.fileInput.click());
    }

    // Cancel buttons
    if (this.cancelFileBtn) {
      this.cancelFileBtn.addEventListener('click', () => this.resetForm());
    }

    if (this.cancelProcessBtn) {
      this.cancelProcessBtn.addEventListener('click', () => this.resetForm());
    }

    // Process file button (success state)
    if (this.processFileBtn) {
      this.processFileBtn.addEventListener('click', () => this.handleProcessFile());
    }

    // Keyboard support
    this.dropZone?.addEventListener('keydown', (e) => this.handleKeyDown(e));
  }

  /**
   * Handle drag over event - add visual feedback
   */
  handleDragOver(e) {
    e.preventDefault();
    e.stopPropagation();
    this.dropZone.classList.add('tw-bg-gray-100', 'tw-border-form-border');
  }

  /**
   * Handle drag leave event - remove visual feedback
   */
  handleDragLeave() {
    this.dropZone.classList.remove('tw-bg-gray-100', 'tw-border-form-border');
  }

  /**
   * Handle file drop event
   */
  handleDrop(e) {
    e.preventDefault();
    e.stopPropagation();

    this.dropZone.classList.remove('tw-bg-gray-100', 'tw-border-form-border');

    const files = e.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFiles(files);
    }
  }

  /**
   * Handle file selection from input
   */
  handleFileSelect(e) {
    const files = e.target.files;
    if (files && files.length > 0) {
      this.processFiles(files);
    }
  }

  /**
   * Process selected files and trigger validation
   */
  processFiles(files) {
    const file = files[0]; // Only accept first file
    
    // Validación 1: Tipo de archivo
    const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
    
    if (!this.ALLOWED_EXTENSIONS.includes(fileExtension)) {
      this.dispatchValidationError(
        `Tipo de archivo no válido. Se aceptan: ${this.ALLOWED_EXTENSIONS.join(', ')}`
      );
      return;
    }

    // Validación 2: Tamaño de archivo
    if (file.size > this.MAX_FILE_SIZE) {
      this.dispatchValidationError(
        `El archivo excede el tamaño máximo de 25 MB. Tamaño actual: ${this.formatFileSize(file.size)}`
      );
      return;
    }

    // Dispatch validating event (inicia loading state)
    const fileInfo = {
      name: file.name,
      size: this.formatFileSize(file.size),
      sizeBytes: file.size,
      file: file
    };

    const validatingEvent = new CustomEvent('fileValidating', {
      detail: fileInfo,
      bubbles: true,
      cancelable: true
    });

    this.dropZone.dispatchEvent(validatingEvent);

    // Simular validación: en producción enviar a servidor para validar columnas
    setTimeout(() => {
      const successEvent = new CustomEvent('fileValid', {
        detail: {
          ...fileInfo,
          recordCount: 500,
          identifierColumn: 'ID',
          detectedColumns: ['ID', 'Nombre', 'Email']
        },
        bubbles: true,
        cancelable: true
      });
      this.dropZone.dispatchEvent(successEvent);
    }, 1500);
  }

  /**
   * Dispatch validation error event
   */
  dispatchValidationError(errorMessage) {
    const event = new CustomEvent('fileInvalid', {
      detail: {
        errorMessage: errorMessage,
        timestamp: new Date()
      },
      bubbles: true,
      cancelable: true
    });

    this.dropZone.dispatchEvent(event);
  }

  /**
   * Handle download template button click
   */
  handleDownloadTemplate(e) {
    e.preventDefault();
    
    const event = new CustomEvent('downloadTemplate', {
      bubbles: true,
      cancelable: true
    });

    this.dropZone.dispatchEvent(event);
  }

  /**
   * Handle process file button (success state)
   */
  handleProcessFile() {
    const event = new CustomEvent('processFile', {
      bubbles: true,
      cancelable: true
    });

    this.dropZone.dispatchEvent(event);
  }

  /**
   * Reset form to initial state
   */
  resetForm() {
    this.fileInput.value = '';
    
    const resetEvent = new CustomEvent('fileReset', {
      bubbles: true,
      cancelable: true
    });

    this.dropZone.dispatchEvent(resetEvent);
  }

  /**
   * Handle keyboard support (Enter or Space to open file dialog)
   */
  handleKeyDown(e) {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      this.fileInput.click();
    }
  }

  /**
   * Format file size to human-readable format
   */
  formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return Math.round((bytes / Math.pow(k, i)) * 10) / 10 + ' ' + sizes[i];
  }
}

// Initialize component when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  if (document.querySelector('[data-component="section-light-green-form"]')) {
    window.fileUploadComponent = new FileUploadComponent();
  }
});

// Export for use in modules if needed
if (typeof module !== 'undefined' && module.exports) {
  module.exports = FileUploadComponent;
}
