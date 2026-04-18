document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("owner-login-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const email = form.querySelector("#OwnerLoginEmail").value;
        const password = form.querySelector("#OwnerLoginPassword").value;
        const user = {
            email: email,
            role: "owner",
            pwd: password,
        };
        const { ok, token } = await LoginWithStatus(user);
        if (ok && token) {
            localStorage.setItem("token", token);
            window.location.href = "/ownerhomepage";
        }
    });
});
