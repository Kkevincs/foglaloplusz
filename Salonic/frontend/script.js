document.querySelectorAll('.nav-links a').forEach(a => {
    a.addEventListener('click', e => {
        if (a.getAttribute('href') === '#') e.preventDefault();
    });
});

document.getElementById('login-form')?.addEventListener('submit',async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector('#Name').value;
    const password = form.querySelector('#Password').value;
    const role = form.querySelector('input[name="role"]:checked')?.value;
    const user = { name, role, pwd: password };
   const token = await Login(user);
   localStorage.setItem("token", token);
    await GetById(localStorage.getItem("token"))
});

document.getElementById('register-form')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    const name = form.querySelector('#Name').value;
    const email = form.querySelector('#Email').value;
    const password = form.querySelector('#Password').value;
    const role = form.querySelector('input[name="role"]:checked')?.value;
    const user = { name, role, pwd: password };
   await Register(user);

});
