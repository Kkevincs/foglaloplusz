document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("owner-register-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const name = form.querySelector("#OwnerRegName").value;
        const email = form.querySelector("#OwnerRegEmail").value;
        const password = form.querySelector("#OwnerRegPassword").value;
        const user = {
            name: name,
            email: email,
            pwd: password,
        };
        const { ok } = await RegisterOwner(user);
        if (ok) {
            window.location.href = "/maker";
        }
    });
});
