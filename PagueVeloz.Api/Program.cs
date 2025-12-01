using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PagueVeloz.TransactionProcessor.Infrastructure.Database;
using System.Data;

var builder = WebApplication.CreateBuilder(args);


// Lê connection string do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// REGISTRA a conexão usada pelo Dapper
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(connectionString));

// REGISTRA o DbContext do EF Core 
builder.Services.AddDbContext<PagueVelozDbContext>(options =>
    options.UseSqlServer(connectionString));



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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
