using Abp.Auditing;
using CadentManagement.Configuration.Dto;

namespace CadentManagement.Configuration.Tenants.Dto;

public class TenantEmailSettingsEditDto : EmailSettingsEditDto
{
    public bool UseHostDefaultEmailSettings { get; set; }
}

