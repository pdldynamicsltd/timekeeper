(function () {
    var _projectTaskService = abp.services.app.projectTask;
    var _timeEntryService = abp.services.app.timeEntry;
    var _projectService = abp.services.app.project;
    var _userTaskService = abp.services.app.userTask;

    var projectId = parseInt($('#CurrentProjectId').val());
    var _currentDate = new Date();
    var _currentMode = 'month';
    var _canCreateTimeEntries = abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Create');
    var _canDeleteTimeEntries = abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Delete');
    var _canEditTodos = abp.auth.isGranted('Pages.Tasks.Edit');

    var _createOrEditTaskModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditTaskModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditTaskModal.js',
        modalClass: 'CreateOrEditTaskModal'
    });

    var _createOrEditTimeEntryModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditTimeEntryModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditTimeEntryModal.js',
        modalClass: 'CreateOrEditTimeEntryModal'
    });

    var _createOrEditTodoModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Tasks/CreateOrEditUserTaskModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Tasks/_CreateOrEditUserTaskModal.js',
        modalClass: 'CreateOrEditUserTaskModal'
    });

    function toLocalDateTimeString(date) {
        return app.workspace.toLocalDateTimeString(date);
    }

    function openCreateTimeEntryModal(startDate, endDate) {
        var startTime = toLocalDateTimeString(startDate);
        var endTime = toLocalDateTimeString(endDate);

        if (!startTime || !endTime) {
            return;
        }

        _createOrEditTimeEntryModal.open({
            projectId: projectId,
            startTime: startTime,
            endTime: endTime
        });
    }

    function duplicateTimeEntryForNextDay(event) {
        if (!event || !event.dbId || !event.projectId) {
            return;
        }

        var duplicatedStart = new Date(event.start_date);
        var duplicatedEnd = new Date(event.end_date);
        duplicatedStart.setDate(duplicatedStart.getDate() + 1);
        duplicatedEnd.setDate(duplicatedEnd.getDate() + 1);

        _timeEntryService.create({
            projectId: event.projectId,
            taskId: event.taskId || null,
            startTime: toLocalDateTimeString(duplicatedStart),
            endTime: toLocalDateTimeString(duplicatedEnd),
            description: event.description || ''
        }).done(function () {
            abp.notify.success(app.localize('SavedSuccessfully'));
            loadSchedulerEntries();
            refreshBudget();
            loadTaskTable();
        });
    }

    function deleteTimeEntry(event) {
        if (!event || !event.dbId || !_canDeleteTimeEntries) {
            return;
        }

        abp.message.confirm(app.localize('TimeEntryDeleteWarningMessage'), app.localize('AreYouSure'), function (confirmed) {
            if (!confirmed) {
                return;
            }

            _timeEntryService.delete({ id: event.dbId }).done(function () {
                abp.notify.success(app.localize('SuccessfullyDeleted'));
                loadSchedulerEntries();
                refreshBudget();
                loadTaskTable();
            });
        });
    }

    function initScheduler() {
        scheduler.config.xml_date = '%Y-%m-%d %H:%i';
        scheduler.config.first_hour = 6;
        scheduler.config.last_hour = 22;
        scheduler.config.multi_day = true;
        scheduler.config.drag_create = true;
        scheduler.config.confirm_deleting = false;
        scheduler.config.details_on_create = true;
        scheduler.config.details_on_dblclick = true;
        scheduler.config.header = ['day', 'week', 'month', 'date', 'prev', 'today', 'next'];

        scheduler.locale.labels.dhx_cal_today_button = app.localize('Today');

        scheduler.templates.event_bar_text = function (start, end, event) {
            var duplicateButton = _canCreateTimeEntries && event.dbId
                ? '<span class="tt-event-duplicate js-duplicate-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Copy') + '"><i class="fa fa-copy"></i></span>'
                : '';
            var deleteButton = _canDeleteTimeEntries && event.dbId
                ? '<span class="tt-event-delete js-delete-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Delete') + '"><i class="fa fa-trash"></i></span>'
                : '';

            return '<span class="tt-event-title"><b>' + event.text + '</b></span>' + deleteButton + duplicateButton;
        };

        scheduler.templates.event_text = function (start, end, event) {
            var duplicateButton = _canCreateTimeEntries && event.dbId
                ? '<span class="tt-event-duplicate js-duplicate-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Copy') + '"><i class="fa fa-copy"></i></span>'
                : '';
            var deleteButton = _canDeleteTimeEntries && event.dbId
                ? '<span class="tt-event-delete js-delete-time-entry" data-event-id="' + event.id + '" title="' + app.localize('Delete') + '"><i class="fa fa-trash"></i></span>'
                : '';

            return '<span class="tt-event-title"><b>' + event.text + '</b></span>' + deleteButton + duplicateButton;
        };

        scheduler.templates.event_bar_date = function (start, end, event) {
            return scheduler.templates.event_date(start) + ' - ';
        };

        scheduler.attachEvent('onEventChanged', function (id, event) {
            if (!event.dbId) {
                return;
            }

            var startTime = scheduler.date.date_to_str('%Y-%m-%dT%H:%i')(event.start_date);
            var endTime = scheduler.date.date_to_str('%Y-%m-%dT%H:%i')(event.end_date);

            _timeEntryService.update({
                id: event.dbId,
                projectId: event.projectId,
                taskId: event.taskId,
                startTime: startTime,
                endTime: endTime,
                description: event.description || event.text
            }).done(function () {
                refreshBudget();
                loadTaskTable();
            });
        });

        scheduler.attachEvent('onClick', function (id, e) {
            var deleteButton = e ? $(e.target).closest('.js-delete-time-entry') : $();
            if (deleteButton.length) {
                var deleteEventId = deleteButton.attr('data-event-id') || id;
                deleteTimeEntry(scheduler.getEvent(deleteEventId));
                return false;
            }

            var duplicateButton = e ? $(e.target).closest('.js-duplicate-time-entry') : $();
            if (!duplicateButton.length) {
                return true;
            }

            var eventId = duplicateButton.attr('data-event-id') || id;
            var entryToDuplicate = scheduler.getEvent(eventId);
            duplicateTimeEntryForNextDay(entryToDuplicate);
            return false;
        });

        scheduler.attachEvent('onBeforeEventDelete', function (id, event) {
            if (!event.dbId) {
                return true;
            }

            deleteTimeEntry(event);

            return false;
        });

        scheduler.attachEvent('onBeforeLightbox', function (id) {
            var event = scheduler.getEvent(id);

            if (!event || event.dbId) {
                return true;
            }

            var startDate = event.start_date;
            var endDate = event.end_date;
            scheduler.deleteEvent(id);

            setTimeout(function () {
                openCreateTimeEntryModal(startDate, endDate);
            }, 0);

            return false;
        });

        scheduler.attachEvent('onDblClick', function (id) {
            var event = scheduler.getEvent(id);
            if (event && event.dbId) {
                _createOrEditTimeEntryModal.open({ id: event.dbId });
            }

            return false;
        });

        scheduler.attachEvent('onEmptyClick', function (date) {
            var times = app.workspace.defaultEntryTimes(date);
            openCreateTimeEntryModal(times.start, times.end);

            return false;
        });

        scheduler.attachEvent('onViewChange', function (newMode, newDate) {
            _currentMode = newMode || _currentMode;
            _currentDate = newDate ? new Date(newDate) : _currentDate;
            loadSchedulerEntries();
        });

        scheduler.init('scheduler_here', _currentDate, _currentMode);
        loadSchedulerEntries();
    }

    function loadSchedulerEntries() {
        var view = scheduler.getState();
        var input = {
            projectId: projectId,
            startDate: view.min_date ? toLocalDateTimeString(view.min_date) : null,
            endDate: view.max_date ? toLocalDateTimeString(view.max_date) : null
        };

        _timeEntryService.getSchedulerEntries(input).done(function (result) {
            scheduler.clearAll();

            var events = [];
            $.each(result || [], function (i, entry) {
                events.push({
                    id: 'tt_' + entry.id,
                    dbId: entry.id,
                    start_date: new Date(entry.startDate),
                    end_date: new Date(entry.endDate),
                    text: (entry.taskName || entry.projectName) + (entry.description ? ': ' + entry.description : ''),
                    color: entry.color || '#3498db',
                    projectId: entry.projectId,
                    taskId: entry.taskId,
                    description: entry.description
                });
            });

            scheduler.parse(events, 'json');
        });
    }

    function loadTaskTable() {
        _projectTaskService.getProjectTaskTree({ id: projectId }).done(function (tasks) {
            var container = $('#TaskTableBody');
            container.empty();

            if (!tasks || tasks.length === 0) {
                container.html('<tr><td colspan="6" class="text-muted text-center py-5">' + app.localize('NoTasks') + '</td></tr>');
                return;
            }

            $.each(flattenTasks(tasks, 0), function (i, row) {
                container.append(
                    '<tr>' +
                    '<td>' + row.indentedName + '</td>' +
                    '<td><span class="badge badge-light-' + row.statusClass + '">' + row.statusText + '</span></td>' +
                    '<td>' + row.budgetHours + '</td>' +
                    '<td>' + row.usedHours + '</td>' +
                    '<td class="' + row.remainingClass + '">' + row.remainingHours + '</td>' +
                    '<td class="text-end">' +
                    (abp.auth.isGranted('Pages.TimeTracking.Tasks.Edit')
                        ? '<button class="btn btn-xs btn-icon btn-light-primary edit-task-btn" data-id="' + row.id + '"><i class="ki-outline ki-pencil fs-5"></i></button>'
                        : '') +
                    '</td>' +
                    '</tr>'
                );
            });
        });
    }

    function flattenTasks(tasks, depth) {
        var rows = [];

        $.each(tasks, function (i, task) {
            var statusClass = task.status === 1 ? 'success' : task.status === 2 ? 'secondary' : 'primary';
            var statusText = task.status === 1 ? app.localize('ActiveStatus') : task.status === 2 ? app.localize('ArchivedStatus') : app.localize('CompletedStatus');

            rows.push({
                id: task.id,
                indentedName: '<span style="padding-left:' + (depth * 1.5) + 'rem" class="fw-semibold">' + escapeHtml(task.name) + '</span>',
                statusClass: statusClass,
                statusText: statusText,
                budgetHours: task.budgetHours
                    ? task.budgetHours.toFixed(1) + 'h <span class="text-muted fs-8">(' + (task.budgetHours / 8).toFixed(1) + 'd)</span>'
                    : '-',
                usedHours: (task.usedHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((task.usedHours || 0) / 8).toFixed(1) + 'd)</span>',
                remainingHours: (task.remainingHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((task.remainingHours || 0) / 8).toFixed(1) + 'd)</span>',
                remainingClass: (task.remainingHours || 0) < 0 ? 'text-danger' : ''
            });

            if (task.subTasks && task.subTasks.length) {
                rows = rows.concat(flattenTasks(task.subTasks, depth + 1));
            }
        });

        return rows;
    }

    function loadProjectTodos() {
        _userTaskService.getTasks({
            projectId: projectId,
            maxResultCount: 500
        }).done(function (result) {
            var items = result.items || [];
            var allTable = $('#ProjectTodosTableBody');
            var completedTable = $('#ProjectCompletedTodosTableBody');

            allTable.empty();
            completedTable.empty();

            if (!items.length) {
                allTable.append('<tr><td colspan="4" class="text-muted text-center py-4">' + app.localize('NoToDosFound') + '</td></tr>');
                completedTable.append('<tr><td colspan="3" class="text-muted text-center py-4">' + app.localize('NoCompletedToDosFound') + '</td></tr>');
                return;
            }

            $.each(items, function (i, todo) {
                var dueText = todo.dueDate ? moment(todo.dueDate).format('L') : '-';
                var actionButtons = '';

                if (_canEditTodos && !todo.completedAt) {
                    actionButtons += '<button class="btn btn-xs btn-icon btn-light-success me-1 todo-complete-btn" data-id="' + todo.id + '" title="' + app.localize('Complete') + '"><i class="fa fa-check"></i></button>';
                }

                if (_canEditTodos) {
                    actionButtons += '<button class="btn btn-xs btn-icon btn-light-primary todo-edit-btn" data-id="' + todo.id + '" title="' + app.localize('Edit') + '"><i class="ki-outline ki-pencil fs-5"></i></button>';
                }

                allTable.append(
                    '<tr>' +
                    '<td>' + escapeHtml(todo.title) + '</td>' +
                    '<td>' + escapeHtml(todo.statusName || '') + '</td>' +
                    '<td>' + dueText + '</td>' +
                    '<td class="text-end">' + actionButtons + '</td>' +
                    '</tr>'
                );

                if (todo.completedAt) {
                    var completedActions = _canEditTodos
                        ? '<button class="btn btn-xs btn-icon btn-light-primary todo-edit-btn" data-id="' + todo.id + '" title="' + app.localize('Edit') + '"><i class="ki-outline ki-pencil fs-5"></i></button>'
                        : '';

                    completedTable.append(
                        '<tr>' +
                        '<td>' + escapeHtml(todo.title) + '</td>' +
                        '<td>' + moment(todo.completedAt).format('L LT') + '</td>' +
                        '<td class="text-end">' + completedActions + '</td>' +
                        '</tr>'
                    );
                }
            });

            if (!completedTable.children().length) {
                completedTable.append('<tr><td colspan="3" class="text-muted text-center py-4">' + app.localize('NoCompletedToDosFound') + '</td></tr>');
            }
        });
    }

    function refreshBudget() {
        _projectService.getProjectBudgetSummary({ id: projectId }).done(function (summary) {
            var totalBudgetHours = summary.totalBudgetHours || 0;
            var usedHours = summary.usedHours || 0;
            var remainingHours = summary.remainingHours || 0;
            var percent = Math.min(Math.round(summary.utilizationPercentage || 0), 100);
            var cls = percent >= 90 ? 'danger' : percent >= 75 ? 'warning' : 'success';

            $('#BudgetTypeLabel').text(summary.budgetType || '');
            $('#TotalBudgetHoursLabel').text(totalBudgetHours.toFixed(1));
            $('#TotalBudgetDaysLabel').text((totalBudgetHours / 8).toFixed(1) + 'd');
            $('#UsedHoursLabel').text(usedHours.toFixed(1));
            $('#UsedDaysLabel').text((usedHours / 8).toFixed(1) + 'd');
            $('#RemainingHoursLabel').text(remainingHours.toFixed(1))
                .removeClass('text-success text-danger')
                .addClass(remainingHours < 0 ? 'text-danger' : 'text-success');
            $('#RemainingDaysLabel').text((remainingHours / 8).toFixed(1) + 'd');
            $('#BudgetProgressBar').css('width', percent + '%')
                .removeClass('bg-success bg-warning bg-danger')
                .addClass('bg-' + cls);

            $('#BudgetPercentLabel').text(percent + '%')
                .removeClass('text-success text-warning text-danger')
                .addClass('text-' + cls);
        });
    }

    function escapeHtml(text) {
        return app.workspace.escapeHtml(text);
    }

    $('#AddTimeEntryButton').click(function () {
        var times = app.workspace.defaultEntryTimes(new Date());
        _createOrEditTimeEntryModal.open({
            projectId: projectId,
            startTime: toLocalDateTimeString(times.start),
            endTime: toLocalDateTimeString(times.end)
        });
    });

    $('#AddTaskButton').click(function () {
        _createOrEditTaskModal.open({ projectId: projectId });
    });

    $('#AddTodoButton').click(function () {
        _createOrEditTodoModal.open({ projectId: projectId });
    });

    $('#AddTodoInlineButton').click(function () {
        _createOrEditTodoModal.open({ projectId: projectId });
    });

    $('#TaskTableBody').on('click', '.edit-task-btn', function () {
        _createOrEditTaskModal.open({ id: $(this).data('id') });
    });

    $('#ProjectTodosTableBody, #ProjectCompletedTodosTableBody').on('click', '.todo-edit-btn', function () {
        _createOrEditTodoModal.open({ id: $(this).data('id') });
    });

    $('#ProjectTodosTableBody').on('click', '.todo-complete-btn', function () {
        var todoId = $(this).data('id');
        _userTaskService.complete({ id: todoId }).done(function () {
            abp.notify.success(app.localize('SavedSuccessfully'));
            loadProjectTodos();
        });
    });

    abp.event.on('app.createOrEditTimeEntryModalSaved', function () {
        loadSchedulerEntries();
        refreshBudget();
        loadTaskTable();
    });

    abp.event.on('app.createOrEditTaskModalSaved', function () {
        loadTaskTable();
    });

    abp.event.on('app.createOrEditUserTaskModalSaved', function () {
        loadProjectTodos();
    });

    initScheduler();
    loadTaskTable();
    loadProjectTodos();
})();