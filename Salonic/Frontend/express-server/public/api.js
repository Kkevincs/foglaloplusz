
async function Login(User) {
    const response = await fetch(`https://localhost:7156/api/authentication/authenticate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const data = await response.text();
    console.log(data);
    return data;
}

async function GetAvaliableSlots(timeData) {
    const response = await fetch("https://localhost:7156/api/appointment/available", {
        method: 'PATCH',
        headers: {
            "content-type" : "application/json"
        },
        body : JSON.stringify(timeData)
    });
    const data = await response.json();
    console.log(data);
    return data;
}

async function Register(User) {
    const response = await fetch(`https://localhost:7156/api/register/guest`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const data = await response.json();
    console.log(data);
    return data;
}


async function GetServices(token) {
    const response = await fetch("https://localhost:7156/api/service", {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    })

    const data = await response.json();
    return data
}

async function GetLoggedInUser(token) {
    const response = await fetch("https://localhost:7156/api/guest/me", {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    })

    const data = await response.json();
    return data
}

async function getLoggedInArtist(token) {
    const response = await fetch("https://localhost:7156/api/artist/me", {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    })

    const data = await response.json();
    return data
}

async function GetArtists(token) {
    const response = await fetch("https://localhost:7156/api/Artist", {
        headers :{
            "Authorization": `Bearer ${token}`
        }
    })

    const data = await response.json();
    return data;       
}

async function getAppointmentByGuestId(token)
{
    const response = await fetch("https://localhost:7156/api/appointment/guest", {
        headers: {
            "authorization" : `Bearer ${token}`
        }
    })

    const data = await response.json();
    return data;

}

async function getServiceById(id, token){
    const response = await fetch(`https://localhost:7156/api/services/${id}`, {
        headers: {
            "authorization" : `Bearer ${token}`
        }
    })

    const data = response.json();
    return data;

}

async function PostAppointment(appointment, token) {
    const response = await fetch(`https://localhost:7156/api/appointment`, {
        method: "POST",
        headers: {
            "authorization" : `Bearer ${token}`,
            "content-type": "application/json"
        },
        body: JSON.stringify(appointment)
        
    });
    const data = await response.json();
    return data;
    
}

async function GetAvailableDays(date) {
    const response = await fetch(`https://localhost:7156/api/availability/month`, {
        method: 'POST',
        headers: {
            "content-type" : "application/json"
        },
        body: JSON.stringify(date)
    });

    const data = await response.json();
    return data;
}

async function AddService(service, token) {
    const response = await fetch('https://localhost:7156/api/service', {
        method: 'POST',
        headers: {
            "authorization" : `Bearer ${token}`,
            "content-type": "application/json"
        },
        body: JSON.stringify(service)
    })

    const data = await response.json();
    return data;
}

async function setWorkingHoursByDay(token, date) {
    const response = await fetch('https://localhost:7156/api/workinghours', {
        method: 'POST',
        headers: {
            "authorization" : `Bearer ${token}`,
            "content-type": "application/json"
        },
        body: JSON.stringify(date)
    })

    const data = await response.json();
    return data;
}

async function setWorkingHoursByWeek(token, date) {
    const response = await fetch('https://localhost:7156/api/workinghours/schedule', {
        method: 'POST',
        headers: {
            "authorization" : `Bearer ${token}`,
            "content-type": "application/json"
        },
        body: JSON.stringify(date)
    })

    const data = await response.json();
    return data;
}
