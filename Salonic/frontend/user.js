document.addEventListener("DOMContentLoaded", () => {
    loadInfos();
});

function loadInfos() {
    if (localStorage.getItem("email")) {
        document.getElementById("userName").textContent = localStorage.getItem("name");
        document.getElementById("userEmail").textContent = localStorage.getItem("email");
        document.getElementById("userId").textContent = localStorage.getItem("id");
    }
}

function logout(){
    localStorage.clear();
    window.location.href = "index.html"

}
