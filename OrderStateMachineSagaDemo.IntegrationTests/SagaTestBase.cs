using MassTransit;
// using MassTransit.Testing; // Removed for v9 compatibility
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrderStateMachineSagaDemo.Data;
using OrderStateMachineSagaDemo.Infrastructure;
using OrderStateMachineSagaDemo.Services;

using OrderStateMachineSagaDemo.Models;

using OrderStateMachineSagaDemo.StateMachines;
using Xunit;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public abstract class SagaTestBase : IAsyncLifetime
{
    protected IBusControl _bus = null!;
    protected IServiceProvider _services = null!;

    protected SagaTestBase()
    {
    }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlite("Data Source=sagas.db")
             .EnableSensitiveDataLogging());

        services.AddSingleton<IPaymentRetryHandler, PaymentRetryHandler>();
        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>(cfg => new OrderStateMachine(new PaymentRetryHandler()))
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<AppDbContext>();
                    r.UseSqlite();
                });


            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });



        _services = services.BuildServiceProvider();

        // Start bus
        _bus = _services.GetRequiredService<IBusControl>();
        await _bus.StartAsync();

        // Ensure DB created
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_bus != null) await _bus.StopAsync();
        if (_services is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else 
            (_services as IDisposable)?.Dispose();

    }


    protected async Task PublishEvent<T>(T @event) where T : class
    {
        await _bus.Publish(@event);
    }

    protected async Task<OrderState?> GetSagaState(Guid correlationId)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.OrderSagas.FindAsync(correlationId);
    }

    protected async Task WaitForState(Guid orderId, string expectedState, int timeoutMs = 30000)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            var saga = await GetSagaState(orderId);
            if (saga != null && saga.CurrentState == expectedState)
                return;
            await Task.Delay(500);
        }
        throw new TimeoutException($"Saga did not reach state '{expectedState}' in {timeoutMs}ms");
    }
}
