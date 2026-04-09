using OrderStateMachineSagaDemo.Contracts;

namespace OrderStateMachineSagaDemo.IntegrationTests.Mocks;

public class MockStockChecked : IStockChecked
{
    public Guid OrderId { get; set; }
    public bool StockAvailable { get; set; }
    public DateTime CheckedAt { get; set; }
}
