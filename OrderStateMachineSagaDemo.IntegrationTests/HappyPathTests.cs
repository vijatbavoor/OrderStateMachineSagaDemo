using OrderStateMachineSagaDemo.Contracts;
using Xunit;

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
        await WaitForState(orderId, "Delivered", 30000);

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
