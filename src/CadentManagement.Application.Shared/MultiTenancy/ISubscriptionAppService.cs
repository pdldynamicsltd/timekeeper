using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.MultiTenancy.Dto;
using CadentManagement.MultiTenancy.Payments.Dto;

namespace CadentManagement.MultiTenancy;

public interface ISubscriptionAppService : IApplicationService
{
    Task DisableRecurringPayments();

    Task EnableRecurringPayments();

    Task<long> StartExtendSubscription(StartExtendSubscriptionInput input);

    Task<StartUpgradeSubscriptionOutput> StartUpgradeSubscription(StartUpgradeSubscriptionInput input);

    Task<long> StartTrialToBuySubscription(StartTrialToBuySubscriptionInput input);
}

