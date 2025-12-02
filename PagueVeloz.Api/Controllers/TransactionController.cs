using Microsoft.AspNetCore.Mvc;
using PagueVeloz.API.DTOs;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Processa: credit, debit, reserve, capture, reversal.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto dto)
    {
        try
        {
            var model = new TransactionModel
            {
                TransactionId = Guid.NewGuid(),
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Operation = dto.Operation,
                ReferenceId = dto.ReferenceId
            };

            var result = await _transactionService.ProcessTransactionAsync(model);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                detail = ex.StackTrace
            });
        }
    }


    /// <summary>
    /// Endpoint exclusivo para transferências.
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferCreateDto dto)
    {
        var model = new TransactionModel
        {
            TransactionId = Guid.NewGuid(),
            AccountId = dto.AccountId,
            DestinationAccountId = dto.DestinationAccountId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Operation = TransactionType.Transfer,
            ReferenceId = dto.ReferenceId
        };

        var result = await _transactionService.ProcessTransactionAsync(model);
        return Ok(result);
    }

}
