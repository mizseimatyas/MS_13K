let port = 7171

//Dark/Light Button
const btn = document.getElementById("themeBtn");

btn.addEventListener("click", function () {
    document.body.classList.toggle("dark-mode");
    if (btn.classList.contains("btn-light")) {
        btn.classList.remove("btn-light");
        btn.classList.add("btn-dark")
    } 
    else {
        btn.classList.remove("btn-dark");
        btn.classList.add("btn-light");
    }
});