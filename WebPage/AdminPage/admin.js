const API_BASE = 'https://localhost:5001';

const navButtons = document.querySelectorAll('.nav-btn');
const panels = document.querySelectorAll('.panel');

navButtons.forEach(btn => {
    btn.addEventListener('click', () => {
        const target = btn.dataset.view;
        panels.forEach(p => p.classList.remove('active'));
        document.getElementById(target).classList.add('active');
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

/* ===== REGISZTRÁCIÓ ===== */

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
        await apiFetch(`${API_BASE}/api/users/register`, {
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

/* ===== JELSZÓVÁLTÁS ===== */

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
        await apiFetch(`${API_BASE}/api/users/password`, {
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

/* ===== ITEM TÁBLÁZAT RENDER ===== */

function renderItemsTable(items) {
    const container = document.getElementById('items-table-container');
    if (!container) return;

    if (!items || items.length === 0) {
        container.innerHTML = '<p>Nincs egyetlen termék sem.</p>';
        return;
    }

    const headers = ['categoryId', 'itemName', 'quantity', 'description', 'price'];

    let html = '<table><thead><tr>';
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

/* ===== ALLITEMS ===== */

document.getElementById('btn-all-items').addEventListener('click', async () => {
    try {
        const items = await apiFetch(`${API_BASE}/api/items/allitems`, {
            method: 'GET'
        });
        renderItemsTable(items);
        setLog('item-query-log', `AllItems sikeres. Találatok száma: ${items.length}.`);
    } catch (err) {
        setLog('item-query-log', `Hiba AllItems hívás közben: ${err.message}`, true);
    }
});

/* ===== ITEMBYID ===== */

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
        const url = `${API_BASE}/api/items/itembyid?id=${encodeURIComponent(id)}`;
        const item = await apiFetch(url, { method: 'GET' });

        renderItemsTable([item]);
        setLog('item-query-log', `ItemById sikeres, id = ${id}`);
        form.reset();
    } catch (err) {
        setLog('item-query-log', `Hiba ItemById hívás közben: ${err.message}`, true);
    }
});

/* ===== ADDNEWITEM ===== */

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
        await apiFetch(`${API_BASE}/api/items`, {
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

/* ===== MODIFYITEM ===== */

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
        await apiFetch(`${API_BASE}/api/items/${id}`, {
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

/* ===== DELETEITEM ===== */

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
        await apiFetch(`${API_BASE}/api/items/${id}`, {
            method: 'DELETE'
        });

        setLog('item-modify-log', `Termék sikeresen törölve, id = ${id}.`);
        form.reset();
    } catch (err) {
        setLog('item-modify-log', `Hiba termék törlése közben: ${err.message}`, true);
    }
});
