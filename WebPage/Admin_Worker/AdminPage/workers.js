function renderWorkersTable(workers) {
    const container = document.getElementById('worker-table-container');
    if (!container) return;

    if (!workers || workers.length === 0) {
        container.innerHTML = '<p>Nincsenek dolgozók.</p>';
        return;
    }

    let html = `<table class="data-table"><thead><tr>
        <th>WorkerId</th><th>WorkerName</th><th>Role</th><th>Phone</th><th>Password</th><th></th>
    </tr></thead><tbody>`;

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
        const workers = await apiFetch(`${API_BASE}/Admins/allworkers`);
        renderWorkersTable(workers);
        setLog('worker-query-log', `Dolgozók száma: ${workers.length}`);
    } catch (err) {
        setLog('worker-query-log', `Hiba AllWorkers hívás közben: ${err.message}`, true);
    }
}

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
                    `${API_BASE}/Admins/adminregistry?username=${encodeURIComponent(data.username)}&password=${encodeURIComponent(data.password)}`,
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