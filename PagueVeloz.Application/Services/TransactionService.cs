using Dapper;
using PagueVeloz.Application.Interfaces;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Infrastructure.Repositories.Account;
using PagueVeloz.Infrastructure.Repositories.Transactions;
using PagueVeloz.Shared.Middlewares;
using Serilog;
using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using WebApiBiblioteca.Service.RabbitMQ;


namespace PagueVeloz.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IDbConnection _connection;
        private readonly IRabbitMQService _rabbitMQService;


        public TransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IIdempotencyService idempotencyService,
            IDbConnection connection,
            IRabbitMQService rabbitMQService)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _idempotencyService = idempotencyService;
            _connection = connection;
            _rabbitMQService = rabbitMQService;
        }

        public async Task<TransactionModel> ProcessTransactionAsync(TransactionModel dto, string idempotencyKey)
        {
            var retryPolicy = RetryPolicyProvider.GetRetryPolicy();

            return await retryPolicy.ExecuteAsync(async () =>
            { 
               
                    Log.Information("Iniciando movimentação de conta. IdempotencyKey: {IdempotencyKey}", idempotencyKey);

                    // Idempotência
                    var savedResponse = await _idempotencyService.GetSavedResponseAsync(idempotencyKey);
                    if (!string.IsNullOrEmpty(savedResponse))
                        return JsonSerializer.Deserialize<TransactionModel>(savedResponse)!;

                    // Verifica referência
                    if (await _transactionRepository.ExistsByReferenceIdAsync(dto.ReferenceId))
                        throw new BusinessException("Transação já foi realizada.");

                    // Carrega conta principal
                    var account = await _accountRepository.GetAccountByIdAsync(dto.AccountId);
                    if (account == null)
                        throw new BusinessException("Número da conta não existe!");


                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using var transaction = _connection.BeginTransaction();
                try
                {
                    // Executa operação
                    TransactionModel result = dto.Operation switch
                    {
                        TransactionType.Credit => Credit(account, dto.Amount),
                        TransactionType.Debit => Debit(account, dto.Amount),
                        TransactionType.Reserve => Reserve(account, dto.Amount),
                        TransactionType.Capture => Capture(account, dto.Amount),
                        TransactionType.Reversal => Reversal(account, dto.Amount),
                        TransactionType.Transfer => await TransferAsync(account, dto.DestinationAccountId!.Value, dto.Amount, transaction),
                        _ => throw new BusinessException("Operação inválida")
                    };

                 
                    // Atualiza conta principal
                    await _accountRepository.UpdateAccountAsync(account, transaction);

                    // Persiste transação
                    var transactionToSave = new TransactionModel
                    {
                        TransactionId = Guid.NewGuid(),
                        Operation = dto.Operation,
                        AccountId = dto.AccountId,
                        DestinationAccountId = dto.DestinationAccountId,
                        Amount = dto.Amount,
                        Currency = dto.Currency,
                        ReferenceId = dto.ReferenceId,
                        Status = result.Status,
                        CreatedAt = DateTime.UtcNow,
                        Balance = result.Balance,
                        AvailableBalance = result.AvailableBalance,
                        Message = result.Message
                    };

                    await _transactionRepository.SaveAsync(transactionToSave, transaction);

                    transaction.Commit();

                    // Publica a transação no RabbitMQ
                    _rabbitMQService.PublicarMensagem(new
                    {
                        transactionToSave.TransactionId,
                        transactionToSave.Operation,
                        transactionToSave.AccountId,
                        transactionToSave.DestinationAccountId,
                        transactionToSave.Amount,
                        transactionToSave.Currency,
                        transactionToSave.ReferenceId,
                        transactionToSave.Status,
                        transactionToSave.CreatedAt
                    }, "transaction");


                    return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        #region Operações Financeiras
        private TransactionModel Credit(AccountModel account, decimal amount)
        {
            
            account.Balance += amount;
            return SuccessResult(account, "Crédito realizado com sucesso");
        }

        private TransactionModel Debit(AccountModel account, decimal amount)
        {
            decimal available = account.Balance + account.CreditLimit;
            if (amount > available)
                return FailResult(account, "Saldo insuficiente");

            if (amount <= account.Balance)
                account.Balance -= amount;
            else
            {
                decimal remaining = amount - account.Balance;
                account.Balance = 0;
                account.CreditLimit -= remaining;
            }

            return SuccessResult(account, "Débito realizado com sucesso");
        }

        private TransactionModel Reserve(AccountModel account, decimal amount)
        {
            if (amount > account.Balance)
                return FailResult(account, "Saldo insuficiente para reserva");

            account.Balance -= amount;
            account.ReservedBalance += amount;
            return SuccessResult(account, "Reserva realizada com sucesso");
        }

        private TransactionModel Capture(AccountModel account, decimal amount)
        {
            if (amount > account.ReservedBalance)
                return FailResult(account, "Não há reserva suficiente para capturar");

            account.ReservedBalance -= amount;
            return SuccessResult(account, "Captura realizada com sucesso");
        }

        private TransactionModel Reversal(AccountModel account, decimal amount)
        {
            if (account.ReservedBalance <= 0)
                return FailResult(account, "Não é possível estornar: valor já capturado.");

            decimal fromReserved = Math.Min(amount, account.ReservedBalance);
            account.ReservedBalance -= fromReserved;
            account.Balance += fromReserved;

            decimal remaining = amount - fromReserved;
            if (remaining > 0)
                account.Balance += remaining;

            return SuccessResult(account, "Estorno realizado com sucesso");
        }

        #endregion

        #region Transferência
        private async Task<TransactionModel> TransferAsync(
            AccountModel source,
            Guid destinationId,
            decimal amount,
            IDbTransaction dbTransaction)
        {
            var destination = await _accountRepository.GetAccountByIdAsync(destinationId, dbTransaction);
            if (destination == null)
                throw new BusinessException("Conta destino não encontrada!");

            decimal available = source.Balance;
            if (amount > available)
                return FailResult(source, "Saldo insuficiente para transferência");

            // Débito origem
            source.Balance -= amount;

            // Crédito destino
            destination.Balance += amount;

            // Atualiza conta destino dentro da MESMA transação
            await _accountRepository.UpdateAccountAsync(destination, dbTransaction);

            return SuccessResult(source, "Transferência realizada com sucesso");
        }

        #endregion

        #region Resultados
        private TransactionModel SuccessResult(AccountModel account, string message) =>
            new()
            {
                TransactionId = Guid.NewGuid(),
                Status = TransactionStatus.Completed,
                Balance = account.Balance,
                AvailableBalance = account.Balance - account.ReservedBalance,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

        private TransactionModel FailResult(AccountModel account, string message) =>
            new()
            {
                TransactionId = Guid.NewGuid(),
                Status = TransactionStatus.Failed,
                Balance = account.Balance,
                AvailableBalance = account.Balance - account.ReservedBalance,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };
        #endregion
    }
}
