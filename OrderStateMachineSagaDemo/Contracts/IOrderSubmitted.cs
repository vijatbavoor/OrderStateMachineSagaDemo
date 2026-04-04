namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when a customer places an order.
/// </summary>
public interface IOrderSubmitted
{
    Guid OrderId    { get; }
    Guid CustomerId { get; }
    string ProductName { get; }
    int    Quantity    { get; }
    decimal TotalAmount { get; }
    DateTime SubmittedAt { get; }
}
