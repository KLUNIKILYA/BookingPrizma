// Прячем trial-баннер Syncfusion. Он инжектится как fixed-div прямо в <body> (без id/класса,
// z-index 999999999) и может переинжектиться после ре-рендера — поэтому вешаем MutationObserver.
(function () {
    function isTrialBanner(el) {
        return el && el.nodeType === 1 && el.parentElement === document.body &&
            getComputedStyle(el).position === 'fixed' &&
            /trial version of Syncfusion/i.test(el.textContent || '') &&
            (el.textContent || '').length < 500;
    }
    function hideBanners() {
        var kids = document.body ? document.body.children : [];
        for (var i = 0; i < kids.length; i++) {
            if (isTrialBanner(kids[i])) kids[i].style.setProperty('display', 'none', 'important');
        }
    }
    function start() {
        hideBanners();
        try {
            var mo = new MutationObserver(hideBanners);
            mo.observe(document.body, { childList: true });
        } catch (e) { /* нет body/observer — не критично */ }
    }
    if (document.body) start();
    else document.addEventListener('DOMContentLoaded', start);
    window.bkHideTrialBanner = hideBanners; // ручной вызов из Blazor при желании
})();

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
        // Сами обрабатываем переход на день — гасим событие, иначе Syncfusion откроет popup.
        e.preventDefault();
        e.stopPropagation();
        if (e.stopImmediatePropagation) e.stopImmediatePropagation();
        var ref = window.__bkDateRef;
        if (ref) ref.invokeMethodAsync('OpenDay', Number(ms));
    }, true);
};

// Клик по шапке зоны (колонке) -> открыть эту зону отдельно (drill-in). Вызов в Blazor.
window.bkSetupResourceClick = function (dotNetRef) {
    window.__bkResRef = dotNetRef;
    if (window.__bkResClickBound) return;
    window.__bkResClickBound = true;

    document.addEventListener('click', function (e) {
        if (!e.target.closest) return;
        var cell = e.target.closest('.e-resource-cells');
        if (!cell || !cell.closest('.e-schedule')) return;
        var giAttr = cell.getAttribute('data-group-index');
        var gi = (giAttr === null || giAttr === '') ? -1 : Number(giAttr);
        if (gi < 0 && cell.parentNode) {
            // запасной вариант: индекс ячейки среди соседних шапок-зон
            var sibs = cell.parentNode.querySelectorAll('.e-resource-cells');
            gi = Array.prototype.indexOf.call(sibs, cell);
        }
        if (gi < 0) return;
        e.preventDefault();
        e.stopPropagation();
        if (e.stopImmediatePropagation) e.stopImmediatePropagation();
        var ref = window.__bkResRef;
        if (ref) ref.invokeMethodAsync('DrillZone', Number(gi));
    }, true);
};

// Включать собственное выделение только в режиме одного дня (колонки = комнаты).
window.bkSetGridSelEnabled = function (on) { window.__bkGridSelEnabled = !!on; };

// ---------------------------------------------------------------------------
// Собственное выделение ячеек сетки: вниз по времени и влево/вправо по комнатам.
// Drag — прямоугольник, Ctrl — добавить/убрать, Shift — диапазон от якоря.
// Клик по выделенной зоне (или по пустой ячейке) — открыть окно бронирования.
// Родное выделение Syncfusion подавляем, чтобы не было авто-скролла и дёрганья.
// ---------------------------------------------------------------------------
(function () {
    var dragging = false;   // идёт ли наш drag
    var moved = false;      // ушли ли с начальной ячейки (drag, а не клик)
    var dragBase = null;    // выделение-основа для ctrl-drag
    var dragStart = null;   // {date, gi}
    var startSelected = false;
    var anchor = null;      // якорь для shift

    function enabled() { return window.__bkGridSelEnabled !== false; }
    function workCells() {
        return Array.prototype.slice.call(
            document.querySelectorAll('.e-schedule td.e-work-cells[data-group-index]'));
    }
    function info(td) { return { date: Number(td.getAttribute('data-date')), gi: Number(td.getAttribute('data-group-index')) }; }
    function key(d, g) { return d + '|' + g; }
    function keyOf(td) { return key(td.getAttribute('data-date'), td.getAttribute('data-group-index')); }
    function currentKeys() {
        var s = {};
        document.querySelectorAll('.e-schedule td.bk-cell-sel').forEach(function (td) { s[keyOf(td)] = true; });
        return s;
    }
    function applyKeys(set) {
        workCells().forEach(function (td) {
            if (set[keyOf(td)]) td.classList.add('bk-cell-sel'); else td.classList.remove('bk-cell-sel');
        });
    }
    function rectKeys(a, b) {
        var dMin = Math.min(a.date, b.date), dMax = Math.max(a.date, b.date);
        var gMin = Math.min(a.gi, b.gi), gMax = Math.max(a.gi, b.gi), set = {};
        workCells().forEach(function (c) {
            var d = Number(c.getAttribute('data-date')), g = Number(c.getAttribute('data-group-index'));
            if (d >= dMin && d <= dMax && g >= gMin && g <= gMax) set[key(d, g)] = true;
        });
        return set;
    }
    function union(a, b) { var s = {}, k; for (k in a) s[k] = true; for (k in b) s[k] = true; return s; }
    function toggle(set, k) { var s = {}, x; for (x in set) s[x] = true; if (s[k]) delete s[k]; else s[k] = true; return s; }

    function cellFrom(e) {
        if (!enabled()) return null;
        if (!e.target.closest || !e.target.closest('.e-schedule')) return null;
        if (e.target.closest('.e-appointment')) return null; // запись — на редактирование
        return e.target.closest('td.e-work-cells[data-group-index]');
    }
    function stop(e) { if (e.preventDefault) e.preventDefault(); e.stopPropagation(); if (e.stopImmediatePropagation) e.stopImmediatePropagation(); }
    function openSelection() { var ref = window.__bkDateRef; if (ref) ref.invokeMethodAsync('OpenGridSelection'); }

    function onDown(e) {
        if (e.button != null && e.button !== 0) return;
        var td = cellFrom(e); if (!td) return;
        stop(e);
        var ci = info(td), k = key(ci.date, ci.gi);
        dragging = true; moved = false; dragStart = ci; startSelected = td.classList.contains('bk-cell-sel');

        if (e.shiftKey && anchor) {
            var base = e.ctrlKey ? currentKeys() : {};
            applyKeys(union(base, rectKeys(anchor, ci)));
            dragging = false; // shift — разовый диапазон
            return;
        }
        if (e.ctrlKey) {
            applyKeys(toggle(currentKeys(), k));
            anchor = ci; dragBase = currentKeys();
            return;
        }
        // обычный: решаем на move/up (чтобы клик по выделенной зоне не сбрасывал её)
        anchor = ci; dragBase = {};
    }
    function onMove(e) {
        if (!dragging) return;
        var t = document.elementFromPoint(e.clientX, e.clientY);
        t = t && t.closest ? t.closest('td.e-work-cells[data-group-index]') : null;
        stop(e);
        if (!t) return;
        var ci = info(t);
        if (ci.date !== dragStart.date || ci.gi !== dragStart.gi) moved = true;
        if (!moved && !e.ctrlKey) return; // пока клик — не трогаем выделение
        applyKeys(union(dragBase, rectKeys(dragStart, ci)));
    }
    function onUp(e) {
        if (!dragging) return;
        dragging = false;
        stop(e);
        if (e.shiftKey || e.ctrlKey) return; // shift/ctrl — только набираем выделение
        if (moved) return;                    // был drag — выделение готово
        // клик без движения:
        if (startSelected) {
            openSelection();                  // клик по выделенной зоне — открыть окно бронирования
        } else if (document.querySelector('.e-schedule td.bk-cell-sel')) {
            applyKeys({}); anchor = null;     // клик мимо выделения — сбросить выделение
        } else {
            var s = {}; s[key(dragStart.date, dragStart.gi)] = true; applyKeys(s); // выделить одну ячейку
        }
    }

    function blockMouse(e) {
        if (!enabled()) return;
        if (dragging) { stop(e); return; }
        if (cellFrom(e)) stop(e); // глушим мышиные события на ячейках, чтобы Syncfusion не вмешивался
    }

    document.addEventListener('pointerdown', onDown, true);
    document.addEventListener('pointermove', onMove, true);
    document.addEventListener('pointerup', onUp, true);
    ['mousedown', 'mousemove', 'mouseup', 'click', 'dblclick'].forEach(function (t) {
        document.addEventListener(t, blockMouse, true);
    });

    // Прочитать выделение: эпохи начала/конца (мс) + индексы колонок-комнат, либо null.
    window.bkGetGridSelection = function () {
        var cells = document.querySelectorAll('.e-schedule td.bk-cell-sel[data-date]');
        if (!cells.length) return null;
        var dates = [], groups = {};
        cells.forEach(function (c) {
            var d = Number(c.getAttribute('data-date'));
            if (!isNaN(d)) dates.push(d);
            var gi = c.getAttribute('data-group-index');
            if (gi !== null && gi !== '') groups[gi] = true;
        });
        if (!dates.length) return null;
        dates.sort(function (a, b) { return a - b; });
        var slot = 1800000;
        for (var i = 1; i < dates.length; i++) { var diff = dates[i] - dates[i - 1]; if (diff > 0) { slot = diff; break; } }
        var groupIndices = Object.keys(groups).map(Number).sort(function (a, b) { return a - b; });
        return { startMs: dates[0], endMs: dates[dates.length - 1] + slot, groupIndices: groupIndices };
    };

    window.bkClearGridSelection = function () {
        document.querySelectorAll('.e-schedule td.bk-cell-sel').forEach(function (td) { td.classList.remove('bk-cell-sel'); });
        anchor = null; dragStart = null; dragBase = null; dragging = false; moved = false;
    };
})();
