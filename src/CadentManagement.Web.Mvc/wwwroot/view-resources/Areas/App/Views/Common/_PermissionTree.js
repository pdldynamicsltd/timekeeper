var PermissionsTree = (function ($) {
  return function () {
    var $tree;
    var currentFilter = '';
    var isOnlyShowEnabledActive = false;

    function initTree($treeContainer, options) {
      $tree = $treeContainer;

      var jsTreeOptions = {
        types: {
          default: { icon: 'fa fa-folder text-warning' },
          file: { icon: 'fa fa-file text-warning' },
        },
        checkbox: {
          keep_selected_style: false,
          three_state: false,
          cascade: '',
        },
        search: {
          show_only_matches: true,
          show_only_matches_children: true,
        },
        plugins: ['checkbox', 'types', 'search'],
      };

      if (options && options.singleSelect) {
        jsTreeOptions.core = { multiple: false };
      }

      $tree.jstree(jsTreeOptions);

      if (!(options && (options.singleSelect || options.disableCascade))) {
        $tree.on('changed.jstree', function (e, data) {
          if (!data.node) return;

          var childrenNodes = $.makeArray($tree.jstree('get_node', data.node).children);
          if (data.node.state.selected) {
            selectNodeAndAllParents($tree.jstree('get_parent', data.node));
            $tree.jstree('select_node', childrenNodes);
          } else {
            $tree.jstree('deselect_node', childrenNodes);
          }
        });
      }
    }

    function initControls() {
      var to = false;
      $('#PermissionTreeFilter').keyup(function () {
        if (to) clearTimeout(to);
        to = setTimeout(function () {
          var v = $('#PermissionTreeFilter').val();
          currentFilter = v;

          if ($tree.jstree(true)) {
            $tree.jstree(true).search(v);

            if (!v && isOnlyShowEnabledActive) {
              showOnlyGrantedPermissions();
            }
          }
        }, 250);
      });

      var $label = $('#permissionsSwitchLabel');
      var enabledText = $label.data('enabled-text');
      var allText = $label.data('all-text');
      var tooltipEnabled = $label.data('tooltip-enabled');
      var tooltipAll = $label.data('tooltip-all');

      $('#onlyShowEnabledPermissionsSwitch').on('change', function () {
        isOnlyShowEnabledActive = $(this).is(':checked');

        var newLabelText;
        var newTooltipText;

        if (isOnlyShowEnabledActive) {
          showOnlyGrantedPermissions();
          newLabelText = allText;
          newTooltipText = tooltipAll;
        } else {
          showAllPermissions();
          newLabelText = enabledText;
          newTooltipText = tooltipEnabled;
        }

        $label.text(newLabelText);
        $label.attr('data-bs-original-title', newTooltipText);

        var tooltip = bootstrap.Tooltip.getInstance($label[0]);
        if (tooltip) {
          tooltip.setContent({ '.tooltip-inner': newTooltipText });
        }
      });

      $('#toggle-expand-collapse-button').on('click', function () {
        var $button = $(this);
        var $icon = $button.find('i');
        var $text = $button.find('span');
        var state = $button.data('state');
        var tooltip = bootstrap.Tooltip.getInstance($button[0]);

        if (state === 'collapsed') {
          $tree.jstree('open_all');

          $icon.removeClass('fa-angles-down').addClass('fa-angles-up');
          $button.data('state', 'expanded');

          var newTitle = $button.data('collapse-title');
          $button.attr('title', newTitle);
          $text.text(newTitle);

        } else {
          $tree.jstree('close_all');

          $icon.removeClass('fa-angles-up').addClass('fa-angles-down');
          $button.data('state', 'collapsed');

          var newTitle = $button.data('expand-title');
          $button.attr('title', newTitle);
          $text.text(newTitle);
        }

        if (tooltip) {
          tooltip.setContent({ '.tooltip-inner': $button.attr('title') });
        }
      });
    }

    function selectNodeAndAllParents(node) {
      $tree.jstree('select_node', node, true);
      var parent = $tree.jstree('get_parent', node);
      if (parent) {
        selectNodeAndAllParents(parent);
      }
    }

    function getSelectedPermissionNames() {
      var permissionNames = [];
      var selectedPermissions = $tree.jstree('get_selected', true);
      for (var i = 0; i < selectedPermissions.length; i++) {
        permissionNames.push(selectedPermissions[i].id);
      }
      return permissionNames;
    }

    function getVisibleNodes() {
      var treeInstance = $tree.jstree(true);
      var allNodes = treeInstance.get_json('#', { flat: true });
      var visibleNodes = [];

      for (var i = 0; i < allNodes.length; i++) {
        var node = allNodes[i];
        if (!treeInstance.is_hidden(node)) {
          visibleNodes.push(node.id);
        }
      }

      return visibleNodes;
    }

    function showOnlyGrantedPermissions() {
      if (!$tree || !$tree.jstree(true)) return;

      var treeInstance = $tree.jstree(true);
      var grantedIds = getSelectedPermissionNames();
      var visibleNodes = getVisibleNodes();

      if (currentFilter) {
        visibleNodes.forEach(function (id) {
          treeInstance.hide_node(id);
        });

        grantedIds.forEach(function (id) {
          if (visibleNodes.indexOf(id) !== -1) {
            treeInstance.show_node(id);
          }
        });
      } else {
        treeInstance.hide_all();
        grantedIds.forEach(function (id) {
          treeInstance.show_node(id);
        });
      }
    }

    function showAllPermissions() {
      if (!$tree || !$tree.jstree(true)) return;

      var treeInstance = $tree.jstree(true);

      if (currentFilter) {
        treeInstance.search(currentFilter);
      } else {
        treeInstance.show_all();
      }
    }

    return {
      init: function ($treeContainer, options) {
        initTree($treeContainer, options);
        initControls();
      },

      getSelectedPermissionNames: getSelectedPermissionNames,

      getTree: function () {
        return $tree.jstree(true);
      },
    };
  };
})(jQuery);