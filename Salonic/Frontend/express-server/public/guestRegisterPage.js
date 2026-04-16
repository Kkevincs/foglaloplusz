document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

    document.getElementById("guest-register-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const form = e.target;
        const name = form.querySelector("#Name").value;
        const email = form.querySelector("#Email").value;
        const password = form.querySelector("#Password").value;
        const user = {
            name: name,
            email: email,
            pwd: password,
        };
        await Register(user);
        const { ok, token } = await LoginWithStatus({
            email: email,
            role: "guest",
            pwd: password,
        });
        if (ok && token) {
            localStorage.setItem("token", token);
            const params = new URLSearchParams(window.location.search);
            const salonId = params.get("salonId") || params.get("salonid");
            try {
                const completed = await tryCompletePendingBookingAfterGuestLogin(token);
                if (completed) return;
            } catch (err) {
                console.error(err);
                alert(err?.message || "Could not save your appointment. You can book again from the salon.");
            }
            const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
            window.location.assign(`/Salon/GuestInfo${salonQuery}`);
        } else {
            alert("Account may have been created, but automatic login failed. Try signing in.");
        }
    });
});
