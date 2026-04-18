document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    if (!document.getElementById("services-list")) return;
    const params = new URLSearchParams(window.location.search);
    const aid = params.get("artistId") || params.get("artistid");
    if (aid) {
        const n = Number(aid);
        await LoadServices(Number.isFinite(n) && n > 0 ? n : undefined);
    } else {
        await LoadServices();
    }
});
