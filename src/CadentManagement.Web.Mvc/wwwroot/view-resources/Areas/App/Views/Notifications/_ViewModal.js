(function () {
  $(function () {
    var _notificationAppService = abp.services.app.notification;

    abp.event.on('app.viewNotificationModalOpened', function (notificationId) {
      _notificationAppService.setNotificationAsRead({
        id: notificationId
      });
    });
  });
})();
