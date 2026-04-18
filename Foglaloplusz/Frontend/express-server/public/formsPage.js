document.addEventListener("DOMContentLoaded", () => {
    if (!enforceOwnerRoleOrRedirect()) return;

    document.getElementById("salon-setup-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const name = form.querySelector("#salon-name").value;
        const logoInput = form.querySelector("#salon-logo");
        const logoFile = logoInput?.files?.[0] ?? null;
        const description = form.querySelector("#salon-description").value;
        const theme = form.querySelector("#salon-theme").value;
        const locationInput = form.querySelector("#salon-location");
        const location = locationInput ? String(locationInput.value || "").trim() : "";
        const email = form.querySelector("#support-email").value;

        const newSalon = {
            name,
            email,
            theme,
            description,
            location: location || undefined,
            logoFile,
        };
        let createdSalon;
        try {
            createdSalon = await AddSalon(localStorage.getItem("token"), newSalon);
        } catch (err) {
            if (!err?.salonicApiShown) console.error(err);
            return;
        }

        const salonId = createdSalon?.id ?? createdSalon?.Id;
        if (salonId) {
            const cache = getSalonThemeCache();
            cache[String(salonId)] = theme;
            setSalonThemeCache(cache);
        }
        salonicShowModal("A szalon mentése sikeres.", { title: "Mentve", variant: "success" });
        const modalEl = document.getElementById("salonic-unified-modal");
        const goOwner = () => {
            window.location.assign("/ownerhomepage");
        };
        if (modalEl) {
            if (typeof window.bootstrap !== "undefined" && window.bootstrap.Modal) {
                modalEl.addEventListener("hidden.bs.modal", goOwner, { once: true });
            } else {
                modalEl.querySelector(".salonic-u-ok")?.addEventListener("click", goOwner, { once: true });
                modalEl.querySelector(".salonic-u-close")?.addEventListener("click", goOwner, { once: true });
            }
        } else {
            goOwner();
        }
    });
});
