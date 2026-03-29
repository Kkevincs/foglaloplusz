document.addEventListener('DOMContentLoaded', () => {
    const ownerOnlyPaths = ["/maker", "/owneruserpage", "/salondashboard"];
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

    if (window.location.pathname === '/ownerhomepage') {
        loadSalons();
    }
    if (window.location.pathname === '/salondashboard' ) {
        const params = new URLSearchParams(window.location.search);
        const salonId = params.get('salonId');
        loadArtistsBySalonId(salonId)
        loadServices(salonId)
    }
    if (document.getElementById('services-list')) LoadServices();
    if (document.getElementById('user-profile')) LoadUserData();
    if (document.getElementById('artists-list')) LoadArtists();
    if (document.getElementById('artist-profile')) LoadArtistData();
    if (document.getElementById('owner-profile')) LoadOwnerData();
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
    token = await Login(artist);
    localStorage.setItem("token", token);

})

document.getElementById('salon-setup-form')?.addEventListener('submit',async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector("#salon-name").value
    const logo = "notimplementedyet"
    const description = form.querySelector("#salon-description").value
    const theme = form.querySelector('#salon-theme').value
    const email = form.querySelector('#support-email').value

    const newSalon = 
    {
        name, email, theme, logo, description
    }
    await AddSalon(localStorage.getItem('token'), newSalon)
})



document.getElementById('guest-login-form')?.addEventListener('submit',async (e) => {
    e.preventDefault();
    const form = e.target;
    const email = form.querySelector('#Email').value;
    const password = form.querySelector('#Password').value;
    const user = {
        email : email,
        role : "guest",
        pwd : password
    };
    const token = await Login(user);
    localStorage.setItem("token", token);
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
        window.location.href = '/maker';
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
        name : name,
        email : email,
        pwd : password,
    };
   const response = await Register(user);
   console.log(await response);
   const autoLoginGuest =
   {
        email: email,
        role: "guest",
        pwd: password
   }
   await Login(autoLoginGuest)


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
    const currentUrl = window.location.href;
    window.location.href = `/Salon/SelectArtist?serviceids=${ids.join(',')}`;
}

function SelectArtist(){
    const selectedRadio = document.querySelector('.artist-radio-cell input[type="radio"]:checked');
    if (!selectedRadio) return;
    const id = selectedRadio.id;
    const params = new URLSearchParams(window.location.search);
    const serviceids = params.get('serviceids') ?? '';
    window.location.href = `/Salon/DateSelection?serviceids=${encodeURIComponent(serviceids)}&artistid=${encodeURIComponent(id)}`;
}

async function loadServices(id) {
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
    artists.forEach(x => 
        document.getElementById('salon-dash-artists').innerHTML += `
            <li class="salon-dash-artist-card">
                    <div>
                        <h3 class="salon-dash-card-name">${x.name}</h3>
                        <p class="salon-dash-card-meta">${x.description}</p>
                    </div>
                    <button type="button" class="owner-btn" disabled>View schedule</button>
                </li>
        `
    )
}

async function loadSalons(){
    const salons = await GetMySalons(localStorage.getItem('token'))
    salons.forEach(x => 
        document.getElementById("owner-salons-list").innerHTML += `
            <li class="owner-salon-card">
                    <div class="owner-salon-main">
                        <h3 class="owner-salon-name">${x.name}</h3>
                        <p class="owner-salon-meta">${x.email}</p>
                        <p class="owner-salon-desc">${x.description}</p>
                    </div>
                    <div class="owner-salon-actions">
                        <a href="/salondashboard?salonId=${x.id}" class="owner-btn">Open dashboard</a>
                        <a href="/maker" class="owner-btn owner-btn-ghost">Edit</a>
                    </div>
                </li>
        `
    )
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

async function loadAppointments(salonId, serviceIds) {
    
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

