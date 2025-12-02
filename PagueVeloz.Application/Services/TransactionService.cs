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

namespace PagueVeloz.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IAuditService _serviceAudi;
        private readonly IDbConnection _connection;

        public TransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IIdempotencyService idempotencyService,
            IAuditService serviceAudi,
            IDbConnection connection)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _idempotencyService = idempotencyService;
            _serviceAudi = serviceAudi;
            _connection = connection;
        }

        public async Task<TransactionModel> ProcessTransactionAsync(TransactionModel dto, string idempotencyKey)
        {
            var retryPolicy = RetryPolicyProvider.GetRetryPolicy();

            return await retryPolicy.ExecuteAsync(async () =>
            {

                Log.Information("Iniciando a movimentação de conta.", dto.AccountId);

                if (await _transactionRepository.ExistsByReferenceIdAsync(dto.ReferenceId))
                {
                    var savedResponse = await _idempotencyService.GetSavedResponseAsync(dto.ReferenceId);
                    Log.Information("Idempotência encontrada para a chave {IdempotencyKey}", dto.ReferenceId);

                    var contaexistente = JsonSerializer.Deserialize<TransactionModel>(savedResponse)!;
                    return contaexistente;

                }

                // Carrega a conta dentro da transação
                var account = await _accountRepository.GetAccountByIdAsync(dto.AccountId);
                if (account == null)
                    throw new BusinessException($"Numero da conta não existe!");

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
                        TransactionType.Transfer => await TransferAsync(account, dto.DestinationAccountId.Value, dto.Amount, transaction),
                        _ => throw new BusinessException("Operação inválida")
                    };

                    // Atualiza conta principal
                    await _accountRepository.UpdateAccountAsync(account, transaction);


                    // Persiste a transação
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

                    _serviceAudi.LogTransaction(transactionToSave);

                    await _transactionRepository.SaveAsync(transactionToSave, transaction);

                    transaction.Commit();
                    return result;
                }
                catch
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
            if (amount > account.Balance + account.CreditLimit)
                return FailResult(account, "Saldo insuficiente");

            account.Balance -= amount;
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


            decimal fromReserved = Math.Min(amount, account.ReservedBalance);
            account.ReservedBalance -= fromReserved;
            account.Balance += fromReserved;
            return SuccessResult(account, "Estorno realizado com sucesso");
        }

        private async Task<TransactionModel> TransferAsync(
            AccountModel source,
            Guid destinationId,
            decimal amount,
            IDbTransaction dbTransaction)
        {
            var destination = await _accountRepository.GetAccountByIdAsync(destinationId);
            if (destination == null)
            {
                Log.Warning("Conta destino não encontrada. SourceId: {SourceId}, DestinationId: {DestinationId}",
                source.AccountId, destinationId);
                throw new BusinessException("Conta destino não encontrada!");
            }


            decimal available = source.Balance - source.ReservedBalance + source.CreditLimit;

            if (amount > available)
                return FailResult(source, "Saldo insuficiente para transferência");

            // Débita origem
            source.Balance -= amount;

            // Credita destino
            destination.Balance += amount;

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

