using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace OrderStateMachineSagaDemo.IntegrationTests.Fixtures;

public class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqBuilder _rabbitMqBuilder = new RabbitMqBuilder().WithName(Guid.NewGuid().ToString("N")[..8]).WithImage("rabbitmq:3-management").WithPortBinding(5672, 5672).WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672));
private RabbitMqContainer _rabbitMqContainer = null!;
public string HostName { get; private set; } = null!;
public int RabbitMqPort { get; private set; }
public string RabbitMqConnectionString { get; private set; } = null!;

public async Task InitializeAsync()
    {
        _rabbitMqContainer = _rabbitMqBuilder.Build();
        await _rabbitMqContainer.StartAsync();
        HostName = _rabbitMqContainer.Hostname;
        RabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        RabbitMqConnectionString = $"rabbitmq://{HostName}:{RabbitMqPort}/?username=guest&password=guest";
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }
}
