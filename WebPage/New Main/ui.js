function getSections() {
  return {
    home: document.getElementById("homepage"),
    search: document.getElementById("searchSection"),
    register: document.getElementById("registerSection"),
    login: document.getElementById("loginSection"),
    profile: document.getElementById("profileSection"),
    orders: document.getElementById("ordersSection"),
    productDetail: document.getElementById("productDetailSection"),
    checkout: document.getElementById("checkoutSection"),
  };
}

function getSectionByName(name) {
  const sections = getSections();
  return sections[name] || sections.home;
}

function getSectionNameByElement(sectionElement) {
  const sections = getSections();
  for (const [name, element] of Object.entries(sections)) {
    if (element === sectionElement) return name;
  }
  return "home";
}

function closeMobileMenu() {
  const mobileMenu = document.getElementById("mobileMenu");
  if (mobileMenu) mobileMenu.style.display = "none";
}

function closeProfileMenu() {
  const profileMenu = document.getElementById("profileMenu");
  if (profileMenu) profileMenu.classList.remove("show");
}

function closeAllMenus() {
  closeMobileMenu();
  closeProfileMenu();
  closeCartMenu();
}

function toggleMenu() {
  const menu = document.getElementById("mobileMenu");
  if (!menu) return;
  const isOpen = menu.style.display === "block";
  closeAllMenus();
  if (!isOpen) menu.style.display = "block";
}

function toggleProfileMenu() {
  const menu = document.getElementById("profileMenu");
  if (!menu) return;
  const isOpen = menu.classList.contains("show");
  closeAllMenus();
  if (!isOpen) menu.classList.add("show");
}

function handleResponsiveMenu() {
  const menu = document.getElementById("mobileMenu");
  const desktopMenu = document.querySelector(".desktop-menu");
  const hamburger = document.querySelector(".hamburger");
  const categoryList = document.getElementById("categoryList");
  const categoryArrowIcon = document.getElementById("categoryArrowIcon");

  if (!menu || !desktopMenu || !hamburger) return;

  if (window.innerWidth > 768) {
    desktopMenu.style.display = "flex";
    hamburger.style.display = "none";
    menu.style.display = "none";

    if (categoryList) {
      categoryList.classList.remove("show");
      categoryList.style.display = "block";
    }

    if (categoryArrowIcon) categoryArrowIcon.src = "arrow-down.svg";
  } else {
    desktopMenu.style.display = "none";
    hamburger.style.display = "flex";
    if (categoryList) categoryList.style.display = "";
  }
}

function initSidebarButtons() {
  const items = document.querySelectorAll(".sidebar .list-group-item");
  items.forEach((item) => {
    item.addEventListener("click", () => {
      items.forEach((i) => i.classList.remove("active"));
      item.classList.add("active");

      const category = item.textContent.trim();
      const searchInput = document.getElementById("searchInput");

      if (searchInput) searchInput.value = category;

      sessionStorage.setItem("selectedCategory", category);

      performSearch(category);
    });
  });
}

function initCategoryToggle() {
  const categoryToggleBtn = document.getElementById("categoryToggleBtn");
  const categoryList = document.getElementById("categoryList");
  const categoryArrowIcon = document.getElementById("categoryArrowIcon");
  if (!categoryToggleBtn || !categoryList || !categoryArrowIcon) return;

  categoryToggleBtn.addEventListener("click", () => {
    if (window.innerWidth > 768) return;
    categoryList.classList.toggle("show");
    categoryArrowIcon.src = categoryList.classList.contains("show")
      ? "arrow-up.svg"
      : "arrow-down.svg";
  });
}

function updateBodyScroll(sectionToShow) {
  const searchSection = document.getElementById("searchSection");
  const isMobile = window.innerWidth <= 768;
  if (sectionToShow === searchSection && !isMobile) {
    document.body.style.overflow = "hidden";
  } else {
    document.body.style.overflow = "auto";
  }
}

function showSection(sectionToShow, pushToHistory = true) {
  const sections = getSections();
  Object.values(sections).forEach((section) =>
    section?.classList.add("d-none"),
  );
  sectionToShow?.classList.remove("d-none");
  updateBodyScroll(sectionToShow);
  closeAllMenus();

  if (pushToHistory) {
    const sectionName = getSectionNameByElement(sectionToShow);
    history.pushState({ section: sectionName }, "", `#${sectionName}`);
  }

  if (sectionToShow?.id === "searchSection") {
    requestAnimationFrame(() => refreshPriceSliderUI());
  }

  window.scrollTo({ top: 0, behavior: "smooth" });
}

function showSectionByName(sectionName, pushToHistory = true) {
  showSection(getSectionByName(sectionName), pushToHistory);
}

function initViewToggle() {
  const viewToggleBtn = document.getElementById("viewToggleBtn");
  const productsWrapper = document.getElementById("productsWrapper");
  if (!viewToggleBtn || !productsWrapper) return;

  viewToggleBtn.addEventListener("click", () => {
    productsWrapper.classList.toggle("list-view");
    viewToggleBtn.textContent = productsWrapper.classList.contains("list-view")
      ? "Rács nézet"
      : "Lista nézet";
  });
}

function initThemeButtons() {
  const desktopThemeBtn = document.getElementById("themeBtn");
  const mobileThemeBtn = document.getElementById("mobileThemeBtn");

  function updateThemeButtons() {
    if (!desktopThemeBtn) return;
    desktopThemeBtn.classList.toggle(
      "btn-dark",
      document.body.classList.contains("dark-mode"),
    );
    desktopThemeBtn.classList.toggle(
      "btn-light",
      !document.body.classList.contains("dark-mode"),
    );
  }

  function toggleTheme() {
    document.body.classList.toggle("dark-mode");
    updateThemeButtons();
  }

  desktopThemeBtn?.addEventListener("click", toggleTheme);
  mobileThemeBtn?.addEventListener("click", () => {
    toggleTheme();
    closeMobileMenu();
  });

  updateThemeButtons();
}

function initPriceFilters() {
  const minRange = document.getElementById("minRange");
  const maxRange = document.getElementById("maxRange");
  const minInput = document.getElementById("minPriceInput");
  const maxInput = document.getElementById("maxPriceInput");
  const sliderFill = document.getElementById("sliderFill");
  const resetFiltersBtn = document.getElementById("resetFiltersBtn");
  const categorySelect = document.getElementById("brandSelect");
  const modelInput = document.getElementById("modelSelect");
  const sortSelect = document.getElementById("sortSelect");

  const MIN_LIMIT = 0;
  const MAX_LIMIT = 2000000;
  const STEP = 1000;
  const MIN_GAP = 1000;

  if (!minRange || !maxRange || !minInput || !maxInput || !sliderFill) return;

  function clamp(value, min, max) {
    const num = parseInt(value, 10);
    if (isNaN(num)) return min;
    return Math.min(Math.max(num, min), max);
  }

  function parsePrice(value) {
    const cleaned = String(value).replace(/[^\d]/g, "");
    return cleaned ? parseInt(cleaned, 10) : 0;
  }

  function formatPrice(value) {
    return Number(value).toLocaleString("hu-HU");
  }

  function updateSliderFill() {
    const min = parseInt(minRange.value, 10);
    const max = parseInt(maxRange.value, 10);
    const minPercent = (min / MAX_LIMIT) * 100;
    const maxPercent = (max / MAX_LIMIT) * 100;
    sliderFill.style.left = `${minPercent}%`;
    sliderFill.style.width = `${maxPercent - minPercent}%`;
  }

  function updateUI(min, max) {
    minRange.value = min;
    maxRange.value = max;
    minInput.value = formatPrice(min);
    maxInput.value = formatPrice(max);
    updateSliderFill();
  }

  function normalizeValues(min, max, changed) {
    min = clamp(min, MIN_LIMIT, MAX_LIMIT);
    max = clamp(max, MIN_LIMIT, MAX_LIMIT);
    if (changed === "min" && min >= max) min = max - MIN_GAP;
    if (changed === "max" && max <= min) max = min + MIN_GAP;
    min = clamp(min, MIN_LIMIT, MAX_LIMIT - MIN_GAP);
    max = clamp(max, MIN_LIMIT + MIN_GAP, MAX_LIMIT);
    return { min, max };
  }

  function syncFromRanges(changed) {
    const normalized = normalizeValues(minRange.value, maxRange.value, changed);
    updateUI(normalized.min, normalized.max);
  }

  function syncFromInputs(changed) {
    const min =
      minInput.value.trim() === "" ? MIN_LIMIT : parsePrice(minInput.value);
    const max =
      maxInput.value.trim() === "" ? MAX_LIMIT : parsePrice(maxInput.value);
    const normalized = normalizeValues(min, max, changed);
    updateUI(normalized.min, normalized.max);
  }

  function handleTyping(input) {
    const raw = input.value.replace(/[^\d]/g, "");
    input.value = raw ? formatPrice(raw) : "";
  }

  async function resetFilters() {
    const searchInput = document.getElementById("searchInput");
    if (searchInput) {
      searchInput.value = "";
    }

    if (categorySelect) categorySelect.selectedIndex = 0;
    if (modelInput) modelInput.value = "";
    if (sortSelect) sortSelect.selectedIndex = 0;

    updateUI(MIN_LIMIT, MAX_LIMIT);

    const productsWrapper = document.getElementById("productsWrapper");
    const viewToggleBtn = document.getElementById("viewToggleBtn");

    if (productsWrapper) {
      productsWrapper.classList.remove("list-view");
    }

    if (viewToggleBtn) {
      viewToggleBtn.textContent = "Lista nézet";
    }

    try {
      const allItems = (await apiGetAllItems()) || [];
      currentSearchMode = "general";
      currentSearchResults = allItems.map(mapAllItemToCardItem);

      renderSearchProducts(currentSearchResults);
      applyFiltersAndRender();
    } catch (error) {
      console.error("Szűrők törlése utáni betöltési hiba:", error);
      currentSearchResults = [];
      renderSearchProducts([]);
    }
  }

  minRange.min = MIN_LIMIT;
  minRange.max = MAX_LIMIT;
  minRange.step = STEP;

  maxRange.min = MIN_LIMIT;
  maxRange.max = MAX_LIMIT;
  maxRange.step = STEP;

  minRange.addEventListener("input", () => {
    syncFromRanges("min");
    applyFiltersAndRender();
  });

  maxRange.addEventListener("input", () => {
    syncFromRanges("max");
    applyFiltersAndRender();
  });

  minInput.addEventListener("input", () => handleTyping(minInput));
  maxInput.addEventListener("input", () => handleTyping(maxInput));

  minInput.addEventListener("blur", () => {
    syncFromInputs("min");
    applyFiltersAndRender();
  });

  maxInput.addEventListener("blur", () => {
    syncFromInputs("max");
    applyFiltersAndRender();
  });

  minInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      syncFromInputs("min");
      applyFiltersAndRender();
    }
  });

  maxInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      syncFromInputs("max");
      applyFiltersAndRender();
    }
  });

  resetFiltersBtn?.addEventListener("click", async () => {
    await resetFilters();
  });

  modelInput?.addEventListener("change", applyFiltersAndRender);
  sortSelect?.addEventListener("change", applyFiltersAndRender);

  updateUI(MIN_LIMIT, MAX_LIMIT);
}

function refreshPriceSliderUI() {
  const minRange = document.getElementById("minRange");
  const maxRange = document.getElementById("maxRange");
  const minInput = document.getElementById("minPriceInput");
  const maxInput = document.getElementById("maxPriceInput");
  const sliderFill = document.getElementById("sliderFill");
  if (!minRange || !maxRange || !minInput || !maxInput || !sliderFill) return;

  const parsePrice = (value) => {
    const cleaned = String(value).replace(/[^\d]/g, "");
    return cleaned ? parseInt(cleaned, 10) : 0;
  };

  const min = parsePrice(minInput.value);
  const max = parsePrice(maxInput.value);
  sliderFill.style.left = `${(min / 2000000) * 100}%`;
  sliderFill.style.width = `${((max - min) / 2000000) * 100}%`;
}

function initPhoneInput() {
  const phoneInput = document.getElementById("phoneInput");
  if (!phoneInput) return;

  const PREFIX = "+36 ";
  const PREFIX_LENGTH = PREFIX.length;

  function getDigits(value) {
    let digits = value.replace(/\D/g, "");
    if (digits.startsWith("36")) digits = digits.slice(2);
    return digits.slice(0, 9);
  }

  function formatPhone(value) {
    const digits = getDigits(value);
    let formatted = PREFIX;
    if (digits.length > 0) formatted += digits.slice(0, 2);
    if (digits.length > 2) formatted += " " + digits.slice(2, 5);
    if (digits.length > 5) formatted += " " + digits.slice(5, 9);
    return formatted;
  }

  function moveCaretToEnd() {
    requestAnimationFrame(() => {
      const end = phoneInput.value.length;
      phoneInput.setSelectionRange(end, end);
    });
  }

  phoneInput.value = phoneInput.value.trim()
    ? formatPhone(phoneInput.value)
    : PREFIX;

  phoneInput.addEventListener("focus", moveCaretToEnd);
  phoneInput.addEventListener("keydown", (e) => {
    if (e.ctrlKey || e.metaKey) return;
    if (
      [
        "Tab",
        "ArrowLeft",
        "ArrowRight",
        "ArrowUp",
        "ArrowDown",
        "Home",
        "End",
      ].includes(e.key)
    )
      return;
    if (
      e.key === "Backspace" &&
      (phoneInput.selectionStart ?? 0) <= PREFIX_LENGTH
    ) {
      e.preventDefault();
      return;
    }
    if (
      e.key === "Delete" &&
      (phoneInput.selectionStart ?? 0) < PREFIX_LENGTH
    ) {
      e.preventDefault();
      return;
    }
    if (!["Backspace", "Delete"].includes(e.key) && !/^\d$/.test(e.key)) {
      e.preventDefault();
    }
  });
  phoneInput.addEventListener("input", () => {
    phoneInput.value = formatPhone(phoneInput.value);
    moveCaretToEnd();
  });
  phoneInput.addEventListener("paste", (e) => {
    e.preventDefault();
    const pastedText = (e.clipboardData || window.clipboardData).getData(
      "text",
    );
    phoneInput.value = formatPhone(pastedText);
    moveCaretToEnd();
  });
}

function initOutsideClickHandlers() {
  document.addEventListener("click", (e) => {
    const mobileMenu = document.getElementById("mobileMenu");
    const hamburger = document.getElementById("hamburgerBtn");
    const profileMenu = document.getElementById("profileMenu");
    const profileWrapper = document.querySelector(".profile-dropdown-wrapper");
    const searchContainer = document.querySelector(".search-container");
    const searchSuggestions = document.getElementById("searchSuggestions");

    if (
      hamburger &&
      mobileMenu &&
      !hamburger.contains(e.target) &&
      !mobileMenu.contains(e.target)
    ) {
      mobileMenu.style.display = "none";
    }

    if (profileWrapper && profileMenu && !profileWrapper.contains(e.target)) {
      profileMenu.classList.remove("show");
    }

    if (
      searchContainer &&
      searchSuggestions &&
      !searchContainer.contains(e.target)
    ) {
      searchSuggestions.classList.add("d-none");
    }
  });
}

function initHistoryHandling() {
  window.addEventListener("popstate", (event) => {
    const sectionName = event.state?.section || "home";
    showSectionByName(sectionName, false);
  });
}

const CART_STORAGE_KEY = "woltmarket_cart";

function getCartState() {
  try {
    return JSON.parse(localStorage.getItem(CART_STORAGE_KEY)) || [];
  } catch {
    return [];
  }
}

function saveCartState(cart) {
  localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cart));
}

function formatCartPrice(value) {
  return `${Number(value).toLocaleString("hu-HU")} Ft`;
}

function getCartTotal(cart) {
  return cart.reduce(
    (sum, item) => sum + Number(item.price) * item.quantity,
    0,
  );
}

function renderCartDropdown() {
  const cartItemsContainer = document.getElementById("cartItems");
  const cartTotalPrice = document.getElementById("cartTotalPrice");

  if (!cartItemsContainer || !cartTotalPrice) return;

  const cart = getCartState();

  if (cart.length === 0) {
    cartItemsContainer.innerHTML = `<div class="cart-empty">A kosár üres.</div>`;
    cartTotalPrice.textContent = "0 Ft";
    return;
  }

  cartItemsContainer.innerHTML = cart
    .map(
      (item) => `
        <div class="cart-item">
          <div class="cart-item-name">${item.itemName}</div>
          <div class="cart-item-row">
            <div class="cart-qty-controls">
              <button class="cart-qty-btn" data-action="decrease" data-id="${item.itemId}" type="button">-</button>
              <span class="cart-qty-value">${item.quantity}</span>
              <button class="cart-qty-btn" data-action="increase" data-id="${item.itemId}" type="button">+</button>
            </div>
            <div class="cart-item-price">${formatCartPrice(Number(item.price) * item.quantity)}</div>
          </div>
        </div>
      `,
    )
    .join("");

  cartTotalPrice.textContent = formatCartPrice(getCartTotal(cart));
}

function addToCart(product) {
  if (!window.isUserLoggedIn || !window.isUserLoggedIn()) {
    alert("A kosár használatához először be kell jelentkezned.");
    return;
  }

  if (!product || product.itemId === null || product.itemId === undefined) {
    console.error("Hiányzó itemId a kosárhoz adásnál:", product);
    alert("A termék nem adható kosárhoz, mert hiányzik az azonosítója.");
    return;
  }

  const cart = getCartState();
  const existingItem = cart.find(
    (x) => String(x.itemId) === String(product.itemId),
  );

  if (existingItem) {
    existingItem.quantity += 1;
  } else {
    cart.push({
      itemId: product.itemId,
      itemName: product.itemName,
      price: Number(product.price),
      quantity: 1,
    });
  }

  saveCartState(cart);
  renderCartDropdown();
}

function updateCartQuantity(itemId, delta) {
  let cart = getCartState();

  cart = cart
    .map((item) => {
      if (String(item.itemId) === String(itemId)) {
        return {
          ...item,
          quantity: item.quantity + delta,
        };
      }
      return item;
    })
    .filter((item) => item.quantity > 0);

  saveCartState(cart);
  renderCartDropdown();
}

function closeCartMenu() {
  const cartMenu = document.getElementById("cartMenu");
  if (cartMenu) cartMenu.classList.remove("show");
}

function toggleCartMenu() {
  const cartMenu = document.getElementById("cartMenu");
  if (!cartMenu) return;

  const isOpen = cartMenu.classList.contains("show");

  closeProfileMenu();
  closeMobileMenu();

  if (isOpen) {
    cartMenu.classList.remove("show");
  } else {
    renderCartDropdown();
    cartMenu.classList.add("show");
  }
}

function initCartUI() {
  const cartBtn = document.getElementById("cartBtn");
  const cartItems = document.getElementById("cartItems");
  const checkoutBtn = document.getElementById("checkoutBtn");
  const detailAddToCartBtn = document.getElementById("detailAddToCartBtn");
  const placeOrderBtn = document.getElementById("placeOrderBtn");

  cartBtn?.addEventListener("click", (e) => {
    e.stopPropagation();
    toggleCartMenu();
  });

  cartItems?.addEventListener("click", (e) => {
    const button = e.target.closest(".cart-qty-btn");
    if (!button) return;

    e.stopPropagation();

    const itemId = button.dataset.id;
    const action = button.dataset.action;

    if (action === "increase") updateCartQuantity(itemId, 1);
    if (action === "decrease") updateCartQuantity(itemId, -1);

    const cartMenu = document.getElementById("cartMenu");
    if (cartMenu) {
      cartMenu.classList.add("show");
    }
  });

  checkoutBtn?.addEventListener("click", () => {
    const cart = getCartState();

    if (!window.isUserLoggedIn || !window.isUserLoggedIn()) {
      alert("A rendeléshez először be kell jelentkezned.");
      return;
    }

    if (cart.length === 0) {
      alert("A kosarad üres.");
      return;
    }

    closeCartMenu();
    renderCheckoutSection();
    showSectionByName("checkout");
  });

  detailAddToCartBtn?.addEventListener("click", () => {
    if (!window.currentDetailProduct) {
      alert("Ehhez a termékhez most nincs betöltött adat.");
      return;
    }

    addToCart(window.currentDetailProduct);
  });

  document.addEventListener("click", (e) => {
    const cartWrapper = document.querySelector(".cart-dropdown-wrapper");
    if (cartWrapper && !cartWrapper.contains(e.target)) {
      closeCartMenu();
    }
  });

  placeOrderBtn?.addEventListener("click", () => {
    placeOrder();
  });

  renderCartDropdown();
}

window.renderCartDropdown = renderCartDropdown;
window.addToCart = addToCart;

const ORDERS_STORAGE_KEY = "woltmarket_orders";

function getOrdersState() {
  try {
    return JSON.parse(localStorage.getItem(ORDERS_STORAGE_KEY)) || [];
  } catch {
    return [];
  }
}

function saveOrdersState(orders) {
  localStorage.setItem(ORDERS_STORAGE_KEY, JSON.stringify(orders));
}

function renderCheckoutSection() {
  const currentUser = window.getCurrentUser?.();
  const cart = getCartState();

  const checkoutAddress = document.getElementById("checkoutAddress");
  const checkoutName = document.getElementById("checkoutName");
  const checkoutPhone = document.getElementById("checkoutPhone");
  const checkoutEmail = document.getElementById("checkoutEmail");

  const shippingAddress = document.getElementById("shippingAddress");
  const shippingName = document.getElementById("shippingName");
  const shippingPhone = document.getElementById("shippingPhone");
  const shippingEmail = document.getElementById("shippingEmail");

  const sameAsCustomerData = document.getElementById("sameAsCustomerData");

  const checkoutItems = document.getElementById("checkoutItems");
  const checkoutTotalPrice = document.getElementById("checkoutTotalPrice");

  if (!currentUser || !checkoutItems || !checkoutTotalPrice) return;

  const userAddress = currentUser.address || "";
  const userName = currentUser.name || "";
  const userPhone = currentUser.phone || "";
  const userEmail = currentUser.email || "";

  if (checkoutAddress) checkoutAddress.value = userAddress;
  if (checkoutName) checkoutName.value = userName;
  if (checkoutPhone) checkoutPhone.value = userPhone;
  if (checkoutEmail) checkoutEmail.value = userEmail;

  if (sameAsCustomerData) {
    sameAsCustomerData.checked = false;
  }

  if (shippingAddress) shippingAddress.value = "";
  if (shippingName) shippingName.value = "";
  if (shippingPhone) shippingPhone.value = "";
  if (shippingEmail) shippingEmail.value = "";

  if (sameAsCustomerData && !sameAsCustomerData.dataset.bound) {
    sameAsCustomerData.addEventListener("change", () => {
      if (sameAsCustomerData.checked) {
        if (shippingAddress) shippingAddress.value = userAddress;
        if (shippingName) shippingName.value = userName;
        if (shippingPhone) shippingPhone.value = userPhone;
        if (shippingEmail) shippingEmail.value = userEmail;
      } else {
        if (shippingAddress) shippingAddress.value = "";
        if (shippingName) shippingName.value = "";
        if (shippingPhone) shippingPhone.value = "";
        if (shippingEmail) shippingEmail.value = "";
      }
    });

    sameAsCustomerData.dataset.bound = "true";
  }

  if (cart.length === 0) {
    checkoutItems.innerHTML = `<div class="text-muted">A kosár üres.</div>`;
    checkoutTotalPrice.textContent = "0 Ft";
    return;
  }

  checkoutItems.innerHTML = cart
    .map(
      (item) => `
        <div class="d-flex justify-content-between align-items-center border rounded p-3 mb-2">
          <div>
            <div><strong>${item.itemName}</strong></div>
            <div>Darabszám: ${item.quantity}</div>
          </div>
          <div><strong>${formatCartPrice(Number(item.price) * item.quantity)}</strong></div>
        </div>
      `,
    )
    .join("");

  checkoutTotalPrice.textContent = formatCartPrice(getCartTotal(cart));
}

function renderOrdersList() {
  const ordersList = document.getElementById("ordersList");
  if (!ordersList) return;

  const orders = getOrdersState();

  if (orders.length === 0) {
    ordersList.innerHTML = `<div class="text-muted">Még nincs leadott rendelésed.</div>`;
    return;
  }

  ordersList.innerHTML = orders
    .slice()
    .reverse()
    .map(
      (order) => `
        <div class="border rounded p-3">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <strong>Rendelés időpontja</strong>
            <span>${order.createdAt}</span>
          </div>

          <div class="mb-2">
            ${order.items
              .map(
                (item) => `
                  <div class="d-flex justify-content-between">
                    <span>${item.itemName} (${item.quantity} db)</span>
                    <span>${formatCartPrice(Number(item.price) * item.quantity)}</span>
                  </div>
                `,
              )
              .join("")}
          </div>

          <div class="d-flex justify-content-between">
            <strong>Összesen:</strong>
            <strong>${formatCartPrice(order.total)}</strong>
          </div>
        </div>
      `,
    )
    .join("");
}

function placeOrder() {
  const cart = getCartState();
  const currentUser = window.getCurrentUser?.();

  const shippingAddress =
    document.getElementById("shippingAddress")?.value.trim() || "";
  const shippingName =
    document.getElementById("shippingName")?.value.trim() || "";
  const shippingPhone =
    document.getElementById("shippingPhone")?.value.trim() || "";
  const shippingEmail =
    document.getElementById("shippingEmail")?.value.trim() || "";

  if (!currentUser) {
    alert("A rendeléshez be kell jelentkezned.");
    return;
  }

  if (cart.length === 0) {
    alert("A kosár üres.");
    return;
  }

  if (!shippingAddress || !shippingName || !shippingPhone || !shippingEmail) {
    alert("Kérlek töltsd ki a szállítási információkat.");
    return;
  }

  const newOrder = {
    createdAt: new Date().toLocaleString("hu-HU"),
    total: getCartTotal(cart),
    items: cart,
    user: {
      name: currentUser.name || "",
      email: currentUser.email || "",
      phone: currentUser.phone || "",
      address: currentUser.address || "",
    },
    shipping: {
      name: shippingName,
      email: shippingEmail,
      phone: shippingPhone,
      address: shippingAddress,
    },
  };

  const orders = getOrdersState();
  orders.push(newOrder);
  saveOrdersState(orders);

  localStorage.removeItem(CART_STORAGE_KEY);
  renderCartDropdown();
  renderOrdersList();

  alert("Sikeres rendelés!");
  showSectionByName("home");
}
