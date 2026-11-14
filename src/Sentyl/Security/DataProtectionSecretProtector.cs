using Microsoft.AspNetCore.DataProtection;

namespace Sentyl.Security;

public sealed class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private const string Purpose = "Sentyl.UntisPassword";
    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public string Protect(string plainText) => _protector.Protect(plainText);
    public string Unprotect(string protectedText) => _protector.Unprotect(protectedText);
}
