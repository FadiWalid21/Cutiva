using FluentValidation;
using MediatR;

namespace CuttingOptimizer.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationContext = new ValidationContext<TRequest>(request);

        var errors = (await Task.WhenAll(
                validators.Select(validator =>
                    validator.ValidateAsync(validationContext, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (errors.Count != 0)
            throw new ValidationException(errors);

        return await next();
    }
}