using MassTransit;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Services;

namespace OrderStateMachineSagaDemo.Services;

public class PaymentRetryHandler : IPaymentRetryHandler
{
    public async Task HandleAsync(BehaviorContext<OrderState, IPaymentFailed> context)
    {
        context.Saga.PaymentAttempts++;
        if (context.Saga.PaymentAttempts >= 3)
        {
            context.Saga.CurrentState = "Cancelled";
            Console.WriteLine($"Saga: Payment max attempts -> Cancelled {context.Saga.CorrelationId} (attempts: {context.Saga.PaymentAttempts})");
        }
        else
        {
            context.Saga.CurrentState = "PaymentPending";
            Console.WriteLine($"Saga: Payment fail #{context.Saga.PaymentAttempts} -> PaymentPending {context.Saga.CorrelationId}");
        }
        await Task.CompletedTask;
    }
}

