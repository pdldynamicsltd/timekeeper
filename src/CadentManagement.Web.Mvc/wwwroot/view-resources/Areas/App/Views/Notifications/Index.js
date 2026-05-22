(function () {
  $(function () {
    var _$notificationsTable = $('#NotificationsTable');
    var _notificationService = abp.services.app.notification;

    var _$targetValueFilterSelectionCombobox = $('#TargetValueFilterSelectionCombobox');

    var _appUserNotificationHelper = new app.UserNotificationHelper();

    var _viewNotificationModal = new app.ModalManager({
      viewUrl: abp.appPath + 'App/Notifications/ViewModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Notifications/_ViewModal.js',
      modalClass: 'ViewNotificationModal',
    });

    var _selectedDateRangeNotification = {
      startDate: moment().startOf('day').subtract(7, 'days'),
      endDate: moment().endOf('day'),
    };

    $(document)
      .find('#ReceivedNotifications_StartEndRange')
      .daterangepicker(
        $.extend(true, app.createDateRangePickerOptions(), _selectedDateRangeNotification),
        function (start, end) {
          _selectedDateRangeNotification.startDate = start.format('YYYY-MM-DDT00:00:00Z');
          _selectedDateRangeNotification.endDate = end.format('YYYY-MM-DDT23:59:59.999Z');

          getNotifications();
        },
      );

    var createNotificationActionButtons = function ($td, record) {
      var formattedRecord = _appUserNotificationHelper.format(record);
      var isRead = formattedRecord.state === 'READ';

      var $container = $('<div class="d-flex align-items-center gap-1"></div>');

      var $viewBtn = $('<button/>')
        .addClass('btn btn-icon btn-sm btn-light-info')
        .attr('title', app.localize('View'))
        .attr('data-bs-toggle', 'tooltip')
        .append('<i class="bi bi-eye fs-5"></i>')
        .on('click', function (e) {
          e.preventDefault();
          openViewNotificationModal(record.id);
        });

      var $readBtn = $('<button/>')
        .addClass('btn btn-icon btn-sm ' + (isRead ? 'btn-light-success' : 'btn-light-primary'))
        .attr('title', app.localize('SetAsRead'))
        .attr('data-bs-toggle', 'tooltip')
        .prop('disabled', isRead)
        .append(
          `<i class="bi ${isRead ? 'bi-envelope-open' : 'bi-envelope'} fs-5"></i>`
        )
        .on('click', function (e) {
          e.preventDefault();
          if (isRead) return;

          setNotificationAsRead(record, function () {
            getNotifications();
          });
        });

      var $deleteBtn = $('<button/>')
        .addClass('btn btn-icon btn-sm btn-light-danger')
        .attr('title', app.localize('Delete'))
        .attr('data-bs-toggle', 'tooltip')
        .append('<i class="bi bi-trash fs-5"></i>')
        .on('click', function () {
          deleteNotification(record);
        });

      $container.append($viewBtn, $readBtn, $deleteBtn);
      $td.addClass('text-end').append($container);
    };

    function getNotificationTextBySeverity(severity) {
      switch (severity) {
        case abp.notifications.severity.SUCCESS:
          return app.localize('Success');
        case abp.notifications.severity.WARN:
          return app.localize('Warning');
        case abp.notifications.severity.ERROR:
          return app.localize('Error');
        case abp.notifications.severity.FATAL:
          return app.localize('Fatal');
        case abp.notifications.severity.INFO:
        default:
          return app.localize('Info');
      }
    }

    var dataTable = _$notificationsTable.DataTable({
      paging: true,
      serverSide: true,
      processing: true,
      listAction: {
        ajaxFunction: _notificationService.getUserNotifications,
        inputFilter: function () {
          return {
            state: _$targetValueFilterSelectionCombobox.val(),
            startDate: _selectedDateRangeNotification.startDate,
            endDate: _selectedDateRangeNotification.endDate,
          };
        },
      },
      columnDefs: [
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
          responsivePriority: 0,
          data: null,
          orderable: false,
          defaultContent: '',
          type: 'html',
          width: '140px',
          createdCell: function (td, cellData, rowData) {
            createNotificationActionButtons($(td), rowData);
          },
        },
        {
          targets: 2,
          data: 'severity',
          orderable: false,
          render: function (severity, type, row, meta) {
            var icon = app.notification.getUiIconBySeverity(row.notification.severity);
            var iconFontClass = app.notification.getIconFontClassBySeverity(row.notification.severity);
            var $span = $('<span></span>');
            var $icon = $(
              `<i class="${icon} ${iconFontClass} fa-2x" data-bs-toggle="tooltip" data-bs-placement="right" data-bs-container="body" data-bs-original-title="${getNotificationTextBySeverity(
                row.notification.severity,
              )}"></i>`,
            );
            $span.append($icon);

            return $span[0].outerHTML;
          },
        },
        {
          targets: 3,
          data: 'notification',
          responsivePriority: 0,
          orderable: false,
          render: function (notification, type, row, meta) {
            var $container;
            var formattedRecord = _appUserNotificationHelper.format(row, false);
            var rowClass = getRowClass(formattedRecord);

            if (formattedRecord.url && formattedRecord.url !== '#') {
              $container = $(
                '<a title="' +
                  formattedRecord.text +
                  '" href="' +
                  formattedRecord.url +
                  '" class="' +
                  rowClass +
                  '">' +
                  abp.utils.truncateStringWithPostfix(formattedRecord.text, 120) +
                  '</a>',
              );
            } else {
              $container = $(
                '<span title="' +
                  formattedRecord.text +
                  '" class="' +
                  rowClass +
                  '">' +
                  abp.utils.truncateStringWithPostfix(formattedRecord.text, 120) +
                  '</span>',
              );
            }

            return $container[0].outerHTML;
          },
        },
        {
          targets: 4,
          data: 'creationTime',
          orderable: false,
          render: function (creationTime, type, row, meta) {
            var formattedRecord = _appUserNotificationHelper.format(row);
            var rowClass = getRowClass(formattedRecord);
            var $container = $(
              '<span title="' +
                moment(row.notification.creationTime).format('llll') +
                '" class="' +
                rowClass +
                '">' +
                formattedRecord.timeAgo +
                '</span> &nbsp;',
            );
            return $container[0].outerHTML;
          },
        },
      ],
      initComplete: function (settings, json) {
        KTApp.init();
      },
    });

    function deleteNotification(notification) {
      abp.message.confirm(
        app.localize('NotificationDeleteWarningMessage'),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            _notificationService
              .deleteNotification({
                id: notification.id,
              })
              .done(function () {
                getNotifications();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
              });
          }
        },
      );
    }

    function deleteNotifications() {
      abp.message.confirm(
        app.localize('DeleteListedNotificationsWarningMessage'),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            _notificationService
              .deleteAllUserNotifications({
                state: _$targetValueFilterSelectionCombobox.val(),
                startDate: _selectedDateRangeNotification.startDate,
                endDate: _selectedDateRangeNotification.endDate,
              })
              .done(function () {
                getNotifications();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
              });
          }
        },
      );
    }

    function getRowClass(formattedRecord) {
      return formattedRecord.state === 'READ' ? 'notification-read text-muted' : '';
    }

    function getNotifications() {
      dataTable.ajax.reload();
    }

    function setNotificationAsRead(userNotification, callback) {
      _appUserNotificationHelper.setAsRead(userNotification.id, function () {
        if (callback) {
          callback();
        }
      });
    }

    function setAllNotificationsAsRead() {
      _appUserNotificationHelper.setAllAsRead(function () {
        getNotifications();
      });
    }

    function openNotificationSettingsModal() {
      _appUserNotificationHelper.openSettingsModal();
    }

    function openViewNotificationModal(notificationId) {
      _viewNotificationModal.open({
        id: notificationId,
      });
    }

    _$targetValueFilterSelectionCombobox.change(function () {
      getNotifications();
    });

    $('#RefreshNotificationTableButton').click(function (e) {
      e.preventDefault();
      getNotifications();
    });

    $('#btnOpenNotificationSettingsModal').click(function (e) {
      openNotificationSettingsModal();
    });

    $('#btnSetAllNotificationsAsRead').click(function (e) {
      e.preventDefault();
      setAllNotificationsAsRead();
    });

    $('#DeleteAllNotificationsButton').click(function (e) {
      e.preventDefault();
      deleteNotifications();
    });
  });
})();
