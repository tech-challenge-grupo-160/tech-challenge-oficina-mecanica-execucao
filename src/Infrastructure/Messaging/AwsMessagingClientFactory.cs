using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Credentials;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Infrastructure.Messaging;

internal static class AwsMessagingClientFactory
{
    public static IAmazonSQS CreateSqs(MessagingOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            var config = new AmazonSQSConfig
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region
            };
            var credentials = DefaultAWSCredentialsIdentityResolver.GetCredentials(config);
            return new AmazonSQSClient(credentials, config);
        }

        return new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region));
    }

    public static IAmazonSimpleNotificationService CreateSns(MessagingOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            var config = new AmazonSimpleNotificationServiceConfig
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region
            };
            var credentials = DefaultAWSCredentialsIdentityResolver.GetCredentials(config);
            return new AmazonSimpleNotificationServiceClient(credentials, config);
        }

        return new AmazonSimpleNotificationServiceClient(
            RegionEndpoint.GetBySystemName(options.Region));
    }
}
