const API_BASE = 'https://localhost:7149/api';

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