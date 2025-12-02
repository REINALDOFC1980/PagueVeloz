using PagueVeloz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Infrastructure.Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task SaveAsync(TransactionModel transaction, IDbTransaction dbTransaction = null);
        Task<bool> ExistsByReferenceIdAsync(string referenceId);
    }
}
