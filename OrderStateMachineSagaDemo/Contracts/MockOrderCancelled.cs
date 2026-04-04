using System;

namespace OrderStateMachineSagaDemo.Contracts;

public class MockOrderCancelled : IOrderCancelled
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime CancelledAt { get; set; }
}
