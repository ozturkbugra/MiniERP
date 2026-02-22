using FluentValidation;
using MediatR;

namespace MiniERP.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(); // Kural yoksa direkt geç
        }

        var context = new ValidationContext<TRequest>(request);

        // 1. Kuralları çalıştır ve hata nesnelerini (ValidationFailure) topla
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        // 2. Eğer hata varsa, FluentValidation'ın kendi exception'ına listeyi doğrudan ver
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}