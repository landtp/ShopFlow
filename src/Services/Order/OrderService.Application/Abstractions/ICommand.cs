using MediatR;

namespace OrderService.Application.Abstractions;

// Command không trả về data — chỉ trả về Unit (void)
public interface ICommand : IRequest<Unit> { }


// Command có trả về data — ví dụ trả về Id sau khi tạo
public interface ICommand<TResponse> : IRequest<TResponse> { }