using MassTransit;
using OrderStateMachineSagaDemo.StateMachines;

namespace OrderStateMachineSagaDemo.Infrastructure;

public class StateMachineFactory : IStateMachineFactory
{
    public OrderStateMachine Create()
    {
        return new OrderStateMachine();
    }
}

