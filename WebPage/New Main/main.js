window.addEventListener("resize", () => {
  handleResponsiveMenu();
  const visibleSection = document.querySelector(".page-section:not(.d-none)");
  updateBodyScroll(visibleSection);
});

document.addEventListener("DOMContentLoaded", async () => {
  const sections = getSections();
  Object.values(sections).forEach((section) => section?.classList.add("d-none"));

  const hash = window.location.hash.replace("#", "");
  const validSections = ["home", "search", "register", "login", "profile", "productDetail"];
  const initialSection = validSections.includes(hash) ? hash : "home";

  showSectionByName(initialSection, false);
  history.replaceState({ section: initialSection }, "", `#${initialSection}`);

  document.getElementById("hamburgerBtn")?.addEventListener("click", toggleMenu);
  document.getElementById("homeLogo")?.addEventListener("click", async () => {
    const searchInput = document.getElementById("searchInput");
    if (searchInput) searchInput.value = "";
    showSectionByName("home");
    await loadHomeProducts();
  });
  document.getElementById("registerBtn")?.addEventListener("click", () => showSectionByName("register"));
  document.getElementById("mobileRegisterBtn")?.addEventListener("click", () => showSectionByName("register"));
  document.getElementById("loginBtn")?.addEventListener("click", () => showSectionByName("login"));
  document.getElementById("mobileLoginBtn")?.addEventListener("click", () => showSectionByName("login"));

  handleResponsiveMenu();
  initSidebarButtons();
  initCategoryToggle();
  initProductDetailOpening();
  initViewToggle();
  initThemeButtons();
  initPriceFilters();
  initPhoneInput();
  initOutsideClickHandlers();
  initHistoryHandling();
  initSearch();
  initAuth();

  await loadAllProducts();
  await loadCategories();
  await loadHomeProducts();
  await checkAuthState();

  requestAnimationFrame(() => {
    if (initialSection === "search") refreshPriceSliderUI();
  });
});
