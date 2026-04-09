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
        // Demo decision logic: derive a deterministic "payment success" from the OrderId bytes.
        // Replace this with real payment call/integration as needed.
        var idBytes = context.Saga.OrderId.ToByteArray();
        bool paymentSucceeded = (idBytes.Length > 0 && (idBytes[0] % 2 == 0));
        var currentState = context.Saga.CurrentState;
        Console.WriteLine($"SagaService: OrderId bytes[0]={idBytes[0]} -> paymentSucceeded={paymentSucceeded}");

        if (paymentSucceeded)
        {
            return context.Publish<IPaymentProcessed>(new {
                OrderId = context.Saga.CorrelationId,
                TransactionId = $"txn-{context.Saga.CorrelationId:N}",
                ProcessedAt = DateTime.UtcNow
            });
        }

        return context.Publish<IPaymentFailed>(new {
            OrderId = context.Saga.CorrelationId,
            Reason = "Payment declined",
            FailedAt = DateTime.UtcNow
        });
    }
}

