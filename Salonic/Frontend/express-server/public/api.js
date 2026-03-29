
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

/** Same as Login but returns whether the response succeeded (avoids storing error HTML as token). */
async function LoginWithStatus(User) {
    const response = await fetch(`https://localhost:7156/api/authentication/authenticate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const body = await response.text();
    console.log(body);
    return { ok: response.ok, token: response.ok ? body : null };
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

async function RegisterOwner(User) {
    const response = await fetch(`https://localhost:7156/api/register/owner`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    let data = null;
    try {
        data = await response.json();
    } catch (_) {
    }
    console.log(data);
    return { ok: response.ok, data };
}


async function GetServices(salonId) {
    const response = await fetch(`https://localhost:7156/api/salon/${salonId}/services`);
    const data = await response.json();
    return data;
}

async function getBookingNames(artistId, guestId, serviceIds) {
    const response = await fetch(`https://localhost:7156/api/lookup/names`, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({ artistId, guestId, serviceIds })
    });
    const data = await response.json();
    return data;
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

async function AddSalon(token, salon) {
    const response = await fetch(`https://localhost:7156/api/salon`, {
        method: "POST",
        headers: {
            "authorization" : `Bearer ${token}`,
            "content-type": "application/json"
        },
        body: JSON.stringify(salon)
        
    });
    const data = await response.json();
    console.log(data)
    return data;
    
}

async function GetMySalons(token) {
    const response = await fetch(`https://localhost:7156/api/salon/my`,{
        method: 'GET',
        headers:{
            "authorization" : `Bearer ${token}`,
        }
    })
    const data = await response.json();
    console.log(data)
    return data;
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

async function getArtistBySalonId(token, id) {
    const response = await fetch(`https://localhost:7156/api/salon/${id}/artists`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    })

    const data = await response.json();
    console.log(data);
    return data
}

async function GetLoggedInOwner(token) {
    const response = await fetch("https://localhost:7156/api/owner/me", {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const data = await response.json();
    return data;
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

async function getServiceById(salonId, id, token) {
    const response = await fetch(`https://localhost:7156/api/salon/${salonId}/services/${id}`, {
        headers: {
            "authorization": `Bearer ${token}`
        }
    });

    const data = await response.json();
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

/** Artist: salon resolved from JWT + artist row (no salonId in URL). */
async function AddArtistService(service, token) {
    const response = await fetch(`https://localhost:7156/api/Artist/services`, {
        method: 'POST',
        headers: {
            "authorization": `Bearer ${token}`,
            "content-type": "application/json"
        },
        body: JSON.stringify(service)
    });

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
