using MediatR;

namespace RentPakHaji.Common.Application.Abstractions;

/// <summary>Handler for ICommand&lt;TResponse&gt;.</summary>
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }

/// <summary>Handler for ICommand (no response payload).</summary>
public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }
