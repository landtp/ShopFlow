namespace OrderService.Application.Exceptions;

// Application exception — API layer sẽ catch và map sang 404
public sealed class NotFoundException(string name, object key)
    : Exception($"{name} với id '{key}' không tìm thấy");
