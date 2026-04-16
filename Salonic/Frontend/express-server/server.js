const express = require("express");
const path = require("path");

const app = express();
const PORT = 67;

app.use(express.static(path.join(__dirname, "public")));

const appointmentCreatedView = path.join(__dirname, "views", "appointmentCreated.html");

function requireSalonId(req, res, next) {
  const rawSalonId = req.query.salonId ?? req.query.salonid;
  const normalizedSalonId = String(rawSalonId ?? "").trim();
  const hasPendingGuestBooking =
    String(req.query.appointmentDate ?? "").trim() &&
    String(req.query.startTime ?? "").trim();

  if (!normalizedSalonId) {
    // Guest register after "book without login" — keep query (serviceids, times, etc.)
    // Mounted at /Salon: req.path is "/Register", not "/Salon/Register".
    const isGuestRegister =
      (req.path === "/Register" ||
        String(req.originalUrl || "").split("?")[0].endsWith("/Salon/Register")) &&
      hasPendingGuestBooking;
    if (isGuestRegister) {
      return next();
    }
    return res.redirect("/ownerregister");
  }

  // Normalize query key to `salonId` on every /Salon route.
  if (!req.query.salonId || req.query.salonid) {
    const normalizedQuery = new URLSearchParams(req.query);
    normalizedQuery.set("salonId", normalizedSalonId);
    normalizedQuery.delete("salonid");
    const pathname =
      String(req.originalUrl || "").split("?")[0] ||
      `${req.baseUrl || ""}${req.path}`;
    return res.redirect(`${pathname}?${normalizedQuery.toString()}`);
  }

  next();
}

app.get("/Salon/AppointmentCreated", requireSalonId, (req, res) => {
  res.sendFile(appointmentCreatedView);
});

const salonRouter = express.Router();
salonRouter.use(requireSalonId);

// Salon home page
salonRouter.get("/", (req, res) => {
  const rawSalonId = String(req.query.salonId ?? req.query.salonid ?? "");
  const artistSuffix = "/Artist";
  if (rawSalonId.endsWith(artistSuffix)) {
    const normalizedSalonId = rawSalonId.slice(0, -artistSuffix.length);
    return res.redirect(
      `/Salon/Artist?salonId=${encodeURIComponent(normalizedSalonId)}`
    );
  }

  const salonId = parseInt(rawSalonId, 10);
  res.sendFile(path.join(__dirname, "views", "HomePage.html"));
});

salonRouter.get("/Artist", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistHome.html"));
});

salonRouter.get("/Login", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "login.html"));
});

salonRouter.get("/Artist/Login", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistLogin.html"));
});

salonRouter.get("/UnloggedGuest", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "unloggedGuest.html"));
});

salonRouter.get("/Artist/UnloggedArtist", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "unloggedArtist.html"));
});

salonRouter.get("/Register", (req, res) => {
  const salonId = parseInt(req.query.salonid, 10);
  res.sendFile(path.join(__dirname, "views", "register.html"));
});

salonRouter.get("/Services", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "services.html"));
});

salonRouter.get("/Artist/Services", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistServices.html"));
});

salonRouter.get("/Artists", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artists.html"));
});

salonRouter.get("/Artist/AddService", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "addservice.html"));
});

salonRouter.get("/Artist/User", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistUser.html"));
});

salonRouter.get("/Artist/SetWorkingHours", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "setWorkingHours.html"));
});

salonRouter.get("/Artist/Hours", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "artistHours.html"));
});

salonRouter.get("/DateSelection", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "selectAppointment.html"));
});

salonRouter.get("/GuestInfo", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "user.html"));
});

salonRouter.get("/SelectArtist", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "selectArtist.html"));
});

app.use("/Salon", salonRouter);

// ─── Routes ───────────────────────────────────────────────────────────────────

// Landing: choose salon guest / artist / owner
app.get("/portal", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "portalHome.html"));
});

// Kezdőlap: portál (vendég / szakember / tulajdonos választás)
app.get("/", (req, res) => {
  res.redirect(302, "/portal");
});
app.get("/maker", (req, res) => {
  res.sendFile(path.join(__dirname, "views", "forms.html"));
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
