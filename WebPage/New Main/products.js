let allProducts = [];
let currentSearchResults = [];

// Ár formázása magyar pénznem szerint.
// A számot magyar forint formátumra alakítja.
function formatPrice(price) {
  return Number(price).toLocaleString("hu-HU") + " Ft";
}

// Tömb elemeinek véletlen sorrendbe keverése.
// Összekeveri a tömb elemeit véletlenszerű sorrendbe.
function shuffleArray(array) {
  const copy = [...array];
  for (let i = copy.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }
  return copy;
}

// Termékadatok kártyaformátumra alakítása.
function mapAllItemToCardItem(item) {
  return {
    itemId: item.itemId,
    itemName: item.itemName,
    description: item.description,
    quantity: item.quantity,
    price: item.price,
    categoryName: getCategoryNameById(item.categoryId),
  };
}

// Keresési találat kártyaformátumra alakítása.
function mapSearchResultToCardItem(item) {
  const normalizedName = (item.itemName ?? item.itemNamE ?? "").toLowerCase();

  const matchedFullItem = allProducts.find(
    (full) => (full.itemName || "").toLowerCase() === normalizedName,
  );

  return {
    itemId: item.itemId ?? matchedFullItem?.itemId ?? null,
    itemName: item.itemName ?? item.itemNamE ?? "Ismeretlen termék",
    categoryName:
      item.categoryName ??
      item.categoryNamE ??
      getCategoryNameById(item.categoryId ?? matchedFullItem?.categoryId),
    description:
      item.description ?? matchedFullItem?.description ?? "Nincs leírás",
    quantity: item.quantity ?? matchedFullItem?.quantity ?? 0,
    price: Number(item.price ?? item.pricE ?? matchedFullItem?.price ?? 0),
  };
}

// Összes termék betöltése.
async function loadAllProducts() {
  const items = await apiGetAllItems();
  allProducts = Array.isArray(items) ? items : [];
}

// Főoldali termékek kirajzolása.
// Kirendereli a főoldali termékeket.
function renderHomeProducts(products) {
  const wrapper = document.getElementById("homeProductsWrapper");
  if (!wrapper) return;

  if (!products.length) {
    wrapper.innerHTML = `<div class="col-12"><div class="product">Nincs megjeleníthető termék.</div></div>`;
    return;
  }

  wrapper.innerHTML = products
    .map(
      (product) => `
        <div class="col-12 col-sm-6 col-md-6 col-lg-4 col-xl-3">
          <div class="product open-product-detail" data-id="${product.itemId}">
            ${product.itemName}
          </div>
        </div>
      `,
    )
    .join("");
}

// Keresési találatok kirajzolása.
// Kirendereli a keresési találatokat.
function renderSearchProducts(products) {
  const wrapper = document.getElementById("productsWrapper");
  if (!wrapper) return;

  if (!products.length) {
    wrapper.innerHTML = `
      <div class="col-12">
        <div class="product-card">
          <div class="product-name">Nincs találat</div>
        </div>
      </div>
    `;
    return;
  }

  wrapper.innerHTML = products
    .map(
      (product) => `
        <div class="col-12 col-lg-6 col-xl-4 col-xxl-3 product-item">
          <div class="product-card open-product-detail" data-id="${product.itemId ?? ""}">
            <div class="product-image">Termék fotó</div>
            <div class="product-name">${product.itemName}</div>
            <div class="product-description">
              ${product.description || "Nincs leírás"}<br />
              <small>${product.categoryName || ""}</small>
            </div>
            <div class="product-price">${formatPrice(product.price)}</div>
          </div>
        </div>
      `,
    )
    .join("");
}

// Szűrők és rendezés alkalmazása.
// Alkalmazza az összes szűrőt és frissíti a terméklistát.
function applyFiltersAndRender() {
  const categorySelect = document.getElementById("brandSelect");
  const modelInput = document.getElementById("modelSelect");
  const sortSelect = document.getElementById("sortSelect");
  const minPriceInput = document.getElementById("minPriceInput");
  const maxPriceInput = document.getElementById("maxPriceInput");

  let filtered = [...currentSearchResults];

  const selectedCategory = categorySelect?.value?.trim() || "Összes";
  const modelValue = modelInput?.value?.trim() || "Összes";
  const selectedSort = sortSelect?.value?.trim() || "Ár szerint növekvő";

  const minPrice =
    parseInt((minPriceInput?.value || "0").replace(/[^\d]/g, ""), 10) || 0;

  const maxPrice =
    parseInt((maxPriceInput?.value || "2000000").replace(/[^\d]/g, ""), 10) ||
    2000000;

  if (selectedCategory !== "Összes") {
    filtered = filtered.filter(
      (item) =>
        (item.categoryName || "").toLowerCase() ===
        selectedCategory.toLowerCase(),
    );
  }

  if (modelValue !== "Összes" && modelValue !== "") {
    filtered = filtered.filter((item) =>
      (item.itemName || "").toLowerCase().includes(modelValue.toLowerCase()),
    );
  }

  filtered = filtered.filter((item) => {
    const price = Number(item.price) || 0;
    return price >= minPrice && price <= maxPrice;
  });

  if (selectedSort === "Ár szerint növekvő") {
    filtered.sort((a, b) => a.price - b.price);
  } else if (selectedSort === "Ár szerint csökkenő") {
    filtered.sort((a, b) => b.price - a.price);
  } else if (selectedSort === "Név szerint") {
    filtered.sort((a, b) => a.itemName.localeCompare(b.itemName, "hu"));
  }

  renderSearchProducts(filtered);
}

// Főoldali kiemelt termékek betöltése.
async function loadHomeProducts() {
  if (!allProducts.length) await loadAllProducts();
  renderHomeProducts(shuffleArray(allProducts).slice(0, 24));
}

// Termékrészletek megnyitása.
async function openProductDetailById(itemId) {
  if (!itemId) return;

  sessionStorage.setItem("selectedProductId", String(itemId));

  const product = await apiGetItemById(itemId);
  if (!product) return;

  const fullItem = allProducts.find((item) => item.itemId === Number(itemId));

  const categoryName =
    product.categoryName ||
    getCategoryNameById(product.categoryId ?? fullItem?.categoryId) ||
    "Ismeretlen kategória";

  const quantity = Number(product.quantity ?? fullItem?.quantity ?? 0);
  const isOutOfStock = quantity <= 0;

  document.getElementById("detailProductTitle").textContent =
    product.itemName || "Ismeretlen termék";

  document.getElementById("detailProductPrice").textContent = formatPrice(
    product.price ?? 0,
  );

  window.currentDetailProduct = {
    itemId: product.itemId ?? product.id ?? fullItem?.itemId ?? null,
    itemName:
      product.itemName ??
      product.name ??
      fullItem?.itemName ??
      "Ismeretlen termék",
    price: Number(product.price ?? fullItem?.price ?? 0),
    maxQuantity: quantity,
  };

  document.querySelector(".product-gallery-image").innerHTML = `
    <span>${product.itemName || "Ismeretlen termék"}</span>
  `;

  document.getElementById("detailSpecText").textContent =
    product.description || "Nincs leírás.";

  document.getElementById("detailDescText").textContent = `
    ${product.itemName || "Ez a termék"} a(z) ${categoryName} kategóriába tartozik. 
    Jelenlegi készlet: ${isOutOfStock ? "A termék elfogyott" : `${quantity} db`}.
  `;

  document.getElementById("detailStockBox").innerHTML = `
    <div class="stock-title">
      Készlet: ${isOutOfStock ? "A termék elfogyott" : `${quantity} db`}
    </div>
    <div class="stock-item">Kategória: ${categoryName}</div>
  `;

  const detailAddToCartBtn = document.getElementById("detailAddToCartBtn");
  if (detailAddToCartBtn) {
    detailAddToCartBtn.disabled = isOutOfStock;
    detailAddToCartBtn.textContent = isOutOfStock ? "Elfogyott" : "Kosárba";
    detailAddToCartBtn.style.opacity = isOutOfStock ? "0.5" : "1";
    detailAddToCartBtn.style.cursor = isOutOfStock ? "not-allowed" : "pointer";
  }

  showSectionByName("productDetail");
}

// Termékkártya kattintások kezelése.
function initProductDetailOpening() {
  document.addEventListener("click", (event) => {
    const trigger = event.target.closest(".open-product-detail");
    if (!trigger) return;
    const itemId = trigger.dataset.id;
    if (itemId) openProductDetailById(itemId);
  });
}

// Termékrészlet visszaállítása frissítés után.
async function restoreProductDetailAfterRefresh() {
  const savedProductId = sessionStorage.getItem("selectedProductId");
  if (!savedProductId) return;

  await openProductDetailById(savedProductId);
}
