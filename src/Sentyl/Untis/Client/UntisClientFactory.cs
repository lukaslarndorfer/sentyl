namespace Sentyl.Untis.Client;

public sealed class UntisClientFactory(IHttpClientFactory httpClientFactory) : IUntisClientFactory
{
    public const string HttpClientName = "untis";

    public UntisClient Create(UntisCredentials credentials) =>
        new(httpClientFactory.CreateClient(HttpClientName), credentials);
}
