var CurrentPage = (function () {
  var handleLogin = function () {
    var $loginForm = $('form.login-form');
    var $submitButton = $('#kt_login_signin_submit');

    var $passwordInput = $loginForm.find('input[name="password"]');
    var $capsLockIcon = $('#caps-lock-icon');
    var passwordFocused = false;

    function checkCapsLock(e) {
      var originalEvent = e.originalEvent || e;

      if (originalEvent &&
        typeof originalEvent.getModifierState === 'function') {
        
        var capsLockOn = originalEvent.getModifierState('CapsLock');
        
        if (capsLockOn && passwordFocused) {
          $capsLockIcon.removeClass('d-none');
        } else {
          $capsLockIcon.addClass('d-none');
        }
      }
    }

    $passwordInput.on('keydown keyup click', checkCapsLock);

    $passwordInput.on('focus', function () {
      passwordFocused = true;
    });

    $passwordInput.on('blur', function () {
      passwordFocused = false;
      $capsLockIcon.addClass('d-none');
    });

    $submitButton.click(function () {
      trySubmitForm();
    });

    $loginForm.validate({
      rules: {
        username: {
          required: true,
        },
        password: {
          required: true,
        },
      },
    });

    $loginForm.find('input').keypress(function (e) {
      if (e.which === 13) {
        trySubmitForm();
      }
    });

    $('a.social-login-icon').click(function () {
      var $a = $(this);
      var $form = $a.closest('form');
      $form.find('input[name=provider]').val($a.attr('data-provider'));
      $form.submit();
    });

    $loginForm.find('input[name=returnUrlHash]').val(location.hash);

    $('input[type=text]').first().focus();

    function trySubmitForm() {
      if (!$('form.login-form').valid()) {
        return;
      }

      function setCaptchaToken(callback) {
        callback = callback || function () {};
        if (!abp.setting.getBoolean('App.UserManagement.UseCaptchaOnLogin')) {
          callback();
        } else {
          grecaptcha.reExecute(function (token) {
            $('#recaptchaResponse').val(token);
            callback();
          });
        }
      }

      setCaptchaToken(function () {
        abp.ui.setBusy(
          null,
          abp
            .ajax({
              contentType: app.consts.contentTypes.formUrlencoded,
              url: $loginForm.attr('action'),
              data: $loginForm.serialize(),
              abpHandleError: false,
            })
            .fail(function (error) {
              setCaptchaToken();
              abp.ajax.showError(error);
            }),
        );
      });
    }
  };

  return {
    init: function () {
      KTPasswordMeter.createInstances();
      handleLogin();
    },
  };
})();
