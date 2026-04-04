using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Models;
using System;

namespace OrderStateMachineSagaDemo.Services;

/// <summary>
/// Implementation of the <see cref="OrderInitializService"/>.
/// </summary>
public class OrderInitializService : IOrderInitializService
{
    public void InitializeOrder(OrderState saga, IOrderCreated message)
    {
        saga.CorrelationId = message.OrderId;
        saga.OrderId = message.OrderId;
        Console.WriteLine($"Saga: Order Created for {saga.CorrelationId}");
    }

    public void LogStockCheck(OrderState saga, IStockChecked message)
    {
        string status = message.StockAvailable ? "OK" : "fail";
        string nextState = message.StockAvailable ? "StockChecked" : "Cancelled";
        Console.WriteLine($"Saga: Stock {status} -> {nextState} for {saga.CorrelationId}");
    }
}
