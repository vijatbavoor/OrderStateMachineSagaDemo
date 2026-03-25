using System;

namespace OrderStateMachineSagaDemo.Contracts;

public interface IAddressValidated
{
    Guid OrderId { get; }
    string Address { get; }
    DateTime ValidatedAt { get; }
}

