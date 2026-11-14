using System.Diagnostics.CodeAnalysis;

namespace Sentyl.Untis;

public sealed record UntisCredentials
{
    public override string ToString() => $"{User} - {Host} ({School})";

    private const string AllowedHostSuffix = ".webuntis.com";
    private const int MaxHostLength = 253;
    private const int MaxFieldLength = 256;

    public string Host { get; }
    public string School { get; }
    public string User { get; }
    public string Password { get; }

    private UntisCredentials(string host, string school, string user, string password)
    {
        Host = host;
        School = school;
        User = user;
        Password = password;
    }


    public static UntisCredentials Create(string host, string school, string user, string password)
        => TryCreate(host, school, user, password, out var credentials, out string? error)
            ? credentials
            : throw new ArgumentException(error);

    public static bool TryCreate(string host, string school, string user, string password,
                                 [NotNullWhen(true)] out UntisCredentials? credentials,
                                 [NotNullWhen(false)] out string? error)
    {
        host = host.Trim();

        if (!IsValidHost(host))
        {
            error = $"Host must be a WebUntis host, e.g. school{AllowedHostSuffix}";

            credentials = null;
            return false;
        }

        if (string.IsNullOrWhiteSpace(school) || string.IsNullOrWhiteSpace(user) || string.IsNullOrEmpty(password))
        {
            error = "All fields must have a value";

            credentials = null;
            return false;
        }

        if (school.Length > MaxFieldLength || user.Length > MaxFieldLength || password.Length > MaxFieldLength)
        {
            error = $"Values must not be longer than {MaxFieldLength} characters";

            credentials = null;
            return false;
        }

        error = null;
        credentials = new UntisCredentials(host, school.Trim(), user.Trim(), password);

        return true;
    }

    private static bool IsValidHost(string host) =>
        host.Length <= MaxHostLength
        // only plain ASCII host names, no IDN look-alikes, ports, paths or credentials
        && host.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '.')
        && Uri.CheckHostName(host) == UriHostNameType.Dns
        // leading dot, so "xyzwebuntis.com" is not accepted
        && host.EndsWith(AllowedHostSuffix, StringComparison.OrdinalIgnoreCase);
}
