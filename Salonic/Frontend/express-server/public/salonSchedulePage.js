document.addEventListener("DOMContentLoaded", () => {
    if (!enforceOwnerRoleOrRedirect()) return;
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId");
    (async () => {
        const ok = await assertOwnerHasSalonOrRedirect(salonId);
        if (!ok) return;
        loadOwnerArtistSchedulePage();
    })();
});
