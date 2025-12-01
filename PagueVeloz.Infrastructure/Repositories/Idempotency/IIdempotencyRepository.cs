using PagueVeloz.Domain.Entities;
using System.Threading.Tasks;

namespace PagueVeloz.Infrastructure.Repositories.Idempotency
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetByKeyAsync(string key);
        Task SaveAsync(IdempotencyRecord record);
    }
}
