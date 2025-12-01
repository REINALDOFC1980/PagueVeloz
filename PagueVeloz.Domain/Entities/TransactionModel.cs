using System;

namespace PagueVeloz.Domain.Entities
{
    public enum TransactionOperation
    {
        Credit,
        Debit,
        Reserve,
        Capture,
        Reversal,
        Transfer
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed
    }

    public class TransactionModel
    {
        public Guid TransactionId { get; set; }
        public TransactionOperation Operation { get; set; }
        public Guid AccountId { get; set; }
        public long Amount { get; set; } // em centavos
        public string Currency { get; set; } // ex: BRL
        public string ReferenceId { get; set; } // idempotência
        public string Status { get; set; }
        public long Balance { get; set; } // saldo após a operação
        public long ReservedBalance { get; set; } // saldo reservado após a operação
        public long AvailableBalance { get; set; } // saldo disponível após a operação
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ErrorMessage { get; set; }
    }
}
