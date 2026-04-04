using MassTransit;

namespace OrderStateMachineSagaDemo.IntegrationTests.Fixtures;

public static class LocalRabbitMqConfig
{
    public const string ConnectionString = "rabbitmq://guest:guest@localhost:5672/";
    
// ConfigureBus not used; SagaTestBase configures directly
}
