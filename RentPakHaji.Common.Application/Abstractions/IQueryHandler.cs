using MediatR;

namespace RentPakHaji.Common.Application.Abstractions;

/// <summary>Handler for IQuery&lt;TResponse&gt;.</summary>
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
