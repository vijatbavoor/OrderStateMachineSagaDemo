using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderStateMachineSagaDemo.Services;

public class SimulationHostedService : BackgroundService
{
    private readonly IBus _bus;
    private readonly ILogger<SimulationHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SimulationHostedService(IBus bus, ILogger<SimulationHostedService> logger, IServiceProvider serviceProvider)
    {
        _bus = bus;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Waiting 2s for saga startup...");
        await Task.Delay(2000, stoppingToken);

        var orderId = Guid.NewGuid();
        _logger.LogInformation("Simulating order workflow for OrderId: {OrderId}", orderId);

        // Workflow
        await _bus.Publish<IOrderCreated>(new { OrderId = orderId, CreatedAt = DateTime.UtcNow });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IStockChecked>(new { OrderId = orderId, StockAvailable = true, CheckedAt = DateTime.UtcNow });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IPaymentFailed>(new { OrderId = orderId, Reason = "Card declined" });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IPaymentFailed>(new { OrderId = orderId, Reason = "Insufficient funds" });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IPaymentProcessed>(new { OrderId = orderId, TransactionId = "TX123", ProcessedAt = DateTime.UtcNow });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IAddressValidated>(new { OrderId = orderId, Address = "123 Main St, City", ValidatedAt = DateTime.UtcNow });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IOrderShipped>(new { OrderId = orderId, TrackingNumber = "TRACK001", ShippedAt = DateTime.UtcNow });
        await Task.Delay(500, stoppingToken);

        await _bus.Publish<IOrderDelivered>(new { OrderId = orderId, DeliveredAt = DateTime.UtcNow });
        await Task.Delay(1000, stoppingToken);

        // Final state
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saga = await dbContext.OrderSagas.FirstOrDefaultAsync(s => s.CorrelationId == orderId);
        if (saga != null)
        {
            _logger.LogInformation("Final state: {CurrentState}, Attempts: {PaymentAttempts}, Address: {Address}", saga.CurrentState, saga.PaymentAttempts, saga.Address ?? "null");
        }

        _logger.LogInformation("Simulation complete. Saga persists in sagas.db. Press Ctrl+C to exit.");
    }
}

