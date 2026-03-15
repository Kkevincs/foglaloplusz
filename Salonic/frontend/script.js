document.addEventListener("DOMContentLoaded", () => {
    LoadServices();

})

document.querySelectorAll('.nav-links a').forEach(a => {
    a.addEventListener('click', e => {
        if (a.getAttribute('href') === '#') e.preventDefault();
    });
});

document.getElementById('login-form')?.addEventListener('submit', (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector('#Name').value;
    const password = form.querySelector('#Password').value;
    const role = form.querySelector('input[name="role"]:checked')?.value;
    const user = { name, role, pwd: password };
    login(user)
});

document.getElementById('register-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector('#Name').value;
    const email = form.querySelector('#Email').value;
    const password = form.querySelector('#Password').value;
    const role = form.querySelector('input[name="role"]:checked')?.value;
    const user = { name, role, pwd: password };
    Register(user)
});

async function LoadServices(){
    const services = await GetServices()
    services.forEach(x => 
        document.getElementById("services-list").innerHTML += `
            <div class="service-card"><div class="name">${x.name}</div><div class="desc">${x.description}</div></div>
        `
        
    )
}


