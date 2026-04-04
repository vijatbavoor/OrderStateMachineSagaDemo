using OrderStateMachineSagaDemo.StateMachines;

namespace OrderStateMachineSagaDemo.Infrastructure;

public interface IStateMachineFactory
{
    OrderStateMachine Create();
}

