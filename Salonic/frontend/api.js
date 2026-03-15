async function login(user){
    const response = await fetch("https://localhost:7156/api/authentication/authenticate", {
        method : "POST",
        headers: {
            "content-type" : "application/json"
        },
        body: JSON.stringify(user)
    })
    const token = await response.text();
    localStorage.setItem("Token" , token);
    console.log(token)

}

async function Register(User){
    const response = await fetch("https://localhost:7156/api/register", {
        method : "POST",
        headers: {
            "content-type" : "application/json"
        },
        body: JSON.stringify(User)
    })
    console.log(await response.json());

}

async function GetServices(){
    const response = await fetch("https://localhost:7156/api/service")
    const data = await response.json();
    return data;
}