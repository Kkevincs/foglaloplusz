let selectedDate = "";
let selectedTimeStamp = "";
let selectedArtistId = 0;
let selectedServiceIds = [];

document.addEventListener('DOMContentLoaded', () => {
    const dateInput = document.getElementById('appointment-date');
    const slots = document.querySelector('.appointment-page .time-slots');
    if (!dateInput) return;

    const params = new URLSearchParams(window.location.search);
    const serviceidsRaw = params.get('serviceids') ?? "";
    selectedServiceIds = serviceidsRaw ? serviceidsRaw.split(',').map(Number) : [];
    selectedArtistId = Number(params.get('artistid')) || 0;
    async function loadSlotsForSelectedDate() {
        selectedDate = dateInput.value;
        console.log(selectedDate);
        if (!selectedDate || selectedServiceIds.length === 0 || selectedArtistId <= 0) return;

        const timeData = {
            artistId: selectedArtistId,
            serviceIds: selectedServiceIds,
            date: selectedDate
        };

        const stamps = await GetAvaliableSlots(timeData);
        const list = document.getElementById('time-slots');
        if (!list) return;

        list.innerHTML = '';
        stamps.forEach((x) => {
            list.innerHTML += `
            <li class="time-slot">${x.startTime.split('T')[1].slice(0, 5)}</li>
        `;
        });
        selectedTimeStamp = "";
    }

    dateInput.addEventListener('change', () => {
        void loadSlotsForSelectedDate();
    });
    void loadSlotsForSelectedDate();

    if (!slots) return;

    slots.addEventListener('click', (e) => {
        const slot = e.target.closest('.time-slot');
        if (!slot) return;
        slots.querySelectorAll('.time-slot').forEach((t) => t.classList.remove('time-slot-selected'));
        slot.classList.add('time-slot-selected');
        selectedTimeStamp = slot.textContent.trim();
        console.log(selectedTimeStamp);
    });
});

window.SaveDate = async function SaveDate() {
    if (!selectedDate || !selectedTimeStamp) return;

    const response = await GetLoggedInUser(localStorage.getItem('token'));
    const id = Number(response.id);
    const startTime = `${selectedDate}T${selectedTimeStamp}`;
    const appointment = {
        guestId: id,
        artistId: selectedArtistId,
        salonId: 1,
        serviceIds: selectedServiceIds,
        startTime: startTime,
        notes: "semmi"
    };
    console.log(appointment);
    const created = await PostAppointment(appointment, localStorage.getItem('token'));
    console.log(created);
};

