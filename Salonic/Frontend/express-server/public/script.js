document.addEventListener('DOMContentLoaded', async () => {
    const ownerOnlyPaths = ["/maker", "/owneruserpage", "/salondashboard", "/salondashboard/schedule", "/ownerartistregister"];
    if (ownerOnlyPaths.includes(window.location.pathname)) {
        const token = localStorage.getItem("token");
        let role = null;
        if (token) {
            try {
                role = parseJwtPayload(token)?.role ?? null;
            } catch (_) {
                role = null;
            }
        }
        if (role !== "owner") {
            window.location.assign("/ownerlogin");
            return;
        }
    }

    const path = window.location.pathname;
    if (path === '/Salon' || path.startsWith('/Salon/')) {
        const guestParams = new URLSearchParams(window.location.search);
        const guestSalonId = guestParams.get('salonId') || guestParams.get('salonid');
        if (!guestSalonId) {
            window.location.assign('/ownerregister');
            return;
        }
        if (!(await assertPublicSalonExistsOrRedirect(guestSalonId))) return;
        await applySalonThemeForPublicPages();
        await applySalonPersonalLogo();
    }
    
    if (window.location.pathname === '/ownerhomepage') {
        loadSalons();
        document.getElementById('owner-salons-list')?.addEventListener('click', async (e) => {
            const btn = e.target.closest('[data-action="remove-salon"]');
            if (!btn) return;
            const salonId = btn.getAttribute('data-salon-id');
            if (!salonId) return;
            if (!confirm('Remove this salon permanently? All artists, services, appointments, and working hours for this salon will be deleted.')) {
                return;
            }
            const token = localStorage.getItem('token');
            try {
                btn.disabled = true;
                await DeleteSalon(token, salonId);
                await loadSalons();
            } catch (err) {
                console.error(err);
                alert(err?.message || 'Could not remove salon.');
                btn.disabled = false;
            }
        });
    }

    if (window.location.pathname === '/ownerartistregister') {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get('salonId');
        (async () => {
            const ok = await assertOwnerHasSalonOrRedirect(salonId);
            if (!ok) return;
            formRegisterArtist(salonId);
        })();
    }

    if(window.location.pathname === '/Salon/Artist/Hours'){
        loadWorkingHours()
    }

    if(window.location.pathname === '/Salon/Artist/Services')
    {
        loadServicesForArtists();
    }
    if (window.location.pathname === '/salondashboard') {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get('salonId');
        (async () => {
            const ok = await assertOwnerHasSalonOrRedirect(salonId);
            if (!ok) return;
            loadArtistsBySalonId(salonId);
            loadServicesBySalon(salonId);
            loadAppointments(salonId);
        })();
    }

    if (window.location.pathname === '/salondashboard/schedule') {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get('salonId');
        (async () => {
            const ok = await assertOwnerHasSalonOrRedirect(salonId);
            if (!ok) return;
            loadOwnerArtistSchedulePage();
        })();
    }

    if (document.getElementById('services-list')) LoadServices();
    if (document.getElementById('user-profile')) LoadUserData();
    if (document.getElementById('artists-list')) LoadArtists();
    if (document.getElementById('artist-profile')) LoadArtistData();
    if (document.getElementById('owner-profile')) LoadOwnerData();
    if (document.getElementById('salon-info')) applySalonPersonalHomepage();
});
document.querySelectorAll('.nav-links a').forEach(a => {
    a.addEventListener('click', e => {
        if (a.getAttribute('href') === '#') e.preventDefault();
    });
});

function parseJwtPayload(token) {
    const part = token.split('.')[1];
    if (!part) return null;
    const json = atob(part.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json);
  }

function getRoleFromToken() {
    const token = localStorage.getItem("token");
    if (!token) return null;
    try {
        return parseJwtPayload(token)?.role ?? null;
    } catch (_) {
        return null;
    }
}

window.getRoleFromToken = getRoleFromToken;

async function applySalonPersonalHomepage() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
    document.getElementById('salon-name').textContent = salon.name;
    document.getElementById('salon-description').textContent = salon.description;
    document.getElementById('salon-support-email').href = `mailto:${salon.email}`;
    document.getElementById('salon-support-email').textContent = salon.email;
}
async function applySalonPersonalLogo() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
    const logoUrl = `http://localhost:5181${salon.logo}`
    logoLink.innerHTML = `<img src="${logoUrl}" alt="${salon.name}">`;
}

// Only keep this if you actually need to store multiple salon themes in one browser
function getCache() {
    return JSON.parse(localStorage.getItem("salonThemeById") || "{}");
}

function applyTheme(theme) {
    if (!theme) return;
    document.body.className = document.body.className
        .split(' ')
        .filter(cls => !cls.startsWith('theme-'))
        .join(' ');
    document.body.classList.add(theme.trim());
}
async function applySalonThemeForPublicPages() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const cache = getCache();
    try {
        const salon = await GetSalonBySalonId(localStorage.getItem('token'), salonId);
        if (salon) {
            applyTheme(salon.theme);
            cache[salonId] = salon.theme;
            localStorage.setItem("salonThemeById", JSON.stringify(cache));
            return;
        }
    } catch (e) {
        applyTheme(cache[salonId]);
    }
}

/**
 * Guest /Salon routes: salon must exist in the database.
 */
async function assertPublicSalonExistsOrRedirect(salonIdRaw) {
    const id = parseInt(String(salonIdRaw ?? '').trim(), 10);
    if (!Number.isFinite(id) || id < 1) {
        window.location.replace('/404');
        return false;
    }
    try {
        const response = await fetch(`https://localhost:7156/api/salon/${id}/exists`);
        if (!response.ok) {
            window.location.replace('/404');
            return false;
        }
    } catch (e) {
        console.error(e);
        window.location.replace('/404');
        return false;
    }
    return true;
}

/**
 * For owner-only pages: salonId must be a real salon row that belongs to the logged-in owner.
 * Otherwise redirects to /404 (covers unknown IDs and other owners’ salons).
 */
async function assertOwnerHasSalonOrRedirect(salonIdRaw) {
    const id = parseInt(String(salonIdRaw ?? '').trim(), 10);
    if (!Number.isFinite(id) || id < 1) {
        window.location.replace('/404');
        return false;
    }
    try {
        const token = localStorage.getItem('token');
        const salons = await GetMySalons(token);
        const sid = (s) => Number(s.id ?? s.Id);
        if (!Array.isArray(salons) || !salons.some((s) => sid(s) === id)) {
            window.location.replace('/404');
            return false;
        }
    } catch (e) {
        console.error(e);
        window.location.replace('/404');
        return false;
    }
    return true;
}

document.getElementById('artist-login-form')?.addEventListener('submit',async (e) => {
    e.preventDefault();
    const form = e.target;
    const email = form.querySelector('#ArtistEmail').value;
    const password = form.querySelector('#ArtistPassword').value
    const role = "artist"

    const artist = {
        email: email,
        role: role,
        pwd: password
    }
    const token = await Login(artist);
    localStorage.setItem("token", token);
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
    window.location.assign(`/Salon/Artist/User${salonQuery}`);

})

async function loadAppointments(id) {
    const response = await GetAppointmentBySalonId(localStorage.getItem('token'), id)
    for (const x of response) {
        // Fix: Replace all hyphens with colons before splitting, but only on the date part causes unwanted token.
        // The original code only replaces the first '-' with ':', which is incorrect for date formatting.
        // Instead, we likely want the local date string or just the date part as YYYY-MM-DD.
        // If you want to output only the date portion in the original format, do:
        const dt = new Date(x.startTime);
        const yyyy = dt.getFullYear();
        const mm = String(dt.getMonth() + 1).padStart(2, '0');
        const dd = String(dt.getDate()).padStart(2, '0');
        const hours = String(dt.getHours()).padStart(2, '0');
        const mins = String(dt.getMinutes()).padStart(2, '0');
        const start = `${yyyy}:${mm}:${dd} & ${hours}:${mins}`;
        const guestId = x.guestId
        const artistId = x.artistId
        const services = x.serviceIds
        const ids = {
            guestId: guestId,
            artistId: artistId,
            serviceIds: services
        }
        console.log(ids)
        const names = await getBookingNames(ids)
        const serviceNames = Array.isArray(names.services)
            ? names.services.map((s) => s.name).join(", ")
            : "";
        document.getElementById('apps-table').innerHTML += `
        
                        <tr>
                            <td>${start}</td>
                            <td>${names.artistName}</td>
                            <td>${names.guestName}</td>
                            <td>${serviceNames}</td>
                        </tr>
        `
    }
    return
    
}

async function formRegisterArtist(id){
    document.getElementById('owner-artist-register-form').addEventListener('submit',async (e) => {
        e.preventDefault();
        const form = e.target;
        const name = form.querySelector('#ArtistRegName').value;
        const email = form.querySelector('#ArtistRegEmail').value;
        const password = form.querySelector('#ArtistRegPassword').value;
        const description = form.querySelector('#ArtistRegDescription').value;

        const artist = {
            name: name,
            email: email,
            pwd: password,
            salonId: id,
            description: description
        }

        console.log(await RegisterArtist(artist));
    })
}

async function loadWorkingHours() {
    const startRaw = new Date();
    const endRaw = new Date(startRaw);
    endRaw.setDate(endRaw.getDate() + 7);

    const formatYmd = (d) => {
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, "0");
        const day = String(d.getDate()).padStart(2, "0");
        return `${y}-${m}-${day}`;
    };

    const start = formatYmd(startRaw);
    const end = formatYmd(endRaw);
    const date = {
        startDate: start,
        endDate: end
    }
    console.log(date)
    const hours = await GetArtistWorkingHoursByMonth(localStorage.getItem('token'), date )
    const days = ["Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek", "Szombat", "Vasárnap"]
    hours.forEach(x => {
        const [year, month, dayPart] = x.date.split("-");
        const day = dayPart.slice(0, 2);
        const dayName = days[new Date(x.date).getDay() - 1]
        const startTime = String(x.startTime).trim();
        const endTime = String(x.endTime).trim();
        const isClosed =
            (startTime === "0" && endTime === "0") ||
            (startTime === "00:00" && endTime === "00:00");
        openHours = isClosed ? "closed" : `${startTime} - ${endTime}`;
        document.getElementById('artist-hours-list').innerHTML += `

            <li><span>${dayName} (${day}/${month})</span> <strong>${openHours}</strong></li>
        `
})
    
    
}

document.getElementById('salon-setup-form')?.addEventListener('submit',async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector("#salon-name").value;
    const logoInput = form.querySelector("#salon-logo");
    const logoFile = logoInput?.files?.[0] ?? null;
    const description = form.querySelector("#salon-description").value;
    const theme = form.querySelector('#salon-theme').value;
    const email = form.querySelector('#support-email').value;

    const newSalon = {
        name,
        email,
        theme,
        description,
        logoFile
    };
    const createdSalon = await AddSalon(localStorage.getItem('token'), newSalon);

    const salonId = createdSalon?.id ?? createdSalon?.Id;
    if (salonId) {
        const cache = getSalonThemeCache();
        cache[String(salonId)] = theme;
        setSalonThemeCache(cache);
    }
})



document.getElementById('guest-login-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const email = form.querySelector('#Email').value;
    const password = form.querySelector('#Password').value;
    const user = {
        email: email,
        role: 'guest',
        pwd: password,
    };
    const { ok, token } = await LoginWithStatus(user);
    if (ok && token) {
        localStorage.setItem('token', token);
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get('salonId') || params.get('salonid');
        const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : '';
        window.location.assign(`/Salon/GuestInfo${salonQuery}`);
    } else {
        alert('Login failed. Check your email and password.');
    }
});

document.getElementById('owner-login-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const email = form.querySelector('#OwnerLoginEmail').value;
    const password = form.querySelector('#OwnerLoginPassword').value;
    const user = {
        email: email,
        role: 'owner',
        pwd: password,
    };
    const { ok, token } = await LoginWithStatus(user);
    if (ok && token) {
        localStorage.setItem('token', token);
        window.location.href = '/ownerhomepage';
    }
});

document.getElementById('addservice-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const serviceName = form.querySelector('#name').value;
    const durationInMinutes = Number(form.querySelector('#durationInMinute').value);
    const price = Number(form.querySelector('#price').value);
    const description = form.querySelector('#description').value;

    const newService = {
        name: serviceName,
        description: description,
        price: price,
        durationInMinute: durationInMinutes
    };

    const token = localStorage.getItem('token');
    await AddArtistService(newService, token);

})

document.getElementById('guest-register-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector('#Name').value;
    const email = form.querySelector('#Email').value;
    const password = form.querySelector('#Password').value;
    const user = {
        name: name,
        email: email,
        pwd: password,
    };
    const reg = await Register(user);
    console.log(reg);
    const { ok, token } = await LoginWithStatus({
        email: email,
        role: 'guest',
        pwd: password,
    });
    if (ok && token) {
        localStorage.setItem('token', token);
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get('salonId') || params.get('salonid');
        const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : '';
        window.location.assign(`/Salon/GuestInfo${salonQuery}`);
    } else {
        alert('Account may have been created, but automatic login failed. Try signing in.');
    }
});

document.getElementById('owner-register-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector('#OwnerRegName').value;
    const email = form.querySelector('#OwnerRegEmail').value;
    const password = form.querySelector('#OwnerRegPassword').value;
    const user = {
        name: name,
        email: email,
        pwd: password,
    };
    const { ok } = await RegisterOwner(user);
    if (ok) {
        window.location.href = '/maker';
    }
});

async function AddToCart() {
    

    const pathSegments = window.location.pathname.split('/');
    pathSegments.forEach(x => console.log(x))
    const ids = [];
    document.querySelectorAll('#services-list input[type="checkbox"]:checked').forEach((cb) => {
        ids.push(cb.id);
    });
    console.log(ids)
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get("salonId") || params.get("salonid");
    const salonQuery = salonId ? `&salonId=${encodeURIComponent(salonId)}` : "";
    window.location.href = `/Salon/SelectArtist?serviceids=${ids.join(',')}${salonQuery}`;
}

function SelectArtist(){
    const selectedRadio = document.querySelector('.artist-radio-cell input[type="radio"]:checked');
    if (!selectedRadio) return;
    const id = selectedRadio.id;
    const params = new URLSearchParams(window.location.search);
    const serviceids = params.get('serviceids') ?? '';
    const salonId = params.get("salonId") || params.get("salonid");
    const salonQuery = salonId ? `&salonId=${encodeURIComponent(salonId)}` : "";
    window.location.href = `/Salon/DateSelection?serviceids=${encodeURIComponent(serviceids)}&artistid=${encodeURIComponent(id)}${salonQuery}`;
}

async function loadServicesBySalon(id) {
    const services = await GetServices(id)
    services.forEach(x => 
        document.getElementById("services-table").innerHTML += `
                        <tr>
                            <td>${x.name}</td>
                            <td>${x.durationInMinute} min</td>
                            <td>${x.price} Ft</td>
                        </tr>
        `
    )
}




async function loadArtistsBySalonId(id) {
    const artists = await getArtistBySalonId(localStorage.getItem('token'), id)
    const salonIdEnc = encodeURIComponent(id);
    artists.forEach(x => {
        const nameEnc = encodeURIComponent(x.name || 'Artist');
        document.getElementById('salon-dash-artists').innerHTML += `
            <li class="salon-dash-artist-card">
                    <div>
                        <h3 class="salon-dash-card-name">${x.name}</h3>
                        <p class="salon-dash-card-meta">${x.description}</p>
                    </div>
                    <a href="/salondashboard/schedule?salonId=${salonIdEnc}&artistId=${x.id}&artistName=${nameEnc}" class="owner-btn">View schedule</a>
                </li>
        `;
    });
}

async function loadSalons(){
    const list = document.getElementById("owner-salons-list");
    if (!list) return;

    const token = localStorage.getItem('token');
    const salons = await GetMySalons(token);
    list.innerHTML = '';
    salons.forEach(x =>
        list.innerHTML += `
            <li class="owner-salon-card">
                    <div class="owner-salon-main">
                        <h3 class="owner-salon-name">${x.name}</h3>
                        <p class="owner-salon-meta">${x.email}</p>
                        <p class="owner-salon-desc">${x.description}</p>
                    </div>
                    <div class="owner-salon-actions">
                        <a href="/salondashboard?salonId=${x.id}" class="owner-btn">Open dashboard</a>
                        <a href="/ownerartistregister?salonId=${x.id}" class="owner-btn owner-btn-ghost">Edit</a>
                        <button type="button" class="owner-btn owner-btn-danger" data-action="remove-salon" data-salon-id="${x.id}">Remove salon</button>
                    </div>
                </li>
        `
    );
}

async function LoadServices() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get('salonId');
    if (!salonId) {
        console.warn('Add ?salonId= to the URL to load services.');
        return;
    }
    const services = await GetServices(salonId);

    services.forEach(x => 
        document.getElementById("services-list").innerHTML += `
            <div class="service-card">
                <div class="name">
                    ${x.name}
                </div>
                <div class="desc">
                    ${x.description}
                </div>
                <div class="service-checkbox-cell" style="display: flex; justify-content: flex-end; align-items: center;">
                    <input type="checkbox" id="${x.id}">
                </div>

            </div>
        

        `
    )

}

async function LoadUserData() {
    const userData = await GetLoggedInUser(localStorage.getItem("token"))

    document.getElementById("user-profile").innerHTML = `
        <div class="user-info">
                <div class="info-row"><span class="info-label">Name</span><span class="info-value" id="user-name">${userData.name}</span></div>
                <div class="info-row"><span class="info-label">Email</span><span class="info-value" id="user-email">${userData.email}</span></div>
                <div class="info-row"><span class="info-label">Role</span><span class="info-value" id="user-role">Guest</span></div>
            </div>

    `
}

async function LoadArtistData() {
    const userData = await getLoggedInArtist(localStorage.getItem("token"))

    document.getElementById("artist-profile").innerHTML = `
        <div class="user-info">
                <div class="info-row"><span class="info-label">Name</span><span class="info-value" id="user-name">${userData.name}</span></div>
                <div class="info-row"><span class="info-label">Email</span><span class="info-value" id="user-email">${userData.email}</span></div>
                <div class="info-row"><span class="info-label">Role</span><span class="info-value" id="user-role">Guest</span></div>
            </div>

    `
}


async function LoadOwnerData() {
    const el = document.getElementById("owner-profile");
    if (!el) return;
    const userData = await GetLoggedInOwner(localStorage.getItem("token"));

    el.innerHTML = `
        <div class="user-info">
                <div class="info-row"><span class="info-label">Name</span><span class="info-value">${userData.name}</span></div>
                <div class="info-row"><span class="info-label">Email</span><span class="info-value">${userData.email}</span></div>
                <div class="info-row"><span class="info-label">Role</span><span class="info-value">Owner</span></div>
            </div>
    `;
}

async function LoadArtists() {
    const artists = await GetArtists(localStorage.getItem("token"))

    artists.forEach(x => 
        document.getElementById("artists-list").innerHTML += `
             <div class="artist-card"><div class="name">${x.name}</div><div class="role">${x.description}</div>
             <div class="artist-radio-cell" style="display: flex; justify-content: flex-end; align-items: center;">
                    <input type="radio" name="artist" id="${x.id}">
                </div>
             </div>
             
        `
    )
}

async function loadServicesForArtists(){
    const services = await getServicesByArtist(localStorage.getItem('token'))
    services.forEach(x => 
        document.getElementById("service-card").innerHTML += `
            <div class="name">${x.name}</div>
            <div class="desc">${x.durationInMinute}</div>
        `
    )
}

function scheduleGetMondayOfWeek(d) {
    const date = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    const dow = date.getDay();
    const delta = dow === 0 ? -6 : 1 - dow;
    date.setDate(date.getDate() + delta);
    return date;
}

function scheduleFormatIsoLocal(d) {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
}

function scheduleParseYmd(iso) {
    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso || '');
    if (!m) return null;
    return new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]), 12, 0, 0, 0);
}

function renderOwnerScheduleGrid(mount, data) {
    if (!mount || !data || !data.days || !data.hours) return;
    const grid = document.createElement('div');
    grid.className = 'schedule-grid';

    const corner = document.createElement('div');
    corner.className = 'schedule-grid-corner';
    corner.setAttribute('aria-hidden', 'true');
    grid.appendChild(corner);

    for (const day of data.days) {
        const head = document.createElement('div');
        head.className = 'schedule-grid-dayhead';
        head.textContent = day.label;
        grid.appendChild(head);
    }

    const hours = data.hours;
    const days = data.days;
    for (const hour of hours) {
        const timeEl = document.createElement('div');
        timeEl.className = 'schedule-grid-time';
        timeEl.textContent = `${hour}:00`;
        grid.appendChild(timeEl);

        for (const day of days) {
            const cell = day.cells.find((c) => c.hour === hour);
            const status = cell ? cell.status : 'closed';
            const cellWrap = document.createElement('div');
            cellWrap.className = 'schedule-grid-cell';

            if (status === 'closed') {
                cellWrap.classList.add('schedule-cell-muted');
                cellWrap.textContent = '—';
            } else if (status === 'booked') {
                const pill = document.createElement('span');
                pill.className = 'schedule-cell-pill schedule-pill-booked';
                pill.textContent = (cell && cell.detail) ? cell.detail : 'Booked';
                cellWrap.appendChild(pill);
            } else {
                const pill = document.createElement('span');
                pill.className = 'schedule-cell-pill schedule-pill-open';
                pill.textContent = 'Open';
                cellWrap.appendChild(pill);
            }
            grid.appendChild(cellWrap);
        }
    }

    mount.innerHTML = '';
    mount.appendChild(grid);
}

function wireOwnerScheduleWeekNav(salonId, artistId, artistName, currentMonday) {
    const prev = document.getElementById('schedule-nav-prev');
    const next = document.getElementById('schedule-nav-next');
    const go = (deltaWeeks) => {
        const d = new Date(currentMonday.getFullYear(), currentMonday.getMonth(), currentMonday.getDate());
        d.setDate(d.getDate() + 7 * deltaWeeks);
        const qs = new URLSearchParams({
            salonId: String(salonId),
            artistId: String(artistId),
            weekStart: scheduleFormatIsoLocal(d)
        });
        if (artistName) qs.set('artistName', artistName);
        window.location.assign(`/salondashboard/schedule?${qs.toString()}`);
    };
    if (prev) {
        prev.disabled = false;
        prev.removeAttribute('title');
        prev.addEventListener('click', () => go(-1));
    }
    if (next) {
        next.disabled = false;
        next.removeAttribute('title');
        next.addEventListener('click', () => go(1));
    }
}

async function loadOwnerArtistSchedulePage() {
    const params = new URLSearchParams(window.location.search);
    const salonId = params.get('salonId');
    const artistId = params.get('artistId');
    const rawName = params.get('artistName') || '';
    let artistName = rawName;
    try {
        if (rawName) artistName = decodeURIComponent(rawName);
    } catch (_) { /* keep encoded */ }
    const weekStartParam = params.get('weekStart');

    const mount = document.getElementById('schedule-grid-mount');

    const scheduleBack = document.getElementById('schedule-back');
    if (scheduleBack && salonId) {
        scheduleBack.href = `/salondashboard?salonId=${encodeURIComponent(salonId)}`;
    }

    const labelEl = document.querySelector('.salon-schedule-header .salon-schedule-label');
    if (labelEl) labelEl.textContent = 'Weekly availability';

    const artistEl = document.getElementById('schedule-artist-label');
    if (artistEl) artistEl.textContent = artistName || 'Artist';

    if (!salonId || !artistId) {
        if (mount) mount.innerHTML = '<p class="subtitle" role="alert">Missing salon or artist in the URL.</p>';
        return;
    }

    let monday;
    const parsedWeek = weekStartParam ? scheduleParseYmd(weekStartParam) : null;
    if (parsedWeek) {
        monday = scheduleGetMondayOfWeek(parsedWeek);
    } else {
        monday = scheduleGetMondayOfWeek(new Date());
    }

    const weekLabel = document.getElementById('schedule-week-label');
    if (weekLabel) {
        weekLabel.textContent = `Week of ${monday.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })}`;
    }

    if (mount) mount.innerHTML = '<p class="subtitle">Loading schedule…</p>';

    const token = localStorage.getItem('token');
    const weekStartIso = scheduleFormatIsoLocal(monday);

    try {
        const data = await GetOwnerArtistWeekSchedule(token, salonId, artistId, weekStartIso);
        renderOwnerScheduleGrid(mount, data);
        wireOwnerScheduleWeekNav(salonId, artistId, artistName, monday);
    } catch (err) {
        console.error(err);
        if (mount) {
            const msg = err && err.message ? err.message : 'Could not load schedule.';
            mount.innerHTML = `<p class="subtitle" role="alert">${msg}</p>`;
        }
    }
}

