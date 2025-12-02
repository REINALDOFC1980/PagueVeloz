using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Interfaces
{
    public interface IAuditService
    {
        void LogAccountCreation(AccountModel account);
        void LogTransaction(TransactionModel transaction);


    }
}
