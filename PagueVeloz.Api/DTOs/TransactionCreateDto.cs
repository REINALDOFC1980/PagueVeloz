using PagueVeloz.Domain.Entities;

namespace PagueVeloz.API.DTOs
{
    public class TransactionCreateDto
    {
        public TransactionType Operation { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BRL";
        public string ReferenceId { get; set; }
    }
}
