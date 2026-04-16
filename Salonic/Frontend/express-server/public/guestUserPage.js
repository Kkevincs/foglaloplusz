document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    if (document.getElementById("user-profile")) LoadUserData();

    document.getElementById("user-appointments")?.addEventListener("click", async (e) => {
        const btn = e.target.closest("[data-cancel-appointment]");
        if (!btn) return;
        e.preventDefault();
        const id = btn.getAttribute("data-cancel-appointment");
        if (!id) return;
        if (!confirm("Cancel this appointment? This cannot be undone.")) return;
        const token = localStorage.getItem("token");
        const mount = document.getElementById("user-appointments");
        if (!token || !mount) return;
        try {
            btn.disabled = true;
            await cancelGuestAppointment(token, id);
            await loadGuestUpcomingAppointments(token, mount);
        } catch (err) {
            console.error(err);
            alert(err?.message || "Could not cancel the appointment.");
            btn.disabled = false;
        }
    });
});
