using Abp.Configuration;

namespace CadentManagement.Timing.Dto;

public class GetTimezoneComboboxItemsInput
{
    public SettingScopes DefaultTimezoneScope;

    public string SelectedTimezoneId { get; set; }
}

