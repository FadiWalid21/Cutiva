using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(_ => localizer["AuthEmailRequired"])
            .EmailAddress()
            .WithMessage(_ => localizer["AuthEmailInvalid"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(_ => localizer["AuthPasswordRequired"]);
    }
}
