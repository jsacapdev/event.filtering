using Azure.Core;
using Azure.Messaging.EventGrid.Namespaces;
using CloudNative.CloudEvents.SystemTextJson;

namespace PublishWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly EventGridSenderClient _eventGridSenderClient;

    public Worker(ILogger<Worker> logger, EventGridSenderClient eventGridSenderClient)
    {
        _logger = logger;

        _eventGridSenderClient = eventGridSenderClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var evt = new CloudNative.CloudEvents.CloudEvent
                {
                    Source = new Uri("/distribution-service"),
                    Type = "com.ext.consumer.1.event.eventcreated",
                    Subject = "/canonical/transport-movement",
                    Time = DateTime.UtcNow,
                    Data = new CustomModel()
                    {
                        key1 = "7",
                        latitude = "38.360778548329144",
                        longitude = "-125.22284557865196"
                    },
                    Id = Guid.NewGuid().ToString()
                };

                var jsonFormatter = new JsonEventFormatter();

                await _eventGridSenderClient.SendEventAsync(
                    RequestContent.Create(jsonFormatter.EncodeStructuredModeMessage(evt, out _)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
