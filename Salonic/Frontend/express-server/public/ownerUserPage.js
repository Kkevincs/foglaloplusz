document.addEventListener("DOMContentLoaded", () => {
    if (!enforceOwnerRoleOrRedirect()) return;
    if (document.getElementById("owner-profile")) LoadOwnerData();
});
