using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockOrderShipped : IOrderShipped
{
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; } = null!;
    public DateTime ShippedAt { get; set; }
}
