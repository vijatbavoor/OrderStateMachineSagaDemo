using MassTransit;
using OrderStateMachineSagaDemo.Models;

namespace OrderStateMachineSagaDemo.Services;

/// <summary>
/// Encapsulates the payment retry / cancel decision for the saga.
/// Inject this into <see cref="StateMachines.OrderStateMachine"/> so
/// the logic can be swapped or tested in isolation.
/// </summary>
public interface IPaymentRetryPolicy
{
    /// <summary>Maximum number of payment attempts before the order is cancelled.</summary>
    int MaxAttempts { get; }

    /// <summary>Returns true if another retry should be attempted.</summary>
    bool ShouldRetry(OrderState saga);
}
