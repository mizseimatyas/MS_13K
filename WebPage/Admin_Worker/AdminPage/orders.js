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

const STATUS_TRANSITIONS = {
    'PaymentSuccess': ['Delivering'],
    'Delivering':     ['OrderCompleted']
};

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
        showToast('Érvénytelen rendelés ID.', true);
        return;
    }

    try {
        const order = await apiFetch(`${API_BASE}/Orders/orderDetailsByOrderId?orderId=${encodeURIComponent(orderId)}`);

        const completeBtn      = document.getElementById('order-modal-complete-btn');
        const detailsContainer = document.getElementById('order-modal-details');
        const itemsContainer   = document.getElementById('order-modal-items');
        const nextStatus         = STATUS_TRANSITIONS[order.status]?.[0] ?? null;
        const currentStatusLabel = document.getElementById('order-modal-current-status');
        const nextStatusLabel    = document.getElementById('order-modal-next-status');
        const nextBtn            = document.getElementById('order-modal-next-btn');

        if (currentStatusLabel) currentStatusLabel.textContent = order.status;
        if (nextStatusLabel)    nextStatusLabel.textContent    = nextStatus ?? '–';
        if (nextBtn)            { nextBtn.disabled = !nextStatus; nextBtn.dataset.orderId = order.orderId; nextBtn.dataset.nextStatus = nextStatus; }
        if (completeBtn)        { completeBtn.disabled = order.status !== 'Delivering'; completeBtn.dataset.orderId = order.orderId; }

        if (detailsContainer) {
            detailsContainer.innerHTML = `
                <p><strong>Rendelés ID:</strong> ${order.orderId}</p>
                <p><strong>Dátum:</strong> ${formatDate(order.date)}</p>
                <p><strong>Célcím:</strong> ${order.targetAddress}</p>
                <p><strong>Végösszeg:</strong> ${order.totalPrice} Ft</p>`;
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
        showToast(`Hiba rendelés betöltésekor: ${err.message}`, true);
    }
}

document.getElementById('order-modal-next-btn')
    ?.addEventListener('click', async e => {
        const orderId   = Number(e.currentTarget.dataset.orderId    || 0);
        const newStatus =        e.currentTarget.dataset.nextStatus || '';

        if (!orderId || !newStatus) { showToast('Érvénytelen adat.', true); return; }


        try {
            await apiFetch(`${API_BASE}/Orders/updateorderstatus`, {
                method: 'PUT',
                body: JSON.stringify({ orderId, orderStatus: newStatus })
            });
            closeOrderModal();
            showToast(`Státusz frissítve → ${newStatus}`);
            await loadAllOrders();
        } catch (err) {
            showToast(`Hiba: ${err.message}`, true);
        }
    });

document.getElementById('load-all-orders-btn')
    ?.addEventListener('click', loadAllOrders);

document.getElementById('order-modal-complete-btn')
    ?.addEventListener('click', async e => {
        const orderId = Number(e.currentTarget.dataset.orderId || 0);

        if (!orderId || orderId <= 0) { showToast('Érvénytelen rendelés ID.', true); return; }
        if (!confirm(`Biztosan lezárod ezt a rendelést? (id: ${orderId})`)) return;

        try {
            await apiFetch(`${API_BASE}/Orders/completeorder?orderId=${encodeURIComponent(orderId)}`, { method: 'PUT' });
            closeOrderModal();
            showToast(`Rendelés lezárva (id = ${orderId}).`);
            await loadAllOrders();
        } catch (err) {
            showToast(`Hiba: ${err.message}`, true);
        }
});