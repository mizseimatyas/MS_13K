function toggleMenu() {
  const menu = document.getElementById("mobileMenu");
  if (!menu) return;
  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

function toggleProfileMenu() {
  const menu = document.getElementById("profileMenu");
  if (!menu) return;
  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

document.addEventListener("click", function (e) {
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

document.addEventListener("DOMContentLoaded", () => {
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

    if (changed === "min") {
      if (min >= max) {
        max = min + MIN_GAP;
      }
    }

    if (changed === "max") {
      if (max <= min) {
        min = max - MIN_GAP;
      }
    }

    min = clamp(min, MIN_LIMIT, MAX_LIMIT - MIN_GAP);
    max = clamp(max, MIN_LIMIT + MIN_GAP, MAX_LIMIT);

    if (max - min < MIN_GAP) {
      if (changed === "min") {
        max = min + MIN_GAP;
      } else {
        min = max - MIN_GAP;
      }
    }

    updateUI(min, max);
  }

  function syncFromInputs(changed) {
    let min =
      minInput.value.trim() === "" ? MIN_LIMIT : parsePrice(minInput.value);
    let max =
      maxInput.value.trim() === "" ? MAX_LIMIT : parsePrice(maxInput.value);

    min = clamp(min, MIN_LIMIT, MAX_LIMIT);
    max = clamp(max, MIN_LIMIT, MAX_LIMIT);

    if (changed === "min") {
      if (min >= max) {
        max = min + MIN_GAP;
      }
    }

    if (changed === "max") {
      if (max <= min) {
        min = max - MIN_GAP;
      }
    }

    min = clamp(min, MIN_LIMIT, MAX_LIMIT - MIN_GAP);
    max = clamp(max, MIN_LIMIT + MIN_GAP, MAX_LIMIT);

    if (max - min < MIN_GAP) {
      if (changed === "min") {
        max = min + MIN_GAP;
      } else {
        min = max - MIN_GAP;
      }
    }

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

  minInput.addEventListener("blur", () => syncFromInputs("min"));
  maxInput.addEventListener("blur", () => syncFromInputs("max"));

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
});
