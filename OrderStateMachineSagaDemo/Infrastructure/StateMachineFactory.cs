using MassTransit;
using OrderStateMachineSagaDemo.Services;
using OrderStateMachineSagaDemo.StateMachines;

namespace OrderStateMachineSagaDemo.Infrastructure;

public class StateMachineFactory : IStateMachineFactory
{
    private readonly IPaymentRetryPolicy _retryPolicy;
    private readonly IPaymentRetryHandler _paymentRetryHandler;
    private readonly IOrderInitializService _sagaService;


    public StateMachineFactory(IPaymentRetryPolicy retryPolicy, IPaymentRetryHandler paymentRetryHandler, IOrderInitializService sagaService)

    {
        _retryPolicy = retryPolicy;
        _paymentRetryHandler = paymentRetryHandler;
        _sagaService = sagaService;

    }

    public OrderStateMachine Create()
    {
        return new OrderStateMachine(_retryPolicy, _paymentRetryHandler, _sagaService);

    }
}
