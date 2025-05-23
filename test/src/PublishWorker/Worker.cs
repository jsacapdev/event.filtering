using System.Text.Json;
using Azure.Core.Serialization;
using Azure.Messaging;
using Azure.Messaging.EventGrid;

namespace PublishWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly EventGridPublisherClient _eventGridPublisherClient;

    public Worker(ILogger<Worker> logger, EventGridPublisherClient eventGridPublisherClient)
    {
        _logger = logger;

        _eventGridPublisherClient = eventGridPublisherClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<EventGridEvent> eventsList = new List<EventGridEvent>
                {
                    new EventGridEvent("/canonical/transport-movement",
                                       "com.ext.consumer.1.event.eventcreated",
                                       "1.0",
                                       new CustomModel()
                                       {
                                            key1 = "7",
                                            longitude = "38.360778548329144",
                                            latitude = "-125.22284557865196"
                                       })
                };

                await _eventGridPublisherClient.SendEventsAsync(eventsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
