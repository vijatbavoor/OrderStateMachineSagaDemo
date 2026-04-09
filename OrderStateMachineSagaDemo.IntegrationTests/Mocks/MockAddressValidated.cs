using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockAddressValidated : IAddressValidated
{
    public Guid OrderId { get; set; }
    public string Address { get; set; } = null!;
    public DateTime ValidatedAt { get; set; }
}
