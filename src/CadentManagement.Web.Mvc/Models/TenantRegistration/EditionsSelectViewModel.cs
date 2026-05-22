using System.Collections.Generic;
using System.Linq;
using CadentManagement.MultiTenancy.Dto;
using CadentManagement.MultiTenancy.Payments;

namespace CadentManagement.Web.Models.TenantRegistration;

public class EditionsSelectViewModel : EditionsSelectOutput
{
    public List<PaymentPeriodType> GetAvailablePaymentPeriodTypes()
    {
        var result = new List<PaymentPeriodType>();

        if (EditionsWithFeatures.Any(e => e.Edition.MonthlyPrice.HasValue))
        {
            result.Add(PaymentPeriodType.Monthly);
        }

        if (EditionsWithFeatures.Any(e => e.Edition.AnnualPrice.HasValue))
        {
            result.Add(PaymentPeriodType.Annual);
        }

        return result;
    }
}

