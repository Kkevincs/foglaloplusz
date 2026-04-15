(function () {
    var BTN_SELECTOR =
        'button, input[type="submit"], input[type="button"], a.portal-btn, a.owner-btn, a.auth-cta, a.btn';

    function hexToRgb(hex) {
        var m = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec((hex || '').trim());
        if (!m) return null;
        return {
            r: parseInt(m[1], 16),
            g: parseInt(m[2], 16),
            b: parseInt(m[3], 16),
        };
    }

    function primaryGlowRgb() {
        var raw = getComputedStyle(document.body).getPropertyValue('--primary').trim();
        var rgb = hexToRgb(raw);
        if (rgb) return rgb;
        return { r: 236, g: 132, b: 148 };
    }

    function isInsideNavbar(el) {
        return !!(el && el.closest && el.closest('nav'));
    }

    function animateSparkle(el) {
        if (typeof anime !== 'function') return;
        var c = primaryGlowRgb();
        var r = c.r;
        var g = c.g;
        var b = c.b;

        anime({
            targets: el,
            scale: [1, 1.08, 1],
            rotate: [0, 5, -5, 3, -3, 0],
            duration: 520,
            easing: 'easeOutElastic(1, .55)',
        });

        anime({
            targets: el,
            boxShadow: [
                '0 0 0 0 rgba(' + r + ',' + g + ',' + b + ',0)',
                '0 0 26px 8px rgba(' + r + ',' + g + ',' + b + ',0.5)',
                '0 0 0 0 rgba(' + r + ',' + g + ',' + b + ',0)',
            ],
            duration: 580,
            easing: 'easeOutQuad',
        });
    }

    function showToast(message) {
        if (typeof anime !== 'function') return;
        var host = document.getElementById('salonic-toast-root');
        if (!host) {
            host = document.createElement('div');
            host.id = 'salonic-toast-root';
            host.setAttribute('aria-live', 'polite');
            document.body.appendChild(host);
        }

        var toast = document.createElement('div');
        toast.className = 'salonic-toast';
        toast.textContent = message;
        host.appendChild(toast);

        anime({
            targets: toast,
            opacity: [0, 1],
            translateY: [18, 0],
            duration: 280,
            easing: 'easeOutQuad',
            complete: function () {
                anime({
                    targets: toast,
                    opacity: [1, 0],
                    translateY: [0, -10],
                    duration: 420,
                    delay: 2000,
                    easing: 'easeInQuad',
                    complete: function () {
                        toast.remove();
                    },
                });
            },
        });
    }

    function defaultToastText(el) {
        var label = (el.getAttribute('aria-label') || el.textContent || '').trim();
        if (label.length > 0 && label.length <= 48) return label;
        return 'Action received';
    }

    function onDocumentClick(e) {
        var el = e.target.closest(BTN_SELECTOR);
        if (!el || isInsideNavbar(el)) return;
        if ((el.matches('button') || el.matches('input')) && el.disabled) return;

        var msg = el.getAttribute('data-toast');
        if (!msg || !msg.trim()) msg = defaultToastText(el);
        else msg = msg.trim();

        animateSparkle(el);
        showToast(msg);
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof anime !== 'function') return;
        document.addEventListener('click', onDocumentClick);
    });
})();
