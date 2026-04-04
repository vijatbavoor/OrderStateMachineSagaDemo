using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderStateMachineSagaDemo.Data;
using OrderStateMachineSagaDemo.Models;
using OrderStateMachineSagaDemo.Services;
using OrderStateMachineSagaDemo.StateMachines;


var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options => {
    options.IncludeScopes = true;
    options.TimestampFormat = "HH:mm:ss ";
});



// Payment retry policy — swap implementation here to change retry behaviour
builder.Services.AddSingleton<IPaymentRetryPolicy, PaymentRetryPolicy>();
builder.Services.AddSingleton<IOrderSagaService, OrderSagaService>();

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

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// builder.Services.AddHostedService<SimulationHostedService>();

var host = builder.Build();

// Ensure DB created for saga repo
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

await host.RunAsync();
