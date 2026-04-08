using MassTransit;
using System;
using System.Threading.Tasks;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Services;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class TestOrderStateMachine : 
    MassTransitStateMachine<OrderState>
{
    public State Created { get; set; } = null!;
    public State StockChecked { get; set; } = null!;
    public State PaymentPending { get; set; } = null!;
    public State PaymentCompleted { get; set; } = null!;
    public State AddressValidated { get; set; } = null!;
    public State Shipped { get; set; } = null!;
    public State Delivered { get; set; } = null!;
    public State Cancelled { get; set; } = null!;

    public Event<IOrderCreated> OrderCreated { get; set; } = null!;
    public Event<IStockChecked> StockCheckedEvent { get; set; } = null!;
    public Event<IPaymentProcessed> PaymentSucceeded { get; set; } = null!;
    public Event<IPaymentFailed> PaymentFailed { get; set; } = null!;
    public Event<IAddressValidated> AddressValidatedEvent { get; set; } = null!;
    public Event<IOrderShipped> OrderShipped { get; set; } = null!;
    public Event<IOrderDelivered> OrderDelivered { get; set; } = null!;
    public Event<IOrderCancelled> OrderCancelled { get; set; } = null!;

    public TestOrderStateMachine(IPaymentRetryPolicy retryPolicy, IPaymentRetryHandler paymentRetryHandler, IOrderInitializService sagaService)
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockCheckedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentSucceeded, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => AddressValidatedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderShipped, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderDelivered, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderCancelled, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderCreated)
                .Then(ctx => sagaService.InitializeOrder(ctx.Saga, ctx.Message))
                .TransitionTo(Created)
            );

        During(Created,
            When(StockCheckedEvent)
                .Then(ctx => sagaService.LogStockCheck(ctx.Saga, ctx.Message))
                .IfElse(ctx => ctx.Message.StockAvailable,
                    success => success
                        .TransitionTo(StockChecked),
                    fail => fail
                        .TransitionTo(Cancelled)
                )
            );

        During(StockChecked,
            When(PaymentSucceeded)
                .TransitionTo(PaymentCompleted)
                .Then(ctx => Console.WriteLine($"Saga: PaymentCompleted for {ctx.Saga.CorrelationId}")),
            When(PaymentFailed)
                .Then(ctx => ctx.Saga.PaymentAttempts++)
                .Then(ctx => paymentRetryHandler.RetryAsync(ctx))
                .TransitionTo(PaymentPending)
            );

        During(PaymentPending,
            When(PaymentSucceeded)
                .TransitionTo(PaymentCompleted)
                .Then(ctx => Console.WriteLine($"Saga: PaymentCompleted for {ctx.Saga.CorrelationId}")),
            When(PaymentFailed)
                .Then(ctx => ctx.Saga.PaymentAttempts++)
                .IfElse(
                    ctx => retryPolicy.ShouldRetry(ctx.Saga),
                    retry => retry
                        .Then(ctx => paymentRetryHandler.RetryAsync(ctx))
                        .TransitionTo(PaymentPending),
                    cancel => cancel
                        .Then(ctx => paymentRetryHandler.CancelAsync(ctx))
                        .PublishAsync(ctx => ctx.Init<IOrderCancelled>(new MockOrderCancelled {
                            OrderId = ctx.Saga.OrderId,
                            Reason = "Max payment attempts reached",
                            CancelledAt = DateTime.UtcNow
                        }))
                        .TransitionTo(Cancelled)
                )
            );

        During(PaymentCompleted,
            When(AddressValidatedEvent)
                .Then(ctx => ctx.Saga.Address = ctx.Message.Address)
                .TransitionTo(AddressValidated)
                .Then(ctx => Console.WriteLine($"Saga: AddressValidated for {ctx.Saga.CorrelationId}"))
            );

        During(AddressValidated,
            When(OrderShipped)
                .TransitionTo(Shipped)
                .Then(ctx => Console.WriteLine($"Saga: OrderShipped for {ctx.Saga.CorrelationId}"))
            );

        During(Shipped,
            When(OrderDelivered)
                .TransitionTo(Delivered)
                .Then(ctx => Console.WriteLine($"Saga: OrderDelivered for {ctx.Saga.CorrelationId}"))
            );

        DuringAny(
            When(OrderCancelled)
                .TransitionTo(Cancelled)
                .Then(ctx => Console.WriteLine($"Saga: OrderCancelled for {ctx.Saga.CorrelationId}"))
            );
    }
}

