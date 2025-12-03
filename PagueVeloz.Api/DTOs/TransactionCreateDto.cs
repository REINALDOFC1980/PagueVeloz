
using CommandLine.Text;
using PagueVeloz.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;


namespace PagueVeloz.API.DTOs
{

    public class TransactionCreateDto
    {
        /// <summary>
        /// Tipo de operação: (Credit, Debit, Reserve, Capture, Reversal)
        /// </summary>
        [SwaggerSchema("Tipo da operação")]
        public TransactionType Operation { get; set; }

        /// <summary>ID da conta de origem | ex:D4F55942-79E4-4080-B14E-7F9F6FBC5394</summary>
        [SwaggerSchema("ID da conta de origem")]
        public Guid AccountId { get; set; }

        /// <summary>Valor da operação em centavos | ex:1000.00</summary>
        [SwaggerSchema(Description = "50000")]
        public decimal Amount { get; set; }

        /// <summary>Moeda da transação | ex:BRL</summary>
        [SwaggerSchema(Description = "BRL")]
        public string Currency { get; set; } = "BRL";

        /// <summary>ID de referência ou chave de idempotência | ex:credit-001</summary>
        [SwaggerSchema(Description = "credit-001")]
        public string ReferenceId { get; set; }
    }
}
