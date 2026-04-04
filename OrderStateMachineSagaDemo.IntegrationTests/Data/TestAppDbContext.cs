using Microsoft.EntityFrameworkCore;
using OrderStateMachineSagaDemo.Data;
using OrderStateMachineSagaDemo.Models;

namespace OrderStateMachineSagaDemo.IntegrationTests.Data;

public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

