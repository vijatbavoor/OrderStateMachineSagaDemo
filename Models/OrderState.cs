using MassTransit;

namespace OrderStateMachineSagaDemo.Models;

public enum OrderStateEnum
{
    Created,
    StockChecked,
    PaymentPending,
    PaymentCompleted,
    AddressValidated,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public int PaymentAttempts { get; set; } = 0;
    public string? Address { get; set; }
}


