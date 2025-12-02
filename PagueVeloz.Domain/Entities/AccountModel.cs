
using System;

namespace PagueVeloz.Domain.Entities
{
    public enum AccountStatus
    {
        
        Inactive,
        Active,
        Blocked
    }

    public class AccountModel
    {
        public Guid AccountId { get; set; }
        public string? AccountNumber { get; set; }
        public decimal Balance { get; set; } // saldo disponível em centavos
        public decimal ReservedBalance { get; set; } // saldo reservado em centavos
        public decimal CreditLimit { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; set; }
    }
}
