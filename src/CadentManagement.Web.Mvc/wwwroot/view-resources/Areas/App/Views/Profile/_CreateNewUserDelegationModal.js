(function () {
  app.modals.CreateNewUserDelegationModal = function () {
    var __userDelegationService = abp.services.app.userDelegation;

    var _modalManager;
    var _$form = null;
    var modal = null;

    var openUserSearchModal = function () {
      var lookupModal = app.modals.LookupModal.create({
        title: app.localize('SelectAUser'),
        serviceMethod: abp.services.app.commonLookup.findUsers,
        filterText: '',
        excludeCurrentUser: true,
      });

      lookupModal.open({}, function (selectedItem) {
        var userId = selectedItem.id;
        $('#TargetUserId').val(userId);
        $('#UsernameOrEmailAddress').val(selectedItem.name);
      });
    };

    var $selectedDateTime = {
      startDate: moment().startOf('minute'),
      endDate: moment().startOf('minute').add(1, 'days'),
    };

    this.init = function (modalManager) {
      _modalManager = modalManager;
      modal = _modalManager.getModal();

      _$form = modal.find('form[name=DelegateUserModalForm]');
      _$form.validate({
        ignore: {},
      });

      var $StartTime = modal.find('#StartTime');
      var $EndTime = modal.find('#EndTime');

      $StartTime.daterangepicker(
        {
          timePicker: true,
          singleDatePicker: true,
          parentEl: '#DelegateUserModalForm',
          startDate: moment().startOf('minute'),
          minDate: moment().startOf('minute'),
          locale: {
            format: 'L LT',
          },
        },
        (start) => {
          $selectedDateTime.startDate = start;

          const minEndDate = moment(start).add(1, 'days');
          const endPicker = $EndTime.data('daterangepicker');

          endPicker.minDate = minEndDate;

          if ($selectedDateTime.endDate.isBefore(minEndDate)) {
            $selectedDateTime.endDate = minEndDate;
            endPicker.setStartDate(minEndDate);
            endPicker.setEndDate(minEndDate);
          }

          endPicker.updateView?.();
          endPicker.updateCalendars?.();
        }
      );

      $EndTime.daterangepicker(
        {
          timePicker: true,
          singleDatePicker: true,
          parentEl: '#DelegateUserModalForm',
          startDate: $selectedDateTime.endDate,
          minDate: $selectedDateTime.endDate,
          locale: {
            format: 'L LT',
          },
        },
        (start) => {
          $selectedDateTime.endDate = start;
          $StartTime.data('daterangepicker').maxDate = start;
        }
      );
    };

    $('#ClearUserButton').click(function () {
      _$form.find('input[name=TargetUser]').val('');
      _$form.find('input[name=TargetUserId]').val('');
    });

    $('#OpenUserLookupTableButton').click(function () {
      openUserSearchModal();
    });

    this.save = function () {
      if (!_$form.valid()) {
        return;
      }

      var userDelegation = _$form.serializeFormToObject();

      userDelegation.StartTime = $selectedDateTime.startDate.format('YYYY-MM-DDTHH:mm:ss');
      userDelegation.EndTime = $selectedDateTime.endDate.format('YYYY-MM-DDTHH:mm:ss');

      _modalManager.setBusy(true);
      __userDelegationService
        .delegateNewUser(userDelegation)
        .done(function (result) {
          abp.notify.info(app.localize('SavedSuccessfully'));
          _modalManager.setResult(result);
          _modalManager.close();
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})();
