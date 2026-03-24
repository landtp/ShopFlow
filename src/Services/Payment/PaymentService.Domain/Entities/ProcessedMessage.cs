namespace PaymentService.Domain.Entities;

// Idempotency table — đảm bảo mỗi Kafka message chỉ xử lý 1 lần
public sealed class ProcessedMessage
{
    public Guid MessageId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}