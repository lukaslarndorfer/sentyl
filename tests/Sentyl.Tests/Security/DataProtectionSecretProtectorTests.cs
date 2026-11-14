using System.Security.Cryptography;

using AwesomeAssertions;

using Microsoft.AspNetCore.DataProtection;

using Sentyl.Security;

namespace Sentyl.Tests.Security;

public class DataProtectionSecretProtectorTests
{
    private const string Secret = "secret";

    // in-memory keys, so the tests do not need a key directory
    private readonly EphemeralDataProtectionProvider _provider = new();
    private readonly DataProtectionSecretProtector _protector;

    public DataProtectionSecretProtectorTests()
    {
        _protector = new DataProtectionSecretProtector(_provider);
    }

    [Fact]
    public void Unprotect_ReturnsOriginalText()
    {
        string protectedText = _protector.Protect(Secret);
        string unprotectedText = _protector.Unprotect(protectedText);

        unprotectedText.Should().Be(Secret);
    }

    [Fact]
    public void Protect_DoesNotContainPlainText()
    {
        string protectedText = _protector.Protect(Secret);

        protectedText.Should().NotContain(Secret);
    }

    [Fact]
    public void Protect_ReturnsDifferentTextOnEveryCall()
    {
        string firstProtectedText = _protector.Protect(Secret);
        string secondProtectedText = _protector.Protect(Secret);

        firstProtectedText.Should().NotBe(secondProtectedText);
    }

    [Fact]
    public void Unprotect_WhenTextWasChanged_Throws()
    {
        string protectedText = _protector.Protect(Secret);

        string changed = string.Create(protectedText.Length, protectedText, static (span, src) =>
        {
            src.CopyTo(span);
            int mid = span.Length / 2;
            span[mid] = span[mid] == 'A' ? 'B' : 'A';
        });

        Assert.Throws<CryptographicException>(() => _protector.Unprotect(changed));
    }

    [Fact]
    public void Unprotect_WithOtherPurpose_Throws()
    {
        IDataProtector anotherProtector = _provider.CreateProtector("another Purpose");
        string protectedText = anotherProtector.Protect(Secret);

        Assert.Throws<CryptographicException>(() => _protector.Unprotect(protectedText));
    }
}
