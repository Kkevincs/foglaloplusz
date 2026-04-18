document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    await loadServicesForArtists();
    wireArtistServiceEditForms();
});

function wireArtistServiceEditForms() {
    const list = document.getElementById("artist-services-list");
    if (!list || list.dataset.editWired === "1") return;
    list.dataset.editWired = "1";

    list.addEventListener("submit", (e) => {
        const form = e.target.closest("form.artist-service-edit-form");
        if (form) e.preventDefault();
    });

    list.addEventListener("keydown", (e) => {
        const form = e.target.closest("form.artist-service-edit-form");
        if (!form) return;
        if (e.key !== "Enter") return;
        if (e.target.tagName === "TEXTAREA") return;
        e.preventDefault();
    });

    list.addEventListener("click", async (e) => {
        const btn = e.target.closest("button.artist-service-save-btn");
        if (!btn) return;
        const form = btn.closest("form.artist-service-edit-form");
        if (!form) return;
        await saveArtistServiceForm(form, btn);
    });
}

async function saveArtistServiceForm(form, btn) {
    const sid = form.getAttribute("data-service-id");
    if (!sid) return;
    const token = localStorage.getItem("token");
    if (!token) {
        salonicShowModal("Jelentkezz be a mentéshez.", { title: "Bejelentkezés szükséges", variant: "warning" });
        return;
    }
    const msgEl = form.querySelector(".artist-service-edit-msg");
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
        await updateArtistService(Number(sid), payload, token);
        if (msgEl) {
            msgEl.textContent = "Mentve.";
            msgEl.classList.remove("text-danger");
            msgEl.classList.add("text-success");
        }
        await loadServicesForArtists();
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
