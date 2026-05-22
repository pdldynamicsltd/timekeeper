using CadentManagement.MultiTenancy.Dto;

namespace CadentManagement.Web.Models.TenantRegistration;

public class TenantRegisterResultViewModel : RegisterTenantOutput
{
    public string TenantLoginAddress { get; set; }
}

