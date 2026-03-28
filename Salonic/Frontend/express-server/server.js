const express = require("express");
const path = require("path");

const app = express();
const PORT = 67;

app.use(express.static(path.join(__dirname, "public")));

// ─── Routes ───────────────────────────────────────────────────────────────────
// Salon maker (customize salon landing / branding form)
app.get("/maker", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "forms.html"));
});
//Salon Home Page
app.get("/Salon", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
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

// 404
app.use((req, res) => {
  res.status(404).sendFile(path.join(__dirname, "views", "404.html"));
});

// ─── Start ────────────────────────────────────────────────────────────────────
app.listen(PORT, () => {
  console.log(`Server running at http://localhost:${PORT}`);
});