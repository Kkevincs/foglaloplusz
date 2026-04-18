document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

    document.getElementById("artist-login-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const email = form.querySelector("#ArtistEmail").value;
        const password = form.querySelector("#ArtistPassword").value;
        const role = "artist";

        const artist = {
            email: email,
            role: role,
            pwd: password,
        };
        const token = await Login(artist);
        if (!token) return;
        localStorage.setItem("token", token);
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get("salonId") || params.get("salonid");
        const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
        window.location.assign(`/Salon/Artist/User${salonQuery}`);
    });
});
