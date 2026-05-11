using MediatR;

namespace RentPakHaji.Common.Application.Abstractions;

/// <summary>Query that returns a typed result wrapped in Result&lt;T&gt;.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
