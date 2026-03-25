namespace OrderStateMachineSagaDemo.Contracts;

/// <summary>
/// Published when the payment gateway declines or errors.
/// </summary>
public interface IPaymentFailed
{
    Guid   OrderId { get; }
    string Reason  { get; }
}
