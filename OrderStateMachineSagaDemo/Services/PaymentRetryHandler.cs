using MassTransit;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Services;

namespace OrderStateMachineSagaDemo.Services;

public class PaymentRetryHandler : IPaymentRetryHandler
{
    public Task CancelAsync(BehaviorContext<OrderState, IPaymentFailed> context)
    {
        Console.WriteLine($"Saga: Payment max attempts -> Cancelled {context.Saga.CorrelationId} (attempts: {context.Saga.PaymentAttempts})");
       return Task.CompletedTask;
    }

    public Task RetryAsync(BehaviorContext<OrderState, IPaymentFailed> context)
    {
        Console.WriteLine($"Saga: Payment fail #{context.Saga.PaymentAttempts} -> PaymentPending (retry) {context.Saga.CorrelationId}");
        return Task.CompletedTask;
    }
}

