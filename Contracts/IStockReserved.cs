namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when the warehouse confirms stock has been reserved.
/// </summary>
public interface IStockReserved
{
    Guid OrderId { get; }
    DateTime ReservedAt { get; }
}
