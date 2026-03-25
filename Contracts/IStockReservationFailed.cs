namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when stock is insufficient or reservation fails.
/// </summary>
public interface IStockReservationFailed
{
    Guid   OrderId { get; }
    string Reason  { get; }
}
