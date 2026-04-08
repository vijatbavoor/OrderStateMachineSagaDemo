using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Models;
using MassTransit;
using System.Threading.Tasks;

namespace OrderStateMachineSagaDemo.Services;

/// <summary>
/// Service to handle saga-specific logic and actions.
/// </summary>
public interface IOrderInitializService
{
    void InitializeOrder(OrderState saga, IOrderCreated message);
    void LogStockCheck(OrderState saga, IStockChecked message);
    Task PublishNextStockCheckedAsync(BehaviorContext<OrderState, IOrderCreated> ctx);
}
