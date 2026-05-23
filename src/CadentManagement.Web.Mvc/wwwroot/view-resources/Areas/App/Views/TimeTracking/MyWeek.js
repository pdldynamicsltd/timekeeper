(function () {
    var _timeEntryService = abp.services.app.timeEntry;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditTimeEntryModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditTimeEntryModal.js',
        modalClass: 'CreateOrEditTimeEntryModal'
    });

    var _currentWeekStart = getWeekStart(new Date());

    // ─── Date helpers ──────────────────────────────────────────────────
    function getWeekStart(date) {
        var d = new Date(date);
        var day = d.getDay(); // 0=Sun
        var diff = d.getDate() - day + (day === 0 ? -6 : 1); // Mon
        d.setDate(diff);
        d.setHours(0, 0, 0, 0);
        return d;
    }

    function getWeekEnd(weekStart) {
        var d = new Date(weekStart);
        d.setDate(d.getDate() + 6);
        d.setHours(23, 59, 59, 999);
        return d;
    }

    function formatDateLabel(date) {
        return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
    }

    function toSchedulerDate(dt) {
        // DHTMLX expects Date objects
        return dt instanceof Date ? dt : new Date(dt);
    }

    // ─── Scheduler setup ───────────────────────────────────────────────
    function initScheduler() {
        scheduler.config.header = [
            'day',
            'week',
            'date',
            'prev',
            'today',
            'next'
        ];
        scheduler.config.multi_day = true;
        scheduler.config.first_hour = 7;
        scheduler.config.last_hour = 20;
        scheduler.config.hour_size_px = 44;
        scheduler.config.time_step = 15;
        scheduler.config.details_on_create = false;
        scheduler.config.details_on_dblclick = true;
        scheduler.config.readonly = !abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Edit');

        // Custom event template — show project name in addition to description
        scheduler.templates.event_bar_text = function (start, end, event) {
            return '<b>' + (event.projectName || '') + '</b>' +
                (event.taskName ? ' / ' + event.taskName : '') +
                (event.description ? '<br/><small>' + event.description + '</small>' : '');
        };

        scheduler.templates.event_text = function (start, end, event) {
            return '<b>' + (event.projectName || '') + '</b>' +
                (event.taskName ? ' / ' + event.taskName : '') +
                (event.description ? '<br/><small>' + event.description + '</small>' : '');
        };

        scheduler.templates.event_class = function (start, end, event) {
            return 'tt-project-event';
        };

        // Colour events by project colour
        scheduler.templates.event_bar_date = function (start, end, event) { return ''; };

        scheduler.attachEvent('onBeforeEventChanged', function (ev, e, is_new, original) {
            // Allow drag-drop only if edit permission granted
            return abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Edit');
        });

        scheduler.attachEvent('onEventChanged', function (id, ev) {
            if (id < 0) return true; // newly added via drag
            _timeEntryService.update({
                id: id,
                projectId: ev.projectId,
                taskId: ev.taskId || null,
                startTime: ev.start_date,
                endTime: ev.end_date,
                description: ev.description
            }).done(function () {
                abp.notify.success(app.localize('SuccessfullySaved'));
                loadWeekEntries();
            }).fail(function () {
                abp.notify.error(app.localize('LoadError'));
                loadWeekEntries();
            });
        });

        scheduler.attachEvent('onEventDeleted', function (id, ev) {
            if (id < 0) return;
            abp.message.confirm(
                app.localize('TimeEntryDeleteWarningMessage'),
                app.localize('AreYouSure'),
                function (confirmed) {
                    if (confirmed) {
                        _timeEntryService.delete({ id: id }).done(function () {
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                            loadWeekEntries();
                        });
                    } else {
                        loadWeekEntries();
                    }
                }
            );
        });

        scheduler.attachEvent('onDblClick', function (id, e) {
            _createOrEditModal.open({ id: id });
            return false;
        });

        // Click on empty slot — open create modal with pre-filled dates
        scheduler.attachEvent('onClick', function (id, e) {
            return true;
        });

        scheduler.attachEvent('onEmptyClick', function (date, e) {
            var endDate = new Date(date.getTime() + 60 * 60 * 1000); // +1 hour
            _createOrEditModal.open({
                startTime: date.toISOString(),
                endTime: endDate.toISOString()
            });
            return false;
        });

        scheduler.init('myWeekScheduler', _currentWeekStart, 'week');
    }

    // ─── Data loading ──────────────────────────────────────────────────
    function loadWeekEntries() {
        var weekEnd = getWeekEnd(_currentWeekStart);

        _timeEntryService.getSchedulerEntries({
            forCurrentUserOnly: true,
            startDate: _currentWeekStart,
            endDate: weekEnd
        }).done(function (entries) {
            scheduler.clearAll();

            var events = $.map(entries, function (e) {
                return {
                    id: e.id,
                    text: e.text,
                    start_date: toSchedulerDate(e.startDate),
                    end_date: toSchedulerDate(e.endDate),
                    color: e.color || '#3498db',
                    projectId: e.projectId,
                    projectName: e.projectName,
                    taskId: e.taskId,
                    taskName: e.taskName,
                    description: e.description
                };
            });

            scheduler.parse(events, 'json');
            renderWeekSummary(events);
        }).fail(function () {
            abp.notify.error(app.localize('LoadError'));
        });

        updateWeekLabel();
    }

    // ─── Week summary ──────────────────────────────────────────────────
    function renderWeekSummary(events) {
        var projectMap = {};
        var totalHours = 0;

        $.each(events, function (i, ev) {
            var durationMs = toSchedulerDate(ev.end_date) - toSchedulerDate(ev.start_date);
            var hours = durationMs / (1000 * 60 * 60);
            totalHours += hours;

            var key = ev.projectId;
            if (!projectMap[key]) {
                projectMap[key] = { name: ev.projectName, color: ev.color, hours: 0 };
            }
            projectMap[key].hours += hours;
        });

        $('#WeekTotalHours').text(totalHours.toFixed(1) + ' hrs');

        var container = $('#WeekProjectSummary');
        container.empty();

        $.each(projectMap, function (pid, info) {
            container.append(
                '<div class="week-project-badge">' +
                '<div class="week-project-dot" style="background:' + info.color + '"></div>' +
                '<span class="fw-semibold">' + info.name + '</span>' +
                '<span class="text-muted">' + info.hours.toFixed(1) + ' hrs</span>' +
                '</div>'
            );
        });

        if (Object.keys(projectMap).length === 0) {
            container.html('<span class="text-muted fs-7">' + app.localize('NoTimeEntriesFound') + '</span>');
        }
    }

    function updateWeekLabel() {
        var weekEnd = getWeekEnd(_currentWeekStart);
        $('#WeekLabel').text(
            formatDateLabel(_currentWeekStart) + ' – ' + formatDateLabel(weekEnd)
        );
    }

    // ─── Navigation ────────────────────────────────────────────────────
    $('#PrevWeekButton').click(function () {
        _currentWeekStart = new Date(_currentWeekStart);
        _currentWeekStart.setDate(_currentWeekStart.getDate() - 7);
        scheduler.setCurrentView(_currentWeekStart, 'week');
        loadWeekEntries();
    });

    $('#NextWeekButton').click(function () {
        _currentWeekStart = new Date(_currentWeekStart);
        _currentWeekStart.setDate(_currentWeekStart.getDate() + 7);
        scheduler.setCurrentView(_currentWeekStart, 'week');
        loadWeekEntries();
    });

    $('#TodayButton').click(function () {
        _currentWeekStart = getWeekStart(new Date());
        scheduler.setCurrentView(_currentWeekStart, 'week');
        loadWeekEntries();
    });

    // ─── Log time button ───────────────────────────────────────────────
    $('#LogTimeButton').click(function () {
        var now = new Date();
        var end = new Date(now.getTime() + 60 * 60 * 1000);
        _createOrEditModal.open({
            startTime: now.toISOString(),
            endTime: end.toISOString()
        });
    });

    // Reload after time entry saved
    abp.event.on('app.createOrEditTimeEntryModalSaved', function () {
        loadWeekEntries();
    });

    // ─── Init ──────────────────────────────────────────────────────────
    initScheduler();
    loadWeekEntries();

    // Override scheduler navigation to sync our week state
    scheduler.attachEvent('onViewChange', function (new_mode, new_date) {
        _currentWeekStart = getWeekStart(new_date);
        loadWeekEntries();
    });
})();
