using MediatR;

namespace OrderService.Application.Abstractions;

// Query luôn trả về data
public interface IQuery<TResponse> : IRequest<TResponse> { }

