using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Hair.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hair.Infrastructure.Context;

public class ConnDbContext : IdentityDbContext<ApplicationUser>, IHairDbContext
{
    public ConnDbContext(DbContextOptions<ConnDbContext> options) : base(options) { }
    public ConnDbContext() {}

    public DbSet<Company> Companies { get; set; }
    public DbSet<Barber> Barbers { get; set; }
    
    public DbSet<Haircut> Haircuts { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AnonymousUser> AnonymousUser { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    //private readonly string _connectionString;
    
    /*
    public ConnDbContext(DbContextOptions<ConnDbContext> options, IOptions<PostgresDbConfiguration> postgresConfig): base(options)
    {
        _connectionString = postgresConfig.Value.ConnectionString;
    }


    public ConnDbContext(DbContextOptions<ConnDbContext> options, IOptions<PostgresDbConfiguration> postgresConfig)
        : base(options)
    {
        var config = postgresConfig.Value;

        if (string.IsNullOrEmpty(config.DbHost))
        {
            throw new ArgumentException("Database Host is not set in the configuration.");
        }

        _connectionString = config.GetConnectionString();
        Console.WriteLine($"Using Connection String: {_connectionString}");
    }*/
    
    
    
    
    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
    }
    */
    //  => optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=ivan");
}