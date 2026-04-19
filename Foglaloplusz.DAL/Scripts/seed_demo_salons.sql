-- Demo seed: owners, salons (nail / barber / hair), catalog + artist services, offerings,
-- working hours, guests, and sample appointments.
-- Target: PostgreSQL after EF migrations (Foglaloplusz.DAL).
--
-- WARNING: TRUNCATE wipes the listed tables. Run only on a dev database.
-- Login passwords are plain text (same as the app's current auth): Demo123!

BEGIN;

TRUNCATE TABLE
  "AppointmentServices",
  "Appointments",
  "Offerings",
  "WorkingHours",
  "Services",
  "Artists",
  "Salons",
  "Guests",
  "Owners"
RESTART IDENTITY CASCADE;

INSERT INTO "Owners" ("Id", "Name", "Email", "Role", "Pwd")
VALUES
  (1, N'Kovács Eszter', 'eszter.kovacs@demo.local', 'Owner', 'Demo123!'),
  (2, N'Nagy Dániel', 'daniel.nagy@demo.local', 'Owner', 'Demo123!'),
  (3, N'Szabó Anna', 'anna.szabo@demo.local', 'Owner', 'Demo123!');

-- Theme values match public/styles.css + ownerSalonEdit.html (body class):
--   theme-pink | theme-sky-blue | theme-ruby | theme-forest-green | theme-amethyst
INSERT INTO "Salons" ("Id", "Name", "Email", "Description", "Location", "OwnerId", "Theme", "LogoUrl")
VALUES
  (1, N'Polished Studio – Körömszalon',
      'hello@polishedstudio.demo',
      N'Minőségi manikűr, gél lakk és pedikűr belvárosi stúdióban.',
      N'Budapest, Király utca 42.',
      1, 'theme-pink', ''),
  (2, N'Blade & Co. Barber Shop',
      'bookings@bladeandco.demo',
      N'Klasszikus és modern férfi hajvágás, szakáll formázás, hot towel.',
      N'Budapest, Bartók Béla út 18.',
      2, 'theme-ruby', ''),
  (3, N'Curl & Colour Hair Atelier',
      'info@curlcolour.demo',
      N'Női hajvágás, melír, balayage és alkalmi kontyok.',
      N'Budapest, Ráday utca 7.',
      3, 'theme-sky-blue', '');

-- Artists: two per salon
INSERT INTO "Artists" ("Id", "Name", "Email", "Pwd", "Description", "SalonId", "Role")
VALUES
  (1, N'Farkas Lili', 'lili.farkas@demo.local', 'Demo123!',
      N'Gél lakk és nail art specialista.', 1, 'Artist'),
  (2, N'Horváth Petra', 'petra.horvath@demo.local', 'Demo123!',
      N'Pedikűr és körömépítés.', 1, 'Artist'),
  (3, N'Tóth Márk', 'mark.toth@demo.local', 'Demo123!',
      N'Fade, klasszikus hajvágás.', 2, 'Artist'),
  (4, N'Kiss Ádám', 'adam.kiss@demo.local', 'Demo123!',
      N'Szakáll design és borotválás.', 2, 'Artist'),
  (5, N'Varga Réka', 'reka.varga@demo.local', 'Demo123!',
      N'Melír, balayage, hosszú haj.', 3, 'Artist'),
  (6, N'Molnár Zsófi', 'zsofi.molnar@demo.local', 'Demo123!',
      N'Alkalmi frizurák és kontyok.', 3, 'Artist');

INSERT INTO "Guests" ("Id", "Name", "Email", "Pwd", "Role")
VALUES
  (1, N'Benkő Csilla', 'csilla.benko@demo.local', 'Demo123!', 'Guest'),
  (2, N'Lakatos Gergő', 'gergo.lakatos@demo.local', 'Demo123!', 'Guest'),
  (3, N'Papp Norbert', 'norbert.papp@demo.local', 'Demo123!', 'Guest'),
  (4, N'Juhász Emese', 'emese.juhasz@demo.local', 'Demo123!', 'Guest'),
  (5, N'Fehér Roland', 'roland.feher@demo.local', 'Demo123!', 'Guest'),
  (6, N'Takács Bianka', 'bianka.takacs@demo.local', 'Demo123!', 'Guest'),
  (7, N'Simon Dávid', 'david.simon@demo.local', 'Demo123!', 'Guest'),
  (8, N'Rácz Noémi', 'noemi.racz@demo.local', 'Demo123!', 'Guest');

-- Services: owner catalog (OwnerId set, ArtistId null) and artist-owned (ArtistId set, OwnerId null).
-- Ids are fixed for offerings / appointments below.
INSERT INTO "Services" ("Id", "Name", "Description", "Price", "DurationInMinute", "SalonId", "OwnerId", "ArtistId")
VALUES
  -- Salon 1 – owner catalog (nail)
  (1, N'Manikűr + gél lakk', N'Körömreszelés, bőrápolás, egyszínű gél lakk.', 6500, 50, 1, 1, NULL),
  (2, N'Gél lakk eltávolítás + újrafestés', N'Leragasztás, előkészítés, új szín.', 7200, 60, 1, 1, NULL),
  (3, N'Wellness pedikűr', N'Áztatás, bőrkeményedés, reszelés, hidratálás.', 8900, 65, 1, 1, NULL),
  (4, N'Körömépítés utántöltés', N'Zselé / porcelán töltés forma szerint.', 12500, 90, 1, 1, NULL),
  -- Salon 1 – artist-only
  (5, N'Mini nail art (2 ujj)', N'Egyszerű minta vagy matrica 2 körmön.', 2800, 25, 1, NULL, 1),
  (6, N'Paraffin kézápolás', N'Meleg paraffin és kézkrém.', 2200, 20, 1, NULL, 2),

  -- Salon 2 – owner catalog (barber)
  (7, N'Hajmosás + szárítás', N'Frissítő mosás, gépi / kézi szárítás.', 3200, 30, 2, 2, NULL),
  (8, N'Szakáll formázás + kontúr', N'Olló, gépi vonal, borotva kontúr.', 3800, 25, 2, 2, NULL),
  (9, N'Hajvágás + styling', N'Konzultáció, mosás, vágás, wax / pomádé.', 5800, 45, 2, 2, NULL),
  (10, N'Gyerek hajvágás (12 év alatt)', N'Gyors, barátságos hajvágás.', 3600, 30, 2, 2, NULL),
  -- Salon 2 – artist-only
  (11, N'Nullás gépi hajvágás', N'Egy hosszúság géppel, kontúrral.', 4500, 35, 2, NULL, 3),
  (12, N'Hot towel + arcszesszió', N'Meleg törölköző, hab, hidratáló.', 2900, 20, 2, NULL, 4),

  -- Salon 3 – owner catalog (hair)
  (13, N'Mosás + blow-dry', N'Hajmosás, balzsam, kerekkefe / diffúzor.', 5200, 45, 3, 3, NULL),
  (14, N'Részleges melír fóliával', N'Csomószám egyeztetés szerint, tonerrel.', 19500, 120, 3, 3, NULL),
  (15, N'Balayage frissítés', N'Kézi festés, olvasztás, ápoló pakolás.', 24500, 150, 3, 3, NULL),
  -- Salon 3 – artist-only
  (16, N'Pixie / rövid hajvágás', N'Női rövid forma, textúra.', 6800, 40, 3, NULL, 5),
  (17, N'Alkalmi konty / feltűzés', N'Próba nélküli egyszerű alkalmi konty.', 9800, 75, 3, NULL, 6);

-- Offerings: every artist-owned service has IsImported = false.
-- Imported rows link artists to owner catalog services they “sell” (IsImported = true).
INSERT INTO "Offerings" ("ServiceId", "ArtistId", "IsImported")
VALUES
  -- Salon 1 / artist 1: catalog 1,2 + own 5
  (1, 1, TRUE), (2, 1, TRUE), (5, 1, FALSE),
  -- Salon 1 / artist 2: catalog 3,4 + own 6
  (3, 2, TRUE), (4, 2, TRUE), (6, 2, FALSE),
  -- Salon 2 / artist 3: catalog 7,9,10 + own 11
  (7, 3, TRUE), (9, 3, TRUE), (10, 3, TRUE), (11, 3, FALSE),
  -- Salon 2 / artist 4: catalog 8,9 + own 12
  (8, 4, TRUE), (9, 4, TRUE), (12, 4, FALSE),
  -- Salon 3 / artist 5: catalog 13,14 + own 16
  (13, 5, TRUE), (14, 5, TRUE), (16, 5, FALSE),
  -- Salon 3 / artist 6: catalog 13,15 + own 17
  (13, 6, TRUE), (15, 6, TRUE), (17, 6, FALSE);

-- Working hours: Mon 2026-04-20 – Fri 2026-04-24 (UTC calendar days). 09:00–18:00 salon time as interval-from-midnight.
INSERT INTO "WorkingHours" ("ArtistId", "Date", "StartTime", "EndTime")
SELECT a."Id", d.day::timestamptz, INTERVAL '9 hours', INTERVAL '18 hours'
FROM "Artists" a
CROSS JOIN (
  VALUES
    ('2026-04-20T00:00:00Z'::timestamptz),
    ('2026-04-21T00:00:00Z'::timestamptz),
    ('2026-04-22T00:00:00Z'::timestamptz),
    ('2026-04-23T00:00:00Z'::timestamptz),
    ('2026-04-24T00:00:00Z'::timestamptz)
) AS d(day);

-- Appointments: EndTime = StartTime + sum(DurationInMinute) + 15 minutes (see AppointmentService.BlockBufferAfterServices).
INSERT INTO "Appointments" ("Id", "GuestId", "ArtistId", "SalonId", "StartTime", "EndTime", "Notes")
VALUES
  (1, 1, 1, 1, '2026-04-21T08:00:00Z', '2026-04-21T09:05:00Z', N'Első időpont, klasszikus piros lakk.'),
  (2, 2, 1, 1, '2026-04-21T10:30:00Z', '2026-04-21T11:10:00Z', N'Két ujj minta + meglévő gél lakk.'),
  (3, 3, 3, 2, '2026-04-21T09:00:00Z', '2026-04-21T10:00:00Z', N'Hajvágás fade-del.'),
  (4, 4, 5, 3, '2026-04-22T07:00:00Z', '2026-04-22T09:15:00Z', N'Melír + toner – hosszú haj.'),
  (5, 5, 6, 3, '2026-04-22T11:00:00Z', '2026-04-22T12:30:00Z', N'Esküvő próba konty.'),
  (6, 6, 2, 1, '2026-04-23T12:00:00Z', '2026-04-23T13:45:00Z', N'Körömépítés töltés.'),
  (7, 7, 4, 2, '2026-04-23T08:30:00Z', '2026-04-23T09:30:00Z', N'Szakáll + hot towel csomag.');

INSERT INTO "AppointmentServices" ("AppointmentsId", "ServicesId")
VALUES
  (1, 1),   -- manikűr 50 + 15
  (2, 5),   -- mini nail art 25 + 15
  (3, 9),   -- hajvágás 45 + 15
  (4, 14),  -- melír 120 + 15
  (5, 17),  -- konty 75 + 15
  (6, 4),   -- töltés 90 + 15
  (7, 8), (7, 12);  -- szakáll 25 + hot towel 20 + 15 = 60

SELECT setval(pg_get_serial_sequence('"Owners"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Owners"));
SELECT setval(pg_get_serial_sequence('"Salons"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Salons"));
SELECT setval(pg_get_serial_sequence('"Artists"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Artists"));
SELECT setval(pg_get_serial_sequence('"Guests"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Guests"));
SELECT setval(pg_get_serial_sequence('"Services"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Services"));
SELECT setval(pg_get_serial_sequence('"Offerings"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Offerings"));
SELECT setval(pg_get_serial_sequence('"WorkingHours"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "WorkingHours"));
SELECT setval(pg_get_serial_sequence('"Appointments"', 'Id'), (SELECT COALESCE(MAX("Id"), 1) FROM "Appointments"));

COMMIT;
