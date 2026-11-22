// Theme toggle (light / dark). The initial theme is applied in the layout <head>.
(function () {
    var btn = document.getElementById('themeToggle');
    if (!btn) return;
    btn.addEventListener('click', function () {
        var root = document.documentElement;
        var next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        root.setAttribute('data-bs-theme', next);
        try { localStorage.setItem('tl-theme', next); } catch (e) { }
    });
})();
