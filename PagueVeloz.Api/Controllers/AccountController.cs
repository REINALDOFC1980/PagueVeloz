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
        public async Task<ActionResult> CriarConta(AccountCreateDto dto)
        {
            Log.Information("Recebida solicitação de criação de conta.");



            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = new AccountModel
            {
                Balance = dto.Balance,
                ReservedBalance = dto.ReservedBalance,
                CreditLimit = dto.CreditLimit
            };

            var created = await _serviceAccount.CreateAccountAsync(account);

            Log.Information("Conta {AccountId} criada no banco.", account.AccountId);

            return Ok(created);

        }

        [HttpGet("BuscarConta/{accountId}")]
        public async Task<IActionResult> GetById(Guid accountId)
        {
            var account = await _serviceAccount.GetAccountByIdAsync(accountId);
            return Ok(account);
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> Update(Guid accountId, AccountUpdateDto dto)
        {
            var account = await _serviceAccount.GetAccountByIdAsync(accountId);

            account.Balance = dto.Balance;
            account.ReservedBalance = dto.ReservedBalance;
            account.CreditLimit = dto.CreditLimit;
            account.Status = AccountStatus.Active;

            await _serviceAccount.UpdateAccountAsync(account);

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
