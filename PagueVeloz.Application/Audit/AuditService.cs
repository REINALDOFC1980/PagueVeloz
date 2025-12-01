using Serilog;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Application.Interfaces;

namespace PagueVeloz.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        public void LogAccountCreation(AccountModel account)
        {
            Log.ForContext("AccountId", account.AccountId)
               .ForContext("Balance", account.Balance)
               .ForContext("CreditLimit", account.CreditLimit)
               .ForContext("CreatedAt", account.CreatedAt)
               .Information("[AUDIT] Conta criada com sucesso:");
        }
    }
}
