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
    hamburger.style.display = "flex";
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

/* ================= BODY SCROLL CONTROL ================= */

function updateBodyScroll(sectionToShow) {
  const searchSection = document.getElementById("searchSection");

  if (sectionToShow === searchSection) {
    document.body.style.overflow = "hidden";
  } else {
    document.body.style.overflow = "auto";
  }
}

/* ================= SECTION HELPERS ================= */

function getSections() {
  return {
    home: document.getElementById("homepage"),
    search: document.getElementById("searchSection"),
    register: document.getElementById("registerSection"),
    login: document.getElementById("loginSection"),
  };
}

function getSectionByName(name) {
  const sections = getSections();
  return sections[name] || sections.home;
}

function getSectionNameByElement(sectionElement) {
  const sections = getSections();

  for (const [name, element] of Object.entries(sections)) {
    if (element === sectionElement) {
      return name;
    }
  }

  return "home";
}

/* ================= SECTION SWITCH ================= */

function showSection(sectionToShow, pushToHistory = true) {
  const sections = getSections();

  Object.values(sections).forEach((section) => {
    if (section) section.classList.add("d-none");
  });

  if (sectionToShow) {
    sectionToShow.classList.remove("d-none");
  }

  updateBodyScroll(sectionToShow);

  if (pushToHistory) {
    const sectionName = getSectionNameByElement(sectionToShow);
    history.pushState({ section: sectionName }, "", `#${sectionName}`);
  }

  window.scrollTo({
    top: 0,
    behavior: "smooth",
  });
}

function showSectionByName(sectionName, pushToHistory = true) {
  const section = getSectionByName(sectionName);
  showSection(section, pushToHistory);
}

/* ================= PAGE SWITCH ================= */

function initPageSwitching() {
  const searchButton = document.getElementById("search_icon");
  const searchInput = document.getElementById("searchInput");

  const registerBtn = document.getElementById("registerBtn");
  const mobileRegisterBtn = document.getElementById("mobileRegisterBtn");

  const loginBtn = document.getElementById("loginBtn");
  const mobileLoginBtn = document.getElementById("mobileLoginBtn");

  const homeLogo = document.getElementById("homeLogo");

  const profileMenu = document.getElementById("profileMenu");
  const mobileMenu = document.getElementById("mobileMenu");

  function showSearchView() {
    const value = searchInput.value.trim();

    if (value !== "") {
      showSectionByName("search");
    }
  }

  if (searchButton) {
    searchButton.addEventListener("click", showSearchView);
  }

  if (searchInput) {
    searchInput.addEventListener("keydown", (event) => {
      if (event.key === "Enter") {
        showSearchView();
      }
    });
  }

  if (homeLogo) {
    homeLogo.addEventListener("click", () => {
      if (searchInput) searchInput.value = "";
      showSectionByName("home");
    });
  }

  if (registerBtn) {
    registerBtn.addEventListener("click", () => {
      showSectionByName("register");
      if (profileMenu) profileMenu.style.display = "none";
    });
  }

  if (mobileRegisterBtn) {
    mobileRegisterBtn.addEventListener("click", () => {
      showSectionByName("register");
      if (mobileMenu) mobileMenu.style.display = "none";
    });
  }

  if (loginBtn) {
    loginBtn.addEventListener("click", () => {
      showSectionByName("login");
      if (profileMenu) profileMenu.style.display = "none";
    });
  }

  if (mobileLoginBtn) {
    mobileLoginBtn.addEventListener("click", () => {
      showSectionByName("login");
      if (mobileMenu) mobileMenu.style.display = "none";
    });
  }
}

/* ================= VIEW TOGGLE ================= */

function initViewToggle() {
  const viewToggleBtn = document.getElementById("viewToggleBtn");
  const productsWrapper = document.getElementById("productsWrapper");

  if (!viewToggleBtn || !productsWrapper) return;

  viewToggleBtn.addEventListener("click", () => {
    productsWrapper.classList.toggle("list-view");

    if (productsWrapper.classList.contains("list-view")) {
      viewToggleBtn.textContent = "Rács nézet";
    } else {
      viewToggleBtn.textContent = "Lista nézet";
    }
  });
}

/* ================= DARK MODE ================= */

function initThemeButtons() {
  const desktopThemeBtn = document.getElementById("themeBtn");
  const mobileThemeBtn = document.getElementById("mobileThemeBtn");

  function updateThemeButtons() {
    if (document.body.classList.contains("dark-mode")) {
      if (desktopThemeBtn) {
        desktopThemeBtn.classList.remove("btn-light");
        desktopThemeBtn.classList.add("btn-dark");
      }
    } else {
      if (desktopThemeBtn) {
        desktopThemeBtn.classList.remove("btn-dark");
        desktopThemeBtn.classList.add("btn-light");
      }
    }
  }

  function toggleTheme() {
    document.body.classList.toggle("dark-mode");
    updateThemeButtons();
  }

  if (desktopThemeBtn) {
    desktopThemeBtn.addEventListener("click", toggleTheme);
  }

  if (mobileThemeBtn) {
    mobileThemeBtn.addEventListener("click", () => {
      toggleTheme();

      const mobileMenu = document.getElementById("mobileMenu");
      if (mobileMenu) {
        mobileMenu.style.display = "none";
      }
    });
  }

  updateThemeButtons();
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

/* ================= BROWSER BACK / FORWARD ================= */

window.addEventListener("popstate", (event) => {
  const sectionName = event.state?.section || "home";
  showSectionByName(sectionName, false);
});

/* ================= INIT ================= */

window.addEventListener("resize", handleResponsiveMenu);

document.addEventListener("DOMContentLoaded", () => {
  const sections = getSections();

  Object.values(sections).forEach((section) => {
    if (section) section.classList.add("d-none");
  });

  const hash = window.location.hash.replace("#", "");
  const validSections = ["home", "search", "register", "login"];
  const initialSection = validSections.includes(hash) ? hash : "home";

  showSectionByName(initialSection, false);
  history.replaceState({ section: initialSection }, "", `#${initialSection}`);

  handleResponsiveMenu();
  initSidebarButtons();
  initPageSwitching();
  initViewToggle();
  initThemeButtons();
  initPriceFilters();
});
