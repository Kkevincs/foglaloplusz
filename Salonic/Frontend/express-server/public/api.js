
function normalizeAuthToken(raw) {
    if (!raw) return raw;
    const trimmed = raw.trim();
    if (trimmed.startsWith("\"") && trimmed.endsWith("\"")) {
        try {
            return JSON.parse(trimmed);
        } catch (_) {
            return trimmed.slice(1, -1);
        }
    }
    return trimmed;
}
async function getServicesByArtist(token){
    const response = await fetch(`https://localhost:7156/api/Artist/services`, {
        headers: {"Authorization": `Bearer ${token}`}
    });
    const data = await response.json();
    return data;
}

async function Login(User) {
    const response = await fetch(`https://localhost:7156/api/authentication/authenticate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const data = await response.text();
    const token = normalizeAuthToken(data);
    console.log(token);
    return token;
}

async function LoginWithStatus(User) {
    const response = await fetch(`https://localhost:7156/api/authentication/authenticate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(User)
    });
    const body = await response.text();
    const token = normalizeAuthToken(body);
    console.log(token);
    return { ok: response.ok, token: response.ok ? token : null };
}
async function GetArtistWorkingHoursByMonth(token, date){
    const response = await fetch(`https://localhost:7156/api/workinghours/range`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json',  
                    "Authorization": `Bearer ${token}` },
        body: JSON.stringify(date)
    });

    const data = await response.json();
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
async function RegisterArtist(user) {
    const response = await fetch(`https://localhost:7156/api/register/artist`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(user)
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

async function getBookingNames(ids) {
    const response = await fetch(`https://localhost:7156/api/lookup/names`, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify(ids)
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

/** @param salon {{ name: string, email: string, theme: string, description: string, logoFile?: File | null }} */
async function AddSalon(token, salon) {
    const formData = new FormData();
    formData.append("name", salon.name);
    formData.append("email", salon.email);
    formData.append("theme", salon.theme ?? "");
    formData.append("description", salon.description ?? "");
    if (salon.logoFile instanceof File) {
        formData.append("logo", salon.logoFile);
    }

    const response = await fetch(`https://localhost:7156/api/salon`, {
        method: "POST",
        headers: {
            "authorization": `Bearer ${token}`
        },
        body: formData
    });
    const data = await response.json();
    console.log(data);
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

async function DeleteSalon(token, salonId) {
    const response = await fetch(`https://localhost:7156/api/salon/${salonId}`, {
        method: "DELETE",
        headers: {
            "authorization": `Bearer ${token}`
        }
    });
    if (!response.ok) {
        let detail = response.statusText;
        try {
            const body = await response.text();
            if (body) detail = body;
        } catch (_) { /* ignore */ }
        throw new Error(detail || `HTTP ${response.status}`);
    }
    return true;
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
    const text = await response.text();
    if (!response.ok) {
        let msg = text || `HTTP ${response.status}`;
        try {
            const j = JSON.parse(text);
            if (typeof j === "string") msg = j;
            else msg = j?.detail || j?.title || j?.message || j?.errors
                ? JSON.stringify(j.errors || j)
                : msg;
        } catch (_) {
            if (msg.length > 300) msg = msg.slice(0, 300) + "…";
        }
        throw new Error(typeof msg === "string" ? msg : "Request failed");
    }
    if (!text) return null;
    try {
        return JSON.parse(text);
    } catch (_) {
        return text;
    }
}

async function GetAppointmentBySalonId(token, id){
    const response = await fetch(`https://localhost:7156/api/appointment/salon/${id}`, {
        headers: {
            "authorization": `Bearer ${token}`
        }
    });

    const data = await response.json();
    console.log(data)
    return data;
}

/** @param weekStartIso {string|undefined} Monday as yyyy-MM-dd, optional */
async function GetOwnerArtistWeekSchedule(token, salonId, artistId, weekStartIso) {
    const qs = weekStartIso ? `?weekStart=${encodeURIComponent(weekStartIso)}` : "";
    const response = await fetch(`https://localhost:7156/api/salon/${salonId}/artists/${artistId}/schedule${qs}`, {
        headers: {
            "authorization": `Bearer ${token}`
        }
    });
    if (!response.ok) {
        let detail = response.statusText;
        try {
            const body = await response.text();
            if (body) detail = body;
        } catch (_) { /* ignore */ }
        throw new Error(detail || `HTTP ${response.status}`);
    }
    return response.json();
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
//async function GetSalonThemeBySalonId(token, salonId) {
//    const response = await fetch(`https://localhost:7156/api/salon/${salonId}/theme`, {
//        headers: { authorization: `Bearer ${token}` }
//    });
//    return await response.text();
//}
async function GetSalonBySalonId(token, salonId) {
    const response = await fetch(`https://localhost:7156/api/salon/${salonId}/public-profile`, {
        headers: { authorization: `Bearer ${token}` }
    });
    return await response.json();
}
//async function applySalonHomepage(salonId) {
//    const response = await fetch(`https://localhost:7156/api/salon/${salonId}/public-profile`);
//    const salon = await GetSalonBySalonId();
//    applyTheme(salon.theme);
//    document.getElementById('logo').src = salon.logo;
//    document.getElementById('salon-name').src = salon.logo;
//    document.getElementById('salon-description').textContent = salon.description;
//    document.getElementById('salon-support-email').href = `mailto:${salon.supportEmail}`;
//}