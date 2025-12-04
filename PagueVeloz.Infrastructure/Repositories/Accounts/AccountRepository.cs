using Dapper;
using PagueVeloz.Domain.Entities;
using Serilog;
using System;
using System.Data;
using System.Threading.Tasks;

namespace PagueVeloz.Infrastructure.Repositories.Account
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbConnection _connection;

        public AccountRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<AccountModel> CreateAccountAsync(AccountModel account, IDbTransaction dbTransaction = null)
        {
            Log.Debug("Iniciando INSERT de conta {@Account}", account);

            account.AccountId = Guid.NewGuid();

            var sql = @"
                INSERT INTO Accounts 
                (AccountId, AccountNumber, Balance, ReservedBalance, CreditLimit, Status, CreatedAt, UpdatedAt)
                VALUES 
                (@AccountId,@AccountNumber, @Balance, @ReservedBalance, @CreditLimit, @Status, @CreatedAt, @UpdatedAt)";

            await _connection.ExecuteAsync(sql, account, dbTransaction);

            Log.Information("Conta {AccountNumber} criada com sucesso.", account.AccountNumber);
            return account;
        }

        public async Task<AccountModel> GetAccountByNumberAsync(string AccountNumber)
        {
            var sql = "SELECT * FROM Accounts WHERE AccountNumber = @AccountNumber";
            // 🔹 Passando a transação para Dapper
            return await _connection.QueryFirstOrDefaultAsync<AccountModel>(sql, new { AccountNumber });
        }

        public async Task<AccountModel> GetAccountByIdAsync(Guid AccountId, IDbTransaction dbTransaction = null)
        {
            var sql = "SELECT * FROM Accounts WHERE AccountId = @AccountId";
            var parameters = new { AccountId = AccountId.ToString("N") };
            // 🔹 Passando a transação para Dapper
            return await _connection.QueryFirstOrDefaultAsync<AccountModel>(sql, new { AccountId }, transaction: dbTransaction);
        }

        public async Task<bool> UpdateAccountAsync(AccountModel account, IDbTransaction dbTransaction = null)
        {
            account.UpdatedAt = DateTime.UtcNow;

            var sql = @"
                UPDATE Accounts
                SET Balance = @Balance,
                    ReservedBalance = @ReservedBalance,
                    CreditLimit = @CreditLimit,
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE AccountId = @AccountId 
                  AND RowVersion = @RowVersion";

            var parameters = new
            {
                account.Balance,
                account.ReservedBalance,
                account.CreditLimit,
                account.Status,
                account.UpdatedAt,
                account.AccountId,
                account.RowVersion
            };

            var affectedRows = await _connection.ExecuteAsync(sql, parameters, dbTransaction);
            return affectedRows > 0;
        }
    }
}
