using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.IntegrationTests.Mocks;
using Xunit;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class AutoUnhappyPathTests : SagaTestBase
{
    [Fact]
    public async Task CancelsOrder_When_StockIsNotAvailable()
    {
        var orderId = GetGuid(false);

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Cancelled");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga.CurrentState);
         Assert.Equal(0, saga.PaymentAttempts);
    }

    private static Guid GetGuid(bool stockAvailable = true)
    {
        Guid orderId = Guid.NewGuid();

        // Convert to byte array
        var bytes = orderId.ToByteArray();

        // Update the first element
        bytes[0] = 1;

        if (stockAvailable)
        {
            // Set the last byte to an even value to indicate stock available
            bytes[15] = 2; // Even value
        }
        else
        {
            // Set the last byte to an odd value to indicate stock not available
            bytes[15] = 1; // Odd value
        }

        // Create a new Guid with the modified bytes
        orderId = new Guid(bytes);
        return orderId;
    }

    [Fact]
    public async Task CancelsOrder_When_PaymentFails_For_3_Times()
    {
        var orderId = GetGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Cancelled");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga.CurrentState);
        Assert.Equal(3, saga.PaymentAttempts);
    }
}

