using System;

namespace OrderStateMachineSagaDemo.Contracts;

public interface IStockChecked
{
    Guid OrderId { get; }
    bool StockAvailable { get; }
    DateTime CheckedAt { get; }
}

