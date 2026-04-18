document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

    await loadOwnerCatalogPicker();

    document.getElementById("addservice-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const serviceName = form.querySelector("#name").value;
        const durationInMinutes = Number(form.querySelector("#durationInMinute").value);
        const price = Number(form.querySelector("#price").value);
        const description = form.querySelector("#description").value;

        const newService = {
            name: serviceName,
            description: description,
            price: price,
            durationInMinute: durationInMinutes,
        };

        const token = localStorage.getItem("token");
        if (!token) {
            salonicShowModal("Jelentkezz be szakemberként.", { title: "Bejelentkezés szükséges", variant: "warning" });
            return;
        }
        try {
            await AddArtistService(newService, token);
            salonicShowModal("Szolgáltatás elmentve.", { title: "Mentve", variant: "success" });
            form.reset();
        } catch (err) {
            console.error(err);
            if (!err?.salonicApiShown) {
                salonicShowModal(err?.message || "Nem sikerült menteni a szolgáltatást.", { title: "Hiba", variant: "danger" });
            }
        }
    });

    document.getElementById("artist-csv-template-btn")?.addEventListener("click", () => {
        const sample =
            "servicename;duration;description;price\nClassic cut;45;Wash and cut;3500\nColor;90;;12000\n";
        const blob = new Blob([sample], { type: "text/csv;charset=utf-8" });
        const a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = "services-template.csv";
        a.click();
        URL.revokeObjectURL(a.href);
    });

    document.getElementById("artist-csv-import-btn")?.addEventListener("click", async () => {
        const fileInput = document.getElementById("artist-csv-file");
        const resultEl = document.getElementById("artist-csv-result");
        const btn = document.getElementById("artist-csv-import-btn");
        const file = fileInput?.files?.[0];
        if (!file) {
            salonicShowModal("Válassz előbb egy CSV fájlt.", { title: "Import", variant: "warning" });
            return;
        }
        const token = localStorage.getItem("token");
        if (!token) {
            salonicShowModal("Jelentkezz be szakemberként.", { title: "Bejelentkezés szükséges", variant: "warning" });
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
        resultEl.classList.remove("alert-danger", "alert-success");
        resultEl.classList.add("alert-light", "border");
        resultEl.hidden = false;
        resultEl.innerHTML = '<p class="text-muted mb-0">Importálás…</p>';
        if (btn) btn.disabled = true;
        try {
            const data = await importArtistServicesFromCsv(csvText, token);
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
            if (okCount > 0 && fileInput) fileInput.value = "";
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
});

async function loadOwnerCatalogPicker() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const section = document.getElementById("owner-catalog-section");
    const listEl = document.getElementById("owner-catalog-catalog-list");
    const emptyEl = document.getElementById("owner-catalog-empty");
    if (!section || !listEl || !salonId) return;

    const token = localStorage.getItem("token");
    if (!token) return;

    try {
        const [myList, salonList] = await Promise.all([
            getServicesByArtist(token),
            GetSalonServicesCatalog(salonId, token),
        ]);
        const myIds = new Set((myList || []).map((x) => x.id ?? x.Id));
        const catalog = (salonList || []).filter((s) => {
            const oid = s.ownerId ?? s.OwnerId;
            return oid != null && oid !== 0;
        });
        const pickable = catalog.filter((s) => !myIds.has(s.id ?? s.Id));

        section.hidden = false;
        listEl.innerHTML = "";
        if (pickable.length === 0) {
            emptyEl?.classList.remove("d-none");
            return;
        }
        emptyEl?.classList.add("d-none");

        pickable.forEach((s) => {
            const id = s.id ?? s.Id;
            const name = s.name ?? s.Name ?? "";
            const price = s.price ?? s.Price ?? "";
            const dur = s.durationInMinute ?? s.DurationInMinute ?? "";
            const row = document.createElement("div");
            row.className =
                "owner-catalog-picker-row d-flex flex-wrap align-items-center justify-content-between gap-2 gap-md-3";
            row.innerHTML = `<span><strong>${escapeHtml(String(name))}</strong> — ${escapeHtml(String(dur))} perc, ${escapeHtml(String(price))} Ft</span>`;
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "btn btn-sm adservice-outline-btn";
            btn.textContent = "Felveszem";
            btn.addEventListener("click", async () => {
                btn.disabled = true;
                try {
                    await linkArtistServiceFromSalonCatalog(token, id);
                    salonicShowModal("Szolgáltatás felvéve a listádra.", { title: "Kész", variant: "success" });
                    await loadOwnerCatalogPicker();
                } catch (err) {
                    console.error(err);
                    if (!err?.salonicApiShown) {
                        salonicShowModal(err?.message || "Nem sikerült felvenni.", { title: "Hiba", variant: "danger" });
                    }
                    btn.disabled = false;
                }
            });
            row.appendChild(btn);
            listEl.appendChild(row);
        });
    } catch (e) {
        console.warn("Owner catalog picker:", e);
    }
}
