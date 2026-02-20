// Login page functionality
document.addEventListener("DOMContentLoaded", () => {
    setupLoginForm();
});


function setupLoginForm() {
    const loginForm = document.getElementById('loginForm');
    if (!loginForm) return;

    loginForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const messageEl = document.getElementById('loginMessage');
        if (messageEl) {
            messageEl.textContent = '';
            messageEl.style.color = '';
        }

        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;

        if (!email || !password) {
            if (messageEl) {
                messageEl.textContent = 'Please fill in all fields.';
                messageEl.style.color = 'red';
            }
            return;
        }

        try {
            const user = await loginUser(email, password);
            
            // Store user in localStorage
            
            if (messageEl) {
                messageEl.textContent = 'Login successful! Redirecting...';
                messageEl.style.color = 'green';
            }
            
            // Redirect to user page
            setTimeout(() => {
                window.location.href = 'user.html';
            }, 1000);
        } catch (error) {
            console.error("Login failed:", error);
            if (messageEl) {
                messageEl.textContent = 'Login failed: ' + (error.message || 'Invalid email or password');
                messageEl.style.color = 'red';
            }
        }
    });
}
