(function () {
    var _projectTaskService = abp.services.app.projectTask;
    var _timeEntryService = abp.services.app.timeEntry;
    var _projectService = abp.services.app.project;

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

    // DHTMLX Scheduler
    function initScheduler() {
        scheduler.config.xml_date = "%Y-%m-%d %H:%i";
        scheduler.config.first_hour = 6;
        scheduler.config.last_hour = 22;
        scheduler.config.multi_day = true;
        scheduler.config.details_on_create = true;
        scheduler.config.details_on_dblclick = true;
        scheduler.config.header = ['day', 'week', 'month', 'date', 'prev', 'today', 'next'];

        scheduler.locale.labels.dhx_cal_today_button = app.localize('Today');

        scheduler.templates.event_bar_text = function (start, end, event) {
            return '<b>' + event.text + '</b>';
        };

        scheduler.templates.event_class = function (start, end, event) {
            return event.color ? '' : '';
        };

        scheduler.templates.event_bar_date = function (start, end, event) {
            return scheduler.templates.event_date(start) + ' - ';
        };

        scheduler.attachEvent('onEventAdded', function (id, event) {
            if (!event.is_new) return;
            _createOrEditTimeEntryModal.open({ projectId: projectId });
            scheduler.deleteEvent(id);
        });

        scheduler.attachEvent('onEventChanged', function (id, event) {
            if (!event.dbId) return;
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
            if (!event.dbId) return true;
            abp.message.confirm(
                app.localize('TimeEntryDeleteWarningMessage'),
                app.localize('AreYouSure'),
                function (confirmed) {
                    if (confirmed) {
                        _timeEntryService.delete({ id: event.dbId }).done(function () {
                            scheduler.deleteEvent(id);
                            refreshBudget();
                        });
                    }
                }
            );
            return false;
        });

        scheduler.attachEvent('onDblClick', function (id, e) {
            var event = scheduler.getEvent(id);
            if (event && event.dbId) {
                _createOrEditTimeEntryModal.open({ id: event.dbId });
            }
            return false;
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
            $.each(result, function (i, entry) {
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

    // Task tree
    function loadTaskTree() {
        _projectTaskService.getProjectTaskTree({ id: projectId }).done(function (tasks) {
            var container = $('#TaskTreeContainer');
            container.empty();

            if (!tasks || tasks.length === 0) {
                container.html('<div class="text-muted text-center py-5">' + app.localize('NoTasks') + '</div>');
                return;
            }

            var ul = buildTreeHtml(tasks, 0);
            container.append(ul);
        });
    }

    function buildTreeHtml(tasks, depth) {
        var ul = $('<ul class="list-unstyled' + (depth > 0 ? ' ms-5' : '') + ' mb-0">');
        $.each(tasks, function (i, task) {
            var statusClass = task.status === 1 ? 'success' : task.status === 2 ? 'secondary' : 'primary';
            var li = $('<li class="d-flex align-items-center py-2 border-bottom">' +
                '<i class="ki-outline ki-check-circle fs-4 text-' + statusClass + ' me-2"></i>' +
                '<span class="flex-grow-1 fw-semibold">' + task.name + '</span>' +
                (task.budgetHours ? '<span class="badge badge-light-info ms-2">' + task.budgetHours.toFixed(1) + 'h</span>' : '') +
                (abp.auth.isGranted('Pages.TimeTracking.Tasks.Edit')
                    ? '<button class="btn btn-xs btn-icon btn-light-primary ms-1 edit-task-btn" data-id="' + task.id + '"><i class="ki-outline ki-pencil fs-5"></i></button>'
                    : '') +
                '</li>');
            ul.append(li);
            if (task.subTasks && task.subTasks.length) {
                ul.append(buildTreeHtml(task.subTasks, depth + 1));
            }
        });
        return ul;
    }

    function refreshBudget() {
        _projectService.getProjectBudgetSummary({ id: projectId }).done(function (summary) {
            var percent = Math.min(Math.round(summary.utilizationPercentage), 100);
            var cls = percent >= 90 ? 'danger' : percent >= 75 ? 'warning' : 'success';
            $('#BudgetProgressBar').css('width', percent + '%')
                .removeClass('bg-success bg-warning bg-danger').addClass('bg-' + cls);
            $('#BudgetPercentLabel').text(percent + '%').removeClass('text-success text-warning text-danger').addClass('text-' + cls);
        });
    }

    $('#AddTimeEntryButton').click(function () {
        _createOrEditTimeEntryModal.open({ projectId: projectId });
    });

    $('#AddTaskButton').click(function () {
        _createOrEditTaskModal.open({ projectId: projectId });
    });

    $('#TaskTreeContainer').on('click', '.edit-task-btn', function () {
        _createOrEditTaskModal.open({ id: $(this).data('id') });
    });

    abp.event.on('app.createOrEditTimeEntryModalSaved', function () {
        loadSchedulerEntries();
        refreshBudget();
    });

    abp.event.on('app.createOrEditTaskModalSaved', function () {
        loadTaskTree();
    });

    scheduler.attachEvent('onViewChange', function (new_mode, new_date) {
        _currentMode = new_mode || _currentMode;
        _currentDate = new_date ? new Date(new_date) : _currentDate;
        loadSchedulerEntries();
    });

    initScheduler();
    loadTaskTree();
})();
