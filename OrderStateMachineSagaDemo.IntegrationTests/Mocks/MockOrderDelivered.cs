using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockOrderDelivered : IOrderDelivered
{
    public Guid OrderId { get; set; }
    public DateTime DeliveredAt { get; set; }
}
