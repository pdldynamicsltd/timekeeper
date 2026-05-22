(function () {
  $(function () {
    var _$policiesTable = $('#PoliciesTable');
    var _rateLimitPolicyService = abp.services.app.rateLimitPolicy;

    var _permissions = {
      create: abp.auth.hasPermission('Pages.Administration.RateLimiting.Create'),
      edit: abp.auth.hasPermission('Pages.Administration.RateLimiting.Edit'),
      delete: abp.auth.hasPermission('Pages.Administration.RateLimiting.Delete'),
    };

    var _createOrEditModal = new app.ModalManager({
      viewUrl: abp.appPath + 'App/RateLimiting/CreateOrEditModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/RateLimiting/_CreateOrEditModal.js',
      modalClass: 'CreateOrEditRateLimitPolicyModal',
    });

    var algorithmLabels = {
      0: app.localize('FixedWindow'),
      1: app.localize('SlidingWindow'),
      2: app.localize('TokenBucket'),
      3: app.localize('Concurrency'),
    };

    var partitionTypeLabels = {
      0: app.localize('ByClientIp'),
      1: app.localize('ByUser'),
      2: app.localize('ByApiKey'),
    };

    // Load initial enabled state
    _rateLimitPolicyService.getIsEnabled().done(function (isEnabled) {
      $('#RateLimitingEnabledToggle').prop('checked', isEnabled);
      updateEnabledLabel(isEnabled);
    });

    function updateEnabledLabel(isEnabled) {
      $('#RateLimitingEnabledLabel').text(
        isEnabled ? app.localize('RateLimitingIsEnabled') : app.localize('RateLimitingIsDisabled')
      );
    }

    $('#RateLimitingEnabledToggle').change(function () {
      var isEnabled = $(this).is(':checked');
      _rateLimitPolicyService.setIsEnabled(isEnabled).done(function () {
        updateEnabledLabel(isEnabled);
        abp.notify.success(
          isEnabled
            ? app.localize('RateLimitingIsEnabled')
            : app.localize('RateLimitingIsDisabled')
        );
      });
    });

    function deletePolicy(policy) {
      abp.message.confirm(
        app.localize('RateLimitPolicyDeleteWarningMessage', policy.name),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            _rateLimitPolicyService
              .delete({ id: policy.id })
              .done(function () {
                getPolicies();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
              });
          }
        }
      );
    }

    function togglePolicyEnabled(policy) {
      _rateLimitPolicyService
        .togglePolicyEnabled({ id: policy.id })
        .done(function () {
          getPolicies();
          abp.notify.success(app.localize('SavedSuccessfully'));
        });
    }

    $('#CreateNewPolicyButton').click(function () {
      _createOrEditModal.open();
    });

    abp.event.on('app.createOrEditRateLimitPolicyModalSaved', function () {
      getPolicies();
    });

    $('#GetPoliciesButton').click(function (e) {
      e.preventDefault();
      getPolicies();
    });

    $('#PoliciesTableFilter').on('keypress', function (e) {
      if (e.which === 13) {
        e.preventDefault();
        getPolicies();
      }
    });

    var dataTable = _$policiesTable.DataTable({
      paging: true,
      serverSide: true,
      processing: true,
      listAction: {
        ajaxFunction: _rateLimitPolicyService.getPolicies,
        inputFilter: function () {
          return {
            filter: $('#PoliciesTableFilter').val(),
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
          data: null,
          orderable: false,
          autoWidth: false,
          defaultContent: '',
          rowAction: {
            text:
              '<i class="fa fa-cog"></i> <span class="d-none d-md-inline-block d-lg-inline-block d-xl-inline-block">' +
              app.localize('Actions') +
              '</span> <span class="caret"></span>',
            items: [
              {
                text: app.localize('Edit'),
                visible: function () {
                  return _permissions.edit;
                },
                action: function (data) {
                  _createOrEditModal.open({ id: data.record.id });
                },
              },
              {
                text: app.localize('Delete'),
                visible: function () {
                  return _permissions.delete;
                },
                action: function (data) {
                  deletePolicy(data.record);
                },
              },
            ],
          },
        },
        {
          targets: 2,
          data: 'name',
          orderable: true,
        },
        {
          targets: 3,
          data: 'algorithm',
          orderable: false,
          render: function (algorithm) {
            return '<span class="badge badge-info">' + algorithmLabels[algorithm] + '</span>';
          },
        },
        {
          targets: 4,
          data: 'partitionType',
          orderable: false,
          render: function (partitionType) {
            return partitionTypeLabels[partitionType];
          },
        },
        {
          targets: 5,
          data: 'permitLimit',
          orderable: false,
        },
        {
          targets: 6,
          data: 'windowInSeconds',
          orderable: false,
        },
        {
          targets: 7,
          data: 'isGlobal',
          orderable: false,
          render: function (isGlobal) {
            return isGlobal
              ? '<span class="badge badge-success">' + app.localize('Yes') + '</span>'
              : '<span class="badge badge-secondary">' + app.localize('No') + '</span>';
          },
        },
        {
          targets: 8,
          data: 'isEnabled',
          orderable: false,
          render: function (isEnabled, type, row) {
            var checked = isEnabled ? 'checked' : '';
            return (
              '<div class="form-check form-switch">' +
              '<input class="form-check-input toggle-policy-enabled" type="checkbox" ' +
              checked +
              ' data-policy-id="' +
              row.id +
              '">' +
              '</div>'
            );
          },
        },
      ],
    });

    _$policiesTable.on('change', '.toggle-policy-enabled', function () {
      var policyId = $(this).data('policy-id');
      togglePolicyEnabled({ id: policyId });
    });

    function getPolicies() {
      dataTable.ajax.reload();
    }
  });
})();
