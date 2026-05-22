using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.MultiTenancy.Payments.Dto;
using CadentManagement.MultiTenancy.Payments.Stripe.Dto;

namespace CadentManagement.MultiTenancy.Payments.Stripe;

public interface IStripePaymentAppService : IApplicationService
{
    Task ConfirmPayment(StripeConfirmPaymentInput input);

    StripeConfigurationDto GetConfiguration();

    Task<string> CreatePaymentSession(StripeCreatePaymentSessionInput input);
}

