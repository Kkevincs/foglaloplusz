async function checkUserExists(u) {
    const users = await getUsers();
    const bool = users.some(user => (user.email || user.Email) === u);
    return bool;
}

document.getElementById("registerForm").addEventListener("submit", async function(e) {
    e.preventDefault();
    const messageEl = document.getElementById('registerMessage');
    const selectedValue = document.querySelector('input[name="accountType"]:checked').value;
    const name = document.getElementById("name").value.trim();
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    if (selectedValue == "user" && (await checkUserExists(email)) === false) {
        try {
            await createUser({ Name: name, Email: email, Password: password });
            if (messageEl) { messageEl.textContent = 'Account created! Redirecting...'; messageEl.style.color = 'green'; }
            setTimeout(() => { window.location.href = 'login.html'; }, 1500);
        } catch (err) {
            if (messageEl) { messageEl.textContent = 'Registration failed: ' + (err.message || ''); messageEl.style.color = 'red'; }
        }
    } else if (selectedValue == "worker" && (await checkUserExists(email)) === false) {
        try {
            await createWorker({ Name: name, Email: email, Pwd: password, Description: '' });
            if (messageEl) { messageEl.textContent = 'Account created! Redirecting...'; messageEl.style.color = 'green'; }
            setTimeout(() => { window.location.href = 'login.html'; }, 1500);
        } catch (err) {
            if (messageEl) { messageEl.textContent = 'Registration failed: ' + (err.message || ''); messageEl.style.color = 'red'; }
        }
    } else {
        if (messageEl) { messageEl.textContent = 'Email is already taken.'; messageEl.style.color = 'red'; }
    }
});
