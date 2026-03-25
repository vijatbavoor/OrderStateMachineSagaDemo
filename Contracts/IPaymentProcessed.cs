namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when payment is successfully charged.
/// </summary>
public interface IPaymentProcessed
{
    Guid   OrderId         { get; }
    string TransactionId   { get; }
    DateTime ProcessedAt   { get; }
}
