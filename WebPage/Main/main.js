let port = 7171;

/* ================= ALAP SEGÉDFÜGGVÉNYEK ================= */

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

function closeMobileMenu() {
  const mobileMenu = document.getElementById("mobileMenu");
  if (mobileMenu) mobileMenu.style.display = "none";
}

function closeProfileMenu() {
  const profileMenu = document.getElementById("profileMenu");
  if (profileMenu) profileMenu.style.display = "none";
}

function closeAllMenus() {
  closeMobileMenu();
  closeProfileMenu();
}

/* ================= MOBILE MENU ================= */

function toggleMenu() {
  const menu = document.getElementById("mobileMenu");
  if (!menu) return;

  const isOpen = menu.style.display === "block";
  closeAllMenus();

  if (!isOpen) {
    menu.style.display = "block";
  }
}

/* ================= PROFILE MENU ================= */

function toggleProfileMenu() {
  const menu = document.getElementById("profileMenu");
  if (!menu) return;

  const isOpen = menu.style.display === "block";
  closeAllMenus();

  if (!isOpen) {
    menu.style.display = "block";
  }
}

/* ================= RESPONSIVE ================= */

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

    if (categoryArrowIcon) {
      categoryArrowIcon.src = "arrow-down.svg";
    }
  } else {
    desktopMenu.style.display = "none";
    hamburger.style.display = "flex";

    if (categoryList) {
      categoryList.style.display = "";
    }
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

/* ================= MOBILE CATEGORY TOGGLE ================= */

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

/* ================= BODY SCROLL CONTROL ================= */

function updateBodyScroll(sectionToShow) {
  const searchSection = document.getElementById("searchSection");

  if (sectionToShow === searchSection) {
    document.body.style.overflow = "hidden";
  } else {
    document.body.style.overflow = "auto";
  }
}

/* ================= PRICE SLIDER UI REFRESH ================= */

function refreshPriceSliderUI() {
  const minRange = document.getElementById("minRange");
  const maxRange = document.getElementById("maxRange");
  const minInput = document.getElementById("minPriceInput");
  const maxInput = document.getElementById("maxPriceInput");
  const sliderFill = document.getElementById("sliderFill");

  const MAX_LIMIT = 2000000;
  const MIN_LIMIT = 0;
  const MIN_GAP = 1000;

  if (!minRange || !maxRange || !minInput || !maxInput || !sliderFill) return;

  function parsePrice(value) {
    const cleaned = String(value).replace(/[^\d]/g, "");
    return cleaned ? parseInt(cleaned, 10) : 0;
  }

  function formatPrice(value) {
    return Number(value).toLocaleString("hu-HU");
  }

  let min = parsePrice(minInput.value);
  let max = parsePrice(maxInput.value);

  if (isNaN(min)) min = MIN_LIMIT;
  if (isNaN(max)) max = MAX_LIMIT;

  min = Math.max(MIN_LIMIT, Math.min(min, MAX_LIMIT - MIN_GAP));
  max = Math.max(MIN_LIMIT + MIN_GAP, Math.min(max, MAX_LIMIT));

  if (min >= max) {
    min = max - MIN_GAP;
    if (min < MIN_LIMIT) {
      min = MIN_LIMIT;
      max = MIN_LIMIT + MIN_GAP;
    }
  }

  minRange.value = min;
  maxRange.value = max;
  minInput.value = formatPrice(min);
  maxInput.value = formatPrice(max);

  const minPercent = (min / MAX_LIMIT) * 100;
  const maxPercent = (max / MAX_LIMIT) * 100;

  sliderFill.style.left = `${minPercent}%`;
  sliderFill.style.width = `${maxPercent - minPercent}%`;
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
  closeAllMenus();

  if (pushToHistory) {
    const sectionName = getSectionNameByElement(sectionToShow);
    history.pushState({ section: sectionName }, "", `#${sectionName}`);
  }

  if (sectionToShow && sectionToShow.id === "searchSection") {
    requestAnimationFrame(() => {
      refreshPriceSliderUI();
    });
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

  function showSearchView() {
    if (!searchInput) return;

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
    });
  }

  if (mobileRegisterBtn) {
    mobileRegisterBtn.addEventListener("click", () => {
      showSectionByName("register");
    });
  }

  if (loginBtn) {
    loginBtn.addEventListener("click", () => {
      showSectionByName("login");
    });
  }

  if (mobileLoginBtn) {
    mobileLoginBtn.addEventListener("click", () => {
      showSectionByName("login");
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
    viewToggleBtn.textContent = productsWrapper.classList.contains("list-view")
      ? "Rács nézet"
      : "Lista nézet";
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
      closeMobileMenu();
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

    if (changed === "min" && min >= max) {
      min = max - MIN_GAP;
    }

    if (changed === "max" && max <= min) {
      max = min + MIN_GAP;
    }

    min = clamp(min, MIN_LIMIT, MAX_LIMIT - MIN_GAP);
    max = clamp(max, MIN_LIMIT + MIN_GAP, MAX_LIMIT);

    return { min, max };
  }

  function syncFromRanges(changed) {
    const min = minRange.value;
    const max = maxRange.value;
    const normalized = normalizeValues(min, max, changed);
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

  function resetFilters() {
    if (brandSelect) brandSelect.selectedIndex = 0;
    if (modelSelect) modelSelect.selectedIndex = 0;
    if (sortSelect) sortSelect.selectedIndex = 0;

    updateUI(MIN_LIMIT, MAX_LIMIT);
  }

  minRange.min = MIN_LIMIT;
  minRange.max = MAX_LIMIT;
  minRange.step = STEP;

  maxRange.min = MIN_LIMIT;
  maxRange.max = MAX_LIMIT;
  maxRange.step = STEP;

  minRange.addEventListener("input", () => syncFromRanges("min"));
  maxRange.addEventListener("input", () => syncFromRanges("max"));

  minInput.addEventListener("input", () => handleTyping(minInput));
  maxInput.addEventListener("input", () => handleTyping(maxInput));

  minInput.addEventListener("blur", () => syncFromInputs("min"));
  maxInput.addEventListener("blur", () => syncFromInputs("max"));

  minInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") syncFromInputs("min");
  });

  maxInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") syncFromInputs("max");
  });

  if (resetFiltersBtn) {
    resetFiltersBtn.addEventListener("click", resetFilters);
  }

  updateUI(MIN_LIMIT, MAX_LIMIT);

  requestAnimationFrame(() => {
    updateSliderFill();
  });
}

/* ================= PHONE INPUT ================= */

function initPhoneInput() {
  const phoneInput = document.getElementById("phoneInput");
  if (!phoneInput) return;

  const PREFIX = "+36 ";
  const PREFIX_LENGTH = PREFIX.length;

  function getDigits(value) {
    let digits = value.replace(/\D/g, "");

    if (digits.startsWith("36")) {
      digits = digits.slice(2);
    }

    return digits.slice(0, 9);
  }

  function formatPhone(value) {
    const digits = getDigits(value);
    let formatted = PREFIX;

    if (digits.length > 0) {
      formatted += digits.slice(0, 2);
    }
    if (digits.length > 2) {
      formatted += " " + digits.slice(2, 5);
    }
    if (digits.length > 5) {
      formatted += " " + digits.slice(5, 9);
    }

    return formatted;
  }

  function moveCaretToEnd() {
    requestAnimationFrame(() => {
      const end = phoneInput.value.length;
      phoneInput.setSelectionRange(end, end);
    });
  }

  function clampSelection() {
    requestAnimationFrame(() => {
      let start = phoneInput.selectionStart ?? PREFIX_LENGTH;
      let end = phoneInput.selectionEnd ?? PREFIX_LENGTH;

      if (start < PREFIX_LENGTH) start = PREFIX_LENGTH;
      if (end < PREFIX_LENGTH) end = PREFIX_LENGTH;

      phoneInput.setSelectionRange(start, end);
    });
  }

  function selectEditableOnly() {
    requestAnimationFrame(() => {
      phoneInput.setSelectionRange(PREFIX_LENGTH, phoneInput.value.length);
    });
  }

  if (!phoneInput.value.trim()) {
    phoneInput.value = PREFIX;
  } else {
    phoneInput.value = formatPhone(phoneInput.value);
  }

  phoneInput.addEventListener("focus", () => {
    if (!phoneInput.value.startsWith(PREFIX)) {
      phoneInput.value = PREFIX;
    }
    moveCaretToEnd();
  });

  phoneInput.addEventListener("click", clampSelection);
  phoneInput.addEventListener("mouseup", clampSelection);
  phoneInput.addEventListener("select", clampSelection);

  phoneInput.addEventListener("dblclick", (e) => {
    e.preventDefault();
    selectEditableOnly();
  });

  phoneInput.addEventListener("keydown", (e) => {
    const allowedControlKeys = [
      "Tab",
      "ArrowLeft",
      "ArrowRight",
      "ArrowUp",
      "ArrowDown",
      "Home",
      "End",
    ];

    if (e.ctrlKey || e.metaKey) return;

    if (allowedControlKeys.includes(e.key)) {
      requestAnimationFrame(() => {
        if ((phoneInput.selectionStart ?? 0) < PREFIX_LENGTH) {
          phoneInput.setSelectionRange(PREFIX_LENGTH, PREFIX_LENGTH);
        }
      });
      return;
    }

    if (e.key === "Backspace") {
      if ((phoneInput.selectionStart ?? 0) <= PREFIX_LENGTH) {
        e.preventDefault();
      }
      return;
    }

    if (e.key === "Delete") {
      if ((phoneInput.selectionStart ?? 0) < PREFIX_LENGTH) {
        e.preventDefault();
      }
      return;
    }

    if (!/^\d$/.test(e.key)) {
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
      "text"
    );
    phoneInput.value = formatPhone(pastedText);
    moveCaretToEnd();
  });

  phoneInput.addEventListener("dragstart", (e) => {
    const start = phoneInput.selectionStart ?? 0;
    if (start < PREFIX_LENGTH) {
      e.preventDefault();
    }
  });
}

/* ================= CLICK OUTSIDE MENUS ================= */

function initOutsideClickHandlers() {
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

    if (
      profileWrapper &&
      profileMenu &&
      !profileWrapper.contains(e.target) &&
      !profileMenu.contains(e.target)
    ) {
      profileMenu.style.display = "none";
    }
  });
}

/* ================= BROWSER BACK / FORWARD ================= */

function initHistoryHandling() {
  window.addEventListener("popstate", (event) => {
    const sectionName = event.state?.section || "home";
    showSectionByName(sectionName, false);
  });
}

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
  initCategoryToggle();
  initPageSwitching();
  initViewToggle();
  initThemeButtons();
  initPriceFilters();
  initPhoneInput();
  initOutsideClickHandlers();
  initHistoryHandling();

  requestAnimationFrame(() => {
    if (initialSection === "search") {
      refreshPriceSliderUI();
    }
  });
});
