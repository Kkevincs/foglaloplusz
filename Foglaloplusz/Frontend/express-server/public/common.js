/**
 * Shared UI and data helpers for Salonic front-end. API calls live in api.js.
 */

function enforceOwnerRoleOrRedirect() {
    const ownerOnlyPaths = ["/maker", "/owneruserpage", "/ownersalonedit", "/salondashboard", "/salondashboard/schedule", "/ownerartistregister"];
    if (!ownerOnlyPaths.includes(window.location.pathname)) return true;
    const token = localStorage.getItem("token");
    let role = null;
    if (token) {
        try {
            role = parseJwtPayload(token)?.role ?? null;
        } catch (_) {
            role = null;
        }
    }
    if (role !== "owner") {
        window.location.assign("/ownerlogin");
        return false;
    }
    return true;
}

/** Guest /Salon/* routes: validate salon, apply theme and logo. */
async function initSalonGuestPublicShell() {
    const path = window.location.pathname;
    if (path !== "/Salon" && !path.startsWith("/Salon/")) return;
    const guestParams = new URLSearchParams(window.location.search);
    const guestSalonId = guestParams.get("salonId") || guestParams.get("salonid");
    if (!guestSalonId) {
        window.location.assign("/ownerregister");
        return;
    }
    if (!(await assertPublicSalonExistsOrRedirect(guestSalonId))) return;
    await applySalonThemeForPublicPages();
    await applySalonPersonalLogo();
}

let salonicLogoutModalEl = null;

function ensureSalonicLogoutModal() {
    if (salonicLogoutModalEl && document.body.contains(salonicLogoutModalEl)) {
        return salonicLogoutModalEl;
    }
    const existing = document.getElementById("salonicLogoutConfirmModal");
    if (existing) {
        salonicLogoutModalEl = existing;
        return salonicLogoutModalEl;
    }
    const tpl = document.createElement("template");
    tpl.innerHTML = `
<div class="modal fade" id="salonicLogoutConfirmModal" tabindex="-1" aria-labelledby="salonicLogoutConfirmTitle" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h2 class="modal-title h5" id="salonicLogoutConfirmTitle">Kijelentkezés</h2>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Bezárás"></button>
      </div>
      <div class="modal-body">Biztosan ki szeretnél jelentkezni?</div>
      <div class="modal-footer">
        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Mégse</button>
        <button type="button" class="btn btn-danger" id="salonicLogoutConfirmBtn">Kijelentkezés</button>
      </div>
    </div>
  </div>
</div>`.trim();
    salonicLogoutModalEl = tpl.content.firstElementChild;
    document.body.appendChild(salonicLogoutModalEl);

    const confirmBtn = salonicLogoutModalEl.querySelector("#salonicLogoutConfirmBtn");
    if (confirmBtn && !confirmBtn.dataset.salonicWired) {
        confirmBtn.dataset.salonicWired = "1";
        confirmBtn.addEventListener("click", () => {
            const href = salonicLogoutModalEl.dataset.logoutRedirect || "/ownerlogin";
            localStorage.setItem("token", "");
            window.location.assign(href);
        });
    }
    return salonicLogoutModalEl;
}

/**
 * Megerősítő modál kijelentkezés előtt; megerősítéskor törli a tokent és átirányít.
 * @param {string} redirectHref pl. /ownerlogin vagy /Salon/Login?salonId=…
 */
function confirmLogoutThen(redirectHref) {
    const href = typeof redirectHref === "string" && redirectHref.trim() ? redirectHref.trim() : "/ownerlogin";
    const modalEl = ensureSalonicLogoutModal();
    modalEl.dataset.logoutRedirect = href;
    if (typeof bootstrap !== "undefined" && bootstrap.Modal) {
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    } else if (window.confirm("Biztosan ki szeretnél jelentkezni?")) {
        localStorage.setItem("token", "");
        window.location.assign(href);
    }
}

window.confirmLogoutThen = confirmLogoutThen;

document.addEventListener("DOMContentLoaded", async () => {
    document.addEventListener("click", (e) => {
        const logoutEl = e.target.closest("[data-salonic-logout-href]");
        if (!logoutEl) return;
        e.preventDefault();
        const target = logoutEl.getAttribute("data-salonic-logout-href");
        if (target) confirmLogoutThen(target);
    });
    document.querySelectorAll(".salonic-navbar .nav-links a, .salonic-navbar .nav-link").forEach((a) => {
        a.addEventListener("click", (e) => {
            if (a.getAttribute("href") === "#") e.preventDefault();
        });
    });
    const path = window.location.pathname;
    const sp = new URLSearchParams(window.location.search);
    if (!(sp.get("salonId") || sp.get("salonid"))) return;
    const applyThemeHere =
        path === "/Salon" ||
        path.startsWith("/Salon/") ||
        path === "/salondashboard" ||
        path === "/salondashboard/schedule" ||
        path === "/ownerartistregister" ||
        path === "/ownersalonedit";
    if (applyThemeHere) {
        await applySalonThemeForPublicPages();
        await applySalonPersonalLogo();
    }
    const isSalonGuestPath =
        path === "/Salon" || path === "/Salon/" || path.startsWith("/Salon/");
    if (isSalonGuestPath) {
        ensureSalonSharedFooterInDom();
        const isHomeSalon = path === "/Salon" || path === "/Salon/";
        if (!isHomeSalon) {
            try {
                await fillSalonSharedFooterFromApi();
            } catch (e) {
                console.warn("Salon shared footer:", e);
            }
        }
    }
});

function parseJwtPayload(token) {
    const part = token.split('.')[1];
    if (!part) return null;
    const json = atob(part.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json);
  }

function getRoleFromToken() {
    const token = localStorage.getItem("token");
    if (!token) return null;
    try {
        return parseJwtPayload(token)?.role ?? null;
    } catch (_) {
        return null;
    }
}

window.getRoleFromToken = getRoleFromToken;

/** Rejti a „Bejelentkezés vendégként” lábléc linket, ha már van bejelentkezett szerepkör. */
function syncFooterGuestLoginNavVisibility() {
    const loginEl = document.getElementById("footer-nav-login");
    if (!loginEl) return;
    const li = loginEl.closest("li");
    const hide = Boolean(getRoleFromToken());
    if (li) li.hidden = hide;
    else loginEl.hidden = hide;
}

window.syncFooterGuestLoginNavVisibility = syncFooterGuestLoginNavVisibility;

async function applySalonPersonalHomepage() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const pathNorm = (window.location.pathname || "/").replace(/\/$/, "") || "/";
    const isArtistSalonHome = pathNorm === "/Salon/Artist";
    const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
    document.getElementById('salon-name').textContent = salon.name;
    document.getElementById('salon-description').textContent = salon.description;

    const emailRaw = salon.email ?? salon.Email ?? '';
    const sq = salonId ? `?salonId=${encodeURIComponent(salonId)}` : '';
    fillSalonSharedFooterFromSalon(salon, salonId);
    if (isArtistSalonHome) {
        const footerNavSvc = document.getElementById("footer-nav-services");
        if (footerNavSvc) footerNavSvc.href = `/Salon/Artist/Services${sq}`;
    }
    const supportMail = document.getElementById('salon-support-email');
    if (supportMail && emailRaw) {
        supportMail.href = `mailto:${emailRaw}`;
        supportMail.textContent = emailRaw;
    }

    const guestLogin = document.getElementById('salon-guest-login-link');
    if (guestLogin) guestLogin.href = `/Salon/Login${sq}`;
    const artistLogin = document.getElementById('salon-artist-login-link');
    if (artistLogin) artistLogin.href = `/Salon/Artist/Login${sq}`;
    const services = document.getElementById('salon-link-services');
    if (services) {
        services.href = isArtistSalonHome ? `/Salon/Artist/Services${sq}` : `/Salon/Services${sq}`;
        if (isArtistSalonHome) {
            services.hidden = getRoleFromToken() !== 'artist';
        } else {
            services.hidden = false;
        }
    }
    const artists = document.getElementById('salon-link-artists');
    if (artists) artists.href = `/Salon/Artists${sq}`;
    const contact = document.getElementById('salon-link-contact');
    if (contact && emailRaw) contact.href = `mailto:${emailRaw}`;

    const artistGuestSite = document.getElementById('artist-public-guest-site-link');
    if (artistGuestSite) artistGuestSite.href = `/Salon${sq}`;
    const artistLoginPublic = document.getElementById('artist-public-artist-login-link');
    if (artistLoginPublic) artistLoginPublic.href = `/Salon/Artist/Login${sq}`;
    const ahServices = document.getElementById('artist-home-link-services');
    if (ahServices) ahServices.href = `/Salon/Artist/Services${sq}`;
    const ahProfile = document.getElementById('artist-home-link-profile');
    if (ahProfile) ahProfile.href = `/Salon/Artist/User${sq}`;
    const ahHours = document.getElementById('artist-home-link-hours');
    if (ahHours) ahHours.href = `/Salon/Artist/Hours${sq}`;
    const ahAdd = document.getElementById('artist-home-link-add');
    if (ahAdd) ahAdd.href = `/Salon/Artist/AddService${sq}`;

    const roleCard = document.querySelector('.salon-home-role');
    if (roleCard) {
        const mode = roleCard.getAttribute('data-hide-role-when') || 'any';
        const role = getRoleFromToken();
        if (mode === 'artist') {
            roleCard.hidden = role === 'artist';
        } else {
            roleCard.hidden = Boolean(role);
        }
    }
}
/** Same origin as api.js — uploaded salon logos are served by the API host. */
function getSalonPublicApiOrigin() {
    return typeof window !== "undefined" && window.SALONIC_API_BASE !== undefined && window.SALONIC_API_BASE !== null
        ? String(window.SALONIC_API_BASE)
        : "";
}

function resolvePublicSalonLogoUrl(salon) {
    const raw = salon?.logo ?? salon?.Logo ?? "";
    if (!raw || !String(raw).trim()) return "";
    const p = String(raw).trim();
    if (/^https?:\/\//i.test(p)) return p;
    const origin = getSalonPublicApiOrigin();
    return `${origin}${p.startsWith("/") ? p : `/${p}`}`;
}

function resolveSalonBrandLogoElement() {
    return (
        document.getElementById("logoLink") ||
        document.querySelector("nav a.logo") ||
        document.querySelector("header a.portal-brand") ||
        document.querySelector("a.portal-brand")
    );
}

async function applySalonPersonalLogo() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    if (!salonId) return;
    let salon;
    try {
        salon = await GetSalonBySalonId(localStorage.getItem("token"), salonId);
    } catch (_) {
        return;
    }
    if (!salon) return;
    const logoEl = resolveSalonBrandLogoElement();
    if (!logoEl) return;
    const name = salon.name ?? salon.Name ?? "";
    const logoUrl = resolvePublicSalonLogoUrl(salon);
    if (logoUrl) {
        logoEl.innerHTML = `<img src="${logoUrl}" alt="${escapeHtml(name)}">`;
    } else if (name) {
        // Megtartjuk az alap FoglalóPlusz wordmark képet; csak tooltip / akadálymentes név.
        logoEl.setAttribute("title", name);
        logoEl.setAttribute("aria-label", name);
    }
}

async function applyAppointmentCreatedPage() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get('salonId') || params.get('salonid');
    const sq = salonId ? `?salonId=${encodeURIComponent(salonId)}` : '';

    const home = document.getElementById('appointment-created-home');
    const guest = document.getElementById('appointment-created-guest');
    if (home) home.href = `/Salon${sq}`;
    if (guest) guest.href = `/Salon/GuestInfo${sq}`;

    const nameEl = document.getElementById('appointment-created-salon-name');
    const lineEl = document.getElementById('appointment-created-salon-line');
    if (!salonId || !nameEl) {
        if (lineEl) lineEl.hidden = true;
        return;
    }
    try {
        const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
        nameEl.textContent = salon.name;
        if (lineEl) lineEl.hidden = false;
    } catch (_) {
        if (lineEl) lineEl.hidden = true;
    }
}

function getCache() {
    return JSON.parse(localStorage.getItem("salonThemeById") || "{}");
}

function getSalonThemeCache() {
    return getCache();
}

function setSalonThemeCache(obj) {
    localStorage.setItem("salonThemeById", JSON.stringify(obj));
}

/** DB-ből jövő téma: legyen `theme-xyz`, hogy a body.theme-xyz változók érvényesüljenek */
function normalizeSalonThemeClass(raw) {
    let t = String(raw ?? "").trim();
    if (!t) return "";
    if (!t.startsWith("theme-")) {
        t = `theme-${t.replace(/^theme-?/i, "").replace(/\s+/g, "-")}`;
    }
    return t;
}

function applyTheme(theme) {
    const t = normalizeSalonThemeClass(theme);
    if (!t) return;
    const rest = document.body.className
        .split(/\s+/)
        .filter((cls) => cls && !cls.startsWith("theme-"));
    /* theme-* legyen az első class: a CSS body[class^="theme-"] szabályok így mindig illeszkednek */
    document.body.className = [t, ...rest].join(" ").trim();
}
async function applySalonThemeForPublicPages() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const cache = getCache();
    try {
        const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
        if (salon) {
            const th = salon.theme ?? salon.Theme ?? "";
            applyTheme(th);
            cache[salonId] = th;
            localStorage.setItem("salonThemeById", JSON.stringify(cache));
            return;
        }
    } catch (e) {
        applyTheme(cache[salonId]);
    }
}

/**
 * Guest /Salon routes: salon must exist in the database.
 */
async function assertPublicSalonExistsOrRedirect(salonIdRaw) {
    const id = parseInt(String(salonIdRaw ?? '').trim(), 10);
    if (!Number.isFinite(id) || id < 1) {
        window.location.replace('/404');
        return false;
    }
    try {
        const exists = await CheckSalonExists(id);
        if (!exists) {
            window.location.replace('/404');
            return false;
        }
    } catch (e) {
        console.error(e);
        return false;
    }
    return true;
}

/**
 * For owner-only pages: salonId must be a real salon row that belongs to the logged-in owner.
 * Otherwise redirects to /404 (covers unknown IDs and other owners’ salons).
 */
async function assertOwnerHasSalonOrRedirect(salonIdRaw) {
    const id = parseInt(String(salonIdRaw ?? '').trim(), 10);
    if (!Number.isFinite(id) || id < 1) {
        window.location.replace('/404');
        return false;
    }
    try {
        const token = localStorage.getItem('token');
        const salons = await GetMySalons(token);
        const sid = (s) => Number(s.id ?? s.Id);
        if (!Array.isArray(salons) || !salons.some((s) => sid(s) === id)) {
            window.location.replace('/404');
            return false;
        }
    } catch (e) {
        console.error(e);
        window.location.replace('/404');
        return false;
    }
    return true;
}

async function loadAppointments(id) {
    const response = await GetAppointmentBySalonId(localStorage.getItem('token'), id)
    for (const x of response) {
        // Fix: Replace all hyphens with colons before splitting, but only on the date part causes unwanted token.
        // The original code only replaces the first '-' with ':', which is incorrect for date formatting.
        // Instead, we likely want the local date string or just the date part as YYYY-MM-DD.
        // If you want to output only the date portion in the original format, do:
        const dt = new Date(x.startTime);
        const yyyy = dt.getFullYear();
        const mm = String(dt.getMonth() + 1).padStart(2, '0');
        const dd = String(dt.getDate()).padStart(2, '0');
        const hours = String(dt.getHours()).padStart(2, '0');
        const mins = String(dt.getMinutes()).padStart(2, '0');
        const start = `${yyyy}:${mm}:${dd} & ${hours}:${mins}`;
        const guestId = x.guestId
        const artistId = x.artistId
        const services = x.serviceIds
        const ids = {
            guestId: guestId,
            artistId: artistId,
            serviceIds: services
        };
        const names = await getBookingNames(ids)
        const serviceNames = Array.isArray(names.services)
            ? names.services.map((s) => s.name).join(", ")
            : "";
        document.getElementById('apps-table').innerHTML += `
        
                        <tr>
                            <td>${start}</td>
                            <td>${names.artistName}</td>
                            <td>${names.guestName}</td>
                            <td>${serviceNames}</td>
                        </tr>
        `
    }
    return
    
}

async function formRegisterArtist(id){
    document.getElementById('owner-artist-register-form').addEventListener('submit',async (e) => {
        e.preventDefault();
        const form = e.target;
        const name = form.querySelector('#ArtistRegName').value;
        const email = form.querySelector('#ArtistRegEmail').value;
        const password = form.querySelector('#ArtistRegPassword').value;
        const description = form.querySelector('#ArtistRegDescription').value;

        const artist = {
            name: name,
            email: email,
            pwd: password,
            salonId: Number(id),
            description: description
        }

        const result = await RegisterArtist(artist);
        if (result.ok) {
            window.location.assign(`/salondashboard?salonId=${encodeURIComponent(String(id))}`);
        }
    })
}

async function loadWorkingHours() {
    const startRaw = new Date();
    const endRaw = new Date(startRaw);
    endRaw.setDate(endRaw.getDate() + 7);

    const formatYmd = (d) => {
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, "0");
        const day = String(d.getDate()).padStart(2, "0");
        return `${y}-${m}-${day}`;
    };

    const start = formatYmd(startRaw);
    const end = formatYmd(endRaw);
    const date = {
        startDate: start,
        endDate: end
    }
    const hours = await GetArtistWorkingHoursByMonth(localStorage.getItem('token'), date )
    const days = ["Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek", "Szombat", "Vasárnap"];
    hours.forEach(x => {
        const [year, month, dayPart] = x.date.split("-");
        const day = dayPart.slice(0, 2);
        const dow = new Date(x.date).getDay();
        const dayName = days[(dow + 6) % 7];
        const startTime = String(x.startTime).trim();
        const endTime = String(x.endTime).trim();
        const isClosed =
            (startTime === "0" && endTime === "0") ||
            (startTime === "00:00" && endTime === "00:00");
        const openHours = isClosed ? "closed" : `${startTime} - ${endTime}`;
        document.getElementById('artist-hours-list').innerHTML += `

            <li><span>${dayName} (${day}/${month})</span> <strong>${openHours}</strong></li>
        `
})
    
    
}

async function tryCompletePendingBookingAfterGuestLogin(token) {
    const params = new URLSearchParams(window.location.search);
    const pendingDate = params.get('appointmentDate');
    const pendingStart = params.get('startTime');
    const serviceidsRaw = params.get('serviceids') || '';
    const artistId = Number(params.get('artistid')) || 0;
    const salonIdRaw = params.get('salonId') || params.get('salonid');
    const salonIdNum = Number(salonIdRaw) || 0;

    if (!pendingDate || !pendingStart || artistId < 1 || !String(serviceidsRaw).trim()) {
        return false;
    }

    const serviceIds = serviceidsRaw
        .split(',')
        .map((x) => Number(String(x).trim()))
        .filter((n) => Number.isFinite(n) && n > 0);
    if (serviceIds.length === 0) return false;

    const guestUser = await GetLoggedInUser(token);
    const guestId = Number(guestUser?.id ?? guestUser?.Id);
    if (!Number.isFinite(guestId) || guestId < 1) {
        throw new Error('Could not load your profile.');
    }

    const appointment = {
        guestId,
        artistId,
        salonId: salonIdNum > 0 ? salonIdNum : 0,
        serviceIds,
        startTime: pendingStart,
        notes: '',
    };
    await PostAppointment(appointment, token);
    const salonQ = salonIdNum > 0 ? `?salonId=${encodeURIComponent(String(salonIdNum))}` : '';
    window.location.assign(`/Salon/AppointmentCreated${salonQ}`);
    return true;
}

function escapeHtml(str) {
    if (str == null) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

const SALON_SHARED_FOOTER_HTML = `
<footer class="salon-landing-footer salon-landing-footer--rich salon-landing-footer--fullbleed" id="salon-landing-footer" aria-label="Lábléc és elérhetőség">
    <div class="container salon-landing-footer__inner">
        <div class="salon-landing-footer__grid">
            <div class="salon-landing-footer__col">
                <h3 class="salon-landing-footer__heading">Kapcsolat</h3>
                <ul class="salon-landing-footer__contacts" id="salon-footer-contacts"></ul>
            </div>
            <div class="salon-landing-footer__col">
                <h3 class="salon-landing-footer__heading">Navigáció</h3>
                <ul class="salon-landing-footer__nav">
                    <li><a id="footer-nav-services" href="#">Szolgáltatások</a></li>
                    <li><a id="footer-nav-artists" href="#">Szakemberek</a></li>
                    <li><a id="footer-nav-login" href="#">Bejelentkezés vendégként</a></li>
                </ul>
            </div>
        </div>
        <p class="salon-landing-footer__meta salon-landing-footer__meta--legacy" id="salon-footer-meta" hidden></p>
        <div class="salon-landing-footer__bottom">
            <p class="salon-landing-footer__brand-line">
                <a class="salon-landing-footer__logo-link" href="/portal" title="FoglalóPlusz" aria-label="FoglalóPlusz – kezdőlap">
                    <img src="/img/Foglaloplusz_Logo.svg" alt="" width="168" height="27" decoding="async" />
                </a>
                <span class="salon-landing-footer__sep-dot" aria-hidden="true">·</span>
                <span>Időpontfoglalás egyszerűen</span>
            </p>
        </div>
    </div>
</footer>`;

function ensureSalonSharedFooterInDom() {
    if (document.getElementById("salon-landing-footer")) return;
    const path = window.location.pathname;
    if (path !== "/Salon" && path !== "/Salon/" && !path.startsWith("/Salon/")) return;
    const sp = new URLSearchParams(window.location.search);
    if (!(sp.get("salonId") || sp.get("salonid"))) return;
    const tpl = document.createElement("template");
    tpl.innerHTML = SALON_SHARED_FOOTER_HTML.trim();
    document.body.appendChild(tpl.content.firstElementChild);
}

function fillSalonSharedFooterFromSalon(salon, salonId) {
    const emailRaw = salon?.email ?? salon?.Email ?? "";
    const ownerName = salon?.ownerName ?? salon?.OwnerName ?? "";
    const locationStr = salon?.location ?? salon?.Location ?? "";
    const sq = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";

    const contactsEl = document.getElementById("salon-footer-contacts");
    if (contactsEl) {
        const rows = [];
        if (ownerName && String(ownerName).trim()) {
            rows.push(
                `<li class="salon-landing-footer__contact-row"><span class="salon-landing-footer__label">Tulajdonos</span><span class="salon-landing-footer__value">${escapeHtml(String(ownerName).trim())}</span></li>`,
            );
        }
        if (emailRaw && String(emailRaw).trim()) {
            const em = String(emailRaw).trim();
            rows.push(
                `<li class="salon-landing-footer__contact-row"><span class="salon-landing-footer__label">E-mail</span><a class="salon-landing-footer__value salon-landing-footer__link" href="mailto:${em.replace(/"/g, "")}">${escapeHtml(em)}</a></li>`,
            );
        }
        if (locationStr && String(locationStr).trim()) {
            rows.push(
                `<li class="salon-landing-footer__contact-row"><span class="salon-landing-footer__label">Helyszín</span><span class="salon-landing-footer__value">${escapeHtml(String(locationStr).trim())}</span></li>`,
            );
        }
        contactsEl.innerHTML =
            rows.length > 0
                ? rows.join("")
                : `<li class="salon-landing-footer__contact-row salon-landing-footer__contact-row--muted"><span class="salon-landing-footer__value">A szalon még nem adott meg elérhetőségi adatot.</span></li>`;
    }

    const footerNavSvc = document.getElementById("footer-nav-services");
    const footerNavArt = document.getElementById("footer-nav-artists");
    const footerNavLogin = document.getElementById("footer-nav-login");
    if (footerNavSvc) footerNavSvc.href = `/Salon/Services${sq}`;
    if (footerNavArt) footerNavArt.href = `/Salon/Artists${sq}`;
    if (footerNavLogin) footerNavLogin.href = `/Salon/Login${sq}`;
    syncFooterGuestLoginNavVisibility();

    const footerMeta = document.getElementById("salon-footer-meta");
    if (footerMeta) {
        footerMeta.innerHTML = "";
        footerMeta.hidden = true;
    }
}

async function fillSalonSharedFooterFromApi() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    if (!salonId) return;
    const salon = await GetSalonBySalonId(localStorage.getItem("token"), salonId);
    fillSalonSharedFooterFromSalon(salon, salonId);
}

function formatDurationMinutes(totalMinutes) {
    const m = Math.max(0, Math.floor(Number(totalMinutes) || 0));
    if (m < 60) return `${m} min`;
    const h = Math.floor(m / 60);
    const r = m % 60;
    const hourWord = h === 1 ? 'hour' : 'hours';
    if (r === 0) return `${h} ${hourWord}`;
    return `${h} ${hourWord} ${r} min`;
}

/** Magyar idő — szolgáltatás sorokhoz (pl. „1 óra 20 perc”). */
function formatDurationMinutesHu(totalMinutes) {
    const m = Math.max(0, Math.floor(Number(totalMinutes) || 0));
    if (m === 0) return '0 perc';
    if (m < 60) return `${m} perc`;
    const h = Math.floor(m / 60);
    const r = m % 60;
    if (r === 0) return `${h} óra`;
    return `${h} óra ${r} perc`;
}

function formatPriceFt(amount) {
    const n = Math.round(Number(amount) || 0);
    return `${n.toLocaleString('hu-HU').replace(/\u00a0/g, ' ')} Ft`;
}

/** Vendég- és szakember kezdőlapon: szolgáltatás- és szakemberlista betöltése. */
async function loadSalonLandingLists() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    if (!salonId) return;

    const servicesList = document.getElementById("salon-services-list");
    const artistsList = document.getElementById("salon-artists-list");
    const svcEmpty = document.getElementById("salon-services-empty");
    const artEmpty = document.getElementById("salon-artists-empty");
    if (!servicesList || !artistsList) return;

    try {
        const services = await GetServices(salonId);
        const arr = Array.isArray(services) ? services : [];
        servicesList.innerHTML = "";
        if (arr.length === 0) {
            if (svcEmpty) svcEmpty.hidden = false;
        } else {
            if (svcEmpty) svcEmpty.hidden = true;
            arr.forEach((s, i) => {
                const name = escapeHtml(s.name ?? s.Name ?? "");
                const price = Number(s.price ?? s.Price ?? 0);
                const dur = Number(s.durationInMinute ?? s.DurationInMinute ?? 0);
                const priceStr = formatPriceFt(price);
                const durStr = formatDurationMinutesHu(dur);
                servicesList.innerHTML += `<li class="salon-landing-li async-card-enter" style="--async-card-i:${i}"><span class="salon-landing-li__name">${name}</span><span class="salon-landing-li__meta">${durStr} · ${priceStr}</span></li>`;
            });
        }
    } catch (e) {
        console.error(e);
        if (svcEmpty) {
            svcEmpty.hidden = false;
            svcEmpty.textContent = "Nem sikerült betölteni a szolgáltatásokat.";
        }
    }

    try {
        const artists = await GetPublicArtistsForSalon(salonId);
        const arr = Array.isArray(artists) ? artists : [];
        artistsList.innerHTML = "";
        if (arr.length === 0) {
            if (artEmpty) artEmpty.hidden = false;
        } else {
            if (artEmpty) artEmpty.hidden = true;
            arr.forEach((a, i) => {
                const name = escapeHtml(a.name ?? a.Name ?? "");
                const desc = escapeHtml(a.description ?? a.Description ?? "");
                const id = String(a.id ?? a.Id);
                const bookHref = `/Salon/Services?salonId=${encodeURIComponent(salonId)}&artistId=${encodeURIComponent(id)}`;
                const descHtml = desc ? `<span class="salon-landing-li__desc">${desc}</span>` : "";
                artistsList.innerHTML += `<li class="salon-landing-li salon-landing-li--artist async-card-enter" style="--async-card-i:${i}"><div class="salon-landing-li__artist-text"><span class="salon-landing-li__name">${name}</span>${descHtml}</div><a class="salon-landing-li__book" href="${bookHref}">Szolgáltatások</a></li>`;
            });
        }
    } catch (e) {
        console.error(e);
        if (artEmpty) {
            artEmpty.hidden = false;
            artEmpty.textContent = "Nem sikerült betölteni a szakembereket.";
        }
    }
}

function syncServicesCartButton() {
    const list = document.getElementById('services-list');
    const btn = document.getElementById('AddToCart');
    const holder = document.getElementById('services-cart-holder');
    const countEl = document.getElementById('services-cart-count');
    const totalsEl = document.getElementById('services-cart-totals');
    if (!list || !btn || !holder) return;
    const checked = list.querySelectorAll('input.booking-choice-input[type="checkbox"]:checked');
    const has = checked.length > 0;
    btn.hidden = !has;
    holder.hidden = !has;
    document.body.classList.toggle('services-cart-visible', has);
    if (countEl && totalsEl) {
        let totalMin = 0;
        let totalPrice = 0;
        checked.forEach((cb) => {
            totalMin += Number(cb.dataset.durationMinute || 0);
            totalPrice += Number(cb.dataset.price || 0);
        });
        const n = checked.length;
        countEl.textContent = has
            ? (n === 1 ? '1 szolgáltatás' : `${n} szolgáltatás`)
            : '0 szolgáltatás';
        totalsEl.textContent = has
            ? `${totalMin} perc - ${formatPriceFt(totalPrice)}`
            : '0 perc — 0 Ft';
    }
}

document.addEventListener('change', (e) => {
    if (e.target.matches('#services-list input.booking-choice-input[type="checkbox"]')) {
        syncServicesCartButton();
    }
    if (e.target.matches('#artists-list input.booking-choice-input[type="radio"]')) {
        syncArtistPickBar();
    }
});

function syncArtistPickBar() {
    const list = document.getElementById('artists-list');
    const holder =
        document.getElementById('artist-cart-holder') ||
        document.getElementById('artist-browse-cart-holder');
    const btn =
        document.getElementById('artistContinueBtn') ||
        document.getElementById('artist-browse-continue-btn');
    const nameEl =
        document.getElementById('artist-cart-count') ||
        document.getElementById('artist-browse-cart-name');
    if (!list || !holder || !btn) return;
    const checked = list.querySelector('input.booking-choice-input[type="radio"]:checked');
    const has = Boolean(checked);
    btn.hidden = !has;
    holder.hidden = !has;
    document.body.classList.toggle('artist-picker-visible', has);
    if (nameEl) {
        nameEl.textContent = has ? (checked.getAttribute('data-artist-name') || '') : '';
    }
}

async function AddToCart() {
    const ids = [];
    document.querySelectorAll('#services-list input.booking-choice-input[type="checkbox"]:checked').forEach((cb) => {
        ids.push(cb.id);
    });
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const artistId = params.get("artistId") || params.get("artistid");
    const salonQuery = salonId ? `&salonId=${encodeURIComponent(salonId)}` : "";
    if (artistId) {
        window.location.href = `/Salon/DateSelection?serviceids=${ids.join(',')}&artistid=${encodeURIComponent(artistId)}${salonQuery}`;
    } else {
        window.location.href = `/Salon/SelectArtist?serviceids=${ids.join(',')}${salonQuery}`;
    }
}

function SelectArtist(){
    const selectedRadio = document.querySelector('#artists-list input.booking-choice-input[type="radio"]:checked');
    if (!selectedRadio) return;
    const id = selectedRadio.id;
    const params = new URLSearchParams(window.location.search);
    const serviceids = params.get('serviceids') ?? '';
    const salonId = params.get("salonId") || params.get("salonid");
    const salonQuery = salonId ? `&salonId=${encodeURIComponent(salonId)}` : "";
    window.location.href = `/Salon/DateSelection?serviceids=${encodeURIComponent(serviceids)}&artistid=${encodeURIComponent(id)}${salonQuery}`;
}

function wireOwnerDashboardCatalogEdit() {
    const list = document.getElementById("owner-dash-catalog-list");
    if (!list || list.dataset.editWired === "1") return;
    list.dataset.editWired = "1";
    list.addEventListener("submit", (e) => {
        if (e.target.closest("form.owner-catalog-edit-form")) e.preventDefault();
    });
    list.addEventListener("keydown", (e) => {
        const form = e.target.closest("form.owner-catalog-edit-form");
        if (!form) return;
        if (e.key !== "Enter") return;
        if (e.target.tagName === "TEXTAREA") return;
        e.preventDefault();
    });
    list.addEventListener("click", async (e) => {
        const saveBtn = e.target.closest("button.owner-catalog-save-btn");
        if (saveBtn) {
            const form = saveBtn.closest("form.owner-catalog-edit-form");
            if (!form) return;
            await saveOwnerCatalogServiceForm(form, saveBtn);
            return;
        }
        const delBtn = e.target.closest("button.owner-catalog-delete-btn");
        if (!delBtn) return;
        const sidAttr = delBtn.getAttribute("data-service-id");
        const salonId = list?.dataset?.salonId;
        if (!sidAttr || !salonId) return;
        const token = localStorage.getItem("token");
        if (!token) {
            salonicShowModal("Jelentkezz be tulajdonosként.", { title: "Bejelentkezés szükséges", variant: "warning" });
            return;
        }
        if (
            !confirm(
                "Biztosan törlöd ezt a katalógus-szolgáltatást? A szakemberek listájáról is eltűnik, és a kapcsolódó offeringek törlődnek.",
            )
        )
            return;
        delBtn.disabled = true;
        try {
            await deleteOwnerSalonService(Number(salonId), Number(sidAttr), token);
            await loadServicesBySalon(salonId);
        } catch (err) {
            console.error(err);
            if (!err?.salonicApiShown) {
                salonicShowModal(err?.message || "Törlés sikertelen.", { title: "Hiba", variant: "danger" });
            }
        } finally {
            delBtn.disabled = false;
        }
    });
}

async function saveOwnerCatalogServiceForm(form, btn) {
    const list = document.getElementById("owner-dash-catalog-list");
    const salonId = list?.dataset?.salonId;
    const sid = form.getAttribute("data-service-id");
    if (!salonId || !sid) return;
    const token = localStorage.getItem("token");
    if (!token) {
        salonicShowModal("Jelentkezz be tulajdonosként.", { title: "Bejelentkezés szükséges", variant: "warning" });
        return;
    }
    const msgEl = form.querySelector(".owner-catalog-edit-msg");
    const nameEl = form.querySelector('[name="name"]');
    const descEl = form.querySelector('[name="description"]');
    const durEl = form.querySelector('[name="durationInMinute"]');
    const priceEl = form.querySelector('[name="price"]');
    const payload = {
        name: (nameEl?.value ?? "").trim(),
        description: descEl?.value ?? "",
        durationInMinute: Math.max(1, parseInt(String(durEl?.value ?? "1"), 10) || 1),
        price: Math.max(0, parseInt(String(priceEl?.value ?? "0"), 10) || 0),
    };
    if (!payload.name) {
        if (msgEl) {
            msgEl.textContent = "A megnevezés kötelező.";
            msgEl.classList.add("text-danger");
            msgEl.classList.remove("text-success");
        }
        return;
    }
    if (msgEl) {
        msgEl.textContent = "";
        msgEl.classList.remove("text-danger", "text-success");
    }
    if (btn) btn.disabled = true;
    try {
        await updateOwnerSalonService(Number(salonId), Number(sid), payload, token);
        if (msgEl) {
            msgEl.textContent = "Mentve.";
            msgEl.classList.remove("text-danger");
            msgEl.classList.add("text-success");
        }
        await loadServicesBySalon(salonId);
    } catch (err) {
        console.error(err);
        if (!err?.salonicApiShown && msgEl) {
            msgEl.textContent = err?.message || "Mentés sikertelen.";
            msgEl.classList.add("text-danger");
            msgEl.classList.remove("text-success");
        }
    } finally {
        if (btn) btn.disabled = false;
    }
}

async function loadServicesBySalon(id) {
    const list = document.getElementById("owner-dash-catalog-list");
    if (!list) return;
    const emptyEl = document.getElementById("owner-dash-catalog-empty");
    const token = localStorage.getItem("token");
    if (!token) return;
    const sid = String(id).trim();
    list.dataset.salonId = sid;
    list.innerHTML = '<p class="text-muted mb-0">Betöltés…</p>';
    let services;
    try {
        services = await GetSalonServicesCatalog(sid, token);
    } catch {
        list.innerHTML = '<p class="text-danger mb-0" role="alert">Nem sikerült betölteni a katalógust.</p>';
        return;
    }
    if (!Array.isArray(services) || services.length === 0) {
        list.innerHTML = "";
        if (emptyEl) {
            emptyEl.hidden = false;
        }
        return;
    }
    if (emptyEl) emptyEl.hidden = true;
    list.innerHTML = "";
    services.forEach((x, i) => {
        const svcId = Number(x.id ?? x.Id ?? 0);
        const ownerIdRaw = x.ownerId ?? x.OwnerId;
        const isOwnerCatalog = ownerIdRaw != null && Number(ownerIdRaw) > 0;
        const nameRaw = String(x.name ?? x.Name ?? "").trim();
        const name = escapeHtml(nameRaw);
        const descRaw = x.description ?? x.Description ?? "";
        const descTrim = String(descRaw ?? "").trim();
        const descBlock = descTrim
            ? `<div class="artist-service-desc-wrap"><span class="artist-service-desc">${escapeHtml(descTrim)}</span></div>`
            : '<p class="artist-service-no-desc text-muted small mb-0">Nincs leírás.</p>';
        const mins = Math.max(0, Math.round(Number(x.durationInMinute ?? x.DurationInMinute ?? 0)));
        const price = Number(x.price ?? x.Price ?? 0);
        const priceLine = formatPriceFt(price);
        const nameInputVal = escapeHtml(nameRaw);
        const descInputVal = escapeHtml(descTrim);
        const sourceBadge = isOwnerCatalog
            ? '<span class="badge bg-primary ms-2">Tulaj katalógus</span>'
            : '<span class="badge bg-secondary ms-2">Szakember szolgáltatása</span>';
        const editTabNav = isOwnerCatalog
            ? `
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="owner-svc-tab-edit-${svcId}" data-bs-toggle="tab" data-bs-target="#owner-svc-pane-edit-${svcId}" type="button" role="tab" aria-controls="owner-svc-pane-edit-${svcId}" aria-selected="false">Szerkesztés</button>
                    </li>`
            : "";
        const editTabPane = isOwnerCatalog
            ? `
                    <div class="tab-pane fade" id="owner-svc-pane-edit-${svcId}" role="tabpanel" aria-labelledby="owner-svc-tab-edit-${svcId}" tabindex="0">
                        <form class="owner-catalog-edit-form" data-service-id="${svcId}">
                            <div class="mb-3">
                                <label class="form-label" for="owner-svc-name-${svcId}">Megnevezés</label>
                                <input class="form-control" id="owner-svc-name-${svcId}" name="name" type="text" required maxlength="200" value="${nameInputVal}">
                            </div>
                            <div class="mb-3">
                                <label class="form-label" for="owner-svc-desc-${svcId}">Leírás</label>
                                <textarea class="form-control" id="owner-svc-desc-${svcId}" name="description" rows="3" maxlength="2000">${descInputVal}</textarea>
                            </div>
                            <div class="row g-3 mb-3">
                                <div class="col-sm-6">
                                    <label class="form-label" for="owner-svc-dur-${svcId}">Időtartam (perc)</label>
                                    <input class="form-control" id="owner-svc-dur-${svcId}" name="durationInMinute" type="number" min="1" max="1440" step="1" required value="${mins}">
                                </div>
                                <div class="col-sm-6">
                                    <label class="form-label" for="owner-svc-price-${svcId}">Ár (Ft)</label>
                                    <input class="form-control" id="owner-svc-price-${svcId}" name="price" type="number" min="0" max="10000000" step="1" required value="${Math.round(price)}">
                                </div>
                            </div>
                            <div class="d-flex flex-wrap align-items-center gap-2">
                                <button type="button" class="btn btn-primary btn-sm adservice-accent-btn owner-catalog-save-btn">Változtatások elmentése</button>
                                <button type="button" class="btn btn-outline-danger btn-sm owner-catalog-delete-btn" data-service-id="${svcId}">Katalógus törlése</button>
                                <span class="owner-catalog-edit-msg small" role="status" aria-live="polite"></span>
                            </div>
                        </form>
                    </div>`
            : "";
        list.innerHTML += `
            <div class="service-card artist-service-card async-card-enter" style="--async-card-i:${i}" role="listitem" data-service-id="${svcId}">
                <ul class="nav nav-tabs artist-service-nav-tabs" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active" id="owner-svc-tab-view-${svcId}" data-bs-toggle="tab" data-bs-target="#owner-svc-pane-view-${svcId}" type="button" role="tab" aria-controls="owner-svc-pane-view-${svcId}" aria-selected="true">Részletek</button>
                    </li>${editTabNav}
                </ul>
                <div class="tab-content artist-service-tab-content">
                    <div class="tab-pane fade show active" id="owner-svc-pane-view-${svcId}" role="tabpanel" aria-labelledby="owner-svc-tab-view-${svcId}" tabindex="0">
                        <div class="name">${name}${sourceBadge}</div>
                        ${descBlock}
                        <div class="artist-service-meta" role="group" aria-label="Időtartam és ár">
                            <span class="artist-service-meta-item">
                                <span class="artist-service-meta-label">Időtartam</span>
                                <strong class="artist-service-meta-value">${mins} perc</strong>
                            </span>
                            <span class="artist-service-meta-item">
                                <span class="artist-service-meta-label">Ár</span>
                                <strong class="artist-service-meta-value">${priceLine}</strong>
                            </span>
                        </div>
                        ${
                            isOwnerCatalog
                                ? '<p class="small text-muted mb-0 mt-2">A módosítás minden szakember listáján is megjelenik, aki felvette ezt a katalógus-tételt.</p>'
                                : ""
                        }
                    </div>${editTabPane}
                </div>
            </div>
        `;
    });
}

/**
 * Tulaj irányítópult: egyedi + CSV katalógus-szolgáltatás.
 * @param {string} salonId
 */
function initOwnerSalonDashboardServices(salonId) {
    const form = document.getElementById("owner-dash-addservice-form");
    if (!form) return;

    wireOwnerDashboardCatalogEdit();

    const sid = String(salonId).trim();
    if (!sid) return;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const token = localStorage.getItem("token");
        if (!token) {
            salonicShowModal("Jelentkezz be tulajdonosként.", { title: "Bejelentkezés szükséges", variant: "warning" });
            return;
        }
        const newService = {
            name: document.getElementById("owner-dash-svc-name")?.value?.trim() ?? "",
            description: document.getElementById("owner-dash-svc-description")?.value ?? "",
            price: Number(document.getElementById("owner-dash-svc-price")?.value),
            durationInMinute: Number(document.getElementById("owner-dash-svc-duration")?.value),
        };
        try {
            await createOwnerSalonService(sid, newService, token);
            salonicShowModal("Szolgáltatás elmentve.", { title: "Mentve", variant: "success" });
            form.reset();
            await loadServicesBySalon(sid);
        } catch (err) {
            console.error(err);
            if (!err?.salonicApiShown) {
                salonicShowModal(err?.message || "Nem sikerült menteni a szolgáltatást.", { title: "Hiba", variant: "danger" });
            }
        }
    });

    document.getElementById("owner-dash-csv-template-btn")?.addEventListener("click", () => {
        const sample =
            "servicename;duration;description;price\nClassic cut;45;Wash and cut;3500\nColor;90;;12000\n";
        const blob = new Blob([sample], { type: "text/csv;charset=utf-8" });
        const a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = "salon-catalog-template.csv";
        a.click();
        URL.revokeObjectURL(a.href);
    });

    document.getElementById("owner-dash-csv-import-btn")?.addEventListener("click", async () => {
        const fileInput = document.getElementById("owner-dash-csv-file");
        const resultEl = document.getElementById("owner-dash-csv-result");
        const btn = document.getElementById("owner-dash-csv-import-btn");
        const file = fileInput?.files?.[0];
        if (!file) {
            salonicShowModal("Válassz előbb egy CSV fájlt.", { title: "Import", variant: "warning" });
            return;
        }
        const token = localStorage.getItem("token");
        if (!token) {
            salonicShowModal("Jelentkezz be tulajdonosként.", { title: "Bejelentkezés szükséges", variant: "warning" });
            return;
        }
        let csvText;
        try {
            csvText = await file.text();
        } catch (err) {
            console.error(err);
            salonicShowModal("A fájl nem olvasható.", { title: "Hiba", variant: "danger" });
            return;
        }
        if (!resultEl) return;
        resultEl.classList.remove("alert-danger", "alert-success", "alert-warning");
        resultEl.classList.add("alert-light", "border");
        resultEl.hidden = false;
        resultEl.innerHTML = '<p class="text-muted mb-0">Importálás…</p>';
        if (btn) btn.disabled = true;
        try {
            const data = await importOwnerSalonServicesFromCsv(sid, csvText, token);
            const created = data.created ?? data.Created ?? [];
            const errors = data.errors ?? data.Errors ?? [];
            const okCount = created.length;
            const errCount = errors.length;
            let html = `<p class="fw-semibold mb-2">Kész: <strong>${okCount}</strong> szolgáltatás létrehozva${
                errCount ? `, <strong class="text-danger">${errCount}</strong> sor hibás` : ""
            }.</p>`;
            if (okCount > 0) {
                html += '<ul class="list-unstyled mb-2 small">';
                created.slice(0, 20).forEach((s) => {
                    const n = escapeHtml(s.name ?? s.Name ?? "");
                    html += `<li class="text-success">${n}</li>`;
                });
                if (created.length > 20) html += '<li class="text-muted">…</li>';
                html += "</ul>";
            }
            if (errCount > 0) {
                html += '<p class="text-danger fw-semibold mb-1">Hibák</p><ul class="mb-0 small">';
                errors.forEach((e) => {
                    const line = e.line ?? e.Line;
                    const msg = escapeHtml(e.message ?? e.Message ?? "");
                    html += `<li><span class="text-nowrap">${line}. sor:</span> ${msg}</li>`;
                });
                html += "</ul>";
            }
            resultEl.classList.remove("alert-light");
            if (errCount > 0 && okCount === 0) {
                resultEl.classList.add("alert-danger");
            } else if (okCount > 0 && errCount === 0) {
                resultEl.classList.add("alert-success");
            } else {
                resultEl.classList.add("alert-warning");
            }
            resultEl.innerHTML = html;
            if (okCount > 0) {
                await loadServicesBySalon(sid);
                if (fileInput) fileInput.value = "";
            }
        } catch (err) {
            console.error(err);
            if (!err?.salonicApiShown) {
                resultEl.classList.remove("alert-light", "alert-success");
                resultEl.classList.add("alert-danger");
                resultEl.innerHTML = `<p class="text-danger mb-0" role="alert">${escapeHtml(err?.message || "Az importálás sikertelen.")}</p>`;
            }
        } finally {
            if (btn) btn.disabled = false;
        }
    });
}




async function loadArtistsBySalonId(id) {
    let artists;
    try {
        artists = await getArtistBySalonId(localStorage.getItem("token"), id);
    } catch {
        return;
    }
    const salonIdEnc = encodeURIComponent(id);
    artists.forEach((x, i) => {
        const nameEnc = encodeURIComponent(x.name || "Artist");
        document.getElementById("salon-dash-artists").innerHTML += `
            <li class="salon-dash-artist-card async-card-enter" style="--async-card-i:${i}">
                    <div>
                        <h3 class="salon-dash-card-name">${x.name}</h3>
                        <p class="salon-dash-card-meta">${x.description}</p>
                    </div>
                    <a href="/salondashboard/schedule?salonId=${salonIdEnc}&artistId=${x.id}&artistName=${nameEnc}" class="owner-btn">Naptár megtekintése</a>
                </li>
        `;
    });
}

async function loadSalons() {
    const list = document.getElementById("owner-salons-list");
    if (!list) return;

    const token = localStorage.getItem("token");
    let salons;
    try {
        salons = await GetMySalons(token);
    } catch {
        return;
    }
    list.innerHTML = "";
    salons.forEach(
        (x, i) =>
            (list.innerHTML += `
            <li class="owner-salon-card async-card-enter" style="--async-card-i:${i}">
                    <div class="owner-salon-main">
                        <h3 class="owner-salon-name">${x.name}</h3>
                        <p class="owner-salon-meta">${x.email}</p>
                        <p class="owner-salon-desc">${x.description}</p>
                    </div>
                    <div class="owner-salon-actions">
                        <a href="/salondashboard?salonId=${x.id}" class="owner-btn">Irányítópult megnyitása</a>
                        <a href="/ownersalonedit?salonId=${x.id}" class="owner-btn owner-btn-ghost">Szalon szerkesztése</a>
                        <a href="/ownerartistregister?salonId=${x.id}" class="owner-btn owner-btn-ghost">Új dolgozó felvétele</a>
                        <button type="button" class="owner-btn owner-btn-danger" data-action="remove-salon" data-salon-id="${x.id}">Szalon eltávolítása</button>
                    </div>
                </li>
        `),
    );
}

async function loadPortalSalons() {
    const list = document.getElementById("portal-salons-list");
    if (!list) return;

    list.innerHTML = `
        <div class="placeholder-glow"><span class="placeholder col-12 rounded" style="height:2.75rem"></span></div>
        <div class="placeholder-glow"><span class="placeholder col-12 rounded" style="height:2.75rem"></span></div>`;
    try {
        const salons = await GetPublicSalons();
        if (!Array.isArray(salons) || salons.length === 0) {
            list.innerHTML = '<p class="subtitle text-muted mb-0">No salons available yet.</p>';
            return;
        }

        list.innerHTML = '';
        salons.forEach((salon, i) => {
            const salonId = salon.id ?? salon.Id;
            const name = escapeHtml(salon.name ?? salon.Name ?? `Salon ${salonId}`);
            list.innerHTML += `
                <a class="btn btn-primary w-100 async-card-enter" style="--async-card-i:${i}" href="/Salon?salonId=${encodeURIComponent(salonId)}">${name}</a>
            `;
        });
    } catch (err) {
        console.error(err);
        list.innerHTML = '<p class="text-danger small mb-0" role="alert">Could not load salons from the database.</p>';
    }
}

/** @param {number|undefined|null} filterArtistId If set, only services this artist offers (Offerings). */
async function LoadServices(filterArtistId) {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get('salonId') || params.get('salonid');
    const list = document.getElementById('services-list');
    const emptyEl = document.getElementById('services-page-empty');
    const errEl = document.getElementById('services-page-error');
    if (!list) return;
    if (emptyEl) {
        emptyEl.hidden = true;
        emptyEl.textContent = '';
    }
    if (errEl) {
        errEl.hidden = true;
        errEl.textContent = '';
    }
    if (!salonId) {
        console.warn('Adj meg ?salonId= paramétert az URL-ben a szolgáltatások betöltéséhez.');
        list.innerHTML = '';
        if (emptyEl) {
            emptyEl.hidden = false;
            emptyEl.textContent =
                'Hiányzik a szalon azonosítója. Nyisd meg ezt az oldalt a szalon kezdőlapjáról vagy a portálról.';
        }
        syncServicesCartButton();
        return;
    }
    list.innerHTML =
        '<div class="service-pick-row service-pick-row--loading" role="status"><div class="service-pick-row__text"><span class="spinner-border spinner-border-sm text-primary me-2" aria-hidden="true"></span>Betöltés…</div></div>';
    let services;
    try {
        const fa = filterArtistId != null ? Number(filterArtistId) : NaN;
        if (Number.isFinite(fa) && fa > 0) {
            services = await GetServicesForArtistInSalon(salonId, fa);
        } else {
            services = await GetServices(salonId);
        }
    } catch (e) {
        console.error(e);
        list.innerHTML = '';
        if (errEl) {
            errEl.hidden = false;
            errEl.textContent = 'Nem sikerült betölteni a szolgáltatásokat. Próbáld újra később.';
        }
        syncServicesCartButton();
        return;
    }
    const arr = Array.isArray(services) ? services : [];
    list.innerHTML = '';
    if (arr.length === 0) {
        if (emptyEl) {
            emptyEl.hidden = false;
            emptyEl.textContent =
                Number.isFinite(fa) && fa > 0
                    ? 'Ehhez a szakemberhez még nincs közzétett szolgáltatás.'
                    : 'Ehhez a szalonhoz még nincs közzétett szolgáltatás.';
        }
        syncServicesCartButton();
        return;
    }
    arr.forEach((x, i) => {
        const name = escapeHtml(x.name ?? x.Name);
        const id = String(x.id ?? x.Id);
        const durationMin = Number(x.durationInMinute ?? x.DurationInMinute ?? 0);
        const price = Number(x.price ?? x.Price ?? 0);
        const durationLine = formatDurationMinutesHu(durationMin);
        const priceLine = formatPriceFt(price);
        list.innerHTML += `
            <div class="service-pick-row async-card-enter" style="--async-card-i:${i}" role="listitem">
                <div class="service-pick-row__text">
                    <div class="service-pick-row__title">${name}</div>
                    <div class="service-pick-row__price">${priceLine}</div>
                    <div class="service-pick-row__meta">Várható időtartam: ${durationLine}</div>
                </div>
                <label class="service-pick-toggle">
                    <input type="checkbox" class="booking-choice-input service-pick-toggle-input" id="${id}"
                        data-duration-minute="${durationMin}" data-price="${price}"
                        aria-label="${name} — kiválasztás">
                    <span class="service-pick-toggle__face" aria-hidden="true"></span>
                </label>
            </div>
        `;
    });
    syncServicesCartButton();
}

function userInitials(name) {
    if (!name || typeof name !== 'string') return '?';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

async function loadGuestUpcomingAppointments(token, mount) {
    mount.innerHTML = `
        <div class="guest-appt-loading" aria-busy="true" aria-label="Loading appointments">
            <div class="guest-appt-loading__bar"></div>
            <div class="guest-appt-loading__bar" style="opacity:0.85"></div>
        </div>`;
    try {
        const raw = await getAppointmentByGuestId(token);
        const list = Array.isArray(raw) ? raw : [];
        const now = new Date();
        const upcoming = list
            .filter((a) => {
                const end = new Date(a.endTime ?? a.EndTime);
                return !Number.isNaN(end.getTime()) && end >= now;
            })
            .sort(
                (a, b) =>
                    new Date(a.startTime ?? a.StartTime) - new Date(b.startTime ?? b.StartTime),
            );

        if (upcoming.length === 0) {
            mount.innerHTML = `
                <div class="guest-appt-empty guest-appt-empty--card">
                    <p class="guest-appt-empty__title">Nincs közelgő időpont</p>
                    <p class="guest-appt-empty__hint">Ha foglalsz, az időpontok itt jelennek meg.</p>
                </div>`;
            return;
        }

        const timeOpts = { hour: '2-digit', minute: '2-digit' };
        const rows = await Promise.all(
            upcoming.map(async (a) => {
                const guestId = a.guestId ?? a.GuestId;
                const artistId = a.artistId ?? a.ArtistId;
                const serviceIds = a.serviceIds ?? a.ServiceIds ?? [];
                let artistName = '';
                let serviceNames = '';
                try {
                    const names = await getBookingNames({ guestId, artistId, serviceIds });
                    const svc = names.services ?? names.Services ?? [];
                    serviceNames = Array.isArray(svc)
                        ? svc.map((s) => s.name ?? s.Name).filter(Boolean).join(', ')
                        : '';
                    artistName = names.artistName ?? names.ArtistName ?? '';
                } catch (err) {
                    console.warn(err);
                }
                const start = new Date(a.startTime ?? a.StartTime);
                const end = new Date(a.endTime ?? a.EndTime);
                const notes = a.notes ?? a.Notes ?? '';
                const salonName = a.salonName ?? a.SalonName ?? '';
                const dow = start.toLocaleDateString(undefined, { weekday: 'short' });
                const day = String(start.getDate());
                const mon = start.toLocaleDateString(undefined, { month: 'short' });
                const fullDate = start.toLocaleDateString(undefined, {
                    weekday: 'long',
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                });
                const timeRange = `${start.toLocaleTimeString(undefined, timeOpts)} – ${end.toLocaleTimeString(undefined, timeOpts)}`;
                const appointmentId = String(a.id ?? a.Id);
                return {
                    appointmentId,
                    dow,
                    day,
                    mon,
                    fullDate,
                    timeRange,
                    salonName,
                    artistName,
                    serviceNames,
                    notes,
                };
            }),
        );

        mount.innerHTML = rows
            .map(
                (r) => `
            <article class="guest-appt-card">
                <div class="guest-appt-card__date" aria-hidden="true">
                    <span class="guest-appt-card__dow">${escapeHtml(r.dow)}</span>
                    <span class="guest-appt-card__day">${escapeHtml(r.day)}</span>
                    <span class="guest-appt-card__mon">${escapeHtml(r.mon)}</span>
                </div>
                <div class="guest-appt-card__body">
                    <div class="guest-appt-card__time">${escapeHtml(r.timeRange)}</div>
                    <div class="guest-appt-card__full-date">${escapeHtml(r.fullDate)}</div>
                    <div class="guest-appt-card__salon"><span>Szalon</span> ${escapeHtml(r.salonName || '—')}</div>
                    <div class="guest-appt-card__artist"><span>Szakember</span> ${escapeHtml(r.artistName || '—')}</div>
                    ${
                        r.serviceNames
                            ? `<div class="guest-appt-card__services">${escapeHtml(r.serviceNames)}</div>`
                            : ''
                    }
                    ${
                        r.notes
                            ? `<div class="guest-appt-card__notes">${escapeHtml(r.notes)}</div>`
                            : ''
                    }
                    <div class="guest-appt-card__footer">
                        <button type="button" class="guest-appt-btn guest-appt-btn--cancel" data-cancel-appointment="${escapeHtml(r.appointmentId)}">Időpont lemondása</button>
                    </div>
                </div>
            </article>`,
            )
            .join('');
    } catch (err) {
        console.error(err);
        mount.innerHTML = `
            <div class="guest-appt-empty guest-appt-empty--card guest-appt-empty--error" role="alert">
                <p class="guest-appt-empty__title">Nem sikerült betölteni az időpontokat</p>
                <p class="guest-appt-empty__hint">Próbáld újra később.</p>
            </div>`;
    }
}

/**
 * @param {"guest"|"owner"|"artist"} role
 */
function getProfileEditFormHtml(role) {
    const descBlock =
        role === "artist"
            ? `<label class="field-label" for="profile-description">Bemutatkozás</label>
            <textarea id="profile-description" class="form-control mb-2" name="description" rows="3"></textarea>`
            : "";
    return `
        <section class="profile-edit-section mt-4">
            <h2 class="section-title h6 mb-3">Adataid szerkesztése</h2>
            <form id="profile-edit-form" class="auth-form profile-edit-form">
                <label class="field-label" for="profile-display-name">Név</label>
                <input id="profile-display-name" class="form-control mb-2" type="text" required autocomplete="name" />
                <label class="field-label" for="profile-email-input">E-mail</label>
                <input id="profile-email-input" class="form-control mb-2" type="email" required autocomplete="email" />
                ${descBlock}
                <p class="subtitle small mb-2 mt-3 text-muted">Jelszó megváltoztatása (opcionális)</p>
                <label class="field-label" for="profile-current-password">Jelenlegi jelszó</label>
                <input id="profile-current-password" class="form-control mb-2" type="password" autocomplete="current-password" />
                <label class="field-label" for="profile-new-password">Új jelszó</label>
                <input id="profile-new-password" class="form-control mb-2" type="password" autocomplete="new-password" />
                <button type="submit" class="btn btn-primary mt-2">Változások mentése</button>
            </form>
        </section>`;
}

function fillProfileEditForm(userData, role) {
    const name = userData.name ?? userData.Name ?? "";
    const email = userData.email ?? userData.Email ?? "";
    const nameEl = document.getElementById("profile-display-name");
    const emailEl = document.getElementById("profile-email-input");
    if (nameEl) nameEl.value = name;
    if (emailEl) emailEl.value = email;
    if (role === "artist") {
        const desc = userData.description ?? userData.Description ?? "";
        const dEl = document.getElementById("profile-description");
        if (dEl) dEl.value = desc;
    }
}

/**
 * @param {"guest"|"owner"|"artist"} role
 * @param {() => Promise<void>} reloadFn
 */
function wireProfileEditFormSubmit(role, token, reloadFn) {
    const form = document.getElementById("profile-edit-form");
    if (!form) return;
    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const newPwd = document.getElementById("profile-new-password")?.value?.trim() ?? "";
        const curPwd = document.getElementById("profile-current-password")?.value ?? "";
        if (newPwd && !curPwd) {
            salonicShowModal("Új jelszó megadásához add meg a jelenlegi jelszót is.", {
                title: "Hiányzó adat",
                variant: "warning",
            });
            return;
        }
        const body = {
            name: document.getElementById("profile-display-name")?.value?.trim() ?? "",
            email: document.getElementById("profile-email-input")?.value?.trim() ?? "",
        };
        if (role === "artist") {
            body.description = document.getElementById("profile-description")?.value ?? "";
        }
        if (newPwd) {
            body.currentPassword = curPwd;
            body.newPassword = newPwd;
        }
        try {
            await updateMyUserProfile(role, token, body);
            salonicShowModal("Adataid mentve.", { title: "Kész", variant: "success" });
            const curEl = document.getElementById("profile-current-password");
            const newEl = document.getElementById("profile-new-password");
            if (curEl) curEl.value = "";
            if (newEl) newEl.value = "";
            await reloadFn();
        } catch (err) {
            console.error(err);
        }
    });
}

async function LoadUserData() {
    const token = localStorage.getItem('token');
    const userData = await GetLoggedInUser(token);

    const name = escapeHtml(userData.name ?? '');
    const email = escapeHtml(userData.email ?? '');
    const initials = escapeHtml(userInitials(userData.name ?? ''));

    document.getElementById('user-profile').innerHTML = `
        <header class="guest-user-hero">
            <div class="guest-user-avatar" aria-hidden="true">${initials}</div>
            <div class="guest-user-hero__text">
                <p class="guest-user-eyebrow">Vendég fiók</p>
                <h1 class="guest-user-title">${name}</h1>
                <p class="guest-user-email">${email}</p>
                <span class="guest-user-role-pill">Vendég</span>
            </div>
        </header>
        ${getProfileEditFormHtml("guest")}
    `;

    fillProfileEditForm(userData, "guest");
    wireProfileEditFormSubmit("guest", token, LoadUserData);

    const appointmentsMount = document.getElementById('user-appointments');
    if (appointmentsMount) {
        await loadGuestUpcomingAppointments(token, appointmentsMount);
    }
}

async function loadArtistUpcomingAppointments(token, mount) {
    mount.innerHTML = `
        <div class="guest-appt-loading" aria-busy="true" aria-label="Loading appointments">
            <div class="guest-appt-loading__bar"></div>
            <div class="guest-appt-loading__bar" style="opacity:0.85"></div>
        </div>`;
    try {
        const raw = await getAppointmentByArtistId(token);
        const list = Array.isArray(raw) ? raw : [];
        const now = new Date();
        const upcoming = list
            .filter((a) => {
                const end = new Date(a.endTime ?? a.EndTime);
                return !Number.isNaN(end.getTime()) && end >= now;
            })
            .sort(
                (a, b) =>
                    new Date(a.startTime ?? a.StartTime) - new Date(b.startTime ?? b.StartTime),
            );

        if (upcoming.length === 0) {
            mount.innerHTML = `
                <div class="guest-appt-empty guest-appt-empty--card">
                    <p class="guest-appt-empty__title">Nincs közelgő időpont</p>
                    <p class="guest-appt-empty__hint">Ha vendégek foglalnak, az időpontok itt jelennek meg.</p>
                </div>`;
            return;
        }

        const timeOpts = { hour: '2-digit', minute: '2-digit' };
        const rows = await Promise.all(
            upcoming.map(async (a) => {
                const guestId = a.guestId ?? a.GuestId;
                const artistId = a.artistId ?? a.ArtistId;
                const serviceIds = a.serviceIds ?? a.ServiceIds ?? [];
                let guestName = '';
                let serviceNames = '';
                try {
                    const names = await getBookingNames({ guestId, artistId, serviceIds });
                    const svc = names.services ?? names.Services ?? [];
                    serviceNames = Array.isArray(svc)
                        ? svc.map((s) => s.name ?? s.Name).filter(Boolean).join(', ')
                        : '';
                    guestName = names.guestName ?? names.GuestName ?? '';
                } catch (err) {
                    console.warn(err);
                }
                const start = new Date(a.startTime ?? a.StartTime);
                const end = new Date(a.endTime ?? a.EndTime);
                const notes = a.notes ?? a.Notes ?? '';
                const dow = start.toLocaleDateString(undefined, { weekday: 'short' });
                const day = String(start.getDate());
                const mon = start.toLocaleDateString(undefined, { month: 'short' });
                const fullDate = start.toLocaleDateString(undefined, {
                    weekday: 'long',
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                });
                const timeRange = `${start.toLocaleTimeString(undefined, timeOpts)} – ${end.toLocaleTimeString(undefined, timeOpts)}`;
                return {
                    dow,
                    day,
                    mon,
                    fullDate,
                    timeRange,
                    guestName,
                    serviceNames,
                    notes,
                };
            }),
        );

        mount.innerHTML = rows
            .map(
                (r, i) => `
            <article class="guest-appt-card async-card-enter" style="--async-card-i:${i}">
                <div class="guest-appt-card__date" aria-hidden="true">
                    <span class="guest-appt-card__dow">${escapeHtml(r.dow)}</span>
                    <span class="guest-appt-card__day">${escapeHtml(r.day)}</span>
                    <span class="guest-appt-card__mon">${escapeHtml(r.mon)}</span>
                </div>
                <div class="guest-appt-card__body">
                    <div class="guest-appt-card__time">${escapeHtml(r.timeRange)}</div>
                    <div class="guest-appt-card__full-date">${escapeHtml(r.fullDate)}</div>
                    <div class="guest-appt-card__artist"><span>Guest</span> ${escapeHtml(r.guestName || 'Guest')}</div>
                    ${
                        r.serviceNames
                            ? `<div class="guest-appt-card__services">${escapeHtml(r.serviceNames)}</div>`
                            : ''
                    }
                    ${
                        r.notes
                            ? `<div class="guest-appt-card__notes">${escapeHtml(r.notes)}</div>`
                            : ''
                    }
                </div>
            </article>`,
            )
            .join('');
    } catch (err) {
        console.error(err);
        mount.innerHTML = `
            <div class="guest-appt-empty guest-appt-empty--card guest-appt-empty--error" role="alert">
                <p class="guest-appt-empty__title">Nem sikerült betölteni az időpontokat</p>
                <p class="guest-appt-empty__hint">Próbáld újra később.</p>
            </div>`;
    }
}

async function LoadArtistData() {
    const token = localStorage.getItem('token');
    const userData = await getLoggedInArtist(token);

    const name = escapeHtml(userData.name ?? '');
    const email = escapeHtml(userData.email ?? '');
    const initials = escapeHtml(userInitials(userData.name ?? ''));

    document.getElementById('artist-profile').innerHTML = `
        <header class="guest-user-hero">
            <div class="guest-user-avatar" aria-hidden="true">${initials}</div>
            <div class="guest-user-hero__text">
                <p class="guest-user-eyebrow">Szakember fiók</p>
                <h1 class="guest-user-title">${name}</h1>
                <p class="guest-user-email">${email}</p>
                <span class="guest-user-role-pill">Szakember</span>
            </div>
        </header>
        ${getProfileEditFormHtml("artist")}
    `;

    fillProfileEditForm(userData, "artist");
    wireProfileEditFormSubmit("artist", token, LoadArtistData);

    const appointmentsMount = document.getElementById('artist-appointments');
    if (appointmentsMount) {
        await loadArtistUpcomingAppointments(token, appointmentsMount);
    }
}


async function LoadOwnerData() {
    const el = document.getElementById("owner-profile");
    if (!el) return;
    let userData;
    try {
        userData = await GetLoggedInOwner(localStorage.getItem("token"));
    } catch {
        return;
    }
    const name = escapeHtml(userData.name ?? "");
    const email = escapeHtml(userData.email ?? "");
    const initials = escapeHtml(userInitials(userData.name ?? ""));

    el.innerHTML = `
        <header class="guest-user-hero">
            <div class="guest-user-avatar" aria-hidden="true">${initials}</div>
            <div class="guest-user-hero__text">
                <p class="guest-user-eyebrow">Tulajdonos fiók</p>
                <h1 class="guest-user-title">${name}</h1>
                <p class="guest-user-email">${email}</p>
                <span class="guest-user-role-pill">Tulajdonos</span>
            </div>
        </header>
        ${getProfileEditFormHtml("owner")}
    `;

    const token = localStorage.getItem("token");
    fillProfileEditForm(userData, "owner");
    wireProfileEditFormSubmit("owner", token, LoadOwnerData);
}

async function LoadArtists() {
    const list = document.getElementById('artists-list');
    if (!list) return;
    const urlParams = new URLSearchParams(window.location.search);
    const salonIdFromUrl = urlParams.get('salonId') || urlParams.get('salonid');
    const serviceidsRaw = urlParams.get('serviceids') || urlParams.get('serviceIds') || '';
    const serviceIdsForBooking = String(serviceidsRaw)
        .split(',')
        .map((x) => Number(String(x).trim()))
        .filter((n) => Number.isFinite(n) && n > 0);
    const bookingStep = window.location.pathname === '/Salon/SelectArtist';
    const filterArtistsByServices = bookingStep && serviceIdsForBooking.length > 0;

    let artists = [];
    if (salonIdFromUrl) {
        try {
            const raw = await GetPublicArtistsForSalon(
                salonIdFromUrl,
                filterArtistsByServices ? serviceIdsForBooking : undefined,
            );
            artists = Array.isArray(raw) ? raw : [];
        } catch (e) {
            console.error(e);
            artists = [];
        }
    } else {
        const token = localStorage.getItem('token');
        if (token) {
            const allArtists = await GetArtists(token);
            artists = Array.isArray(allArtists) ? allArtists : [];
        }
    }

    const bookingBrowseSalon =
        window.location.pathname === '/Salon/Artists' && Boolean(salonIdFromUrl);
    const useArtistPickRows = bookingStep || bookingBrowseSalon;
    list.innerHTML = '';
    if (useArtistPickRows) {
        list.className = 'artists-list artists-pick-list services-pick-list';
        list.setAttribute('role', 'list');
    }
    artists.forEach((x, i) => {
        const name = escapeHtml(x.name ?? '');
        const desc = escapeHtml(x.description ?? '');
        const id = String(x.id);
        if (useArtistPickRows) {
            const metaLine = desc
                ? `<div class="service-pick-row__meta">${desc}</div>`
                : '';
            list.innerHTML += `
                <div class="service-pick-row artist-pick-row async-card-enter" style="--async-card-i:${i}" role="listitem">
                    <div class="service-pick-row__text">
                        <div class="service-pick-row__title">${name}</div>
                        ${metaLine}
                    </div>
                    <label class="service-pick-toggle">
                        <input type="radio" name="booking-artist" class="booking-choice-input service-pick-toggle-input" id="${id}"
                            data-artist-name="${name}"
                            aria-label="${name} — kiválasztás">
                        <span class="service-pick-toggle__face" aria-hidden="true"></span>
                    </label>
                </div>`;
        } else {
            const bookHref = salonIdFromUrl
                ? `/Salon/Services?salonId=${encodeURIComponent(salonIdFromUrl)}&artistId=${encodeURIComponent(id)}`
                : '';
            const bookCta = bookHref
                ? `<a class="btn btn-primary btn-sm artist-card-book flex-shrink-0" href="${bookHref}">Foglalás</a>`
                : '';
            list.innerHTML += `
             <div class="artist-card artist-card--browse async-card-enter" style="--async-card-i:${i}">
                <div class="artist-card-main">
                    <div class="name">${name}</div>
                    <div class="role">${desc}</div>
                </div>
                ${bookCta}
             </div>`;
        }
    });
    const artistsEmptyEl = document.getElementById("artists-page-empty");
    if (artistsEmptyEl) {
        artistsEmptyEl.hidden = artists.length > 0;
        if (artists.length === 0) {
            if (filterArtistsByServices) {
                if (serviceIdsForBooking.length > 1) {
                    try {
                        const svcList = await GetServices(salonIdFromUrl);
                        const arr = Array.isArray(svcList) ? svcList : [];
                        const byId = new Map(
                            arr.map((s) => [Number(s.id ?? s.Id), String(s.name ?? s.Name ?? "").trim()]),
                        );
                        const nameLines = serviceIdsForBooking.map((id) => {
                            const nm = byId.get(id);
                            return nm ? nm : `Szolgáltatás (#${id})`;
                        });
                        const listHtml = nameLines.map((n) => `<li>${escapeHtml(n)}</li>`).join("");
                        artistsEmptyEl.innerHTML = `
<p class="mb-2 fw-semibold">Egy szakember nem tudja egyszerre mindezeket elvégezni.</p>
<p class="mb-2 small">A választott szolgáltatásokhoz valószínűleg különböző szakemberekre van szükség:</p>
<ul class="mb-2 ps-3 small">${listHtml}</ul>
<p class="mb-0 small">Foglalj inkább több külön időpontot (szolgáltatásonként).</p>`;
                    } catch (e) {
                        console.warn(e);
                        artistsEmptyEl.textContent =
                            "Nincs olyan szakember, aki minden kiválasztott szolgáltatást felvéve kínálja. Foglalj több külön időpontot.";
                    }
                } else {
                    artistsEmptyEl.textContent =
                        "Nincs elérhető szakember ehhez a szolgáltatáshoz. Módosítsd a választást.";
                }
            } else {
                artistsEmptyEl.textContent = salonIdFromUrl
                    ? "Ehhez a szalonhoz még nincs közzétett szakember a foglaláshoz."
                    : "Nem érhető el szakember.";
            }
        }
    }
    if (bookingStep) {
        syncArtistPickBar();
    }
    if (bookingBrowseSalon) {
        syncArtistPickBar();
        const cont = document.getElementById('artist-browse-continue-btn');
        if (cont && !cont.dataset.wiredBrowse) {
            cont.dataset.wiredBrowse = '1';
            cont.addEventListener('click', () => {
                const selected = document.querySelector(
                    '#artists-list input.booking-choice-input[type="radio"]:checked',
                );
                if (!selected || !salonIdFromUrl) return;
                window.location.href = `/Salon/Services?salonId=${encodeURIComponent(salonIdFromUrl)}&artistId=${encodeURIComponent(selected.id)}`;
            });
        }
    }
}

async function loadServicesForArtists() {
    const list = document.getElementById("artist-services-list");
    if (!list) return;
    const token = localStorage.getItem("token");
    let services;
    try {
        services = await getServicesByArtist(token);
    } catch (e) {
        console.error(e);
        list.innerHTML =
            '<p class="text-danger mb-0" role="alert">Nem sikerült betölteni a szolgáltatásokat.</p>';
        return;
    }
    list.innerHTML = "";
    if (!Array.isArray(services) || services.length === 0) {
        list.innerHTML =
            '<p class="subtitle mb-0">Még nincs szolgáltatás felvezetve.</p>';
        return;
    }
    services.forEach((x, i) => {
        const svcId = Number(x.id ?? x.Id ?? 0);
        const isImported = Boolean(x.isImported ?? x.IsImported);
        const offeringId = Number(x.offeringId ?? x.OfferingId ?? 0);
        const nameRaw = String(x.name ?? x.Name ?? "").trim();
        const name = escapeHtml(nameRaw);
        const descRaw = x.description ?? x.Description ?? "";
        const descTrim = String(descRaw ?? "").trim();
        const descBlock = descTrim
            ? `<div class="artist-service-desc-wrap"><span class="artist-service-desc">${escapeHtml(descTrim)}</span></div>`
            : '<p class="artist-service-no-desc text-muted small mb-0">Nincs leírás.</p>';
        const mins = Math.max(0, Math.round(Number(x.durationInMinute ?? x.DurationInMinute ?? 0)));
        const price = Number(x.price ?? x.Price ?? 0);
        const priceLine = formatPriceFt(price);
        const nameInputVal = escapeHtml(nameRaw);
        const descInputVal = escapeHtml(descTrim);
        const importBadge = isImported
            ? '<span class="badge bg-secondary ms-2">Tulaj katalógus</span>'
            : "";
        const importActions =
            isImported && offeringId > 0
                ? `<div class="mt-2"><button type="button" class="btn btn-outline-danger btn-sm artist-remove-import-btn" data-offering-id="${offeringId}">Leválasztás a listámról</button></div>`
                : "";
        const editTabNav = isImported
            ? ""
            : `
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="artist-svc-tab-edit-${svcId}" data-bs-toggle="tab" data-bs-target="#artist-svc-pane-edit-${svcId}" type="button" role="tab" aria-controls="artist-svc-pane-edit-${svcId}" aria-selected="false">Szerkesztés</button>
                    </li>`;
        const editTabPane = isImported
            ? ""
            : `
                    <div class="tab-pane fade" id="artist-svc-pane-edit-${svcId}" role="tabpanel" aria-labelledby="artist-svc-tab-edit-${svcId}" tabindex="0">
                        <form class="artist-service-edit-form" data-service-id="${svcId}">
                            <div class="mb-3">
                                <label class="form-label" for="artist-svc-name-${svcId}">Megnevezés</label>
                                <input class="form-control" id="artist-svc-name-${svcId}" name="name" type="text" required maxlength="200" value="${nameInputVal}">
                            </div>
                            <div class="mb-3">
                                <label class="form-label" for="artist-svc-desc-${svcId}">Leírás</label>
                                <textarea class="form-control" id="artist-svc-desc-${svcId}" name="description" rows="3" maxlength="2000">${descInputVal}</textarea>
                            </div>
                            <div class="row g-3 mb-3">
                                <div class="col-sm-6">
                                    <label class="form-label" for="artist-svc-dur-${svcId}">Időtartam (perc)</label>
                                    <input class="form-control" id="artist-svc-dur-${svcId}" name="durationInMinute" type="number" min="1" max="1440" step="1" required value="${mins}">
                                </div>
                                <div class="col-sm-6">
                                    <label class="form-label" for="artist-svc-price-${svcId}">Ár (Ft)</label>
                                    <input class="form-control" id="artist-svc-price-${svcId}" name="price" type="number" min="0" max="10000000" step="1" required value="${Math.round(price)}">
                                </div>
                            </div>
                            <div class="d-flex flex-wrap align-items-center gap-2 flex-wrap">
                                <button type="button" class="btn btn-primary btn-sm adservice-accent-btn artist-service-save-btn">Változtatások elmentése</button>
                                <button type="button" class="btn btn-outline-danger btn-sm artist-service-delete-btn" data-service-id="${svcId}">Szolgáltatás törlése</button>
                                <span class="artist-service-edit-msg small" role="status" aria-live="polite"></span>
                            </div>
                        </form>
                    </div>`;
        list.innerHTML += `
            <div class="service-card artist-service-card async-card-enter" style="--async-card-i:${i}" role="listitem" data-service-id="${svcId}">
                <ul class="nav nav-tabs artist-service-nav-tabs" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active" id="artist-svc-tab-view-${svcId}" data-bs-toggle="tab" data-bs-target="#artist-svc-pane-view-${svcId}" type="button" role="tab" aria-controls="artist-svc-pane-view-${svcId}" aria-selected="true">Részletek</button>
                    </li>${editTabNav}
                </ul>
                <div class="tab-content artist-service-tab-content">
                    <div class="tab-pane fade show active" id="artist-svc-pane-view-${svcId}" role="tabpanel" aria-labelledby="artist-svc-tab-view-${svcId}" tabindex="0">
                        <div class="name">${name}${importBadge}</div>
                        ${descBlock}
                        <div class="artist-service-meta" role="group" aria-label="Időtartam és ár">
                            <span class="artist-service-meta-item">
                                <span class="artist-service-meta-label">Időtartam</span>
                                <strong class="artist-service-meta-value">${mins} perc</strong>
                            </span>
                            <span class="artist-service-meta-item">
                                <span class="artist-service-meta-label">Ár</span>
                                <strong class="artist-service-meta-value">${priceLine}</strong>
                            </span>
                        </div>
                        ${importActions}
                    </div>${editTabPane}
                </div>
            </div>
        `;
    });
    list.querySelectorAll(".artist-remove-import-btn").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const oid = btn.getAttribute("data-offering-id");
            if (!oid || !token) return;
            if (!confirm("Leválasztod ezt a szolgáltatást a saját listádról?")) return;
            btn.disabled = true;
            try {
                await unlinkArtistOffering(token, oid);
                await loadServicesForArtists();
            } catch (err) {
                console.error(err);
                if (!err?.salonicApiShown) {
                    salonicShowModal(err?.message || "Nem sikerült leválasztani.", { title: "Hiba", variant: "danger" });
                }
                btn.disabled = false;
            }
        });
    });

    list.querySelectorAll(".artist-service-delete-btn").forEach((btn) => {
        btn.addEventListener("click", async () => {
            const sid = btn.getAttribute("data-service-id");
            if (!sid || !token) return;
            if (
                !confirm(
                    "Véglegesen törlöd ezt a saját szolgáltatásodat? A foglalásokról lekerül a hivatkozás; a művelet nem vonható vissza.",
                )
            )
                return;
            btn.disabled = true;
            try {
                await deleteArtistService(Number(sid), token);
                await loadServicesForArtists();
            } catch (err) {
                console.error(err);
                if (!err?.salonicApiShown) {
                    salonicShowModal(err?.message || "Törlés sikertelen.", { title: "Hiba", variant: "danger" });
                }
                btn.disabled = false;
            }
        });
    });
}

function scheduleGetMondayOfWeek(d) {
    const date = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    const dow = date.getDay();
    const delta = dow === 0 ? -6 : 1 - dow;
    date.setDate(date.getDate() + delta);
    return date;
}

function scheduleFormatIsoLocal(d) {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
}

function scheduleParseYmd(iso) {
    if (typeof iso !== "string") return null;
    // Expecting "YYYY-MM-DD"
    const parts = iso.split("-");
    if (parts.length !== 3) return null;
    const year = Number(parts[0]);
    const month = Number(parts[1]);
    const day = Number(parts[2]);
    // Check for valid numbers and ranges
    if (
        !Number.isInteger(year) || year < 1000 || year > 9999 ||
        !Number.isInteger(month) || month < 1 || month > 12 ||
        !Number.isInteger(day) || day < 1 || day > 31
    ) {
        return null;
    }
    // Months are 0-indexed in JS
    return new Date(year, month - 1, day, 12, 0, 0, 0);
}

function renderOwnerScheduleGrid(mount, data) {
    if (!mount || !data || !data.days || !data.hours) return;
    const grid = document.createElement('div');
    grid.className = 'schedule-grid';

    const corner = document.createElement('div');
    corner.className = 'schedule-grid-corner';
    corner.setAttribute('aria-hidden', 'true');
    grid.appendChild(corner);

    for (const day of data.days) {
        const head = document.createElement('div');
        head.className = 'schedule-grid-dayhead';
        head.textContent = day.label;
        grid.appendChild(head);
    }

    const hours = data.hours;
    const days = data.days;
    for (const hour of hours) {
        const timeEl = document.createElement('div');
        timeEl.className = 'schedule-grid-time';
        timeEl.textContent = `${hour}:00`;
        grid.appendChild(timeEl);

        for (const day of days) {
            const cell = day.cells.find((c) => c.hour === hour);
            const status = cell ? cell.status : 'closed';
            const cellWrap = document.createElement('div');
            cellWrap.className = 'schedule-grid-cell';

            if (status === 'closed') {
                cellWrap.classList.add('schedule-cell-muted');
                cellWrap.textContent = '—';
            } else if (status === 'booked') {
                const pill = document.createElement('span');
                pill.className = 'schedule-cell-pill schedule-pill-booked';
                pill.textContent = (cell && cell.detail) ? cell.detail : 'Booked';
                cellWrap.appendChild(pill);
            } else {
                const pill = document.createElement('span');
                pill.className = 'schedule-cell-pill schedule-pill-open';
                pill.textContent = 'Open';
                cellWrap.appendChild(pill);
            }
            grid.appendChild(cellWrap);
        }
    }

    mount.innerHTML = '';
    mount.appendChild(grid);
}

function wireOwnerScheduleWeekNav(salonId, artistId, artistName, currentMonday) {
    const prev = document.getElementById('schedule-nav-prev');
    const next = document.getElementById('schedule-nav-next');
    const go = (deltaWeeks) => {
        const d = new Date(currentMonday.getFullYear(), currentMonday.getMonth(), currentMonday.getDate());
        d.setDate(d.getDate() + 7 * deltaWeeks);
        const qs = new URLSearchParams({
            salonId: String(salonId),
            artistId: String(artistId),
            weekStart: scheduleFormatIsoLocal(d)
        });
        if (artistName) qs.set('artistName', artistName);
        window.location.assign(`/salondashboard/schedule?${qs.toString()}`);
    };
    if (prev) {
        prev.disabled = false;
        prev.removeAttribute('title');
        prev.addEventListener('click', () => go(-1));
    }
    if (next) {
        next.disabled = false;
        next.removeAttribute('title');
        next.addEventListener('click', () => go(1));
    }
}

async function loadOwnerArtistSchedulePage() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get('salonId');
    const artistId = params.get('artistId');
    const rawName = params.get('artistName') || '';
    let artistName = rawName;
    try {
        if (rawName) artistName = decodeURIComponent(rawName);
    } catch (_) { /* keep encoded */ }
    const weekStartParam = params.get('weekStart');

    const mount = document.getElementById('schedule-grid-mount');

    const scheduleBack = document.getElementById('schedule-back');
    if (scheduleBack && salonId) {
        scheduleBack.href = `/salondashboard?salonId=${encodeURIComponent(salonId)}`;
    }

    const labelEl = document.querySelector('.salon-schedule-header .salon-schedule-label');
    if (labelEl) labelEl.textContent = 'Weekly availability';

    const artistEl = document.getElementById('schedule-artist-label');
    if (artistEl) artistEl.textContent = artistName || 'Artist';

    if (!salonId || !artistId) {
        if (mount) mount.innerHTML = '<p class="subtitle" role="alert">Missing salon or artist in the URL.</p>';
        return;
    }

    let monday;
    const parsedWeek = weekStartParam ? scheduleParseYmd(weekStartParam) : null;
    if (parsedWeek) {
        monday = scheduleGetMondayOfWeek(parsedWeek);
    } else {
        monday = scheduleGetMondayOfWeek(new Date());
    }

    const weekLabel = document.getElementById('schedule-week-label');
    if (weekLabel) {
        weekLabel.textContent = `Week of ${monday.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })}`;
    }

    if (mount) mount.innerHTML = '<p class="subtitle">Loading schedule…</p>';

    const token = localStorage.getItem('token');
    const weekStartIso = scheduleFormatIsoLocal(monday);

    try {
        const data = await GetOwnerArtistWeekSchedule(token, salonId, artistId, weekStartIso);
        renderOwnerScheduleGrid(mount, data);
        wireOwnerScheduleWeekNav(salonId, artistId, artistName, monday);
    } catch (err) {
        console.error(err);
        if (err?.salonicApiShown) return;
        if (mount) {
            const msg = err?.message || "Could not load schedule.";
            mount.innerHTML = `<p class="subtitle" role="alert">${msg}</p>`;
        }
    }
}

window.AddToCart = AddToCart;
window.SelectArtist = SelectArtist;
