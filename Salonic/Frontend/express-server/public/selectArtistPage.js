document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    if (document.getElementById("artists-list")) LoadArtists();
});
