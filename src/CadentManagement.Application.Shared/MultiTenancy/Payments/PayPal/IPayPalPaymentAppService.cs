using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.MultiTenancy.Payments.PayPal.Dto;

namespace CadentManagement.MultiTenancy.Payments.PayPal;

public interface IPayPalPaymentAppService : IApplicationService
{
    Task ConfirmPayment(long paymentId, string paypalOrderId);

    PayPalConfigurationDto GetConfiguration();
}

