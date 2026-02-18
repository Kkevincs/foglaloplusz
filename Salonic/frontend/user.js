// User page functionality
document.addEventListener("DOMContentLoaded", () => {
    checkAuthentication();
    setupLogout();
});

function checkAuthentication() {
    const currentUser = getCurrentUser();
    
    if (!currentUser) {
        // Redirect to login if not authenticated
        alert('Please login to view your profile.');
        window.location.href = 'login.html';
        return;
    }
    
    // Load user data
    loadUserData(currentUser);
}

function getCurrentUser() {
    const userStr = localStorage.getItem('currentUser');
    return userStr ? JSON.parse(userStr) : null;
}

async function loadUserData(user) {
    try {
        // Use the user ID from localStorage or fetch fresh data
        const userId = user.id ?? user.Id ?? user.userId;
        
        if (!userId) {
            // If no ID, try to get it from the stored user object
            console.error("No user ID found");
            displayUserInfo(user);
            return;
        }
        
        // Fetch fresh user data from API
        const userData = await getUserById(userId);
        displayUserInfo(userData);
    } catch (error) {
        console.error("Failed to load user data:", error);
        // Fallback to displaying data from localStorage
        displayUserInfo(user);
    }
}

function displayUserInfo(user) {
    const name = user.name ?? user.Name ?? 'N/A';
    const email = user.email ?? user.Email ?? 'N/A';
    const id = user.id ?? user.Id ?? user.userId ?? 'N/A';
    
    document.getElementById('userName').textContent = name;
    document.getElementById('userEmail').textContent = email;
    document.getElementById('userId').textContent = id;
}

function setupLogout() {
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function() {
            if (confirm('Are you sure you want to logout?')) {
                localStorage.removeItem('currentUser');
                window.location.href = 'index.html';
            }
        });
    }
}
