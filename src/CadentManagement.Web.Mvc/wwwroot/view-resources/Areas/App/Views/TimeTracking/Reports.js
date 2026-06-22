(function () {
    var _reportingService = abp.services.app.reporting;
    var _projectService = abp.services.app.project;
    var _taskService = abp.services.app.projectTask;
    var _dt;

    function loadProjects() {
        _projectService.getProjects({ maxResultCount: 200, skipCount: 0 }).done(function (result) {
            var sel = $('#ReportProjectFilter');
            $.each(result.items, function (i, p) {
                sel.append($('<option>').val(p.id).text(p.name));
            });
        });
    }

    $('#ReportProjectFilter').change(function () {
        var projectId = $(this).val();
        var taskSel = $('#ReportTaskFilter');
        taskSel.find('option:not(:first)').remove();

        if (!projectId) return;

        _taskService.getTasks({ projectId: parseInt(projectId), maxResultCount: 200, skipCount: 0 }).done(function (result) {
            $.each(result.items, function (i, t) {
                taskSel.append($('<option>').val(t.id).text(t.name));
            });
        });
    });

    function getReportInput() {
        return {
            projectId: $('#ReportProjectFilter').val() ? parseInt($('#ReportProjectFilter').val()) : null,
            taskId: $('#ReportTaskFilter').val() ? parseInt($('#ReportTaskFilter').val()) : null,
            startDate: $('#ReportStartDate').val() || null,
            endDate: $('#ReportEndDate').val() || null,
            maxResultCount: 500,
            skipCount: 0
        };
    }

    function loadBudgetSummary() {
        var projectId = $('#ReportProjectFilter').val();
        var container = $('#BudgetSummaryContainer');
        container.empty();
        if (!projectId) return;

        _reportingService.getProjectBudgetReport({ id: parseInt(projectId) }).done(function (report) {
            var percent = Math.min(Math.round(report.utilizationPercentage), 100);
            var cls = percent >= 90 ? 'danger' : percent >= 75 ? 'warning' : 'success';

            var card = $('<div class="col-md-4">' +
                '<div class="card">' +
                '<div class="card-body">' +
                '<div class="fw-bold fs-5 mb-3">' + report.projectName + '</div>' +
                '<div class="d-flex justify-content-between mb-2">' +
                '<span>' + app.localize('BudgetHours') + '</span>' +
                '<span class="fw-bold">' + (report.totalBudgetHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((report.totalBudgetHours || 0) / 8).toFixed(1) + 'd)</span></span>' +
                '</div>' +
                '<div class="d-flex justify-content-between mb-2">' +
                '<span>' + app.localize('UsedHours') + '</span>' +
                '<span class="fw-bold text-' + cls + '">' + (report.usedHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((report.usedHours || 0) / 8).toFixed(1) + 'd)</span></span>' +
                '</div>' +
                '<div class="d-flex justify-content-between mb-3">' +
                '<span>' + app.localize('RemainingHours') + '</span>' +
                '<span class="fw-bold text-' + (report.remainingHours < 0 ? 'danger' : 'success') + '">' + (report.remainingHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((report.remainingHours || 0) / 8).toFixed(1) + 'd)</span></span>' +
                '</div>' +
                '<div class="progress h-8px">' +
                '<div class="progress-bar bg-' + cls + '" style="width:' + percent + '%"></div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>');

            container.append(card);

            // Task budget rows
            if (report.taskBudgets && report.taskBudgets.length) {
                var taskCard = $('<div class="col-md-8"><div class="card"><div class="card-body">' +
                    '<div class="fw-bold fs-6 mb-3">' + app.localize('TaskBudgets') + '</div>' +
                    '<div class="table-responsive"><table class="table table-sm"><thead><tr>' +
                    '<th>' + app.localize('TaskName') + '</th>' +
                    '<th>' + app.localize('BudgetHours') + '</th>' +
                    '<th>' + app.localize('UsedHours') + '</th>' +
                    '<th>' + app.localize('RemainingHours') + '</th>' +
                    '<th>' + app.localize('UtilizationPercentage') + '</th>' +
                    '</tr></thead><tbody>');

                $.each(report.taskBudgets, function (i, t) {
                    var tp = Math.min(Math.round(t.utilizationPercentage), 100);
                    var tc = tp >= 90 ? 'danger' : tp >= 75 ? 'warning' : 'success';
                    taskCard.find('tbody').append($('<tr>' +
                        '<td>' + t.taskName + '</td>' +
                        '<td>' + (t.taskBudgetHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((t.taskBudgetHours || 0) / 8).toFixed(1) + 'd)</span></td>' +
                        '<td class="text-' + tc + '">' + (t.usedHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((t.usedHours || 0) / 8).toFixed(1) + 'd)</span></td>' +
                        '<td class="text-' + (t.remainingHours < 0 ? 'danger' : 'success') + '">' + (t.remainingHours || 0).toFixed(1) + 'h <span class="text-muted fs-8">(' + ((t.remainingHours || 0) / 8).toFixed(1) + 'd)</span></td>' +
                        '<td><div class="progress h-6px" style="min-width:80px"><div class="progress-bar bg-' + tc + '" style="width:' + tp + '%"></div></div></td>' +
                        '</tr>'));
                });

                taskCard.find('table').append('</tbody></table></div></div></div></div>');
                container.append(taskCard);
            }
        });
    }

    function loadReport() {
        if (_dt) {
            _dt.ajax.reload();
        }
        loadBudgetSummary();
    }

    function initDataTable() {
        _dt = $('#TimeEntriesReportTable').DataTable({
            paging: true,
            serverSide: false,
            processing: true,
            listAction: {
                ajaxFunction: _reportingService.getTimeEntriesReport,
                inputFilter: getReportInput
            },
            columnDefs: [
                { targets: 0, data: null, defaultContent: '', orderable: false, className: 'control' },
                { targets: 1, data: 'startTime', render: function (d) { return d ? moment(d).format('DD/MM/YYYY') : ''; } },
                { targets: 2, data: 'startTime', render: function (d) { return d ? moment(d).format('HH:mm') : ''; } },
                { targets: 3, data: 'endTime', render: function (d) { return d ? moment(d).format('HH:mm') : ''; } },
                {
                    targets: 4,
                    data: 'durationHours',
                    render: function (d) {
                        var hours = Number(d || 0);
                        var wholeHours = Math.floor(hours);
                        var minutes = Math.round((hours - wholeHours) * 60);
                        return wholeHours + 'h ' + (minutes < 10 ? '0' : '') + minutes + 'm';
                    }
                },
                { targets: 5, data: 'projectName' },
                { targets: 6, data: 'taskName', defaultContent: '-' },
                { targets: 7, data: 'userName' },
                { targets: 8, data: 'description', defaultContent: '' }
            ]
        });
    }

    $('#GetReportButton').click(function () {
        loadReport();
    });

    $('#ExportToExcelButton').click(function () {
        _reportingService.exportToExcel(getReportInput()).done(function (result) {
            app.downloadTempFile(result);
        });
    });

    $('#ReportProjectFilter, #ReportTaskFilter').change(function () {
        loadBudgetSummary();
    });

    loadProjects();
    initDataTable();
})();
