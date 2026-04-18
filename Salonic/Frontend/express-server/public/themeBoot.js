/**
 * Azonnali téma a cache-ből (localStorage salonThemeById), még a többi script előtt.
 * Ugyanaz a normalizálás, mint common.js normalizeSalonThemeClass / applyTheme.
 */
(function () {
    function normalizeTheme(t) {
        t = String(t || "").trim();
        if (!t) return "";
        if (!t.startsWith("theme-")) {
            t = "theme-" + t.replace(/^theme-?/i, "").replace(/\s+/g, "-");
        }
        return t;
    }
    try {
        var p =
            new URLSearchParams(window.location.search).get("salonId") ||
            new URLSearchParams(window.location.search).get("salonid");
        if (!p) return;
        var cache = JSON.parse(localStorage.getItem("salonThemeById") || "{}");
        var raw = cache[p];
        if (raw == null || String(raw).trim() === "") return;
        var themeClass = normalizeTheme(raw);
        if (!themeClass) return;
        var rest = (document.body.className || "")
            .split(/\s+/)
            .filter(function (c) {
                return c && !/^theme-/.test(c);
            });
        document.body.className = [themeClass].concat(rest).join(" ").trim();
        document.documentElement.classList.add("salon-theme-bootstrapped");
    } catch (e) {
        /* ignore */
    }
})();
