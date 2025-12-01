using Microsoft.AspNetCore.Http.HttpResults;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using Serilog;

namespace PagueVeloz.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepositoty _accountRepository;

        private readonly IAuditService _serviceAudi;

        public AccountService(IAccountRepositoty accountRepository, IAuditService serviceAudi)
        {
            _accountRepository = accountRepository;
            _serviceAudi = serviceAudi;
        }

        public async Task<AccountModel> CreateAccountAsync(AccountModel account)
        {
            Log.Information("Iniciando abertura de conta.");
            try
            {
                account.Status = AccountStatus.Active;
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;

                _serviceAudi.LogAccountCreation(account);

                return await _accountRepository.CreateAccountAsync(account);

            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public async Task<AccountModel> GetAccountByIdAsync(Guid accountId)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Conta com Id {accountId} não encontrada.");
            }
            return account;
        }

        public async Task UpdateAccountAsync(AccountModel account)
        {
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.UpdateAccountAsync(account);
        }
    }
}
