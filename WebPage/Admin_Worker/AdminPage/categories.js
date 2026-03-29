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