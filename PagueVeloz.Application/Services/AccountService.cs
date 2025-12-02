using Microsoft.AspNetCore.Http.HttpResults;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Shared.Middlewares;
using Serilog;
using System.Data;
using System.Text.Json;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PagueVeloz.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAuditService _serviceAudi;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IDbConnection _connection;

        public AccountService(
            IAccountRepository accountRepository,
            IAuditService serviceAudi,
            IIdempotencyService idempotencyService,
            IDbConnection connection)
        {
            _accountRepository = accountRepository;
            _serviceAudi = serviceAudi;
            _idempotencyService = idempotencyService;
            _connection = connection;
        }


        public async Task<AccountModel> CreateAccountAsync(AccountModel account, string idempotencyKey)
        {
              Log.Information("Iniciando abertura de conta.");
                 
            var savedResponse = await _idempotencyService.GetSavedResponseAsync(idempotencyKey);
            if (!string.IsNullOrEmpty(savedResponse))
            {
                Log.Information("Idempotência encontrada para a chave {IdempotencyKey}", idempotencyKey);
                var contaexistente = JsonSerializer.Deserialize<AccountModel>(savedResponse)!;
                return contaexistente;
            }

            var existingAccount = await GetAccountByNumberAsync(account.AccountNumber);
            if (existingAccount != null)
            {
                throw new BusinessException("Conta já existe.");
            }

            account.Status = AccountStatus.Active;
            account.CreatedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;


            _serviceAudi.LogAccountCreation(account);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            using var transaction = _connection.BeginTransaction();
            try
            {
                var createdAccount = await _accountRepository.CreateAccountAsync(account, transaction);
                transaction.Commit();
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

       
        public async Task<AccountModel> GetAccountByNumberAsync(string AccountNumber)
        {
            var account = await _accountRepository.GetAccountByNumberAsync(AccountNumber);
            return account;
        }

        public async Task<AccountModel> UpdateBalanceAsync(AccountModel account)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            using var transaction = _connection.BeginTransaction();

            account.UpdatedAt = DateTime.UtcNow;
            try
            {
                bool updated = await _accountRepository.UpdateAccountAsync(account, transaction);

                if (!updated)
                    throw new BusinessException("Falha ao atualizar a conta. Possível conflito de concorrência.");
                else
                {
                    transaction.Commit();
                    return account;
                }
                    
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
         

         
        }

    }
}
