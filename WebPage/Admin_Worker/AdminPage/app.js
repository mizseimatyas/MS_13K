const navButtons = document.querySelectorAll('.nav-btn');
const panels     = document.querySelectorAll('.panel');

let currentRole = null;

navButtons.forEach(btn => {
    btn.addEventListener('click', () => {
        const target = btn.dataset.view;
        panels.forEach(p => p.classList.remove('active'));
        const panel = document.getElementById(target);
        if (panel) panel.classList.add('active');
    });
});

function showPanel(id) {
    panels.forEach(p => p.classList.remove('active'));
    const panel = document.getElementById(id);
    if (panel) panel.classList.add('active');
}

function applyRoleUi() {
    const userTabBtn = document.querySelector('.nav-btn[data-view="user-panel"]');
    const userPanel  = document.getElementById('user-panel');

    if (!currentRole) return;

    if (currentRole === 'Admin') {
        if (userTabBtn) userTabBtn.style.display = '';
        if (userPanel)  userPanel.style.removeProperty('display');
        showPanel('user-panel');
    } else if (currentRole === 'Worker') {
        if (userTabBtn) userTabBtn.style.display = 'none';
        if (userPanel)  userPanel.style.display  = 'none';
        showPanel('item-panel');
    }
}
async function checkAuth() {
    try {
        const me = await apiFetch(`${API_BASE}/Admins/me`);
        currentRole = me.role;
        applyRoleUi();
        loadInitialItems();
        loadAllOrders();
        loadAllCategories();
        if (currentRole === 'Admin') {
            loadAllWorkers();
            loadAllUsers();
        }
    } catch {
        window.location.href = '/WebPage/Admin_Worker/Login/login.html';
    }
}

document.getElementById('logout-btn')
    ?.addEventListener('click', async () => {
        try {
            await apiFetch(`${API_BASE}/Admins/logout`, { method: 'POST' });
        } catch {}
        window.location.href = '/WebPage/Admin_Worker/Login/login.html';
    });
    
window.addEventListener('DOMContentLoaded', () => {
    checkAuth();

});