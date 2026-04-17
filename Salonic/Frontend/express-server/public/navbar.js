const params = new URLSearchParams(window.location.search);
const salonId = params.get("salonId") || params.get("salonid");
const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
const artistIdFromUrl = params.get("artistId") || params.get("artistid");

function buildGuestServicesHref() {
    if (!salonId) return "/Salon/Services";
    let q = `?salonId=${encodeURIComponent(salonId)}`;
    if (artistIdFromUrl) q += `&artistId=${encodeURIComponent(artistIdFromUrl)}`;
    return `/Salon/Services${q}`;
}

function parseJwtPayloadLocal(token) {
    const part = token.split(".")[1];
    if (!part) return null;
    try {
        const json = atob(part.replace(/-/g, "+").replace(/_/g, "/"));
        return JSON.parse(json);
    } catch (_) {
        return null;
    }
}

function resolveRole() {
    if (typeof window.getRoleFromToken === "function") {
        return window.getRoleFromToken();
    }
    const token = localStorage.getItem("token");
    if (!token) return null;
    return parseJwtPayloadLocal(token)?.role ?? null;
}

/** Csak a „Profil” linket rejtsük el, ha nincs vendég bejelentkezés — a többi mindig látható. */
const guestNavLinksWhenLoggedIn = ["userLink"];

function setGuestNavItemVisible(anchorId, visible) {
    const el = document.getElementById(anchorId);
    if (!el) return;
    const li = el.closest("li");
    (li || el).style.display = visible ? "" : "none";
}

function applyGuestNavbarState() {
    const logo = document.getElementById("logoLink");
    if (logo) logo.href = `/Salon${salonQuery}`;

    const home = document.getElementById("homeLink");
    if (home) home.href = `/Salon${salonQuery}`;
    const artists = document.getElementById("artistsLink");
    if (artists) artists.href = `/Salon/Artists${salonQuery}`;
    const services = document.getElementById("servicesLink");
    if (services) services.href = buildGuestServicesHref();
    const user = document.getElementById("userLink");
    if (user) user.href = `/Salon/GuestInfo${salonQuery}`;

    const isGuestLoggedIn = resolveRole() === "guest";

    guestNavLinksWhenLoggedIn.forEach((id) => {
        setGuestNavItemVisible(id, isGuestLoggedIn);
    });

    const loginLink = document.getElementById("loginLink");
    if (loginLink) {
        if (isGuestLoggedIn) {
            loginLink.textContent = "Kijelentkezés";
            loginLink.href = `/Salon/Login${salonQuery}`;
            loginLink.onclick = function (e) {
                e.preventDefault();
                localStorage.setItem("token", "");
                window.location.assign(`/Salon/Login${salonQuery}`);
            };
        } else {
            loginLink.textContent = "Bejelentkezés";
            loginLink.href = `/Salon/Login${salonQuery}`;
            loginLink.onclick = null;
        }
        setGuestNavItemVisible("loginLink", true);
    }

    preserveSalonIdOnSalonLinks();
}

function preserveSalonIdOnSalonLinks() {
    if (!salonId) return;
    document.querySelectorAll('a[href^="/Salon"]').forEach((el) => {
        const rawHref = el.getAttribute("href");
        if (!rawHref || rawHref.startsWith("/Salon/AppointmentCreated")) return;
        const url = new URL(rawHref, window.location.origin);
        if (!url.searchParams.get("salonId")) {
            url.searchParams.set("salonId", salonId);
        }
        if (
            artistIdFromUrl &&
            url.pathname === "/Salon/Services" &&
            !url.searchParams.get("artistId") &&
            !url.searchParams.get("artistid")
        ) {
            url.searchParams.set("artistId", artistIdFromUrl);
        }
        el.href = `${url.pathname}${url.search}${url.hash}`;
    });
}

applyGuestNavbarState();
document.addEventListener("DOMContentLoaded", () => {
    applyGuestNavbarState();
});
