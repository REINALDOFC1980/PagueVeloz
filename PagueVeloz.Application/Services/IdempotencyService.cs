using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Idempotency;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyRepository _repository;

        public IdempotencyService(IIdempotencyRepository repository)
        {
            _repository = repository;
        }

        public async Task<string?> GetSavedResponseAsync(string key)
        {
            var record = await _repository.GetByKeyAsync(key);
            return record?.Response;
        }

        public async Task SaveResponseAsync(string key, string responseJson)
        {
            var record = new IdempotencyRecord
            {
                Key = key,
                Response = responseJson
            };

            await _repository.SaveAsync(record);
        }
    }
}
