using MassTransit;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.Services;

public interface IPaymentRetryHandler
{
    Task RetryAsync(BehaviorContext<OrderState, IPaymentFailed> context);
    Task CancelAsync(BehaviorContext<OrderState, IPaymentFailed> context);
}

