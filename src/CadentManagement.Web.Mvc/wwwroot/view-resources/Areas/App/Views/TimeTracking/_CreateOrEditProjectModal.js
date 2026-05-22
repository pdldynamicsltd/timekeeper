(function () {
    app.modals.CreateOrEditProjectModal = function () {
        var _modalManager;
        var _projectService = abp.services.app.project;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            toggleBudgetHours();
            $('#ProjectBudgetType').change(toggleBudgetHours);
        };

        function toggleBudgetHours() {
            var val = parseInt($('#ProjectBudgetType').val());
            // 0 = NoBudget, 1 = ProjectBudget, 2 = TaskBudget
            $('#BudgetHoursRow').toggle(val === 1);
        }

        this.save = function () {
            if (!$('form[name=ProjectForm]').valid()) {
                return;
            }

            var project = {
                id: $('#ProjectId').val() ? parseInt($('#ProjectId').val()) : null,
                name: $('#ProjectName').val(),
                description: $('#ProjectDescription').val(),
                status: parseInt($('#ProjectStatus').val()),
                budgetType: parseInt($('#ProjectBudgetType').val()),
                budgetHours: parseFloat($('#ProjectBudgetHours').val()) || null,
                startDate: $('#ProjectStartDate').val() || null,
                endDate: $('#ProjectEndDate').val() || null,
                color: $('#ProjectColor').val(),
                isPublic: $('#ProjectIsPublic').is(':checked')
            };

            _modalManager.setBusy(true);

            var saveFunc = project.id
                ? _projectService.update(project)
                : _projectService.create(project);

            saveFunc.done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditProjectModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})();
