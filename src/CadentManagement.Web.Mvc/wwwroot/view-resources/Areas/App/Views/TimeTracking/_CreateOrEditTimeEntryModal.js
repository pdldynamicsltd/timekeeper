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

            _modalManager.getModal().find('.duplicate-button').click(function (e) {
                e.preventDefault();
                duplicateToNextDay();
            });

            updateDurationPreview();
        };

        function formatDateTimeLocal(date) {
            var localDate = date instanceof Date ? date : new Date(date);
            var year = localDate.getFullYear();
            var month = String(localDate.getMonth() + 1).padStart(2, '0');
            var day = String(localDate.getDate()).padStart(2, '0');
            var hours = String(localDate.getHours()).padStart(2, '0');
            var minutes = String(localDate.getMinutes()).padStart(2, '0');
            return year + '-' + month + '-' + day + 'T' + hours + ':' + minutes;
        }

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

        function duplicateToNextDay() {
            if (!$('form[name=TimeEntryForm]').valid()) {
                abp.notify.warn(app.localize('InvalidFormMessage'));
                return;
            }

            var duplicatedStart = new Date($('#TimeEntryStartTime').val());
            var duplicatedEnd = new Date($('#TimeEntryEndTime').val());

            if (isNaN(duplicatedStart.getTime()) || isNaN(duplicatedEnd.getTime()) || duplicatedEnd <= duplicatedStart) {
                abp.notify.warn(app.localize('InvalidFormMessage'));
                return;
            }

            duplicatedStart.setDate(duplicatedStart.getDate() + 1);
            duplicatedEnd.setDate(duplicatedEnd.getDate() + 1);

            var duplicateEntry = {
                projectId: parseInt($('#TimeEntryProjectId').val()),
                taskId: $('#TimeEntryTaskId').val() ? parseInt($('#TimeEntryTaskId').val()) : null,
                startTime: formatDateTimeLocal(duplicatedStart),
                endTime: formatDateTimeLocal(duplicatedEnd),
                description: $('#TimeEntryDescription').val()
            };

            _modalManager.setBusy(true);
            _timeEntryService.create(duplicateEntry)
                .done(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.createOrEditTimeEntryModalSaved');
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
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
