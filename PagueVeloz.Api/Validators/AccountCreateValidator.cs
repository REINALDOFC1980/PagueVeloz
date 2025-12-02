using FluentValidation;
using PagueVeloz.Api.DTOs;

namespace PagueVeloz.Api.Validators
{
    public class AccountCreateValidator : AbstractValidator<AccountCreateDto>
    {
        public AccountCreateValidator()
        {

            RuleFor(x => x.AccountNumber)
                 .NotNull()
                 .WithMessage("O número da conta não pode ser nulo.")
                 .NotEmpty()
                 .WithMessage("O número da conta não pode ser vazio.")
                 .MaximumLength(20)
                 .WithMessage("O número da conta não pode ter mais que 20 caracteres.");

            RuleFor(x => x.Balance)
                .GreaterThanOrEqualTo(0)
                .WithMessage("O saldo inicial não pode ser negativo.");

            RuleFor(x => x.ReservedBalance)
                .GreaterThanOrEqualTo(0)
                .WithMessage("O saldo reservado não pode ser negativo.");

            RuleFor(x => x.CreditLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage("O limite de crédito deve ser maior ou igual a zero.");

            RuleFor(x => x)
                .Must(x => x.Balance + x.CreditLimit >= x.ReservedBalance)
                .WithMessage("Saldo + limite precisa ser >= saldo reservado.");
        }
    }
}
