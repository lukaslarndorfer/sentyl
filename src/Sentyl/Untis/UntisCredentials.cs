namespace Sentyl.Untis;

public sealed record UntisCredentials(string Host, string School, string User, string Password)
{
    public override string ToString() => $"{User} - {Host} ({School})";
}
