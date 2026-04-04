// Removed unused Testcontainers fixture - using local Postgres config
// using DotNet.Testcontainers.Builders;
// using DotNet.Testcontainers.Containers;
// using Xunit;

// namespace OrderStateMachineSagaDemo.IntegrationTests.Fixtures;

// public class PostgresDbFixture : IAsyncLifetime
// {
//     private readonly PostgreSqlBuilder _postgresBuilder = new PostgreSqlBuilder().WithName(Guid.NewGuid().ToString("N")[..8]).WithImage("postgres:16-alpine").WithPassword("Password123!").WithDatabase("sagas").WithPortBinding(5432, 5432).WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432));
//     private PostgreSqlContainer _postgresContainer = null!;

//     public string ConnectionString { get; private set; } = null!;

//     public async Task InitializeAsync()
//     {
//         _postgresContainer = _postgresBuilder.Build();
//         await _postgresContainer.StartAsync();
//         ConnectionString = _postgresContainer.GetConnectionString();
//     }

//     public async Task DisposeAsync()
//     {
//         await _postgresContainer.DisposeAsync();
//     }
// }
