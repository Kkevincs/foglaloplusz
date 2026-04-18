document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

    document.getElementById("guest-login-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const email = form.querySelector("#Email").value;
        const password = form.querySelector("#Password").value;
        const user = {
            email: email,
            role: "guest",
            pwd: password,
        };
        const { ok, token } = await LoginWithStatus(user);
        if (ok && token) {
            localStorage.setItem("token", token);
            const params = new URLSearchParams(window.location.search);
            const salonId = params.get("salonId") || params.get("salonid");
            try {
                const completed = await tryCompletePendingBookingAfterGuestLogin(token);
                if (completed) return;
            } catch (err) {
                console.error(err);
                if (!err?.salonicApiShown) {
                    salonicShowModal(err?.message || "Could not save your appointment. You can book again from the salon.", {
                        title: "Foglalás",
                        variant: "danger",
                    });
                }
            }
            const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
            window.location.assign(`/Salon/GuestInfo${salonQuery}`);
        }
    });
});
