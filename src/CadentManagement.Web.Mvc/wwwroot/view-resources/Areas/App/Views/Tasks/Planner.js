(function () {
    var _userTaskService = abp.services.app.userTask;
    var _projectService = abp.services.app.project;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Tasks/CreateOrEditUserTaskModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Tasks/_CreateOrEditUserTaskModal.js',
        modalClass: 'CreateOrEditUserTaskModal'
    });

    var _statuses = [];
    var _completedStatusValues = [];
    var _sortableInstances = {};
    var _daysToShow = 14;
    var _weekOffset = 0;
    var _searchDebounceHandle = null;

    function getStartOfDay(date) {
        var value = new Date(date);
        value.setHours(0, 0, 0, 0);
        return value;
    }

    function pad2(value) {
        return value < 10 ? '0' + value : value.toString();
    }

    function getDateKey(date) {
        var d = getStartOfDay(date);
        return d.getFullYear() + '-' + pad2(d.getMonth() + 1) + '-' + pad2(d.getDate());
    }

    function parseDateKey(dateKey) {
        var parts = dateKey.split('-');
        return new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
    }

    function getDateColumnsStart() {
        var today = getStartOfDay(new Date());
        today.setDate(today.getDate() + (_weekOffset * 7));
        return today;
    }

    function getColumns() {
        var startDate = getDateColumnsStart();
        var columns = [{ key: 'overdue', title: app.localize('Overdue'), date: null, isOverdue: true }];

        for (var i = 0; i < _daysToShow; i++) {
            var day = new Date(startDate);
            day.setDate(startDate.getDate() + i);
            columns.push({
                key: getDateKey(day),
                title: day.toLocaleDateString(undefined, { weekday: 'short', day: 'numeric', month: 'short' }),
                date: day,
                isOverdue: false
            });
        }

        return columns;
    }

    function updateRangeLabel(columns) {
        var firstDateColumn = columns.find(function (x) { return !x.isOverdue; });
        var lastDateColumn = columns.slice().reverse().find(function (x) { return !x.isOverdue; });

        if (!firstDateColumn || !lastDateColumn) {
            $('#PlannerRangeLabel').text('');
            return;
        }

        var firstText = firstDateColumn.date.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
        var lastText = lastDateColumn.date.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
        $('#PlannerRangeLabel').text(firstText + ' - ' + lastText);
    }

    function loadStatuses(callback) {
        _userTaskService.getTodoStatuses().done(function (statuses) {
            _statuses = statuses || [];
            _completedStatusValues = _statuses
                .filter(function (s) { return s.isCompleted; })
                .map(function (s) { return s.value; });

            if (callback) {
                callback();
            }
        });
    }

    function getColumnKey(task) {
        if (!task.dueDate) {
            return 'overdue';
        }

        var due = getStartOfDay(task.dueDate);
        var today = getStartOfDay(new Date());

        if (due < today) {
            return 'overdue';
        }

        return getDateKey(due);
    }

    function render(tasks) {
        var columns = getColumns();
        updateRangeLabel(columns);

        var board = $('#TodoPlannerBoard');
        board.empty();

        var filteredTasks = (tasks || []).filter(function (task) {
            return isShowingCompleted() || _completedStatusValues.indexOf(task.status) === -1;
        });

        columns.forEach(function (column) {
            var columnTasks = filteredTasks.filter(function (task) {
                return getColumnKey(task) === column.key;
            });

            var html = '<div class="todo-planner-column" data-column-key="' + column.key + '">' +
                '<div class="todo-planner-column-header">' +
                '<span>' + escapeHtml(column.title) + '</span>' +
                '<span class="todo-planner-column-count">' + columnTasks.length + '</span>' +
                '</div>' +
                '<ul class="todo-planner-list">';

            columnTasks.forEach(function (task) {
                html += '<li class="todo-planner-card" data-id="' + task.id + '">' +
                    '<div class="todo-planner-card-title">' + escapeHtml(task.title) + '</div>' +
                    '<div class="todo-planner-card-meta">' + (task.projectName ? escapeHtml(task.projectName) : app.localize('NoProject')) + '</div>' +
                    '<div class="todo-planner-actions">' +
                    '<button class="todo-planner-btn todo-planner-btn-complete complete-btn"><i class="fa fa-check"></i> ' + app.localize('Complete') + '</button>' +
                    '<button class="todo-planner-btn edit-btn"><i class="fa fa-edit"></i></button>' +
                    '</div>' +
                    '</li>';
            });

            html += '</ul>';

            if (!column.isOverdue) {
                html += '<button class="todo-planner-add add-btn" data-due-date="' + getDateKey(column.date) + '">' +
                    '<i class="fa fa-plus"></i> ' + app.localize('AddToDo') +
                    '</button>';
            }

            html += '</div>';
            board.append(html);
        });

        initSortable(columns);
        attachBoardHandlers();
    }

    function initSortable(columns) {
        columns.forEach(function (column) {
            var key = column.key;
            var element = $('[data-column-key="' + key + '"] .todo-planner-list')[0];

            if (_sortableInstances[key]) {
                _sortableInstances[key].destroy();
            }

            _sortableInstances[key] = Sortable.create(element, {
                group: 'planner',
                animation: 150,
                onEnd: function (evt) {
                    var taskId = parseInt($(evt.item).attr('data-id'));
                    var targetKey = $(evt.to).closest('.todo-planner-column').attr('data-column-key');
                    var dueDate = targetKey === 'overdue' ? null : getDateKey(parseDateKey(targetKey));

                    _userTaskService.updateDueDate({
                        taskId: taskId,
                        dueDate: dueDate
                    }).done(loadTasks);
                }
            });
        });
    }

    function attachBoardHandlers() {
        var board = $('#TodoPlannerBoard');

        board.off('click', '.complete-btn').on('click', '.complete-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.todo-planner-card').attr('data-id'));
            _userTaskService.complete({ id: taskId }).done(loadTasks);
        });

        board.off('click', '.edit-btn').on('click', '.edit-btn', function (e) {
            e.preventDefault();
            var taskId = parseInt($(this).closest('.todo-planner-card').attr('data-id'));
            _createOrEditModal.open({ id: taskId });
        });

        board.off('click', '.add-btn').on('click', '.add-btn', function (e) {
            e.preventDefault();
            var dueDate = $(this).data('due-date');
            var selectedProjectId = getSelectedProjectId();
            var modalArgs = { dueDate: dueDate };

            if (selectedProjectId) {
                modalArgs.projectId = selectedProjectId;
            }

            _createOrEditModal.open(modalArgs);
        });
    }

    function attachToolbarHandlers() {
        $('#PlannerPrevWeekButton').on('click', function () {
            _weekOffset--;
            loadTasks();
        });

        $('#PlannerNextWeekButton').on('click', function () {
            _weekOffset++;
            loadTasks();
        });

        $('#PlannerTodayButton').on('click', function () {
            _weekOffset = 0;
            loadTasks();
        });

        $('#PlannerProjectFilter').on('change', function () {
            loadTasks();
        });

        $('#PlannerSearchFilter').on('input', function () {
            if (_searchDebounceHandle) {
                clearTimeout(_searchDebounceHandle);
            }

            _searchDebounceHandle = setTimeout(function () {
                loadTasks();
            }, 300);
        });

        $('#PlannerShowCompletedToggle').on('change', function () {
            loadTasks();
        });
    }

    function loadTasks() {
        var selectedProjectId = getSelectedProjectId();
        _userTaskService.getTasks({
            maxResultCount: 500,
            projectId: selectedProjectId,
            filter: getSearchFilterText()
        }).done(function (result) {
            render(result.items || []);
        });
    }

    function getSelectedProjectId() {
        var selectedProjectId = $('#PlannerProjectFilter').val();
        return selectedProjectId ? parseInt(selectedProjectId) : null;
    }

    function getSearchFilterText() {
        return ($('#PlannerSearchFilter').val() || '').trim();
    }

    function isShowingCompleted() {
        return $('#PlannerShowCompletedToggle').is(':checked');
    }

    function loadProjectFilterOptions(callback) {
        _projectService.getProjects({
            maxResultCount: 500,
            skipCount: 0
        }).done(function (result) {
            var select = $('#PlannerProjectFilter');
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

    attachToolbarHandlers();
    loadProjectFilterOptions(function () {
        loadStatuses(loadTasks);
    });

    abp.event.on('app.createOrEditUserTaskModalSaved', function () {
        loadTasks();
    });
})();
