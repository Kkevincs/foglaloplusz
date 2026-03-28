(function () {
    const singleRadio = document.getElementById("schedule-mode-single");
    const weekRadio = document.getElementById("schedule-mode-week");
    const singlePanel = document.getElementById("single-day-panel");
    const weeklyPanel = document.getElementById("weekly-panel");
    const weekList = document.getElementById("weekly-rows-root");
    const workDate = document.getElementById("work-date");
    const whRangeStart = document.getElementById("wh-range-start");
    const whRangeFinish = document.getElementById("wh-range-finish");

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
            endInput.classList.toggle("wh-time-invalid", isOpen && endInput.value <= startInput.value);
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
                calendarDay.toLocaleDateString(undefined, { weekday: "short" }) +
                '</span><span class="wh-calendar-date">' +
                calendarDay.toLocaleDateString(undefined, { month: "short", day: "numeric" }) +
                '</span></div><label class="working-hours-open-label"><input type="checkbox" class="wh-open" checked> Open</label><input type="time" class="wh-start" value="' +
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
        workDate.disabled = isSingleMode;
        document.querySelectorAll("#single-day-times [type=time]").forEach((timeInput) => {
            timeInput.disabled = isSingleMode;
        });
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
    function syncSingleDayEndStyle() {
        singleEndInput.classList.toggle("wh-time-invalid", singleEndInput.value <= singleStartInput.value);
    }
    singleStartInput.oninput = singleEndInput.oninput = syncSingleDayEndStyle;

    singleRadio.onchange = weekRadio.onchange = syncScheduleMode;
    syncScheduleMode();
})();
