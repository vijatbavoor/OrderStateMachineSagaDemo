using Microsoft.EntityFrameworkCore;
using OrderStateMachineSagaDemo.Models;

namespace OrderStateMachineSagaDemo.Data;

public class AppDbContext : DbContext
{
    public DbSet<OrderState> OrderSagas { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderState>(b => {
            b.HasKey(x => x.CorrelationId);
            b.Property(x => x.CurrentState).HasConversion<string>();
            b.Property(x => x.OrderId);
            b.Property(x => x.PaymentAttempts);
            b.Property(x => x.Address);
            b.ToTable("OrderSagas");
        });
    }
}

