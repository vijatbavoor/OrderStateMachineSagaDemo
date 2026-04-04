using MassTransit;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.IntegrationTests.Fixtures;
using OrderStateMachineSagaDemo.Models;
using Xunit;
using System;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class HappyPathTests : SagaTestBase
{
    public HappyPathTests() : base()
    {
    }

    [Fact]
    public async Task HappyPath_OrderDelivered()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Start saga
        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });
        await WaitForState(orderId, "Created");

        // Stock check OK
        await PublishEvent(new MockStockChecked { OrderId = orderId, StockAvailable = true, CheckedAt = DateTime.UtcNow });
        await WaitForState(orderId, "StockChecked");

        // Payment success
        await PublishEvent(new MockPaymentProcessed { OrderId = orderId, TransactionId = "txn123", ProcessedAt = DateTime.UtcNow });
        await WaitForState(orderId, "PaymentCompleted");

        // Address validated
        await PublishEvent(new MockAddressValidated { OrderId = orderId, Address = "123 Test St", ValidatedAt = DateTime.UtcNow });
        await WaitForState(orderId, "AddressValidated");

        // Shipped
        await PublishEvent(new MockOrderShipped { OrderId = orderId, TrackingNumber = "track123", ShippedAt = DateTime.UtcNow });
        await WaitForState(orderId, "Shipped");

        // Delivered
        await PublishEvent(new MockOrderDelivered { OrderId = orderId, DeliveredAt = DateTime.UtcNow });
        await WaitForState(orderId, "Delivered");

        // Assert
        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Delivered", saga.CurrentState);
        Assert.Equal("123 Test St", saga.Address);
        Assert.Equal(0, saga.PaymentAttempts);
    }
}

class MockOrderCreated : IOrderCreated
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

class MockStockChecked : IStockChecked
{
    public Guid OrderId { get; set; }
    public bool StockAvailable { get; set; }
    public DateTime CheckedAt { get; set; }
}

class MockPaymentProcessed : IPaymentProcessed
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = null!;
    public DateTime ProcessedAt { get; set; }
}

class MockAddressValidated : IAddressValidated
{
    public Guid OrderId { get; set; }
    public string Address { get; set; } = null!;
    public DateTime ValidatedAt { get; set; }
}

class MockOrderShipped : IOrderShipped
{
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; } = null!;
    public DateTime ShippedAt { get; set; }
}

class MockOrderDelivered : IOrderDelivered
{
    public Guid OrderId { get; set; }
    public DateTime DeliveredAt { get; set; }
}
