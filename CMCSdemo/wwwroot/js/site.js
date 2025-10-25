// ========================================
// CMCS - Site JavaScript
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    console.log('CMCS Application Loaded');

    // File upload handling
    initializeFileUpload();

    // Auto-hide alerts after 5 seconds
    autoHideAlerts();

    // Form validation
    initializeFormValidation();

    // Confirmation dialogs for actions
    initializeConfirmations();
});

// ========================================
// FILE UPLOAD FUNCTIONALITY
// ========================================

function initializeFileUpload() {
    const fileInput = document.getElementById('fileInput');
    const fileList = document.getElementById('fileList');
    const fileUploadArea = document.getElementById('fileUploadArea');

    if (!fileInput) return;

    // Handle file selection
    fileInput.addEventListener('change', function (e) {
        displaySelectedFiles(e.target.files);
    });

    // Drag and drop functionality
    if (fileUploadArea) {
        fileUploadArea.addEventListener('dragover', function (e) {
            e.preventDefault();
            fileUploadArea.style.borderColor = '#3498db';
            fileUploadArea.style.backgroundColor = '#f8f9fa';
        });

        fileUploadArea.addEventListener('dragleave', function (e) {
            e.preventDefault();
            fileUploadArea.style.borderColor = '#a0a0a0';
            fileUploadArea.style.backgroundColor = 'transparent';
        });

        fileUploadArea.addEventListener('drop', function (e) {
            e.preventDefault();
            fileUploadArea.style.borderColor = '#a0a0a0';
            fileUploadArea.style.backgroundColor = 'transparent';

            const files = e.dataTransfer.files;
            fileInput.files = files;
            displaySelectedFiles(files);
        });
    }

    function displaySelectedFiles(files) {
        if (!fileList) return;

        fileList.innerHTML = '';

        if (files.length === 0) {
            fileList.innerHTML = '<p style="color: #7f8c8d;">No files selected</p>';
            return;
        }

        const allowedExtensions = ['.pdf', '.docx', '.xlsx', '.jpg', '.png'];
        const maxSize = 5 * 1024 * 1024; // 5MB

        Array.from(files).forEach((file, index) => {
            const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
            const fileSize = (file.size / 1024).toFixed(2);

            let statusIcon = '✅';
            let statusText = 'Valid';
            let statusColor = '#27ae60';

            // Validate file type
            if (!allowedExtensions.includes(fileExtension)) {
                statusIcon = '❌';
                statusText = 'Invalid file type';
                statusColor = '#e74c3c';
            }

            // Validate file size
            if (file.size > maxSize) {
                statusIcon = '❌';
                statusText = 'File too large (max 5MB)';
                statusColor = '#e74c3c';
            }

            const fileItem = document.createElement('div');
            fileItem.style.cssText = `
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 12px;
                background-color: #f8f9fa;
                border-radius: 8px;
                margin-bottom: 10px;
            `;

            fileItem.innerHTML = `
                <div style="display: flex; align-items: center; gap: 10px;">
                    <span style="font-size: 24px;">📄</span>
                    <div>
                        <div style="font-weight: 600; color: #2c3e50;">${file.name}</div>
                        <div style="font-size: 12px; color: #7f8c8d;">${fileSize} KB</div>
                    </div>
                </div>
                <div style="display: flex; align-items: center; gap: 10px;">
                    <span style="color: ${statusColor}; font-size: 12px; font-weight: 600;">${statusIcon} ${statusText}</span>
                </div>
            `;

            fileList.appendChild(fileItem);
        });
    }
}

// ========================================
// AUTO-HIDE ALERTS
// ========================================

function autoHideAlerts() {
    const alerts = document.querySelectorAll('.alert');

    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.remove();
            }, 500);
        }, 5000);
    });
}

// ========================================
// FORM VALIDATION
// ========================================

function initializeFormValidation() {
    const claimForm = document.getElementById('claimForm');

    if (!claimForm) return;

    claimForm.addEventListener('submit', function (e) {
        const hoursWorked = parseFloat(document.getElementById('HoursWorked')?.value || 0);
        const hourlyRate = parseFloat(document.getElementById('HourlyRate')?.value || 0);

        if (hoursWorked <= 0) {
            e.preventDefault();
            alert('Please enter valid hours worked (greater than 0)');
            return false;
        }

        if (hourlyRate <= 0) {
            e.preventDefault();
            alert('Please enter valid hourly rate (greater than 0)');
            return false;
        }

        // Show loading state
        const submitButton = claimForm.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.textContent = 'Submitting...';
        }

        return true;
    });
}

// ========================================
// CONFIRMATION DIALOGS
// ========================================

function initializeConfirmations() {
    // Confirm reject actions
    const rejectButtons = document.querySelectorAll('button[value="reject"]');

    rejectButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (!confirm('Are you sure you want to reject this claim? This action cannot be undone.')) {
                e.preventDefault();
                return false;
            }
        });
    });

    // Confirm approve actions
    const approveButtons = document.querySelectorAll('button[value="approve"]');

    approveButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const action = button.textContent.includes('Verify') ? 'verify and forward' : 'approve';
            if (!confirm(`Are you sure you want to ${action} this claim?`)) {
                e.preventDefault();
                return false;
            }
        });
    });
}

// ========================================
// UTILITY FUNCTIONS
// ========================================

// Format currency
function formatCurrency(amount) {
    return 'R ' + parseFloat(amount).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
}

// Format date
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('en-ZA', options);
}

// Show notification
function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        animation: slideIn 0.3s ease-out;
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease-out';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }

    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);