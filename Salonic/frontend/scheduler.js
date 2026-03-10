document.addEventListener("DOMContentLoaded", () => {
    loadServices();
    loadWorkers();
    setupBookingForm();
});

async function loadServices() {
    try {
        const services = await getServices();
        const sel = document.getElementById("service");
        if (!sel) return;
        sel.innerHTML = '<option value="">Choose a service...</option>';
        services.forEach(s => {
            const opt = document.createElement('option');
            opt.value = s.id ?? s.Id;
            opt.textContent = s.name ?? s.Name;
            sel.appendChild(opt);
        });
    } catch (error) { console.error("Failed to load services:", error); }
}

async function loadWorkers() {
    try {
        const workers = await getWorkers();
        const sel = document.getElementById("artist");
        if (!sel) return;
        sel.innerHTML = '<option value="">Choose an artist...</option>';
        workers.forEach(w => {
            const opt = document.createElement('option');
            opt.value = w.id ?? w.Id;
            opt.textContent = w.name ?? w.Name;
            sel.appendChild(opt);
        });
    } catch (error) { console.error("Failed to load workers:", error); }
}

function getCurrentUser() {
    if (localStorage.getItem("active") === "true" && localStorage.getItem("id")) {
        return { id: parseInt(localStorage.getItem("id"), 10), Id: parseInt(localStorage.getItem("id"), 10) };
    }
    return null;
}

function setupBookingForm() {
    const form = document.getElementById('bookingForm');
    if (!form) return;
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        const serviceId = parseInt(document.getElementById("service").value);
        const workerId = parseInt(document.getElementById("artist").value);
        const dateValue = document.getElementById('date').value;
        const timeValue = document.getElementById("time").value;
        const firstName = document.getElementById('firstName').value.trim();
        const lastName = document.getElementById('lastName').value.trim();
        const email = document.getElementById('email').value.trim();
        const notes = document.getElementById('notes').value.trim() || "";
        if (!serviceId || !workerId || !dateValue || !timeValue || !firstName || !lastName || !email) {
            alert('Please fill in all required fields.');
            return;
        }
        const [year, month, day] = dateValue.split('-').map(Number);
        const [hour, minute] = timeValue.split(':').map(Number);
        try {
            let guestId;
            const currentUser = getCurrentUser();
            if (currentUser && (currentUser.id || currentUser.Id)) {
                guestId = currentUser.id || currentUser.Id;
            } else {
                const newGuest = await createUser({ Name: firstName + ' ' + lastName, Email: email, Password: '' });
                guestId = newGuest.id ?? newGuest.Id;
            }
            await createAppointment({
                GuestId: guestId,
                WorkerId: workerId,
                ServiceId: serviceId,
                Year: year,
                Month: month,
                Day: day,
                Hour: hour,
                Minute: minute,
                Notes: notes,
                DurationInMinutes: 60
            });
            alert('Appointment booked successfully!');
            form.reset();
        } catch (error) {
            alert('Failed to book: ' + (error.message || 'Unknown error'));
        }
    });
}
