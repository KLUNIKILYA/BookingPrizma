// Клик по дате в шапке планировщика Syncfusion -> открыть этот день (вызов в Blazor).
window.bkSetupDateClick = function (dotNetRef) {
    window.__bkDateRef = dotNetRef;
    if (window.__bkDateClickBound) return;
    window.__bkDateClickBound = true;

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.e-schedule')) return;
        var cell = e.target.closest('.e-header-cells');
        if (!cell) return;
        var ms = cell.getAttribute('data-date');
        if (!ms) {
            var inner = cell.querySelector('[data-date]');
            if (inner) ms = inner.getAttribute('data-date');
        }
        if (!ms) return;
        var ref = window.__bkDateRef;
        if (ref) ref.invokeMethodAsync('OpenDay', Number(ms));
    }, true);
};
