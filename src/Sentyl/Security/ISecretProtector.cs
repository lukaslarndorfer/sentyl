namespace Sentyl.Security;

public interface ISecretProtector
{
    public string Protect(string plainText);
    public string Unprotect(string protectedText);
}
