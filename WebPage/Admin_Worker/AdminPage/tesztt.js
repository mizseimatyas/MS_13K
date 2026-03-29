const API_BASE = 'https://localhost:7149/api';
const ORDERS_BASE = `${API_BASE}/Orders`;

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

async function apiFetch(url, options = {}) {
    const response = await fetch(url, {
        headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
        credentials: 'include',
        ...options
    });

    if (!response.ok) {
        const text = await response.text();
        throw new Error(text || `HTTP hiba: ${response.status}`);
    }

    const contentType = response.headers.get('Content-Type') || '';
    if (contentType.includes('application/json')) return await response.json();
    return await response.text();
}

async function checkAuth() {
    try {
        const me = await apiFetch(`${API_BASE}/Admins/me`, { method: 'GET' });
        currentRole = me.role;
        applyRoleUi();
        if (currentRole === 'Admin') loadAllWorkers();
    } catch {
        window.location.href = '/WebPage/Admin_Worker/Login/login.html';
    }
}

function applyRoleUi() {
    const userTabBtn = document.querySelector('.nav-btn[data-view="user-panel"]');
    const userPanel = document.getElementById('user-panel');
    const itemPanel = document.getElementById('item-panel');
    const orderTabBtn = document.querySelector('.nav-btn[data-view="order-panel"]');
    const orderPanel = document.getElementById('order-panel');

    if (!currentRole) return;

    if (currentRole === 'Worker') {
        if (userTabBtn) userTabBtn.style.display = 'none';
        if (userPanel) userPanel.style.display = 'none';
        if (orderTabBtn) orderTabBtn.style.display = '';
        if (orderPanel) orderPanel.style.display = '';
        if (itemPanel) {
            panels.forEach(p => p.classList.remove('active'));
            itemPanel.classList.add('active');
        }
    } else if (currentRole === 'Admin') {
        if (userTabBtn) userTabBtn.style.display = '';
        if (userPanel) userPanel.style.display = '';
        if (orderPanel) orderPanel.style.display = '';
    }
}


//#region Admin

document.getElementById('register-form')
    ?.addEventListener('submit', async e => {
        e.preventDefault();
        const form = e.target;
        if (!validateForm(form)) return;

        const data = Object.fromEntries(new FormData(form));

        if (data.phone.length < 8) {
            setLog('register-log', 'A telefonszámnak legalább 8 karakter hosszúnak kell lennie.', true);
            return;
        }

        try {
            if (data.role === 'admin') {
                await apiFetch(
                    `${API_BASE}/Admins/adminregistry?username=${encodeURIComponent(data.username)}&password=${encodeURIComponent(data.password)}&phone=${encodeURIComponent(data.phone)}`,
                    { method: 'POST' }
                );
            } else {
                await apiFetch(
                    `${API_BASE}/Workers/workerregistry?username=${encodeURIComponent(data.username)}&password=${encodeURIComponent(data.password)}&phone=${encodeURIComponent(data.phone)}`,
                    { method: 'POST' }
                );
            }
            setLog('register-log', 'Sikeres regisztráció.');
            form.reset();
            if (currentRole === 'Admin') loadAllWorkers();
        } catch (err) {
            setLog('register-log', `Hiba regisztráció közben: ${err.message}`, true);
        }
    });

function renderWorkersTable(workers) {
    const container = document.getElementById('worker-table-container');
    if (!container) return;

    if (!workers || workers.length === 0) {
        container.innerHTML = '<p>Nincsenek dolgozók.</p>';
        return;
    }

    let html = '<table class="data-table"><thead><tr>';
    html += '<th>WorkerId</th><th>WorkerName</th><th>Role</th><th>Phone</th><th>Password</th><th></th>';
    html += '</tr></thead><tbody>';

    workers.forEach(work => {
        html += `<tr data-id="${work.workerId}">
            <td class="cell-id">${work.workerId}</td>
            <td class="cell-name">${work.workerName}</td>
            <td class="cell-role">${work.role}</td>
            <td class="cell-phone">${work.phone ?? ''}</td>
            <td class="cell-password">********</td>
            <td class="cell-actions">
                <button class="secondary-btn btn-edit-worker">Módosítás</button>
                <button class="secondary-btn btn-cancel-edit" style="display:none;">Vissza</button>
                <button class="danger-btn btn-delete-worker">Törlés</button>
            </td>
        </tr>`;
    });

    html += '</tbody></table>';
    container.innerHTML = html;

    container.querySelectorAll('.btn-edit-worker').forEach(btn => btn.addEventListener('click', onEditWorkerClick));
    container.querySelectorAll('.btn-cancel-edit').forEach(btn => btn.addEventListener('click', onCancelEditClick));
    container.querySelectorAll('.btn-delete-worker').forEach(btn => btn.addEventListener('click', onDeleteWorkerClick));
}

function onEditWorkerClick(e) {
    const btn = e.currentTarget;
    const row = btn.closest('tr');
    const cancelBtn = row.querySelector('.btn-cancel-edit');

    if (row.dataset.editing !== 'true') {
        row.dataset.editing = 'true';
        btn.textContent = 'Mentés';
        cancelBtn.style.display = 'inline-block';

        const nameCell = row.querySelector('.cell-name');
        const phoneCell = row.querySelector('.cell-phone');
        const passwordCell = row.querySelector('.cell-password');

        row.dataset.originalName = nameCell.textContent.trim();
        row.dataset.originalPhone = phoneCell.textContent.trim();

        nameCell.innerHTML = `<input type="text" class="input-name" value="${row.dataset.originalName}">`;
        phoneCell.innerHTML = `<input type="number" class="input-phone" value="${row.dataset.originalPhone}">`;
        passwordCell.innerHTML = `
            <div class="password-edit">
                <input type="password" class="input-password" placeholder="Új jelszó (opcionális)">
                <button type="button" class="secondary-btn btn-toggle-password">👁</button>
            </div>
            <div class="password-edit">
                <input type="password" class="input-password-confirm" placeholder="Új jelszó ismét">
                <button type="button" class="secondary-btn btn-toggle-password-confirm">👁</button>
            </div>`;

        passwordCell.querySelector('.btn-toggle-password').addEventListener('click', () => {
            const inp = passwordCell.querySelector('.input-password');
            inp.type = inp.type === 'password' ? 'text' : 'password';
        });
        passwordCell.querySelector('.btn-toggle-password-confirm').addEventListener('click', () => {
            const inp = passwordCell.querySelector('.input-password-confirm');
            inp.type = inp.type === 'password' ? 'text' : 'password';
        });
    } else {
        saveWorkerRow(row, btn);
    }
}

function onCancelEditClick(e) {
    const row = e.currentTarget.closest('tr');
    const editBtn = row.querySelector('.btn-edit-worker');

    row.querySelector('.cell-name').textContent = row.dataset.originalName ?? '';
    row.querySelector('.cell-phone').textContent = row.dataset.originalPhone ?? '';
    row.querySelector('.cell-password').textContent = '********';

    row.dataset.editing = 'false';
    delete row.dataset.originalName;
    delete row.dataset.originalPhone;

    editBtn.textContent = 'Módosítás';
    e.currentTarget.style.display = 'none';
}

async function saveWorkerRow(row, btn) {
    const id = Number(row.dataset.id);
    const workerName = row.querySelector('.input-name').value.trim();
    const phoneVal = row.querySelector('.input-phone').value;
    const phone = phoneVal ? Number(phoneVal) : null;
    const newPassword = row.querySelector('.input-password')?.value || '';
    const newPasswordConfirm = row.querySelector('.input-password-confirm')?.value || '';
    const roleName = row.querySelector('.cell-role').textContent.trim();

    if (!workerName) { alert('A név nem lehet üres.'); return; }

    if (newPassword || newPasswordConfirm) {
        if (newPassword.length < 6) { alert('Az új jelszónak legalább 6 karakter hosszúnak kell lennie.'); return; }
        if (newPassword !== newPasswordConfirm) { alert('Az új jelszó és a megerősítés nem egyeznek.'); return; }
    }

    try {
        if (newPassword) {
            await apiFetch(`${API_BASE}/Workers/changepassword?workerId=${encodeURIComponent(id)}&newPassword=${encodeURIComponent(newPassword)}`, { method: 'PUT' });
        }

        await apiFetch(`${API_BASE}/Workers/changedata/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ workerName, role: roleName, phone })
        });

        row.dataset.editing = 'false';
        btn.textContent = 'Módosítás';
        const cancelBtn = row.querySelector('.btn-cancel-edit');
        if (cancelBtn) cancelBtn.style.display = 'none';
        delete row.dataset.originalName;
        delete row.dataset.originalPhone;

        row.querySelector('.cell-name').textContent = workerName;
        row.querySelector('.cell-phone').textContent = phone ?? '';
        row.querySelector('.cell-password').textContent = '********';

        setLog('worker-query-log', `Dolgozó módosítva (id = ${id}).`);
    } catch (err) {
        setLog('worker-query-log', `Hiba dolgozó módosítás közben: ${err.message}`, true);
    }
}

async function onDeleteWorkerClick(e) {
    const row = e.currentTarget.closest('tr');
    const id = Number(row.dataset.id);
    const name = row.querySelector('.cell-name').textContent.trim();

    if (!confirm(`Biztosan törlöd ezt a dolgozót? ${name} (id: ${id})`)) return;

    try {
        await apiFetch(`${API_BASE}/Workers/${id}`, { method: 'DELETE' });
        row.remove();
        setLog('worker-query-log', `Dolgozó törölve (id = ${id}).`);
    } catch (err) {
        setLog('worker-query-log', `Hiba dolgozó törlése közben: ${err.message}`, true);
    }
}

async function loadAllWorkers() {
    if (currentRole !== 'Admin') return;
    try {
        const workers = await apiFetch(`${API_BASE}/Admins/allworkers`, { method: 'GET' });
        renderWorkersTable(workers);
        setLog('worker-query-log', `Dolgozók száma: ${workers.length}`);
    } catch (err) {
        setLog('worker-query-log', `Hiba AllWorkers hívás közben: ${err.message}`, true);
    }
}

//#endregion


//#region Termékek

let currentSort = { key: 'itemId', dir: 'asc' };
let currentItems = [];

function renderItemsTable(items) {
    currentItems = items.slice();
    const container = document.getElementById('items-table-container');
    if (!container) return;

    if (!items || items.length === 0) {
        container.innerHTML = '<p>Nincs egyetlen termék sem.</p>';
        return;
    }

    const headers = [
        { label: 'ItemId',    key: 'itemId' },
        { label: 'Kategória', key: 'categoryName' },
        { label: 'Terméknév', key: 'itemName' },
        { label: 'Mennyiség', key: 'quantity' },
        { label: 'Leírás',    key: null },
        { label: 'Ár',        key: 'price' }
    ];

    let html = '<table class="data-table"><thead><tr>';
    headers.forEach(h => {
        if (h.key) {
            const arrow = currentSort.key === h.key ? (currentSort.dir === 'asc' ? ' ▲' : ' ▼') : '';
            html += `<th class="sortable" data-sort-key="${h.key}">${h.label}${arrow}</th>`;
        } else {
            html += `<th>${h.label}</th>`;
        }
    });
    html += '<th></th></tr></thead><tbody>';

    items.forEach(item => {
        html += `<tr data-item-id="${item.itemId}">
            <td class="cell-itemid">${item.itemId}</td>
            <td class="cell-categoryname">${item.categoryName}</td>
            <td class="cell-name">${item.itemName}</td>
            <td class="cell-quantity">${item.quantity}</td>
            <td class="cell-description">${item.description}</td>
            <td class="cell-price">${item.price}</td>
            <td class="cell-actions">
                <button class="secondary-btn btn-edit-item">Módosítás</button>
                <button class="secondary-btn btn-cancel-edit-item" style="display:none;">Vissza</button>
                <button class="danger-btn btn-delete-item-row">Törlés</button>
            </td>
        </tr>`;
    });

    html += '</tbody></table>';
    container.innerHTML = html;

    container.querySelectorAll('th.sortable').forEach(th => {
        th.addEventListener('click', () => {
            const key = th.getAttribute('data-sort-key');
            if (currentSort.key === key) {
                currentSort.dir = currentSort.dir === 'asc' ? 'desc' : 'asc';
            } else {
                currentSort.key = key;
                currentSort.dir = 'asc';
            }
            renderItemsTable(currentItems.slice().sort((a, b) => compareByKey(a, b, currentSort.key, currentSort.dir)));
        });
    });

    container.querySelectorAll('.btn-edit-item').forEach(btn => btn.addEventListener('click', onEditItemClick));
    container.querySelectorAll('.btn-cancel-edit-item').forEach(btn => btn.addEventListener('click', onCancelEditItemClick));
    container.querySelectorAll('.btn-delete-item-row').forEach(btn => btn.addEventListener('click', onDeleteItemRowClick));
}

function compareByKey(a, b, key, dir) {
    const va = a[key];
    const vb = b[key];
    let res = (typeof va === 'number' && typeof vb === 'number')
        ? va - vb
        : String(va ?? '').localeCompare(String(vb ?? ''), 'hu');
    return dir === 'asc' ? res : -res;
}

function onEditItemClick(e) {
    const btn = e.currentTarget;
    const row = btn.closest('tr');
    const cancelBtn = row.querySelector('.btn-cancel-edit-item');

    if (row.dataset.editing !== 'true') {
        row.dataset.editing = 'true';
        btn.textContent = 'Mentés';
        cancelBtn.style.display = 'inline-block';

        const nameCell        = row.querySelector('.cell-name');
        const categoryCell    = row.querySelector('.cell-categoryname');
        const quantityCell    = row.querySelector('.cell-quantity');
        const descriptionCell = row.querySelector('.cell-description');
        const priceCell       = row.querySelector('.cell-price');

        row.dataset.originalName        = nameCell.textContent.trim();
        row.dataset.originalCategory    = categoryCell.textContent.trim();
        row.dataset.originalQuantity    = quantityCell.textContent.trim();
        row.dataset.originalDescription = descriptionCell.textContent.trim();
        row.dataset.originalPrice       = priceCell.textContent.trim();

        nameCell.innerHTML        = `<input type="text" class="input-item-name" value="${row.dataset.originalName}">`;
        categoryCell.innerHTML    = `<input type="text" class="input-item-category" value="${row.dataset.originalCategory}">`;
        quantityCell.innerHTML    = `<input type="number" class="input-item-quantity" min="1" value="${row.dataset.originalQuantity}">`;
        descriptionCell.innerHTML = `<textarea class="input-item-description">${row.dataset.originalDescription}</textarea>`;
        priceCell.innerHTML       = `<input type="number" class="input-item-price" min="1" value="${row.dataset.originalPrice}">`;
    } else {
        saveItemRow(row, btn);
    }
}

function onCancelEditItemClick(e) {
    const row = e.currentTarget.closest('tr');
    const editBtn = row.querySelector('.btn-edit-item');

    row.querySelector('.cell-name').textContent        = row.dataset.originalName ?? '';
    row.querySelector('.cell-categoryname').textContent = row.dataset.originalCategory ?? '';
    row.querySelector('.cell-quantity').textContent    = row.dataset.originalQuantity ?? '';
    row.querySelector('.cell-description').textContent = row.dataset.originalDescription ?? '';
    row.querySelector('.cell-price').textContent       = row.dataset.originalPrice ?? '';

    row.dataset.editing = 'false';
    delete row.dataset.originalName;
    delete row.dataset.originalCategory;
    delete row.dataset.originalQuantity;
    delete row.dataset.originalDescription;
    delete row.dataset.originalPrice;

    editBtn.textContent = 'Módosítás';
    e.currentTarget.style.display = 'none';
}

async function saveItemRow(row, btn) {
    const id          = Number(row.getAttribute('data-item-id'));
    const name        = row.querySelector('.input-item-name').value.trim();
    const category    = row.querySelector('.input-item-category').value.trim();
    const quantity    = Number(row.querySelector('.input-item-quantity').value);
    const description = row.querySelector('.input-item-description').value.trim();
    const price       = Number(row.querySelector('.input-item-price').value);

    if (!name) { alert('A név nem lehet üres.'); return; }
    if (!Number.isInteger(quantity) || quantity <= 0) { alert('A mennyiségnek pozitív egész számnak kell lennie.'); return; }
    if (!Number.isInteger(price)    || price <= 0)    { alert('Az árnak pozitív egész számnak kell lennie.'); return; }

    try {
        await apiFetch(`${API_BASE}/items/modifyitem`, {
            method: 'PUT',
            body: JSON.stringify({ itemId: id, categoryName: category, itemName: name, quantity, description, price })
        });

        row.dataset.editing = 'false';
        btn.textContent = 'Módosítás';
        const cancelBtn = row.querySelector('.btn-cancel-edit-item');
        if (cancelBtn) cancelBtn.style.display = 'none';

        row.querySelector('.cell-name').textContent         = name;
        row.querySelector('.cell-categoryname').textContent = category;
        row.querySelector('.cell-quantity').textContent     = quantity;
        row.querySelector('.cell-description').textContent  = description;
        row.querySelector('.cell-price').textContent        = price;

        setLog('item-modify-log', `Termék sikeresen módosítva, id = ${id}.`);
    } catch (err) {
        setLog('item-modify-log', `Hiba termék módosítása közben: ${err.message}`, true);
    }
}

async function onDeleteItemRowClick(e) {
    const row  = e.currentTarget.closest('tr');
    const id   = Number(row.getAttribute('data-item-id'));
    const name = row.querySelector('.cell-name').textContent.trim();

    if (!confirm(`Biztosan törlöd ezt a terméket? ${name} (id: ${id})`)) return;

    try {
        await apiFetch(`${API_BASE}/items/deleteitem?id=${encodeURIComponent(id)}`, { method: 'DELETE' });
        row.remove();
        setLog('item-modify-log', `Termék sikeresen törölve, id = ${id}.`);
    } catch (err) {
        setLog('item-modify-log', `Hiba termék törlése közben: ${err.message}`, true);
    }
}

async function loadInitialItems() {
    try {
        const items = await apiFetch(`${API_BASE}/items/allitems`, { method: 'GET' });
        renderItemsTable(items);
        setLog('item-query-log', `AllItems automatikus betöltés. Találatok: ${items.length}.`);
    } catch (err) {
        setLog('item-query-log', `Hiba AllItems automatikus betöltés közben: ${err.message}`, true);
    }
}

document.getElementById('item-by-id-form')
    ?.addEventListener('submit', async e => {
        e.preventDefault();
        const form = e.target;
        if (!validateForm(form)) return;

        const id = Number(Object.fromEntries(new FormData(form)).itemId);
        if (!Number.isInteger(id) || id <= 0) {
            setLog('item-query-log', 'Az Item ID csak pozitív egész szám lehet.', true);
            return;
        }

        try {
            const item  = await apiFetch(`${API_BASE}/items/itembyid?id=${encodeURIComponent(id)}`, { method: 'GET' });
            const table = document.getElementById('items-table-container').querySelector('table');

            if (!table) {
                renderItemsTable([item]);
            } else {
                table.querySelectorAll('tbody tr').forEach(tr => {
                    tr.classList.remove('item-highlight');
                    if (Number(tr.getAttribute('data-item-id')) === item.itemId) tr.classList.add('item-highlight');
                });
            }

            setLog('item-query-log', `ItemById sikeres, id = ${id}`);
            form.reset();
        } catch (err) {
            setLog('item-query-log', `Hiba ItemById hívás közben: ${err.message}`, true);
        }
    });

document.getElementById('item-by-name-form')
    ?.addEventListener('submit', async e => {
        e.preventDefault();
        const form = e.target;
        if (!validateForm(form)) return;

        const frag = (Object.fromEntries(new FormData(form)).fragname || '').trim();
        if (!frag) { setLog('item-query-log', 'A névrészlet nem lehet üres.', true); return; }

        try {
            const items = await apiFetch(`${API_BASE}/items/admitembyname?iname=${encodeURIComponent(frag)}`, { method: 'GET' });
            renderItemsTable(items);
            setLog('item-query-log', `AdmItemByName sikeres, találatok: ${items.length}.`);
            form.reset();
        } catch (err) {
            setLog('item-query-log', `Hiba AdmItemByName közben: ${err.message}`, true);
        }
    });

document.getElementById('reset-items-btn')
    ?.addEventListener('click', async () => {
        await loadInitialItems();
        setLog('item-query-log', 'Táblázat visszaállítva teljes listára.');
    });

document.getElementById('add-item-form')
    ?.addEventListener('submit', async e => {
        e.preventDefault();
        const form = e.target;
        if (!validateForm(form)) return;

        const data = Object.fromEntries(new FormData(form));

        if (Number(data.quantity) <= 0) { setLog('item-modify-log', 'A mennyiségnek pozitívnak kell lennie.', true); return; }
        if (Number(data.price) <= 0)    { setLog('item-modify-log', 'Az árnak pozitívnak kell lennie.', true); return; }

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
            loadInitialItems();
        } catch (err) {
            setLog('item-modify-log', `Hiba új termék hozzáadása közben: ${err.message}`, true);
        }
    });

//#endregion


//#region Orders

const orderModal      = document.getElementById('order-modal');
const orderModalCloseX = document.getElementById('order-modal-close-x');

function openOrderModal()  { if (orderModal) orderModal.style.display = 'flex'; }
function closeOrderModal() {
    if (!orderModal) return;
    orderModal.style.display = 'none';
    const log = document.getElementById('order-modal-log');
    if (log) log.textContent = '';
}

orderModalCloseX?.addEventListener('click', () => closeOrderModal());
orderModal?.addEventListener('click', e => { if (e.target === orderModal) closeOrderModal(); });

function renderOrdersTable(orders) {
    const container = document.getElementById('orders-table-container');
    if (!container) return;

    if (!orders || orders.length === 0) {
        container.innerHTML = '<p>Nincs egyetlen megrendelés sem.</p>';
        return;
    }

    let html = `<table class="data-table"><thead><tr>
        <th>OrderId</th><th>Dátum</th><th>Státusz</th><th>Végösszeg</th><th>Cím</th><th>Műveletek</th>
    </tr></thead><tbody>`;

    orders.forEach(o => {
        html += `<tr data-order-id="${o.orderId}">
            <td>${o.orderId}</td>
            <td>${o.date}</td>
            <td>${o.status}</td>
            <td>${o.totalPrice}</td>
            <td>${o.targetAddress}</td>
            <td><button class="secondary-btn btn-order-details">Részletek</button></td>
        </tr>`;
    });

    html += '</tbody></table>';
    container.innerHTML = html;
    container.querySelectorAll('.btn-order-details').forEach(btn => btn.addEventListener('click', onOrderDetailsClick));
}

async function loadAllOrders() {
    try {
        const orders = await apiFetch(`${ORDERS_BASE}/allorders`, { method: 'GET' });
        renderOrdersTable(orders);
        setLog('orders-log', `Összes megrendelés betöltve. Darabszám: ${orders.length}.`);
    } catch (err) {
        setLog('orders-log', `Hiba AllOrders hívás közben: ${err.message}`, true);
    }
}

async function onOrderDetailsClick(e) {
    const orderId = Number(e.currentTarget.closest('tr').dataset.orderId);
    if (orderId) await loadOrderDetailsToModal(orderId);
}

async function loadOrderDetailsToModal(orderId) {
    if (!Number.isInteger(Number(orderId)) || orderId <= 0) {
        setLog('orders-log', 'Érvénytelen rendelés ID.', true);
        return;
    }

    try {
        const order = await apiFetch(`${ORDERS_BASE}/orderDetailsByOrderId?orderId=${encodeURIComponent(orderId)}`, { method: 'GET' });

        const hiddenId    = document.getElementById('order-modal-order-id');
        const statusSelect = document.getElementById('order-modal-status-select');
        const completeBtn = document.getElementById('order-modal-complete-btn');
        const detailsContainer = document.getElementById('order-modal-details');
        const itemsContainer   = document.getElementById('order-modal-items');

        if (hiddenId) hiddenId.value = order.orderId;
        if (statusSelect) statusSelect.value = order.status || '';
        if (completeBtn) { completeBtn.disabled = false; completeBtn.dataset.orderId = order.orderId; }

        if (detailsContainer) {
            detailsContainer.innerHTML = `
                <p><strong>Rendelés ID:</strong> ${order.orderId}</p>
                <p><strong>Dátum:</strong> ${order.date}</p>
                <p><strong>Státusz:</strong> ${order.status}</p>
                <p><strong>Célcím:</strong> ${order.targetAddress}</p>
                <p><strong>Végösszeg:</strong> ${order.totalPrice}</p>`;
        }

        if (itemsContainer) {
            if (!order.items || order.items.length === 0) {
                itemsContainer.innerHTML = '<p>Nincsenek tételek ehhez a rendeléshez.</p>';
            } else {
                let html = `<table class="data-table"><thead><tr>
                    <th>ItemId</th><th>Név</th><th>Mennyiség</th><th>Egységár</th>
                </tr></thead><tbody>`;
                order.items.forEach(it => {
                    html += `<tr><td>${it.itemId}</td><td>${it.itemName}</td><td>${it.quantity}</td><td>${it.price}</td></tr>`;
                });
                html += '</tbody></table>';
                itemsContainer.innerHTML = html;
            }
        }

        openOrderModal();
    } catch (err) {
        setLog('orders-log', `Hiba rendelés részleteinek lekérése közben: ${err.message}`, true);
    }
}

document.getElementById('load-all-orders-btn')
    ?.addEventListener('click', async () => await loadAllOrders());

document.getElementById('order-modal-update-status-form')
    ?.addEventListener('submit', async e => {
        e.preventDefault();
        if (!validateForm(e.target)) return;

        const orderId   = Number(document.getElementById('order-modal-order-id')?.value || 0);
        const newStatus = document.getElementById('order-modal-status-select')?.value;
        const logEl     = document.getElementById('order-modal-log');

        if (!orderId || orderId <= 0) { if (logEl) logEl.textContent = 'Érvénytelen rendelés ID.'; return; }
        if (!newStatus)               { if (logEl) logEl.textContent = 'Válassz új státuszt.'; return; }

        try {
            await apiFetch(`${ORDERS_BASE}/updateorderstatus`, {
                method: 'PUT',
                body: JSON.stringify({ orderId, orderStatus: newStatus })
            });
            if (logEl) logEl.textContent = `Státusz frissítve (id = ${orderId}, új: ${newStatus}).`;
            await loadOrderDetailsToModal(orderId);
            await loadAllOrders();
        } catch (err) {
            if (logEl) logEl.textContent = `Hiba státusz frissítése közben: ${err.message}`;
        }
    });

document.getElementById('order-modal-complete-btn')
    ?.addEventListener('click', async e => {
        const orderId = Number(e.currentTarget.dataset.orderId || 0);
        const logEl   = document.getElementById('order-modal-log');

        if (!orderId || orderId <= 0) { if (logEl) logEl.textContent = 'Érvénytelen rendelés ID.'; return; }
        if (!confirm(`Biztosan lezárod ezt a rendelést? (id: ${orderId})`)) return;

        try {
            await apiFetch(`${ORDERS_BASE}/completeorder?orderId=${encodeURIComponent(orderId)}`, { method: 'PUT' });
            if (logEl) logEl.textContent = `Rendelés lezárva (id = ${orderId}).`;
            await loadOrderDetailsToModal(orderId);
            await loadAllOrders();
        } catch (err) {
            if (logEl) logEl.textContent = `Hiba rendelés lezárása közben: ${err.message}`;
        }
    });

//#endregion


//#region Kategóriák

async function loadAllCategories() {
    const container = document.getElementById('category-list-container');
    try {
        const categories = await apiFetch(`${API_BASE}/Categories/allcategories`);
        renderCategoryList(categories);
        setLog('add-category-log', `Kategóriák betöltve. Darabszám: ${categories.length}.`);
    } catch (err) {
        if (err.message.includes('404') || err.message.includes('Not Found')) {
            renderCategoryList([]);
        } else {
            if (container) container.innerHTML = '<p style="color:#b91c1c;font-size:0.85rem;">Nem sikerült betölteni a kategóriákat.</p>';
            setLog('add-category-log', `Hiba kategóriák betöltése közben: ${err.message}`, true);
        }
    }
}

function renderCategoryList(categories) {
    const container = document.getElementById('category-list-container');
    if (!container) return;

    if (!categories || categories.length === 0) {
        container.innerHTML = '<p style="font-size:0.85rem;color:#6b7280;">Nincs egyetlen kategória sem.</p>';
        return;
    }

    let html = `<table class="data-table"><thead><tr>
        <th>ID</th><th>Kategória neve</th><th>Termékek száma</th><th></th>
    </tr></thead><tbody>`;
    categories.forEach(cat => {
        html += `<tr data-cat-id="${cat.categoryId}">
            <td class="cell-cat-id">${cat.categoryId}</td>
            <td class="cell-cat-name">${cat.categoryName}</td>
            <td class="cell-cat-count">${cat.itemCount}</td>
            <td class="cell-actions">
                <button class="secondary-btn btn-edit-category">Módosítás</button>
                <button class="secondary-btn btn-cancel-edit-category" style="display:none;">Vissza</button>
                <button class="danger-btn btn-delete-category">Törlés</button>
            </td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;

    container.querySelectorAll('.btn-edit-category').forEach(btn => btn.addEventListener('click', onEditCategoryClick));
    container.querySelectorAll('.btn-cancel-edit-category').forEach(btn => btn.addEventListener('click', onCancelEditCategoryClick));
    container.querySelectorAll('.btn-delete-category').forEach(btn => btn.addEventListener('click', onDeleteCategoryClick));
}

function onEditCategoryClick(e) {
    const btn = e.currentTarget;
    const row = btn.closest('tr');
    const cancelBtn = row.querySelector('.btn-cancel-edit-category');

    if (row.dataset.editing !== 'true') {
        row.dataset.editing = 'true';
        btn.textContent = 'Mentés';
        cancelBtn.style.display = 'inline-block';

        const nameCell = row.querySelector('.cell-cat-name');
        row.dataset.originalName = nameCell.textContent.trim();
        nameCell.innerHTML = `<input type="text" class="input-cat-name" value="${row.dataset.originalName}">`;
    } else {
        saveCategoryRow(row, btn);
    }
}

function onCancelEditCategoryClick(e) {
    const row = e.currentTarget.closest('tr');
    const editBtn = row.querySelector('.btn-edit-category');

    row.querySelector('.cell-cat-name').textContent = row.dataset.originalName ?? '';
    row.dataset.editing = 'false';
    delete row.dataset.originalName;

    editBtn.textContent = 'Módosítás';
    e.currentTarget.style.display = 'none';
}

async function saveCategoryRow(row, btn) {
    const id   = Number(row.dataset.catId);
    const name = row.querySelector('.input-cat-name').value.trim();

    if (!name) { alert('A kategória neve nem lehet üres.'); return; }

    try {
        await apiFetch(`${API_BASE}/Categories/modifycategory`, {
            method: 'PUT',
            body: JSON.stringify({ categId: id, categName: name })
        });

        row.dataset.editing = 'false';
        btn.textContent = 'Módosítás';
        const cancelBtn = row.querySelector('.btn-cancel-edit-category');
        if (cancelBtn) cancelBtn.style.display = 'none';
        delete row.dataset.originalName;

        row.querySelector('.cell-cat-name').textContent = name;
        setLog('add-category-log', `Kategória módosítva (id = ${id}).`);
    } catch (err) {
        if (err.message.includes('409') || err.message.includes('Conflict')) {
            setLog('add-category-log', `Már létezik ilyen nevű kategória.`, true);
        } else {
            setLog('add-category-log', `Hiba kategória módosítása közben: ${err.message}`, true);
        }
    }
}

async function onDeleteCategoryClick(e) {
    const row  = e.currentTarget.closest('tr');
    const id   = Number(row.dataset.catId);
    const name = row.querySelector('.cell-cat-name').textContent.trim();

    if (!confirm(`Biztosan törlöd ezt a kategóriát? ${name} (id: ${id})`)) return;

    try {
        await apiFetch(`${API_BASE}/Categories/deletecategory?categid=${encodeURIComponent(id)}`, { method: 'DELETE' });
        row.remove();
        setLog('add-category-log', `Kategória törölve (id = ${id}).`);
    } catch (err) {
        setLog('add-category-log', `Hiba kategória törlése közben: ${err.message}`, true);
    }
}

document.getElementById('add-category-form')
    ?.addEventListener('submit', async e => {
        e.preventDefault();
        const form = e.target;
        if (!validateForm(form)) return;

        const name = form.elements['categoryName'].value.trim();

        try {
            await apiFetch(`${API_BASE}/Categories/addnewcategory?categ=${encodeURIComponent(name)}`, { method: 'POST' });
            setLog('add-category-log', `„${name}" kategória sikeresen létrehozva.`);
            form.reset();
            loadAllCategories();
        } catch (err) {
            if (err.message.includes('409') || err.message.includes('Conflict')) {
                setLog('add-category-log', `Már létezik „${name}" nevű kategória.`, true);
            } else {
                setLog('add-category-log', `Hiba: ${err.message}`, true);
            }
        }
    });

document.getElementById('load-categories-btn')
    ?.addEventListener('click', loadAllCategories);

document.querySelector('.nav-btn[data-view="category-panel"]')
    ?.addEventListener('click', loadAllCategories);

//#endregion


window.addEventListener('DOMContentLoaded', () => {
    checkAuth();
    loadInitialItems();
    loadAllOrders();
    loadAllCategories();
});