const API_BASE = 'https://localhost:7149/api';

async function apiFetch(url, options = {}) {
    const response = await fetch(url, {
        headers: {
            'Content-Type': 'application/json',
            ...(options.headers || {})
        },
        credentials: 'include',
        ...options
    });

    if (!response.ok) {
        switch (response.status) {
            case 401:
                throw new Error('Hibás felhasználónév vagy jelszó.');
            case 403:
                throw new Error('Nincs jogosultságod a belépéshez.');
            case 404:
                throw new Error('A felhasználó nem található.');
            case 409:
                throw new Error('Ez a felhasználónév már foglalt.');
            case 500:
                throw new Error('Szerverhiba történt, próbáld újra később.');
            default:
                throw new Error(`Hiba történt (${response.status}).`);
        }
    }

    const contentType = response.headers.get('Content-Type') || '';
    if (contentType.includes('application/json')) {
        return await response.json();
    }
    return await response.text();
}

document.getElementById('login-form').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    const data = Object.fromEntries(new FormData(form));
    const log = document.getElementById('login-log');
    log.textContent = '';

    if (!data.role) {
        log.textContent = 'Válassz szerepkört (admin / worker).';
        return;
    }

    try {
        if (data.role === 'admin') {
            await apiFetch(
                `${API_BASE}/Admins/adminlogin?username=${encodeURIComponent(data.username)}&password=${encodeURIComponent(data.password)}`,
                { method: 'POST' }
            );
        } else if (data.role === 'worker') {
            await apiFetch(
                `${API_BASE}/Workers/workerlogin?username=${encodeURIComponent(data.username)}&password=${encodeURIComponent(data.password)}`,
                { method: 'POST' }
            );
        }

        // Sikeres login → átirányítás az admin felületre
        window.location.href = '../AdminPage/index.html';
    } catch (err) {
        log.textContent = `Hiba bejelentkezés közben: ${err.message}`;
    }
});
