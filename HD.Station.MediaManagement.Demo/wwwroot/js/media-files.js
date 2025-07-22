// Final Working Media Files JavaScript

console.log('🚀 Media Files JS Loading...');

// DOM Ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ DOM Ready - Initializing Media Files');

    // Auto-hide alerts
    setTimeout(() => {
        document.querySelectorAll('.alert').forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    console.log('📋 Media Files System Ready');
});

// Utility Functions
function showLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.remove('d-none');
    }
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.add('d-none');
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

function showDetailsModal(data) {
    const filePath = data.storagePath.startsWith('/') ? data.storagePath : '/' + data.storagePath;

    let mediaPreview = '';
    if (data.mediaType === 'Image') {
        mediaPreview = `
            <div class="text-center mb-4">
                <img src="${filePath}" class="img-fluid rounded shadow" 
                     style="max-height: 400px; max-width: 100%;" alt="${escapeHtml(data.fileName)}">
            </div>`;
    } else if (data.mediaType === 'Video') {
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
    } else if (data.mediaType === 'Audio') {
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
        mediaPreview = `
            <div class="text-center mb-4">
                <div class="bg-light rounded p-4">
                    <i class="fas fa-file fa-4x text-muted mb-3"></i>
                    <h5>${data.format} File</h5>
                    <p>${escapeHtml(data.fileName)}</p>
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
                    <tr><th>Kích thước:</th><td>${formatFileSize(data.size)}</td></tr>
                </table>
            </div>
            <div class="col-md-6">
                <h6 class="text-success mb-3"><i class="fas fa-cog me-2"></i>Chi tiết</h6>
                <table class="table table-sm">
                    <tr><th>Ngày tải:</th><td>${new Date(data.uploadTime).toLocaleDateString('vi-VN')}</td></tr>
                    <tr><th>Trạng thái:</th><td><span class="badge bg-success">${data.status}</span></td></tr>
                    <tr><th>Đường dẫn:</th><td><code class="small">${data.storagePath}</code></td></tr>
                    ${data.description ? `<tr><th>Mô tả:</th><td>${escapeHtml(data.description)}</td></tr>` : ''}
                </table>
            </div>
        </div>
    `;

    showModal('Chi tiết File: ' + escapeHtml(data.fileName), modalContent, [
        { text: 'Đóng', class: 'btn-secondary', dismiss: true },
        { text: 'Tải xuống', class: 'btn-primary', click: `window.open('${filePath}', '_blank')` }
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
            
            <div class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                <strong>Đang chỉnh sửa:</strong> ${escapeHtml(data.fileName)}
            </div>
            
            <div class="mb-3">
                <label class="form-label fw-bold">Mô tả</label>
                <textarea class="form-control" name="Description" rows="3">${data.description || ''}</textarea>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <label class="form-label fw-bold">Loại Media</label>
                    <select class="form-select" name="MediaType">
                        <option value="Image" ${data.mediaType === 'Image' ? 'selected' : ''}>Image</option>
                        <option value="Video" ${data.mediaType === 'Video' ? 'selected' : ''}>Video</option>
                        <option value="Audio" ${data.mediaType === 'Audio' ? 'selected' : ''}>Audio</option>
                        <option value="Document" ${data.mediaType === 'Document' ? 'selected' : ''}>Document</option>
                        <option value="Other" ${data.mediaType === 'Other' ? 'selected' : ''}>Other</option>
                    </select>
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-bold">Trạng thái</label>
                    <select class="form-select" name="Status">
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
    const formData = new FormData(form);

    showLoading();

    fetch('/MediaFiles/Update', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            hideLoading();
            if (response.ok) {
                hideCurrentModal();
                alert('Cập nhật thành công!');
                location.reload();
            } else {
                alert('Lỗi cập nhật file');
            }
        })
        .catch(error => {
            hideLoading();
            console.error('Error:', error);
            alert('Lỗi cập nhật file');
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
    const formData = new FormData(form);

    showLoading();

    fetch(`/MediaFiles/Delete/${fileId}`, {
        method: 'POST',
        body: formData
    })
        .then(response => {
            hideLoading();
            if (response.ok) {
                hideCurrentModal();
                alert('Xóa file thành công!');
                location.reload();
            } else {
                alert('Lỗi xóa file');
            }
        })
        .catch(error => {
            hideLoading();
            console.error('Error:', error);
            alert('Lỗi xóa file');
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

    // Cleanup
    document.getElementById('dynamicModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

function hideCurrentModal() {
    const modal = bootstrap.Modal.getInstance(document.getElementById('dynamicModal'));
    if (modal) modal.hide();
}

console.log('✅ Media Files JavaScript Ready!');