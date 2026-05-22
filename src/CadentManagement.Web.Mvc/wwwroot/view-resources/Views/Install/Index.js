(function ($) {
    $(function () {
        var _$smtpCredentialFormGroups = $(
            'smtp-group'
        ).closest('.form-group');

        function toggleSmtpCredentialFormGroups() {
            if ($('#Settings_SmtpUseAuthentication').is(':checked')) {
                _$smtpCredentialFormGroups.slideUp('fast');
                _$smtpCredentialFormGroups.find('input').prop('disabled', true);
            } else {
                _$smtpCredentialFormGroups.slideDown('fast');
                _$smtpCredentialFormGroups.find('input').prop('disabled', false);
            }
        }

        toggleSmtpCredentialFormGroups();

        $('#Settings_SmtpUseAuthentication').change(function () {
            toggleSmtpCredentialFormGroups();
        });
    });
})(jQuery);
