namespace PagueVeloz.API.DTOs
{
    public class TransferCreateDto
    {
        public Guid AccountId { get; set; }
        public Guid DestinationAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BRL";
        public string ReferenceId { get; set; }
    }
}
