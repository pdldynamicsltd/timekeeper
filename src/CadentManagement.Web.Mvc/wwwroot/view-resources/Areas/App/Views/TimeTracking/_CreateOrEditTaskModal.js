(function () {
    app.modals.CreateOrEditTaskModal = function () {
        var _modalManager;
        var _taskService = abp.services.app.projectTask;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            $('#TaskProjectId').change(function () {
                reloadParentTasks($(this).val());
            });
        };

        function reloadParentTasks(projectId) {
            var sel = $('#TaskParentTaskId');
            sel.find('option:not(:first)').remove();
            if (!projectId) return;

            abp.services.app.projectTask.getTasks({
                projectId: parseInt(projectId),
                maxResultCount: 200,
                skipCount: 0
            }).done(function (result) {
                $.each(result.items, function (i, t) {
                    sel.append($('<option>').val(t.id).text(t.name));
                });
            });
        }

        this.save = function () {
            if (!$('form[name=TaskForm]').valid()) {
                return;
            }

            var task = {
                id: $('#TaskId').val() ? parseInt($('#TaskId').val()) : null,
                projectId: parseInt($('#TaskProjectId').val()),
                parentTaskId: $('#TaskParentTaskId').val() ? parseInt($('#TaskParentTaskId').val()) : null,
                name: $('#TaskName').val(),
                description: $('#TaskDescription').val(),
                status: parseInt($('#TaskStatus').val()),
                budgetHours: parseFloat($('#TaskBudgetHours').val()) || null,
                assignedToUserId: $('#TaskAssignedToUserId').val() ? parseInt($('#TaskAssignedToUserId').val()) : null
            };

            _modalManager.setBusy(true);

            var saveFunc = task.id
                ? _taskService.update(task)
                : _taskService.create(task);

            saveFunc.done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditTaskModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})();
