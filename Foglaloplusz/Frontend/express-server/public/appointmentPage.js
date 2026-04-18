document.addEventListener("DOMContentLoaded", async () => {
    if (typeof initSalonGuestPublicShell === "function") {
        await initSalonGuestPublicShell();
    }
});
