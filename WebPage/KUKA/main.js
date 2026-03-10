let port = 7171;

function toggleMenu() {
  const menu = document.getElementById("mobileMenu");
  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

window.addEventListener("resize", () => {
  const menu = document.getElementById("mobileMenu");
  const desktopMenu = document.querySelector(".desktop-menu");
  const hamburger = document.querySelector(".hamburger");
  if (window.innerWidth > 768) {
    desktopMenu.style.display = "flex";
    hamburger.style.display = "none";
    menu.style.display = "none";
  } else {
    desktopMenu.style.display = "none";
    hamburger.style.display = "block";
  }
});

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

function toggleMenu() {
  const menu = document.getElementById("mobileMenu");
  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

function toggleProfileMenu() {
  const menu = document.getElementById("profileMenu");
  menu.style.display = menu.style.display === "block" ? "none" : "block";
}

document.addEventListener("click", function (e) {
  const mobileMenu = document.getElementById("mobileMenu");
  const hamburger = document.querySelector(".hamburger");

  const profileMenu = document.getElementById("profileMenu");
  const profileWrapper = document.querySelector(".profile-dropdown-wrapper");

  if (!hamburger.contains(e.target) && !mobileMenu.contains(e.target)) {
    mobileMenu.style.display = "none";
  }

  if (!profileWrapper.contains(e.target)) {
    profileMenu.style.display = "none";
  }
});
