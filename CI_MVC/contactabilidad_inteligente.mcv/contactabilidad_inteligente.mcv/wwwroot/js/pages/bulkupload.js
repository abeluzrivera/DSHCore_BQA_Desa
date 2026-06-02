// Bulk Upload Page JavaScript
(function() {
    'use strict';

    let currentStep = 1;
    let selectedFile = null;
    let uploadHistory = JSON.parse(localStorage.getItem('uploadHistory') || '[]');

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeFileUpload();
        initializeButtons();
        initializeDragAndDrop();
        renderUploadHistory();
    });

    // Initialize file upload
    function initializeFileUpload() {
        const fileInput = document.getElementById('fileInput');
        const dropzone = document.getElementById('dropzone');

        if (fileInput) {
            fileInput.addEventListener('change', handleFileSelect);
        }
                                                                                                                                                            
        if (dropzone) {
            // Prevenir el click del input cuando se hace click en el dropzone
            dropzone.addEventListener('click', function(e) {
                // Solo abrir el selector si no se hizo click directamente en el input
                if (e.target !== fileInput) {
                    e.preventDefault();
                    fileInput.click();
                }
            });
        }
    }

    // Handle file selection
    function handleFileSelect(event) {
        const file = event.target.files[0];
        
        if (!file) return;

        // Validate file type
        const allowedExtensions = ['.xlsx', '.xls', '.csv'];
        const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

        if (!allowedExtensions.includes(fileExtension)) {
            alert('❌ Formato no válido\n\nPor favor, seleccione un archivo Excel (.xlsx, .xls) o CSV (.csv)');
            event.target.value = '';
            return;
        }

        // Validate file size (25MB max)
        const maxSize = 25 * 1024 * 1024;
        if (file.size > maxSize) {
            alert('❌ Archivo muy grande\n\nEl archivo no debe superar 25 MB.\n\nTamaño actual: ' + 
                  (file.size / 1024 / 1024).toFixed(2) + ' MB');
            event.target.value = '';
            return;
        }

        selectedFile = file;
        showFilePreview(file);
    }

    // Show file preview
    function showFilePreview(file) {
        const filePreview = document.getElementById('filePreview');
        const fileName = document.getElementById('fileName');
        const fileSize = document.getElementById('fileSize');
        const uploadBtn = document.getElementById('uploadBtn');

        if (filePreview && fileName && fileSize) {
            fileName.textContent = file.name;
            fileSize.textContent = formatFileSize(file.size);
            filePreview.classList.remove('d-none');
        }

        if (uploadBtn) {
            uploadBtn.classList.remove('d-none');
        }
    }

    // Format file size
    function formatFileSize(bytes) {
        const sizes = ['B', 'KB', 'MB', 'GB'];
        if (bytes === 0) return '0 B';
        const i = Math.floor(Math.log(bytes) / Math.log(1024));
        return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
    }

    // Add to upload history
    function addToUploadHistory(fileName, fileSize) {
        const historyItem = {
            name: fileName,
            size: formatFileSize(fileSize),
            timestamp: new Date().toISOString(),
            displayTime: getRelativeTime(new Date())
        };

        // Add to beginning of array
        uploadHistory.unshift(historyItem);

        // Keep only last 5 items
        uploadHistory = uploadHistory.slice(0, 5);

        // Save to localStorage
        localStorage.setItem('uploadHistory', JSON.stringify(uploadHistory));

        // Re-render history
        renderUploadHistory();
    }

    // Get relative time (e.g., "Hace 2 horas")
    function getRelativeTime(date) {
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Ahora mismo';
        if (diffMins < 60) return `Hace ${diffMins} min`;
        if (diffHours < 24) return `Hace ${diffHours} hora${diffHours > 1 ? 's' : ''}`;
        if (diffDays < 7) return `Hace ${diffDays} día${diffDays > 1 ? 's' : ''}`;
        return date.toLocaleDateString('es-ES');
    }

    // Render upload history
    function renderUploadHistory() {
        const historyList = document.getElementById('historyList');
        
        if (!historyList) return;

        if (uploadHistory.length === 0) {
            historyList.innerHTML = `
                <div class="history-empty">
                    <span class="material-icons">history</span>
                    <p>No hay archivos recientes</p>
                </div>
            `;
            return;
        }

        historyList.innerHTML = uploadHistory.map(item => {
            const icon = item.name.endsWith('.csv') ? 'csv' : 'xlsx';
            const timestamp = new Date(item.timestamp);
            const relativeTime = getRelativeTime(timestamp);

            return `
                <div class="history-item">
                    <div class="history-icon ${icon}">
                        <span class="material-icons">description</span>
                    </div>
                    <div class="history-details">
                        <p class="history-name">${item.name}</p>
                        <p class="history-time">${relativeTime}</p>
                    </div>
                    <button class="history-delete" onclick="deleteHistoryItem('${item.timestamp}')" title="Eliminar">
                        <span class="material-icons">close</span>
                    </button>
                </div>
            `;
        }).join('');
    }

    // Delete history item
    window.deleteHistoryItem = function(timestamp) {
        uploadHistory = uploadHistory.filter(item => item.timestamp !== timestamp);
        localStorage.setItem('uploadHistory', JSON.stringify(uploadHistory));
        renderUploadHistory();
    };

    // Initialize buttons
    function initializeButtons() {
        const uploadBtn = document.getElementById('uploadBtn');
        const cancelBtn = document.getElementById('cancelBtn');
        const reloadBtn = document.getElementById('reloadBtn');
        const downloadLogBtn = document.getElementById('downloadLogBtn');
        const downloadTemplateBtn = document.getElementById('downloadTemplateBtn');
        const cancelProcessingBtn = document.getElementById('cancelProcessingBtn');

        if (uploadBtn) {
            uploadBtn.addEventListener('click', startUpload);
        }

        if (cancelBtn) {
            cancelBtn.addEventListener('click', resetUpload);
        }

        if (reloadBtn) {
            reloadBtn.addEventListener('click', resetUpload);
        }

        if (downloadLogBtn) {
            downloadLogBtn.addEventListener('click', downloadReport);
        }

        if (downloadTemplateBtn) {
            // Remover el event listener que previene la descarga
            // El link ahora funcionará normalmente
            downloadTemplateBtn.addEventListener('click', function(e) {
                console.log('Iniciando descarga de plantilla...');
            });
        }

        if (cancelProcessingBtn) {
            cancelProcessingBtn.addEventListener('click', resetUpload);
        }
    }

    // Start upload process
    async function startUpload() {
        if (!selectedFile) {
            alert('⚠️ No hay archivo seleccionado');
            return;
        }

        // Add to history before processing
        addToUploadHistory(selectedFile.name, selectedFile.size);

        // Update processing file info
        const processingFileName = document.getElementById('processingFileName');
        const processingFileSize = document.getElementById('processingFileSize');
        
        if (processingFileName) processingFileName.textContent = selectedFile.name;
        if (processingFileSize) processingFileSize.textContent = formatFileSize(selectedFile.size);

        // Move to step 2
        changeStep(2);

        // Simulate upload with progress
        await simulateUpload();

        // Process file
        await processFile();
    }

    // Simulate upload progress
    function simulateUpload() {
        return new Promise((resolve) => {
            const progressBarFill = document.getElementById('progressBarFill');
            const progressPercentage = document.getElementById('progressPercentage');
            const currentRecords = document.getElementById('currentRecords');
            const totalRecords = document.getElementById('totalRecords');
            const estimatedTime = document.getElementById('estimatedTime');

            let progress = 0;
            const total = 500;

            if (totalRecords) totalRecords.textContent = total;

            const interval = setInterval(() => {
                progress += Math.random() * 15;
                if (progress > 100) progress = 100;

                if (progressBarFill) {
                    progressBarFill.style.width = progress + '%';
                }

                if (progressPercentage) {
                    progressPercentage.textContent = Math.round(progress);
                }

                if (currentRecords) {
                    const current = Math.round((progress / 100) * total);
                    currentRecords.textContent = current;
                }

                if (estimatedTime) {
                    const seconds = Math.round((100 - progress) / 10);
                    estimatedTime.textContent = seconds > 0 ? `${seconds}s` : 'Finalizando...';
                }

                if (progress >= 100) {
                    clearInterval(interval);
                    setTimeout(resolve, 500);
                }
            }, 200);
        });
    }

    // Process file
    async function processFile() {
        try {
            const formData = new FormData();
            formData.append('file', selectedFile);

            const response = await fetch('?handler=Upload', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            const result = await response.json();

            if (result.success) {
                showReport(result);
                changeStep(3);
            } else {
                alert('❌ Error al procesar el archivo\n\n' + (result.message || 'Error desconocido'));
                resetUpload();
            }
        } catch (error) {
            console.error('Error:', error);
            alert('❌ Error al procesar el archivo\n\nPor favor, intente nuevamente.');
            resetUpload();
        }
    }

    // Show report
    function showReport(result) {
        const finalFileName = document.getElementById('finalFileName');
        const finalSuccessCount = document.getElementById('finalSuccessCount');
        const finalErrorCount = document.getElementById('finalErrorCount');
        const errorDetailsContainer = document.getElementById('errorDetailsContainer');
        const errorBadge = document.getElementById('errorBadge');
        const errorTableBody = document.getElementById('errorTableBody');

        if (finalFileName) finalFileName.textContent = selectedFile.name;
        if (finalSuccessCount) finalSuccessCount.textContent = result.successCount || 0;
        if (finalErrorCount) finalErrorCount.textContent = result.errorCount || 0;

        // Show/hide error details
        if (errorDetailsContainer) {
            if (result.errorCount > 0 && result.errors && result.errors.length > 0) {
                errorDetailsContainer.classList.remove('d-none');
                
                if (errorBadge) {
                    errorBadge.textContent = `${result.errorCount} error${result.errorCount > 1 ? 'es' : ''} detectado${result.errorCount > 1 ? 's' : ''}`;
                }

                if (errorTableBody) {
                    errorTableBody.innerHTML = result.errors.map(error => `
                        <tr>
                            <td>${error.row || 'N/A'}</td>
                            <td>${error.field || 'N/A'}</td>
                            <td>
                                <span class="material-icons">warning</span>
                                ${error.message || 'Error desconocido'}
                            </td>
                        </tr>
                    `).join('');
                }
            } else {
                // Si no hay errores, ocultar la tabla
                errorDetailsContainer.classList.add('d-none');
            }
        }
    }

    // Get icon for detail type
    function getIconForType(type) {
        switch (type) {
            case 'success': return 'check_circle';
            case 'warning': return 'warning';
            case 'error': return 'error';
            default: return 'info';
        }
    }

    // Change step
    function changeStep(step) {
        currentStep = step;

        // Update step indicator
        const steps = document.querySelectorAll('.step-item');
        steps.forEach((stepItem, index) => {
            const stepNumber = index + 1;
            stepItem.classList.remove('active', 'completed');
            
            if (stepNumber < step) {
                stepItem.classList.add('completed');
            } else if (stepNumber === step) {
                stepItem.classList.add('active');
            }
        });

        // Show/hide content
        document.getElementById('step1Content')?.classList.toggle('d-none', step !== 1);
        document.getElementById('step2Content')?.classList.toggle('d-none', step !== 2);
        document.getElementById('step3Content')?.classList.toggle('d-none', step !== 3);
    }

    // Reset upload
    function resetUpload() {
        selectedFile = null;
        currentStep = 1;

        const fileInput = document.getElementById('fileInput');
        const filePreview = document.getElementById('filePreview');
        const uploadBtn = document.getElementById('uploadBtn');
        const progressBarFill = document.getElementById('progressBarFill');
        const progressPercentage = document.getElementById('progressPercentage');

        if (fileInput) fileInput.value = '';
        if (filePreview) filePreview.classList.add('d-none');
        if (uploadBtn) uploadBtn.classList.add('d-none');
        if (progressBarFill) progressBarFill.style.width = '0%';
        if (progressPercentage) progressPercentage.textContent = '0';

        changeStep(1);
    }

    // Download report
    function downloadReport() {
        const errorTableBody = document.getElementById('errorTableBody');
        
        if (!errorTableBody || errorTableBody.children.length === 0) {
            alert('⚠️ No hay errores para descargar');
            return;
        }

        try {
            // Preparar datos para CSV
            const headers = ['FILA', 'CAMPO', 'MOTIVO DEL ERROR'];
            const rows = [];

            // Obtener todas las filas de la tabla
            Array.from(errorTableBody.querySelectorAll('tr')).forEach(row => {
                const cells = row.querySelectorAll('td');
                if (cells.length >= 3) {
                    const rowData = [
                        cells[0].textContent.trim(),
                        cells[1].textContent.trim(),
                        // Remover el icono del mensaje de error
                        cells[2].textContent.replace('warning', '').trim()
                    ];
                    rows.push(rowData);
                }
            });

            // Crear contenido CSV
            let csvContent = '\uFEFF'; // BOM para UTF-8
            csvContent += headers.join(',') + '\n';
            
            rows.forEach(row => {
                // Escapar comillas y valores con comas
                const escapedRow = row.map(cell => {
                    const escaped = cell.replace(/"/g, '""');
                    return escaped.includes(',') || escaped.includes('"') || escaped.includes('\n') 
                        ? `"${escaped}"` 
                        : escaped;
                });
                csvContent += escapedRow.join(',') + '\n';
            });

            // Crear blob y descargar
            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            
            if (link.download !== undefined) {
                const url = URL.createObjectURL(blob);
                const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
                const fileName = `Log_Errores_${selectedFile ? selectedFile.name.split('.')[0] : 'Carga'}_${timestamp}.csv`;
                
                link.setAttribute('href', url);
                link.setAttribute('download', fileName);
                link.style.visibility = 'hidden';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                URL.revokeObjectURL(url);
                
                console.log('Log de errores descargado exitosamente');
            }
        } catch (error) {
            console.error('Error al descargar log:', error);
            alert('❌ Error al generar el archivo de log\n\nPor favor, intente nuevamente.');
        }
    }

    // Initialize drag and drop
    function initializeDragAndDrop() {
        const dropzone = document.getElementById('dropzone');

        if (!dropzone) return;

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropzone.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        ['dragenter', 'dragover'].forEach(eventName => {
            dropzone.addEventListener(eventName, () => {
                dropzone.classList.add('drag-over');
            }, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropzone.addEventListener(eventName, () => {
                dropzone.classList.remove('drag-over');
            }, false);
        });

        dropzone.addEventListener('drop', handleDrop, false);
    }

    // Handle drop
    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;

        if (files.length > 0) {
            const fileInput = document.getElementById('fileInput');
            if (fileInput) {
                fileInput.files = files;
                handleFileSelect({ target: fileInput });
            }
        }
    }

})();
