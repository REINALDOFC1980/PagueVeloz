using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace PagueVeloz.Domain.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
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

        public TransactionType Operation { get; set; } // usando enum
        public Guid AccountId { get; set; }
        public Guid? DestinationAccountId { get; set; } // opcional para transfer

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BRL";

        public string ReferenceId { get; set; } // idempotência

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Dados retornados no processamento
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Message { get; set; }
    }

}
