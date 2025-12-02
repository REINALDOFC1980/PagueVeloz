namespace PagueVeloz.Api.DTOs
{
    public class TransactionResponseDto
    {
        public string TransactionId { get; set; }
        public string Status { get; set; } // success | failed
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Message { get; set; }
    }
}
