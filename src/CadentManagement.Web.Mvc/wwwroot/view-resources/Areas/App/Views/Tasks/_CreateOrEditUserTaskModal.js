(function () {
    app.modals.CreateOrEditUserTaskModal = function () {
        var _modalManager;
        var _userTaskService = abp.services.app.userTask;
        var _$form;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$form = _modalManager.getModal().find('form');
        };

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }

            var input = _$form.serializeFormToObject();

            input.id = input.id ? parseInt(input.id) : null;
            input.priority = parseInt(input.priority);
            input.status = parseInt(input.status);
            input.estimatedMinutes = parseInt(input.estimatedMinutes) || null;
            input.projectId = input.projectId ? parseInt(input.projectId) : null;

            abp.ui.setBusy(_$form);

            var promise = input.id
                ? _userTaskService.update(input)
                : _userTaskService.create(input);

            promise.done(function () {
                abp.notify.success(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditUserTaskModalSaved');
            }).fail(function (error) {
                abp.message.error(
                    error.responseJSON ? error.responseJSON.error.message : app.localize('Error')
                );
            }).always(function () {
                abp.ui.clearBusy(_$form);
            });
        };
    };
})();
