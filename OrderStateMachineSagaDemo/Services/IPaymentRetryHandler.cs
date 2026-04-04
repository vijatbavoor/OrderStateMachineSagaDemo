using MassTransit;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.Services;

public interface IPaymentRetryHandler
{
    Task HandleAsync(BehaviorContext<OrderState, IPaymentFailed> context);
}

