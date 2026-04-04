namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when the courier confirms delivery.
/// </summary>
public interface IOrderDelivered
{
    Guid   OrderId     { get; }
    DateTime DeliveredAt { get; }
}
