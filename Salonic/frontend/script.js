// Navigation and common UI functionality
document.addEventListener('DOMContentLoaded', function() {
    setupNavigation();
    setupSmoothScrolling();
});


function setupNavigation() {

    if(!localStorage.getItem("email")){
    document.getElementById('nav-bar').innerHTML = `
            <li><a href="index.html" class="active">Home</a></li>
                <li><a href="about.html">About</a></li>
                <li><a href="services.html">Services</a></li>
                <li><a href="profiles.html">Our Team</a></li>
                <li><a href="scheduler.html">Book Appointment</a></li>
                <li><a href="contact.html">Contact</a></li>
                <li><a href="nonloggedinuser.html">User</a></li>
                <li><a href="login.html">Login</a></li>
                <li><a href="register.html">Register</a></li>

        `;
    }
    else{
        document.getElementById('nav-bar').innerHTML = `
            <li><a href="index.html" class="active">Home</a></li>
                <li><a href="about.html">About</a></li>
                <li><a href="services.html">Services</a></li>
                <li><a href="addService.html">Add Service</a></li>
                <li><a href="profiles.html">Our Team</a></li>
                <li><a href="user.html">User</a></li>
                <li><a href="scheduler.html">Book Appointment</a></li>
                <li><a href="contact.html">Contact</a></li>

        `;
    }

    
    //


    const hamburger = document.querySelector('.hamburger');
    const navMenu = document.querySelector('.nav-menu');
    if (hamburger && navMenu) {
        hamburger.addEventListener('click', function() {
            navMenu.classList.toggle('active');
            hamburger.classList.toggle('active');
        });

        // Close menu when clicking on a link
        const navLinks = document.querySelectorAll('.nav-menu a');
        navLinks.forEach(link => {
            link.addEventListener('click', function() {
                navMenu.classList.remove('active');
                hamburger.classList.remove('active');
            });
        });

        // Close menu when clicking outside
        document.addEventListener('click', function(event) {
            const isClickInsideNav = navMenu.contains(event.target);
            const isClickOnHamburger = hamburger.contains(event.target);
            
            if (!isClickInsideNav && !isClickOnHamburger && navMenu.classList.contains('active')) {
                navMenu.classList.remove('active');
                hamburger.classList.remove('active');
            }
        });
    }



    const bookingForm = document.getElementById('bookingForm');
    if (bookingForm) {
        bookingForm.addEventListener('submit', async function(e) {
            e.preventDefault();

           //TODO logic
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
}

function setupSmoothScrolling() {
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
}

