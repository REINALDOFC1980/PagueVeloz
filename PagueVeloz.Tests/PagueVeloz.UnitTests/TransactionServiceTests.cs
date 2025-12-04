using Moq;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Application.Services;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Infrastructure.Repositories.Transactions;
using System;
using System.Data;
using System.Threading.Tasks;
using WebApiBiblioteca.Service.RabbitMQ;
using Xunit;

public class TransactionServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepoMock = new();
    private readonly Mock<IDbConnection> _dbConnectionMock = new();
    private readonly Mock<IDbTransaction> _dbTransactionMock = new();
    private readonly Mock<IIdempotencyService> _idempotencyMock = new();
    private readonly Mock<IRabbitMQService> _rabbitMQMock = new();
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        // Configura conexão e transação mock
        _dbTransactionMock = new Mock<IDbTransaction>();
        _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _dbConnectionMock.Setup(c => c.BeginTransaction()).Returns(_dbTransactionMock.Object);

        // Idempotency simples
        _idempotencyMock.Setup(x => x.GetSavedResponseAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
        _idempotencyMock.Setup(x => x.SaveResponseAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Cria service
        _service = new TransactionService(
            _accountRepoMock.Object,
            _transactionRepoMock.Object,
            _idempotencyMock.Object,
            _dbConnectionMock.Object,
            _rabbitMQMock.Object // não será verificado
        );
    }

    [Fact]
    public async Task Debito_DeveDiminuirSaldo()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            AccountId = accountId,
            Balance = 200,
            CreditLimit = 50
        };

        var transactionDto = new TransactionModel
        {
            AccountId = accountId,
            Amount = 100,
            Operation = TransactionType.Debit,
            ReferenceId = "ref-123"
        };

        _accountRepoMock.Setup(r => r.GetAccountByIdAsync(accountId, null)).ReturnsAsync(account);
        _accountRepoMock.Setup(r => r.UpdateAccountAsync(account, _dbTransactionMock.Object)).ReturnsAsync(true);

        _transactionRepoMock.Setup(r => r.SaveAsync(It.IsAny<TransactionModel>(), _dbTransactionMock.Object))
                            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessTransactionAsync(transactionDto, "key-123");

        // Assert
        Assert.Equal(100, account.Balance); // 200 - 100
        Assert.Equal(TransactionStatus.Completed, result.Status);
    }
}
