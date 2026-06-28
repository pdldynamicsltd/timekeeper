// Shared utilities for time-tracking workspace views.
// Load this before any page-specific JS that depends on it.
// Scheduler-aware helpers (toSchedulerDate, toLocalDateTimeString) require
// dhtmlxscheduler to have been loaded first.
window.app = window.app || {};
app.workspace = app.workspace || {};

(function (workspace) {
    workspace.escapeHtml = function (text) {
        if (text === undefined || text === null) {
            return '';
        }
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    };

    workspace.toSchedulerDate = function (dt) {
        return dt instanceof Date ? dt : new Date(dt);
    };

    workspace.toLocalDateTimeString = function (date) {
        var value = workspace.toSchedulerDate(date);
        if (!isNaN(value.getTime())) {
            return scheduler.date.date_to_str('%Y-%m-%dT%H:%i')(value);
        }
        return '';
    };

    workspace.debounce = function (fn, delay) {
        var timer = null;
        return function () {
            var args = arguments;
            if (timer) { clearTimeout(timer); }
            timer = setTimeout(function () { fn.apply(null, args); }, delay);
        };
    };
}(app.workspace));
