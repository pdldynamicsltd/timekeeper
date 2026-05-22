(function () {
  app.modals.MySettingsModal = function () {
    var _profileService = abp.services.app.profile;
    var _initialTimezone = null;

    var _modalManager;
    var _$form = null;
    var _currentEmail = null;

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _currentEmail = $modal.find('#EmailAddress').val();

      let twoFactorModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Profile/TwoFactorAuthenticationModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_TwoFactorAuthenticationModal.min.js',
        modalClass: 'TwoFactorAuthenticationModal',
      });

      $('#enableTwoFactorAuthenticationButton').on('click', (e) => {
        twoFactorModal.open();
      });

      abp.event.on('app.profile.twoFactorAuthenticationEnabled', () => {
        $('#two_factor_enabled_section').removeClass('d-none');
        $('#two_factor_disabled_section').addClass('d-none');
      });

      abp.event.on('app.profile.twoFactorAuthenticationDisabled', () => {
        $('#two_factor_enabled_section').addClass('d-none');
        $('#two_factor_disabled_section').removeClass('d-none');
      });

      var $viewRecoveryCodes = $modal.find('#btnViewRecoveryCodes');

      var viewRecoveryCodesModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Profile/ViewRecoveryCodesModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_ViewRecoveryCodesModal.min.js',
        modalClass: 'ViewRecoveryCodesModal',
      });

      $viewRecoveryCodes.click(function () {
        viewRecoveryCodesModal.open();
      });

      var $removeAuthenticator = $modal.find('#btnRemoveAuthenticator');

      var removeAuthenticatorModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Profile/RemoveAuthenticatorModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_RemoveAuthenticatorModal.min.js',
        modalClass: 'RemoveAuthenticatorModal',
      });

      $removeAuthenticator.click(function () {
        removeAuthenticatorModal.open();
      });

      _$form = $modal.find('form[name=MySettingsModalForm]');
      _$form.validate();

      _initialTimezone = _$form.find("[name='Timezone']").val();

      var $btnEnableGoogleAuthenticator = $modal.find('#btnEnableGoogleAuthenticator');

      $btnEnableGoogleAuthenticator.click(function () {
        _profileService
          .updateGoogleAuthenticatorKey()
          .done(function (result) {
            $modal.find('.google-authenticator-enable').show();
            $modal.find('.google-authenticator-disable').hide();
            $modal.find('img').attr('src', result.qrCodeSetupImageUrl);
          })
          .always(function () {
            _modalManager.setBusy(false);
          });
      });

      var $btnDisableGoogleAuthenticator = $modal.find('#btnDisableGoogleAuthenticator');

      $btnDisableGoogleAuthenticator.click(function () {
        let code = $modal.find('#GoogleAuthenticatorCode').val();

        _profileService
          .disableGoogleAuthenticator({ Code: code })
          .done(function (result) {
            $modal.find('.google-authenticator-enable').hide();
            $modal.find('.google-authenticator-disable').show();
            $modal.find('img').attr('src', '');
          })
          .always(function () {
            _modalManager.setBusy(false);
          });
      });

      var $SmsVerification = $modal.find('#btnSmsVerification');
      var smsVerificationModal = new app.ModalManager({
        viewUrl: abp.appPath + 'App/Profile/SmsVerificationModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_SmsVerificationModal.min.js',
        modalClass: 'SmsVerificationModal',
      });

      $SmsVerification.click(function () {
        _profileService.sendVerificationSms({ phoneNumber: $('#PhoneNumber').val() }).done(function () {
          smsVerificationModal.open({}, function () {
            $('#SpanSmsVerificationVerified').show();
            $('#btnSmsVerification').attr('disabled', true);
            _$form.find('.tooltips').tooltip();
          });
        });
      });

      _$form.find('.tooltips').tooltip();
      $('#PhoneNumber').keyup(function () {
        if ($('#savedPhoneNumber').val() != $(this).val() || $('#isPhoneNumberConfirmed').val() == false) {
          $('#SpanSmsVerificationVerified').hide();
          $('#btnSmsVerification').removeAttr('disabled');
        } else {
          $('#SpanSmsVerificationVerified').show();
          $('#btnSmsVerification').attr('disabled', true);
        }
      });

      initExternalLoginsTab();
    };

    var _providers = [];
    var _busyProvider = null;
    var _mergeArgs = null;

    function initExternalLoginsTab() {
      var $modal = _modalManager.getModal();

      $modal.find('#MergeCancelButton').on('click', hideMergePanel);
      $modal.find('#MergeSubmitButton').on('click', submitMerge);
      $modal.find('#MergeExternalLoginPassword').on('keypress', function (e) {
        if (e.which === 13) {
          submitMerge();
        }
      });

      $('a[href="#ExternalLoginsTab"]').on('shown.bs.tab', function () {
        loadExternalLogins();
      });

      handleLinkRedirectResult();
    }

    function showMergePanel(args) {
      _mergeArgs = args;
      var $modal = _modalManager.getModal();
      var prompt = app.localize('ExternalLoginAlreadyLinkedMergePrompt')
        .replace('{0}', args.providerName)
        .replace('{1}', args.existingUserEmail);
      $modal.find('#MergeProviderMessage').text(prompt);
      $modal.find('#MergeExternalLoginPassword').val('');
      $modal.find('#ExternalLoginsContainer').addClass('d-none');
      $modal.find('#MergeExternalLoginPanel').removeClass('d-none');
      setTimeout(function () { $modal.find('#MergeExternalLoginPassword').trigger('focus'); }, 100);
    }

    function hideMergePanel() {
      var $modal = _modalManager.getModal();
      $modal.find('#MergeExternalLoginPanel').addClass('d-none');
      $modal.find('#ExternalLoginsContainer').removeClass('d-none');
      _mergeArgs = null;
    }

    function submitMerge() {
      var $modal = _modalManager.getModal();
      var password = $modal.find('#MergeExternalLoginPassword').val();
      if (!password) {
        abp.notify.warn(app.localize('ThisFieldIsRequired'));
        return;
      }

      _modalManager.setBusy(true);

      var url = _mergeArgs.mergeType === 'serverSide'
        ? abp.appPath + 'Account/MergeAndLinkExternalLoginServerSide'
        : abp.appPath + 'api/services/app/ExternalLoginLink/MergeAndLinkExternalLogin';

      var requestData = _mergeArgs.mergeType === 'serverSide'
        ? {
          mergeToken: _mergeArgs.mergeToken,
          targetUserPassword: password,
        }
        : {
          authProvider: _mergeArgs.authProvider,
          providerKey: _mergeArgs.providerKey,
          providerAccessCode: _mergeArgs.providerAccessCode,
          targetUserPassword: password,
        };

      abp.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
      }).done(function () {
        _modalManager.setBusy(false);
        var providerName = _mergeArgs.providerName;
        hideMergePanel();
        abp.notify.success(app.localize('ExternalLoginLinkedSuccessfully', providerName));
        loadExternalLogins();
      }).fail(function () {
        _modalManager.setBusy(false);
      });
    }

    function handleLinkRedirectResult() {
      var stored = sessionStorage.getItem('externalLoginLinkResult');
      if (!stored) return;

      sessionStorage.removeItem('externalLoginLinkResult');

      var data;
      try {
        data = JSON.parse(stored);
      } catch (e) {
        return;
      }

      var linkResult = data.linkResult;
      var linkProvider = data.linkProvider;

      if (!linkResult || !linkProvider) return;

      if (linkResult === 'success') {
        abp.notify.success(app.localize('ExternalLoginLinkedSuccessfully', linkProvider));
        $('a[href="#ExternalLoginsTab"]').tab('show');
      } else if (linkResult === 'merge') {
        $('a[href="#ExternalLoginsTab"]').tab('show');
        showServerSideMergeModal(linkProvider, data.linkMergeToken || '', data.linkExistingEmail || '');
      } else if (linkResult === 'alreadyLinked') {
        abp.message.warn(app.localize('ExternalLoginAlreadyLinkedToAnotherUser'));
        $('a[href="#ExternalLoginsTab"]').tab('show');
      } else if (linkResult === 'error') {
        abp.notify.error(app.localize('AnErrorOccurred'));
        $('a[href="#ExternalLoginsTab"]').tab('show');
      }
    }

    function showServerSideMergeModal(providerName, mergeToken, existingUserEmail) {
      showMergePanel({
        mergeType: 'serverSide',
        providerName: providerName,
        existingUserEmail: existingUserEmail,
        mergeToken: mergeToken,
      });
    }

    function loadExternalLogins() {
      var $container = $('#ExternalLoginsContainer');
      $container.html('<div class="d-flex justify-content-center py-10"><i class="fa fa-spinner fa-spin fa-2x"></i></div>');

      abp.ajax({
        url: abp.appPath + 'api/services/app/ExternalLoginLink/GetExternalLogins',
        type: 'GET',
      }).done(function (result) {
        _providers = result;
        renderExternalLogins();
      }).fail(function () {
        $container.html('<div class="alert alert-danger">' + app.localize('AnErrorOccurred') + '</div>');
      });
    }

    function renderExternalLogins() {
      var $container = $('#ExternalLoginsContainer');

      if (!_providers || _providers.length === 0) {
        $container.html(
          '<div class="notice d-flex bg-light-info rounded border-info border border-dashed p-6">' +
          '<i class="fa fa-info-circle fs-2 text-info me-4"></i>' +
          '<div class="d-flex flex-column"><span class="fs-6">' + app.localize('NoExternalLoginProvidersAvailable') + '</span></div></div>'
        );
        return;
      }

      var html = '<div class="d-flex flex-column gap-4">';
      for (var i = 0; i < _providers.length; i++) {
        var p = _providers[i];
        var providerName = p.name || '';
        var providerNameHtml = htmlEncode(providerName);
        var providerNameAttribute = htmlAttributeEncode(providerName);
        var borderClass = p.isLinked ? 'border-success' : 'border-dashed';
        var iconClass = getProviderIconClass(providerName);
        var linkedEmailHtml = p.isLinked && p.emailAddress
          ? '<div class="text-muted fs-8 mt-1"><i class="fa fa-envelope me-1"></i>' + htmlEncode(p.emailAddress) + '</div>'
          : '';
        var statusHtml = p.isLinked
          ? '<span class="badge badge-light-success ms-2">' + app.localize('ExternalLoginLinked') + '</span>' + linkedEmailHtml
          : '<div class="text-muted fs-7">' + app.localize('ExternalLoginNotLinked') + '</div>';
        var buttonHtml;
        if (p.isLinked) {
          if (p.canUnlink) {
            buttonHtml = '<button type="button" class="btn btn-sm btn-light-danger btn-unlink-provider" data-provider="' + providerNameAttribute + '"><i class="fa fa-unlink me-1"></i>' + app.localize('UnlinkProvider') + '</button>';
          } else {
            buttonHtml = '<div class="text-end"><button type="button" class="btn btn-sm btn-light-danger" disabled><i class="fa fa-unlink me-1"></i>' + app.localize('UnlinkProvider') + '</button><div class="text-muted fs-8 mt-1">' + app.localize('CannotUnlinkLastLoginSetPassword') + '</div></div>';
          }
        } else {
          buttonHtml = '<button type="button" class="btn btn-sm btn-light-primary btn-link-provider" data-provider="' + providerNameAttribute + '"><i class="fa fa-link me-1"></i>' + app.localize('Link') + '</button>';
        }

        html += '<div class="d-flex align-items-center justify-content-between p-4 rounded border ' + borderClass + '">' +
          '<div class="d-flex align-items-center">' +
          '<div class="me-4"><i class="fab fa-' + getProviderIconName(providerName) + ' fs-1 ' + iconClass + '"></i></div>' +
          '<div><span class="fw-bold fs-5">' + providerNameHtml + '</span>' + statusHtml + '</div>' +
          '</div>' +
          '<div>' + buttonHtml + '</div>' +
          '</div>';
      }
      html += '</div>';

      $container.html(html);

      $container.find('.btn-link-provider').click(function () {
        var providerName = $(this).data('provider');
        linkProvider(providerName);
      });

      $container.find('.btn-unlink-provider').click(function () {
        var providerName = $(this).data('provider');
        unlinkProvider(providerName);
      });
    }

    function htmlEncode(value) {
      return $('<div />').text(value || '').html();
    }

    function htmlAttributeEncode(value) {
      return htmlEncode(value).replace(/"/g, '&quot;');
    }

    function getProviderIconClass(name) {
      switch (name) {
        case 'Facebook': return 'text-primary';
        case 'Google': return 'text-danger';
        case 'Microsoft': return 'text-info';
        case 'Twitter': return 'text-info';
        default: return 'text-dark';
      }
    }

    function getProviderIconName(name) {
      switch (name) {
        case 'Facebook': return 'facebook';
        case 'Google': return 'google';
        case 'Microsoft': return 'microsoft';
        case 'Twitter': return 'twitter';
        case 'OpenIdConnect': return 'openid';
        case 'WsFederation': return 'windows';
        default: return (name || '').toLowerCase().replace(/[^a-z0-9-]/g, '');
      }
    }

    function getProviderByName(name) {
      for (var i = 0; i < _providers.length; i++) {
        if (_providers[i].name === name) return _providers[i];
      }
      return null;
    }

    function linkProvider(providerName) {
      var provider = getProviderByName(providerName);
      if (!provider) return;

      _busyProvider = providerName;

      getExternalLoginToken(provider, function (tokenResult) {
        abp.ajax({
          url: abp.appPath + 'api/services/app/ExternalLoginLink/LinkExternalLogin',
          type: 'POST',
          data: JSON.stringify({
            authProvider: providerName,
            providerKey: tokenResult.providerKey,
            providerAccessCode: tokenResult.providerAccessCode,
          }),
          contentType: 'application/json',
        }).done(function (result) {
          _busyProvider = null;

          if (result.success) {
            abp.notify.success(app.localize('ExternalLoginLinkedSuccessfully', providerName));
            loadExternalLogins();
          } else if (result.providerAlreadyLinkedToAnotherUser && result.canMerge) {
            showMergeModal(providerName, tokenResult.providerKey, tokenResult.providerAccessCode, result.existingUserEmail);
          } else if (result.providerAlreadyLinkedToAnotherUser) {
            abp.message.warn(app.localize('ExternalLoginAlreadyLinkedToAnotherUser'));
          }
        }).fail(function () {
          _busyProvider = null;
        });
      }, function () {
        _busyProvider = null;
      });
    }

    function unlinkProvider(providerName) {
      abp.message.confirm(
        app.localize('UnlinkExternalLoginConfirmation', providerName),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (!isConfirmed) return;

          _busyProvider = providerName;

          abp.ajax({
            url: abp.appPath + 'api/services/app/ExternalLoginLink/UnlinkExternalLogin',
            type: 'POST',
            data: JSON.stringify({ authProvider: providerName }),
            contentType: 'application/json',
          }).done(function (result) {
            _busyProvider = null;

            if (result.success) {
              abp.notify.success(app.localize('ExternalLoginUnlinkedSuccessfully', providerName));
              loadExternalLogins();
            } else if (result.requiresPasswordSetup) {
              abp.message.warn(app.localize('CannotUnlinkLastLogin'));
            }
          }).fail(function () {
            _busyProvider = null;
          });
        }
      );
    }

    function showMergeModal(providerName, providerKey, providerAccessCode, existingUserEmail) {
      showMergePanel({
        mergeType: 'clientSide',
        providerName: providerName,
        existingUserEmail: existingUserEmail,
        authProvider: providerName,
        providerKey: providerKey,
        providerAccessCode: providerAccessCode,
      });
    }

    function getExternalLoginToken(provider, successCallback, errorCallback) {
      var providerName = provider.name;

      if (providerName === 'Facebook') {
        getFacebookToken(provider, successCallback, errorCallback);
      } else if (providerName === 'Google') {
        getGoogleToken(provider, successCallback, errorCallback);
      } else if (providerName === 'Microsoft') {
        getMicrosoftToken(provider, successCallback, errorCallback);
      } else if (providerName === 'WsFederation') {
        getWsFederationToken(provider, successCallback, errorCallback);
      } else if (providerName === 'OpenIdConnect' || providerName === 'Twitter') {
        startServerSideLinkFlow(providerName);
        errorCallback();
      } else {
        abp.message.warn(app.localize('ProviderRequiresRedirect'));
        errorCallback();
      }
    }

    function getFacebookToken(provider, successCallback, errorCallback) {
      loadScript('//connect.facebook.net/en_US/sdk.js', function () {
        FB.init({ appId: provider.clientId, cookie: false, xfbml: true, version: 'v2.5' });
        FB.login(function (response) {
          if (response.status === 'connected') {
            successCallback({
              providerKey: response.authResponse.userID,
              providerAccessCode: response.authResponse.accessToken,
            });
          } else {
            errorCallback();
          }
        }, { scope: 'email' });
      });
    }

    function getGoogleToken(provider, successCallback, errorCallback) {
      loadScripts(['https://apis.google.com/js/api.js', 'https://accounts.google.com/gsi/client'], function () {
        gapi.load('client', function () {
          gapi.client.init({}).then(function () {
            gapi.client.load('oauth2', 'v2', function () {
              var tokenClient = google.accounts.oauth2.initTokenClient({
                client_id: provider.clientId,
                scope: 'openid profile email',
                callback: function (resp) {
                  if (resp.error !== undefined) {
                    errorCallback();
                    return;
                  }
                  gapi.client.oauth2.userinfo.get().execute(function (userInfo) {
                    successCallback({
                      providerKey: userInfo.id,
                      providerAccessCode: resp.access_token,
                    });
                  });
                },
              });

              if (gapi.client.getToken() === null) {
                tokenClient.requestAccessToken({ prompt: 'consent' });
              } else {
                tokenClient.requestAccessToken({ prompt: '' });
              }
            });
          });
        });
      });
    }

    function getMicrosoftToken(provider, successCallback, errorCallback) {
      loadScript('https://alcdn.msauth.net/browser/2.30.0/js/msal-browser.min.js', function () {
        var msalInstance = new msal.PublicClientApplication({
          auth: { clientId: provider.clientId, redirectUri: window.location.origin },
        });

        msalInstance.initialize().then(function () {
          return msalInstance.loginPopup({ scopes: ['user.read'] });
        }).then(function () {
          return msalInstance.acquireTokenSilent({
            account: msalInstance.getAllAccounts()[0],
            scopes: ['user.read'],
          });
        }).then(function (accessTokenResponse) {
          successCallback({
            providerKey: accessTokenResponse.uniqueId,
            providerAccessCode: accessTokenResponse.accessToken,
          });
        }).catch(function (error) {
          errorCallback();
        });
      });
    }

    function getWsFederationToken(provider, successCallback, errorCallback) {
      loadScript('https://alcdn.msauth.net/browser/2.30.0/js/msal-browser.min.js', function () {
        var authority = provider.additionalParams && provider.additionalParams['Authority']
          ? provider.additionalParams['Authority']
          : provider.additionalParams && provider.additionalParams['Tenant']
            ? 'https://login.microsoftonline.com/' + provider.additionalParams['Tenant'] + '/v2.0'
            : 'https://login.microsoftonline.com/common/v2.0';

        var msalInstance = new msal.PublicClientApplication({
          auth: {
            clientId: provider.clientId,
            authority: authority,
            redirectUri: window.location.origin,
          },
        });

        msalInstance.initialize().then(function () {
          msalInstance.clearCache();
          return msalInstance.loginPopup({ scopes: ['openid', 'profile', 'email'] });
        }).then(function (response) {
          var providerKey = response.idTokenClaims && response.idTokenClaims['sub']
            ? response.idTokenClaims['sub']
            : response.uniqueId;

          successCallback({
            providerKey: providerKey,
            providerAccessCode: response.idToken,
          });
        }).catch(function (error) {
          errorCallback();
        });
      });
    }

    function startServerSideLinkFlow(providerName) {
      window.location.href = abp.appPath + 'Account/ExternalLoginLink?provider=' + encodeURIComponent(providerName);
    }

    function loadScript(url, callback) {
      var script = document.createElement('script');
      script.type = 'text/javascript';
      script.src = url;
      script.onload = callback;
      script.onerror = callback;
      document.head.appendChild(script);
    }

    function loadScripts(urls, callback) {
      var loaded = 0;
      for (var i = 0; i < urls.length; i++) {
        loadScript(urls[i], function () {
          loaded++;
          if (loaded === urls.length) callback();
        });
      }
    }

    this.save = function () {
      if (!_$form.valid()) {
        return;
      }

      var profile = _$form.serializeFormToObject();

      _modalManager.setBusy(true);
      _profileService
        .updateCurrentUserProfile(profile)
        .done(function () {
          $('#HeaderCurrentUserName').text(profile.UserName);

          if (profile.EmailAddress !== _currentEmail) {
            abp.notify.info(app.localize('ChangeEmailRequestSentMessage'));
          } else {
            abp.notify.info(app.localize('SavedSuccessfully'));
          }

          _modalManager.close();

          var newTimezone = _$form.find("[name='Timezone']").val();

          if (abp.clock.provider.supportsMultipleTimezone && _initialTimezone !== newTimezone) {
            abp.message.info(app.localize('TimeZoneSettingChangedRefreshPageNotification')).done(function () {
              window.location.reload();
            });
          }
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})();
