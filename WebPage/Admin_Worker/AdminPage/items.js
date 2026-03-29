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

    row.querySelector('.cell-name').textContent         = row.dataset.originalName ?? '';
    row.querySelector('.cell-categoryname').textContent = row.dataset.originalCategory ?? '';
    row.querySelector('.cell-quantity').textContent     = row.dataset.originalQuantity ?? '';
    row.querySelector('.cell-description').textContent  = row.dataset.originalDescription ?? '';
    row.querySelector('.cell-price').textContent        = row.dataset.originalPrice ?? '';

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
    const id          = Number(row.dataset.itemId);
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
    const id   = Number(row.dataset.itemId);
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
        const items = await apiFetch(`${API_BASE}/items/allitems`);
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
            const item  = await apiFetch(`${API_BASE}/items/itembyid?id=${encodeURIComponent(id)}`);
            const table = document.getElementById('items-table-container').querySelector('table');

            if (!table) {
                renderItemsTable([item]);
            } else {
                table.querySelectorAll('tbody tr').forEach(tr => {
                    tr.classList.remove('item-highlight');
                    if (Number(tr.dataset.itemId) === item.itemId) tr.classList.add('item-highlight');
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
            const items = await apiFetch(`${API_BASE}/items/admitembyname?iname=${encodeURIComponent(frag)}`);
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
                    itemName:     data.itemName,
                    categoryName: data.categoryName,
                    quantity:     Number(data.quantity),
                    description:  data.description,
                    price:        Number(data.price)
                })
            });
            setLog('item-modify-log', 'Új termék sikeresen hozzáadva.');
            form.reset();
            loadInitialItems();
        } catch (err) {
            setLog('item-modify-log', `Hiba új termék hozzáadása közben: ${err.message}`, true);
        }
    });