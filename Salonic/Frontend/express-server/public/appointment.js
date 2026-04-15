let selectedDate = "";
/** ISO start time from the API slot (DateTime), not display text */
let selectedSlotStartIso = "";
let selectedArtistId = 0;
let selectedServiceIds = [];
let selectedSalonId = 0;

function appointmentFormatLocalYMD(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
}

document.addEventListener("DOMContentLoaded", async () => {
    await initSalonGuestPublicShell();

    const dateInput = document.getElementById("appointment-date");
    if (!dateInput) return;

    const todayStr = appointmentFormatLocalYMD(new Date());
    dateInput.min = todayStr;
    if (!dateInput.value || dateInput.value < todayStr) {
        dateInput.value = todayStr;
    }

    const params = new URLSearchParams(window.location.search);
    const serviceidsRaw = params.get("serviceids") ?? "";
    selectedServiceIds = serviceidsRaw
        ? serviceidsRaw.split(",").map((x) => Number(x.trim())).filter((n) => n > 0)
        : [];
    selectedArtistId = Number(params.get("artistid")) || 0;
    selectedSalonId = Number(params.get("salonId") || params.get("salonid")) || 0;

    async function loadSlotsForSelectedDate() {
        selectedDate = dateInput.value;
        if (!selectedDate || selectedServiceIds.length === 0 || selectedArtistId <= 0) return;

        const timeData = {
            artistId: selectedArtistId,
            serviceIds: selectedServiceIds,
            date: selectedDate
        };
        const stamps = await GetAvaliableSlots(timeData);
        const list = document.getElementById("time-slots");
        if (!list) return;

        list.innerHTML = "";
        stamps.forEach((slot) => {
            const raw = slot.startTime;
            const isoStr = typeof raw === "string" ? raw : String(raw);
            const label = isoStr.includes("T") ? isoStr.split("T")[1].slice(0, 5) : isoStr;
            const enc = encodeURIComponent(isoStr);
            list.innerHTML += `<li class="time-slot" data-start-iso="${enc}">${label}</li>`;
        });
        selectedSlotStartIso = "";
    }

    dateInput.addEventListener("change", () => void loadSlotsForSelectedDate());
    void loadSlotsForSelectedDate();

    const slotsPanel = document.querySelector(".appointment-page .time-slots");
    if (!slotsPanel) return;
    slotsPanel.addEventListener("click", (event) => {
        const slot = event.target.closest(".time-slot");
        if (!slot) return;
        slotsPanel.querySelectorAll(".time-slot").forEach((item) => item.classList.remove("time-slot-selected"));
        slot.classList.add("time-slot-selected");
        const rawEnc = slot.getAttribute("data-start-iso") || "";
        try {
            selectedSlotStartIso = rawEnc ? decodeURIComponent(rawEnc) : "";
        } catch (_) {
            selectedSlotStartIso = rawEnc;
        }
    });
});

function resolveSalonIdForBooking() {
    if (selectedSalonId > 0) return selectedSalonId;
    const p = new URLSearchParams(window.location.search);
    const id = Number(p.get("salonId") || p.get("salonid"));
    return Number.isFinite(id) && id > 0 ? id : 0;
}

function buildPendingBookingRegisterUrl() {
    const salon = resolveSalonIdForBooking();
    const p = new URLSearchParams();
    if (salon > 0) p.set("salonId", String(salon));
    p.set("serviceids", selectedServiceIds.join(","));
    p.set("artistid", String(selectedArtistId));
    p.set("appointmentDate", selectedDate);
    p.set("startTime", selectedSlotStartIso);
    return `/Salon/Register?${p.toString()}`;
}

window.SaveDate = async function SaveDate() {
    if (!selectedDate || !selectedSlotStartIso) {
        alert("Choose a date and a time slot.");
        return;
    }

    if (selectedDate < appointmentFormatLocalYMD(new Date())) {
        alert("Choose today or a future date.");
        return;
    }

    if (selectedServiceIds.length === 0 || selectedArtistId <= 0) {
        alert("Missing services or artist. Go back and select again.");
        return;
    }

    const registerWithBooking = buildPendingBookingRegisterUrl();

    const token = (localStorage.getItem("token") || "").trim();

    if (!token) {
        alert(
            "Create an account to complete your booking. After you register, you'll be signed in and your appointment will be saved.",
        );
        window.location.assign(registerWithBooking);
        return;
    }

    const role =
        typeof window.getRoleFromToken === "function" ? window.getRoleFromToken() : null;
    if (role !== "guest") {
        alert(
            "Bookings use a guest account. Register (or log in as a guest) to complete this booking — your date and time are kept in the link.",
        );
        window.location.assign(registerWithBooking);
        return;
    }

    let guestUser;
    try {
        guestUser = await GetLoggedInUser(token);
    } catch (err) {
        console.error(err);
        alert("Could not load your profile. Register again to finish your booking.");
        window.location.assign(registerWithBooking);
        return;
    }

    const guestId = Number(guestUser?.id ?? guestUser?.Id);
    if (!Number.isFinite(guestId) || guestId < 1) {
        alert("Invalid guest session. Register to complete your booking.");
        window.location.assign(registerWithBooking);
        return;
    }

    const salonForAppt = resolveSalonIdForBooking();
    const appointment = {
        guestId: guestId,
        artistId: selectedArtistId,
        salonId: salonForAppt > 0 ? salonForAppt : 0,
        serviceIds: selectedServiceIds,
        startTime: selectedSlotStartIso,
        notes: "",
    };
    try {
        await PostAppointment(appointment, token);
        const salonQ =
            salonForAppt > 0 ? `?salonId=${encodeURIComponent(String(salonForAppt))}` : "";
        window.location.assign(`/Salon/AppointmentCreated${salonQ}`);
    } catch (err) {
        console.error(err);
        alert(err?.message || "Could not book this time. Try another slot.");
    }
};
