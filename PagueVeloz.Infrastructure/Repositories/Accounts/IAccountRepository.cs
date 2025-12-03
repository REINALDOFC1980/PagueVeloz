using PagueVeloz.Domain.Entities;
using System.Data;


namespace PagueVeloz.Infrastructure.Repositories.Account
{
    public interface IAccountRepository
    {
        Task<AccountModel> CreateAccountAsync(AccountModel account, IDbTransaction dbTransaction = null);
        Task<bool> UpdateAccountAsync(AccountModel account, IDbTransaction dbTransaction = null);
        Task<AccountModel> GetAccountByNumberAsync(string AccountNumber);
        Task<AccountModel> GetAccountByIdAsync(Guid AccountId, IDbTransaction dbTransaction = null);
   
    }
}
