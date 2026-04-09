using OrderStateMachineSagaDemo.Contracts;
using Xunit;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class AutoUnhappyPathTests : AutoSagaTestBase
{
    [Fact]
    public async Task AutoStockFail_CancelsOrder()
    {
        var orderId = NewMethod();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Cancelled");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga.CurrentState);
    }

    private static Guid NewMethod()
    {
        Guid orderId = Guid.NewGuid();

        // Convert to byte array
        var bytes = orderId.ToByteArray();

        // Update the first element
        bytes[0] = 1;

        // Create a new Guid with the modified bytes
        orderId = new Guid(bytes);
        return orderId;
    }

    [Fact]
    public async Task AutoPaymentFail3_CancelsOrder()
    {
        var orderId = NewMethod();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Cancelled");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga.CurrentState);
        Assert.Equal(3, saga.PaymentAttempts);
    }
}

