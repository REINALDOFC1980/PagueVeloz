using Moq;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Application.Services;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using System.Data;
using System.Threading.Tasks;
using Xunit;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _repoMock = new();
    private readonly Mock<IDbConnection> _dbConnectionMock = new();
    private readonly Mock<IDbTransaction> _dbTransactionMock = new();
    private readonly Mock<IAuditService> _auditMock = new();
    private readonly Mock<IIdempotencyService> _idempotencyMock = new();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        // Configura a conexão e a transação mock
        _dbTransactionMock = new Mock<IDbTransaction>();
        _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _dbConnectionMock.Setup(c => c.BeginTransaction()).Returns(_dbTransactionMock.Object);

        // Configura idempotency para não quebrar o metodo
        _idempotencyMock.Setup(x => x.GetSavedResponseAsync(It.IsAny<string>()))
                        .ReturnsAsync((string?)null);
        _idempotencyMock.Setup(x => x.SaveResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(Task.CompletedTask);

        // Cria o serviço
        _service = new AccountService(
            _repoMock.Object,
            _auditMock.Object,
            _idempotencyMock.Object,
            _dbConnectionMock.Object
        );
    }

    [Fact]
    public async Task Inserir_DeveChamarRepositorio()
    {
        // Arrange: Cria uma conta de teste
        var account = new AccountModel { AccountNumber = "CC-0001" };


        // Configura o mock para retorna a conta quando CreateAccountAsync for chamado
        _repoMock.Setup(r => r.CreateAccountAsync(account, _dbTransactionMock.Object))
                 .ReturnsAsync(account);

        // Act: Chama o método do serviço que estamos testando
        var result = await _service.CreateAccountAsync(account, "key");

        // Assert: Verifica se o método do repositorio foi chamado exatamente uma vez
        _repoMock.Verify(r => r.CreateAccountAsync(account, _dbTransactionMock.Object), Times.Once);
        Assert.Equal(account, result);
    }

    [Fact]
    public async Task Encontrar_DeveRetornarConta()
    {
        var account = new AccountModel { AccountNumber = "CC-0001" };
        _repoMock.Setup(r => r.GetAccountByNumberAsync("CC-0001")).ReturnsAsync(account);

        var result = await _service.GetAccountByNumberAsync("CC-0001");

        Assert.Equal("CC-0001", result.AccountNumber);
    }

    [Fact]
    public async Task Atualizar_DeveChamarRepositorio()
    {
        var account = new AccountModel
        {
            AccountId = Guid.NewGuid(),
            AccountNumber = "CC-0001",
            Balance = 100
        };

        _repoMock.Setup(r => r.UpdateAccountAsync(account, _dbTransactionMock.Object))
                 .ReturnsAsync(true);

        var result = await _service.UpdateBalanceAsync(account);

        //Verifica se o método do repositório foi chamado exatamente uma vez
        _repoMock.Verify(r => r.UpdateAccountAsync(account, _dbTransactionMock.Object), Times.Once);
        Assert.Equal(account, result);
    }

    [Fact]
    public async Task Buscar_DeveLancarExcecao_QuandoRepositorioFalha()
    {

        var accountNumber = "CC-0001";

        // Configura o mock para lançar uma exceção ao tentar buscar a conta
        _repoMock.Setup(r => r.GetAccountByNumberAsync(accountNumber))
                 .ThrowsAsync(new Exception("Erro ao consultar o banco"));

        // Verifica se o servico lança a exceção corretamente
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetAccountByNumberAsync(accountNumber));

        // Verifica se a mensagem da exceção é a esperada
        Assert.Equal("Erro ao consultar o banco", exception.Message);
    }

}
