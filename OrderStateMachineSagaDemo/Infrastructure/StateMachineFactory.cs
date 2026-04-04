using MassTransit;
using OrderStateMachineSagaDemo.Services;
using OrderStateMachineSagaDemo.StateMachines;

namespace OrderStateMachineSagaDemo.Infrastructure;

public class StateMachineFactory : IStateMachineFactory
{
    private readonly IPaymentRetryPolicy _retryPolicy;
    private readonly IOrderSagaService _sagaService;

    public StateMachineFactory(IPaymentRetryPolicy retryPolicy, IOrderSagaService sagaService)
    {
        _retryPolicy = retryPolicy;
        _sagaService = sagaService;
    }

    public OrderStateMachine Create()
    {
        return new OrderStateMachine(_retryPolicy, _sagaService);
    }
}
