using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.IntegrationTests.Mocks;
using Xunit;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class AutoHappyPathTests : SagaTestBase
{
    [Fact]
    public async Task AutoHappyPath_OrderDelivered()
    {
        var orderId = GetGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });

        await WaitForState(orderId, "Delivered");

        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Delivered", saga.CurrentState);
        Assert.Equal("123 Default St", saga.Address);
        Assert.Equal(0, saga.PaymentAttempts);
    }

    private static Guid GetGuid(bool stockAvailable = true)
    {
        Guid orderId = Guid.NewGuid();

        // Convert to byte array
        var bytes = orderId.ToByteArray();

        // Update the first element
        bytes[0] = 2;

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

}

