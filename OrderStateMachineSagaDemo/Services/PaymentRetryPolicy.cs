using OrderStateMachineSagaDemo.Models;

namespace OrderStateMachineSagaDemo.Services;

/// <summary>
/// Default policy: retry up to 2 times (3 total attempts), then cancel.
/// </summary>
public class PaymentRetryPolicy : IPaymentRetryPolicy
{
    /// <inheritdoc/>
    public int MaxAttempts => 3;

    /// <inheritdoc/>
    public bool ShouldRetry(OrderState saga) => saga.PaymentAttempts < MaxAttempts;
}
