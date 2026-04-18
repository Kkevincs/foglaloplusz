(function () {
    const form = document.getElementById("working-hours-form");
    const singleRadio = document.getElementById("schedule-mode-single");
    const weekRadio = document.getElementById("schedule-mode-week");
    const singlePanel = document.getElementById("single-day-panel");
    const weeklyPanel = document.getElementById("weekly-panel");
    const weekList = document.getElementById("weekly-rows-root");
    const workDate = document.getElementById("work-date");
    const whRangeStart = document.getElementById("wh-range-start");
    const whRangeFinish = document.getElementById("wh-range-finish");
    const singleOpenCheckbox = document.getElementById("wh-single-open");

    function formatLocalYMD(date) {
        return (
            date.getFullYear() +
            "-" +
            String(date.getMonth() + 1).padStart(2, "0") +
            "-" +
            String(date.getDate()).padStart(2, "0")
        );
    }

    function bindRow(row) {
        const openCheckbox = row.querySelector(".wh-open");
        const startInput = row.querySelector(".wh-start");
        const endInput = row.querySelector(".wh-end");
        function syncRow() {
            const isOpen = openCheckbox.checked;
            startInput.disabled = endInput.disabled = !isOpen;
        }
        openCheckbox.onchange = syncRow;
        startInput.oninput = endInput.oninput = syncRow;
        syncRow();
    }

    function buildRowsFromRange() {
        weekList.innerHTML = "";
        if (!whRangeStart.value || !whRangeFinish.value) return;

        const startParts = whRangeStart.value.split("-");
        const endParts = whRangeFinish.value.split("-");
        const rangeStartDate = new Date(+startParts[0], +startParts[1] - 1, +startParts[2]);
        const rangeEndDate = new Date(+endParts[0], +endParts[1] - 1, +endParts[2]);

        for (let calendarDay = new Date(rangeStartDate); calendarDay <= rangeEndDate; calendarDay.setDate(calendarDay.getDate() + 1)) {
            const isWeekend = calendarDay.getDay() === 0 || calendarDay.getDay() === 6;
            const row = document.createElement("div");
            row.className = "working-hours-day-row";
            row.dataset.whDate = formatLocalYMD(calendarDay);
            row.innerHTML =
                '<div class="working-hours-day-col"><span class="working-hours-day-name">' +
                calendarDay.toLocaleDateString("hu-HU", { weekday: "short" }) +
                '</span><span class="wh-calendar-date">' +
                calendarDay.toLocaleDateString("hu-HU", { month: "short", day: "numeric" }) +
                '</span></div><label class="working-hours-open-label"><input type="checkbox" class="wh-open" checked> Nyitva</label><input type="time" class="wh-start" value="' +
                (isWeekend ? "10:00" : "09:00") +
                '"><input type="time" class="wh-end" value="' +
                (isWeekend ? "16:00" : "17:00") +
                '">';
            weekList.appendChild(row);
            bindRow(row);
        }
    }

    function syncScheduleMode() {
        const isSingleMode = singleRadio.checked;
        singlePanel.hidden = !isSingleMode;
        weeklyPanel.hidden = isSingleMode;
        workDate.disabled = !isSingleMode;
        const singleTimesOpen = isSingleMode && singleOpenCheckbox.checked;
        document.querySelectorAll("#single-day-times [type=time]").forEach((timeInput) => {
            timeInput.disabled = !singleTimesOpen;
        });
        if (singleOpenCheckbox) singleOpenCheckbox.disabled = !isSingleMode;
        whRangeStart.disabled = isSingleMode;
        whRangeFinish.disabled = isSingleMode;

        weekList.querySelectorAll(".working-hours-day-row").forEach((dayRow) => {
            const openCheckbox = dayRow.querySelector(".wh-open");
            const startInput = dayRow.querySelector(".wh-start");
            const endInput = dayRow.querySelector(".wh-end");
            openCheckbox.disabled = isSingleMode;
            if (isSingleMode) {
                startInput.disabled = true;
                endInput.disabled = true;
            } else {
                startInput.disabled = endInput.disabled = !openCheckbox.checked;
            }
        });

        if (!isSingleMode) {
            const tomorrow = new Date();
            tomorrow.setDate(tomorrow.getDate() + 1);
            if (!whRangeStart.value) whRangeStart.value = formatLocalYMD(tomorrow);
            if (!whRangeFinish.value) {
                const defaultFinish = new Date(tomorrow);
                defaultFinish.setDate(defaultFinish.getDate() + 7);
                whRangeFinish.value = formatLocalYMD(defaultFinish);
            }
            buildRowsFromRange();
        }
    }

    whRangeStart.onchange = whRangeFinish.onchange = function () {
        if (!weeklyPanel.hidden) buildRowsFromRange();
    };

    const singleStartInput = document.getElementById("wh-single-start");
    const singleEndInput = document.getElementById("wh-single-end");

    function syncSingleDayOpenState() {
        if (!singleRadio.checked || !singleOpenCheckbox) return;
        const on = singleOpenCheckbox.checked;
        singleStartInput.disabled = !on;
        singleEndInput.disabled = !on;
    }
    singleOpenCheckbox?.addEventListener("change", syncSingleDayOpenState);

    form?.addEventListener("submit", async (e) => {
        e.preventDefault();

        const token = localStorage.getItem("token");
        if (!token) {
            salonicShowModal("Először jelentkezz be.", { title: "Bejelentkezés szükséges", variant: "warning" });
            return;
        }

        try {
            if (singleRadio.checked) {
                if (!workDate.value) {
                    salonicShowModal("Add meg a dátumot.", { title: "Hiányzó adat", variant: "warning" });
                    return;
                }
                const isOpenDay = singleOpenCheckbox?.checked !== false;
                if (isOpenDay && (!singleStartInput.value || !singleEndInput.value)) {
                    salonicShowModal(
                        "Add meg a kezdő és záró időt, vagy vedd ki a „Nyitva” pipát, ha zárva van a nap.",
                        { title: "Hiányzó adat", variant: "warning" },
                    );
                    return;
                }

                await setWorkingHoursByDay(token, {
                    date: workDate.value,
                    startTime: isOpenDay ? singleStartInput.value : "0",
                    endTime: isOpenDay ? singleEndInput.value : "0"
                });
            } else {
                if (!whRangeStart.value || !whRangeFinish.value) {
                    salonicShowModal("Add meg a kezdő és záró dátumot.", { title: "Hiányzó adat", variant: "warning" });
                    return;
                }

                const rows = Array.from(weekList.querySelectorAll(".working-hours-day-row"));
                if (!rows.length) {
                    salonicShowModal("Válassz érvényes dátumtartományt.", { title: "Hiányzó adat", variant: "warning" });
                    return;
                }

                const days = rows.map((row) => {
                    const isOpen = row.querySelector(".wh-open").checked;
                    const start = row.querySelector(".wh-start").value;
                    const end = row.querySelector(".wh-end").value;
                    return isOpen ? { start, end } : { start: null, end: null };
                });

                await setWorkingHoursByWeek(token, {
                    startDate: whRangeStart.value,
                    days
                });
            }

            const salonId = new URLSearchParams(window.location.search).get("salonId");
            const salonQuery = salonId ? `?salonId=${encodeURIComponent(salonId)}` : "";
            window.location.assign(`/Salon/Artist/Hours${salonQuery}`);
        } catch (err) {
            if (!err?.salonicApiShown) {
                salonicShowModal("Nem sikerült menteni a nyitvatartást. Ellenőrizd a mezőket, és próbáld újra.", {
                    title: "Hiba",
                    variant: "danger",
                });
            }
        }
    });

    singleRadio.onchange = weekRadio.onchange = syncScheduleMode;
    syncScheduleMode();
    syncSingleDayOpenState();
})();
