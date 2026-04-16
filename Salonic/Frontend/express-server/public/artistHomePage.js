document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    if (document.getElementById("salon-info")) await applySalonPersonalHomepage();
});
