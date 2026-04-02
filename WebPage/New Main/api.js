const API_BASE = "https://localhost:7149/api";

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

async function apiGetAllItems() {
  return await fetchJSON(`${API_BASE}/items/allitems`);
}

async function apiGetItemById(id) {
  return await fetchJSON(`${API_BASE}/items/itembyid?id=${id}`);
}

async function apiGetItemByName(name) {
  return await fetchJSON(
    `${API_BASE}/items/itembyname?name=${encodeURIComponent(name)}`,
  );
}

async function apiGetItemsByNameFragment(fragment) {
  return await fetchJSON(
    `${API_BASE}/items/itemnamebyfragment?fragname=${encodeURIComponent(fragment)}`,
  );
}

async function apiGetItemsInCategory(category) {
  return await fetchJSON(
    `${API_BASE}/items/itemsincategory?category=${encodeURIComponent(category)}`,
  );
}

async function apiGetAllCategories() {
  return await fetchJSON(`${API_BASE}/categories/allcategories`);
}

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

async function apiGetCurrentUser() {
  const response = await fetch(`${API_BASE}/Users/me`, {
    method: "GET",
    credentials: "include",
  });

  if (response.status === 401) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Felhasználó lekérési hiba: ${response.status}`);
  }

  return await response.json();
}

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
