using System.Net;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Sentyl.Tests.Helpers;
using Sentyl.Untis.Client;

namespace Sentyl.Tests.Untis;

public class UntisCredentialsValidatorTests
{
    private static readonly UntisCredentials credentials
        = UntisCredentials.Create("school.webuntis.com", "school", "user", "password");

    [Fact]
    public async ValueTask CheckAsync_WhenUntisAcceptsCredentials_ReturnsValid()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK,
                                                  """{"jsonrpc":"2.0","id":"1","result":{"sessionId":"abc","personType":5,"personId":1}}"""));

        UntisCredentialsCheck result = await validator.CheckAsync(credentials, TestContext.Current.CancellationToken);

        result.Should().Be(UntisCredentialsCheck.Valid);
    }

    [Fact]
    public async ValueTask CheckAsync_WhenUntisReportsBadCredentials_ReturnsInvalidCredentials()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK,
                                                  """{"jsonrpc":"2.0","id":"1","error":{"message":"bad credentials","code":-8504}}"""));

        UntisCredentialsCheck result = await validator.CheckAsync(credentials, TestContext.Current.CancellationToken);

        result.Should().Be(UntisCredentialsCheck.InvalidCredentials);
    }

    [Fact]
    public async ValueTask CheckAsync_WhenUntisAnswersNotFound_ReturnsSchoolNotFound()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.NotFound,
                                                  """{"jsonrpc":"2.0","id":"error","error":{"message":"invalid schoolname","code":-8500}}"""));

        UntisCredentialsCheck result = await validator.CheckAsync(credentials, TestContext.Current.CancellationToken);

        result.Should().Be(UntisCredentialsCheck.SchoolNotFound);
    }

    [Fact]
    public async ValueTask CheckAsync_WhenUntisReportsInvalidSchoolWithOk_ReturnsSchoolNotFound()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK,
                                                  """{"jsonrpc":"2.0","id":"1","error":{"message":"invalid schoolname","code":-8500}}"""));

        UntisCredentialsCheck result = await validator.CheckAsync(credentials, TestContext.Current.CancellationToken);

        result.Should().Be(UntisCredentialsCheck.SchoolNotFound);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "{}")]
    [InlineData(HttpStatusCode.OK, """{"jsonrpc":"2.0","id":"1","error":{"message":"something else","code":-32601}}""")]
    public async ValueTask CheckAsync_WhenUntisAnswersUnexpectedly_ReturnsUnreachable(HttpStatusCode statusCode, string body)
    {
        var validator = CreateValidator(_ => Json(statusCode, body));

        UntisCredentialsCheck result = await validator.CheckAsync(credentials, TestContext.Current.CancellationToken);

        result.Should().Be(UntisCredentialsCheck.Unreachable);
    }

    [Fact]
    public async ValueTask CheckAsync_WhenNetworkFails_ReturnsUnreachable()
    {
        var validator = CreateValidator(_ => throw new HttpRequestException("no connection"));

        UntisCredentialsCheck result = await validator.CheckAsync(credentials, TestContext.Current.CancellationToken);

        result.Should().Be(UntisCredentialsCheck.Unreachable);
    }

    [Fact]
    public async ValueTask CheckAsync_WhenCallerCancels_ThrowsInsteadOfReturningUnreachable()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK, "{}"));
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => validator.CheckAsync(credentials,
                                                                 cancellation.Token));
    }

    private static UntisCredentialsValidator CreateValidator(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        new(new TestUntisClientFactory(new StubHttpMessageHandler(respond)),
            NullLogger<UntisCredentialsValidator>.Instance);

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string body) =>
        new(statusCode) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
}
