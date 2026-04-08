using OrderStateMachineSagaDemo.Contracts;
using Xunit;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class AutoHappyPathTests : AutoSagaTestBase
{
    [Fact]
    public async Task AutoHappyPath_OrderDelivered()
    {
        var orderId = Guid.NewGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Delivered");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Delivered", saga.CurrentState);
        Assert.Equal("123 Default St", saga.Address);
        Assert.Equal(0, saga.PaymentAttempts);
    }
}

