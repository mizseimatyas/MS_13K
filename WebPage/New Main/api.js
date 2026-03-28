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
