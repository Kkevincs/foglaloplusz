/**
 * Shared UI and data helpers for Salonic front-end. API calls live in api.js.
 */

function enforceOwnerRoleOrRedirect() {
    const ownerOnlyPaths = ["/maker", "/owneruserpage", "/salondashboard", "/salondashboard/schedule", "/ownerartistregister"];
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
    await applySalonPersonalLogo();
}

document.addEventListener("DOMContentLoaded", async () => {
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
        path === "/ownerartistregister";
    if (applyThemeHere) {
        await applySalonThemeForPublicPages();
        await applySalonPersonalLogo();
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

async function applySalonPersonalHomepage() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
    document.getElementById('salon-name').textContent = salon.name;
    document.getElementById('salon-description').textContent = salon.description;

    const emailRaw = salon.email ?? salon.Email ?? '';
    const ownerName = salon.ownerName ?? salon.OwnerName ?? '';
    const locationStr = salon.location ?? salon.Location ?? '';
    const footerMeta = document.getElementById('salon-footer-meta');
    if (footerMeta) {
        const parts = [];
        if (ownerName && String(ownerName).trim()) {
            parts.push(
                `<span class="salon-landing-footer__item"><span class="salon-landing-footer__label">Tulajdonos</span> ${escapeHtml(String(ownerName).trim())}</span>`,
            );
        }
        if (emailRaw && String(emailRaw).trim()) {
            const em = String(emailRaw).trim();
            parts.push(
                `<span class="salon-landing-footer__item"><span class="salon-landing-footer__label">E-mail</span> <a href="mailto:${em.replace(/"/g, "")}">${escapeHtml(em)}</a></span>`,
            );
        }
        if (locationStr && String(locationStr).trim()) {
            parts.push(
                `<span class="salon-landing-footer__item"><span class="salon-landing-footer__label">Helyszín</span> ${escapeHtml(String(locationStr).trim())}</span>`,
            );
        }
        footerMeta.innerHTML = parts.join(
            '<span class="salon-landing-footer__sep" aria-hidden="true"> · </span>',
        );
    }
    const supportMail = document.getElementById('salon-support-email');
    if (supportMail && emailRaw) {
        supportMail.href = `mailto:${emailRaw}`;
        supportMail.textContent = emailRaw;
    }

    const sq = salonId ? `?salonId=${encodeURIComponent(salonId)}` : '';
    const guestLogin = document.getElementById('salon-guest-login-link');
    if (guestLogin) guestLogin.href = `/Salon/Login${sq}`;
    const artistLogin = document.getElementById('salon-artist-login-link');
    if (artistLogin) artistLogin.href = `/Salon/Artist/Login${sq}`;
    const services = document.getElementById('salon-link-services');
    if (services) services.href = `/Salon/Services${sq}`;
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
const SALON_PUBLIC_API_ORIGIN = "https://localhost:7156";

function resolvePublicSalonLogoUrl(salon) {
    const raw = salon?.logo ?? salon?.Logo ?? "";
    if (!raw || !String(raw).trim()) return "";
    const p = String(raw).trim();
    if (/^https?:\/\//i.test(p)) return p;
    return `${SALON_PUBLIC_API_ORIGIN}${p.startsWith("/") ? p : `/${p}`}`;
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
        logoEl.textContent = name;
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

function applyTheme(theme) {
    if (!theme) return;
    document.body.className = document.body.className
        .split(' ')
        .filter(cls => !cls.startsWith('theme-'))
        .join(' ');
    document.body.classList.add(theme.trim());
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
        const response = await fetch(`https://localhost:7156/api/salon/${id}/exists`);
        if (!response.ok) {
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
            salonId: id,
            description: description
        }

        await RegisterArtist(artist);
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
    const holder = document.getElementById('artist-cart-holder');
    const btn = document.getElementById('artistContinueBtn');
    const nameEl = document.getElementById('artist-cart-count');
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

async function loadServicesBySalon(id) {
    const services = await GetServices(id)
    services.forEach(x => 
        document.getElementById("services-table").innerHTML += `
                        <tr>
                            <td>${x.name}</td>
                            <td>${x.durationInMinute} min</td>
                            <td>${x.price} Ft</td>
                        </tr>
        `
    )
}




async function loadArtistsBySalonId(id) {
    const artists = await getArtistBySalonId(localStorage.getItem('token'), id)
    const salonIdEnc = encodeURIComponent(id);
    artists.forEach(x => {
        const nameEnc = encodeURIComponent(x.name || 'Artist');
        document.getElementById('salon-dash-artists').innerHTML += `
            <li class="salon-dash-artist-card">
                    <div>
                        <h3 class="salon-dash-card-name">${x.name}</h3>
                        <p class="salon-dash-card-meta">${x.description}</p>
                    </div>
                    <a href="/salondashboard/schedule?salonId=${salonIdEnc}&artistId=${x.id}&artistName=${nameEnc}" class="btn btn-primary btn-sm">View schedule</a>
                </li>
        `;
    });
}

async function loadSalons(){
    const list = document.getElementById("owner-salons-list");
    if (!list) return;

    const token = localStorage.getItem('token');
    const salons = await GetMySalons(token);
    list.innerHTML = '';
    salons.forEach(x =>
        list.innerHTML += `
            <li class="owner-salon-card">
                    <div class="owner-salon-main">
                        <h3 class="owner-salon-name">${x.name}</h3>
                        <p class="owner-salon-meta">${x.email}</p>
                        <p class="owner-salon-desc">${x.description}</p>
                    </div>
                    <div class="owner-salon-actions">
                        <a href="/salondashboard?salonId=${x.id}" class="btn btn-primary btn-sm">Open dashboard</a>
                        <a href="/ownerartistregister?salonId=${x.id}" class="btn btn-outline-secondary btn-sm">Edit</a>
                        <button type="button" class="btn btn-outline-danger btn-sm" data-action="remove-salon" data-salon-id="${x.id}">Remove salon</button>
                    </div>
                </li>
        `
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
        salons.forEach((salon) => {
            const salonId = salon.id ?? salon.Id;
            const name = escapeHtml(salon.name ?? salon.Name ?? `Salon ${salonId}`);
            list.innerHTML += `
                <a class="btn btn-primary w-100" href="/Salon?salonId=${encodeURIComponent(salonId)}">${name}</a>
            `;
        });
    } catch (err) {
        console.error(err);
        list.innerHTML = '<p class="text-danger small mb-0" role="alert">Could not load salons from the database.</p>';
    }
}

async function LoadServices() {
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
        services = await GetServices(salonId);
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
            emptyEl.textContent = 'Ehhez a szalonhoz még nincs közzétett szolgáltatás.';
        }
        syncServicesCartButton();
        return;
    }
    arr.forEach((x) => {
        const name = escapeHtml(x.name ?? x.Name);
        const id = String(x.id ?? x.Id);
        const durationMin = Number(x.durationInMinute ?? x.DurationInMinute ?? 0);
        const price = Number(x.price ?? x.Price ?? 0);
        const durationLine = formatDurationMinutesHu(durationMin);
        const priceLine = formatPriceFt(price);
        list.innerHTML += `
            <div class="service-pick-row" role="listitem">
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
    `;

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
        <div class="guest-user-meta-grid">
            <div class="guest-meta-tile">
                <span class="guest-meta-tile__label">Név</span>
                <span class="guest-meta-tile__value" id="user-name">${name}</span>
            </div>
            <div class="guest-meta-tile">
                <span class="guest-meta-tile__label">E-mail</span>
                <span class="guest-meta-tile__value" id="user-email">${email}</span>
            </div>
        </div>
    `;

    const appointmentsMount = document.getElementById('artist-appointments');
    if (appointmentsMount) {
        await loadArtistUpcomingAppointments(token, appointmentsMount);
    }
}


async function LoadOwnerData() {
    const el = document.getElementById("owner-profile");
    if (!el) return;
    const userData = await GetLoggedInOwner(localStorage.getItem("token"));
    const name = escapeHtml(userData.name ?? "");
    const email = escapeHtml(userData.email ?? "");
    const initials = escapeHtml(userInitials(userData.name ?? ""));

    el.innerHTML = `
        <header class="guest-user-hero">
            <div class="guest-user-avatar" aria-hidden="true">${initials}</div>
            <div class="guest-user-hero__text">
                <p class="guest-user-eyebrow">Salon owner</p>
                <h1 class="guest-user-title">${name}</h1>
                <p class="guest-user-email">${email}</p>
                <span class="guest-user-role-pill">Owner</span>
            </div>
        </header>
        <div class="guest-user-meta-grid">
            <div class="guest-meta-tile">
                <span class="guest-meta-tile__label">Name</span>
                <span class="guest-meta-tile__value">${name}</span>
            </div>
            <div class="guest-meta-tile">
                <span class="guest-meta-tile__label">Email</span>
                <span class="guest-meta-tile__value">${email}</span>
            </div>
        </div>
    `;
}

async function LoadArtists() {
    const list = document.getElementById('artists-list');
    if (!list) return;
    const urlParams = new URLSearchParams(window.location.search);
    const salonIdFromUrl = urlParams.get('salonId') || urlParams.get('salonid');

    let artists = [];
    if (salonIdFromUrl) {
        try {
            const raw = await GetPublicArtistsForSalon(salonIdFromUrl);
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

    const bookingStep = window.location.pathname === '/Salon/SelectArtist';
    list.innerHTML = '';
    if (bookingStep) {
        list.className = 'artists-list artists-pick-list services-pick-list';
        list.setAttribute('role', 'list');
    }
    artists.forEach((x) => {
        const name = escapeHtml(x.name ?? '');
        const desc = escapeHtml(x.description ?? '');
        const id = String(x.id);
        if (bookingStep) {
            const metaLine = desc
                ? `<div class="service-pick-row__meta">${desc}</div>`
                : '';
            list.innerHTML += `
                <div class="service-pick-row artist-pick-row" role="listitem">
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
                ? `<a class="btn btn-primary btn-sm artist-card-book flex-shrink-0" href="${bookHref}">Book services</a>`
                : '';
            list.innerHTML += `
             <div class="artist-card artist-card--browse">
                <div class="artist-card-main">
                    <div class="name">${name}</div>
                    <div class="role">${desc}</div>
                </div>
                ${bookCta}
             </div>`;
        }
    });
    if (bookingStep) {
        syncArtistPickBar();
    }
}

async function loadServicesForArtists() {
    const list = document.getElementById("artist-services-list");
    if (!list) return;
    const services = await getServicesByArtist(localStorage.getItem("token"));
    list.innerHTML = "";
    if (!Array.isArray(services)) return;
    services.forEach((x) => {
        const desc = x.description
            ? `<span class="artist-service-desc">${x.description}</span>`
            : "";
        list.innerHTML += `
            <li>
                <span><strong>${x.name}</strong>${desc}</span>
                <strong>${x.durationInMinute} min ${x.price} Ft</strong>
            </li>
        `;
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
        if (mount) {
            const msg = err && err.message ? err.message : 'Could not load schedule.';
            mount.innerHTML = `<p class="subtitle" role="alert">${msg}</p>`;
        }
    }
}

window.AddToCart = AddToCart;
window.SelectArtist = SelectArtist;
