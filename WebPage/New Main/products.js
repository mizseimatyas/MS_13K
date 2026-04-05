let allProducts = [];
let currentSearchResults = [];

function formatPrice(price) {
  return Number(price).toLocaleString("hu-HU") + " Ft";
}

function shuffleArray(array) {
  const copy = [...array];
  for (let i = copy.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }
  return copy;
}

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

function mapSearchResultToCardItem(item) {
  const normalizedName = (item.itemNamE || item.itemName || "").toLowerCase();

  const matchedFullItem = allProducts.find(
    (full) => (full.itemName || "").toLowerCase() === normalizedName,
  );

  return {
    itemId: item.itemId ?? item.id ?? matchedFullItem?.itemId ?? null,
    itemName: item.itemNamE ?? item.itemName ?? "Ismeretlen termék",
    categoryName:
      item.categoryNamE ?? item.categoryName ?? "Ismeretlen kategória",
    description:
      item.description ?? matchedFullItem?.description ?? "Nincs leírás",
    quantity: item.quantity ?? matchedFullItem?.quantity ?? 0,
    price: Number(item.pricE ?? item.price ?? 0),
  };
}

async function loadAllProducts() {
  const items = await apiGetAllItems();
  allProducts = Array.isArray(items) ? items : [];
}

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

async function loadHomeProducts() {
  if (!allProducts.length) await loadAllProducts();
  renderHomeProducts(shuffleArray(allProducts).slice(0, 24));
}

async function openProductDetailById(itemId) {
  if (!itemId) return;
  const product = await apiGetItemById(itemId);
  if (!product) return;

  const fullItem = allProducts.find((item) => item.itemId === Number(itemId));
  const categoryName = fullItem
    ? getCategoryNameById(fullItem.categoryId)
    : "Ismeretlen kategória";

  document.getElementById("detailProductTitle").textContent = product.itemName;
  document.getElementById("detailProductPrice").textContent = formatPrice(
    product.price,
  );
  window.currentDetailProduct = {
    itemId: product.itemId ?? product.id ?? fullItem?.itemId ?? null,
    itemName:
      product.itemName ??
      product.name ??
      fullItem?.itemName ??
      "Ismeretlen termék",
    price: Number(product.price ?? fullItem?.price ?? 0),
    maxQuantity: Number(product.quantity ?? fullItem?.quantity ?? 0),
  };
  document.querySelector(".product-gallery-image").innerHTML =
    `<span>${product.itemName}</span>`;
  document.getElementById("detailSpecText").textContent =
    product.description || "Nincs leírás.";
  document.getElementById("detailDescText").textContent =
    `${product.itemName} a(z) ${categoryName} kategóriába tartozik. Jelenlegi elérhető mennyiség: ${product.quantity} db.`;
  document.getElementById("detailStockBox").innerHTML = `
  <div class="stock-title">Készlet</div>
  <div class="stock-item">
    ${
      Number(product.quantity) > 0
        ? `Elérhető: ${product.quantity} db`
        : `A termék elfogyott`
    }
  </div>
  <div class="stock-item">Kategória: ${categoryName}</div>
`;

  const detailAddToCartBtn = document.getElementById("detailAddToCartBtn");

  if (detailAddToCartBtn) {
    const isOutOfStock = Number(product.quantity) <= 0;

    detailAddToCartBtn.disabled = isOutOfStock;
    detailAddToCartBtn.textContent = isOutOfStock ? "Elfogyott" : "Kosárba";
    detailAddToCartBtn.style.opacity = isOutOfStock ? "0.5" : "1";
    detailAddToCartBtn.style.cursor = isOutOfStock ? "not-allowed" : "pointer";
  }

  showSectionByName("productDetail");
}

function initProductDetailOpening() {
  document.addEventListener("click", (event) => {
    const trigger = event.target.closest(".open-product-detail");
    if (!trigger) return;
    const itemId = trigger.dataset.id;
    if (itemId) openProductDetailById(itemId);
  });
}
