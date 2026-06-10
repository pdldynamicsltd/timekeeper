(function () {
    var _userTaskService = abp.services.app.userTask;
    var _statuses = [];

    function loadStatuses() {
        _userTaskService.getTodoStatuses().done(function (statuses) {
            _statuses = (statuses || []).sort(function (a, b) { return a.sortOrder - b.sortOrder; });
            renderTable();
        });
    }

    function renderTable() {
        var tableBody = $('#TodoStatusesTableBody');
        tableBody.empty();

        if (!_statuses.length) {
            tableBody.append('<tr><td colspan="5" class="text-center text-muted py-5">' + app.localize('NoData') + '</td></tr>');
            return;
        }

        _statuses.forEach(function (status) {
            tableBody.append(
                '<tr data-id="' + status.id + '">' +
                '<td><input type="text" class="form-control form-control-sm status-name" value="' + escapeHtml(status.name) + '"></td>' +
                '<td><input type="color" class="form-control form-control-sm form-control-color status-color" value="' + normalizeColor(status.color) + '"></td>' +
                '<td><input type="number" class="form-control form-control-sm status-sort" value="' + status.sortOrder + '"></td>' +
                '<td><label class="form-check form-switch form-check-custom form-check-solid"><input class="form-check-input status-completed" type="checkbox" ' + (status.isCompleted ? 'checked' : '') + '></label></td>' +
                '<td class="text-end">' +
                '<button class="btn btn-sm btn-light-primary save-status-btn me-2"><i class="fa fa-save"></i></button>' +
                '<button class="btn btn-sm btn-light-danger delete-status-btn"><i class="fa fa-trash"></i></button>' +
                '</td>' +
                '</tr>'
            );
        });
    }

    function normalizeColor(color) {
        if (!color || !/^#[0-9A-Fa-f]{6}$/.test(color)) {
            return '#6c757d';
        }

        return color;
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

    $('#AddStatusButton').click(function () {
        var nextSort = _statuses.length ? _statuses[_statuses.length - 1].sortOrder + 10 : 10;

        _userTaskService.createTodoStatus({
            name: app.localize('NewStatus'),
            color: '#6c757d',
            sortOrder: nextSort,
            isCompleted: false
        }).done(function () {
            loadStatuses();
        });
    });

    $('#TodoStatusesTableBody').on('click', '.save-status-btn', function () {
        var row = $(this).closest('tr');
        var id = parseInt(row.data('id'));

        _userTaskService.updateTodoStatus({
            id: id,
            name: row.find('.status-name').val(),
            color: row.find('.status-color').val(),
            sortOrder: parseInt(row.find('.status-sort').val()) || 0,
            isCompleted: row.find('.status-completed').is(':checked')
        }).done(function () {
            abp.notify.success(app.localize('SavedSuccessfully'));
            loadStatuses();
        });
    });

    $('#TodoStatusesTableBody').on('click', '.delete-status-btn', function () {
        var id = parseInt($(this).closest('tr').data('id'));
        abp.message.confirm(app.localize('AreYouSure'), function (confirmed) {
            if (!confirmed) {
                return;
            }

            _userTaskService.deleteTodoStatus({ id: id }).done(function () {
                loadStatuses();
            });
        });
    });

    loadStatuses();
})();
