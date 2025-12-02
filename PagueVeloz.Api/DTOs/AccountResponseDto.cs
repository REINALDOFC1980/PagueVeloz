public class AccountResponseDto
{
    public Guid AccountId { get; set; }
    public string? AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Message { get; set; }
}
