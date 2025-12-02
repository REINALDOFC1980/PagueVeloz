namespace PagueVeloz.Api.DTOs
{
    public class TransactionResponseDto
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public string Operation { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal ReservedBalance { get; set; }
        public string ReferenceId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
