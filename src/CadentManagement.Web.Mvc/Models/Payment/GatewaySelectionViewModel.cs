using System.Collections.Generic;
using System.Linq;
using CadentManagement.MultiTenancy.Payments;
using CadentManagement.MultiTenancy.Payments.Dto;

namespace CadentManagement.Web.Models.Payment;

public class GatewaySelectionViewModel
{
    public SubscriptionPaymentDto Payment { get; set; }

    public List<PaymentGatewayModel> PaymentGateways { get; set; }

    public bool AllowRecurringPaymentOption()
    {
        return Payment.AllowRecurringPayment() && PaymentGateways.Any(gateway => gateway.SupportsRecurringPayments);
    }
}

