using System;

namespace OrderStateMachineSagaDemo.Contracts;

public interface IAddressInvalid
{
    Guid OrderId { get; }
    string Reason { get; }
    DateTime InvalidatedAt { get; }
}

