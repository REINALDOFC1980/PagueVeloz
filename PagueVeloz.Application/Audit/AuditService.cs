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

        public void LogTransaction(TransactionModel transaction)
        {
            Log.ForContext("TransactionId", transaction.TransactionId)
               .ForContext("AccountId", transaction.AccountId)
               .ForContext("DestinationAccountId", transaction.DestinationAccountId)
               .ForContext("Operation", transaction.Operation)
               .ForContext("Amount", transaction.Amount)
               .ForContext("Currency", transaction.Currency)
               .Information("[AUDIT] Transação processada com sucesso:");
        }
    }
}
