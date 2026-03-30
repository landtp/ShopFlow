using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentService.Infrastructure.Messaging;

// Monitor DLQ — log và alert khi có dead letter messages
public sealed class DeadLetterQueueProcessor(
    ServiceBusClient client,
    ILogger<DeadLetterQueueProcessor> logger)
    : BackgroundService
{
    private const string TopicName = "order-created";
    private const string SubscriptionName = "payment-service";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Receiver cho Dead Letter Queue
        var receiver = client.CreateReceiver(
            TopicName,
            SubscriptionName,
            new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter
            });

        logger.LogInformation("DLQ Processor started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessDeadLettersAsync(receiver, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DLQ Processor error");
            }

            // Check DLQ mỗi 30 giây
            await Task.Delay(TimeSpan.FromSeconds(30), ct);
        }

        await receiver.DisposeAsync();
    }

    private async Task ProcessDeadLettersAsync(
        ServiceBusReceiver receiver,
        CancellationToken ct)
    {
        var messages = await receiver.ReceiveMessagesAsync(
            maxMessages: 10,
            maxWaitTime: TimeSpan.FromSeconds(5),
            cancellationToken: ct);

        if (messages.Count == 0) return;

        logger.LogWarning(
            "Found {Count} dead letter messages!", messages.Count);

        foreach (var message in messages)
        {
            // Log đầy đủ thông tin để investigate
            logger.LogError(
                "DEAD LETTER — MessageId: {MessageId} | " +
                "SessionId: {SessionId} | " +
                "Reason: {Reason} | " +
                "Description: {Description} | " +
                "DeliveryCount: {DeliveryCount} | " +
                "EnqueuedAt: {EnqueuedAt}",
                message.MessageId,
                message.SessionId,
                message.DeadLetterReason,
                message.DeadLetterErrorDescription,
                message.DeliveryCount,
                message.EnqueuedTime);

            // TODO production:
            // → Gửi alert email/Slack
            // → Lưu vào DB để investigate
            // → Tự động retry nếu có thể

            // Complete để xoá khỏi DLQ sau khi log
            await receiver.CompleteMessageAsync(message, ct);
        }
    }
}