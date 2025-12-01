
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
        public long Balance { get; set; } // saldo disponível em centavos
        public long ReservedBalance { get; set; } // saldo reservado em centavos
        public long CreditLimit { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
