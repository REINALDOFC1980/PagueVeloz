using PagueVeloz.Domain.Entities;


namespace PagueVeloz.Infrastructure.Repositories.Account
{
    public interface IAccountRepositoty
    {
        Task<AccountModel> CreateAccountAsync(AccountModel account);
        Task<AccountModel> GetAccountByIdAsync(string AccountNumber);
        Task<bool> UpdateAccountAsync(AccountModel account); 
    }
}
