using Dapper;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Shared.Middlewares;
using Serilog;
using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAuditService _auditService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IDbConnection _connection;

        public AccountService(
            IAccountRepository accountRepository,
            IAuditService auditService,
            IIdempotencyService idempotencyService,
            IDbConnection connection)
        {
            _accountRepository = accountRepository;
            _auditService = auditService;
            _idempotencyService = idempotencyService;
            _connection = connection;
        }

        public async Task<AccountModel> CreateAccountAsync(AccountModel account, string idempotencyKey)
        {
            Log.Information("Iniciando abertura de conta.");
                // Idempotência
                var savedResponse = await _idempotencyService.GetSavedResponseAsync(idempotencyKey);
                if (!string.IsNullOrEmpty(savedResponse))
                {
                    Log.Information("Idempotência encontrada para a chave {IdempotencyKey}", idempotencyKey);
                    return JsonSerializer.Deserialize<AccountModel>(savedResponse)!;
                }

                // Verifica se a conta já existe dentro da transação
                var existingAccount = await _accountRepository.GetAccountByNumberAsync(account.AccountNumber);
                if (existingAccount != null)
                    throw new BusinessException("Conta já existe.");

                // Configura campos
                account.AccountId = Guid.NewGuid();
                account.Status = AccountStatus.Active;
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;

                // Auditoria
                _auditService.LogAccountCreation(account);

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                using var transaction = _connection.BeginTransaction();
                try
                {
                    // Cria a conta
                    var createdAccount = await _accountRepository.CreateAccountAsync(account, transaction);

                    transaction.Commit();

                    // Salva resposta idempotente
                    var responseJson = JsonSerializer.Serialize(createdAccount);
                    await _idempotencyService.SaveResponseAsync(idempotencyKey, responseJson);

                    return createdAccount;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
        }

        public async Task<AccountModel> UpdateBalanceAsync(AccountModel account)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            using var transaction = _connection.BeginTransaction();
            try
            {
                account.UpdatedAt = DateTime.UtcNow;
                bool updated = await _accountRepository.UpdateAccountAsync(account, transaction);

                if (!updated)
                    throw new BusinessException("Falha ao atualizar a conta. Possível conflito de concorrência.");

                transaction.Commit();
                return account;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<AccountModel> GetAccountByNumberAsync(string accountNumber)
        {
            return await _accountRepository.GetAccountByNumberAsync(accountNumber);
        }
    }
}
