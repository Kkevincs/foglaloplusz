// API Base URL
const API_BASE_URL = 'https://localhost:7156/api';

// ==================== USER API ====================
async function getUsers() {
    try {
        const response = await fetch(`${API_BASE_URL}/user`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            const errorBody = await response.text();
            console.error(`Server Error (${response.status}):`, errorBody);
            throw new Error(`Server Error: ${response.status}`);
        }
        const users = await response.json();
        console.log("Fetched users successfully:", users);
        return users;
    } catch (error) {
        console.error("Failed to fetch users:", error);
        throw error;
    }
}

async function createUser(userData) {
    try {
        const response = await fetch(`${API_BASE_URL}/user`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(userData)
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        const newUser = await response.json();
        console.log("User created successfully:", newUser);
        return newUser;
    } catch (error) {
        console.error("Failed to create user:", error);
        throw error;
    }
}

async function deleteUser(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/user/${id}`, {
            method: 'DELETE',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        const deletedUser = await response.json();
        console.log("User deleted successfully:", deletedUser);
        return deletedUser;
    } catch (error) {
        console.error("Failed to delete user:", error);
        throw error;
    }
}

async function loginUser(email, password) {
    try {
        const response = await fetch(`${API_BASE_URL}/user/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Email: email, Password: password })
        });
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({ message: 'Login failed' }));
            throw new Error(errorData.message || 'Login failed');
        }
        const user = await response.json();
        console.log("Login successful:", user);
        return user;
    } catch (error) {
        console.error("Failed to login:", error);
        throw error;
    }
}

async function getUserById(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/user/${id}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        return await response.json();
    } catch (error) {
        console.error("Failed to fetch user:", error);
        throw error;
    }
}

async function getUserByEmail(email) {
    try {
        const response = await fetch(`${API_BASE_URL}/user/${email}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        return await response.json();
    } catch (error) {
        console.error("Failed to fetch user:", error);
        throw error;
    }
}

// ==================== SERVICE API ====================
async function getServices(category = '') {
    try {
        const url = category ? `${API_BASE_URL}/service?category=${encodeURIComponent(category)}` : `${API_BASE_URL}/service`;
        const response = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            const errorBody = await response.text();
            console.error(`Server Error (${response.status}):`, errorBody);
            throw new Error(`Server Error: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error("Failed to fetch services:", error);
        throw error;
    }
}

async function createService(serviceData) {
    try {
        const response = await fetch(`${API_BASE_URL}/service`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(serviceData)
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        const newService = await response.json();
        console.log("Service created successfully:", newService);
        return newService;
    } catch (error) {
        console.error("Failed to create service:", error);
        throw error;
    }
}

async function deleteService(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/service/${id}`, {
            method: 'DELETE',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            let errorMessage = `Server Error: ${response.status}`;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorMessage;
                } catch (e) { errorMessage = response.statusText || errorMessage; }
            } else {
                try {
                    const errorText = await response.text();
                    if (errorText) errorMessage = errorText;
                } catch (e) { errorMessage = response.statusText || errorMessage; }
            }
            throw new Error(errorMessage);
        }
        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            const result = await response.json();
            console.log("Service deleted successfully:", result);
            return result;
        }
        console.log("Service deleted successfully");
        return { success: true };
    } catch (error) {
        console.error("Failed to delete service:", error);
        throw error;
    }
}

// ==================== WORKER API ====================
async function getWorkers() {
    try {
        const response = await fetch(`${API_BASE_URL}/worker`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            const errorBody = await response.text();
            console.error(`Server Error (${response.status}):`, errorBody);
            throw new Error(`Server Error: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error("Failed to fetch workers:", error);
        throw error;
    }
}

async function createWorker(workerData) {
    try {
        const response = await fetch(`${API_BASE_URL}/worker`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(workerData)
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        const newWorker = await response.json();
        console.log("Worker created successfully:", newWorker);
        return newWorker;
    } catch (error) {
        console.error("Failed to create worker:", error);
        throw error;
    }
}

// ==================== APPOINTMENT API ====================
async function createAppointment(appointmentData) {
    try {
        const response = await fetch(`${API_BASE_URL}/appointment`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(appointmentData)
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server Error: ${errorText}`);
        }
        const newAppointment = await response.json();
        console.log("Appointment created successfully:", newAppointment);
        return newAppointment;
    } catch (error) {
        console.error("Failed to create appointment:", error);
        throw error;
    }
}
