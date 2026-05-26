(function () {
    var _userTaskService = abp.services.app.userTask;
    var _objectMapper = abp.utils.formatString;

    var _modal = {
        manager: null,
        form: null,

        init: function (manager) {
            _modal.manager = manager;
            _modal.form = manager.getModal().find('form');
            _modal.registerEvents();
        },

        registerEvents: function () {
            _modal.form.find('#SaveButton').off('click').on('click', function (e) {
                e.preventDefault();
                _modal.save();
            });
        },

        save: function () {
            if (!_modal.form.valid()) {
                return false;
            }

            var input = _modal.form.serializeFormToObject();

            // Convert string values to appropriate types
            input.priority = parseInt(input.priority);
            input.status = parseInt(input.status);
            input.estimatedMinutes = parseInt(input.estimatedMinutes) || null;

            abp.ui.setBusy(_modal.form);

            var promise;
            if (input.id) {
                promise = _userTaskService.update(input);
            } else {
                promise = _userTaskService.create(input);
            }

            promise.done(function () {
                abp.notify.success(app.localize('SavedSuccessfully'));
                _modal.manager.close();
                abp.event.trigger('app.createOrEditUserTaskModalSaved');
            }).fail(function (error) {
                abp.message.error(error.responseJSON ? error.responseJSON.error.message : app.localize('Error'));
            }).always(function () {
                abp.ui.clearBusy(_modal.form);
            });

            return false;
        }
    };

    app.modals.CreateOrEditUserTaskModal = function (modalManager) {
        _modal.init(modalManager);
    };
})();
