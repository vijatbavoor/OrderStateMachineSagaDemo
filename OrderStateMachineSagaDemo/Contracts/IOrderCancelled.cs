namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when an order is cancelled for any reason.
/// </summary>
public interface IOrderCancelled
{
    Guid   OrderId { get; }
    string Reason  { get; }
    DateTime CancelledAt { get; }
}
