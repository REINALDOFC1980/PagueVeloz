using FluentValidation;
using PagueVeloz.Api.DTOs;
using PagueVeloz.API.DTOs;

namespace PagueVeloz.Api.Validators
{
    public class TransferCreateValidator : AbstractValidator<TransferCreateDto>
    {
        public TransferCreateValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("A conta de origem é obrigatória.");

            RuleFor(x => x.DestinationAccountId)
                .NotEmpty()
                .WithMessage("A conta de destino é obrigatória.")
                .NotEqual(x => x.AccountId)
                .WithMessage("A conta de origem não pode ser igual à conta de destino.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("O valor da transferência deve ser maior que zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("A moeda é obrigatória.")
                .Length(3)
                .WithMessage("A moeda deve ter exatamente 3 caracteres.");
        }
    }
}
