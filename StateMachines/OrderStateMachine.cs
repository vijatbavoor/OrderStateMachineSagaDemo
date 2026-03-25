using MassTransit;
using System;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Models;

namespace OrderStateMachineSagaDemo.StateMachines;

public class OrderStateMachine : 
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

    public OrderStateMachine()
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
                .Then(ctx => {
                    ctx.Saga.CorrelationId = ctx.Message.OrderId;
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                })
                .TransitionTo(Created)
                .Then(ctx => Console.WriteLine($"Saga: Order Created for {ctx.Saga.CorrelationId}"))
                );

        During(Created,
            When(StockCheckedEvent)
                .Then(ctx => {
                    string status = ctx.Message.StockAvailable ? "OK" : "fail";
                    string nextState = ctx.Message.StockAvailable ? "StockChecked" : "Cancelled";
                    Console.WriteLine($"Saga: Stock {status} -> {nextState} for {ctx.Saga.CorrelationId}");
                })
                .TransitionTo(StockChecked)
                );

        During(StockChecked,
            When(PaymentFailed)
                .Then(ctx => {
                    ctx.Saga.PaymentAttempts++;
                    if (ctx.Saga.PaymentAttempts >= 3)
                    {
                        ctx.Saga.CurrentState = "Cancelled";
                        Console.WriteLine($"Saga: Payment max attempts -> Cancelled {ctx.Saga.CorrelationId} (attempts: {ctx.Saga.PaymentAttempts})");
                    }
                    else
                    {
                        ctx.Saga.CurrentState = "PaymentPending";
                        Console.WriteLine($"Saga: Payment fail #{ctx.Saga.PaymentAttempts} -> PaymentPending {ctx.Saga.CorrelationId}");
                    }
                })
                );

        During(PaymentPending,
            When(PaymentSucceeded)
                .TransitionTo(PaymentCompleted)
                .Then(ctx => Console.WriteLine($"Saga: PaymentCompleted for {ctx.Saga.CorrelationId}"))
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
