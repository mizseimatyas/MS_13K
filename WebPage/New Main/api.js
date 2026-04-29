// Backend API alap URL-je.
const API_BASE = "https://localhost:7149/api";

// Általános fetch segédfüggvény JSON válaszhoz és hibakezeléshez.
// Általános API lekérő függvény. Meghív egy URL-t és JSON választ ad vissza.
async function fetchJSON(url) {
  const response = await fetch(url, {
    credentials: "include",
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`HTTP hiba: ${response.status}`);
  }

  return await response.json();
}

// Az összes termék lekérése.
// Lekéri az összes terméket az adatbázisból.
async function apiGetAllItems() {
  return await fetchJSON(`${API_BASE}/items/allitems`);
}

// Termék lekérése azonosító alapján.
// Lekér egy terméket azonosító alapján.
async function apiGetItemById(id) {
  return await fetchJSON(`${API_BASE}/items/itembyid?id=${id}`);
}

// Termék lekérése pontos név alapján.
// Lekér egy terméket név alapján.
async function apiGetItemByName(name) {
  return await fetchJSON(
    `${API_BASE}/items/itembyname?name=${encodeURIComponent(name)}`,
  );
}

// Termékek keresése névrészlet alapján.
// Termékeket keres névrészlet alapján.
async function apiGetItemsByNameFragment(fragment) {
  return await fetchJSON(
    `${API_BASE}/items/itemnamebyfragment?fragname=${encodeURIComponent(fragment)}`,
  );
}

// Termékek lekérése kategória alapján.
// Lekéri az adott kategóriába tartozó termékeket.
async function apiGetItemsInCategory(category) {
  return await fetchJSON(
    `${API_BASE}/items/itemsincategory?category=${encodeURIComponent(category)}`,
  );
}

// Összes kategória lekérése.
// Lekéri az összes kategóriát.
async function apiGetAllCategories() {
  return await fetchJSON(`${API_BASE}/categories/allcategories`);
}

// Felhasználó bejelentkeztetése.
// Bejelentkezteti a felhasználót.
async function apiLoginUser(email, password) {
  const response = await fetch(
    `${API_BASE}/Users/loginuser?email=${encodeURIComponent(email)}&password=${encodeURIComponent(password)}`,
    {
      method: "POST",
      credentials: "include",
    },
  );

  if (!response.ok) {
    throw new Error(`Bejelentkezési hiba: ${response.status}`);
  }

  return await response.json().catch(() => null);
}

// Profiladatok frissítése.
async function apiUpdateProfile(payload) {
  const response = await fetch(`${API_BASE}/Users/updateprofile`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  const text = await response.text();

  if (!response.ok) {
    throw new Error(text || "A profil mentése nem sikerült.");
  }

  try {
    return JSON.parse(text);
  } catch {
    return text || null;
  }
}

// Felhasználó kijelentkeztetése.
// Kijelentkezteti a felhasználót.
async function apiLogoutUser() {
  const response = await fetch(`${API_BASE}/Users/logout`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error(`Kijelentkezési hiba: ${response.status}`);
  }

  return true;
}

// Aktuális felhasználó lekérése.
async function apiGetCurrentUser() {
  const response = await fetch(`${API_BASE}/Users/me`, {
    method: "GET",
    credentials: "include",
  });

  if (response.status === 401 || response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Felhasználó lekérési hiba: ${response.status}`);
  }

  return await response.json();
}

// Új felhasználó regisztrálása.
// Új felhasználót regisztrál.
async function apiRegisterUser(email, password, address, phone) {
  const response = await fetch(
    `${API_BASE}/Users/userregistry?email=${encodeURIComponent(email)}&password=${encodeURIComponent(password)}&address=${encodeURIComponent(address || "")}&phone=${encodeURIComponent(phone || "")}`,
    {
      method: "POST",
      credentials: "include",
    },
  );

  const text = await response.text();

  if (!response.ok) {
    console.error(text);
    throw new Error(text || "A regisztráció nem sikerült.");
  }

  try {
    return JSON.parse(text);
  } catch {
    return text || null;
  }
}

// Rendelési előzmények lekérése.
async function apiGetOrderHistoryByUserId(userId) {
  return await fetchJSON(
    `${API_BASE}/Orders/orderhistory?userid=${encodeURIComponent(userId)}`,
  );
}

// Rendelés részleteinek lekérése.
async function apiGetOrderDetails(userId, orderId) {
  return await fetchJSON(
    `${API_BASE}/Orders/orderdetails?userid=${encodeURIComponent(userId)}&orderId=${encodeURIComponent(orderId)}`,
  );
}

// Rendelés lemondása.
async function apiCancelOrder(orderId, userId) {
  const response = await fetch(
    `${API_BASE}/Orders/usercancelorder?orderid=${encodeURIComponent(orderId)}&userid=${encodeURIComponent(userId)}`,
    {
      method: "PUT",
      credentials: "include",
    },
  );

  const text = await response.text();

  if (!response.ok) {
    throw new Error(text || "A rendelés törlése nem sikerült.");
  }

  return text || true;
}

// Kosár tartalmának lekérése.
async function apiGetCartInventory(userId) {
  return await fetchJSON(
    `${API_BASE}/Carts/cartinventory?userid=${encodeURIComponent(userId)}`,
  );
}

// Kosár végösszegének lekérése.
async function apiGetCartTotalPrice(userId) {
  return await fetchJSON(
    `${API_BASE}/Carts/cartinventorytotalprice?userid=${encodeURIComponent(userId)}`,
  );
}

// Kosár elem módosítása.
async function apiModifyCartItem(payload) {
  const response = await fetch(`${API_BASE}/Carts/modifycart`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  const text = await response.text();

  if (!response.ok) {
    throw new Error(text || "A kosár módosítása nem sikerült.");
  }

  return text || true;
}

// Rendelés leadása.
async function apiPlaceOrder(payload) {
  const response = await fetch(`${API_BASE}/Orders/placeorder`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  const text = await response.text();

  if (!response.ok) {
    throw new Error(text || "A rendelés leadása nem sikerült.");
  }

  try {
    return JSON.parse(text);
  } catch {
    return text || null;
  }
}

// Termék kosárba helyezése.
async function apiAddToCart(payload) {
  const response = await fetch(`${API_BASE}/Carts/addtocart`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  const text = await response.text();

  if (!response.ok) {
    throw new Error(text || "A termék kosárba helyezése nem sikerült.");
  }

  return text || true;
}
