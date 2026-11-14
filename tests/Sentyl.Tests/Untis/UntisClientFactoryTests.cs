using AwesomeAssertions;

using Microsoft.Extensions.DependencyInjection;

using Sentyl.Untis.Client;

namespace Sentyl.Tests.Untis;

public class UntisClientFactoryTests
{
    private static readonly UntisCredentials credentials
        = UntisCredentials.Create("school.webuntis.com", "school", "user", "password");

    [Fact]
    public void Create_ReturnsNewClientOnEveryCall()
    {
        UntisClientFactory factory = CreateFactory();

        UntisClient first = factory.Create(credentials);
        UntisClient second = factory.Create(credentials);

        first.Should().NotBeSameAs(second);
    }

    private static UntisClientFactory CreateFactory()
    {
        ServiceCollection services = new();
        services.AddHttpClient(UntisClientFactory.HttpClientName);
        IHttpClientFactory httpClientFactory = services.BuildServiceProvider()
                                                       .GetRequiredService<IHttpClientFactory>();

        return new UntisClientFactory(httpClientFactory);
    }
}
