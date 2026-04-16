document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

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
        await AddArtistService(newService, token);
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
            alert("Choose a CSV file first.");
            return;
        }
        const token = localStorage.getItem("token");
        if (!token) {
            alert("Please log in as an artist.");
            return;
        }
        let csvText;
        try {
            csvText = await file.text();
        } catch (err) {
            console.error(err);
            alert("Could not read the file.");
            return;
        }
        if (!resultEl) return;
        resultEl.hidden = false;
        resultEl.innerHTML = '<p class="text-muted mb-0">Importing…</p>';
        if (btn) btn.disabled = true;
        try {
            const data = await importArtistServicesFromCsv(csvText, token);
            const created = data.created ?? data.Created ?? [];
            const errors = data.errors ?? data.Errors ?? [];
            const okCount = created.length;
            const errCount = errors.length;
            let html = `<p class="fw-semibold mb-2">Done: ${okCount} created${errCount ? `, ${errCount} row(s) failed` : ""}.</p>`;
            if (okCount > 0) {
                html += '<ul class="list-unstyled mb-2">';
                created.slice(0, 20).forEach((s) => {
                    const n = escapeHtml(s.name ?? s.Name ?? "");
                    html += `<li class="text-success">${n}</li>`;
                });
                if (created.length > 20) html += '<li class="text-muted">…</li>';
                html += "</ul>";
            }
            if (errCount > 0) {
                html += '<p class="text-danger fw-semibold mb-1">Errors</p><ul class="mb-0">';
                errors.forEach((e) => {
                    const line = e.line ?? e.Line;
                    const msg = escapeHtml(e.message ?? e.Message ?? "");
                    html += `<li>Line ${line}: ${msg}</li>`;
                });
                html += "</ul>";
            }
            resultEl.innerHTML = html;
            if (okCount > 0 && fileInput) fileInput.value = "";
        } catch (err) {
            console.error(err);
            resultEl.innerHTML = `<p class="text-danger mb-0" role="alert">${escapeHtml(err?.message || "Import failed.")}</p>`;
        } finally {
            if (btn) btn.disabled = false;
        }
    });
});
