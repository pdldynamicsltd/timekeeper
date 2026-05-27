(function () {
    var _timeEntryService = abp.services.app.timeEntry;
    var _userTaskService = abp.services.app.userTask;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditTimeEntryModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditTimeEntryModal.js',
        modalClass: 'CreateOrEditTimeEntryModal'
    });

    var _currentDate = getWeekStart(new Date());
    var _currentMode = scheduler.createUnitsView ? 'ttUnits' : 'week';

    function getWeekStart(date) {
        var d = new Date(date);
        var day = d.getDay();
        var diff = d.getDate() - day + (day === 0 ? -6 : 1);
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

    function getMonthStart(date) {
        var d = new Date(date);
        d.setDate(1);
        d.setHours(0, 0, 0, 0);
        return d;
    }

    function getMonthEnd(date) {
        var d = new Date(date);
        d.setMonth(d.getMonth() + 1, 0);
        d.setHours(23, 59, 59, 999);
        return d;
    }

    function formatDateLabel(date) {
        return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
    }

    function toSchedulerDate(dt) {
        return dt instanceof Date ? dt : new Date(dt);
    }

    function isInRange(date, range) {
        var d = toSchedulerDate(date);
        return d >= range.startDate && d <= range.endDate;
    }

    function initScheduler() {
        var header = [
            'day',
            'week',
            'month',
            'date',
            'prev',
            'today',
            'next'
        ];

        if (scheduler.createUnitsView) {
            header.splice(1, 0, 'ttUnits');
        }
        scheduler.plugins({ units: true })

        scheduler.config.header = header;
        scheduler.config.multi_day = true;
        scheduler.config.first_hour = 7;
        scheduler.config.last_hour = 20;
        scheduler.config.hour_size_px = 44;
        scheduler.config.time_step = 15;
        scheduler.config.details_on_create = false;
        scheduler.config.details_on_dblclick = true;
        scheduler.config.readonly = !abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Edit');

        scheduler.locale.labels.ttUnits_tab = app.localize('MyWeek');

        if (scheduler.createUnitsView) {
            scheduler.createUnitsView({
                name: 'ttUnits',
                property: 'lane',
                list: [
                    { key: 'calendar', label: app.localize('TimeEntriesColumn') },
                    { key: 'completedTasks', label: app.localize('CompletedTasksColumn') }
                ],
                size: 2,
                step: 1,
                days: 7
            });
        }

        scheduler.templates.event_bar_text = function (start, end, event) {
            if (event.isCompletedTask) {
                return '<b>' + (event.taskName || event.text || app.localize('TaskTitle')) + '</b>' +
                    (event.projectName ? '<br/><small>' + event.projectName + '</small>' : '') +
                    '<br/><small>' + app.localize('ConvertToTimeEntry') + '</small>';
            }

            return '<b>' + (event.projectName || '') + '</b>' +
                (event.taskName ? ' / ' + event.taskName : '') +
                (event.description ? '<br/><small>' + event.description + '</small>' : '');
        };

        scheduler.templates.event_text = function (start, end, event) {
            if (event.isCompletedTask) {
                return '<b>' + (event.taskName || event.text || app.localize('TaskTitle')) + '</b>' +
                    (event.projectName ? '<br/><small>' + event.projectName + '</small>' : '') +
                    '<br/><small>' + app.localize('ConvertToTimeEntry') + '</small>';
            }

            return '<b>' + (event.projectName || '') + '</b>' +
                (event.taskName ? ' / ' + event.taskName : '') +
                (event.description ? '<br/><small>' + event.description + '</small>' : '');
        };

        scheduler.templates.event_class = function (start, end, event) {
            return event.isCompletedTask ? 'tt-completed-task' : 'tt-project-event';
        };

        scheduler.templates.event_bar_date = function () { return ''; };
        scheduler.templates.tooltip_text = function (start, end, event) {
            if (event.isCompletedTask) {
                return '<b>' + (event.taskName || event.text || app.localize('TaskTitle')) + '</b>' +
                    (event.projectName ? '<br/>' + event.projectName : '') +
                    '<br/><i>' + app.localize('ConvertToTimeEntry') + '</i>';
            }

            return event.description || event.text || '';
        };

        scheduler.attachEvent('onBeforeEventChanged', function (ev) {
            return !ev.isCompletedTask && abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Edit');
        });

        scheduler.attachEvent('onEventChanged', function (id, ev) {
            if (ev.isCompletedTask || id < 0) {
                return true;
            }

            _timeEntryService.update({
                id: id,
                projectId: ev.projectId,
                taskId: ev.taskId || null,
                startTime: ev.start_date,
                endTime: ev.end_date,
                description: ev.description
            }).done(function () {
                abp.notify.success(app.localize('SuccessfullySaved'));
                loadPeriodEntries();
            }).fail(function () {
                abp.notify.error(app.localize('LoadError'));
                loadPeriodEntries();
            });
        });

        scheduler.attachEvent('onEventDeleted', function (id, ev) {
            if (ev && ev.isCompletedTask) {
                return false;
            }

            if (id < 0) {
                return;
            }

            abp.message.confirm(
                app.localize('TimeEntryDeleteWarningMessage'),
                app.localize('AreYouSure'),
                function (confirmed) {
                    if (confirmed) {
                        _timeEntryService.delete({ id: id }).done(function () {
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                            loadPeriodEntries();
                        });
                    } else {
                        loadPeriodEntries();
                    }
                }
            );
        });

        scheduler.attachEvent('onClick', function (id) {
            var ev = scheduler.getEvent(id);
            if (!ev || !ev.isCompletedTask) {
                return true;
            }

            if (!ev.projectId) {
                abp.notify.warn(app.localize('SelectAProject'));
                return false;
            }

            abp.message.confirm(
                app.localize('AddTaskToCalendarConfirmationMessage'),
                app.localize('ConvertToTimeEntry'),
                function (confirmed) {
                    if (!confirmed) {
                        return;
                    }

                    _userTaskService.convertToTimeEntry({
                        taskId: ev.taskSourceId,
                        projectId: ev.projectId,
                        projectTaskId: ev.projectTaskId || null,
                        startTime: ev.start_date,
                        endTime: ev.end_date,
                        description: ev.description || ev.text
                    }).done(function () {
                        abp.notify.success(app.localize('SuccessfullySaved'));
                        loadPeriodEntries();
                    }).fail(function (error) {
                        var message = (error && error.responseJSON && error.responseJSON.error && error.responseJSON.error.message)
                            ? error.responseJSON.error.message
                            : app.localize('LoadError');
                        abp.notify.error(message);
                    });
                }
            );

            return false;
        });

        scheduler.attachEvent('onDblClick', function (id) {
            var ev = scheduler.getEvent(id);
            if (ev && ev.isCompletedTask) {
                return false;
            }

            _createOrEditModal.open({ id: id });
            return false;
        });

        scheduler.attachEvent('onEmptyClick', function (date) {
            var endDate = new Date(date.getTime() + 60 * 60 * 1000);
            _createOrEditModal.open({
                startTime: date.toISOString(),
                endTime: endDate.toISOString()
            });
            return false;
        });

        scheduler.init('myWeekScheduler', _currentDate, _currentMode);
    }

    function getCurrentRange() {
        var state = scheduler.getState();
        if (_currentMode === 'month') {
            return {
                startDate: getMonthStart(state.date || _currentDate),
                endDate: getMonthEnd(state.date || _currentDate)
            };
        }

        return {
            startDate: state.min_date ? new Date(state.min_date) : getWeekStart(_currentDate),
            endDate: state.max_date ? new Date(state.max_date) : getWeekEnd(getWeekStart(_currentDate))
        };
    }

    function toTimeEntryEvents(entries) {
        return $.map(entries, function (e) {
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
                description: e.description,
                lane: 'calendar',
                isCompletedTask: false
            };
        });
    }

    function toCompletedTaskEvents(tasks, range) {
        var results = [];

        $.each(tasks || [], function (_, task) {
            if (!task.completedAt) {
                return;
            }

            var completedAt = toSchedulerDate(task.completedAt);
            if (!isInRange(completedAt, range)) {
                return;
            }

            var startDate = new Date(completedAt);
            var endDate = new Date(completedAt);
            endDate.setMinutes(endDate.getMinutes() + 30);

            results.push({
                id: 'task-' + task.id,
                text: task.title || app.localize('TaskTitle'),
                start_date: startDate,
                end_date: endDate,
                color: '#95a5a6',
                projectId: task.projectId,
                projectName: task.projectName,
                projectTaskId: task.projectTaskId,
                taskName: task.title,
                description: task.description,
                lane: 'completedTasks',
                isCompletedTask: true,
                taskSourceId: task.id,
                readonly: true
            });
        });

        return results;
    }

    function loadPeriodEntries() {
        var range = getCurrentRange();

        _timeEntryService.getSchedulerEntries({
            forCurrentUserOnly: true,
            startDate: range.startDate,
            endDate: range.endDate
        }).done(function (entries) {
            _userTaskService.getTasks({
                maxResultCount: 1000,
                skipCount: 0,
                statusFilter: 3
            }).done(function (taskResult) {
                scheduler.clearAll();

                var timeEntryEvents = toTimeEntryEvents(entries || []);
                var completedTaskEvents = toCompletedTaskEvents(taskResult && taskResult.items ? taskResult.items : [], range);
                var events = timeEntryEvents.concat(completedTaskEvents);

                scheduler.parse(events, 'json');
                renderWeekSummary(timeEntryEvents);
            }).fail(function () {
                abp.notify.error(app.localize('LoadError'));
            });
        }).fail(function () {
            abp.notify.error(app.localize('LoadError'));
        });

        updatePeriodLabel(range);
    }

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

    function updatePeriodLabel(range) {
        if (_currentMode === 'month') {
            $('#WeekLabel').text(range.startDate.toLocaleDateString(undefined, { month: 'long', year: 'numeric' }));
            return;
        }

        $('#WeekLabel').text(formatDateLabel(range.startDate) + ' – ' + formatDateLabel(range.endDate));
    }

    $('#PrevWeekButton').click(function () {
        _currentDate = new Date(_currentDate);
        if (_currentMode === 'month') {
            _currentDate.setMonth(_currentDate.getMonth() - 1);
        } else {
            _currentDate.setDate(_currentDate.getDate() - 7);
        }
        scheduler.setCurrentView(_currentDate, _currentMode);
    });

    $('#NextWeekButton').click(function () {
        _currentDate = new Date(_currentDate);
        if (_currentMode === 'month') {
            _currentDate.setMonth(_currentDate.getMonth() + 1);
        } else {
            _currentDate.setDate(_currentDate.getDate() + 7);
        }
        scheduler.setCurrentView(_currentDate, _currentMode);
    });

    $('#TodayButton').click(function () {
        _currentDate = new Date();
        scheduler.setCurrentView(_currentDate, _currentMode);
    });

    $('#LogTimeButton').click(function () {
        var now = new Date();
        var end = new Date(now.getTime() + 60 * 60 * 1000);
        _createOrEditModal.open({
            startTime: now.toISOString(),
            endTime: end.toISOString()
        });
    });

    abp.event.on('app.createOrEditTimeEntryModalSaved', function () {
        loadPeriodEntries();
    });

    initScheduler();
    loadPeriodEntries();

    scheduler.attachEvent('onViewChange', function (new_mode, new_date) {
        _currentMode = new_mode || _currentMode;
        _currentDate = new_date ? new Date(new_date) : new Date();
        loadPeriodEntries();
    });
})();