using MassTransit;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Services;
using OrderStateMachineSagaDemo.StateMachines;

namespace OrderStateMachineSagaDemo.Infrastructure;

public class StateMachineFactory : IStateMachineFactory

{
    private readonly IPaymentRetryHandler _paymentRetryHandler;

    public StateMachineFactory(IPaymentRetryHandler paymentRetryHandler)
    {
        _paymentRetryHandler = paymentRetryHandler;
    }

    public OrderStateMachine Create()
    {
        var machine = new OrderStateMachine(_paymentRetryHandler);

        return machine;
    }
}

