using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Models;
using System;
using MassTransit;
using System.Threading.Tasks;

namespace OrderStateMachineSagaDemo.Services;

/// <summary>
/// Implementation of the <see cref="OrderInitializService"/>.
/// </summary>
public class OrderInitializService : IOrderInitializService
{
    public void InitializeOrder(OrderState saga, IOrderCreated message)
    {
        saga.CorrelationId = message.OrderId;
        saga.OrderId = message.OrderId;
        Console.WriteLine($"Saga: Order Created for {saga.CorrelationId}");
    }

    public void LogStockCheck(OrderState saga, IStockChecked message)
    {
        string status = message.StockAvailable ? "OK" : "fail";
        string nextState = message.StockAvailable ? "StockChecked" : "Cancelled";
        Console.WriteLine($"Saga: Stock {status} -> {nextState} for {saga.CorrelationId}");
    }

    public Task PublishNextStockCheckedAsync(BehaviorContext<OrderState, IOrderCreated> ctx)
    {
        return ctx.Publish<IStockChecked>(new { OrderId = ctx.Saga.CorrelationId, StockAvailable = true, CheckedAt = DateTime.UtcNow });
    }

    public Task PublishPaymentResultAsync(BehaviorContext<OrderState, IStockChecked> ctx)
    {
        // Demo decision logic: derive a deterministic "payment success" from the OrderId bytes.
        // Replace this with real payment call/integration as needed.
        var idBytes = ctx.Saga.OrderId.ToByteArray();
        bool paymentSucceeded = (idBytes.Length > 0 && (idBytes[0] % 2 == 0));
        var currentState = ctx.Saga.CurrentState;
        Console.WriteLine($"SagaService: OrderId bytes[0]={idBytes[0]} -> paymentSucceeded={paymentSucceeded}");

        if (paymentSucceeded)
        {
            return ctx.Publish<IPaymentProcessed>(new {
                OrderId = ctx.Saga.CorrelationId,
                TransactionId = $"txn-{ctx.Saga.CorrelationId:N}",
                ProcessedAt = DateTime.UtcNow
            });
        }

        return ctx.Publish<IPaymentFailed>(new {
            OrderId = ctx.Saga.CorrelationId,
            Reason = "Payment declined",
            FailedAt = DateTime.UtcNow
        });
    }
}
