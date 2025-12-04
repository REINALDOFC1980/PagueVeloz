using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PagueVeloz.Application.Services;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Infrastructure.Repositories.Transactions;
using WebApiBiblioteca.Service.RabbitMQ;
using Xunit;
using Moq;

public class TransactionServiceIntegrationTests
{
  
    public class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)  // Handler Dapper customizado para converter o tipo Guid (C#) para TEXT (SQLite) e vice-versa.
        {
            parameter.Value = value.ToString();
        }
        public override Guid Parse(object value)
        {  if (value is string s && Guid.TryParse(s, out Guid result))      
                return result;
            else
                throw new DataException($"Não é possível converter o valor do tipo {value.GetType()} para Guid.");
        }
    }

    private IDbConnection CreateConnection()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        conn.Execute(@"

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
        

            CREATE TABLE Transactions (
                TransactionId TEXT PRIMARY KEY,
                Operation TEXT NOT NULL,          
                AccountId TEXT NOT NULL,
                DestinationAccountId TEXT NULL,
                Amount REAL NOT NULL,
                Currency TEXT NOT NULL,             
                ReferenceId TEXT NOT NULL,
                Status TEXT NOT NULL,             
                CreatedAt TEXT NOT NULL,
                Balance REAL NOT NULL,              
                AvailableBalance REAL NOT NULL,    
                Message TEXT NULL
            );
        ");

        return conn;
    }


    [Fact]
    public async Task Debito_DeveDiminuirSaldo_E_PersistirNoBanco()
    {
        // ARRANGE
        var connection = CreateConnection();
        var transactionRepo = new TransactionRepository(connection);
        var accountRepo = new AccountRepository(connection);

        var rabbitMock = new Mock<IRabbitMQService>();
        var idempotencyMock = new Mock<IIdempotencyService>();
        idempotencyMock.Setup(i => i.GetSavedResponseAsync(It.IsAny<string>()))
                        .ReturnsAsync((string?)null);

        //Registrar o Custom Handler antes de qualquer operação Dapper
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());

        var service = new TransactionService(
            accountRepo,
            transactionRepo,
            idempotencyMock.Object,
            connection,
            rabbitMock.Object
        );

        // 1. Simule um valor inicial de RowVersion (simulando o banco de dados)
        var initialRowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };

        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            AccountId = accountId,
            Balance = 200,
            CreditLimit = 50,
            ReservedBalance = 0,
            AccountNumber = "CC-0001",
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = initialRowVersion // ✅ Define o valor inicial para o INSERT
        };

        // 2. Garanta que o INSERT utilize o valor inicial do RowVersion
        await connection.ExecuteAsync(
            @"INSERT INTO Accounts 
            (AccountId, AccountNumber, Balance, ReservedBalance, CreditLimit, Status, CreatedAt, UpdatedAt, RowVersion) 
          VALUES 
            (@AccountId, @AccountNumber, @Balance, @ReservedBalance, @CreditLimit, @Status, @CreatedAt, @UpdatedAt, @RowVersion)",
            account
        );

        // ACT
        var dto = new TransactionModel
        {
            AccountId = accountId,
            Amount = 100,
            Currency = "BRL",
            Operation = TransactionType.Debit,
            ReferenceId = "integ-001"
        };

        var result = await service.ProcessTransactionAsync(dto, dto.ReferenceId);

        // ASSERT
        Assert.Equal(TransactionStatus.Completed, result.Status);

        var updatedAccount = await connection.QueryFirstAsync<AccountModel>(
            "SELECT AccountId, Balance, CreditLimit, ReservedBalance FROM Accounts WHERE AccountId = @id",
            new { id = accountId }
        );

        Assert.Equal(100, updatedAccount.Balance); // Esperado 100

        // ... (o restante dos asserts)
        var savedTransaction = await connection.QueryFirstOrDefaultAsync<TransactionModel>(
            "SELECT * FROM Transactions WHERE ReferenceId = @r",
            new { r = "integ-001" }
        );

        Assert.NotNull(savedTransaction);
        Assert.Equal(100, savedTransaction.Amount);
        Assert.Equal(TransactionStatus.Completed, savedTransaction.Status);
    }
}