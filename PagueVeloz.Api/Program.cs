using FluentValidation.AspNetCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using PagueVeloz.Api.Middlewares;
using PagueVeloz.Api.Validators;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Application.Services;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Infrastructure.Repositories.Idempotency;
using PagueVeloz.Infrastructure.Services;
using PagueVeloz.TransactionProcessor.Infrastructure.Database;
using Serilog;
using System.Data;

var builder = WebApplication.CreateBuilder(args);


// Lê connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registro dos serviços e repositórios
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepositoty, AccountRepositoty>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();




// Registro a conexão usada pelo Dapper
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(connectionString));

// Registro o DbContext do EF Core 
builder.Services.AddDbContext<PagueVelozDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<AccountCreateValidator>();
    });


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
});


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


// Cria o banco via EF Core (se estiver usando EF)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagueVelozDbContext>();
    db.Database.EnsureCreated();
}

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
