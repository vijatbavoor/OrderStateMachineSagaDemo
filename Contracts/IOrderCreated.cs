using System;

namespace OrderStateMachineSagaDemo.Contracts;

public interface IOrderCreated
{
    Guid OrderId { get; }
    DateTime CreatedAt { get; }
}

