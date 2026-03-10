const API_BASE = 'https://localhost:7149/api';

const navButtons = document.querySelectorAll('.nav-btn');
const panels = document.querySelectorAll('.panel');

navButtons.forEach(btn => {
    btn.addEventListener('click', () => {
        const target = btn.dataset.view;
        panels.forEach(p => p.classList.remove('active'));
        const panel = document.getElementById(target);
        if (panel) {
            panel.classList.add('active');
        }
    });
});

function setLog(id, message, isError = false) {
    const el = document.getElementById(id);
    if (el) {
        el.textContent = message;
        el.style.color = isError ? '#b91c1c' : '#4b5563';
        console.log(`[LOG:${id}]`, message);
    }
}

function validateForm(form) {
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }
    return true;
}

async function apiFetch(url, options = {}) {
    const response = await fetch(url, {
        headers: {
            'Content-Type': 'application/json',
            ...(options.headers || {})
        },
        ...options
    });

    if (!response.ok) {
        const text = await response.text();
        const msg = text || `HTTP hiba: ${response.status}`;
        throw new Error(msg);
    }

    const contentType = response.headers.get('Content-Type') || '';
    if (contentType.includes('application/json')) {
        return await response.json();
    }
    return await response.text();
}

/* Regisztráció */
document.getElementById('register-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;

    if (!validateForm(form)) return;

    const data = Object.fromEntries(new FormData(form));

    if (data.phone.length < 8) {
        setLog('register-log', 'A telefonszámnak legalább 8 karakter hosszúnak kell lennie.', true);
        return;
    }

    try {
        await apiFetch(`${API_BASE}/users/register`, {
            method: 'POST',
            body: JSON.stringify({
                username: data.username,
                password: data.password,
                phoneNumber: data.phone,
                role: data.role
            })
        });

        setLog('register-log', 'Sikeres regisztráció.');
        form.reset();
    } catch (err) {
        setLog('register-log', `Hiba regisztráció közben: ${err.message}`, true);
    }
});

/* Jelszó módosítás */
document.getElementById('password-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;

    if (!validateForm(form)) return;

    const data = Object.fromEntries(new FormData(form));

    if (data.newPassword.length < 6) {
        setLog('password-log', 'Az új jelszónak legalább 6 karakter hosszúnak kell lennie.', true);
        return;
    }

    try {
        await apiFetch(`${API_BASE}/users/password`, {
            method: 'PUT',
            body: JSON.stringify({
                workerId: Number(data.workerId),
                newPassword: data.newPassword
            })
        });

        setLog('password-log', 'A jelszó sikeresen módosítva.');
        form.reset();
    } catch (err) {
        setLog('password-log', `Hiba jelszó módosítás közben: ${err.message}`, true);
    }
});

/* Dolgozók táblázat renderelése + inline szerkesztés (jelszó NEM módosítható) */
function renderWorkersTable(workers) {
    const container = document.getElementById('worker-table-container');
    if (!container) return;

    if (!workers || workers.length === 0) {
        container.innerHTML = '<p>Nincsenek dolgozók.</p>';
        return;
    }

    let html = '<table class="data-table"><thead><tr>';
    html += '<th>WorkerId</th>';
    html += '<th>WorkerName</th>';
    html += '<th>Role</th>';
    html += '<th>Phone</th>';
    html += '<th>Password</th>';
    html += '<th></th>';
    html += '</tr></thead><tbody>';

    workers.forEach(work => {
        const phoneValue = work.phone ?? '';
        html += `<tr data-id="${work.workerId}">
            <td class="cell-id">${work.workerId}</td>
            <td class="cell-name">${work.workerName}</td>
            <td class="cell-role">${work.role}</td>
            <td class="cell-phone">${phoneValue}</td>
            <td class="cell-password">********</td>
            <td>
                <button class="secondary-btn btn-edit-worker">Módosítás</button>
            </td>
        </tr>`;
    });

    html += '</tbody></table>';

    container.innerHTML = html;

    const editButtons = container.querySelectorAll('.btn-edit-worker');
    editButtons.forEach(btn => {
        btn.addEventListener('click', onEditWorkerClick);
    });
}

function onEditWorkerClick(e) {
    const btn = e.currentTarget;
    const row = btn.closest('tr');
    const isEditing = row.dataset.editing === 'true';

    if (!isEditing) {
        // Edit mód (csak name, phone)
        row.dataset.editing = 'true';
        btn.textContent = 'Mentés';

        const nameCell = row.querySelector('.cell-name');
        const phoneCell = row.querySelector('.cell-phone');

        const name = nameCell.textContent.trim();
        const phone = phoneCell.textContent.trim();

        nameCell.innerHTML = `<input type="text" class="input-name" value="${name}">`;
        phoneCell.innerHTML = `<input type="number" class="input-phone" value="${phone}">`;
        // role/password cellhez nem nyúlunk
    } else {
        // Mentés
        saveWorkerRow(row, btn);
    }
}

async function saveWorkerRow(row, btn) {
    const id = Number(row.dataset.id);

    const nameInput = row.querySelector('.input-name');
    const phoneInput = row.querySelector('.input-phone');

    const workerName = nameInput.value.trim();
    const phone = phoneInput.value ? Number(phoneInput.value) : null;

    if (!workerName) {
        alert('A név nem lehet üres.');
        return;
    }

    const currentRole = row.querySelector('.cell-role').textContent.trim();

    try {
        await apiFetch(`${API_BASE}/Workers/changedata/${id}`, {
    method: 'PUT',
    body: JSON.stringify({
        workerName,
        role: currentRole,
        phone
    })
});

        row.dataset.editing = 'false';
        btn.textContent = 'Módosítás';

        row.querySelector('.cell-name').textContent = workerName;
        row.querySelector('.cell-phone').textContent = phone ?? '';
        // role/password változatlan
        row.querySelector('.cell-password').textContent = '********';

        setLog('worker-query-log', `Dolgozó módosítva (id = ${id}).`);
    } catch (err) {
        setLog('worker-query-log', `Hiba dolgozó módosítás közben: ${err.message}`, true);
    }
}
/* Dolgozók automatikus betöltése */
async function loadAllWorkers() {
    try {
        const workers = await apiFetch(`${API_BASE}/Admins/allworkers`, {
            method: 'GET'
        });
        renderWorkersTable(workers);
        setLog('worker-query-log', `Dolgozók száma: ${workers.length}`);
    } catch (err) {
        setLog('worker-query-log', `Hiba AllWorkers hívás közben: ${err.message}`, true);
    }
}

/* Termék táblázat */
function renderItemsTable(items) {
    const container = document.getElementById('items-table-container');
    if (!container) return;

    if (!items || items.length === 0) {
        container.innerHTML = '<p>Nincs egyetlen termék sem.</p>';
        return;
    }

    const headers = ['categoryId', 'itemName', 'quantity', 'description', 'price'];

    let html = '<table class="data-table"><thead><tr>';
    headers.forEach(h => {
        html += `<th>${h}</th>`;
    });
    html += '</tr></thead><tbody>';

    items.forEach(item => {
        html += '<tr>';
        headers.forEach(h => {
            html += `<td>${item[h]}</td>`;
        });
        html += '</tr>';
    });

    html += '</tbody></table>';

    container.innerHTML = html;
}

/* Összes termék */
document.getElementById('btn-all-items').addEventListener('click', async () => {
    try {
        const items = await apiFetch(`${API_BASE}/items/allitems`, {
            method: 'GET'
        });
        renderItemsTable(items);
        setLog('item-query-log', `AllItems sikeres. Találatok száma: ${items.length}.`);
    } catch (err) {
        setLog('item-query-log', `Hiba AllItems hívás közben: ${err.message}`, true);
    }
});

/* ItemById */
document.getElementById('item-by-id-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    if (!validateForm(form)) return;

    const data = Object.fromEntries(new FormData(form));
    const id = Number(data.itemId);

    if (!Number.isInteger(id) || id <= 0) {
        setLog('item-query-log', 'Az Item ID csak pozitív egész szám lehet.', true);
        return;
    }

    try {
        const url = `${API_BASE}/items/itembyid?id=${encodeURIComponent(id)}`;
        const item = await apiFetch(url, { method: 'GET' });

        renderItemsTable([item]);
        setLog('item-query-log', `ItemById sikeres, id = ${id}`);
        form.reset();
    } catch (err) {
        setLog('item-query-log', `Hiba ItemById hívás közben: ${err.message}`, true);
    }
});

/* Új termék */
document.getElementById('add-item-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    if (!validateForm(form)) return;

    const data = Object.fromEntries(new FormData(form));

    if (Number(data.quantity) <= 0) {
        setLog('item-modify-log', 'A mennyiségnek pozitívnak kell lennie.', true);
        return;
    }
    if (Number(data.price) <= 0) {
        setLog('item-modify-log', 'Az árnak pozitívnak kell lennie.', true);
        return;
    }

    try {
        await apiFetch(`${API_BASE}/items`, {
            method: 'POST',
            body: JSON.stringify({
                itemName: data.itemName,
                categoryName: data.categoryName,
                quantity: Number(data.quantity),
                description: data.description,
                price: Number(data.price)
            })
        });

        setLog('item-modify-log', 'Új termék sikeresen hozzáadva.');
        form.reset();
    } catch (err) {
        setLog('item-modify-log', `Hiba új termék hozzáadása közben: ${err.message}`, true);
    }
});

/* Termék módosítás */
document.getElementById('modify-item-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    if (!validateForm(form)) return;

    const data = Object.fromEntries(new FormData(form));
    const id = Number(data.itemId);

    if (!Number.isInteger(id) || id <= 0) {
        setLog('item-modify-log', 'Az Item ID csak pozitív egész szám lehet.', true);
        return;
    }
    if (Number(data.quantity) <= 0) {
        setLog('item-modify-log', 'A mennyiségnek pozitívnak kell lennie.', true);
        return;
    }
    if (Number(data.price) <= 0) {
        setLog('item-modify-log', 'Az árnak pozitívnak kell lennie.', true);
        return;
    }

    try {
        await apiFetch(`${API_BASE}/items/${id}`, {
            method: 'PUT',
            body: JSON.stringify({
                itemId: id,
                itemName: data.itemName,
                categoryName: data.categoryName,
                quantity: Number(data.quantity),
                description: data.description,
                price: Number(data.price)
            })
        });

        setLog('item-modify-log', `Termék sikeresen módosítva, id = ${id}.`);
        form.reset();
    } catch (err) {
        setLog('item-modify-log', `Hiba termék módosítása közben: ${err.message}`, true);
    }
});

/* Termék törlés */
document.getElementById('delete-item-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    if (!validateForm(form)) return;

    const data = Object.fromEntries(new FormData(form));
    const id = Number(data.itemId);

    if (!Number.isInteger(id) || id <= 0) {
        setLog('item-modify-log', 'Az Item ID csak pozitív egész szám lehet.', true);
        return;
    }

    if (!confirm(`Biztosan törlöd az itemet? (id = ${id})`)) {
        return;
    }

    try {
        await apiFetch(`${API_BASE}/items/${id}`, {
            method: 'DELETE'
        });

        setLog('item-modify-log', `Termék sikeresen törölve, id = ${id}.`);
        form.reset();
    } catch (err) {
        setLog('item-modify-log', `Hiba termék törlése közben: ${err.message}`, true);
    }
});

/* Oldal betöltésekor dolgozók betöltése */
window.addEventListener('DOMContentLoaded', () => {
    loadAllWorkers();
});
