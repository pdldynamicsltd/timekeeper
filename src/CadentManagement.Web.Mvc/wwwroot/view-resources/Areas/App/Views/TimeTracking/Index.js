(function () {
    var _projectService = abp.services.app.project;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditProjectModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditProjectModal.js',
        modalClass: 'CreateOrEditProjectModal'
    });

    function getProjects() {
        var statusVal = $('#ProjectStatusFilter').val();
        var input = {
            filter: $('#ProjectFilterText').val(),
            statusFilter: statusVal ? parseInt(statusVal) : null,
            maxResultCount: 100,
            skipCount: 0
        };

        $('#ProjectsContainer').html('<div class="col-12 text-center py-10"><i class="ki-outline ki-loading fs-3x spin"></i></div>');

        _projectService.getProjects(input).done(function (result) {
            var container = $('#ProjectsContainer');
            container.empty();

            if (!result.items || result.items.length === 0) {
                container.html('<div class="col-12 text-center text-muted py-10">' +
                    '<i class="ki-outline ki-folder fs-3x mb-3"></i>' +
                    '<div>' + app.localize('NoProjects') + '</div>' +
                    '</div>');
                return;
            }

            $.each(result.items, function (i, project) {
                var budgetPercent = project.budgetHours > 0
                    ? Math.min(Math.round((project.usedHours / project.budgetHours) * 100), 100)
                    : 0;
                var budgetClass = budgetPercent >= 90 ? 'danger' : budgetPercent >= 75 ? 'warning' : 'success';
                var statusBadge = getStatusBadge(project.status);
                var color = project.color || '#3498db';

                var card = $('<div class="col-md-4">' +
                    '<div class="card card-flush h-100" style="border-top: 4px solid ' + color + '">' +
                    '<div class="card-header">' +
                    '<div class="card-title d-flex align-items-center gap-3">' +
                    '<a href="/App/TimeTracking/ProjectDetail/' + project.id + '" class="fw-bold fs-4 text-dark text-hover-primary">' + project.name + '</a>' +
                    statusBadge +
                    '</div>' +
                    '<div class="card-toolbar">' +
                    (abp.auth.isGranted('Pages.TimeTracking.Projects.Edit')
                        ? '<button class="btn btn-sm btn-icon btn-light-primary edit-project-btn me-1" data-id="' + project.id + '" title="' + app.localize('Edit') + '"><i class="ki-outline ki-pencil fs-3"></i></button>'
                        : '') +
                    (abp.auth.isGranted('Pages.TimeTracking.Projects.Delete')
                        ? '<button class="btn btn-sm btn-icon btn-light-danger delete-project-btn" data-id="' + project.id + '" data-name="' + project.name + '" title="' + app.localize('Delete') + '"><i class="ki-outline ki-trash fs-3"></i></button>'
                        : '') +
                    '</div>' +
                    '</div>' +
                    '<div class="card-body py-3">' +
                    (project.description ? '<p class="text-muted fs-6 mb-3">' + project.description + '</p>' : '') +
                    (project.budgetHours > 0
                        ? '<div class="d-flex justify-content-between mb-2">' +
                          '<span class="text-muted fs-7">' + app.localize('BudgetHours') + '</span>' +
                          '<span class="fw-bold fs-7 text-' + budgetClass + '">' + (project.usedHours || 0).toFixed(1) + ' / ' + project.budgetHours.toFixed(1) + ' hrs</span>' +
                          '</div>' +
                          '<div class="progress h-6px mb-3">' +
                          '<div class="progress-bar bg-' + budgetClass + '" style="width:' + budgetPercent + '%"></div>' +
                          '</div>'
                        : '<div class="text-muted fs-7 mb-3">' + app.localize('NoBudget') + '</div>') +
                    '<div class="d-flex gap-2">' +
                    '<a href="/App/TimeTracking/ProjectDetail/' + project.id + '" class="btn btn-sm btn-light-primary flex-grow-1">' +
                    '<i class="ki-outline ki-arrow-right fs-3 me-1"></i>' + app.localize('ViewDetails') +
                    '</a>' +
                    (abp.auth.isGranted('Pages.TimeTracking.TimeEntries.Create')
                        ? '<a href="/App/TimeTracking/MyWeek?projectId=' + project.id + '" class="btn btn-sm btn-light-success" title="' + app.localize('LogTime') + '">' +
                          '<i class="ki-outline ki-time fs-3"></i></a>'
                        : '') +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>');

                container.append(card);
            });
        }).fail(function () {
            abp.notify.error(app.localize('LoadError'));
            $('#ProjectsContainer').empty();
        });
    }

    function getStatusBadge(status) {
        var map = {
            1: ['success', app.localize('ActiveStatus')],
            2: ['secondary', app.localize('ArchivedStatus')],
            3: ['primary', app.localize('CompletedStatus')]
        };
        var info = map[status] || ['secondary', ''];
        return '<span class="badge badge-light-' + info[0] + ' fs-8">' + info[1] + '</span>';
    }

    function deleteProject(id, name) {
        abp.message.confirm(
            app.localize('ProjectDeleteWarningMessage', name),
            app.localize('AreYouSure'),
            function (isConfirmed) {
                if (isConfirmed) {
                    _projectService.delete({ id: id }).done(function () {
                        abp.notify.success(app.localize('SuccessfullyDeleted'));
                        getProjects();
                    });
                }
            }
        );
    }

    $('#CreateNewProjectButton').click(function () {
        _createOrEditModal.open();
    });

    $('#GetProjectsButton').click(function () {
        getProjects();
    });

    $('#ProjectStatusFilter').change(function () {
        getProjects();
    });

    $('#ProjectFilterText').keyup(abp.utils.debounce(function () {
        getProjects();
    }, 300));

    $('#ProjectsContainer').on('click', '.edit-project-btn', function () {
        _createOrEditModal.open({ id: $(this).data('id') });
    });

    $('#ProjectsContainer').on('click', '.delete-project-btn', function () {
        deleteProject($(this).data('id'), $(this).data('name'));
    });

    abp.event.on('app.createOrEditProjectModalSaved', function () {
        getProjects();
    });

    getProjects();
})();

