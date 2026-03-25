document.addEventListener('DOMContentLoaded', () => {
    const dateInput = document.getElementById('appointment-date');
    const slots = document.querySelector('.appointment-page .time-slots');
    if (!dateInput) return;

    async function loadSlotsForSelectedDate() {
        const date = dateInput.value;
        console.log(date);

        const params = new URLSearchParams(window.location.search);
        const serviceidsRaw = params.get('serviceids');
        if (!serviceidsRaw) return;

        const serviceids = serviceidsRaw.split(',').map(Number);
        const artistid = Number(params.get('artistid'));

        const timeData = {
            artistId: artistid,
            serviceIds: serviceids,
            date: date
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

        console.log(date);
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
    });
});
