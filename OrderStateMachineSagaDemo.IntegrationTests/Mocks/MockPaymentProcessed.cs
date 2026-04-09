using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockPaymentProcessed : IPaymentProcessed
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = null!;
    public DateTime ProcessedAt { get; set; }
}
