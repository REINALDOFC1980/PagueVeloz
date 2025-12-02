using FluentValidation;
using PagueVeloz.API.DTOs;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Api.Validators
{
    public class TransactionCreateValidator : AbstractValidator<TransactionCreateDto>
    {
        public TransactionCreateValidator()
        {
            // ------------------------------
            // Campos obrigatórios
            // ------------------------------

            RuleFor(x => x.Operation)
                .IsInEnum()
                .WithMessage("Operação inválida.");

            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("AccountId é obrigatório.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("O valor da transação deve ser maior que zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("A moeda é obrigatória.");

            RuleFor(x => x.ReferenceId)
                .NotEmpty()
                .WithMessage("ReferenceId é obrigatório.");

            // ------------------------------
            // Validações específicas por operação
            // ------------------------------

            // Regras de negócio simples (pré-validação)
            When(x => x.Operation == TransactionType.Credit, () =>
            {
                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Crédito deve ter valor maior que zero.");
            });

            When(x => x.Operation == TransactionType.Debit, () =>
            {
                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Débito deve ter valor maior que zero.");
            });

            When(x => x.Operation == TransactionType.Reserve, () =>
            {
                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Reserva deve ter valor maior que zero.");
            });

            When(x => x.Operation == TransactionType.Capture, () =>
            {
                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Captura deve ter valor maior que zero.");
            });

            When(x => x.Operation == TransactionType.Reversal, () =>
            {
                RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Estorno deve ter valor maior que zero.");
            });
        }
    }
}
