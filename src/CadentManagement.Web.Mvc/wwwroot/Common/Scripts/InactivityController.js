var InactivityController = (function () {
  var _inactivityControllerNotifyModal = app.modals.InactivityControllerNotifyModal.create();

  var lastActivityTimeStorageKey;
  var timeOutSecond;

  function bindActions() {
    window.onmousemove = setUserActivity;
    window.onmousedown = setUserActivity;
    window.onclick = setUserActivity;
    window.onscroll = setUserActivity;
    window.onkeydown = setUserActivity;

    window.addEventListener('touchstart', setUserActivity, { passive: true });

    document.addEventListener('visibilitychange', function () {
      if (document.visibilityState === 'visible') {
        controlStorage();
      }
    });

    window.addEventListener('focus', function () {
      controlStorage();
    });

    window.addEventListener('storage', function (e) {
      if (e.key === lastActivityTimeStorageKey) {
        controlStorage();
      }
    });
  }

  var isUserActive = true;
  function setUserActivity() {
    isUserActive = true;
  }

  function control() {
    writeToStorage();
    controlStorage();
  }

  function writeToStorage() {
    if (isUserActive) {
      if (localStorage) {
        localStorage.setItem(lastActivityTimeStorageKey, Date.now());
      } else {
        abp.utils.setCookieValue(lastActivityTimeStorageKey, Date.now());
      }
    }

    isUserActive = false;
  }

  var notifierIsOpened = false;
  function controlStorage() {
    var lastActivityTime = 0;
    if (localStorage) {
      lastActivityTime = parseInt(localStorage.getItem(lastActivityTimeStorageKey), 10) || 0;
    } else {
      lastActivityTime = parseInt(abp.utils.getCookieValue(lastActivityTimeStorageKey), 10) || 0;
    }

    if (Date.now() - lastActivityTime <= timeOutSecond * 1000) {
      if (notifierIsOpened) {
        _inactivityControllerNotifyModal.close();
        notifierIsOpened = false;
      }

      return;
    }

    if (!notifierIsOpened) {
      _inactivityControllerNotifyModal.open();
      notifierIsOpened = true;
    }
  }

  return {
    init: function (options) {
      lastActivityTimeStorageKey = options.lastActivityTimeStorageKey
        ? options.lastActivityTimeStorageKey
        : 'UserLastActivity';
      timeOutSecond = options.InActivityControlSecond;

      bindActions();
      writeToStorage();
      setInterval(control, 1000);
    },
  };
})();

jQuery(document).ready(function () {
  if (abp.setting.getBoolean('App.UserManagement.SessionTimeOut.IsEnabled')) {
    InactivityController.init({
      lastActivityTimeStorageKey: 'UserLastActivity',
      InActivityControlSecond: abp.setting.getInt('App.UserManagement.SessionTimeOut.TimeOutSecond'), // show inactivity modal when the TimeOutSecond passed
    });
  }
});
