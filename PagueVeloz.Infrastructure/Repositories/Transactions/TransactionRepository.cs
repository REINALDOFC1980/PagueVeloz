using Dapper;
using PagueVeloz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Infrastructure.Repositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbConnection _connection;

        public TransactionRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task SaveAsync(TransactionModel transaction, IDbTransaction dbTransaction = null)
        {
            var sql = @"
                    INSERT INTO Transactions
                    (TransactionId, AccountId, DestinationAccountId, Operation, Amount, Currency, ReferenceId,
                     Status, CreatedAt, Balance, AvailableBalance, Message)
                    VALUES
                    (@TransactionId, @AccountId, @DestinationAccountId, @Operation, @Amount, @Currency, @ReferenceId,
                     @Status, @CreatedAt, @Balance, @AvailableBalance, @Message)";

            await _connection.ExecuteAsync(sql, transaction, dbTransaction);
        }


        public async Task<bool> ExistsByReferenceIdAsync(string referenceId)
        {
            var sql = @"SELECT COUNT(1) FROM Transactions WHERE ReferenceId = @ReferenceId";
            int count = await _connection.ExecuteScalarAsync<int>(sql, new { ReferenceId = referenceId });
            return count > 0;
        }

      
    }
}
