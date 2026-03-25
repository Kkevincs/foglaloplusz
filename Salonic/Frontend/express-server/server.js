const express = require("express");
const path = require("path");

const app = express();
const PORT = 67;

app.use(express.static(path.join(__dirname, "public")));

// ─── Routes ───────────────────────────────────────────────────────────────────
// Salon Maker Form
app.get("/Maker", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "HomePage.html"));
});
//Salon Home Page
app.get("/Salon", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "HomePage.html"));
});

//guest login
app.get("/Salon/Login", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "login.html"));
});

//guest register
app.get("/Salon/Register", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "register.html"));
});


//Salon services
app.get('/Salon/Services', (req, res) => {
  const serviceIds = req.query.serviceid
    ?.split(',')
    .map(id => parseInt(id, 10)); // [1, 2, 3]


  res.sendFile(path.join(__dirname, "views", "services.html"));
});
//Salon artists
app.get("/Salon/Artists", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artists.html"));
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