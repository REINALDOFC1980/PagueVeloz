
using CommandLine.Text;
using PagueVeloz.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;


namespace PagueVeloz.API.DTOs
{

    /// <summary>
    /// DTO para criação de transações
    /// </summary>
    /// 


    public class TransactionCreateDto
    {
        /// <summary>
        /// Tipo da operação (crédito, débito, reserva, captura, estorno, transferência)
        /// </summary>
        /// 

        [SwaggerSchema("Tipo da operação", Description= "")]
        public TransactionType Operation { get; set; }

        /// <summary>ID da conta de origem</summary>
        public Guid AccountId { get; set; }


        /// <summary>Valor da operação em centavos</summary>
        [SwaggerSchema(Description = "50000")]
        public decimal Amount { get; set; }

        /// <summary>Moeda da transação</summary>
        [SwaggerSchema(Description = "BRL")]
        public string Currency { get; set; } = "BRL";

        /// <summary>ID de referência ou chave de idempotência</summary>
        [SwaggerSchema(Description = "test-credit-001")]
        public string ReferenceId { get; set; }
    }
}
