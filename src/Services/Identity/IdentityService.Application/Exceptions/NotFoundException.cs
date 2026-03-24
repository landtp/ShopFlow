namespace IdentityService.Application.Exceptions;

public sealed class NotFoundException(string name, object key)
    : Exception($"{name} với id '{key}' không tìm thấy");
