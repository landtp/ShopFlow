namespace OrderService.Domain.Primitives;

// Exception thuần domain — không biết HTTP status code là gì
public sealed class DomainException(string message) : Exception(message);


