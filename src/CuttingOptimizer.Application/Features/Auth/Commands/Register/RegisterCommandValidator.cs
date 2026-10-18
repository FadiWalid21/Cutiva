using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(_ => localizer["AuthEmailRequired"])
            .EmailAddress()
            .WithMessage(_ => localizer["AuthEmailInvalid"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(_ => localizer["AuthPasswordRequired"])
            .MinimumLength(8)
            .WithMessage(_ => localizer["AuthPasswordMinLength"])
            .Matches(@"\d")
            .WithMessage(_ => localizer["AuthPasswordComplexity"]);

        RuleFor(x => x.DisplayName)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName))
            .WithMessage(_ => localizer["AuthDisplayNameMaxLength"]);
    }
}
