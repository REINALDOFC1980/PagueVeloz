using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Api.DTOs;
using PagueVeloz.API.DTOs;
using PagueVeloz.Application.DTOs;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Shared.Middlewares;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;


namespace PagueVeloz.Api.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _serviceAccount;
        public AccountController(IAccountService autorService)
        {
            _serviceAccount = autorService;
        }
        /// <summary>
        /// Cria uma nova conta bancária
        /// </summary>

        /// <remarks>
        /// Exemplos de request:
        ///
        /// Crédito:
        /// POST /api/Account/CriarConta
        /// {
        ///   "accountNumber": "CC-0001",
        ///   "balance": "0",
        ///   "reservedBalance": 0,
        ///   "creditLimit": "0",
        ///   "referenceId": "test-credit-001"
        /// }
        ///        
        /// </remarks>
        /// <param name="request">Dados da conta a ser criada</param>
        /// <returns>Conta criada com sucesso</returns>
        /// <response code="201">Conta criada</response>
        /// <response code="400">Erro de validação</response>
        /// <response code="500">Erro interno</response>
        [ProducesResponseType(typeof(TransactionCreateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("CriarConta")]
        public async Task<IActionResult> CriarConta([FromBody] AccountCreateDto dto)
        {
            
                Log.Information("Recebida solicitação de criação de conta.");

                string idempotencyKey = Request.Headers["Idempotency-Key"];
                if (string.IsNullOrEmpty(idempotencyKey))
                {                
                    idempotencyKey = Guid.NewGuid().ToString();
                    Log.Warning("Idempotency-Key não enviada, gerando uma aleatória.");
                }

          

            // Mapeia DTO para Model
            var account = new AccountModel
                {
                    AccountNumber = dto.AccountNumber,
                    Balance = dto.Balance,
                    CreditLimit = dto.CreditLimit,
                    ReservedBalance = 0
                };

                // Chama o Service com idempotência
                var createdAccount = await _serviceAccount.CreateAccountAsync(account, idempotencyKey);

                var response = new AccountResponseDto
                {
                    AccountId = createdAccount.AccountId,
                    AccountNumber = createdAccount.AccountNumber,
                    Balance = createdAccount.Balance,
                    ReservedBalance = createdAccount.ReservedBalance,
                    CreditLimit = createdAccount.CreditLimit,
                    Status = createdAccount.Status.ToString(),
                    CreatedAt = createdAccount.CreatedAt,
                    UpdatedAt = createdAccount.UpdatedAt

                };


                return Ok(response);
            
        }


        /// <summary>
        /// Buscar conta bancária
        /// </summary>
        [HttpGet("BuscarConta/{AccountNumber}")]
        public async Task<IActionResult> GetById(string AccountNumber)
        {
            var account = await _serviceAccount.GetAccountByNumberAsync(AccountNumber);
            return Ok(account);
        }

        /// <summary>
        /// Atualizar conta bancária
        /// </summary>
        [HttpPut("AtualizarConta/{AccountNumber}")]
        public async Task<IActionResult> Update(string AccountNumber, AccountUpdateDto dto)
        {
            var account = await _serviceAccount.GetAccountByNumberAsync(AccountNumber);

            account.Balance = dto.Balance;
            account.ReservedBalance = dto.ReservedBalance;
            account.CreditLimit = dto.CreditLimit;
            account.Status = AccountStatus.Active;

            await _serviceAccount.UpdateBalanceAsync(account);

            // Retorna o objeto atualizado
            return Ok(new
            {
                account.AccountId,
                account.Balance,
                account.ReservedBalance,
                account.CreditLimit,
                Status = AccountStatus.Active.ToString(),
                account.CreatedAt,
                account.UpdatedAt
            });
        }

    }
}
