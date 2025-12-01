using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PagueVeloz.Api.Middlewares;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Application.Services;
using PagueVeloz.Infrastructure.Repositories.Account;
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

// Registro do serviço de auditoria
builder.Services.AddScoped<IAuditService, AuditService>();


// REGISTRA a conexão usada pelo Dapper
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(connectionString));

// REGISTRA o DbContext do EF Core 
builder.Services.AddDbContext<PagueVelozDbContext>(options =>
    options.UseSqlServer(connectionString));



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
// Middleware para log de requisições
app.UseMiddleware<RequestLog>();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
