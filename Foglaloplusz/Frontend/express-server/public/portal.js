(function () {
    const API_ORIGIN =
        typeof window !== "undefined" && window.SALONIC_API_BASE !== undefined && window.SALONIC_API_BASE !== null
            ? String(window.SALONIC_API_BASE)
            : "";

    /** Relatív /uploads/... → Express proxy; teljes URL változatlan. */
    function resolveSalonLogoSrc(logo) {
        let path = String(logo ?? "").trim();
        if (!path) return "";
        if (/^https?:\/\//i.test(path)) return path;
        path = path.startsWith("/") ? path : "/" + path;
        const base = String(API_ORIGIN || "").replace(/\/$/, "");
        if (base) return base + path;
        if (typeof location !== "undefined" && location.origin) return location.origin + path;
        return path;
    }

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

    /** 1–2 karakteres monogram a szalon nevéből (kártya, ha nincs feltöltött logó). */
    function initialsFromSalonName(name) {
        const t = String(name || "").trim();
        if (!t) return "•";
        const parts = t.split(/\s+/).filter(Boolean);
        const firstAlnum = (s) => {
            const m = s.match(/[\p{L}\p{N}]/u);
            return m ? m[0] : "";
        };
        if (parts.length >= 2) {
            const a = firstAlnum(parts[0]);
            const b = firstAlnum(parts[parts.length - 1]);
            if (a && b) return (a + b).toUpperCase();
        }
        const letters = [];
        for (const ch of parts[0] || t) {
            if (/[\p{L}\p{N}]/u.test(ch)) {
                letters.push(ch);
                if (letters.length >= 2) break;
            }
        }
        return letters.length ? letters.join("").toUpperCase() : "•";
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
                const logo = s.logo ?? s.Logo ?? s.logoUrl ?? s.LogoUrl ?? "";
                const theme = s.theme ?? s.Theme ?? "";

                const card = document.createElement("article");
                const th = themeClass(theme);
                card.className =
                    "portal-salon-card async-card-enter" + (th ? " " + th : "");
                card.style.setProperty("--async-card-i", String(i));
                card.setAttribute("data-salon-id", String(id));

                const media = document.createElement("div");
                media.className = "portal-salon-card-media";
                const monogramHtml = function () {
                    const lab = initialsFromSalonName(name);
                    return (
                        '<span class="portal-salon-card-placeholder" aria-hidden="true">' +
                        lab.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;") +
                        "</span>"
                    );
                };
                if (logo && String(logo).trim()) {
                    const src = resolveSalonLogoSrc(logo);
                    const img = document.createElement("img");
                    img.src = src;
                    img.alt = name ? String(name) : "Szalon logó";
                    img.loading = "lazy";
                    img.onerror = function () {
                        media.innerHTML = monogramHtml();
                    };
                    media.appendChild(img);
                } else {
                    media.innerHTML = monogramHtml();
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
