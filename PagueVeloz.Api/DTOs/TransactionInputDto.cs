namespace PagueVeloz.Api.DTOs
{
    public class TransactionInputDto
    {
        public string Operation { get; set; } = null!;
        public string AccountId { get; set; } = null!;        // número da conta origem
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BRL";
        public string ReferenceId { get; set; } = null!;
        public string? DestinationAccountId { get; set; }
    }
}
