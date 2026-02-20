// Scheduler page functionality
document.addEventListener("DOMContentLoaded", () => {
    loadServices();
    loadWorkers();
    setupBookingForm();
    setupDateInput();
});

async function loadServices() {
    try {
        const services = await getServices();
        const select = document.getElementById('service');
        
        if (!select) return;
        
        select.innerHTML = '<option value="">Choose a service...</option>';
        (services || []).forEach(service => {
            const id = service.id ?? service.Id;
            const name = service.name ?? service.Name ?? '';
            if (id != null && id !== '') {
                select.innerHTML += `<option value="${id}">${name}</option>`;
            }
        });
    } catch (error) {
        console.error("Failed to load services:", error);
        alert("Failed to load services. Please refresh the page.");
    }
}

async function loadWorkers() {
    try {
        const workers = await getWorkers();
        const select = document.getElementById('artist');
        
        if (!select) return;
        
        select.innerHTML = '<option value="">Any available artist</option>';
        (workers || []).forEach(worker => {
            const id = worker.id ?? worker.Id;
            const name = worker.name ?? worker.Name ?? '';
            if (id != null && id !== '') {
                select.innerHTML += `<option value="${id}">${name}</option>`;
            }
        });
    } catch (error) {
        console.error("Failed to load workers:", error);
        alert("Failed to load workers. Please refresh the page.");
    }
}

function setupBookingForm() {
    const bookingForm = document.getElementById('bookingForm');
    if (!bookingForm) return;

    bookingForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const serviceId = parseInt(document.getElementById('service').value, 10);
        const artistId = document.getElementById('artist').value ? parseInt(document.getElementById('artist').value, 10) : null;
        const dateInput = document.getElementById('date').value;
        const timeDropdown = document.getElementById('time').value;

        // Validation
        if (!serviceId || isNaN(serviceId)) {
            alert('Please select a service.');
            return;
        }
        if (!dateInput) {
            alert('Please select a date.');
            return;
        }
        if (!timeDropdown) {
            alert('Please select a time.');
            return;
        }

        // Check if user is logged in
        const currentUser = getCurrentUser();
        let userId;

        if (currentUser) {
            // Use logged-in user
            userId = currentUser.id ?? currentUser.Id ?? currentUser.userId;
        } else {
            // Get user info from form
            const firstName = document.getElementById('firstName').value.trim();
            const lastName = document.getElementById('lastName').value.trim();
            const email = document.getElementById('email').value.trim();
            const phone = document.getElementById('phone').value.trim();

            if (!firstName || !lastName || !email || !phone) {
                alert('Please fill in all required contact fields or login first.');
                return;
            }

            // Create user first (since Appointment requires UserId)
            const userData = {
                Name: `${firstName} ${lastName}`,
                Email: email,
                Password: phone // Using phone as temporary password
            };

            try {
                const user = await createUser(userData);
                userId = user.id ?? user.Id;
            } catch (error) {
                console.error('Failed to create user:', error);
                alert('Failed to create user account: ' + (error.message || 'Unknown error'));
                return;
            }
        }

        const notesEl = document.getElementById('notes');
        const notes = notesEl ? notesEl.value.trim() : '';

        // Parse date and time
        const [hourStr, minuteStr] = timeDropdown.split(':');
        const hour = parseInt(hourStr, 10);
        const minute = parseInt(minuteStr, 10);
        const [yearStr, monthStr, dayStr] = dateInput.split('-');
        const year = parseInt(yearStr, 10);
        const month = parseInt(monthStr, 10);
        const day = parseInt(dayStr, 10);

        try {
            // Create appointment
            const appointmentData = {
                ServiceId: serviceId,
                UserId: userId,
                Year: year,
                Month: month,
                Day: day,
                Hour: hour,
                Minute: minute,
                Notes: notes || '',
                DurationInMinutes: 60
            };

            const appointment = await createAppointment(appointmentData);
            console.log('Booking created:', appointment);
            alert('Your appointment has been booked successfully!');
            bookingForm.reset();
        } catch (error) {
            console.error('Booking failed:', error);
            alert('Booking failed: ' + (error.message || 'Unknown error'));
        }
    });
}

function setupDateInput() {
    const dateInput = document.getElementById('date');
    if (dateInput) {
        const today = new Date().toISOString().split('T')[0];
        dateInput.setAttribute('min', today);
    }
}

// Helper function to get current user from localStorage
function getCurrentUser() {
    const userStr = localStorage.getItem('currentUser');
    return userStr ? JSON.parse(userStr) : null;
}
