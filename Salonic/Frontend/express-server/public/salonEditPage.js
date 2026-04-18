document.addEventListener("DOMContentLoaded", async () => {
    if (!enforceOwnerRoleOrRedirect()) return;

    const params = new URLSearchParams(window.location.search);
    const salonIdRaw = params.get("salonId") || params.get("salonid");
    if (!salonIdRaw || !String(salonIdRaw).trim()) {
        window.location.assign("/ownerhomepage");
        return;
    }
    const salonIdNum = parseInt(String(salonIdRaw).trim(), 10);
    if (!Number.isFinite(salonIdNum) || salonIdNum < 1) {
        window.location.assign("/ownerhomepage");
        return;
    }

    const token = localStorage.getItem("token");
    const alertEl = document.getElementById("salon-edit-alert");
    const submitBtn = document.getElementById("salon-edit-submit");
    const form = document.getElementById("salon-edit-form");

    function showError(msg) {
        if (!alertEl) return;
        alertEl.textContent = msg || "Ismeretlen hiba.";
        alertEl.classList.remove("d-none");
    }
    function hideError() {
        alertEl?.classList.add("d-none");
    }

    const sid = (s) => Number(s?.id ?? s?.Id);
    let ownedSalon = null;
    try {
        const list = await GetMySalons(token);
        const arr = Array.isArray(list) ? list : [];
        ownedSalon = arr.find((s) => sid(s) === salonIdNum);
    } catch (e) {
        if (!e?.salonicApiShown) {
            showError(e?.message || "Nem sikerült betölteni a szalonokat.");
        }
        if (submitBtn) submitBtn.disabled = true;
        return;
    }

    if (!ownedSalon) {
        window.location.replace("/404");
        return;
    }

    hideError();

    const name = ownedSalon.name ?? ownedSalon.Name ?? "";
    const email = ownedSalon.email ?? ownedSalon.Email ?? "";
    const themeRaw = ownedSalon.theme ?? ownedSalon.Theme ?? "theme-pink";
    const description = ownedSalon.description ?? ownedSalon.Description ?? "";
    const location = ownedSalon.location ?? ownedSalon.Location ?? "";

    const nameInput = document.getElementById("edit-salon-name");
    const emailInput = document.getElementById("edit-support-email");
    const themeSelect = document.getElementById("edit-salon-theme");
    const descInput = document.getElementById("edit-salon-description");
    const locInput = document.getElementById("edit-salon-location");

    if (nameInput) nameInput.value = name;
    if (emailInput) emailInput.value = email;
    if (descInput) descInput.value = description;
    if (locInput) locInput.value = location || "";

    const normTheme = typeof normalizeSalonThemeClass === "function" ? normalizeSalonThemeClass(themeRaw) : String(themeRaw || "").trim();
    if (themeSelect && normTheme) {
        const opt = Array.from(themeSelect.options).find((o) => o.value === normTheme);
        if (opt) themeSelect.value = normTheme;
        else themeSelect.value = "theme-pink";
    }

    const logoWrap = document.getElementById("edit-current-logo-wrap");
    const logoImg = document.getElementById("edit-current-logo-img");
    const logoUrl = typeof resolvePublicSalonLogoUrl === "function" ? resolvePublicSalonLogoUrl(ownedSalon) : "";
    if (logoUrl && logoImg && logoWrap) {
        logoImg.src = logoUrl;
        logoImg.alt = name || "Szalon logó";
        logoWrap.classList.remove("d-none");
    }

    if (typeof applyTheme === "function" && normTheme) {
        applyTheme(normTheme);
    }

    form?.addEventListener("submit", async (e) => {
        e.preventDefault();
        hideError();
        const logoInput = document.getElementById("edit-salon-logo");
        const logoFile = logoInput?.files?.[0] ?? null;
        const locVal = locInput ? String(locInput.value || "").trim() : "";

        const payload = {
            name: nameInput?.value ?? "",
            email: emailInput?.value ?? "",
            theme: themeSelect?.value ?? "",
            description: descInput?.value ?? "",
            location: locVal || undefined,
            logoFile,
        };

        if (submitBtn) submitBtn.disabled = true;
        try {
            await UpdateSalon(token, salonIdNum, payload);
            const cache = getSalonThemeCache();
            cache[String(salonIdNum)] = payload.theme;
            setSalonThemeCache(cache);
            if (typeof applyTheme === "function") applyTheme(payload.theme);
            window.location.assign(`/ownerhomepage`);
        } catch (err) {
            if (!err?.salonicApiShown) {
                showError(err?.message || "A mentés sikertelen.");
            }
        } finally {
            if (submitBtn) submitBtn.disabled = false;
        }
    });
});
