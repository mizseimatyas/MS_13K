let allCategories = [];
let currentSuggestions = [];
let selectedSuggestionIndex = -1;
let currentSearchMode = "general";

function getCategoryNameById(categoryId) {
  const found = allCategories.find(
    (category) => category.categoryId === categoryId,
  );
  return found?.categoryName || "Ismeretlen kategória";
}

async function loadCategories() {
  const categories = await apiGetAllCategories();
  allCategories = Array.isArray(categories)
    ? categories.map((category, index) => ({
        categoryId: index + 1,
        categoryName: category.categoryName,
      }))
    : [];
  fillCategoryFilter();
}

function fillCategoryFilter() {
  const select = document.getElementById("brandSelect");
  if (!select) return;

  select.innerHTML = `<option>Összes</option>`;
  allCategories.forEach((category) => {
    select.innerHTML += `<option>${category.categoryName}</option>`;
  });
}

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

function updateSuggestionSelection() {
  const items = document.querySelectorAll(".search-suggestion-item");
  items.forEach((item, index) => {
    item.classList.toggle("active", index === selectedSuggestionIndex);
  });
}

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
    label: item.itemNamE,
    categoryName: item.categoryNamE,
  }));

  const dedupedItems = matchedItems.filter(
    (item, index, array) =>
      array.findIndex((x) => x.label === item.label) === index,
  );

  renderSearchSuggestions([...matchedCategories, ...dedupedItems].slice(0, 20));
}

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

    if (categoryFilter) {
      categoryFilter.value = categoryMatch.categoryName;
    }

    showSectionByName("search");
    renderSearchProducts(currentSearchResults);
    applyFiltersAndRender();
    return;
  }

  currentSearchMode = "general";

  const nameMatches = (await apiGetItemsByNameFragment(query)) || [];
  currentSearchResults = nameMatches.map(mapSearchResultToCardItem);

  if (categoryFilter) {
    categoryFilter.value = "Összes";
  }

  showSectionByName("search");
  renderSearchProducts(currentSearchResults);
  applyFiltersAndRender();
}

async function handleCategoryFilterChange() {
  const categoryFilter = document.getElementById("brandSelect");
  const searchInput = document.getElementById("searchInput");
  if (!categoryFilter) return;

  const selectedCategory = categoryFilter.value;

  try {
    if (selectedCategory === "Összes") {
      if (searchInput) {
        searchInput.value = "";
      }

      const allItems = (await apiGetAllItems()) || [];
      currentSearchMode = "general";
      currentSearchResults = allItems.map(mapAllItemToCardItem);

      applyFiltersAndRender();
      return;
    }

    if (searchInput) {
      searchInput.value = selectedCategory;
    }

    const categoryItems = (await apiGetItemsInCategory(selectedCategory)) || [];

    currentSearchMode = "category";
    currentSearchResults = categoryItems.map(mapSearchResultToCardItem);

    applyFiltersAndRender();
  } catch (error) {
    console.error("Kategória váltási hiba:", error);
    currentSearchResults = [];
    renderSearchProducts([]);
  }
}

function hideSearchSuggestions() {
  document.getElementById("searchSuggestions")?.classList.add("d-none");
}

function initSearch() {
  const searchButton = document.getElementById("search_icon");
  const searchInput = document.getElementById("searchInput");
  const suggestionsBox = document.getElementById("searchSuggestions");
  const categoryFilter = document.getElementById("brandSelect");

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
