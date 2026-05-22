(function () {
    $(function () {
        var _healthCheckService = abp.services.app.healthCheck;

        function getStatusBadge(status) {
            if (status === 'Healthy') {
                return '<span class="badge badge-success">' + app.localize('Healthy') + '</span>';
            } else if (status === 'Degraded') {
                return '<span class="badge badge-warning">' + app.localize('Degraded') + '</span>';
            } else {
                return '<span class="badge badge-danger">' + app.localize('Unhealthy') + '</span>';
            }
        }

        function formatDuration(duration) {
            // Duration comes as a TimeSpan string like "00:00:00.0012345"
            if (typeof duration === 'string') {
                var parts = duration.split(':');
                var seconds = parseFloat(parts[2]);
                return (seconds * 1000).toFixed(2) + ' ms';
            }
            return duration + ' ms';
        }

        function refreshHealthChecks() {
            abp.ui.setBusy();
            _healthCheckService.getHealthChecks()
                .done(function (result) {
                    var tbody = $('#HealthChecksTableBody');
                    tbody.empty();

                    $.each(result.items, function (index, healthCheck) {
                        var row = '<tr>' +
                            '<td>' + _.escape(healthCheck.name) + '</td>' +
                            '<td>' + getStatusBadge(healthCheck.status) + '</td>' +
                            '<td>' + formatDuration(healthCheck.duration) + '</td>' +
                            '<td>' + _.escape(healthCheck.description || '') + '</td>' +
                            '</tr>';
                        tbody.append(row);
                    });

                    abp.notify.success(app.localize('HealthChecksRefreshed'));
                })
                .always(function () {
                    abp.ui.clearBusy();
                });
        }

        $('#RefreshHealthChecksButton').click(function (e) {
            e.preventDefault();
            refreshHealthChecks();
        });

        // Auto-refresh every 10 seconds
        var refreshInterval = setInterval(function () {
            refreshHealthChecks();
        }, REFRESH_INTERVAL_MS);

        // Clear interval when page is unloaded
        $(window).on('beforeunload', function () {
            if (refreshInterval) {
                clearInterval(refreshInterval);
            }
        });
    });
})();
