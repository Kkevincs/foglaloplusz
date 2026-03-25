document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');

    if (!token) return;

    if (document.getElementById('services-list')) LoadServices();
    if (document.getElementById('user-profile')) LoadUserData();
    if (document.getElementById('artists-list')) LoadArtists();
});
document.querySelectorAll('.nav-links a').forEach(a => {
    a.addEventListener('click', e => {
        if (a.getAttribute('href') === '#') e.preventDefault();
    });
});

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
    await GetById(localStorage.getItem("token"))
});

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
async function loadCalendar() {
   
}
async function LoadServices(){
    const services = await GetServices(localStorage.getItem("token"));

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

async function Loadappointments(params) {
    const apps = await getAppointmentByGuestId(localStorage.getItem("token"));
    
   
}
