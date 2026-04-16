document.addEventListener("DOMContentLoaded", () => {
    if (!enforceOwnerRoleOrRedirect()) return;
    loadSalons();
    document.getElementById("owner-salons-list")?.addEventListener("click", async (e) => {
        const btn = e.target.closest('[data-action="remove-salon"]');
        if (!btn) return;
        const salonId = btn.getAttribute("data-salon-id");
        if (!salonId) return;
        if (
            !confirm(
                "Remove this salon permanently? All artists, services, appointments, and working hours for this salon will be deleted.",
            )
        ) {
            return;
        }
        const token = localStorage.getItem("token");
        try {
            btn.disabled = true;
            await DeleteSalon(token, salonId);
            await loadSalons();
        } catch (err) {
            console.error(err);
            alert(err?.message || "Could not remove salon.");
            btn.disabled = false;
        }
    });
});
