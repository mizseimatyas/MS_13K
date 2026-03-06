let port = 7171

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
    } 
    else {
        desktopMenu.style.display = "none";
        hamburger.style.display = "block";
    }
});