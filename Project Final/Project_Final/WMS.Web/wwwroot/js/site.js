// WMS Global UI Helper Functions

const WMS = {
    // Show Toast notification
    showToast: function (message, type = 'success') {
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) return;

        const bgClass = type === 'success' ? 'bg-success' : type === 'warning' ? 'bg-warning text-dark' : 'bg-danger';
        const icon = type === 'success' ? 'fa-circle-check' : type === 'warning' ? 'fa-triangle-exclamation' : 'fa-circle-xmark';

        const toastEl = document.createElement('div');
        toastEl.className = `toast align-items-center text-white ${bgClass} border-0 show shadow-lg mb-2`;
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');

        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center">
                    <i class="fa-solid ${icon} fs-5 me-2"></i>
                    <span>${message}</span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        toastContainer.appendChild(toastEl);
        setTimeout(() => {
            toastEl.classList.remove('show');
            setTimeout(() => toastEl.remove(), 300);
        }, 4000);
    },

    // Confirm Dialog Modal
    confirm: function (title, message, onConfirm) {
        let modalEl = document.getElementById('wms-confirm-modal');
        if (!modalEl) {
            modalEl = document.createElement('div');
            modalEl.id = 'wms-confirm-modal';
            modalEl.className = 'modal fade';
            modalEl.innerHTML = `
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="confirm-modal-title">Xác nhận</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body" id="confirm-modal-body">
                            Bạn có chắc chắn muốn thực hiện thao tác này?
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                            <button type="button" class="btn btn-primary" id="confirm-modal-btn">Xác nhận</button>
                        </div>
                    </div>
                </div>
            `;
            document.body.appendChild(modalEl);
        }

        document.getElementById('confirm-modal-title').innerText = title;
        document.getElementById('confirm-modal-body').innerText = message;

        const confirmBtn = document.getElementById('confirm-modal-btn');
        const modal = new bootstrap.Modal(modalEl);

        const newBtn = confirmBtn.cloneNode(true);
        confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);

        newBtn.addEventListener('click', function () {
            modal.hide();
            if (typeof onConfirm === 'function') onConfirm();
        });

        modal.show();
    },

    // Language switcher toggle
    setLanguage: function (lang) {
        localStorage.setItem('wms_lang', lang);
        location.reload();
    }
};

document.addEventListener('DOMContentLoaded', function () {
    // Toggle sidebar on mobile
    const toggleBtn = document.getElementById('sidebarCollapse');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            document.getElementById('sidebar').classList.toggle('active');
        });
    }
});
