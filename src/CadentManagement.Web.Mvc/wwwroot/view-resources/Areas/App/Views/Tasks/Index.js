(function () {
    var _userTaskService = abp.services.app.userTask;
    var _projectService = abp.services.app.project;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Tasks/CreateOrEditUserTaskModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Tasks/_CreateOrEditUserTaskModal.js',
        modalClass: 'CreateOrEditUserTaskModal'
    });

    var _statuses = [];
    var _priorities = [
        { value: 0, label: app.localize('LowPriority'), className: 'badge-priority-low' },
        { value: 1, label: app.localize('MediumPriority'), className: 'badge-priority-medium' },
        { value: 2, label: app.localize('HighPriority'), className: 'badge-priority-high' },
        { value: 3, label: app.localize('UrgentPriority'), className: 'badge-priority-urgent' }
    ];
    var _sortableInstances = {};

    function getPriorityLabel(priorityValue) {
        var priority = _priorities.find(function (p) { return p.value === priorityValue; });
        return priority ? priority.label : priorityValue;
    }

    function getPriorityClass(priorityValue) {
        var priority = _priorities.find(function (p) { return p.value === priorityValue; });
        return priority ? priority.className : '';
    }

    function loadStatuses(callback) {
        _userTaskService.getTodoStatuses().done(function (statuses) {
            _statuses = (statuses || []).sort(function (a, b) { return a.sortOrder - b.sortOrder; });
            if (callback) {
                callback();
            }
        });
    }

    function renderKanban(tasks) {
        var board = $('#KanbanBoard');
        board.empty();

        _statuses.forEach(function (status) {
            var statusTasks = tasks.filter(function (t) { return t.status === status.value; });
            var columnHtml = '<div class="kanban-column">' +
                '<div class="kanban-column-header">' +
                '<span><span class="status-color-dot" style="background:' + escapeHtml(status.color || '#e5e7eb') + ';"></span>' + escapeHtml(status.name) + '</span>' +
                '<span class="kanban-column-count">' + statusTasks.length + '</span>' +
                '</div>' +
                '<ul class="kanban-list" data-status-value="' + status.value + '">';

            statusTasks.forEach(function (task) {
                var description = task.description ? escapeHtml(task.description.substring(0, 100)) : '';
                columnHtml += '<li class="kanban-card" data-id="' + task.id + '">' +
                    '<div class="kanban-card-title">' + escapeHtml(task.title) + '</div>' +
                    '<div class="kanban-card-meta">' +
                    (task.projectName ? '<span class="badge badge-secondary">' + escapeHtml(task.projectName) + '</span>' : '') +
                    '<span class="badge ' + getPriorityClass(task.priority) + '">' + getPriorityLabel(task.priority) + '</span>' +
                    '</div>' +
                    (description ? '<div class="kanban-card-description" style="font-size:12px;color:#666;margin-bottom:8px;">' + description + '</div>' : '') +
                    '<div class="kanban-card-actions">' +
                    '<button class="kanban-card-btn edit-btn" title="' + app.localize('Edit') + '"><i class="fa fa-edit"></i></button>' +
                    '<button class="kanban-card-btn delete-btn" title="' + app.localize('Delete') + '"><i class="fa fa-trash"></i></button>' +
                    '</div>' +
                    '</li>';
            });

            columnHtml += '</ul>' +
                '<button class="kanban-add-button" data-status-value="' + status.value + '">' +
                '<i class="fa fa-plus"></i> ' + app.localize('AddToDo') +
                '</button>' +
                '</div>';

            board.append(columnHtml);
        });

        _statuses.forEach(function (status) {
            var list = board.find('[data-status-value="' + status.value + '"]');
            var key = status.value.toString();

            if (_sortableInstances[key]) {
                _sortableInstances[key].destroy();
            }

            _sortableInstances[key] = Sortable.create(list[0], {
                group: 'kanban',
                animation: 150,
                ghostClass: 'sortable-ghost',
                onEnd: function (evt) {
                    var taskId = parseInt($(evt.item).attr('data-id'));
                    var newStatusValue = parseInt($(evt.to).attr('data-status-value'));
                    var newSortOrder = $(evt.to).find('.kanban-card').index(evt.item);

                    _userTaskService.updateStatus({
                        taskId: taskId,
                        newStatus: newStatusValue,
                        newSortOrder: newSortOrder
                    }).done(loadTasks);
                }
            });
        });

        attachHandlers();
    }

    function attachHandlers() {
        var board = $('#KanbanBoard');

        board.off('click', '.edit-btn').on('click', '.edit-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.kanban-card').attr('data-id'));
            _createOrEditModal.open({ id: taskId });
        });

        board.off('click', '.delete-btn').on('click', '.delete-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.kanban-card').attr('data-id'));

            abp.message.confirm(app.localize('TaskDeleteWarningMessage'), app.localize('AreYouSure'), function (confirmed) {
                if (!confirmed) {
                    return;
                }

                _userTaskService.delete({ id: taskId }).done(function () {
                    abp.notify.success(app.localize('SuccessfullyDeleted'));
                    loadTasks();
                });
            });
        });

        board.off('click', '.kanban-add-button').on('click', '.kanban-add-button', function (e) {
            e.preventDefault();
            var statusValue = parseInt($(this).data('status-value'));
            _createOrEditModal.open({ status: statusValue });
        });
    }

    function loadTasks() {
        var selectedProjectId = $('#KanbanProjectFilter').val();
        _userTaskService.getTasks({
            maxResultCount: 500,
            projectId: selectedProjectId ? parseInt(selectedProjectId) : null
        }).done(function (response) {
            renderKanban(response.items || []);
        });
    }

    function loadProjectFilterOptions(callback) {
        _projectService.getProjects({
            maxResultCount: 500,
            skipCount: 0
        }).done(function (result) {
            var select = $('#KanbanProjectFilter');
            var currentValue = select.val();
            select.find('option:not([value=""])').remove();

            (result.items || []).forEach(function (project) {
                select.append('<option value="' + project.id + '">' + escapeHtml(project.name) + '</option>');
            });

            if (currentValue) {
                select.val(currentValue);
            }

            if (callback) {
                callback();
            }
        });
    }

    function escapeHtml(text) {
        if (text === undefined || text === null) {
            return '';
        }

        return text.toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    $('#AddTaskButton').click(function () {
        _createOrEditModal.open({});
    });

    $('#KanbanProjectFilter').on('change', function () {
        loadTasks();
    });

    loadProjectFilterOptions(function () {
        loadStatuses(loadTasks);
    });

    abp.event.on('app.createOrEditUserTaskModalSaved', function () {
        loadTasks();
    });
})();
