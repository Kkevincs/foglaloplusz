const artistNavLinks = [
    ["logoLink", "/Salon/Artist"],
    ["homeLink", "/Salon/Artist"],
    ["servicesLink", "/Salon/Artist/Services"],
    ["userLink", "/Salon/Artist/User"],
    ["addServiceLink", "/Salon/Artist/AddService"],
    ["workingHoursLink", "/Salon/Artist/SetWorkingHours"],
    ["loginLink", "/Salon/Artist/Login"]
];

artistNavLinks.forEach(([elementId, href]) => {
    const el = document.getElementById(elementId);
    if (el) el.href = href;
});
