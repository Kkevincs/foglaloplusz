// Index page functionality
document.addEventListener("DOMContentLoaded", () => {
    setupUserButtons();
});

function setupUserButtons() {
    // Setup getUsers button
    const getUsersBtn = document.getElementById('getUsersBtn');
    if (getUsersBtn) {
        getUsersBtn.addEventListener('click', async function() {
            try {
                const users = await getUsers();
                console.log("Users:", users);
                alert(`Fetched ${users.length} users successfully! Check console for details.`);
            } catch (error) {
                alert("Failed to fetch users: " + error.message);
            }
        });
    }

    // Setup createUser button
    const createUserBtn = document.getElementById('createUserBtn');
    if (createUserBtn) {
        createUserBtn.addEventListener('click', async function() {
            const userData = {
                Name: "Test User",
                Email: "test@example.com",
                Password: "testpassword123"
            };
            
            try {
                const user = await createUser(userData);
                console.log("User created:", user);
                alert("User created successfully! Check console for details.");
            } catch (error) {
                alert("Failed to create user: " + error.message);
            }
        });
    }
}
