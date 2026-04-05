function setLog(id, message, isError = false) {
    const el = document.getElementById(id);
    if (el) {
        el.textContent = message;
        el.style.color = isError ? '#b91c1c' : '#4b5563';
    }

    showToast(message, isError);
}

function showToast(message, isError = false) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast ${isError ? 'toast-error' : 'toast-success'}`;
    toast.textContent = message;

    toast.addEventListener('click', () => removeToast(toast));

    container.appendChild(toast);

    // Trigger animation
    requestAnimationFrame(() => {
        requestAnimationFrame(() => toast.classList.add('show'));
    });

    // Auto remove after 3.5s
    setTimeout(() => removeToast(toast), 3500);
}

function removeToast(toast) {
    toast.classList.remove('show');
    toast.addEventListener('transitionend', () => toast.remove(), { once: true });
}

function validateForm(form) {
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }
    return true;
}