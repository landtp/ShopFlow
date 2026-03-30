namespace PaymentService.Domain.Exceptions;

public sealed class BusinessException(string message)
    : Exception(message);
