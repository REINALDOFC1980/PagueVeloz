namespace PagueVeloz.Api.DTOs
{
    public class AccountCreateDto
    {
        public long Balance { get; set; }
        public long ReservedBalance { get; set; }
        public long CreditLimit { get; set; }
    }
}
