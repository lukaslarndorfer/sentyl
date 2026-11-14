using AwesomeAssertions;

using Sentyl.Untis.Client;

namespace Sentyl.Tests.Untis;

public class UntisCredentialsTests
{
    private const string ValidHost = "school.webuntis.com";
    private const string Password = "secret";

    [Fact]
    public void TryCreate_WithValidValues_ReturnsCredentials()
    {
        bool created = UntisCredentials.TryCreate(ValidHost, "school", "user", Password,
                                                  out var credentials, out var error);

        created.Should().BeTrue();
        error.Should().BeNull();
        credentials.Should().NotBeNull();
        credentials.Host.Should().Be(ValidHost);
        credentials.School.Should().Be("school");
        credentials.User.Should().Be("user");
        credentials.Password.Should().Be(Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("xyz.com")]
    [InlineData("xyzwebuntis.com")]
    [InlineData("webuntis.com")]
    [InlineData("school.webuntis.com.xyz.com")]
    [InlineData("https://school.webuntis.com")]
    [InlineData("school.webuntis.com/path")]
    [InlineData("school.webuntis.com:8080")]
    [InlineData("xyz.com#.webuntis.com")]
    [InlineData("xyz.com?.webuntis.com")]
    [InlineData("xyz.com\\.webuntis.com")]
    [InlineData("xyz.com@school.webuntis.com")]
    [InlineData("school.webuntis.com.")]
    [InlineData("school\n.webuntis.com")]
    [InlineData("ä.webuntis.com")]
    [InlineData("x_y.webuntis.com")]
    public void TryCreate_WithInvalidHost_ReturnsFalse(string host)
    {
        bool created = UntisCredentials.TryCreate(host, "school", "user", Password,
                                                  out var credentials, out var error);

        created.Should().BeFalse();
        credentials.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("", "user", Password)]
    [InlineData("   ", "user", Password)]
    [InlineData("school", "", Password)]
    [InlineData("school", "   ", Password)]
    [InlineData("school", "user", "")]
    public void TryCreate_WithMissingValue_ReturnsFalse(string school, string user, string password)
    {
        bool created = UntisCredentials.TryCreate(ValidHost, school, user, password,
                                                  out var credentials, out var error);

        created.Should().BeFalse();
        credentials.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryCreate_WithTooLongValue_ReturnsFalse()
    {
        string tooLong = new('a', 257);

        bool created = UntisCredentials.TryCreate(ValidHost, tooLong, "user", Password,
                                                  out UntisCredentials? credentials, out string? error);

        created.Should().BeFalse();
        credentials.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryCreate_TrimsHost()
    {
        UntisCredentials.TryCreate($"  {ValidHost} ", "school", "user", Password, out UntisCredentials? credentials,
                                   out _);

        credentials.Should().NotBeNull();
        credentials.Host.Should().Be(ValidHost);
    }

    [Fact]
    public void TryCreate_TrimsSchoolAndUser()
    {
        UntisCredentials.TryCreate(ValidHost, "  school ", " user  ", Password, out var credentials, out _);

        credentials.Should().NotBeNull();
        credentials.School.Should().Be("school");
        credentials.User.Should().Be("user");
    }

    [Fact]
    public void Create_WithInvalidHost_Throws()
    {
        Assert.Throws<ArgumentException>(() => UntisCredentials.Create("xyz.com", "school", "user", Password));
    }

    [Fact]
    public void ToString_DoesNotContainPassword()
    {
        var credentials = UntisCredentials.Create(ValidHost, "school", "user", Password);

        credentials.ToString().Should().NotContain(Password);
    }
}
