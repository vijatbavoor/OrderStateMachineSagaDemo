using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrderStateMachineSagaDemo.Data;
using OrderStateMachineSagaDemo.IntegrationTests.Data;
using OrderStateMachineSagaDemo.IntegrationTests.Fixtures;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.StateMachines;
using Xunit;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public abstract class SagaTestBase : IClassFixture<RabbitMqFixture>, IClassFixture<PostgresDbFixture>, IAsyncLifetime
{
    protected readonly RabbitMqFixture _rabbitMqFixture;
    protected readonly PostgresDbFixture _dbFixture;
    protected IBusControl _bus = null!;
    protected IServiceProvider _services = null!;

    protected SagaTestBase(RabbitMqFixture rabbitMqFixture, PostgresDbFixture dbFixture)
    {
        _rabbitMqFixture = rabbitMqFixture;
        _dbFixture = dbFixture;
    }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TestAppDbContext>(o =>
            o.UseNpgsql(_dbFixture.ConnectionString)
             .EnableSensitiveDataLogging());

        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<TestAppDbContext>();
                    r.UsePostgres();
                });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_rabbitMqFixture.RabbitMqConnectionString);
                cfg.ConfigureEndpoints(context);
            });
        });

        var provider = services.BuildServiceProvider();

        // Start bus
        _bus = provider.GetRequiredService<IBusControl>();
        await _bus.StartAsync();

        // Ensure DB created
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestAppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        _services = provider;
        _bus = provider.GetRequiredService<IBusControl>();
        await _bus.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_bus != null) await _bus.StopAsync();
        (_services as IDisposable)?.Dispose();
    }

    protected async Task PublishEvent<T>(T @event) where T : class
    {
        await _bus.Publish(@event);
    }

    protected async Task<OrderState?> GetSagaState(Guid correlationId)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestAppDbContext>();
        return await dbContext.OrderSagas.FindAsync(correlationId);
    }

    protected async Task WaitForState(Guid orderId, string expectedState, int timeoutMs = 10000)
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
