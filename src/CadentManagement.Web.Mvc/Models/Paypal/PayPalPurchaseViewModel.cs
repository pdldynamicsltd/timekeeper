using System.Linq;
using CadentManagement.MultiTenancy.Payments.Dto;
using CadentManagement.MultiTenancy.Payments.Paypal;

namespace CadentManagement.Web.Models.Paypal;

public class PayPalPurchaseViewModel
{
    public SubscriptionPaymentDto Payment { get; set; }

    public decimal Amount { get; set; }

    public PayPalPaymentGatewayConfiguration Configuration { get; set; }

    public string GetDisabledFundingsQueryString()
    {
        if (Configuration.DisabledFundings == null || !Configuration.DisabledFundings.Any())
        {
            return "";
        }

        return "&disable-funding=" + string.Join(',', Configuration.DisabledFundings.ToList());
    }
}

