using PagueVeloz.Domain.Entities;


namespace PagueVeloz.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> CreateAccountAsync(AccountModel account, string idempotencyKey);
        Task<AccountModel> GetAccountByIdAsync(Guid accountId);
        Task<AccountModel> UpdateBalanceAsync(AccountModel account);
    }
}
