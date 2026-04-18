/**
 * Alap URL (felülírható: window.SALONIC_API_BASE).
 * Üres = ugyanazon a hoston lévő /api (Express továbbítja a dotnet API felé — lásd server.js).
 */
const SALONIC_API_BASE =
    typeof window !== "undefined" && window.SALONIC_API_BASE !== undefined && window.SALONIC_API_BASE !== null
        ? String(window.SALONIC_API_BASE)
        : "";

function salonicApiUrl(path) {
    const p = path.startsWith("/") ? path : `/${path}`;
    return `${String(SALONIC_API_BASE).replace(/\/$/, "")}${p}`;
}

function parseErrorMessageFromText(text, status) {
    const fallback = `HTTP ${status}`;
    const raw = String(text ?? "").trim();
    if (!raw) return fallback;
    try {
        const j = JSON.parse(raw);
        if (typeof j === "string") return j;
        if (j && typeof j === "object") {
            if (j.message) return String(j.message);
            if (j.detail) return String(j.detail);
            if (j.title && j.title !== "One or more validation errors failed.") return String(j.title);
            if (j.errors && typeof j.errors === "object") {
                const parts = [];
                for (const [k, v] of Object.entries(j.errors)) {
                    const msg = Array.isArray(v) ? v.join(", ") : String(v);
                    parts.push(`${k}: ${msg}`);
                }
                if (parts.length) return parts.join("\n");
            }
        }
    } catch {
        /* plain text */
    }
    return raw.length > 800 ? `${raw.slice(0, 797)}…` : raw;
}

function createSalonicApiError(message, response) {
    const err = new Error(message);
    err.name = "SalonicApiError";
    err.salonicApiShown = true;
    if (response) err.status = response.status;
    return err;
}

const SALONIC_UNIFIED_MODAL_ID = "salonic-unified-modal";

const SALONIC_MODAL_HEADER_CLASS = {
    danger: "bg-danger text-white border-0",
    success: "bg-success text-white border-0",
    warning: "bg-warning border-0",
    info: "bg-info border-0",
    primary: "bg-primary text-white border-0",
};

/**
 * Egységes üzenetablak (hiba, információ, siker). Bootstrap Modal, ha elérhető; különben egyszerű megjelenítés alert nélkül.
 * @param {string} message
 * @param {{ title?: string, variant?: keyof SALONIC_MODAL_HEADER_CLASS }} [options]
 */
function salonicShowModal(message, options) {
    const opts = options || {};
    const title = opts.title != null ? String(opts.title) : "Értesítés";
    const variant = Object.prototype.hasOwnProperty.call(SALONIC_MODAL_HEADER_CLASS, opts.variant) ? opts.variant : "primary";
    let modalEl = document.getElementById(SALONIC_UNIFIED_MODAL_ID);
    if (!modalEl) {
        modalEl = document.createElement("div");
        modalEl.id = SALONIC_UNIFIED_MODAL_ID;
        modalEl.className = "modal fade";
        modalEl.setAttribute("tabindex", "-1");
        modalEl.setAttribute("aria-hidden", "true");
        modalEl.innerHTML = `
<div class="modal-dialog modal-dialog-centered">
  <div class="modal-content shadow">
    <div class="modal-header salonic-u-header border-0">
      <h5 class="modal-title salonic-u-title">Cím</h5>
      <button type="button" class="btn-close salonic-u-close" data-salonic-dismiss="1" data-bs-dismiss="modal" aria-label="Bezárás"></button>
    </div>
    <div class="modal-body pt-3">
      <p class="mb-0 salonic-u-message" style="white-space: pre-wrap;"></p>
    </div>
    <div class="modal-footer border-0 pt-0">
      <button type="button" class="btn btn-light border salonic-u-ok" data-salonic-dismiss="1" data-bs-dismiss="modal">Rendben</button>
    </div>
  </div>
</div>`;
        document.body.appendChild(modalEl);
        modalEl.addEventListener("click", (e) => {
            if (e.target.closest("[data-salonic-dismiss]")) salonicHideModal();
        });
    }
    const header = modalEl.querySelector(".salonic-u-header");
    const titleEl = modalEl.querySelector(".salonic-u-title");
    const msgEl = modalEl.querySelector(".salonic-u-message");
    const closeBtn = modalEl.querySelector(".salonic-u-close");
    const headerClass = SALONIC_MODAL_HEADER_CLASS[variant] || SALONIC_MODAL_HEADER_CLASS.primary;
    if (header) header.className = "modal-header salonic-u-header border-0 " + headerClass;
    if (closeBtn) {
        const dark = variant === "danger" || variant === "success" || variant === "primary";
        closeBtn.classList.toggle("btn-close-white", dark);
    }
    if (titleEl) titleEl.textContent = title;
    if (msgEl) msgEl.textContent = String(message != null ? message : "");
    if (typeof window.bootstrap !== "undefined" && window.bootstrap.Modal) {
        window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
        return;
    }
    modalEl.classList.add("show");
    modalEl.style.display = "block";
    modalEl.removeAttribute("aria-hidden");
    modalEl.setAttribute("aria-modal", "true");
    let bd = document.getElementById("salonic-modal-fallback-backdrop");
    if (!bd) {
        bd = document.createElement("div");
        bd.id = "salonic-modal-fallback-backdrop";
        bd.className = "modal-backdrop fade show";
        document.body.appendChild(bd);
    }
    document.body.classList.add("modal-open");
}

function salonicHideModal() {
    const modalEl = document.getElementById(SALONIC_UNIFIED_MODAL_ID);
    if (!modalEl) return;
    if (typeof window.bootstrap !== "undefined" && window.bootstrap.Modal) {
        const inst = window.bootstrap.Modal.getInstance(modalEl);
        if (inst) inst.hide();
        return;
    }
    modalEl.classList.remove("show");
    modalEl.style.display = "none";
    modalEl.setAttribute("aria-hidden", "true");
    document.getElementById("salonic-modal-fallback-backdrop")?.remove();
    document.body.classList.remove("modal-open");
}

const SALONIC_CONFIRM_MODAL_ID = "salonic-confirm-modal";

/**
 * Kétlépéses megerősítés (nem window.confirm).
 * @param {{ title?: string, message: string, confirmText?: string, cancelText?: string, danger?: boolean }} options
 * @returns {Promise<boolean>} true = megerősítve
 */
function salonicConfirmModal(options) {
    const opts = options || {};
    const message = String(opts.message ?? "");
    const title = opts.title != null ? String(opts.title) : "Megerősítés";
    const confirmText = opts.confirmText != null ? String(opts.confirmText) : "OK";
    const cancelText = opts.cancelText != null ? String(opts.cancelText) : "Mégse";
    const danger = Boolean(opts.danger);

    return new Promise((resolve) => {
        let modalEl = document.getElementById(SALONIC_CONFIRM_MODAL_ID);
        if (!modalEl) {
            modalEl = document.createElement("div");
            modalEl.id = SALONIC_CONFIRM_MODAL_ID;
            modalEl.className = "modal fade";
            modalEl.setAttribute("tabindex", "-1");
            modalEl.setAttribute("aria-hidden", "true");
            modalEl.innerHTML = `
<div class="modal-dialog modal-dialog-centered">
  <div class="modal-content shadow">
    <div class="modal-header bg-light border-0">
      <h5 class="modal-title salonic-cf-title">Cím</h5>
      <button type="button" class="btn-close salonic-cf-x" data-bs-dismiss="modal" aria-label="Bezárás"></button>
    </div>
    <div class="modal-body pt-2">
      <p class="mb-0 salonic-cf-message" style="white-space: pre-wrap;"></p>
    </div>
    <div class="modal-footer border-0 gap-2">
      <button type="button" class="btn btn-outline-secondary salonic-cf-no">Mégse</button>
      <button type="button" class="btn salonic-cf-yes">OK</button>
    </div>
  </div>
</div>`;
            document.body.appendChild(modalEl);
        }

        const titleEl = modalEl.querySelector(".salonic-cf-title");
        const msgEl = modalEl.querySelector(".salonic-cf-message");
        const yesBtn = modalEl.querySelector(".salonic-cf-yes");
        const noBtn = modalEl.querySelector(".salonic-cf-no");
        const xBtn = modalEl.querySelector(".salonic-cf-x");

        if (titleEl) titleEl.textContent = title;
        if (msgEl) msgEl.textContent = message;
        if (yesBtn) {
            yesBtn.textContent = confirmText;
            yesBtn.className = "btn salonic-cf-yes " + (danger ? "btn-danger" : "btn-primary");
        }
        if (noBtn) noBtn.textContent = cancelText;

        let settled = false;
        function complete(val) {
            if (settled) return;
            settled = true;
            resolve(val);
            if (typeof window.bootstrap !== "undefined" && window.bootstrap.Modal) {
                const inst = window.bootstrap.Modal.getInstance(modalEl);
                if (inst) inst.hide();
            } else {
                modalEl.classList.remove("show");
                modalEl.style.display = "none";
                modalEl.setAttribute("aria-hidden", "true");
                document.getElementById("salonic-confirm-fallback-backdrop")?.remove();
                document.body.classList.remove("modal-open");
            }
        }

        function onHidden() {
            if (!settled) {
                settled = true;
                resolve(false);
            }
        }

        if (typeof window.bootstrap !== "undefined" && window.bootstrap.Modal) {
            yesBtn?.addEventListener("click", () => complete(true), { once: true });
            noBtn?.addEventListener("click", () => complete(false), { once: true });
            xBtn?.addEventListener("click", () => complete(false), { once: true });
            modalEl.addEventListener("hidden.bs.modal", onHidden, { once: true });
            window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
            return;
        }

        yesBtn?.addEventListener("click", () => complete(true), { once: true });
        noBtn?.addEventListener("click", () => complete(false), { once: true });
        xBtn?.addEventListener("click", () => complete(false), { once: true });

        modalEl.classList.add("show");
        modalEl.style.display = "block";
        modalEl.removeAttribute("aria-hidden");
        modalEl.setAttribute("aria-modal", "true");
        let bd = document.getElementById("salonic-confirm-fallback-backdrop");
        if (!bd) {
            bd = document.createElement("div");
            bd.id = "salonic-confirm-fallback-backdrop";
            bd.className = "modal-backdrop fade show";
            document.body.appendChild(bd);
        }
        bd.addEventListener(
            "click",
            () => {
                complete(false);
            },
            { once: true },
        );
        document.body.classList.add("modal-open");
    });
}

function showSalonicApiErrorModal(message) {
    salonicShowModal(message, { title: "Hiba", variant: "danger" });
}

if (typeof window !== "undefined") {
    window.salonicShowModal = salonicShowModal;
    window.salonicHideModal = salonicHideModal;
    window.salonicConfirmModal = salonicConfirmModal;
    window.showSalonicApiErrorModal = showSalonicApiErrorModal;
    window.SALONIC_API_BASE = SALONIC_API_BASE;
}

/**
 * JSON válasz; csak sikeres (response.ok) esetén tér vissza adattal.
 * @throws {Error} SalonicApiError — modal már megjelent
 */
async function salonicFetchJson(url, init) {
    const response = await fetch(url, init);
    const text = await response.text();
    if (!response.ok) {
        const detail = parseErrorMessageFromText(text, response.status);
        showSalonicApiErrorModal(detail);
        throw createSalonicApiError(detail, response);
    }
    if (!text || !text.trim()) return null;
    try {
        return JSON.parse(text);
    } catch {
        showSalonicApiErrorModal("Érvénytelen JSON válasz a szervertől.");
        throw createSalonicApiError("Érvénytelen JSON válasz a szervertől.", response);
    }
}

/** Szöveg / JWT; csak sikeres válasz esetén. */
async function salonicFetchText(url, init) {
    const response = await fetch(url, init);
    const text = await response.text();
    if (!response.ok) {
        const detail = parseErrorMessageFromText(text, response.status);
        showSalonicApiErrorModal(detail);
        throw createSalonicApiError(detail, response);
    }
    return text;
}

/** Üres törzs elfogadott (204 / üres 200). */
async function salonicFetchVoid(url, init) {
    const response = await fetch(url, init);
    const text = await response.text();
    if (!response.ok) {
        const detail = parseErrorMessageFromText(text, response.status);
        showSalonicApiErrorModal(detail);
        throw createSalonicApiError(detail, response);
    }
}

/** JSON vagy üres; siker esetén parseolt objektum vagy null. */
async function salonicFetchJsonLenient(url, init) {
    const response = await fetch(url, init);
    const text = await response.text();
    if (!response.ok) {
        let data = null;
        try {
            data = text ? JSON.parse(text) : null;
        } catch {
            data = null;
        }
        const detail =
            (typeof data === "object" && data !== null && (data.message || data.title || data.detail)) ||
            parseErrorMessageFromText(text, response.status);
        showSalonicApiErrorModal(String(detail));
        throw createSalonicApiError(String(detail), response);
    }
    if (!text || !text.trim()) return null;
    try {
        return JSON.parse(text);
    } catch {
        return text;
    }
}

function normalizeAuthToken(raw) {
    if (!raw) return raw;
    const trimmed = raw.trim();
    if (trimmed.startsWith('"') && trimmed.endsWith('"')) {
        try {
            return JSON.parse(trimmed);
        } catch (_) {
            return trimmed.slice(1, -1);
        }
    }
    return trimmed;
}

/**
 * Szalon létezik-e (204 = igen, 404 = nem). Egyéb státusz: modal + throw.
 * @returns {Promise<boolean>}
 */
async function CheckSalonExists(salonId) {
    const response = await fetch(salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}/exists`));
    if (response.status === 204) return true;
    if (response.status === 404) return false;
    const text = await response.text();
    const detail = parseErrorMessageFromText(text, response.status);
    showSalonicApiErrorModal(detail);
    throw createSalonicApiError(detail, response);
}

async function getServicesByArtist(token) {
    return salonicFetchJson(salonicApiUrl("/api/Artist/services"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function Login(User) {
    const response = await fetch(salonicApiUrl("/api/authentication/authenticate"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(User),
    });
    const data = await response.text();
    if (!response.ok) {
        const detail = parseErrorMessageFromText(data, response.status);
        showSalonicApiErrorModal(detail);
        return null;
    }
    return normalizeAuthToken(data);
}

async function LoginWithStatus(User) {
    const response = await fetch(salonicApiUrl("/api/authentication/authenticate"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(User),
    });
    const body = await response.text();
    const token = normalizeAuthToken(body);
    if (!response.ok) {
        const detail = parseErrorMessageFromText(body, response.status);
        showSalonicApiErrorModal(detail);
        return { ok: false, token: null };
    }
    return { ok: true, token };
}

async function GetArtistWorkingHoursByMonth(token, date) {
    return salonicFetchJson(salonicApiUrl("/api/workinghours/range"), {
        method: "PATCH",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify(date),
    });
}

async function GetAvaliableSlots(timeData) {
    return salonicFetchJson(salonicApiUrl("/api/appointment/available"), {
        method: "PATCH",
        headers: { "content-type": "application/json" },
        body: JSON.stringify(timeData),
    });
}

async function Register(User) {
    return salonicFetchJson(salonicApiUrl("/api/register/guest"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(User),
    });
}

async function RegisterOwner(User) {
    const response = await fetch(salonicApiUrl("/api/register/owner"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(User),
    });
    const text = await response.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch (_) {
        data = null;
    }
    if (!response.ok) {
        const detail =
            (data && (data.message || data.title || data.detail)) || parseErrorMessageFromText(text, response.status);
        showSalonicApiErrorModal(String(detail));
        return { ok: false, data };
    }
    return { ok: true, data };
}

async function RegisterArtist(user) {
    const response = await fetch(salonicApiUrl("/api/register/artist"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(user),
    });
    const text = await response.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch (_) {
        data = null;
    }
    if (!response.ok) {
        const detail =
            (data && (data.message || data.title || data.detail)) || parseErrorMessageFromText(text, response.status);
        showSalonicApiErrorModal(String(detail));
        return { ok: false, data };
    }
    return { ok: true, data };
}

/** Nyilvános / vendég: csak Offeringhez kötött szolgáltatások a szalonban. */
async function GetServices(salonId) {
    return salonicFetchJson(salonicApiUrl(`/api/salon/${salonId}/services`));
}

/** Tulaj vagy szakember: teljes szalon-katalógus (összes Service sor), kezelőfelülethez. */
async function GetSalonServicesCatalog(salonId, token) {
    return salonicFetchJson(salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}/services/catalog`), {
        headers: { authorization: `Bearer ${token}` },
    });
}

/** Vendég / foglalás: az adott szakember által kínált szolgáltatások a szalonban (Offering alapú). */
async function GetServicesForArtistInSalon(salonId, artistId) {
    return salonicFetchJson(
        salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}/services/for-artist/${encodeURIComponent(artistId)}`),
    );
}

/**
 * @param {string|number} salonId
 * @param {number[]|undefined} mustOfferServiceIds Ha megadott: csak olyan szakemberek, akiknek minden szolgáltatás szerepel az offeringek között.
 */
async function GetPublicArtistsForSalon(salonId, mustOfferServiceIds) {
    let qs = "";
    if (Array.isArray(mustOfferServiceIds) && mustOfferServiceIds.length > 0) {
        const ids = mustOfferServiceIds.filter((n) => Number.isFinite(n) && n > 0);
        if (ids.length > 0) {
            qs = `?serviceIds=${encodeURIComponent(ids.join(","))}`;
        }
    }
    return salonicFetchJson(salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}/public-artists${qs}`));
}

async function getBookingNames(ids) {
    return salonicFetchJson(salonicApiUrl("/api/lookup/names"), {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify(ids),
    });
}

async function GetLoggedInUser(token) {
    return salonicFetchJson(salonicApiUrl("/api/guest/me"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

/** @param salon {{ name: string, email: string, theme: string, description: string, location?: string, logoFile?: File | null }} */
async function AddSalon(token, salon) {
    const formData = new FormData();
    formData.append("name", salon.name);
    formData.append("email", salon.email);
    formData.append("theme", salon.theme ?? "");
    formData.append("description", salon.description ?? "");
    if (salon.location != null && String(salon.location).trim() !== "") {
        formData.append("location", String(salon.location).trim());
    }
    if (salon.logoFile instanceof File) {
        formData.append("logo", salon.logoFile);
    }
    return salonicFetchJsonLenient(salonicApiUrl("/api/salon"), {
        method: "POST",
        headers: { authorization: `Bearer ${token}` },
        body: formData,
    });
}

/** Tulajdonos: meglévő szalon módosítása (név, e-mail, téma, leírás, helyszín, opcionális új logo). */
async function UpdateSalon(token, salonId, salon) {
    const formData = new FormData();
    formData.append("name", salon.name);
    formData.append("email", salon.email);
    formData.append("theme", salon.theme ?? "");
    formData.append("description", salon.description ?? "");
    if (salon.location != null && String(salon.location).trim() !== "") {
        formData.append("location", String(salon.location).trim());
    }
    if (salon.logoFile instanceof File) {
        formData.append("logo", salon.logoFile);
    }
    return salonicFetchJsonLenient(salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}`), {
        method: "PUT",
        headers: { authorization: `Bearer ${token}` },
        body: formData,
    });
}

async function GetMySalons(token) {
    return salonicFetchJson(salonicApiUrl("/api/salon/my"), {
        method: "GET",
        headers: { authorization: `Bearer ${token}` },
    });
}

async function GetPublicSalons() {
    return salonicFetchJson(salonicApiUrl("/api/salon/public"));
}

async function DeleteSalon(token, salonId) {
    await salonicFetchVoid(salonicApiUrl(`/api/salon/${salonId}`), {
        method: "DELETE",
        headers: { authorization: `Bearer ${token}` },
    });
    return true;
}

async function getLoggedInArtist(token) {
    return salonicFetchJson(salonicApiUrl("/api/artist/me"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function getArtistBySalonId(token, id) {
    return salonicFetchJson(salonicApiUrl(`/api/salon/${id}/artists`), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function GetLoggedInOwner(token) {
    return salonicFetchJson(salonicApiUrl("/api/owner/me"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

/** @param {"guest"|"owner"|"artist"} role */
async function updateMyUserProfile(role, token, body) {
    const path =
        role === "guest" ? "/api/guest/me" : role === "owner" ? "/api/owner/me" : "/api/artist/me";
    return salonicFetchJsonLenient(salonicApiUrl(path), {
        method: "PUT",
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
    });
}

async function GetArtists(token) {
    return salonicFetchJson(salonicApiUrl("/api/Artist"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function getAppointmentByArtistId(token) {
    return salonicFetchJson(salonicApiUrl("/api/appointment/artist"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function getAppointmentByGuestId(token) {
    return salonicFetchJson(salonicApiUrl("/api/appointment/guest"), {
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function cancelGuestAppointment(token, appointmentId) {
    await salonicFetchVoid(salonicApiUrl(`/api/appointment/guest/${encodeURIComponent(appointmentId)}`), {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
    });
}

async function getServiceById(salonId, id, token) {
    return salonicFetchJson(salonicApiUrl(`/api/salon/${salonId}/services/${id}`), {
        headers: { authorization: `Bearer ${token}` },
    });
}

async function PostAppointment(appointment, token) {
    return salonicFetchJsonLenient(salonicApiUrl("/api/appointment"), {
        method: "POST",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify(appointment),
    });
}

async function GetAppointmentBySalonId(token, id) {
    return salonicFetchJson(salonicApiUrl(`/api/appointment/salon/${id}`), {
        headers: { authorization: `Bearer ${token}` },
    });
}

/** @param weekStartIso {string|undefined} Monday as yyyy-MM-dd, optional */
async function GetOwnerArtistWeekSchedule(token, salonId, artistId, weekStartIso) {
    const qs = weekStartIso ? `?weekStart=${encodeURIComponent(weekStartIso)}` : "";
    return salonicFetchJson(salonicApiUrl(`/api/salon/${salonId}/artists/${artistId}/schedule${qs}`), {
        headers: { authorization: `Bearer ${token}` },
    });
}

async function GetAvailableDays(date) {
    return salonicFetchJson(salonicApiUrl("/api/availability/month"), {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify(date),
    });
}

/** Havi szabad napok (mely napokon van legalább egy szabad időpont). */
async function GetMonthAvailability({ artistId, serviceIds, year, month }) {
    return salonicFetchJson(salonicApiUrl("/api/availability/month"), {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ artistId, serviceIds, year, month }),
    });
}

/** Artist: salon resolved from JWT + artist row (no salonId in URL). */
async function AddArtistService(service, token) {
    return salonicFetchJsonLenient(salonicApiUrl("/api/Artist/services"), {
        method: "POST",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify(service),
    });
}

/** Artist: frissíti a saját szolgáltatását (név, leírás, ár, perc). */
async function updateArtistService(serviceId, service, token) {
    return salonicFetchJsonLenient(salonicApiUrl(`/api/Artist/services/${encodeURIComponent(serviceId)}`), {
        method: "PUT",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify({
            name: service.name,
            description: service.description ?? "",
            price: service.price,
            durationInMinute: service.durationInMinute,
        }),
    });
}

/** Bulk import: servicename;duration;description;price per line (semicolon-separated). */
async function importArtistServicesFromCsv(csvText, token) {
    return salonicFetchJson(salonicApiUrl("/api/Artist/services/import"), {
        method: "POST",
        headers: { Authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify({ csvText }),
    });
}

/** Artist: felvesz egy tulaj által létrehozott szalon-katalógus szolgáltatást. */
async function linkArtistServiceFromSalonCatalog(token, serviceId) {
    return salonicFetchJsonLenient(salonicApiUrl("/api/Artist/services/link"), {
        method: "POST",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify({ serviceId }),
    });
}

/** Artist: eltávolítja az importált (tulaj katalógus) szolgáltatást a saját listájáról. */
async function unlinkArtistOffering(token, offeringId) {
    await salonicFetchVoid(salonicApiUrl(`/api/Artist/services/offerings/${encodeURIComponent(offeringId)}`), {
        method: "DELETE",
        headers: { authorization: `Bearer ${token}` },
    });
}

/** Tulaj: új katalógus-szolgáltatás a szalonban. */
async function createOwnerSalonService(salonId, service, token) {
    return salonicFetchJsonLenient(salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}/services`), {
        method: "POST",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify(service),
    });
}

/** Tulaj: tömeges import a szalon katalógusába (ugyanaz a CSV formátum, mint a szakember importnál). */
async function importOwnerSalonServicesFromCsv(salonId, csvText, token) {
    return salonicFetchJson(salonicApiUrl(`/api/salon/${encodeURIComponent(salonId)}/services/import`), {
        method: "POST",
        headers: { Authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify({ csvText }),
    });
}

async function setWorkingHoursByDay(token, date) {
    return salonicFetchJson(salonicApiUrl("/api/workinghours"), {
        method: "POST",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify(date),
    });
}

async function setWorkingHoursByWeek(token, date) {
    return salonicFetchJson(salonicApiUrl("/api/workinghours/schedule"), {
        method: "POST",
        headers: { authorization: `Bearer ${token}`, "content-type": "application/json" },
        body: JSON.stringify(date),
    });
}

/** Nyilvános szalonlista a portálhoz (bejelentkezés nélkül). */
async function GetPublicSalonDirectory() {
    return salonicFetchJson(salonicApiUrl("/api/salon/directory"));
}

async function GetSalonBySalonId(token, salonId) {
    const headers = {};
    if (token) headers.authorization = `Bearer ${token}`;
    return salonicFetchJson(salonicApiUrl(`/api/salon/${salonId}/public-profile`), { headers });
}
