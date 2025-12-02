using Microsoft.AspNetCore.Mvc;
using PagueVeloz.API.DTOs;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

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
    /// Processa uma operação financeira (Credit, Debit, Reserve, Capture, Reversal)
    /// </summary>
    /// <remarks>
    /// Exemplos de request:
    ///
    /// Crédito:
    /// POST /api/Transaction
    /// {
    ///   "operation": "Credit",
    ///   "accountId": "4D1746D2-5770-4820-8381-18EDB119846B",
    ///   "amount": 50000,
    ///   "currency": "BRL",
    ///   "referenceId": "test-credit-001"
    /// }
    ///
    /// Transferência:
    /// POST /api/transfer
    /// {
    ///   "operation": "Transfer",
    ///   "accountId": "4D1746D2-5770-4820-8381-18EDB119846B",
    ///   "targetAccountId": "32317970-9624-40B4-B9EE-80D0146D2E3B",
    ///   "amount": 10000,
    ///   "currency": "BRL",
    ///   "referenceId": "test-transfer-001"
    /// }
    ///
    /// </remarks>
    /// <param name="request">Objeto com os dados da transação</param>
    /// <returns>Transação processada com sucesso</returns>
    /// <response code="201">Transação criada</response>
    /// <response code="400">Erro de validação</response>
    /// <response code="500">Erro interno</response>
    [HttpPost]
        [SwaggerOperation(Summary = "Processa uma transação", Description = "Inclui crédito, débito, reserva, captura, estorno ou transferência")]
        [ProducesResponseType(typeof(TransactionCreateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto dto)
    {
       
            Log.Information("Recebida solicitação de movimentação do tipo: {dto.Operation} .");


            string idempotencyKey = Request.Headers["Idempotency-Key"];
            if (string.IsNullOrEmpty(idempotencyKey))
            {
                idempotencyKey = Guid.NewGuid().ToString();
                Log.Warning("Idempotency-Key não enviada, gerando uma aleatória.");
            }

            var model = new TransactionModel
            {
                TransactionId = Guid.NewGuid(),
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Operation = dto.Operation,
                ReferenceId = dto.ReferenceId
            };

            var result = await _transactionService.ProcessTransactionAsync(model,idempotencyKey);
            return Ok(result);
       
       
    }


    /// <summary>
    /// Realizar Trasnferencia bancária
    /// </summary>
    [HttpPost("transferencia")]
    public async Task<IActionResult> Transfer([FromBody] TransferCreateDto dto)
    {
        Log.Information("Recebida solicitação de movimentação do tipo: {dto.Operation} .");


        string idempotencyKey = Request.Headers["Idempotency-Key"];
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            idempotencyKey = Guid.NewGuid().ToString();
            Log.Warning("Idempotency-Key não enviada, gerando uma aleatória.");
        }

       
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

            var result = await _transactionService.ProcessTransactionAsync(model, idempotencyKey);
            return Ok(result);
        

        
    }

}
