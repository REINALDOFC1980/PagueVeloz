using PagueVeloz.Domain.Entities;
using System.Data;


namespace PagueVeloz.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> CreateAccountAsync(AccountModel account, string idempotencyKey);
        Task<AccountModel> GetAccountByNumberAsync(string AccountNumber);
        Task<AccountModel> UpdateBalanceAsync(AccountModel account);
    }
}
