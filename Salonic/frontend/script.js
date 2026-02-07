// Mobile Navigation Toggle
document.addEventListener('DOMContentLoaded', function() {
    const hamburger = document.querySelector('.hamburger');
    const navMenu = document.querySelector('.nav-menu');

    if (hamburger && navMenu) {
        hamburger.addEventListener('click', function() {
            navMenu.classList.toggle('active');
            
            // Animate hamburger
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

        // Close menu when clicking on a link
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

    // Booking Form Handler (prevent default for now)
    const bookingForm = document.getElementById('bookingForm');
    if (bookingForm) {
        bookingForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            const timeDropdown = document.getElementById('time').value;
            const [hourStr, minuteStr] = timeDropdown.split(':');
            const hour = parseInt(hourStr);
            const minute = parseInt(minuteStr);

            const dateInput = document.getElementById('date').value;

            const [yearStr, monthStr, dayStr] = dateInput.split('-')

            const year = parseInt(yearStr, 10);
            const month = parseInt(monthStr, 10);
            const day = parseInt(dayStr, 10);
            alert('Thank you for your booking request! This is a demo form. In a real application, this would be sent to the server.');
            const firstName = document.getElementById("firstName").value;
            const lastName = document.getElementById("lastName").value;
            const name = `${lastName} ${firstName}`
            const notes = document.getElementById("notes").value;
            const newAppointment = {
                year: year,
                month: month,
                day: day,
                hour: hour,
                minute: minute,
                customerName: name,
                notes: notes,
                durationInMinutes: 60  // default 1 hour; change or get from form if needed
            }

            const response = await fetch('https://localhost:7156/api/appointment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newAppointment)
            })

            if (response.ok) {
                const data = await response.json();
                console.log('Booking created:', data);
                alert('Your appointment has been booked successfully!');
            } else {
                const err = await response.text();
                console.error('Booking failed:', response.status, err);
                alert('Booking failed. Please try again.');
            }
            
            // Optional: Show a success message
            const formData = new FormData(bookingForm);
            console.log('Form data:', Object.fromEntries(formData));
        });
    }

    // Contact Form Handler (prevent default for now)
    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Thank you for your message! This is a demo form. In a real application, this would be sent to the server.');
            
            // Optional: Show a success message
            const formData = new FormData(contactForm);
            console.log('Form data:', Object.fromEntries(formData));
        });
    }

    // Set minimum date for booking form to today
    const dateInput = document.getElementById('date');
    if (dateInput) {
        const today = new Date().toISOString().split('T')[0];
        dateInput.setAttribute('min', today);
    }

    // Smooth scroll for anchor links
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


