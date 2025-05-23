using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Azure;

namespace PublishWorker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        builder.Services.AddAzureClients((clientBuilder) =>
        {
            clientBuilder.AddClient<EventGridPublisherClient, EventGridPublisherClientOptions>((options, credential, provider) =>
            {
                var configuration = builder.Configuration;

                var namespaceEndpoint = configuration["NamespaceEndpoint"];
                var topicEndpoint = configuration["TopicEndpoint"];

                if (string.IsNullOrEmpty(topicEndpoint))
                {
                    throw new InvalidOperationException("TopicEndpoint configuration value is missing or empty.");
                }

                return new EventGridPublisherClient(new Uri(topicEndpoint),
                       new DefaultAzureCredential());
            });
        });

        var host = builder.Build();
        host.Run();
    }
}