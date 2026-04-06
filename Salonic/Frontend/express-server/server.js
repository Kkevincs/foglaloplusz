const express = require("express");
const path = require("path");

const app = express();
const PORT = 67;

app.use(express.static(path.join(__dirname, "public")));

function requireSalonId(req, res, next) {
  const rawSalonId = req.query.salonId ?? req.query.salonid;
  const normalizedSalonId = String(rawSalonId ?? "").trim();

  if (!normalizedSalonId) {
    return res.redirect("/ownerregister");
  }

  // Normalize query key to `salonId` on every /Salon route.
  if (!req.query.salonId || req.query.salonid) {
    const normalizedQuery = new URLSearchParams(req.query);
    normalizedQuery.set("salonId", normalizedSalonId);
    normalizedQuery.delete("salonid");
    return res.redirect(`${req.path}?${normalizedQuery.toString()}`);
  }

  next();
}

app.use("/Salon", requireSalonId);

// ─── Routes ───────────────────────────────────────────────────────────────────

// Landing: choose salon guest / artist / owner
app.get("/portal", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "portalHome.html"));
});

app.get("", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "ownerRegister.html"));
});
app.get("/maker", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "forms.html"));
});
//Salon Home Page
app.get("/Salon", (req, res) => {
  const rawSalonId = String(req.query.salonId ?? req.query.salonid ?? "");
  const artistSuffix = "/Artist";
  if (rawSalonId.endsWith(artistSuffix)) {
    const normalizedSalonId = rawSalonId.slice(0, -artistSuffix.length);
    return res.redirect(`/Salon/Artist?salonId=${encodeURIComponent(normalizedSalonId)}`);
  }

  const salonId = parseInt(rawSalonId, 10);
  res.sendFile(path.join(__dirname, "views", "HomePage.html"));
});
//Artist Home Page
app.get("/Salon/Artist", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistHome.html"));
});

//guest login
app.get("/Salon/Login", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "login.html"));
});
//artist login
app.get("/Salon/Artist/Login", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistLogin.html"));
});

// Not logged in — prompt to log in (guest / artist)
app.get("/Salon/UnloggedGuest", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "unloggedGuest.html"));
});
app.get("/Salon/Artist/UnloggedArtist", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "unloggedArtist.html"));
});

//guest register
app.get("/Salon/Register", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "register.html"));
});

// salon owner register
app.get("/ownerregister", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "ownerRegister.html"));
});
// salon owner login
app.get("/ownerlogin", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "ownerLogin.html"));
});
// salon owner dashboard (template)
app.get("/ownerhomepage", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "ownerHomePage.html"));
});
// salon owner profile
app.get("/owneruserpage", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "ownerUserPage.html"));
});
// owner artist registration template
app.get("/ownerartistregister", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "ownerArtistRegister.html"));
});
// salon dashboard (owner template)
app.get("/salondashboard", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "salonDashboard.html"));
});
// salon dashboard — artist schedule (template only)
app.get("/salondashboard/schedule", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "salonSchedule.html"));
});

//Salon services
app.get('/Salon/Services', (req, res) => {


  res.sendFile(path.join(__dirname, "views", "services.html"));
});
//Artist services
app.get('/Salon/Artist/Services', (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistServices.html"));
});
//Salon artists
app.get("/Salon/Artists", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artists.html"));
});
//Add service
app.get("/Salon/Artist/AddService", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "addservice.html"));
});
//Artist user
app.get("/Salon/Artist/User", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistUser.html"));
});
//Artist working hours
app.get("/Salon/Artist/SetWorkingHours", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "setWorkingHours.html"));
});
//Artist hours page (template)
app.get("/Salon/Artist/Hours", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistHours.html"));
});
//Date picker
app.get("/Salon/DateSelection", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "selectAppointment.html"));
});
//Guest data
app.get("/Salon/GuestInfo", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "user.html"));
});
//Select Artist
app.get("/Salon/SelectArtist", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "selectArtist.html"));
});

//Appointment created
app.get("/Salon/AppointmentCreated", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "HomePage.html"));
});

/** Explicit URL for client redirects when e.g. salonId is invalid. */
app.get("/404", (req, res) => {
  res.status(404).sendFile(path.join(__dirname, "views", "404.html"));
});

// 404
app.use((req, res) => {
  res.status(404).sendFile(path.join(__dirname, "views", "404.html"));
});

// ─── Start ────────────────────────────────────────────────────────────────────
app.listen(PORT, () => {
  console.log(`Server running at http://localhost:${PORT}`);
});