using Sentyl.Untis.Client;

namespace Sentyl.Tests.Helpers;

public sealed class TestUntisClientFactory(HttpMessageHandler handler) : IUntisClientFactory
{
    public UntisClient Create(UntisCredentials credentials) => new(new HttpClient(handler), credentials);
}
