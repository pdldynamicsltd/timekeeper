(function () {
  app.modals.CreateOrEditRateLimitPolicyModal = function () {
    var _modalManager;
    var _rateLimitPolicyService = abp.services.app.rateLimitPolicy;
    var _$policyForm = null;

    this.init = function (modalManager) {
      _modalManager = modalManager;
      var $modal = _modalManager.getModal();
      _$policyForm = $modal.find('form[name=PolicyForm]');
      _$policyForm.validate({ ignore: '' });

      var $algorithm = $modal.find('#PolicyAlgorithm');
      var $isGlobal = $modal.find('#PolicyIsGlobal');

      function updateFieldVisibility() {
        var algorithm = parseInt($algorithm.val());

        // FixedWindow = 0, SlidingWindow = 1, TokenBucket = 2, Concurrency = 3
        var showWindow = algorithm === 0 || algorithm === 1;
        var showSlidingWindow = algorithm === 1;
        var showTokenBucket = algorithm === 2;

        $modal.find('.window-field').toggle(showWindow);
        $modal.find('.sliding-window-field').toggle(showSlidingWindow);
        $modal.find('.token-bucket-field').toggle(showTokenBucket);

        // Toggle required attributes based on visibility
        $modal.find('#PolicyWindowInSeconds').prop('required', showWindow);
        $modal.find('#PolicySegmentsPerWindow').prop('required', showSlidingWindow);
        $modal.find('#PolicyTokensPerPeriod').prop('required', showTokenBucket);
        $modal.find('#PolicyReplenishmentPeriod').prop('required', showTokenBucket);
      }

      function updateEndpointPatternVisibility() {
        var isGlobal = $isGlobal.is(':checked');
        $modal.find('#EndpointPatternDiv').toggle(!isGlobal);
      }

      $algorithm.change(updateFieldVisibility);
      $isGlobal.change(updateEndpointPatternVisibility);

      updateFieldVisibility();
      updateEndpointPatternVisibility();
    };

    this.save = function () {
      if (!_$policyForm.valid()) {
        return;
      }

      var policy = _$policyForm.serializeFormToObject();

      // Ensure boolean fields are properly set (unchecked checkboxes don't serialize)
      policy.isEnabled = _$policyForm.find('#PolicyIsEnabled').is(':checked');
      policy.isGlobal = _$policyForm.find('#PolicyIsGlobal').is(':checked');

      _modalManager.setBusy(true);
      _rateLimitPolicyService
        .createOrEdit(policy)
        .done(function () {
          abp.notify.info(app.localize('SavedSuccessfully'));
          _modalManager.close();
          abp.event.trigger('app.createOrEditRateLimitPolicyModalSaved');
        })
        .always(function () {
          _modalManager.setBusy(false);
        });
    };
  };
})();
