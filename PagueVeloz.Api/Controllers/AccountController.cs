using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Api.DTOs;
using PagueVeloz.Application.DTOs;
using PagueVeloz.Application.Interfaces;

using PagueVeloz.Domain.Entities;
using Serilog;


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

        [HttpPost("CriarConta")]
        public async Task<IActionResult> CriarConta([FromBody] AccountCreateDto dto)
        {
            try
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
                    Balance = dto.Balance,
                    CreditLimit = dto.CreditLimit,
                    ReservedBalance = 0
                };

                // Chama o Service com idempotência
                var createdAccount = await _serviceAccount.CreateAccountAsync(account, idempotencyKey);

                var response = new AccountResponseDto
                {
                    AccountId = createdAccount.AccountId,
                    Balance = createdAccount.Balance,
                    ReservedBalance = createdAccount.ReservedBalance,
                    CreditLimit = createdAccount.CreditLimit,
                    Status = createdAccount.Status.ToString(),
                    CreatedAt = createdAccount.CreatedAt,
                    UpdatedAt = createdAccount.UpdatedAt
                };


                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao criar conta.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    

        [HttpGet("BuscarConta/{accountId}")]
        public async Task<IActionResult> GetById(Guid accountId)
        {
            var account = await _serviceAccount.GetAccountByIdAsync(accountId);
            return Ok(account);
        }

        [HttpPut("AtualizarConta/{accountId}")]
        public async Task<IActionResult> Update(Guid accountId, AccountUpdateDto dto)
        {
            var account = await _serviceAccount.GetAccountByIdAsync(accountId);

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
