using MediatR;

namespace IdentityService.Application.Abstractions;

public interface ICommand : IRequest<Unit> { }
public interface ICommand<TResponse> : IRequest<TResponse> { }
