let currentUser = null;
window.isUserLoggedIn = function () {
  return !!currentUser;
};

window.getCurrentUser = function () {
  return currentUser;
};

const cartBtn = document.getElementById("cartBtn");
const mobileCartBtn = document.getElementById("mobileCartBtn");
const mobileOrdersBtn = document.getElementById("mobileOrdersBtn");
const loggedOutProfileMenu = document.getElementById("loggedOutProfileMenu");
const loggedInProfileMenu = document.getElementById("loggedInProfileMenu");
const editProfileBtn = document.getElementById("editProfileBtn");
const saveProfileBtn = document.getElementById("saveProfileBtn");

function setLoggedOutUI() {
  currentUser = null;

  const profileMenu = document.getElementById("profileMenu");
  const loginBtn = document.getElementById("loginBtn");
  const registerBtn = document.getElementById("registerBtn");
  const mobileLoginBtn = document.getElementById("mobileLoginBtn");
  const mobileRegisterBtn = document.getElementById("mobileRegisterBtn");
  const mobileProfileBtn = document.getElementById("mobileProfileBtn");

  profileMenu?.classList.remove("show");

  if (loginBtn) loginBtn.style.display = "block";
  if (registerBtn) registerBtn.style.display = "block";
  if (mobileLoginBtn) mobileLoginBtn.style.display = "block";
  if (mobileRegisterBtn) mobileRegisterBtn.style.display = "block";

  if (cartBtn) cartBtn.classList.add("d-none");
  if (mobileCartBtn) mobileCartBtn.classList.add("d-none");

  if (mobileProfileBtn) mobileProfileBtn.classList.add("d-none");
  if (mobileOrdersBtn) mobileOrdersBtn.classList.add("d-none");

  if (loggedOutProfileMenu) loggedOutProfileMenu.classList.remove("d-none");
  if (loggedInProfileMenu) loggedInProfileMenu.classList.add("d-none");
}

function setLoggedInUI(user) {
  currentUser = user;

  const profileMenu = document.getElementById("profileMenu");
  const loginBtn = document.getElementById("loginBtn");
  const registerBtn = document.getElementById("registerBtn");
  const mobileLoginBtn = document.getElementById("mobileLoginBtn");
  const mobileRegisterBtn = document.getElementById("mobileRegisterBtn");
  const mobileProfileBtn = document.getElementById("mobileProfileBtn");

  profileMenu?.classList.remove("show");

  if (loginBtn) loginBtn.style.display = "none";
  if (registerBtn) registerBtn.style.display = "none";
  if (mobileLoginBtn) mobileLoginBtn.style.display = "none";
  if (mobileRegisterBtn) mobileRegisterBtn.style.display = "none";

  if (cartBtn) cartBtn.classList.remove("d-none");
  if (mobileCartBtn) mobileCartBtn.classList.remove("d-none");

  if (mobileProfileBtn) mobileProfileBtn.classList.remove("d-none");
  if (mobileOrdersBtn) mobileOrdersBtn.classList.remove("d-none");

  if (loggedOutProfileMenu) loggedOutProfileMenu.classList.add("d-none");
  if (loggedInProfileMenu) loggedInProfileMenu.classList.remove("d-none");

  fillProfileSection(user);
  toggleProfileEditMode(false);
}

function fillProfileSection(user) {
  const name = document.getElementById("profileName");
  const email = document.getElementById("profileEmail");
  const phone = document.getElementById("profilePhone");
  const city = document.getElementById("profileCity");
  const zip = document.getElementById("profileZip");
  const address = document.getElementById("profileAddress");

  if (name) name.value = user.name || "";
  if (email) email.value = user.email || "";
  if (phone) phone.value = user.phone || "";
  if (city) city.value = user.city || "";
  if (zip) zip.value = user.zipCode || "";
  if (address) address.value = user.address || "";
}

function toggleProfileEditMode(editMode) {
  const editableIds = [
    "profileName",
    "profileEmail",
    "profilePhone",
    "profileCity",
    "profileZip",
    "profileAddress",
  ];

  editableIds.forEach((id) => {
    const input = document.getElementById(id);
    if (input) input.disabled = !editMode;
  });

  const editBtn = document.getElementById("editProfileBtn");
  const saveBtn = document.getElementById("saveProfileBtn");

  if (editBtn) editBtn.classList.toggle("d-none", editMode);
  if (saveBtn) saveBtn.classList.toggle("d-none", !editMode);
}

async function checkAuthState() {
  try {
    const user = await apiGetCurrentUser();

    if (user) {
      setLoggedInUI(user);
    } else {
      setLoggedOutUI();
    }
  } catch (error) {
    console.error("Auth state hiba:", error);
    setLoggedOutUI();
  }
}

function initAuth() {
  const logoutBtn = document.getElementById("logoutBtn");
  const profileBtn = document.getElementById("profileBtn");
  const mobileProfileBtn = document.getElementById("mobileProfileBtn");
  const profileMenuProfileBtn = document.getElementById(
    "profileMenuProfileBtn",
  );
  const profileMenuOrdersBtn = document.getElementById("profileMenuOrdersBtn");

  submitRegisterBtn?.addEventListener("click", async () => {
    const email = document.getElementById("registerEmail")?.value.trim();
    const password = document.getElementById("registerPassword")?.value.trim();
    const address = document.getElementById("registerAddress")?.value.trim();
    const phone = document.getElementById("phoneInput")?.value.trim();

    try {
      await apiRegisterUser(email, password, address, phone);
      alert(
        "Sikeres regisztráció! Bejelentkezés után kérlek töltsd ki a nevedet, várost és irányítószámot a profilodban.",
      );
      showSectionByName("login");
    } catch (error) {
      console.error(error);
      alert(error.message || "A regisztráció nem sikerült.");
    }
  });

  submitLoginBtn?.addEventListener("click", async () => {
    const email = document.getElementById("loginEmail")?.value.trim();
    const password = document.getElementById("loginPassword")?.value.trim();

    try {
      await apiLoginUser(email, password);
      const user = await apiGetCurrentUser();
      if (user) {
        setLoggedInUI(user);
        showSectionByName("profile");
      }
    } catch (error) {
      console.error(error);
      alert("Hibás bejelentkezési adatok.");
    }
  });

  logoutBtn?.addEventListener("click", async () => {
    try {
      await apiLogoutUser();
      localStorage.removeItem("woltmarket_cart");
      window.renderCartDropdown?.();
      setLoggedOutUI();
      showSectionByName("home");
    } catch (error) {
      console.error(error);
      alert("A kijelentkezés nem sikerült.");
    }
  });

  profileBtn?.addEventListener("click", (e) => {
    e.stopPropagation();

    closeCartMenu();
    profileMenu?.classList.toggle("show");
  });

  document.addEventListener("click", (event) => {
    const profileMenu = document.getElementById("profileMenu");
    const wrapper = document.querySelector(".profile-dropdown-wrapper");

    if (wrapper && !wrapper.contains(event.target)) {
      profileMenu?.classList.remove("show");
    }
  });

  mobileProfileBtn?.addEventListener("click", () => {
    if (currentUser) {
      showSectionByName("profile");
      closeMobileMenu();
    }
  });

  editProfileBtn?.addEventListener("click", () => {
    toggleProfileEditMode(true);
  });

  saveProfileBtn?.addEventListener("click", async () => {
    try {
      const payload = {
        name: document.getElementById("profileName")?.value?.trim() || "",
        email: document.getElementById("profileEmail")?.value?.trim() || "",
        phone: document.getElementById("profilePhone")?.value?.trim() || "",
        city: document.getElementById("profileCity")?.value?.trim() || "",
        zipCode: document.getElementById("profileZip")?.value?.trim() || "",
        address: document.getElementById("profileAddress")?.value?.trim() || "",
      };

      await apiUpdateProfile(payload);

      currentUser = {
        ...currentUser,
        ...payload,
      };

      toggleProfileEditMode(false);
      alert("Profil sikeresen mentve.");
    } catch (error) {
      console.error(error);
      alert(error.message || "A profil mentése nem sikerült.");
    }
  });

  profileMenuProfileBtn?.addEventListener("click", () => {
    const profileMenu = document.getElementById("profileMenu");
    profileMenu?.classList.remove("show");
    showSectionByName("profile");
  });

  profileMenuOrdersBtn?.addEventListener("click", () => {
    const profileMenu = document.getElementById("profileMenu");
    profileMenu?.classList.remove("show");
    showSectionByName("orders");
  });

  mobileOrdersBtn?.addEventListener("click", () => {
    showSectionByName("orders");
    closeMobileMenu();
  });

  mobileCartBtn?.addEventListener("click", (e) => {
    e.preventDefault();
    e.stopPropagation();

    closeMobileMenu();
    closeProfileMenu();
    renderCartDropdown();

    const cartMenu = document.getElementById("cartMenu");
    cartMenu?.classList.add("show");
  });
}
