const orderModal       = document.getElementById('order-modal');
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

function formatDate(raw) {
    if (!raw) return '–';
    return new Date(raw).toLocaleString('hu-HU', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit'
    });
}

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
            <td>${formatDate(o.date)}</td>
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
        const orders = await apiFetch(`${API_BASE}/Orders/allorders`);
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
    if (!Number.isInteger(orderId) || orderId <= 0) {
        setLog('orders-log', 'Érvénytelen rendelés ID.', true);
        return;
    }

    try {
        const order = await apiFetch(`${API_BASE}/Orders/orderDetailsByOrderId?orderId=${encodeURIComponent(orderId)}`);

        const hiddenId         = document.getElementById('order-modal-order-id');
        const statusSelect     = document.getElementById('order-modal-status-select');
        const completeBtn      = document.getElementById('order-modal-complete-btn');
        const detailsContainer = document.getElementById('order-modal-details');
        const itemsContainer   = document.getElementById('order-modal-items');
        const updateForm       = document.getElementById('order-modal-update-status-form'); 
        const submitBtn        = updateForm?.querySelector('button[type="submit"]');
        const isTerminal = order.status === 'OrderCompleted' || order.status === 'Cancelled';

        if (hiddenId) hiddenId.value = order.orderId;
        if (statusSelect) { statusSelect.value = order.status || ''; statusSelect.disabled = isTerminal; }
        if (submitBtn)    submitBtn.disabled = isTerminal;
        if (completeBtn)  { completeBtn.disabled = isTerminal; completeBtn.dataset.orderId = order.orderId; }

        if (detailsContainer) {
            detailsContainer.innerHTML = `
                <p><strong>Rendelés ID:</strong> ${order.orderId}</p>
                <p><strong>Dátum:</strong> ${formatDate(order.date)}</p>
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
    ?.addEventListener('click', loadAllOrders);

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
            await apiFetch(`${API_BASE}/Orders/updateorderstatus`, {
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
            await apiFetch(`${API_BASE}/Orders/completeorder?orderId=${encodeURIComponent(orderId)}`, { method: 'PUT' });
            if (logEl) logEl.textContent = `Rendelés lezárva (id = ${orderId}).`;
            await loadOrderDetailsToModal(orderId);
            await loadAllOrders();
        } catch (err) {
            if (logEl) logEl.textContent = `Hiba rendelés lezárása közben: ${err.message}`;
        }
    });