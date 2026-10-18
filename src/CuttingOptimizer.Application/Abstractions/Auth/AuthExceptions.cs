namespace CuttingOptimizer.Application.Abstractions.Auth;

public sealed class AuthDuplicateEmailException : Exception;

public sealed class AuthOperationException(IReadOnlyList<string> errors) : Exception
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
