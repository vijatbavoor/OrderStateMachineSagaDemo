namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when the order leaves the warehouse.
/// </summary>
public interface IOrderShipped
{
    Guid   OrderId        { get; }
    string TrackingNumber { get; }
    DateTime ShippedAt    { get; }
}
