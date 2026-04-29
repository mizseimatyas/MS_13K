let allCategories = [];
let currentSuggestions = [];
let selectedSuggestionIndex = -1;
let currentSearchMode = "general";

// Kategórianév megkeresése azonosító alapján.
function getCategoryNameById(categoryId) {
  const found = allCategories.find(
    (category) => category.categoryId === categoryId,
  );
  return found?.categoryName || "Ismeretlen kategória";
}

// Kategóriák betöltése.
// Betölti az összes kategóriát az API-ból.
async function loadCategories() {
  const categories = await apiGetAllCategories();

  allCategories = Array.isArray(categories)
    ? categories.map((category) => ({
        categoryId: category.categoryId,
        categoryName: category.categoryName,
      }))
    : [];

  fillCategoryFilter();
  renderSidebarCategories();
}

// Kategória legördülő feltöltése.
function fillCategoryFilter() {
  const select = document.getElementById("brandSelect");
  if (!select) return;

  select.innerHTML = `<option>Összes</option>`;
  allCategories.forEach((category) => {
    select.innerHTML += `<option>${category.categoryName}</option>`;
  });
}

// Oldalsáv kategóriáinak kirajzolása.
function renderSidebarCategories() {
  const categoryList = document.getElementById("categoryList");
  if (!categoryList) return;

  categoryList.innerHTML = allCategories
    .map(
      (category) => `
        <button
          class="list-group-item list-group-item-action"
          type="button"
          data-category="${category.categoryName}"
        >
          ${category.categoryName}
        </button>
      `,
    )
    .join("");
}

// Keresési szűrők alaphelyzetbe állítása.
function resetSearchSidebarState(selectedCategory = "Összes") {
  const categoryFilter = document.getElementById("brandSelect");
  const modelInput = document.getElementById("modelSelect");
  const sortSelect = document.getElementById("sortSelect");
  const minPriceInput = document.getElementById("minPriceInput");
  const maxPriceInput = document.getElementById("maxPriceInput");
  const minRange = document.getElementById("minRange");
  const maxRange = document.getElementById("maxRange");
  const productsWrapper = document.getElementById("productsWrapper");
  const viewToggleBtn = document.getElementById("viewToggleBtn");

  if (categoryFilter) categoryFilter.value = selectedCategory;
  if (modelInput) modelInput.value = "";
  if (sortSelect) sortSelect.selectedIndex = 0;

  if (minPriceInput) minPriceInput.value = "0";
  if (maxPriceInput) maxPriceInput.value = "2000000";

  if (minRange) minRange.value = "0";
  if (maxRange) maxRange.value = "2000000";

  if (productsWrapper) {
    productsWrapper.classList.remove("list-view");
  }

  if (viewToggleBtn) {
    viewToggleBtn.textContent = "Lista nézet";
  }

  refreshPriceSliderUI();
}

// Keresési javaslatok megjelenítése.
function renderSearchSuggestions(suggestions) {
  const box = document.getElementById("searchSuggestions");
  if (!box) return;

  currentSuggestions = suggestions;
  selectedSuggestionIndex = -1;

  if (!suggestions.length) {
    box.innerHTML = "";
    box.classList.add("d-none");
    return;
  }

  box.innerHTML = suggestions
    .map(
      (item, index) => `
        <div class="search-suggestion-item" data-index="${index}">
          <span class="search-suggestion-type">${item.type === "category" ? "Kategória" : "Termék"}</span>
          ${item.label}
        </div>
      `,
    )
    .join("");

  box.classList.remove("d-none");
}

// Aktív keresési javaslat kijelölése.
function updateSuggestionSelection() {
  const items = document.querySelectorAll(".search-suggestion-item");
  items.forEach((item, index) => {
    item.classList.toggle("active", index === selectedSuggestionIndex);
  });
}

// Keresési javaslatok betöltése.
async function loadSearchSuggestions(query = "") {
  const trimmed = query.trim().toLowerCase();

  if (!trimmed) {
    renderSearchSuggestions(
      allCategories.map((category) => ({
        type: "category",
        label: category.categoryName,
      })),
    );
    return;
  }

  const matchedCategories = allCategories
    .filter((category) => category.categoryName.toLowerCase().includes(trimmed))
    .map((category) => ({ type: "category", label: category.categoryName }));

  const itemMatchesRaw = (await apiGetItemsByNameFragment(trimmed)) || [];
  const matchedItems = itemMatchesRaw.map((item) => ({
    type: "item",
    label: item.itemName ?? item.itemNamE ?? "Ismeretlen termék",
    categoryName:
      item.categoryName ??
      item.categoryNamE ??
      getCategoryNameById(item.categoryId),
  }));

  const dedupedItems = matchedItems.filter(
    (item, index, array) =>
      array.findIndex((x) => x.label === item.label) === index,
  );

  renderSearchSuggestions([...matchedCategories, ...dedupedItems].slice(0, 20));
}

// Termék- vagy kategóriakeresés végrehajtása.
// Elindítja a keresést kategória vagy terméknév alapján.
async function performSearch(rawQuery) {
  const query = rawQuery.trim();
  if (!query) return;

  const exactItem = await apiGetItemByName(query);
  if (exactItem) {
    const fullMatch = allProducts.find(
      (item) =>
        item.itemName.toLowerCase() === exactItem.itemName.toLowerCase(),
    );
    if (fullMatch?.itemId) {
      openProductDetailById(fullMatch.itemId);
      return;
    }
  }

  const categoryMatch = allCategories.find(
    (category) => category.categoryName.toLowerCase() === query.toLowerCase(),
  );

  const categoryFilter = document.getElementById("brandSelect");

  if (categoryMatch) {
    currentSearchMode = "category";

    const categoryItems =
      (await apiGetItemsInCategory(categoryMatch.categoryName)) || [];

    currentSearchResults = categoryItems.map(mapSearchResultToCardItem);

    resetSearchSidebarState(categoryMatch.categoryName);
    sessionStorage.setItem("selectedCategory", categoryMatch.categoryName);

    showSectionByName("search");
    renderSearchProducts(currentSearchResults);
    applyFiltersAndRender();
    return;
  }

  currentSearchMode = "general";

  const nameMatches = (await apiGetItemsByNameFragment(query)) || [];
  currentSearchResults = nameMatches.map(mapSearchResultToCardItem);

  resetSearchSidebarState("Összes");
  sessionStorage.setItem("selectedCategory", "Összes");

  showSectionByName("search");
  renderSearchProducts(currentSearchResults);
  applyFiltersAndRender();
}

// Kategóriaszűrő módosításának kezelése.
async function handleCategoryFilterChange() {
  const categoryFilter = document.getElementById("brandSelect");
  const searchInput = document.getElementById("searchInput");
  if (!categoryFilter) return;

  const selectedCategory = categoryFilter.value;
  sessionStorage.setItem("selectedCategory", selectedCategory);

  try {
    if (selectedCategory === "Összes") {
      if (searchInput) {
        searchInput.value = "";
      }

      const allItems = (await apiGetAllItems()) || [];
      currentSearchMode = "general";
      currentSearchResults = allItems.map(mapAllItemToCardItem);

      resetSearchSidebarState("Összes");
      applyFiltersAndRender();
      return;
    }

    if (searchInput) {
      searchInput.value = selectedCategory;
    }

    const categoryItems = (await apiGetItemsInCategory(selectedCategory)) || [];

    currentSearchMode = "category";
    currentSearchResults = categoryItems.map(mapSearchResultToCardItem);

    resetSearchSidebarState(selectedCategory);
    applyFiltersAndRender();
  } catch (error) {
    console.error("Kategória váltási hiba:", error);
    currentSearchResults = [];
    renderSearchProducts([]);
  }
}

// Keresési javaslatok elrejtése.
function hideSearchSuggestions() {
  document.getElementById("searchSuggestions")?.classList.add("d-none");
}

// Kereső inicializálása.
function initSearch() {
  const searchButton = document.getElementById("search_icon");
  const searchInput = document.getElementById("searchInput");
  const suggestionsBox = document.getElementById("searchSuggestions");
  const categoryFilter = document.getElementById("brandSelect");

  // submitSearch feladatát végző blokk.
  function submitSearch() {
    const value = searchInput?.value.trim() || "";
    if (!value) return;
    hideSearchSuggestions();
    performSearch(value);
  }

  searchButton?.addEventListener("click", submitSearch);

  searchInput?.addEventListener("focus", () =>
    loadSearchSuggestions(searchInput.value),
  );
  searchInput?.addEventListener("input", () =>
    loadSearchSuggestions(searchInput.value),
  );

  searchInput?.addEventListener("keydown", (event) => {
    if (event.key === "ArrowDown") {
      event.preventDefault();
      if (currentSuggestions.length) {
        selectedSuggestionIndex = Math.min(
          selectedSuggestionIndex + 1,
          currentSuggestions.length - 1,
        );
        updateSuggestionSelection();
      }
      return;
    }

    if (event.key === "ArrowUp") {
      event.preventDefault();
      if (currentSuggestions.length) {
        selectedSuggestionIndex = Math.max(selectedSuggestionIndex - 1, 0);
        updateSuggestionSelection();
      }
      return;
    }

    if (event.key === "Enter") {
      event.preventDefault();
      if (
        selectedSuggestionIndex >= 0 &&
        currentSuggestions[selectedSuggestionIndex]
      ) {
        const picked = currentSuggestions[selectedSuggestionIndex];
        searchInput.value = picked.label;
      }
      submitSearch();
    }
  });

  suggestionsBox?.addEventListener("click", (event) => {
    const item = event.target.closest(".search-suggestion-item");
    if (!item) return;
    const index = Number(item.dataset.index);
    const picked = currentSuggestions[index];
    if (!picked) return;
    searchInput.value = picked.label;
    hideSearchSuggestions();
    performSearch(picked.label);
  });

  categoryFilter?.addEventListener("change", async () => {
    await handleCategoryFilterChange();
  });
}

// Keresési nézet visszaállítása.
async function restoreSearchViewAfterRefresh() {
  const categoryFilter = document.getElementById("brandSelect");
  const searchInput = document.getElementById("searchInput");

  if (!categoryFilter) return;

  const savedCategory = sessionStorage.getItem("selectedCategory") || "Összes";
  categoryFilter.value = savedCategory;

  try {
    if (savedCategory !== "Összes") {
      if (searchInput) searchInput.value = savedCategory;

      const categoryItems = (await apiGetItemsInCategory(savedCategory)) || [];
      currentSearchMode = "category";
      currentSearchResults = categoryItems.map(mapSearchResultToCardItem);
      applyFiltersAndRender();
      return;
    }

    if (searchInput) searchInput.value = "";

    const allItems = (await apiGetAllItems()) || [];
    currentSearchMode = "general";
    currentSearchResults = allItems.map(mapAllItemToCardItem);
    applyFiltersAndRender();
  } catch (error) {
    console.error("Search restore hiba:", error);
    currentSearchResults = [];
    renderSearchProducts([]);
  }
}
