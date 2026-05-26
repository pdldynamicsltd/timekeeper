(function () {
    var _userTaskService = abp.services.app.userTask;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Tasks/CreateOrEditUserTaskModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Tasks/_CreateOrEditUserTaskModal.js',
        modalClass: 'CreateOrEditUserTaskModal'
    });

    var _statuses = [
        { key: 'Backlog', value: 0, label: app.localize('BacklogStatus') },
        { key: 'Todo', value: 1, label: app.localize('TodoStatus') },
        { key: 'InProgress', value: 2, label: app.localize('InProgressStatus') },
        { key: 'Done', value: 3, label: app.localize('DoneStatus') }
    ];

    var _priorities = [
        { key: 'Low', value: 0, label: app.localize('LowPriority'), className: 'badge-priority-low' },
        { key: 'Medium', value: 1, label: app.localize('MediumPriority'), className: 'badge-priority-medium' },
        { key: 'High', value: 2, label: app.localize('HighPriority'), className: 'badge-priority-high' },
        { key: 'Urgent', value: 3, label: app.localize('UrgentPriority'), className: 'badge-priority-urgent' }
    ];

    var _sortableInstances = {};

    function getPriorityLabel(priorityValue) {
        var priority = _priorities.find(p => p.value === priorityValue);
        return priority ? priority.label : priorityValue;
    }

    function getPriorityClass(priorityValue) {
        var priority = _priorities.find(p => p.value === priorityValue);
        return priority ? priority.className : '';
    }

    function renderKanban(tasks) {
        var board = $('#KanbanBoard');
        board.empty();

        _statuses.forEach(function (status) {
            var statusTasks = tasks.filter(t => t.status === status.value);
            var columnHtml = '<div class="kanban-column">' +
                '<div class="kanban-column-header">' +
                '<span>' + status.label + '</span>' +
                '<span class="kanban-column-count">' + statusTasks.length + '</span>' +
                '</div>' +
                '<ul class="kanban-list" data-status="' + status.key + '">';

            statusTasks.forEach(function (task) {
                var priorityClass = getPriorityClass(task.priority);
                columnHtml += '<li class="kanban-card" data-id="' + task.id + '">' +
                    '<div class="kanban-card-title">' + escapeHtml(task.title) + '</div>' +
                    '<div class="kanban-card-meta">' +
                    (task.projectName ? '<span class="badge badge-secondary">' + escapeHtml(task.projectName) + '</span>' : '') +
                    '<span class="badge ' + priorityClass + '">' + getPriorityLabel(task.priority) + '</span>' +
                    '</div>';

                if (task.description) {
                    columnHtml += '<div class="kanban-card-description" style="font-size: 12px; color: #666; margin-bottom: 8px;">' + escapeHtml(task.description.substring(0, 100)) + '...</div>';
                }

                columnHtml += '<div class="kanban-card-actions">' +
                    '<button class="kanban-card-btn edit-btn" title="' + app.localize('Edit') + '"><i class="fa fa-edit"></i></button>' +
                    '<button class="kanban-card-btn delete-btn" title="' + app.localize('Delete') + '"><i class="fa fa-trash"></i></button>' +
                    (status.key === 'Done' ? '<button class="kanban-card-btn convert-btn" title="' + app.localize('ConvertToTimeEntry') + '"><i class="fa fa-clock"></i></button>' : '') +
                    '</div>' +
                    '</li>';
            });

            columnHtml += '</ul>' +
                '<button class="kanban-add-button" data-status="' + status.key + '">' +
                '<i class="fa fa-plus"></i> ' + app.localize('AddTask') +
                '</button>' +
                '</div>';

            board.append(columnHtml);
        });

        // Setup Sortable for each list
        _statuses.forEach(function (status) {
            var list = board.find('[data-status="' + status.key + '"]');
            if (_sortableInstances[status.key]) {
                _sortableInstances[status.key].destroy();
            }

            _sortableInstances[status.key] = Sortable.create(list[0], {
                group: 'kanban',
                animation: 150,
                ghostClass: 'sortable-ghost',
                onEnd: function (evt) {
                    var taskId = parseInt($(evt.item).attr('data-id'));
                    var newStatus = $(evt.to).attr('data-status');
                    var newSortOrder = $(evt.to).find('.kanban-card').index(evt.item);

                    var statusValue = _statuses.find(s => s.key === newStatus).value;
                    _userTaskService.updateStatus({
                        taskId: taskId,
                        newStatus: statusValue,
                        newSortOrder: newSortOrder
                    }).done(function () {
                        loadTasks();
                    }).fail(function () {
                        abp.notify.error(app.localize('Error'));
                        loadTasks();
                    });
                }
            });
        });

        // Attach event handlers
        attachEventHandlers();
    }

    function attachEventHandlers() {
        var board = $('#KanbanBoard');

        board.off('click', '.edit-btn').on('click', '.edit-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.kanban-card').attr('data-id'));
            _createOrEditModal.open({ id: taskId });
        });

        board.off('click', '.delete-btn').on('click', '.delete-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.kanban-card').attr('data-id'));
            abp.message.confirm(
                app.localize('TaskDeleteWarningMessage'),
                app.localize('AreYouSure'),
                function (confirmed) {
                    if (confirmed) {
                        _userTaskService.delete({ id: taskId }).done(function () {
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                            loadTasks();
                        });
                    }
                }
            );
        });

        board.off('click', '.convert-btn').on('click', '.convert-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.kanban-card').attr('data-id'));
            alert('Convert to time entry: ' + taskId); // TODO: Implement modal
        });

        board.off('click', '.kanban-add-button').on('click', '.kanban-add-button', function (e) {
            e.preventDefault();
            _createOrEditModal.open({});
        });
    }

    function loadTasks() {
        _userTaskService.getTasks({
            maxResultCount: 500
        }).done(function (response) {
            renderKanban(response.items);
        }).fail(function () {
            abp.notify.error(app.localize('Error loading tasks'));
        });
    }

    function escapeHtml(text) {
        if (!text) return '';
        return text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    // Init
    $('#AddTaskButton').click(function () {
        _createOrEditModal.open({});
    });

    loadTasks();

    abp.event.on('app.createOrEditUserTaskModalSaved', function () {
        loadTasks();
    });
})();
