using OrderStateMachineSagaDemo.Contracts;
using Xunit;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class AutoUnhappyPathTests : AutoSagaTestBase
{
    [Fact]
    public async Task AutoStockFail_CancelsOrder()
    {
        var orderId = Guid.NewGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await Task.Delay(2000); // allow Created

        await PublishEvent(new MockStockChecked { OrderId = orderId, StockAvailable = false, CheckedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Cancelled");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga.CurrentState);
    }

    [Fact]
    public async Task AutoPaymentFail3_CancelsOrder()
    {
        var orderId = Guid.NewGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await PublishEvent(new MockStockChecked { OrderId = orderId, StockAvailable = true, CheckedAt = DateTime.UtcNow });

        // Publish 3 fails fast to hit max before auto success
        for (int i = 1; i <= 3; i++)
        {
            await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = $"Test fail #{i}", FailedAt = DateTime.UtcNow });
        }

        await WaitForState(orderId, "Cancelled", 20000);

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga.CurrentState);
        Assert.Equal(3, saga.PaymentAttempts);
    }
}

