(function () {
    var _timeEntryService = abp.services.app.timeEntry;
    var _userTaskService = abp.services.app.userTask;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditTimeEntryModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditTimeEntryModal.js',
        modalClass: 'CreateOrEditTimeEntryModal'
    });
    var _canCreateTimeEntries = abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Create');
    var _canDeleteTimeEntries = abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Delete');

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
        return app.workspace.toSchedulerDate(dt);
    }

    function toLocalDateTimeString(date) {
        return app.workspace.toLocalDateTimeString(date);
    }

    function escapeHtml(text) {
        return app.workspace.escapeHtml(text);
    }

    function openPrefilledTimeEntryModal(task) {
        var payload = {
            startTime: toLocalDateTimeString(task.startTime),
            endTime: toLocalDateTimeString(task.endTime),
            description: task.description || task.title || ''
        };

        if (task.projectId) {
            payload.projectId = task.projectId;
        }

        if (task.projectTaskId) {
            payload.taskId = task.projectTaskId;
        }

        _createOrEditModal.open(payload);
    }

        //abp.message.confirm(
        //    app.localize('AddTaskToCalendarConfirmationMessage'),
        //    app.localize('ConvertToTimeEntry'),
        //    function (confirmed) {
        //        if (!confirmed) {
        //            return;
        //        }

        //        _userTaskService.convertToTimeEntry({
        //            taskId: task.taskId,
        //            projectId: task.projectId,
        //            projectTaskId: task.projectTaskId || null,
        //            startTime: task.startTime,
        //            endTime: task.endTime,
        //            description: task.description || task.title
        //        }).done(function () {
        //            abp.notify.success(app.localize('SuccessfullySaved'));
        //            loadPeriodEntries();
        //        }).fail(function (error) {
        //            var message = (error && error.responseJSON && error.responseJSON.error && error.responseJSON.error.message)
        //                ? error.responseJSON.error.message
        //                : app.localize('LoadError');
        //            abp.notify.error(message);
        //        });
        //    }
        //);
  

    function isInRange(date, range) {
        var d = toSchedulerDate(date);
        return d >= range.startDate && d <= range.endDate;
    }

    function duplicateTimeEntryForNextDay(event) {
        if (!event || event.isCompletedTask || !event.projectId) {
            return;
        }

        var duplicatedStart = new Date(toSchedulerDate(event.start_date).getTime());
        var duplicatedEnd = new Date(toSchedulerDate(event.end_date).getTime());
        duplicatedStart.setDate(duplicatedStart.getDate() + 1);
        duplicatedEnd.setDate(duplicatedEnd.getDate() + 1);

        _timeEntryService.create({
            projectId: event.projectId,
            taskId: event.taskId || null,
            startTime: toLocalDateTimeString(duplicatedStart),
            endTime: toLocalDateTimeString(duplicatedEnd),
            description: event.description || event.text || ''
        }).done(function () {
            abp.notify.success(app.localize('SavedSuccessfully'));
            loadPeriodEntries();
        });
    }

    function deleteTimeEntry(eventId, event) {
        if (!_canDeleteTimeEntries || !event || event.isCompletedTask || eventId < 0) {
            return;
        }

        abp.message.confirm(
            app.localize('TimeEntryDeleteWarningMessage'),
            app.localize('AreYouSure'),
            function (confirmed) {
                if (!confirmed) {
                    return;
                }

                _timeEntryService.delete({ id: eventId }).done(function () {
                    abp.notify.success(app.localize('SuccessfullyDeleted'));
                    loadPeriodEntries();
                });
            }
        );
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
        scheduler.config.confirm_deleting = false;

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
            var duplicateButton = _canCreateTimeEntries && !event.isCompletedTask
                ? '<span class="tt-event-duplicate js-duplicate-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Copy') + '"><i class="fa fa-copy"></i></span>'
                : '';
            var deleteButton = _canDeleteTimeEntries && !event.isCompletedTask
                ? '<span class="tt-event-delete js-delete-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Delete') + '"><i class="fa fa-trash"></i></span>'
                : '';

            if (event.isCompletedTask) {
                return '<b>' + (event.taskName || event.text || app.localize('TaskTitle')) + '</b>' +
                    (event.projectName ? '<br/><small>' + event.projectName + '</small>' : '') +
                    '<br/><small>' + app.localize('ConvertToTimeEntry') + '</small>';
            }

            return '<span class="tt-event-title"><b>' + (event.projectName || '') + '</b>' +
                (event.taskName ? ' / ' + event.taskName : '') +
                (event.description ? '<br/><small>' + event.description + '</small>' : '') +
                '</span>' + deleteButton + duplicateButton;
        };

        scheduler.templates.event_text = function (start, end, event) {
            var duplicateButton = _canCreateTimeEntries && !event.isCompletedTask
                ? '<span class="tt-event-duplicate js-duplicate-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Copy') + '"><i class="fa fa-copy"></i></span>'
                : '';
            var deleteButton = _canDeleteTimeEntries && !event.isCompletedTask
                ? '<span class="tt-event-delete js-delete-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Delete') + '"><i class="fa fa-trash"></i></span>'
                : '';

            if (event.isCompletedTask) {
                return '<b>' + (event.taskName || event.text || app.localize('TaskTitle')) + '</b>' +
                    (event.projectName ? '<br/><small>' + event.projectName + '</small>' : '') +
                    '<br/><small>' + app.localize('ConvertToTimeEntry') + '</small>';
            }

            return '<span class="tt-event-title"><b>' + (event.projectName || '') + '</b>' +
                (event.taskName ? ' / ' + event.taskName : '') +
                (event.description ? '<br/><small>' + event.description + '</small>' : '') +
                '</span>' + deleteButton + duplicateButton;
        };

        scheduler.templates.event_class = function (start, end, event) {
            return event.isCompletedTask ? 'tt-completed-task' : 'tt-project-event';
        };

        scheduler.templates.event_bar_style = function (start, end, event) {
            if (event.isCompletedTask) {
                return 'background-color: #95a5a6;';
            }
            return event.color ? 'background-color: ' + event.color + ';' : 'background-color: #3498db;';
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
                startTime: toLocalDateTimeString(ev.start_date),
                endTime: toLocalDateTimeString(ev.end_date),
                description: ev.description
            }).done(function () {
                abp.notify.success(app.localize('SuccessfullySaved'));
                loadPeriodEntries();
            }).fail(function () {
                abp.notify.error(app.localize('LoadError'));
                loadPeriodEntries();
            });
        });

        scheduler.attachEvent('onBeforeEventDelete', function (id, ev) {
            if (ev && ev.isCompletedTask) {
                return false;
            }

            if (id < 0) {
                return true;
            }

            deleteTimeEntry(id, ev);
            return false;
        });

        scheduler.attachEvent('onEventAdded', function (id, ev) {
            if (!ev || ev.isCompletedTask || id >= 0) {
                return true;
            }

            _createOrEditModal.open({
                startTime: toLocalDateTimeString(ev.start_date),
                endTime: toLocalDateTimeString(ev.end_date)
            });

            scheduler.deleteEvent(id);
            return true;
        });

        scheduler.attachEvent('onClick', function (id, e) {
            var deleteButton = e ? $(e.target).closest('.js-delete-time-entry') : $();
            if (deleteButton.length) {
                var deleteEventId = parseInt(deleteButton.attr('data-event-id') || id);
                var eventToDelete = scheduler.getEvent(deleteEventId);
                deleteTimeEntry(deleteEventId, eventToDelete);
                return false;
            }

            var duplicateButton = e ? $(e.target).closest('.js-duplicate-time-entry') : $();
            if (duplicateButton.length) {
                var eventId = duplicateButton.attr('data-event-id') || id;
                var eventToDuplicate = scheduler.getEvent(eventId);
                duplicateTimeEntryForNextDay(eventToDuplicate);
                return false;
            }

            var ev = scheduler.getEvent(id);
            if (!ev || !ev.isCompletedTask) {
                return true;
            }

            openPrefilledTimeEntryModal({

                projectId: ev.projectId,
                projectTaskId: ev.projectTaskId,
                startTime: ev.start_date,
                endTime: ev.end_date,
                description: ev.description,
                title: ev.taskName || ev.text
            });

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

        scheduler.attachEvent('onEmptyClick', function (date, evt) {
            var startDate = date;
            
            // In units view, we need to extract the actual time from the event position
            if (_currentMode === 'ttUnits' && evt) {
                var target = evt.target || evt.srcElement;
                var container = scheduler.config.container || 'myWeekScheduler';
                var rect = target.getBoundingClientRect();
                var containerRect = document.getElementById(container).getBoundingClientRect();
                
                // Get the position within the container
                var relativeY = rect.top - containerRect.top;
                
                // Calculate the time based on hour_size_px and first_hour
                var hourSize = scheduler.config.hour_size_px || 44;
                var firstHour = scheduler.config.first_hour || 0;
                var timeStep = scheduler.config.time_step || 15;
                
                // Calculate hours and minutes from the click position
                var totalMinutes = Math.round((relativeY / hourSize) * 60);
                var steps = Math.floor(totalMinutes / timeStep);
                totalMinutes = steps * timeStep;
                
                var clickDate = new Date(startDate);
                clickDate.setHours(firstHour + Math.floor(totalMinutes / 60), totalMinutes % 60, 0, 0);
                startDate = clickDate;
            }
            
            var endDate = new Date(startDate.getTime() + 60 * 60 * 1000);
            _createOrEditModal.open({
                startTime: toLocalDateTimeString(startDate),
                endTime: toLocalDateTimeString(endDate)
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

    function renderCompletedTasksList(tasks, range) {
        var container = $("#CompletedTasksList");
        if (!container.length) {
            return;
        }

        container.empty();

        var completedTasks = $.grep(tasks || [], function (task) {
            return task.completedAt && isInRange(task.completedAt, range);
        });

        if (!completedTasks.length) {
            container.html('<div class="text-muted fs-7">' + app.localize('NoCompletedTasksInPeriod') + '</div>');
            return;
        }

        $.each(completedTasks, function (_, task) {
            var completedAt = toSchedulerDate(task.completedAt);
            var endDate = new Date(completedAt);
            endDate.setMinutes(endDate.getMinutes() + 30);
            var meta = (task.projectName ? escapeHtml(task.projectName) + ' - ' : '') + completedAt.toLocaleString();

            var itemHtml = '<div class="completed-task-list-item">' +
                '<div>' +
                '<div class="completed-task-list-title">' + escapeHtml(task.title || app.localize('TaskTitle')) + '</div>' +
                '<div class="completed-task-list-meta">' + meta + '</div>' +
                '</div>' +
                '<button type="button" class="btn btn-sm btn-primary js-convert-completed-task"' +
                ' data-task-id="' + task.id + '"' +
                ' data-project-id="' + (task.projectId || '') + '"' +
                ' data-project-task-id="' + (task.projectTaskId || '') + '"' +
                ' data-start-time="' + toLocalDateTimeString(completedAt) + '"' +
                ' data-end-time="' + toLocalDateTimeString(endDate) + '"' +
                ' data-title="' + escapeHtml(task.title || '') + '"' +
                ' data-description="' + escapeHtml(task.description || '') + '">' +
                app.localize('ConvertToTimeEntry') +
                '</button>' +
                '</div>';

            container.append(itemHtml);
        });
    }

    function loadPeriodEntries() {
        var range = getCurrentRange();

        _timeEntryService.getSchedulerEntries({
            forCurrentUserOnly: true,
            startDate: toLocalDateTimeString(range.startDate),
            endDate: toLocalDateTimeString(range.endDate)
        }).done(function (entries) {
            _userTaskService.getTasks({
                maxResultCount: 1000,
                skipCount: 0,
                statusFilter: 3
            }).done(function (taskResult) {
                scheduler.clearAll();

                var completedTasks = taskResult && taskResult.items ? taskResult.items : [];
                var timeEntryEvents = toTimeEntryEvents(entries || []);
                var completedTaskEvents = toCompletedTaskEvents(completedTasks, range);
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
            startTime: toLocalDateTimeString(now),
            endTime: toLocalDateTimeString(end)
        });
    });

    $('#CompletedTasksList').on('click', '.js-convert-completed-task', function () {
        var button = $(this);
        var taskId = parseInt(button.attr('data-task-id'), 10);
        var projectIdRaw = button.attr('data-project-id');
        var projectTaskIdRaw = button.attr('data-project-task-id');

        openPrefilledTimeEntryModal({
            taskId: taskId,
            projectId: projectIdRaw ? parseInt(projectIdRaw, 10) : null,
            projectTaskId: projectTaskIdRaw ? parseInt(projectTaskIdRaw, 10) : null,
            startTime: button.attr('data-start-time'),
            endTime: button.attr('data-end-time'),
            description: button.attr('data-description'),
            title: button.attr('data-title')
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
