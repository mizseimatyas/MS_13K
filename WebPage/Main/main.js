let port = 7171;

/* ================= MOBILE MENU ================= */

function toggleMenu() {
  const menu = document.getElementById("mobileMenu");
  if (!menu) return;

  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

/* ================= PROFILE MENU ================= */

function toggleProfileMenu() {
  const menu = document.getElementById("profileMenu");
  if (!menu) return;

  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

/* ================= RESPONSIVE ================= */

function handleResponsiveMenu() {
  const menu = document.getElementById("mobileMenu");
  const desktopMenu = document.querySelector(".desktop-menu");
  const hamburger = document.querySelector(".hamburger");

  if (!menu || !desktopMenu || !hamburger) return;

  if (window.innerWidth > 768) {
    desktopMenu.style.display = "flex";
    hamburger.style.display = "none";
    menu.style.display = "none";
  } else {
    desktopMenu.style.display = "none";
    hamburger.style.display = "block";
  }
}

/* ================= SIDEBAR ACTIVE ================= */

function initSidebarButtons() {
  const items = document.querySelectorAll(".sidebar .list-group-item");

  items.forEach((item) => {
    item.addEventListener("click", () => {
      if (item.classList.contains("active")) {
        item.classList.remove("active");
        item.blur();
        return;
      }

      items.forEach((i) => i.classList.remove("active"));
      item.classList.add("active");
    });
  });
}

/* ================= PAGE SWITCH (HOME / SEARCH) ================= */

function initPageSwitching() {
  const searchButton = document.getElementById("search_icon");
  const searchInput = document.getElementById("searchInput");
  const searchSection = document.getElementById("searchSection");
  const homeSection = document.getElementById("homepage");
  const homeLogo = document.getElementById("homeLogo");

  if (
    !searchButton ||
    !searchInput ||
    !searchSection ||
    !homeSection ||
    !homeLogo
  )
    return;

  function showSearchView() {
    const value = searchInput.value.trim();

    if (value !== "") {
      homeSection.classList.add("d-none");
      searchSection.classList.remove("d-none");

      window.scrollTo({
        top: 0,
        behavior: "smooth",
      });
    }
  }

  function showHomeView() {
    searchSection.classList.add("d-none");
    homeSection.classList.remove("d-none");
    searchInput.value = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  }

  searchButton.addEventListener("click", showSearchView);

  searchInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
      showSearchView();
    }
  });

  homeLogo.addEventListener("click", showHomeView);
}

/* ================= PRICE FILTER ================= */

function initPriceFilters() {
  const minRange = document.getElementById("minRange");
  const maxRange = document.getElementById("maxRange");
  const minInput = document.getElementById("minPriceInput");
  const maxInput = document.getElementById("maxPriceInput");
  const sliderFill = document.getElementById("sliderFill");

  const brandSelect = document.getElementById("brandSelect");
  const modelSelect = document.getElementById("modelSelect");
  const sortSelect = document.getElementById("sortSelect");
  const resetFiltersBtn = document.getElementById("resetFiltersBtn");

  const MIN_LIMIT = 0;
  const MAX_LIMIT = 2000000;
  const STEP = 1000;
  const MIN_GAP = 1000;

  if (!minRange || !maxRange || !minInput || !maxInput || !sliderFill) return;

  function clamp(value, min, max) {
    const num = Number(value);
    if (isNaN(num)) return min;
    return Math.min(Math.max(num, min), max);
  }

  function parsePrice(value) {
    return Number(String(value).replace(/\s/g, "").replace(/[^\d]/g, ""));
  }

  function formatPrice(value) {
    return Number(value).toLocaleString("hu-HU");
  }

  function updateSliderFill() {
    const min = Number(minRange.value);
    const max = Number(maxRange.value);

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

  function syncFromRanges(changed) {
    let min = clamp(minRange.value, MIN_LIMIT, MAX_LIMIT);
    let max = clamp(maxRange.value, MIN_LIMIT, MAX_LIMIT);

    if (changed === "min" && min >= max) {
      max = min + MIN_GAP;
    }

    if (changed === "max" && max <= min) {
      min = max - MIN_GAP;
    }

    min = clamp(min, MIN_LIMIT, MAX_LIMIT - MIN_GAP);
    max = clamp(max, MIN_LIMIT + MIN_GAP, MAX_LIMIT);

    updateUI(min, max);
  }

  function syncFromInputs(changed) {
    let min =
      minInput.value.trim() === "" ? MIN_LIMIT : parsePrice(minInput.value);

    let max =
      maxInput.value.trim() === "" ? MAX_LIMIT : parsePrice(maxInput.value);

    min = clamp(min, MIN_LIMIT, MAX_LIMIT);
    max = clamp(max, MIN_LIMIT, MAX_LIMIT);

    if (changed === "min" && min >= max) {
      max = min + MIN_GAP;
    }

    if (changed === "max" && max <= min) {
      min = max - MIN_GAP;
    }

    min = clamp(min, MIN_LIMIT, MAX_LIMIT - MIN_GAP);
    max = clamp(max, MIN_LIMIT + MIN_GAP, MAX_LIMIT);

    updateUI(min, max);
  }

  function handleTyping(input) {
    const raw = input.value.replace(/[^\d]/g, "");
    input.value = raw ? formatPrice(raw) : "";
  }

  function resetFilters() {
    if (brandSelect) brandSelect.selectedIndex = 0;
    if (modelSelect) modelSelect.selectedIndex = 0;
    if (sortSelect) sortSelect.selectedIndex = 0;

    updateUI(MIN_LIMIT, MAX_LIMIT);
  }

  minRange.addEventListener("input", () => syncFromRanges("min"));
  maxRange.addEventListener("input", () => syncFromRanges("max"));

  minInput.addEventListener("input", () => {
    handleTyping(minInput);
    syncFromInputs("min");
  });

  maxInput.addEventListener("input", () => {
    handleTyping(maxInput);
    syncFromInputs("max");
  });

  if (resetFiltersBtn) {
    resetFiltersBtn.addEventListener("click", resetFilters);
  }

  minRange.min = MIN_LIMIT;
  minRange.max = MAX_LIMIT;
  minRange.step = STEP;

  maxRange.min = MIN_LIMIT;
  maxRange.max = MAX_LIMIT;
  maxRange.step = STEP;

  updateUI(MIN_LIMIT, MAX_LIMIT);
}

/* ================= CLICK OUTSIDE MENUS ================= */

document.addEventListener("click", (e) => {
  const mobileMenu = document.getElementById("mobileMenu");
  const hamburger = document.querySelector(".hamburger");
  const profileMenu = document.getElementById("profileMenu");
  const profileWrapper = document.querySelector(".profile-dropdown-wrapper");

  if (
    hamburger &&
    mobileMenu &&
    !hamburger.contains(e.target) &&
    !mobileMenu.contains(e.target)
  ) {
    mobileMenu.style.display = "none";
  }

  if (profileWrapper && profileMenu && !profileWrapper.contains(e.target)) {
    profileMenu.style.display = "none";
  }
});

/* ================= INIT ================= */

window.addEventListener("resize", handleResponsiveMenu);

document.addEventListener("DOMContentLoaded", () => {
  const searchSection = document.getElementById("searchSection");
  const homeSection = document.getElementById("homepage");

  if (searchSection) {
    searchSection.classList.add("d-none");
  }

  if (homeSection) {
    homeSection.classList.remove("d-none");
  }

  handleResponsiveMenu();
  initSidebarButtons();
  initPageSwitching();
  initPriceFilters();
});
