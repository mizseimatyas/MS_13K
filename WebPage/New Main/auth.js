let currentUser = null;

function setLoggedOutUI() {
  currentUser = null;

  const profileMenu = document.getElementById("profileMenu");
  const loginBtn = document.getElementById("loginBtn");
  const registerBtn = document.getElementById("registerBtn");
  const mobileLoginBtn = document.getElementById("mobileLoginBtn");
  const mobileRegisterBtn = document.getElementById("mobileRegisterBtn");
  const mobileProfileBtn = document.getElementById("mobileProfileBtn");

  if (profileMenu) profileMenu.style.display = "none";
  if (loginBtn) loginBtn.style.display = "block";
  if (registerBtn) registerBtn.style.display = "block";
  if (mobileLoginBtn) mobileLoginBtn.style.display = "block";
  if (mobileRegisterBtn) mobileRegisterBtn.style.display = "block";
  if (mobileProfileBtn) mobileProfileBtn.classList.add("d-none");
}

function setLoggedInUI(user) {
  currentUser = user;

  const loginBtn = document.getElementById("loginBtn");
  const registerBtn = document.getElementById("registerBtn");
  const mobileLoginBtn = document.getElementById("mobileLoginBtn");
  const mobileRegisterBtn = document.getElementById("mobileRegisterBtn");
  const mobileProfileBtn = document.getElementById("mobileProfileBtn");

  if (loginBtn) loginBtn.style.display = "none";
  if (registerBtn) registerBtn.style.display = "none";
  if (mobileLoginBtn) mobileLoginBtn.style.display = "none";
  if (mobileRegisterBtn) mobileRegisterBtn.style.display = "none";
  if (mobileProfileBtn) mobileProfileBtn.classList.remove("d-none");

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
  const submitRegisterBtn = document.getElementById("submitRegisterBtn");
  const submitLoginBtn = document.getElementById("submitLoginBtn");
  const logoutBtn = document.getElementById("logoutBtn");
  const profileBtn = document.getElementById("profileBtn");
  const mobileProfileBtn = document.getElementById("mobileProfileBtn");
  const editProfileBtn = document.getElementById("editProfileBtn");
  const saveProfileBtn = document.getElementById("saveProfileBtn");

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
      setLoggedOutUI();
      showSectionByName("home");
    } catch (error) {
      console.error(error);
      alert("A kijelentkezés nem sikerült.");
    }
  });

  profileBtn?.addEventListener("click", () => {
    if (currentUser) {
      showSectionByName("profile");
      return;
    }

    toggleProfileMenu();
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
}
