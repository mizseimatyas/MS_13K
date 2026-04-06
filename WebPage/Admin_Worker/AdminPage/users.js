function renderUsersTable(users) {
    const container = document.getElementById('user-table-container');
    if (!container) return;

    if (!users || users.length === 0) {
        container.innerHTML = '<p>Nincsenek felhasználók.</p>';
        return;
    }

    let html = `<table class="data-table"><thead><tr>
        <th>UserId</th><th>Név</th><th>Email</th><th>Telefon</th><th>Város</th><th>Irányítószám</th><th>Cím</th>
    </tr></thead><tbody>`;

    users.forEach(u => {
        html += `<tr>
            <td>${u.userid}</td>
            <td>${u.name      ?? '–'}</td>
            <td>${u.email     ?? '–'}</td>
            <td>${u.phone     ?? '–'}</td>
            <td>${u.city      ?? '–'}</td>
            <td>${u.zipCode   ?? '–'}</td>
            <td>${u.address   ?? '–'}</td>
        </tr>`;
    });

    html += '</tbody></table>';
    container.innerHTML = html;
}

async function loadAllUsers() {
    try {
        const users = await apiFetch(`${API_BASE}/Admins/allusers`);
        renderUsersTable(users);
        setLog('user-query-log', `Felhasználók száma: ${users.length}`);
    } catch (err) {
        setLog('user-query-log', `Hiba AllUsers hívás közben: ${err.message}`, true);
    }
}