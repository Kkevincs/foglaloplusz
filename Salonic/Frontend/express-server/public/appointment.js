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

document.addEventListener("DOMContentLoaded", () => {
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

window.SaveDate = async function SaveDate() {
    if (!selectedDate || !selectedSlotStartIso) {
        alert("Válassz dátumot és időpontot.");
        return;
    }

    if (selectedDate < appointmentFormatLocalYMD(new Date())) {
        alert("Válassz a mai napot vagy egy későbbi dátumot.");
        return;
    }

    if (selectedServiceIds.length === 0 || selectedArtistId <= 0) {
        alert("Hiányoznak a szolgáltatások vagy a szakember. Lépj vissza, és válassz újra.");
        return;
    }

    const token = localStorage.getItem("token");
    let guestUser;
    try {
        guestUser = await GetLoggedInUser(token);
    } catch (err) {
        console.error(err);
        alert("Nem sikerült betölteni a profilodat. Jelentkezz be újra.");
        return;
    }

    const guestId = Number(guestUser?.id ?? guestUser?.Id);
    if (!Number.isFinite(guestId) || guestId < 1) {
        alert("Érvénytelen vendég munkamenet. Jelentkezz be újra.");
        return;
    }

    const appointment = {
        guestId: guestId,
        artistId: selectedArtistId,
        salonId: selectedSalonId > 0 ? selectedSalonId : 0,
        serviceIds: selectedServiceIds,
        startTime: selectedSlotStartIso,
        notes: "",
    };
    try {
        const result = await PostAppointment(appointment, token);
        console.log(result);
        const salonQ =
            selectedSalonId > 0 ? `?salonId=${encodeURIComponent(String(selectedSalonId))}` : "";
        window.location.assign(`/Salon/AppointmentCreated${salonQ}`);
    } catch (err) {
        console.error(err);
        alert(err?.message || "Nem sikerült lefoglalni ezt az időpontot. Próbálj másik időpontot.");
    }
};
