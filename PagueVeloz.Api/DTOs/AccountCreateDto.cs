using Swashbuckle.AspNetCore.Annotations;

namespace PagueVeloz.Api.DTOs
{
    public class AccountCreateDto
    {



        /// <summary>Número da conta</summary>
        [SwaggerSchema("Número da conta: CC-0001")]
        public string? AccountNumber { get; set; }

        /// <summary>Saldo da conta ex:1000.00</summary>
        [SwaggerSchema("Saldo da conta: 1000.00")]
        public decimal Balance { get; set; }

        /// <summary>Saldo reservado ex:1000.00</summary>
        [SwaggerSchema("Saldo reservado: 1000.00")]
        public decimal ReservedBalance { get; set; }

        /// <summary>Limite de crédito ex:1000.00</summary>
        [SwaggerSchema("Limite de crédito: 1000.00")]
        public decimal CreditLimit { get; set; }
    }
}
