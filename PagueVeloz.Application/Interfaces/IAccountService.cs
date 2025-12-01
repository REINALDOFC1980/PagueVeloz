using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PagueVeloz.Domain.Entities;


namespace PagueVeloz.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountModel> CreateAccountAsync(AccountModel account);
        Task<AccountModel> GetAccountByIdAsync(Guid accountId);
        Task UpdateAccountAsync(AccountModel account);
    }
}
