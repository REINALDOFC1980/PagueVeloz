using Dapper;
using PagueVeloz.Domain.Entities;
using System.Data;
using System.Threading.Tasks;

namespace PagueVeloz.Infrastructure.Repositories.Idempotency
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly IDbConnection _connection;

        public IdempotencyRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IdempotencyRecord?> GetByKeyAsync(string key)
        {
            var sql = @"SELECT * FROM IdempotencyRecords WHERE [Key] = @key";

            return await _connection.QueryFirstOrDefaultAsync<IdempotencyRecord>(sql, new { key });
        }

        public async Task SaveAsync(IdempotencyRecord record)
        {
            var sql = @"
                INSERT INTO IdempotencyRecords (Id, [Key], Response, CreatedAt)
                VALUES (@Id, @Key, @Response, @CreatedAt)";

            await _connection.ExecuteAsync(sql, record);
        }
    }
}
