// Register page functionality
document.addEventListener("DOMContentLoaded", () => {
    setupRegisterForm();
    checkIfAlreadyLoggedIn();
});

function checkIfAlreadyLoggedIn() {
    const currentUser = getCurrentUser();
    if (currentUser) {
        // Redirect to user page if already logged in
        window.location.href = 'user.html';
    }
}

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

        const userData = {
            Name: name,
            Email: email,
            Password: password
        };

        try {
            const newUser = await createUser(userData);
            console.log("User created:", newUser);
            
            // Store user in localStorage
            const userToStore = {
                Id: newUser.id ?? newUser.Id,
                Name: newUser.name ?? newUser.Name,
                Email: newUser.email ?? newUser.Email
            };
            localStorage.setItem('currentUser', JSON.stringify(userToStore));
            
            if (messageEl) {
                messageEl.textContent = 'Account created successfully! Redirecting...';
                messageEl.style.color = 'green';
            }
            
            // Redirect to user page
            setTimeout(() => {
                window.location.href = 'user.html';
            }, 1500);
        } catch (error) {
            console.error("Registration failed:", error);
            if (messageEl) {
                messageEl.textContent = 'Registration failed: ' + (error.message || 'Unknown error');
                messageEl.style.color = 'red';
            }
        }
    });
}

// Helper function to get current user from localStorage
function getCurrentUser() {
    const userStr = localStorage.getItem('currentUser');
    return userStr ? JSON.parse(userStr) : null;
}
