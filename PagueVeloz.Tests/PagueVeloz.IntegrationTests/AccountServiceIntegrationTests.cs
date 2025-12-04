using Xunit;
using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using PagueVeloz.Application.Services;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using Moq;
using Dapper;
using SQLitePCL;

public class AccountServiceIntegrationTests
{
    private IDbConnection CreateConnection()
    {
        Batteries_V2.Init();

        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        connection.Execute(@"
            CREATE TABLE Accounts (
                AccountId TEXT PRIMARY KEY,
                AccountNumber TEXT NOT NULL UNIQUE,
                Balance REAL NOT NULL,
                ReservedBalance REAL NOT NULL,
                CreditLimit REAL NOT NULL,
                Status TEXT NOT NULL,
                RowVersion BLOB NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
        ");

        return connection;
    }

    [Fact]
      public async Task CriarConta_DevePersistirNoBanco()
    {
        using var connection = CreateConnection();

        var auditMock = new Mock<IAuditService>();
        var idempotencyMock = new Mock<IIdempotencyService>();

        idempotencyMock.Setup(x => x.GetSavedResponseAsync(It.IsAny<string>()))
                       .ReturnsAsync((string?)null);

        idempotencyMock.Setup(x => x.SaveResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(Task.CompletedTask);

        var accountRepo = new AccountRepository(connection);
        var service = new AccountService(accountRepo, auditMock.Object, idempotencyMock.Object, connection);

        var account = new AccountModel
        {
            AccountNumber = "CC-0001",
            Balance = 500,
            CreditLimit = 100,
            ReservedBalance = 0
        };

        var created = await service.CreateAccountAsync(account, "test-key");

        var inserted = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM Accounts WHERE AccountNumber = @AccountNumber",
            new { AccountNumber = "CC-0001" }
        );

        Assert.NotNull(inserted);
        Assert.Equal("CC-0001", inserted.AccountNumber);
        Assert.Equal(500, (double)inserted.Balance);
        Assert.Equal(100, (double)inserted.CreditLimit);

        // Ajuste para enum armazenado como texto
        Assert.Equal(AccountStatus.Active, Enum.Parse<AccountStatus>((string)inserted.Status));

        var insertedId = Guid.Parse((string)inserted.AccountId);
        Assert.NotEqual(Guid.Empty, insertedId);
    }

}
