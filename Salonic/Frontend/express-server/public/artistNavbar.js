const params = new URLSearchParams(window.location.search);
const salonId = params.get("salonId") || params.get("salonid");
const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";

function resolveRole() {
    if (typeof window.getRoleFromToken === "function") {
        return window.getRoleFromToken();
    }
    return null;
}

const artistNavLinks = [
    ["logoLink", "/Salon/Artist"],
    ["homeLink", "/Salon/Artist"],
    ["servicesLink", "/Salon/Artist/Services"],
    ["userLink", "/Salon/Artist/User"],
    ["addServiceLink", "/Salon/Artist/AddService"],
    ["workingHoursLink", "/Salon/Artist/Hours"],
    ["loginLink", "/Salon/Artist/Login"]
];

function applyArtistNavbarState() {
    artistNavLinks.forEach(([elementId, href]) => {
        const el = document.getElementById(elementId);
        if (el) el.href = `${href}${salonQuery}`;
    });

    const isArtistLoggedIn = resolveRole() === "artist";
    const linksForGuests = ["homeLink", "loginLink"];
    const linksForArtists = ["homeLink", "servicesLink", "userLink", "addServiceLink", "workingHoursLink"];

    const loginLink = document.getElementById("loginLink");
    if (loginLink && isArtistLoggedIn) {
        loginLink.textContent = "Kijelentkezés";
        loginLink.href = `/Salon/Artist/Login${salonQuery}`;
        loginLink.onclick = function (e) {
            e.preventDefault();
            localStorage.setItem("token", "");
            window.location.assign(`/Salon/Artist/Login${salonQuery}`);
        };
    } else if (loginLink) {
        loginLink.textContent = "Bejelentkezés";
        loginLink.href = `/Salon/Artist/Login${salonQuery}`;
        loginLink.onclick = null;
    }

    linksForGuests.forEach((id) => {
        const el = document.getElementById(id);
        if (!el) return;
        if (id === "loginLink") {
            el.style.display = "";
            return;
        }
        el.style.display = isArtistLoggedIn ? "none" : "";
    });

    linksForArtists.forEach((id) => {
        const el = document.getElementById(id);
        if (!el) return;
        el.style.display = isArtistLoggedIn ? "" : "none";
    });
}

function wireHoursPageLinks() {
    const toSet = document.getElementById("artist-hours-to-set");
    if (toSet) toSet.href = `/Salon/Artist/SetWorkingHours${salonQuery}`;
    const toView = document.getElementById("artist-hours-to-view");
    if (toView) toView.href = `/Salon/Artist/Hours${salonQuery}`;
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
        el.href = `${url.pathname}${url.search}${url.hash}`;
    });
}

applyArtistNavbarState();
wireHoursPageLinks();
preserveSalonIdOnSalonLinks();
document.addEventListener("DOMContentLoaded", () => {
    applyArtistNavbarState();
    wireHoursPageLinks();
    preserveSalonIdOnSalonLinks();
});
