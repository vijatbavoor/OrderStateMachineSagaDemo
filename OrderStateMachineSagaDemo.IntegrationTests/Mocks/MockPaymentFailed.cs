using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockPaymentFailed : IPaymentFailed
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime FailedAt { get; set; }
}
