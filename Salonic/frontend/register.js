// Register page functionality
document.addEventListener("DOMContentLoaded", () => {
    setupRegisterForm();
});


function setupRegisterForm() {
    const registerForm = document.getElementById('registerForm');
    if (!registerForm) return;

    registerForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const messageEl = document.getElementById('registerMessage');
        if (messageEl) {
            messageEl.textContent = '';
            messageEl.style.color = '';
        }

        const name = document.getElementById('name').value.trim();
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        // Validation
        if (!name || !email || !password || !confirmPassword) {
            if (messageEl) {
                messageEl.textContent = 'Please fill in all fields.';
                messageEl.style.color = 'red';
            }
            return;
        }

        if (password !== confirmPassword) {
            if (messageEl) {
                messageEl.textContent = 'Passwords do not match.';
                messageEl.style.color = 'red';
            }
            return;
        }

        if (password.length < 3) {
            if (messageEl) {
                messageEl.textContent = 'Password must be at least 3 characters long.';
                messageEl.style.color = 'red';
            }
            return;
        }

        const accountType = document.querySelector('input[name="accountType"]:checked').value;

        try {
            if (accountType === 'worker') {
                // Create worker account - stores in Workers table
                const workerData = {
                    Name: name,
                    Email: email,
                    Pwd: password  // Worker model uses Pwd field
                };

                const newWorker = await createWorker(workerData);
                console.log("Worker created:", newWorker);
                
                if (messageEl) {
                    messageEl.textContent = 'Account created successfully! Redirecting...';
                    messageEl.style.color = 'green';
                }
                
                // Redirect to login page
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 1500);
            } else {
                // Create user/guest account - stores in Users table
                const userData = {
                    Name: name,
                    Email: email,
                    Password: password  // User model uses Password field
                };

                const newUser = await createUser(userData);
                console.log("User created:", newUser);
                
                if (messageEl) {
                    messageEl.textContent = 'Account created successfully! Redirecting...';
                    messageEl.style.color = 'green';
                }
                
                // Redirect to login page
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 1500);
            }
        } catch (error) {
            console.error("Registration failed:", error);
            if (messageEl) {
                messageEl.textContent = 'Registration failed: ' + (error.message || 'Unknown error');
                messageEl.style.color = 'red';
            }
        }
    });
}


