(function () {
  app.modals.UserPermissionsModal = function () {
    var _userService = abp.services.app.user;
    var _modalManager;
    var _permissionsTree;

    function _resetUserSpecificPermissions() {
      _modalManager.setBusy(true);
      _userService
        .resetUserSpecificPermissions({ id: _modalManager.getArgs().id })
        .done(function () {
          abp.notify.info(app.localize('ResetSuccessfully'));
          _modalManager.getModal().on('hidden.bs.modal', function () {
            _modalManager.reopen();
          });
          _modalManager.close();
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    }

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();

      _permissionsTree = new PermissionsTree();
      _permissionsTree.init($modal.find('.permission-tree'));

      $modal.find('.reset-permissions-button').click(function () {
        _resetUserSpecificPermissions();
      });

      $modal.find('[data-bs-toggle=tooltip]').tooltip();
    };

    this.save = function () {
      _modalManager.setBusy(true);
      _userService
        .updateUserPermissions({
          id: _modalManager.getArgs().id,
          grantedPermissionNames: _permissionsTree.getSelectedPermissionNames(),
        })
        .done(function () {
          abp.notify.info(app.localize('SavedSuccessfully'));
          _modalManager.close();
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})();
