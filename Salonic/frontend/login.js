async function checkEmailPassword(email, password) {
    const users = await getUsers();
    const emailCheck = users.some(user => (user.email || user.Email) === email);
    const passwordCheck = users.some(user => (user.pwd || user.Pwd) === password);
    if (emailCheck && passwordCheck) {
        const getUser = await getUserByEmail(email);
        localStorage.setItem("email", getUser.Email || getUser.email);
        localStorage.setItem("name", getUser.Name || getUser.name);
        localStorage.setItem("id", (getUser.Id || getUser.id).toString());
        localStorage.setItem("active", "true");
        window.location.href = "user.html";
    } else if (!emailCheck) {
        document.getElementById("loginMessage").textContent = "Email not found.";
        document.getElementById("loginMessage").style.color = "red";
    } else {
        document.getElementById("loginMessage").textContent = "Incorrect password.";
        document.getElementById("loginMessage").style.color = "red";
    }
}

document.getElementById("loginForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;
    try {
        await checkEmailPassword(email, password);
    } catch (err) {
        document.getElementById("loginMessage").textContent = "Login failed: " + (err.message || "Unknown error");
        document.getElementById("loginMessage").style.color = "red";
    }
});
