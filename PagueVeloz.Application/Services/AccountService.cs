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
        private readonly IIdempotencyService _idempotencyService;

        public AccountService(
            IAccountRepositoty accountRepository,
            IAuditService serviceAudi,
            IIdempotencyService idempotencyService)
        {
            _accountRepository = accountRepository;
            _serviceAudi = serviceAudi;
            _idempotencyService = idempotencyService;
        }


        public async Task<AccountModel> CreateAccountAsync(AccountModel account, string idempotencyKey)
        {
            Log.Information("Iniciando abertura de conta.");

       
            var savedResponse = await _idempotencyService.GetSavedResponseAsync(idempotencyKey);
            if (!string.IsNullOrEmpty(savedResponse))
            {
                Log.Information("Idempotência encontrada para a chave {IdempotencyKey}", idempotencyKey);
                return System.Text.Json.JsonSerializer.Deserialize<AccountModel>(savedResponse)!;
            }

            try
            {
                account.Status = AccountStatus.Active;
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;

                // 2️⃣ Audit log
                _serviceAudi.LogAccountCreation(account);

                // 3️⃣ Criação de conta no repositório
                var createdAccount = await _accountRepository.CreateAccountAsync(account);

                // 4️⃣ Salvar resposta na tabela de idempotência
                var responseJson = System.Text.Json.JsonSerializer.Serialize(createdAccount);
                await _idempotencyService.SaveResponseAsync(idempotencyKey, responseJson);

                return createdAccount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao criar conta.");
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
