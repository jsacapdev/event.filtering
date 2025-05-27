using Azure.Identity;
using Azure.Messaging.EventGrid.Namespaces;
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
            clientBuilder.AddClient<EventGridSenderClient, EventGridSenderClientOptions>((options, credential, provider) =>
            {
                var configuration = builder.Configuration;

                var namespaceEndpoint = configuration["NamespaceEndpoint"];
                var topicName = configuration["TopicName"];

                if (string.IsNullOrEmpty(namespaceEndpoint))
                {
                    throw new InvalidOperationException("NamespaceEndpoint configuration value is missing or empty.");
                }

                if (string.IsNullOrEmpty(topicName))
                {
                    throw new InvalidOperationException("TopicName configuration value is missing or empty.");
                }

                return new EventGridSenderClient(new Uri($"https://{namespaceEndpoint}"), topicName, new DefaultAzureCredential());
            });
        });

        var host = builder.Build();
        host.Run();
    }
}