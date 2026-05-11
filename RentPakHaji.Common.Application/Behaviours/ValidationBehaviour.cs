using FluentValidation;
using MediatR;

namespace RentPakHaji.Common.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour — runs all FluentValidation validators
/// before the handler executes. Returns a Failure result if validation fails.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // If TResponse is a Result type, return Failure instead of throwing
        if (typeof(TResponse).IsAssignableTo(typeof(Result)))
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var errorCode = failures.First().ErrorCode ?? "VALIDATION_ERROR";

            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(errorCode, errorMessage);

            // Result<T> — find generic arg and call Failure via reflection
            var valueType = typeof(TResponse).GetGenericArguments().FirstOrDefault();
            if (valueType is not null)
            {
                var method = typeof(Result<>)
                    .MakeGenericType(valueType)
                    .GetMethod(nameof(Result.Failure), [typeof(string), typeof(string)]);
                return (TResponse)method!.Invoke(null, [errorCode, errorMessage])!;
            }
        }

        throw new ValidationException(failures);
    }
}
