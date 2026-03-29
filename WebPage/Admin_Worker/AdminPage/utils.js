function setLog(id, message, isError = false) {
    const el = document.getElementById(id);
    if (el) {
        el.textContent = message;
        el.style.color = isError ? '#b91c1c' : '#4b5563';
    }
}

function validateForm(form) {
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }
    return true;
}