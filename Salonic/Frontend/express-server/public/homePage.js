document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    if (document.getElementById("salon-info")) {
        await applySalonPersonalHomepage();
        await loadSalonLandingLists();
    }
});

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
            arr.forEach((s) => {
                const name = escapeHtml(s.name ?? s.Name ?? "");
                const price = Number(s.price ?? s.Price ?? 0);
                const dur = Number(s.durationInMinute ?? s.DurationInMinute ?? 0);
                const priceStr = formatPriceFt(price);
                const durStr = formatDurationMinutesHu(dur);
                servicesList.innerHTML += `<li class="salon-landing-li"><span class="salon-landing-li__name">${name}</span><span class="salon-landing-li__meta">${durStr} · ${priceStr}</span></li>`;
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
            arr.forEach((a) => {
                const name = escapeHtml(a.name ?? a.Name ?? "");
                const desc = escapeHtml(a.description ?? a.Description ?? "");
                const id = String(a.id ?? a.Id);
                const bookHref = `/Salon/Services?salonId=${encodeURIComponent(salonId)}&artistId=${encodeURIComponent(id)}`;
                const descHtml = desc ? `<span class="salon-landing-li__desc">${desc}</span>` : "";
                artistsList.innerHTML += `<li class="salon-landing-li salon-landing-li--artist"><div class="salon-landing-li__artist-text"><span class="salon-landing-li__name">${name}</span>${descHtml}</div><a class="salon-landing-li__book" href="${bookHref}">Szolgáltatások</a></li>`;
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
