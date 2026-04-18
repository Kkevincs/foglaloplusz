/**
 * Globális oldal-inicializálás és űrlapkezelők.
 * Közös logika (JWT, szalon, naptár, API): common.js + api.js
 */
document.addEventListener("DOMContentLoaded", async () => {
    if (typeof enforceOwnerRoleOrRedirect === "function" && !enforceOwnerRoleOrRedirect()) return;

    if (window.location.pathname === "/ownerhomepage") {
        loadSalons();
        document.getElementById("owner-salons-list")?.addEventListener("click", async (e) => {
            const btn = e.target.closest('[data-action="remove-salon"]');
            if (!btn) return;
            const salonId = btn.getAttribute("data-salon-id");
            if (!salonId) return;
            const okDelete = await salonicConfirmModal({
                title: "Szalon törlése",
                message:
                    "Biztosan véglegesen törlöd ezt a szalont? Minden kapcsolódó szakember, szolgáltatás, időpont és nyitvatartás törlődik.",
                confirmText: "Szalon törlése",
                cancelText: "Mégse",
                danger: true,
            });
            if (!okDelete) return;
            const token = localStorage.getItem("token");
            try {
                btn.disabled = true;
                await DeleteSalon(token, salonId);
                await loadSalons();
            } catch (err) {
                console.error(err);
                if (!err?.salonicApiShown) {
                    salonicShowModal(err?.message || "Nem sikerült törölni a szalont.", { title: "Hiba", variant: "danger" });
                }
                btn.disabled = false;
            }
        });
    }

    if (window.location.pathname === "/ownerartistregister") {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get("salonId");
        (async () => {
            const ok = await assertOwnerHasSalonOrRedirect(salonId);
            if (!ok) return;
            formRegisterArtist(salonId);
        })();
    }

    if (window.location.pathname === "/Salon/Artist/Hours") {
        loadWorkingHours();
    }

    if (window.location.pathname === "/salondashboard") {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get("salonId");
        (async () => {
            const ok = await assertOwnerHasSalonOrRedirect(salonId);
            if (!ok) return;
            loadArtistsBySalonId(salonId);
            loadServicesBySalon(salonId);
            loadAppointments(salonId);
            initOwnerSalonDashboardServices(salonId);
        })();
    }

    if (window.location.pathname === "/salondashboard/schedule") {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get("salonId");
        (async () => {
            const ok = await assertOwnerHasSalonOrRedirect(salonId);
            if (!ok) return;
            loadOwnerArtistSchedulePage();
        })();
    }

    if (document.getElementById("services-list")) LoadServices();
    if (document.getElementById("user-profile")) LoadUserData();
    if (document.getElementById("artists-list")) LoadArtists();
    if (document.getElementById("artist-profile")) LoadArtistData();
    if (document.getElementById("owner-profile")) LoadOwnerData();

    const guestApptMount = document.getElementById("user-appointments");
    if (guestApptMount) {
        guestApptMount.addEventListener("click", async (e) => {
            const btn = e.target.closest("[data-cancel-appointment]");
            if (!btn) return;
            e.preventDefault();
            const id = btn.getAttribute("data-cancel-appointment");
            if (!id) return;
            if (!confirm("Biztosan lemondod ezt az időpontot?")) return;
            const token = localStorage.getItem("token");
            if (!token) return;
            try {
                btn.disabled = true;
                await cancelGuestAppointment(token, id);
                await loadGuestUpcomingAppointments(token, guestApptMount);
            } catch (err) {
                console.error(err);
                if (!err?.salonicApiShown) {
                    salonicShowModal(err?.message || "Nem sikerült lemondani az időpontot.", { title: "Hiba", variant: "danger" });
                }
                btn.disabled = false;
            }
        });
    }
});

document.querySelectorAll(".nav-links a").forEach((a) => {
    a.addEventListener("click", (e) => {
        if (a.getAttribute("href") === "#") e.preventDefault();
    });
});

document.getElementById("artist-login-form")?.addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const email = form.querySelector("#ArtistEmail").value;
    const password = form.querySelector("#ArtistPassword").value;
    const role = "artist";

    const params = new URLSearchParams(window.location.search);
    const salonIdRaw = params.get("salonId") || params.get("salonid");
    const salonIdNum = salonIdRaw ? Number.parseInt(String(salonIdRaw).trim(), 10) : NaN;

    const artist = {
        email: email,
        role: role,
        pwd: password,
    };
    if (Number.isFinite(salonIdNum) && salonIdNum > 0) {
        artist.salonId = salonIdNum;
    }
    const token = await Login(artist);
    if (!token) return;
    localStorage.setItem("token", token);
    const salonQuery = salonIdRaw ? `?salonId=${encodeURIComponent(salonIdRaw)}` : "";
    window.location.assign(`/Salon/Artist/User${salonQuery}`);
});

document.getElementById("salon-setup-form")?.addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector("#salon-name").value;
    const logoInput = form.querySelector("#salon-logo");
    const logoFile = logoInput?.files?.[0] ?? null;
    const description = form.querySelector("#salon-description").value;
    const theme = form.querySelector("#salon-theme").value;
    const locationInput = form.querySelector("#salon-location");
    const location = locationInput ? String(locationInput.value || "").trim() : "";
    const email = form.querySelector("#support-email").value;

    const newSalon = {
        name,
        email,
        theme,
        description,
        location: location || undefined,
        logoFile,
    };
    try {
        const createdSalon = await AddSalon(localStorage.getItem("token"), newSalon);
        const salonId = createdSalon?.id ?? createdSalon?.Id;
        if (salonId && typeof getSalonThemeCache === "function" && typeof setSalonThemeCache === "function") {
            const cache = getSalonThemeCache();
            cache[String(salonId)] = theme;
            setSalonThemeCache(cache);
        }
        salonicShowModal("A szalon mentése sikeres.", { title: "Mentve", variant: "success" });
        const modalEl = document.getElementById("salonic-unified-modal");
        const goOwner = () => {
            window.location.assign("/ownerhomepage");
        };
        if (modalEl) {
            if (typeof window.bootstrap !== "undefined" && window.bootstrap.Modal) {
                modalEl.addEventListener("hidden.bs.modal", goOwner, { once: true });
            } else {
                modalEl.querySelector(".salonic-u-ok")?.addEventListener("click", goOwner, { once: true });
                modalEl.querySelector(".salonic-u-close")?.addEventListener("click", goOwner, { once: true });
            }
        } else {
            goOwner();
        }
    } catch (err) {
        if (!err?.salonicApiShown) console.error(err);
    }
});

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
        try {
            if (typeof tryCompletePendingBookingAfterGuestLogin === "function") {
                const completed = await tryCompletePendingBookingAfterGuestLogin(token);
                if (completed) return;
            }
        } catch (err) {
            console.error(err);
            if (!err?.salonicApiShown) {
                salonicShowModal(
                    err?.message || "Nem sikerült lefoglalni az időpontot. Próbáld meg újra a foglalás lépéseiből.",
                    { title: "Foglalás", variant: "danger" },
                );
            }
        }
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get("salonId") || params.get("salonid");
        const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
        window.location.assign(`/Salon/GuestInfo${salonQuery}`);
    }
});

document.getElementById("owner-login-form")?.addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const email = form.querySelector("#OwnerLoginEmail").value;
    const password = form.querySelector("#OwnerLoginPassword").value;
    const user = {
        email: email,
        role: "owner",
        pwd: password,
    };
    const { ok, token } = await LoginWithStatus(user);
    if (ok && token) {
        localStorage.setItem("token", token);
        window.location.href = "/ownerhomepage";
    }
});

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
    try {
        await Register(user);
    } catch (err) {
        if (err && err.salonicApiShown) return;
        throw err;
    }
    const { ok, token } = await LoginWithStatus({
        email: email,
        role: "guest",
        pwd: password,
    });
    if (ok && token) {
        localStorage.setItem("token", token);
        try {
            if (typeof tryCompletePendingBookingAfterGuestLogin === "function") {
                const completed = await tryCompletePendingBookingAfterGuestLogin(token);
                if (completed) return;
            }
        } catch (err) {
            console.error(err);
            if (!err?.salonicApiShown) {
                salonicShowModal(
                    err?.message || "Nem sikerült lefoglalni az időpontot. Próbáld meg újra a foglalás lépéseiből.",
                    { title: "Foglalás", variant: "danger" },
                );
            }
        }
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get("salonId") || params.get("salonid");
        const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
        window.location.assign(`/Salon/GuestInfo${salonQuery}`);
    }
});

document.getElementById("owner-register-form")?.addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector("#OwnerRegName").value;
    const email = form.querySelector("#OwnerRegEmail").value;
    const password = form.querySelector("#OwnerRegPassword").value;
    const user = {
        name: name,
        email: email,
        pwd: password,
    };
    const { ok } = await RegisterOwner(user);
    if (ok) {
        window.location.href = "/maker";
    }
});
