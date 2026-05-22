using CadentManagement.Editions;
using CadentManagement.Editions.Dto;
using CadentManagement.MultiTenancy.Payments;
using CadentManagement.Security;
using CadentManagement.MultiTenancy.Payments.Dto;

namespace CadentManagement.Web.Models.TenantRegistration;

public class TenantRegisterViewModel
{
    public int? EditionId { get; set; }

    public EditionSelectDto Edition { get; set; }

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

    public EditionPaymentType EditionPaymentType { get; set; }

    public SubscriptionStartType? SubscriptionStartType { get; set; }

    public PaymentPeriodType? PaymentPeriodType { get; set; }

    public string SuccessUrl { get; set; }

    public string ErrorUrl { get; set; }
}

