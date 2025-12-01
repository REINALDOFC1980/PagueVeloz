using System.Threading.Tasks;

namespace PagueVeloz.Application.Interfaces
{
    public interface IIdempotencyService
    {
        Task<string?> GetSavedResponseAsync(string key);
        Task SaveResponseAsync(string key, string responseJson);
    }
}
