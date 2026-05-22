(function () {
    app.modals.CreateOrEditTimeEntryModal = function () {
        var _modalManager;
        var _timeEntryService = abp.services.app.timeEntry;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            $('#TimeEntryProjectId').change(function () {
                reloadTasks($(this).val());
            });

            $('#TimeEntryStartTime, #TimeEntryEndTime').change(function () {
                updateDurationPreview();
            });

            updateDurationPreview();
        };

        function reloadTasks(projectId) {
            var sel = $('#TimeEntryTaskId');
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

        function updateDurationPreview() {
            var start = $('#TimeEntryStartTime').val();
            var end = $('#TimeEntryEndTime').val();

            if (!start || !end) {
                $('#DurationDisplay').addClass('d-none');
                return;
            }

            var startMs = new Date(start).getTime();
            var endMs = new Date(end).getTime();

            if (isNaN(startMs) || isNaN(endMs) || endMs <= startMs) {
                $('#DurationDisplay').addClass('d-none');
                return;
            }

            var diffMs = endMs - startMs;
            var hours = Math.floor(diffMs / 3600000);
            var minutes = Math.floor((diffMs % 3600000) / 60000);
            var text = hours + 'h ' + (minutes < 10 ? '0' : '') + minutes + 'm';

            $('#DurationText').text(app.localize('DurationPreview') + ': ' + text);
            $('#DurationDisplay').removeClass('d-none');
        }

        this.save = function () {
            if (!$('form[name=TimeEntryForm]').valid()) {
                return;
            }

            var entry = {
                id: $('#TimeEntryId').val() ? parseInt($('#TimeEntryId').val()) : null,
                projectId: parseInt($('#TimeEntryProjectId').val()),
                taskId: $('#TimeEntryTaskId').val() ? parseInt($('#TimeEntryTaskId').val()) : null,
                startTime: $('#TimeEntryStartTime').val(),
                endTime: $('#TimeEntryEndTime').val(),
                description: $('#TimeEntryDescription').val()
            };

            _modalManager.setBusy(true);

            var saveFunc = entry.id
                ? _timeEntryService.update(entry)
                : _timeEntryService.create(entry);

            saveFunc.done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditTimeEntryModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})();
