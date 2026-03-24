using MediatR;

namespace IdentityService.Application.Abstractions;

public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Unit>
    where TCommand : ICommand
{ }

public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{ }
