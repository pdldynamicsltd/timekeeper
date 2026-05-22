(function ($) {
    $(function () {
        var _$activeSessionsTable = $('#ActiveSessionsTable');
        var _userSessionService = abp.services.app.userSession;
        var _isRevocationEnabled = typeof isSessionRevocationEnabled !== 'undefined' ? isSessionRevocationEnabled : false;

        var columnDefs = [
            {
                className: 'dtr-control responsive',
                orderable: false,
                render: function () {
                    return '';
                },
                targets: 0,
            },
            {
                targets: 1,
                data: 'deviceInfo',
                render: function (deviceInfo) {
                    return (
                        '<i class="fa fa-desktop me-2"></i>' +
                        (deviceInfo || app.localize('Unknown'))
                    );
                },
            },
            {
                targets: 2,
                data: 'ipAddress',
                render: function (ipAddress) {
                    return ipAddress || '-';
                },
            },
            {
                targets: 3,
                data: 'signInTime',
                render: function (signInTime) {
                    return signInTime ? moment(signInTime).format('L LT') : '-';
                },
            },
            {
                targets: 4,
                data: 'lastActivityTime',
                render: function (lastActivityTime) {
                    return lastActivityTime
                        ? moment(lastActivityTime).format('L LT')
                        : '-';
                },
            },
            {
                targets: 5,
                data: 'isCurrent',
                orderable: false,
                render: function (isCurrent) {
                    if (isCurrent) {
                        return (
                            '<span class="badge badge-success">' +
                            app.localize('CurrentSession') +
                            '</span>'
                        );
                    }
                    return (
                        '<span class="badge badge-light-primary">' +
                        app.localize('Active') +
                        '</span>'
                    );
                },
            },
        ];

        if (_isRevocationEnabled) {
            columnDefs.push({
                targets: 6,
                data: null,
                orderable: false,
                className: 'text-center',
                render: function (data, type, row) {
                    if (row.isCurrent) {
                        return '';
                    }
                    return (
                        '<button class="btn btn-sm btn-icon btn-active-danger revoke-session-btn" data-session-id="' +
                        row.id +
                        '" data-bs-toggle="tooltip" data-bs-placement="top" title="' +
                        app.localize('Revoke') +
                        '"><i class="fa fa-ban"></i></button>'
                    );
                },
            });
        }

        var dataTable = _$activeSessionsTable.DataTable({
            paging: false,
            serverSide: false,
            processing: true,
            listAction: {
                ajaxFunction: _userSessionService.getSessions,
                inputFilter: function () {
                    return {};
                },
            },
            drawCallback: function (settings) {
                $('[data-bs-toggle="tooltip"]').tooltip();

                if (_isRevocationEnabled) {
                    var data = settings.aoData;
                    var hasOtherSessions = false;
                    for (var i = 0; i < data.length; i++) {
                        if (data[i]._aData && !data[i]._aData.isCurrent) {
                            hasOtherSessions = true;
                            break;
                        }
                    }
                    if (hasOtherSessions) {
                        $('#RevokeAllOtherSessionsButton').show();
                    } else {
                        $('#RevokeAllOtherSessionsButton').hide();
                    }
                }
            },
            columnDefs: columnDefs,
        });

        function revokeSession(sessionId) {
            abp.message.confirm(
                app.localize('RevokeSessionConfirmation'),
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _userSessionService
                            .revokeSession({ id: sessionId })
                            .done(function () {
                                abp.notify.success(app.localize('SessionRevoked'));
                                dataTable.ajax.reload();
                            });
                    }
                }
            );
        }

        $(document).on('click', '.revoke-session-btn', function () {
            var sessionId = $(this).data('session-id');
            revokeSession(sessionId);
        });

        $('#RevokeAllOtherSessionsButton').click(function () {
            abp.message.confirm(
                app.localize('RevokeAllOtherSessionsConfirmation'),
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _userSessionService
                            .revokeAllOtherSessions()
                            .done(function () {
                                abp.notify.success(app.localize('AllOtherSessionsRevoked'));
                                dataTable.ajax.reload();
                            });
                    }
                }
            );
        });
    });
})(jQuery);
