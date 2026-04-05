const navButtons = document.querySelectorAll('.nav-btn');
const panels = document.querySelectorAll('.panel');

let currentRole = null;

navButtons.forEach(btn => {
    btn.addEventListener('click', () => {
        const target = btn.dataset.view;
        panels.forEach(p => p.classList.remove('active'));
        const panel = document.getElementById(target);
        if (panel) panel.classList.add('active');
    });
});

async function checkAuth() {
    try {
        const me = await apiFetch(`${API_BASE}/Admins/me`);
        currentRole = me.role;
        applyRoleUi();
        if (currentRole === 'Admin') loadAllWorkers();
    } catch {
        window.location.href = '/WebPage/Admin_Worker/Login/login.html';
    }
}

function applyRoleUi() {
    const userTabBtn = document.querySelector('.nav-btn[data-view="user-panel"]');
    const userPanel  = document.getElementById('user-panel');
    const itemPanel  = document.getElementById('item-panel');
    const orderTabBtn = document.querySelector('.nav-btn[data-view="order-panel"]');
    const orderPanel = document.getElementById('order-panel');

    if (!currentRole) return;

    if (currentRole === 'Worker') {
        if (userTabBtn) userTabBtn.style.display = 'none';
        if (userPanel)  userPanel.style.display  = 'none';
        if (orderTabBtn) orderTabBtn.style.display = '';
        if (orderPanel)  orderPanel.style.display  = '';
        if (itemPanel) {
            panels.forEach(p => p.classList.remove('active'));
            itemPanel.classList.add('active');
        }
    } else if (currentRole === 'Admin') {
        if (userTabBtn) userTabBtn.style.display = '';
        if (userPanel)  userPanel.style.display  = '';
        if (orderPanel) orderPanel.style.display = '';
    }
}

window.addEventListener('DOMContentLoaded', () => {
    checkAuth();
    loadInitialItems();
    loadAllOrders();
    loadAllCategories();
});