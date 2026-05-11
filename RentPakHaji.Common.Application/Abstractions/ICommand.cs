using MediatR;

namespace RentPakHaji.Common.Application.Abstractions;

/// <summary>Command that returns a typed result.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

/// <summary>Command with no result payload (only success/failure).</summary>
public interface ICommand : IRequest<Result> { }
