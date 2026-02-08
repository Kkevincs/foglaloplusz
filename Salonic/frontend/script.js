
document.addEventListener('DOMContentLoaded', function() {
    const hamburger = document.querySelector('.hamburger');
    const navMenu = document.querySelector('.nav-menu');

    if (hamburger && navMenu) {
        hamburger.addEventListener('click', function() {
            navMenu.classList.toggle('active');
            
            const spans = hamburger.querySelectorAll('span');
            if (navMenu.classList.contains('active')) {
                spans[0].style.transform = 'rotate(45deg) translate(5px, 5px)';
                spans[1].style.opacity = '0';
                spans[2].style.transform = 'rotate(-45deg) translate(7px, -6px)';
            } else {
                spans[0].style.transform = 'none';
                spans[1].style.opacity = '1';
                spans[2].style.transform = 'none';
            }
        });

        const navLinks = document.querySelectorAll('.nav-menu a');
        navLinks.forEach(link => {
            link.addEventListener('click', function() {
                navMenu.classList.remove('active');
                const spans = hamburger.querySelectorAll('span');
                spans[0].style.transform = 'none';
                spans[1].style.opacity = '1';
                spans[2].style.transform = 'none';
            });
        });
    }



    const bookingForm = document.getElementById('bookingForm');
    if (bookingForm) {
        bookingForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            const serviceVal = document.getElementById('service').value;
            const artistVal = document.getElementById('artist').value;
            const dateInput = document.getElementById('date').value;
            const timeDropdown = document.getElementById('time').value;

            if (!serviceVal) {
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

            const serviceId = parseInt(serviceVal, 10);
            const wokerID = artistVal ? parseInt(artistVal, 10) : 0;
            if (isNaN(serviceId)) {
                alert('Please select a valid service.');
                return;
            }
            if (artistVal && isNaN(wokerID)) {
                alert('Please select a valid artist or leave as "Any available".');
                return;
            }

            const [hourStr, minuteStr] = timeDropdown.split(':');
            const hour = parseInt(hourStr, 10);
            const minute = parseInt(minuteStr, 10);
            const [yearStr, monthStr, dayStr] = dateInput.split('-');
            const year = parseInt(yearStr, 10);
            const month = parseInt(monthStr, 10);
            const day = parseInt(dayStr, 10);

            const firstName = document.getElementById('firstName').value.trim();
            const lastName = document.getElementById('lastName').value.trim();
            const email = document.getElementById('email').value.trim();
            const phone = document.getElementById('phone').value.trim();
            const notesEl = document.getElementById('notes');
            const notes = notesEl ? notesEl.value.trim() : '';

            if (!firstName || !lastName || !email || !phone) {
                alert('Please fill in all required contact fields.');
                return;
            }

            const customerName = `${lastName} ${firstName}`;

            const newAppointment = {
                ServiceID: serviceId,
                wokerID: wokerID,
                year: year,
                month: month,
                day: day,
                hour: hour,
                minute: minute,
                customerName: customerName,
                emailAddress: email,
                phoneNumber: phone,
                notes: notes || null,
                durationInMinutes: 60
            };

            try {
                const response = await fetch('https://localhost:7156/api/appointment', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(newAppointment)
                });

                if (response.ok) {
                    const data = await response.json();
                    console.log('Booking created:', data);
                    alert('Your appointment has been booked successfully!');
                    bookingForm.reset();
                } else {
                    const err = await response.text();
                    console.error('Booking failed:', response.status, err);
                    alert('Booking failed: ' + (err || response.status));
                }
            } catch (err) {
                console.error('Booking request failed:', err);
                alert('Could not reach the server. Please check your connection and that the server is running.');
            }
        });
    }

    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Thank you for your message! This is a demo form. In a real application, this would be sent to the server.');
            
            const formData = new FormData(contactForm);
            console.log('Form data:', Object.fromEntries(formData));
        });
    }

    const dateInput = document.getElementById('date');
    if (dateInput) {
        const today = new Date().toISOString().split('T')[0];
        dateInput.setAttribute('min', today);
    }
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});


