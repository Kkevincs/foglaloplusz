(function () {
    const API_ORIGIN =
        typeof window !== "undefined" && window.SALONIC_API_BASE !== undefined && window.SALONIC_API_BASE !== null
            ? String(window.SALONIC_API_BASE)
            : "";

    function truncate(str, len) {
        const t = (str || "").trim().replace(/\s+/g, " ");
        if (t.length <= len) return t;
        return t.slice(0, len - 1) + "…";
    }

    function themeClass(theme) {
        const t = (theme || "").trim();
        if (/^theme-[a-z0-9-]+$/i.test(t)) return t;
        return "";
    }

    document.addEventListener("DOMContentLoaded", async () => {
        const grid = document.getElementById("portal-salon-grid");
        const loading = document.getElementById("portal-salon-loading");
        const empty = document.getElementById("portal-salon-empty");
        const errEl = document.getElementById("portal-salon-error");
        if (!grid) return;
        if (typeof GetPublicSalonDirectory !== "function") {
            if (loading) loading.hidden = true;
            if (errEl) {
                errEl.hidden = false;
                errEl.textContent = "A szalonlista betöltése nem érhető el (hiányzó API). Frissítsd az oldalt.";
            }
            return;
        }

        try {
            const salons = await GetPublicSalonDirectory();
            if (loading) loading.hidden = true;

            if (!Array.isArray(salons) || salons.length === 0) {
                if (empty) empty.hidden = false;
                return;
            }

            if (empty) empty.hidden = true;
            grid.innerHTML = "";

            salons.forEach((s, i) => {
                const id = Number(s.id ?? s.Id);
                if (!Number.isFinite(id) || id < 1) return;

                const name = s.name ?? s.Name ?? "";
                const description = s.description ?? s.Description ?? "";
                const logo = s.logo ?? s.Logo ?? "";
                const theme = s.theme ?? s.Theme ?? "";

                const card = document.createElement("article");
                const th = themeClass(theme);
                card.className =
                    "portal-salon-card async-card-enter" + (th ? " " + th : "");
                card.style.setProperty("--async-card-i", String(i));
                card.setAttribute("data-salon-id", String(id));

                const media = document.createElement("div");
                media.className = "portal-salon-card-media";
                if (logo && String(logo).trim()) {
                    const src = String(logo).startsWith("http")
                        ? logo
                        : API_ORIGIN + (String(logo).startsWith("/") ? logo : "/" + logo);
                    const img = document.createElement("img");
                    img.src = src;
                    img.alt = "";
                    img.loading = "lazy";
                    img.onerror = function () {
                        media.innerHTML =
                            '<span class="portal-salon-card-placeholder" aria-hidden="true">FP</span>';
                    };
                    media.appendChild(img);
                } else {
                    media.innerHTML =
                        '<span class="portal-salon-card-placeholder" aria-hidden="true">FP</span>';
                }

                const body = document.createElement("div");
                body.className = "portal-salon-card-body";

                const h3 = document.createElement("h3");
                h3.className = "portal-salon-card-title";
                h3.textContent = name || "Szalon";

                const p = document.createElement("p");
                p.className = "portal-salon-card-desc";
                p.textContent = truncate(description, 220) || "Nincs rövid leírás.";

                const actions = document.createElement("div");
                actions.className = "portal-salon-card-actions";

                const q = encodeURIComponent(String(id));
                const aGuest = document.createElement("a");
                aGuest.className = "portal-salon-btn portal-salon-btn-primary";
                aGuest.href = "/Salon?salonId=" + q;
                aGuest.textContent = "Foglalás vendégként";

                const aArtist = document.createElement("a");
                aArtist.className = "portal-salon-btn portal-salon-btn-secondary";
                aArtist.href = "/Salon/Artist?salonId=" + q;
                aArtist.textContent = "Szakember bejelentkezés";

                actions.appendChild(aGuest);
                actions.appendChild(aArtist);

                body.appendChild(h3);
                body.appendChild(p);
                body.appendChild(actions);

                card.appendChild(media);
                card.appendChild(body);
                grid.appendChild(card);
            });
        } catch (e) {
            console.error(e);
            if (loading) loading.hidden = true;
            if (e && e.salonicApiShown) return;
            if (errEl) {
                errEl.hidden = false;
                errEl.textContent =
                    (e && e.message) ||
                    "Nem sikerült betölteni a szalonokat. Ellenőrizd, hogy az API fut-e.";
            }
        }
    });
})();
