using System.ComponentModel;
using FluentValidation.AspNetCore;
using Hair.Api.Filters;
using Hair.Application;
using Hair.Infrastructure;
using Hair.Infrastructure.Configuration;
using Hair.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
//options => options.Filters.Add<ApiExceptionFilterAttribute>()
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();
builder.Services.Configure<PostgresDbConfiguration>(
    builder.Configuration.GetSection("PostgresDbConfiguration"));
/*
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<PostgresDbConfiguration>>().Value);
var config = builder.Configuration.GetSection("PostgresDbConfiguration").Get<PostgresDbConfiguration>();
Console.WriteLine($"DbHost from config: {config.GetConnectionString()}");
builder.Services.AddDbContext<ConnDbContext>((serviceProvider, options) =>
{
    var postgresConfig = serviceProvider.GetRequiredService<IOptions<PostgresDbConfiguration>>().Value;
    options.UseNpgsql(postgresConfig.GetConnectionString());
});
*/

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
