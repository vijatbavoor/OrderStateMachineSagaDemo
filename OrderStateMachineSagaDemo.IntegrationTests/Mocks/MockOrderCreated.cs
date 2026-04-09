using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockOrderCreated : IOrderCreated
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
