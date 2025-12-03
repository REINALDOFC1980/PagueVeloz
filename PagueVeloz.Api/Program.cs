using FluentValidation.AspNetCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using PagueVeloz.Api.Validators;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Application.Services;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Infrastructure.Repositories.Idempotency;
using PagueVeloz.Infrastructure.Repositories.Transactions;
using PagueVeloz.Infrastructure.Services;
using PagueVeloz.Shared.Middlewares;
using PagueVeloz.TransactionProcessor.Infrastructure.Database;
using Serilog;
using System;
using System.Data;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Lê connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registro dos serviços e repositórios
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();




// Registro a conexão usada pelo Dapper
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(connectionString));

// Registro o DbContext do EF Core 
//builder.Services.AddDbContext<PagueVelozDbContext>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PagueVelozDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("PagueVeloz.Infrastructure")));



builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<AccountCreateValidator>();
    });


// Registro dos health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: connectionString,
        name: "sqlserver",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "ready" }
    );

// Configuração Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

// OpenTelemetry Metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("PagueVeloz.Api", serviceVersion: "1.0.0"));

        metrics.AddAspNetCoreInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddPrometheusExporter();
    });


// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PagueVeloz API",
        Version = "v1",
        Description = "API para processamento de transações financeiras"
    });
  

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Adiciona o XML ao Swagger
    c.IncludeXmlComments(xmlPath);
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

var app = builder.Build();

// Aplica migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagueVelozDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error migrating database: {ex.Message}");
        throw;
    }
}


//Cria o banco via EF Core (se estiver usando EF)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagueVelozDbContext>();
    db.Database.EnsureCreated();
}

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<PagueVelozDbContext>();
//    db.Database.Migrate();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TransactionProcessor API V1");
        c.RoutePrefix = "swagger";
    });
}


// Middleware para tratamento global de exceções
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Middleware para log de requisições
app.UseMiddleware<RequestLog>();

app.MapPrometheusScrapingEndpoint();

// Endpoints de health check
app.MapHealthChecks("/health");        
app.MapHealthChecks("/health/ready");

app.UseMiddleware<RequestTimingMiddleware>();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
