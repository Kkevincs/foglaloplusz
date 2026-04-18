document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();
    if (/^\/Salon\/AppointmentCreated\/?$/i.test(window.location.pathname)) {
        await applyAppointmentCreatedPage();
    }
});
