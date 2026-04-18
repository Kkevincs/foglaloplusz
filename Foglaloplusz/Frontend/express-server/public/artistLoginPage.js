document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

    document.getElementById("artist-login-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const email = form.querySelector("#ArtistEmail").value;
        const password = form.querySelector("#ArtistPassword").value;
        const role = "artist";

        const params = new URLSearchParams(window.location.search);
        const salonIdRaw = params.get("salonId") || params.get("salonid");
        const salonIdNum = salonIdRaw ? Number.parseInt(String(salonIdRaw).trim(), 10) : NaN;

        const artist = {
            email: email,
            role: role,
            pwd: password,
        };
        if (Number.isFinite(salonIdNum) && salonIdNum > 0) {
            artist.salonId = salonIdNum;
        }
        const token = await Login(artist);
        if (!token) return;
        localStorage.setItem("token", token);
        const salonQuery = salonIdRaw ? `?salonId=${encodeURIComponent(salonIdRaw)}` : "";
        window.location.assign(`/Salon/Artist/User${salonQuery}`);
    });
});
