namespace Sentyl.Untis;


public sealed record AuthenticateParams(
    string User,
    string Password,
    string Client
    )
{
    public override string ToString()
    {
        // prevent Password from being logged
        return $"{User} {Client}";
    }
}

public sealed record AuthenticateResult(
    string SessionId,
    int PersonType,
    int PersonId)
{

    public override string ToString()
    {
        // prevent SessionId from being logged
        return $"{PersonId} {PersonType}";
    }
}
