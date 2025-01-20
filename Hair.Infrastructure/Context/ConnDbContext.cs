using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Context;

public class ConnDbContext : DbContext,IHairDbContext
{
    public ConnDbContext(DbContextOptions<ConnDbContext> options): base(options)
    {
    }

    public ConnDbContext()
    {
        
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        => optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=ivan");


    public DbSet<Company> Companies { get; set; }
    public DbSet<Barber> Barbers { get; set; }

    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}