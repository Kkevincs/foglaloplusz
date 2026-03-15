
const API_BASE_URL = 'https://localhost:7156/api';

async function Login(User) {
    const response = await fetch(`${API_BASE_URL}/authentication/authenticate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const data = await response.text();
    console.log(data);
    return data;
}

async function Register(User) {
    const response = await fetch(`${API_BASE_URL}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const data = await response.json();
    console.log(data);
    return data;
}

async function GetById(token) {
    console.log(token) // check if token is actually there

    const response = await fetch("https://localhost:7156/api/guest/me", {
    headers: {
        "Authorization": `Bearer ${token}`
    }
});

const data = await response.json(); // only call this ONCE
console.log(data);
}
