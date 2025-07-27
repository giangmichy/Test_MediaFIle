// Complete Media Files JavaScript with Format Filtering, Enhanced Auto-Detection and Storage Type Support

console.log('🚀 Media Files JS Loading...');

// DOM Ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ DOM Ready - Initializing Media Files');

    // Auto-hide alerts after 5 seconds
    setTimeout(() => {
        document.querySelectorAll('.alert').forEach(alert => {
            try {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } catch (e) {
                // Ignore if bootstrap alert is not available
            }
        });
    }, 5000);

    // File upload auto-detection
    const fileInput = document.getElementById('FileUpload');
    if (fileInput) {
        fileInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            console.log('📁 File selected:', file.name);
            autoDetectFileType(file);
        });
    }

    // Media type change event for format filtering
    const mediaTypeSelect = document.getElementById('MediaType');
    if (mediaTypeSelect) {
        mediaTypeSelect.addEventListener('change', filterFormats);
    }

    // Storage type change event
    const storageTypeSelect = document.getElementById('StorageType');
    if (storageTypeSelect) {
        storageTypeSelect.addEventListener('change', showStorageInfo);
        // Initialize storage info on page load
        showStorageInfo();
    }

    console.log('📋 Media Files System Ready');
});

// Storage Type handling function
function showStorageInfo() {
    const storageType = document.getElementById('StorageType')?.value;

    // Hide all info boxes
    const localInfo = document.getElementById('localInfo');
    const uncInfo = document.getElementById('uncInfo');
    const ftpInfo = document.getElementById('ftpInfo');

    if (localInfo) localInfo.classList.add('d-none');
    if (uncInfo) uncInfo.classList.add('d-none');
    if (ftpInfo) ftpInfo.classList.add('d-none');

    // Show selected storage info
    switch (storageType) {
        case 'Local':
            if (localInfo) localInfo.classList.remove('d-none');
            break;
        case 'UNC':
            if (uncInfo) uncInfo.classList.remove('d-none');
            break;
        case 'FTP':
            if (ftpInfo) ftpInfo.classList.remove('d-none');
            break;
    }

    console.log('📁 Storage type changed to:', storageType);
}

// Format filtering function
function filterFormats() {
    const mediaTypeSelect = document.getElementById('MediaType');
    const formatSelect = document.getElementById('Format');

    if (!mediaTypeSelect || !formatSelect) return;

    const selectedMediaType = mediaTypeSelect.value;
    console.log('🔄 Filtering formats for media type:', selectedMediaType);

    // Get all format options
    const allOptions = formatSelect.querySelectorAll('option');

    // Reset format selection
    formatSelect.value = '';

    // Show/hide options based on media type
    allOptions.forEach(option => {
        if (option.value === '') {
            // Keep the default "-- Chọn định dạng --" option
            option.style.display = 'block';
            return;
        }

        const category = option.getAttribute('data-category');
        if (!selectedMediaType || category === selectedMediaType) {
            option.style.display = 'block';
        } else {
            option.style.display = 'none';
        }
    });

    console.log('✅ Format filtering complete');
}

// Auto-detect file type and format - Updated với format mới
function autoDetectFileType(file) {
    const fileName = file.name.toLowerCase();
    const mediaTypeSelect = document.getElementById('MediaType');
    const formatSelect = document.getElementById('Format');

    if (!mediaTypeSelect || !formatSelect) return;

    console.log('🔍 Auto-detecting file type for:', fileName);

    // Reset selections
    mediaTypeSelect.value = '';
    formatSelect.value = '';

    // Image formats
    if (fileName.endsWith('.jpg')) {
        mediaTypeSelect.value = 'Image';
        filterFormats(); // Apply filtering first
        formatSelect.value = 'Jpg';
    } else if (fileName.endsWith('.jpeg')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Jpeg';
    } else if (fileName.endsWith('.png')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Png';
    } else if (fileName.endsWith('.gif')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Gif';
    } else if (fileName.endsWith('.bmp')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Bmp';
    } else if (fileName.endsWith('.svg')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Svg';
    } else if (fileName.endsWith('.webp')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Webp';
    } else if (fileName.endsWith('.tiff') || fileName.endsWith('.tif')) {
        mediaTypeSelect.value = 'Image';
        filterFormats();
        formatSelect.value = 'Tiff';
    }

    // Video formats
    else if (fileName.endsWith('.mp4')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Mp4';
    } else if (fileName.endsWith('.avi')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Avi';
    } else if (fileName.endsWith('.mov')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Mov';
    } else if (fileName.endsWith('.wmv')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Wmv';
    } else if (fileName.endsWith('.mkv')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Mkv';
    } else if (fileName.endsWith('.flv')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Flv';
    } else if (fileName.endsWith('.webm')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Webm';
    } else if (fileName.endsWith('.mpeg') || fileName.endsWith('.mpg')) {
        mediaTypeSelect.value = 'Video';
        filterFormats();
        formatSelect.value = 'Mpeg';
    }

    // Audio formats
    else if (fileName.endsWith('.mp3')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'Mp3';
    } else if (fileName.endsWith('.wav')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'Wav';
    } else if (fileName.endsWith('.ogg')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'Ogg';
    } else if (fileName.endsWith('.flac')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'Flac';
    } else if (fileName.endsWith('.aac')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'Aac';
    } else if (fileName.endsWith('.m4a')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'M4a';
    } else if (fileName.endsWith('.wma')) {
        mediaTypeSelect.value = 'Audio';
        filterFormats();
        formatSelect.value = 'Wma';
    }

    // Document formats
    else if (fileName.endsWith('.pdf')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Pdf';
    } else if (fileName.endsWith('.doc')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Doc';
    } else if (fileName.endsWith('.docx')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Docx';
    } else if (fileName.endsWith('.xls')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Xls';
    } else if (fileName.endsWith('.xlsx')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Xlsx';
    } else if (fileName.endsWith('.ppt')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Ppt';
    } else if (fileName.endsWith('.pptx')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Pptx';
    } else if (fileName.endsWith('.txt')) {
        mediaTypeSelect.value = 'Document';
        filterFormats();
        formatSelect.value = 'Txt';
    }

    // Default to Other
    else {
        mediaTypeSelect.value = 'Other';
        filterFormats();
        formatSelect.value = 'Other';
    }

    console.log('✅ Auto-detection complete:', mediaTypeSelect.value, formatSelect.value);
}

// Utility Functions
function showLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.remove('d-none');
        console.log('⏳ Loading overlay shown');
    }
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.add('d-none');
        console.log('✅ Loading overlay hidden');
    }
}

function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

// 📋 CHI TIẾT FUNCTION
function showFileDetails(fileId) {
    console.log('📋 Showing details for file:', fileId);
    showLoading();

    fetch(`/MediaFiles/GetDetails?id=${fileId}`)
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            hideLoading();
            console.log('File details:', data);

            if (!data.success) {
                alert('Lỗi: ' + data.message);
                return;
            }

            showDetailsModal(data);
        })
        .catch(error => {
            hideLoading();
            console.error('Error:', error);
            alert('Không thể tải thông tin file: ' + error.message);
        });
}

// Enhanced file details modal để hiển thị storage info
function showDetailsModal(data) {
    const filePath = data.storagePath.startsWith('/') ? data.storagePath : '/' + data.storagePath;

    let mediaPreview = '';
    if (data.mediaType === 'Image' && data.storageType === 'Local') {
        mediaPreview = `
            <div class="text-center mb-4">
                <img src="${filePath}" class="img-fluid rounded shadow" 
                     style="max-height: 400px; max-width: 100%;" alt="${escapeHtml(data.fileName)}">
            </div>`;
    } else if (data.mediaType === 'Video' && data.storageType === 'Local') {
        mediaPreview = `
            <div class="text-center mb-4">
                <video controls class="w-100 rounded shadow" 
                       style="max-height: 400px; background: #000;" 
                       preload="metadata">
                    <source src="${filePath}" type="video/mp4">
                    <source src="${filePath}" type="video/webm">
                    <div class="alert alert-warning mt-3">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Trình duyệt không hỗ trợ định dạng video này.
                    </div>
                </video>
            </div>`;
    } else if (data.mediaType === 'Audio' && data.storageType === 'Local') {
        mediaPreview = `
            <div class="text-center mb-4">
                <div class="bg-light rounded p-4">
                    <i class="fas fa-music fa-4x text-info mb-3"></i>
                    <h5>${escapeHtml(data.fileName)}</h5>
                    <audio controls class="w-100 mt-3" style="max-width: 400px;">
                        <source src="${filePath}" type="audio/mp3">
                        <source src="${filePath}" type="audio/wav">
                        Trình duyệt không hỗ trợ audio.
                    </audio>
                </div>
            </div>`;
    } else {
        // For UNC/FTP files or other types, show file icon
        const storageIcon = data.storageType === 'UNC' ? 'fa-network-wired' :
            data.storageType === 'FTP' ? 'fa-cloud' : 'fa-file';
        const storageColor = data.storageType === 'UNC' ? 'text-warning' :
            data.storageType === 'FTP' ? 'text-success' : 'text-muted';

        mediaPreview = `
            <div class="text-center mb-4">
                <div class="bg-light rounded p-4">
                    <i class="fas ${storageIcon} fa-4x ${storageColor} mb-3"></i>
                    <h5>${data.format} File</h5>
                    <p>${escapeHtml(data.fileName)}</p>
                    <span class="badge bg-info">${data.storageType} Storage</span>
                </div>
            </div>`;
    }

    const modalContent = `
        ${mediaPreview}
        <div class="row">
            <div class="col-md-6">
                <h6 class="text-primary mb-3"><i class="fas fa-info-circle me-2"></i>Thông tin File</h6>
                <table class="table table-sm">
                    <tr><th>Tên file:</th><td>${escapeHtml(data.fileName)}</td></tr>
                    <tr><th>Loại:</th><td><span class="badge bg-primary">${data.mediaType}</span></td></tr>
                    <tr><th>Định dạng:</th><td><span class="badge bg-info">${data.format}</span></td></tr>
                    <tr><th>Kích thước:</th><td><strong>${formatFileSize(data.size)}</strong></td></tr>
                    <tr><th>Hash:</th><td><code class="small">${data.hash}</code></td></tr>
                </table>
            </div>
            <div class="col-md-6">
                <h6 class="text-success mb-3"><i class="fas fa-hdd me-2"></i>Lưu trữ & Chi tiết</h6>
                <table class="table table-sm">
                    <tr><th>Phương thức:</th><td><span class="badge ${data.storageType === 'Local' ? 'bg-secondary' : data.storageType === 'UNC' ? 'bg-warning' : 'bg-success'}">${data.storageType}</span></td></tr>
                    <tr><th>Ngày tải:</th><td>${new Date(data.uploadTime).toLocaleDateString('vi-VN')}</td></tr>
                    <tr><th>Trạng thái:</th><td><span class="badge bg-success">${data.status}</span></td></tr>
                    <tr><th>Đường dẫn:</th><td><code class="small">${data.storagePath}</code></td></tr>
                    ${data.networkPath ? `<tr><th>Network Path:</th><td><code class="small">${data.networkPath}</code></td></tr>` : ''}
                    ${data.description ? `<tr><th>Mô tả:</th><td>${escapeHtml(data.description)}</td></tr>` : ''}
                </table>
            </div>
        </div>
    `;

    const downloadButton = data.storageType === 'Local' ?
        { text: 'Tải xuống', class: 'btn-primary', click: `window.open('/MediaFiles/Download/${data.id}', '_blank')` } :
        { text: 'Xem đường dẫn', class: 'btn-info', click: `alert('File được lưu tại: ${data.networkPath || data.storagePath}')` };

    showModal('Chi tiết File: ' + escapeHtml(data.fileName), modalContent, [
        { text: 'Đóng', class: 'btn-secondary', dismiss: true },
        downloadButton
    ]);
}

// ✏️ SỬA FUNCTION
function editFile(fileId) {
    console.log('✏️ Editing file:', fileId);
    showLoading();

    fetch(`/MediaFiles/GetDetails?id=${fileId}`)
        .then(response => response.json())
        .then(data => {
            hideLoading();
            if (!data.success) {
                alert('Lỗi: ' + data.message);
                return;
            }
            showEditModal(data);
        })
        .catch(error => {
            hideLoading();
            console.error('Error:', error);
            alert('Không thể tải thông tin file');
        });
}

function showEditModal(data) {
    const editContent = `
        <form id="editForm">
            <input type="hidden" name="__RequestVerificationToken" value="${getAntiForgeryToken()}">
            <input type="hidden" name="Id" value="${data.id}">
            <input type="hidden" name="FileName" value="${escapeHtml(data.fileName)}">
            <input type="hidden" name="Hash" value="${data.hash || ''}">
            <input type="hidden" name="Size" value="${data.size}">
            <input type="hidden" name="UploadTime" value="${data.uploadTime}">
            <input type="hidden" name="MediaInfoJson" value="${escapeHtml(data.mediaInfoJson || '')}">
            <input type="hidden" name="StoragePath" value="${data.storagePath}">
            <input type="hidden" name="Format" value="${data.format}">
            <input type="hidden" name="StorageType" value="${data.storageType || 'Local'}">
            
            <div class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                <strong>Đang chỉnh sửa:</strong> ${escapeHtml(data.fileName)}
                <span class="badge bg-${data.storageType === 'Local' ? 'secondary' : data.storageType === 'UNC' ? 'warning' : 'success'} ms-2">${data.storageType}</span>
            </div>
            
            <div class="mb-3">
                <label class="form-label fw-bold">
                    <i class="fas fa-comment-alt me-1"></i>Mô tả
                </label>
                <textarea class="form-control" name="Description" rows="3" placeholder="Nhập mô tả cho file...">${data.description || ''}</textarea>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <label class="form-label fw-bold">
                        <i class="fas fa-tags me-1"></i>Loại Media
                    </label>
                    <select class="form-select" name="MediaType" required>
                        <option value="Image" ${data.mediaType === 'Image' ? 'selected' : ''}>Image</option>
                        <option value="Video" ${data.mediaType === 'Video' ? 'selected' : ''}>Video</option>
                        <option value="Audio" ${data.mediaType === 'Audio' ? 'selected' : ''}>Audio</option>
                        <option value="Document" ${data.mediaType === 'Document' ? 'selected' : ''}>Document</option>
                        <option value="Other" ${data.mediaType === 'Other' ? 'selected' : ''}>Other</option>
                    </select>
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">
                        <i class="fas fa-check-circle me-1"></i>Trạng thái
                    </label>
                    <select class="form-select" name="Status" required>
                        <option value="Active" ${data.status === 'Active' ? 'selected' : ''}>Active</option>
                        <option value="Deleted" ${data.status === 'Deleted' ? 'selected' : ''}>Deleted</option>
                        <option value="Processing" ${data.status === 'Processing' ? 'selected' : ''}>Processing</option>
                    </select>
                </div>
            </div>
        </form>
    `;

    showModal('Chỉnh sửa File: ' + escapeHtml(data.fileName), editContent, [
        { text: 'Hủy', class: 'btn-secondary', dismiss: true },
        { text: 'Lưu', class: 'btn-primary', click: 'submitEdit()' }
    ]);
}

function submitEdit() {
    console.log('💾 Submitting edit...');
    const form = document.getElementById('editForm');

    if (!form) {
        alert('Không tìm thấy form chỉnh sửa');
        return;
    }

    const formData = new FormData(form);

    showLoading();

    fetch('/MediaFiles/Update', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            hideLoading();
            if (data.success) {
                hideCurrentModal();
                alert('Cập nhật thành công!');
                location.reload();
            } else {
                alert('Lỗi: ' + (data.message || 'Cập nhật thất bại'));
            }
        })
        .catch(error => {
            hideLoading();
            console.error('Error:', error);
            alert('Lỗi cập nhật file: ' + error.message);
        });
}

// 🗑️ XÓA FUNCTION
function confirmDelete(fileId, fileName) {
    console.log('🗑️ Delete confirmation for:', fileId, fileName);

    const deleteContent = `
        <div class="text-center">
            <div class="mb-4">
                <i class="fas fa-exclamation-triangle fa-4x text-danger"></i>
            </div>
            <h4 class="text-danger mb-3">Xác nhận xóa file</h4>
            <p class="mb-3">Bạn có chắc chắn muốn xóa file:</p>
            <div class="alert alert-light border">
                <strong>"${escapeHtml(fileName)}"</strong>
            </div>
            <div class="alert alert-warning">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>Cảnh báo:</strong> Hành động này không thể hoàn tác!
            </div>
        </div>
        <form id="deleteForm">
            <input type="hidden" name="__RequestVerificationToken" value="${getAntiForgeryToken()}">
        </form>
    `;

    showModal('Xác nhận xóa file', deleteContent, [
        { text: 'Hủy', class: 'btn-secondary', dismiss: true },
        { text: 'Xóa', class: 'btn-danger', click: `submitDelete('${fileId}')` }
    ]);
}

function submitDelete(fileId) {
    console.log('🗑️ Submitting delete for:', fileId);
    const form = document.getElementById('deleteForm');

    if (!form) {
        alert('Không tìm thấy form xóa');
        return;
    }

    const formData = new FormData(form);

    showLoading();

    fetch(`/MediaFiles/Delete/${fileId}`, {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            hideLoading();
            if (data.success) {
                hideCurrentModal();
                alert('Xóa file thành công!');
                location.reload();
            } else {
                alert('Lỗi: ' + (data.message || 'Xóa file thất bại'));
            }
        })
        .catch(error => {
            hideLoading();
            console.error('Error:', error);
            alert('Lỗi xóa file: ' + error.message);
        });
}

// 🎭 MODAL FUNCTIONS
function showModal(title, content, buttons) {
    // Remove existing modal
    const existing = document.getElementById('dynamicModal');
    if (existing) existing.remove();

    let buttonHtml = '';
    buttons.forEach(btn => {
        const dismiss = btn.dismiss ? 'data-bs-dismiss="modal"' : '';
        const click = btn.click ? `onclick="${btn.click}"` : '';
        buttonHtml += `<button type="button" class="btn ${btn.class}" ${dismiss} ${click}>${btn.text}</button>`;
    });

    const modalHtml = `
        <div class="modal fade" id="dynamicModal" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content" style="border-radius: 12px; border: none; box-shadow: 0 10px 30px rgba(0,0,0,0.3);">
                    <div class="modal-header" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 12px 12px 0 0; border: none;">
                        <h5 class="modal-title fw-bold">${title}</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4">${content}</div>
                    <div class="modal-footer border-0 bg-light" style="border-radius: 0 0 12px 12px;">
                        ${buttonHtml}
                    </div>
                </div>
            </div>
        </div>
    `;

    document.body.insertAdjacentHTML('beforeend', modalHtml);

    const modal = new bootstrap.Modal(document.getElementById('dynamicModal'));
    modal.show();

    // Cleanup when modal is hidden
    document.getElementById('dynamicModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

function hideCurrentModal() {
    const modalElement = document.getElementById('dynamicModal');
    if (modalElement) {
        const modal = bootstrap.Modal.getInstance(modalElement);
        if (modal) {
            modal.hide();
        }
    }
}

// Upload form submission with loading
document.addEventListener('DOMContentLoaded', function () {
    const uploadForm = document.getElementById('uploadForm');
    if (uploadForm) {
        uploadForm.addEventListener('submit', function () {
            showLoading();
            // Loading will be hidden when page reloads
        });
    }
});

console.log('✅ Media Files JavaScript Ready!');