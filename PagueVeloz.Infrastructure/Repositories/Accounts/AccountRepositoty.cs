
using Dapper;
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Infrastructure.Repositories.Account
{
    public class AccountRepositoty : IAccountRepositoty
    {

        private readonly IDbConnection _connection;

        public AccountRepositoty(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<AccountModel> CreateAccountAsync(AccountModel account)
        {
            // GARANTE que a conexão está aberta
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            using var transaction = _connection.BeginTransaction();
            try
            {
                Log.Debug("Iniciando INSERT de conta {@Account}", account);

                account.AccountId = Guid.NewGuid();

                var sql = @"
                            INSERT INTO Accounts 
                            (AccountId, Balance, ReservedBalance, CreditLimit, Status, CreatedAt, UpdatedAt)
                            VALUES 
                            (@AccountId, @Balance, @ReservedBalance, @CreditLimit, @Status, @CreatedAt, @UpdatedAt)";

                await _connection.ExecuteAsync(sql, account, transaction);

                transaction.Commit();

                Log.Information("Conta {AccountId} criada com sucesso.", account.AccountId);
                return account;
            }


            catch (Exception ex) // Dapper lança Exception ou SqlException
            {
                transaction.Rollback();

                Log.Error(ex, "Erro ao criar conta {@Account}", account);
                throw;
            }
        }


        public async Task<AccountModel> GetAccountByIdAsync(Guid accountId)
        {
            var sql = "SELECT * FROM Accounts WHERE AccountId = @accountId";
            return await _connection.QueryFirstOrDefaultAsync<AccountModel>(sql, new { accountId });
        }

        public async Task<bool> UpdateAccountAsync(AccountModel account)
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

            var affectedRows = await _connection.ExecuteAsync(sql, parameters);
            return affectedRows > 0;
        }



    }
}
