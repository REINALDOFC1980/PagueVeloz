using PagueVeloz.Domain.Entities;


namespace PagueVeloz.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> CreateAccountAsync(AccountModel account, string idempotencyKey);
        Task<AccountModel> GetAccountByIdAsync(string AccountNumber);
        Task<AccountModel> UpdateBalanceAsync(AccountModel account);
    }
}
