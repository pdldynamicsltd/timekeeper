(function () {
    var _projectTaskService = abp.services.app.projectTask;
    var _timeEntryService = abp.services.app.timeEntry;
    var _projectService = abp.services.app.project;
    var _userTaskService = abp.services.app.userTask;

    var projectId = parseInt($('#CurrentProjectId').val());
    var _currentDate = new Date();
    var _currentMode = 'month';

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

    function initScheduler() {
        scheduler.config.xml_date = '%Y-%m-%d %H:%i';
        scheduler.config.first_hour = 6;
        scheduler.config.last_hour = 22;
        scheduler.config.multi_day = true;
        scheduler.config.details_on_create = false;
        scheduler.config.details_on_dblclick = true;
        scheduler.config.header = ['day', 'week', 'month', 'date', 'prev', 'today', 'next'];

        scheduler.locale.labels.dhx_cal_today_button = app.localize('Today');

        scheduler.templates.event_bar_text = function (start, end, event) {
            return '<b>' + event.text + '</b>';
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
                description: event.text
            }).done(function () {
                refreshBudget();
            });
        });

        scheduler.attachEvent('onBeforeEventDelete', function (id, event) {
            if (!event.dbId) {
                return true;
            }

            abp.message.confirm(app.localize('TimeEntryDeleteWarningMessage'), app.localize('AreYouSure'), function (confirmed) {
                if (!confirmed) {
                    return;
                }

                _timeEntryService.delete({ id: event.dbId }).done(function () {
                    scheduler.deleteEvent(id);
                    refreshBudget();
                });
            });

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
            var endDate = new Date(date.getTime() + 60 * 60 * 1000);
            _createOrEditTimeEntryModal.open({
                projectId: projectId,
                startTime: date.toISOString(),
                endTime: endDate.toISOString()
            });

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
            startDate: view.min_date ? view.min_date.toISOString() : null,
            endDate: view.max_date ? view.max_date.toISOString() : null
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
                    taskId: entry.taskId
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
                budgetHours: task.budgetHours ? task.budgetHours.toFixed(1) + 'h' : '-',
                usedHours: (task.usedHours || 0).toFixed(1) + 'h',
                remainingHours: (task.remainingHours || 0).toFixed(1) + 'h',
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
                allTable.append('<tr><td colspan="3" class="text-muted text-center py-4">' + app.localize('NoToDosFound') + '</td></tr>');
                completedTable.append('<tr><td colspan="2" class="text-muted text-center py-4">' + app.localize('NoCompletedToDosFound') + '</td></tr>');
                return;
            }

            $.each(items, function (i, todo) {
                var dueText = todo.dueDate ? moment(todo.dueDate).format('L') : '-';
                allTable.append(
                    '<tr class="project-todo-row" data-id="' + todo.id + '">' +
                    '<td>' + escapeHtml(todo.title) + '</td>' +
                    '<td>' + escapeHtml(todo.statusName || '') + '</td>' +
                    '<td>' + dueText + '</td>' +
                    '</tr>'
                );

                if (todo.completedAt) {
                    completedTable.append(
                        '<tr class="project-todo-row" data-id="' + todo.id + '">' +
                        '<td>' + escapeHtml(todo.title) + '</td>' +
                        '<td>' + moment(todo.completedAt).format('L LT') + '</td>' +
                        '</tr>'
                    );
                }
            });

            if (!completedTable.children().length) {
                completedTable.append('<tr><td colspan="2" class="text-muted text-center py-4">' + app.localize('NoCompletedToDosFound') + '</td></tr>');
            }
        });
    }

    function refreshBudget() {
        _projectService.getProjectBudgetSummary({ id: projectId }).done(function (summary) {
            var percent = Math.min(Math.round(summary.utilizationPercentage), 100);
            var cls = percent >= 90 ? 'danger' : percent >= 75 ? 'warning' : 'success';

            $('#BudgetProgressBar').css('width', percent + '%')
                .removeClass('bg-success bg-warning bg-danger')
                .addClass('bg-' + cls);

            $('#BudgetPercentLabel').text(percent + '%')
                .removeClass('text-success text-warning text-danger')
                .addClass('text-' + cls);
        });
    }

    function escapeHtml(text) {
        if (!text) {
            return '';
        }

        return text.toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    $('#AddTimeEntryButton').click(function () {
        _createOrEditTimeEntryModal.open({ projectId: projectId });
    });

    $('#AddTaskButton').click(function () {
        _createOrEditTaskModal.open({ projectId: projectId });
    });

    $('#AddTodoButton').click(function () {
        _createOrEditTodoModal.open({ projectId: projectId });
    });

    $('#TaskTableBody').on('click', '.edit-task-btn', function () {
        _createOrEditTaskModal.open({ id: $(this).data('id') });
    });

    $('#ProjectTodosTableBody, #ProjectCompletedTodosTableBody').on('click', '.project-todo-row', function () {
        _createOrEditTodoModal.open({ id: $(this).data('id') });
    });

    abp.event.on('app.createOrEditTimeEntryModalSaved', function () {
        loadSchedulerEntries();
        refreshBudget();
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