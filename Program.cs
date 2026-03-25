using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderStateMachineSagaDemo.Data;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.StateMachines;
using OrderStateMachineSagaDemo.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(l => l.AddConsole(options => {
    options.IncludeScopes = true;
    options.TimestampFormat = "HH:mm:ss ";
}));

builder.Services.AddDbContext<AppDbContext>(o => 
    o.UseSqlite("Data Source=sagas.db")
     .EnableSensitiveDataLogging());

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<AppDbContext>();
            r.UseSqlite();
        });

    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
});

builder.Services.AddHostedService<SimulationHostedService>();

var host = builder.Build();

// Ensure DB created for saga repo
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

await host.RunAsync();
