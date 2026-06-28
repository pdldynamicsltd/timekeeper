(function () {
    var _projectService = abp.services.app.project;
    var _timeEntryService = abp.services.app.timeEntry;
    var _createOrEditModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/TimeTracking/CreateOrEditProjectModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/TimeTracking/_CreateOrEditProjectModal.js',
        modalClass: 'CreateOrEditProjectModal'
    });
    function buildProjectRow(project) {
        var budgetPercent = project.budgetHours > 0
            ? Math.min(Math.round((project.usedHours / project.budgetHours) * 100), 100)
            : 0;
        var budgetClass = budgetPercent >= 90 ? 'danger' : budgetPercent >= 75 ? 'warning' : 'success';
        var statusBadge = getStatusBadge(project.status);
        var color = project.color || '#3498db';
        var isCompleted = project.status === 3;

        var budgetTypeText = project.budgetHours > 0 ? app.localize('ProjectBudget') : app.localize('NoBudget');
        var budgetText = project.budgetHours > 0
            ? project.budgetHours.toFixed(1) + 'h <span class="text-muted fs-8">(' + (project.budgetHours / 8).toFixed(1) + 'd)</span>'
            : '-';
        var usedText = (project.usedHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((project.usedHours || 0) / 8).toFixed(1) + 'd)</span>';
        var remainingText = ((project.remainingHours || 0)).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((project.remainingHours || 0) / 8).toFixed(1) + 'd)</span>';

        return $('<tr' + (isCompleted ? ' class="tt-historic-row"' : '') + '>' +
            '<td>' +
            '<div class="d-flex flex-column">' +
            '<a href="/App/TimeTracking/ProjectDetail/' + project.id + '" class="text-dark text-hover-primary project-name-link">' + project.name + '</a>' +
            (project.description ? '<span class="text-muted fs-8">' + project.description + '</span>' : '') +
            '</div>' +
            '</td>' +
            '<td>' + statusBadge + '</td>' +
            '<td><span class="badge badge-light-info">' + budgetTypeText + '</span></td>' +
            '<td>' + budgetText + '</td>' +
            '<td>' + usedText + '</td>' +
            '<td class="' + ((project.remainingHours || 0) < 0 ? 'text-danger' : '') + '">' + remainingText + '</td>' +
            '<td class="project-budget-cell">' +
            '<div class="d-flex align-items-center gap-2">' +
            '<div class="progress flex-grow-1 h-6px">' +
            '<div class="progress-bar bg-' + budgetClass + '" style="width:' + budgetPercent + '%; background-color:' + color + ' !important;"></div>' +
            '</div>' +
            '<span class="fw-semibold text-' + budgetClass + '">' + budgetPercent + '%</span>' +
            '</div>' +
            '</td>' +
            '<td class="text-end project-actions">' +
            '<a href="/App/TimeTracking/ProjectDetail/' + project.id + '" class="btn btn-sm btn-light-primary me-1">' + app.localize('ViewDetails') + '</a>' +
            (abp.auth.isGranted('Pages.TimeTracking.Projects.Edit')
                ? '<button class="btn btn-sm btn-icon btn-light-primary edit-project-btn me-1" data-id="' + project.id + '" title="' + app.localize('Edit') + '"><i class="ki-outline ki-pencil fs-4"></i></button>'
                : '') +
            (abp.auth.isGranted('Pages.TimeTracking.Projects.Delete')
                ? '<button class="btn btn-sm btn-icon btn-light-danger delete-project-btn" data-id="' + project.id + '" data-name="' + project.name + '" title="' + app.localize('Delete') + '"><i class="ki-outline ki-trash fs-4"></i></button>'
                : '') +
            '</td>' +
            '</tr>');
    }

    function getProjects() {
        var statusVal = $('#ProjectStatusFilter').val();
        var input = {
            filter: $('#ProjectFilterText').val(),
            statusFilter: statusVal ? parseInt(statusVal) : null,
            maxResultCount: 100,
            skipCount: 0
        };

        $('#ProjectsTableBody').html('<tr><td colspan="8" class="text-center py-10"><i class="ki-outline ki-loading fs-3x spin"></i></td></tr>');

        _projectService.getProjects(input).done(function (result) {
            var currentBody = $('#ProjectsTableBody');
            var completedBody = $('#CompletedProjectsTableBody');
            currentBody.empty();
            completedBody.empty();

            var items = result.items || [];
            var completed = items.filter(function (p) { return p.status === 3; });
            var current = items.filter(function (p) { return p.status !== 3; });

            if (current.length === 0) {
                currentBody.html('<tr><td colspan="8" class="text-center text-muted py-10">' + app.localize('NoProjects') + '</td></tr>');
            } else {
                $.each(current, function (i, project) {
                    currentBody.append(buildProjectRow(project));
                });
            }

            $.each(completed, function (i, project) {
                completedBody.append(buildProjectRow(project));
            });

            $('#CompletedProjectsCount').text(completed.length);
            $('#CompletedProjectsCard').toggle(completed.length > 0);
        }).fail(function () {
            abp.notify.error(app.localize('LoadError'));
            $('#ProjectsTableBody').html('<tr><td colspan="8" class="text-center text-danger py-10">' + app.localize('LoadError') + '</td></tr>');
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

    function setImportButtonBusy(buttonSelector, busy) {
        var button = $(buttonSelector);
        button.prop('disabled', busy);
        if (busy) {
            button.data('original-text', button.text());
            button.text(app.localize('Importing'));
            return;
        }

        var originalText = button.data('original-text');
        if (originalText) {
            button.text(originalText);
        }
    }

    function readFileAsText(file) {
        return new Promise(function (resolve, reject) {
            var reader = new FileReader();
            reader.onload = function (event) {
                resolve(event.target.result);
            };
            reader.onerror = function () {
                reject(new Error('File read failed'));
            };
            reader.readAsText(file);
        });
    }

    function showImportResult(result) {
        abp.notify.success(app.localize(
            'CsvImportCompletedMessage',
            result.createdProjects || 0,
            result.updatedProjects || 0,
            result.createdTasks || 0,
            result.createdTimeEntries || 0,
            result.skippedRows || 0));
    }

    function importCsv(fileInputSelector, buttonSelector, importFunc) {
        var input = $(fileInputSelector)[0];
        if (!input || !input.files || input.files.length === 0) {
            abp.notify.warn(app.localize('PleaseSelectCsvFile'));
            return;
        }

        var file = input.files[0];
        setImportButtonBusy(buttonSelector, true);

        readFileAsText(file)
            .then(function (content) {
                importFunc({ csvContent: content })
                    .done(function (result) {
                        showImportResult(result);
                        input.value = '';
                        getProjects();
                    })
                    .fail(function () {
                        abp.notify.error(app.localize('CsvImportFailed'));
                    })
                    .always(function () {
                        setImportButtonBusy(buttonSelector, false);
                    });
            })
            .catch(function () {
                abp.notify.error(app.localize('CsvImportFailed'));
                setImportButtonBusy(buttonSelector, false);
            });
    }

    $('#CreateNewProjectButton').click(function () {
        _createOrEditModal.open();
    });

    $('#ImportProjectsCsvButton').click(function () {
        importCsv('#ProjectsCsvFileInput', '#ImportProjectsCsvButton', function (request) {
            return _projectService.importProjectsFromCsv(request);
        });
    });

    $('#ImportTimeEntriesCsvButton').click(function () {
        importCsv('#TimeEntriesCsvFileInput', '#ImportTimeEntriesCsvButton', function (request) {
            return _timeEntryService.importTimeEntriesFromCsv(request);
        });
    });

    $('#GetProjectsButton').click(function () {
        getProjects();
    });

    $('#ProjectStatusFilter').change(function () {
        getProjects();
    });

    $('#ProjectFilterText').keyup(app.workspace.debounce(function () {
        getProjects();
    }, 300));

    $('#ProjectsTableBody, #CompletedProjectsTableBody').on('click', '.edit-project-btn', function () {
        _createOrEditModal.open({ id: $(this).data('id') });
    });

    $('#ProjectsTableBody, #CompletedProjectsTableBody').on('click', '.delete-project-btn', function () {
        deleteProject($(this).data('id'), $(this).data('name'));
    });

    abp.event.on('app.createOrEditProjectModalSaved', function () {
        getProjects();
    });

    getProjects();
})();

