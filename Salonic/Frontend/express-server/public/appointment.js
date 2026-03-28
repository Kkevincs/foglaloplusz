let selectedDate = "";
let selectedTimeStamp = "";
let selectedArtistId = 0;
let selectedServiceIds = [];

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
    selectedServiceIds = serviceidsRaw ? serviceidsRaw.split(",").map(Number) : [];
    selectedArtistId = Number(params.get("artistid")) || 0;

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
            list.innerHTML +=
                '<li class="time-slot">' + slot.startTime.split("T")[1].slice(0, 5) + "</li>";
        });
        selectedTimeStamp = "";
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
        selectedTimeStamp = slot.textContent.trim();
    });
});

window.SaveDate = async function SaveDate() {
    if (!selectedDate || !selectedTimeStamp) return;

    if (selectedDate < appointmentFormatLocalYMD(new Date())) {
        alert("Choose today or a future date.");
        return;
    }

    const guestUser = await GetLoggedInUser(localStorage.getItem("token"));
    const appointment = {
        guestId: Number(guestUser.id),
        artistId: selectedArtistId,
        salonId: 1,
        serviceIds: selectedServiceIds,
        startTime: selectedDate + "T" + selectedTimeStamp,
        notes: "semmi"
    };
    console.log(appointment);
    console.log(await PostAppointment(appointment, localStorage.getItem("token")));
};
