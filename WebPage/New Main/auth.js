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
}

function fillProfileSection(user) {
  const email = document.getElementById("profileEmail");
  const address = document.getElementById("profileAddress");
  const phone = document.getElementById("profilePhone");

  if (email) email.textContent = user.email || "-";
  if (address) address.textContent = user.address || "-";
  if (phone) phone.textContent = user.phone || "-";
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

  submitRegisterBtn?.addEventListener("click", async () => {
    const email = document.getElementById("registerEmail")?.value.trim();
    const password = document.getElementById("registerPassword")?.value.trim();
    const address = document.getElementById("registerAddress")?.value.trim();
    const phone = document.getElementById("phoneInput")?.value.trim();

    try {
        await apiRegisterUser(email, password, address, phone);
        alert("Sikeres regisztráció!");
        showSectionByName("login");
    } catch (error) {
      console.error(error);
      alert("A regisztráció nem sikerült.");
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
}