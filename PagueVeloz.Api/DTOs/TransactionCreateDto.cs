using PagueVeloz.Domain.Entities;

public class TransactionCreateDto
{
    public Guid AccountId { get; set; }                  // Conta de origem
    public decimal Amount { get; set; }
    public string ReferenceId { get; set; } = null!;
    public TransactionOperation Operation { get; set; }
    public Guid? DestinationAccountId { get; set; }     // Apenas usado para Transfer
}
