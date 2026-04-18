let selectedDate = "";
/** ISO start time from the API slot (DateTime), not display text */
let selectedSlotStartIso = "";
let selectedArtistId = 0;
let selectedServiceIds = [];
let selectedSalonId = 0;

/** Nézet hónapja (naptár) */
let calendarViewYear = 0;
let calendarViewMonth = 0;

function appointmentFormatLocalYMD(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
}

/** Hétfő = első oszlop (0–6) */
function mondayOffset(d) {
    const dow = d.getDay();
    return dow === 0 ? 6 : dow - 1;
}

function formatSlotTimeLabel(iso) {
    if (!iso) return "";
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return "";
    return d.toLocaleTimeString("hu-HU", { hour: "2-digit", minute: "2-digit" });
}

function syncAppointmentBar() {
    const dateEl = document.getElementById("appointment-bar-date");
    const timeEl = document.getElementById("appointment-bar-time");
    const btn = document.getElementById("appointment-save-btn");
    if (!timeEl || !btn) return;

    if (dateEl && selectedDate) {
        const d = new Date(`${selectedDate}T12:00:00`);
        dateEl.textContent = d.toLocaleDateString("hu-HU", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric",
        });
    } else if (dateEl) {
        dateEl.textContent = "";
    }

    if (selectedSlotStartIso) {
        timeEl.textContent = formatSlotTimeLabel(selectedSlotStartIso);
        timeEl.classList.remove("appointment-bar__time--placeholder");
    } else {
        timeEl.textContent = "Válassz időpontot a folytatáshoz.";
        timeEl.classList.add("appointment-bar__time--placeholder");
    }

    btn.disabled = !selectedSlotStartIso;
}

function sortSlotsByStartTime(stamps) {
    return [...stamps].sort((a, b) => {
        const ta = new Date(a.startTime ?? a.StartTime ?? 0).getTime();
        const tb = new Date(b.startTime ?? b.StartTime ?? 0).getTime();
        return ta - tb;
    });
}

document.addEventListener("DOMContentLoaded", () => {
    const dateInput = document.getElementById("appointment-date");
    if (!dateInput) return;

    document.body.classList.add("appointment-bar-visible");

    const now = new Date();
    calendarViewYear = now.getFullYear();
    calendarViewMonth = now.getMonth() + 1;

    const params = new URLSearchParams(window.location.search);
    const serviceidsRaw = params.get("serviceids") ?? "";
    selectedServiceIds = serviceidsRaw
        ? serviceidsRaw.split(",").map((x) => Number(x.trim())).filter((n) => n > 0)
        : [];
    selectedArtistId = Number(params.get("artistid")) || 0;
    selectedSalonId = Number(params.get("salonId") || params.get("salonid")) || 0;

    const statusEl = document.getElementById("time-slots-status");
    const slotsPanel = document.querySelector(".appointment-page .time-slots");
    const earliestBtn = document.getElementById("appointment-earliest-btn");
    const calendarGrid = document.getElementById("calendar-grid");
    const calendarPrev = document.getElementById("calendar-prev");
    const calendarNext = document.getElementById("calendar-next");
    const calendarMsg = document.getElementById("calendar-unavailable-msg");

    function applySlotSelection(slot) {
        if (!slot || !slotsPanel || slot.classList.contains("time-slots-empty")) return;
        slotsPanel.querySelectorAll(".time-slot").forEach((item) => item.classList.remove("time-slot-selected"));
        slot.classList.add("time-slot-selected");
        const rawEnc = slot.getAttribute("data-start-iso") || "";
        try {
            selectedSlotStartIso = rawEnc ? decodeURIComponent(rawEnc) : "";
        } catch (_) {
            selectedSlotStartIso = rawEnc;
        }
        syncAppointmentBar();
    }

    async function loadSlotsForSelectedDate() {
        selectedDate = dateInput.value;
        selectedSlotStartIso = "";
        syncAppointmentBar();

        const list = document.getElementById("time-slots");
        if (!list) return;

        if (!selectedDate || selectedServiceIds.length === 0 || selectedArtistId <= 0) {
            list.innerHTML = "";
            if (statusEl) statusEl.textContent = "";
            return;
        }

        if (statusEl) statusEl.textContent = "Időpontok betöltése…";

        const timeData = {
            artistId: selectedArtistId,
            serviceIds: selectedServiceIds,
            date: selectedDate,
        };

        let stamps = [];
        try {
            stamps = await GetAvaliableSlots(timeData);
        } catch (err) {
            console.error(err);
            list.innerHTML = "";
            if (statusEl) statusEl.textContent = "Nem sikerült betölteni az időpontokat.";
            return;
        }

        list.innerHTML = "";
        if (!Array.isArray(stamps) || stamps.length === 0) {
            if (statusEl) statusEl.textContent = "Ezen a napon nincs szabad időpont.";
            list.innerHTML =
                '<li class="time-slots-empty" role="status">Próbálj másik napot.</li>';
            return;
        }

        stamps = sortSlotsByStartTime(stamps);

        if (statusEl) statusEl.textContent = `${stamps.length} szabad időpont`;

        stamps.forEach((slot, i) => {
            const raw = slot.startTime ?? slot.StartTime;
            const isoStr = typeof raw === "string" ? raw : String(raw);
            const label = isoStr.includes("T") ? isoStr.split("T")[1].slice(0, 5) : isoStr;
            const enc = encodeURIComponent(isoStr);
            list.innerHTML += `<li class="time-slot async-card-enter" style="--async-card-i:${i}" data-start-iso="${enc}">${label}</li>`;
        });
    }

    async function renderCalendar() {
        const grid = document.getElementById("calendar-grid");
        const label = document.getElementById("calendar-month-label");
        if (!grid || !label) return;

        const firstOfMonth = new Date(calendarViewYear, calendarViewMonth - 1, 1);
        label.textContent = firstOfMonth.toLocaleDateString("hu-HU", { year: "numeric", month: "long" });

        if (selectedServiceIds.length === 0 || selectedArtistId <= 0) {
            grid.innerHTML = "";
            if (calendarMsg) {
                calendarMsg.hidden = false;
                calendarMsg.textContent =
                    "A naptár betöltéséhez válassz szolgáltatásokat és szakembert a foglalás előző lépéseiben.";
            }
            if (calendarPrev) calendarPrev.disabled = true;
            if (calendarNext) calendarNext.disabled = true;
            return;
        }

        if (calendarMsg) calendarMsg.hidden = true;

        let monthDays = [];
        try {
            monthDays = await GetMonthAvailability({
                artistId: selectedArtistId,
                serviceIds: selectedServiceIds,
                year: calendarViewYear,
                month: calendarViewMonth,
            });
        } catch (err) {
            console.error(err);
            grid.innerHTML = "";
            if (calendarMsg) {
                calendarMsg.hidden = false;
                calendarMsg.textContent = "A naptár betöltése sikertelen.";
            }
            return;
        }

        const availabilityByYmd = new Map();
        monthDays.forEach((row) => {
            const raw = row.date ?? row.Date;
            if (!raw) return;
            let ymd;
            const s = typeof raw === "string" ? raw : String(raw);
            if (/^\d{4}-\d{2}-\d{2}/.test(s)) {
                ymd = s.slice(0, 10);
            } else {
                const dt = new Date(raw);
                if (Number.isNaN(dt.getTime())) return;
                ymd = appointmentFormatLocalYMD(dt);
            }
            const ok = Boolean(row.hasAvailableAppointments ?? row.HasAvailableAppointments);
            availabilityByYmd.set(ymd, ok);
        });

        const daysInMonth = new Date(calendarViewYear, calendarViewMonth, 0).getDate();
        const firstDay = new Date(calendarViewYear, calendarViewMonth - 1, 1);
        const firstMondayOffset = mondayOffset(firstDay);
        const todayStr = appointmentFormatLocalYMD(new Date());

        const nowDate = new Date();
        const minMonth = new Date(nowDate.getFullYear(), nowDate.getMonth(), 1);
        const maxMonth = new Date(nowDate.getFullYear(), nowDate.getMonth() + 12, 1);
        const viewMonth = new Date(calendarViewYear, calendarViewMonth - 1, 1);
        if (calendarPrev) calendarPrev.disabled = viewMonth.getTime() <= minMonth.getTime();
        if (calendarNext) calendarNext.disabled = viewMonth.getTime() >= maxMonth.getTime();

        let html = "";
        for (let i = 0; i < firstMondayOffset; i++) {
            html += '<div class="calendar-day-spacer" aria-hidden="true"></div>';
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const ymd = `${calendarViewYear}-${String(calendarViewMonth).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
            const isPast = ymd < todayStr;
            const hasFree = availabilityByYmd.get(ymd) === true;
            const isSelected = dateInput.value === ymd;

            if (isPast) {
                html += `<button type="button" class="calendar-day calendar-day--disabled" disabled data-ymd="${ymd}">${day}</button>`;
            } else if (!hasFree) {
                html += `<button type="button" class="calendar-day calendar-day--disabled calendar-day--muted" disabled data-ymd="${ymd}" title="Nincs szabad időpont">${day}</button>`;
            } else {
                let cls = "calendar-day calendar-day--available";
                if (isSelected) cls += " calendar-day--selected";
                html += `<button type="button" class="${cls}" data-ymd="${ymd}" title="Szabad időpont">${day}</button>`;
            }
        }

        grid.classList.remove("calendar-grid-enter");
        grid.innerHTML = html;
        requestAnimationFrame(() => {
            grid.classList.add("calendar-grid-enter");
        });
    }

    async function shiftCalendarMonth(delta) {
        const d = new Date(calendarViewYear, calendarViewMonth - 1 + delta, 1);
        calendarViewYear = d.getFullYear();
        calendarViewMonth = d.getMonth() + 1;
        const cur = dateInput.value;
        if (cur) {
            const parts = cur.split("-").map(Number);
            const sy = parts[0];
            const sm = parts[1];
            if (sy !== calendarViewYear || sm !== calendarViewMonth) {
                dateInput.value = "";
                selectedSlotStartIso = "";
                syncAppointmentBar();
                const list = document.getElementById("time-slots");
                if (list) list.innerHTML = "";
                if (statusEl) statusEl.textContent = "";
            }
        }
        await renderCalendar();
    }

    if (calendarPrev) {
        calendarPrev.addEventListener("click", () => void shiftCalendarMonth(-1));
    }
    if (calendarNext) {
        calendarNext.addEventListener("click", () => void shiftCalendarMonth(1));
    }

    if (calendarGrid) {
        calendarGrid.addEventListener("click", (e) => {
            const btn = e.target.closest("button.calendar-day--available");
            if (!btn) return;
            const ymd = btn.getAttribute("data-ymd");
            if (!ymd) return;
            dateInput.value = ymd;
            void (async () => {
                await renderCalendar();
                await loadSlotsForSelectedDate();
            })();
        });
    }

    async function findEarliestSlotWithinDays(maxDays) {
        const base = { artistId: selectedArtistId, serviceIds: selectedServiceIds };
        for (let i = 0; i < maxDays; i++) {
            const d = new Date();
            d.setDate(d.getDate() + i);
            const ymd = appointmentFormatLocalYMD(d);
            let stamps = [];
            try {
                stamps = await GetAvaliableSlots({ ...base, date: ymd });
            } catch (_) {
                continue;
            }
            if (!Array.isArray(stamps) || stamps.length === 0) continue;
            sortSlotsByStartTime(stamps);
            return { ymd };
        }
        return null;
    }

    if (earliestBtn) {
        earliestBtn.hidden = selectedServiceIds.length === 0 || selectedArtistId <= 0;
        earliestBtn.addEventListener("click", async () => {
            if (selectedServiceIds.length === 0 || selectedArtistId <= 0) {
                salonicShowModal("Hiányoznak a szolgáltatások vagy a szakember. Lépj vissza, és válassz újra.", {
                    title: "Hiányzó adatok",
                    variant: "warning",
                });
                return;
            }
            const prevText = earliestBtn.textContent;
            earliestBtn.disabled = true;
            earliestBtn.textContent = "Keresés…";
            try {
                const found = await findEarliestSlotWithinDays(90);
                if (!found) {
                    salonicShowModal(
                        "A következő 90 napban nem található szabad időpont. Próbáld később újra, vagy másik szakembert.",
                        { title: "Nincs szabad idő", variant: "info" },
                    );
                    return;
                }
                const parts = found.ymd.split("-").map(Number);
                calendarViewYear = parts[0];
                calendarViewMonth = parts[1];
                dateInput.value = found.ymd;
                await renderCalendar();
                await loadSlotsForSelectedDate();
                const list = document.getElementById("time-slots");
                const first = list?.querySelector(".time-slot:not(.time-slots-empty)");
                if (first) {
                    applySlotSelection(first);
                    first.scrollIntoView({ behavior: "smooth", block: "nearest" });
                }
            } finally {
                earliestBtn.disabled = false;
                earliestBtn.textContent = prevText;
            }
        });
    }

    if (slotsPanel) {
        slotsPanel.addEventListener("click", (event) => {
            const slot = event.target.closest(".time-slot");
            if (!slot) return;
            applySlotSelection(slot);
        });
    }

    void (async () => {
        await renderCalendar();
        await loadSlotsForSelectedDate();
    })();
});

function buildPendingBookingLoginSearchParams() {
    const q = new URLSearchParams();
    q.set("appointmentDate", selectedDate);
    q.set("startTime", selectedSlotStartIso);
    q.set("serviceids", selectedServiceIds.join(","));
    q.set("artistid", String(selectedArtistId));
    if (selectedSalonId > 0) {
        q.set("salonId", String(selectedSalonId));
    }
    return q;
}

function redirectToGuestLoginWithPendingBooking() {
    window.location.assign(`/Salon/Login?${buildPendingBookingLoginSearchParams().toString()}`);
}

window.SaveDate = async function SaveDate() {
    if (!selectedDate || !selectedSlotStartIso) {
        salonicShowModal("Válassz dátumot és időpontot.", { title: "Hiányzó adatok", variant: "warning" });
        return;
    }

    if (selectedDate < appointmentFormatLocalYMD(new Date())) {
        salonicShowModal("Válassz a mai napot vagy egy későbbi dátumot.", { title: "Érvénytelen dátum", variant: "warning" });
        return;
    }

    if (selectedServiceIds.length === 0 || selectedArtistId <= 0) {
        salonicShowModal("Hiányoznak a szolgáltatások vagy a szakember. Lépj vissza, és válassz újra.", {
            title: "Hiányzó adatok",
            variant: "warning",
        });
        return;
    }

    const token = localStorage.getItem("token");
    if (!token || !String(token).trim()) {
        salonicShowModal(
            "A foglaláshoz előbb jelentkezz be vendégként. A bejelentkezés után a foglalás automatikusan elküldődik.",
            { title: "Bejelentkezés szükséges", variant: "info" },
        );
        redirectToGuestLoginWithPendingBooking();
        return;
    }

    const role = typeof getRoleFromToken === "function" ? getRoleFromToken() : null;
    if (role !== "guest") {
        salonicShowModal(
            "A foglaláshoz vendég fiókkal kell belépni. Ha tulajdonosként vagy szakemberként vagy bejelentkezve, jelentkezz ki, és jelentkezz be vendégként (vagy regisztrálj vendégként).",
            { title: "Szerepkör", variant: "warning" },
        );
        redirectToGuestLoginWithPendingBooking();
        return;
    }

    let guestUser;
    try {
        guestUser = await GetLoggedInUser(token);
    } catch (err) {
        console.error(err);
        if (!err?.salonicApiShown) {
            salonicShowModal(
                "A foglaláshoz jelentkezz be vendégként. A bejelentkezés után a foglalás automatikusan elküldődik.",
                { title: "Bejelentkezés szükséges", variant: "info" },
            );
        }
        redirectToGuestLoginWithPendingBooking();
        return;
    }

    const guestId = Number(guestUser?.id ?? guestUser?.Id);
    if (!Number.isFinite(guestId) || guestId < 1) {
        salonicShowModal(
            "A foglaláshoz vendégként jelentkezz be. A bejelentkezés után a foglalás automatikusan elküldődik.",
            { title: "Profil", variant: "warning" },
        );
        redirectToGuestLoginWithPendingBooking();
        return;
    }

    const appointment = {
        guestId: guestId,
        artistId: selectedArtistId,
        salonId: selectedSalonId > 0 ? selectedSalonId : 0,
        serviceIds: selectedServiceIds,
        startTime: selectedSlotStartIso,
        notes: "",
    };
    try {
        await PostAppointment(appointment, token);
        const salonQ =
            selectedSalonId > 0 ? `?salonId=${encodeURIComponent(String(selectedSalonId))}` : "";
        window.location.assign(`/Salon/AppointmentCreated${salonQ}`);
    } catch (err) {
        console.error(err);
        if (err?.salonicApiShown) return;
        const msg = String(err?.message || "");
        if (msg.includes("403") || /Forbidden/i.test(msg)) {
            salonicShowModal(
                "A foglalás csak vendég fiókkal engedélyezett, és a vendég azonosítónak egyeznie kell a belépéssel. Jelentkezz be vendégként, majd próbáld újra.",
                { title: "Hozzáférés megtagadva", variant: "warning" },
            );
            return;
        }
        salonicShowModal(err?.message || "Nem sikerült lefoglalni ezt az időpontot. Próbálj másik időpontot.", {
            title: "Foglalás",
            variant: "danger",
        });
    }
};
