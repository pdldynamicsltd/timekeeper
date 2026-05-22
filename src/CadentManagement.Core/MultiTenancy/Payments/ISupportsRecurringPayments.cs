using Abp.Events.Bus.Handlers;
using CadentManagement.MultiTenancy.Subscription;

namespace CadentManagement.MultiTenancy.Payments;

public interface ISupportsRecurringPayments :
    IEventHandler<RecurringPaymentsDisabledEventData>,
    IEventHandler<RecurringPaymentsEnabledEventData>,
    IEventHandler<SubscriptionUpdatedEventData>,
    IEventHandler<SubscriptionCancelledEventData>
{

}

